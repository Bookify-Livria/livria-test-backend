﻿using LivriaBackend.commerce.Domain.Model.Aggregates;
using LivriaBackend.commerce.Domain.Model.Commands;
using LivriaBackend.commerce.Domain.Model.Entities; 
using LivriaBackend.commerce.Domain.Repositories; 
using LivriaBackend.commerce.Domain.Model.Services;
using LivriaBackend.Shared.Domain.Repositories; 
using LivriaBackend.users.Domain.Model.Repositories; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using LivriaBackend.notifications.Domain.Model.Services; 
using LivriaBackend.notifications.Domain.Model.Commands; 
using LivriaBackend.notifications.Domain.Model.ValueObjects; 


namespace LivriaBackend.commerce.Application.Internal.CommandServices
{
    /// <summary>
    /// Implementa el servicio de comandos para la entidad <see cref="Order"/>.
    /// Procesa comandos relacionados con la creación y gestión de órdenes de compra.
    /// </summary>
    public class OrderCommandService : IOrderCommandService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartItemRepository _cartItemRepository; 
        private readonly IBookRepository _bookRepository; 
        private readonly IUserClientRepository _userClientRepository; 
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationCommandService _notificationCommandService; 

        
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="OrderCommandService"/>.
        /// </summary>
        /// <param name="orderRepository">El repositorio de órdenes.</param>
        /// <param name="cartItemRepository">El repositorio de ítems del carrito.</param>
        /// <param name="bookRepository">El repositorio de libros.</param>
        /// <param name="userClientRepository">El repositorio de clientes de usuario.</param>
        /// <param name="unitOfWork">La unidad de trabajo para gestionar transacciones.</param>
        /// <param name="notificationCommandService">El servicio de comandos de notificación para enviar alertas al usuario.</param>

        public OrderCommandService(
            IOrderRepository orderRepository,
            ICartItemRepository cartItemRepository,
            IBookRepository bookRepository,
            IUserClientRepository userClientRepository,
            IUnitOfWork unitOfWork,
            INotificationCommandService notificationCommandService) 
        {
            _orderRepository = orderRepository;
            _cartItemRepository = cartItemRepository;
            _bookRepository = bookRepository;
            _userClientRepository = userClientRepository;
            _unitOfWork = unitOfWork;
            _notificationCommandService = notificationCommandService; 
        }

        /// <summary>
        /// Maneja el comando <see cref="CreateOrderCommand"/> para crear una nueva orden de compra.
        /// </summary>
        /// <param name="command">El comando que contiene los detalles de la orden, incluyendo los IDs de los ítems del carrito.</param>
        /// <returns>El objeto <see cref="Order"/> creado.</returns>
        /// <exception cref="ArgumentException">
        /// Se lanza si el cliente de usuario no se encuentra, si un ítem del carrito no existe o no pertenece al usuario,
        /// si el carrito está vacío, si un libro no se encuentra, o si el estado de la orden es inválido.
        /// </exception>
        /// <exception cref="InvalidOperationException">Se lanza si no hay suficiente stock para un libro.</exception>
        /// <remarks>
        /// Este método:
        /// 1. Valida la existencia del cliente de usuario.
        /// 2. Recupera y valida los ítems del carrito, asegurando que pertenecen al usuario.
        /// 3. Verifica que el carrito no esté vacío.
        /// 4. Itera sobre los ítems del carrito para:
        ///    a. Validar la existencia y stock de cada libro.
        ///    b. Crear <see cref="OrderItem"/>s a partir de los ítems del carrito.
        ///    c. Disminuir el stock del libro.
        /// 5. Crea la nueva <see cref="Order"/> con todos sus <see cref="OrderItem"/>s.
        /// 6. Elimina los ítems del carrito una vez que la orden ha sido creada.
        /// 7. Persiste todos los cambios utilizando la unidad de trabajo.
        /// 8. Envía una notificación de "Orden Recibida" al cliente de usuario.
        /// </remarks>

        public async Task<Order> Handle(CreateOrderCommand command)
        {
            var userClient = await _userClientRepository.GetByIdAsync(command.UserClientId);
            if (userClient == null)
            {
                throw new ArgumentException($"UserClient with ID {command.UserClientId} not found.");
            }

            var cartItems = new List<CartItem>();
            foreach (var cartItemId in command.CartItemIds)
            {
                var cartItem = await _cartItemRepository.GetByIdAsync(cartItemId);
                if (cartItem == null || cartItem.UserClientId != command.UserClientId)
                {
                    throw new ArgumentException($"CartItem with ID {cartItemId} not found or does not belong to UserClient {command.UserClientId}.");
                }
                cartItems.Add(cartItem);
            }

            if (!cartItems.Any())
            {
                throw new ArgumentException("Cannot create an order with an empty cart.");
            }

            var orderItems = new List<OrderItem>();
            foreach (var cartItem in cartItems)
            {
                var book = await _bookRepository.GetByIdAsync(cartItem.BookId);
                if (book == null)
                {
                    throw new ArgumentException($"Book with ID {cartItem.BookId} for CartItem {cartItem.Id} not found.");
                }

                if (book.Stock < cartItem.Quantity)
                {
                    throw new InvalidOperationException($"Insufficient stock for book '{book.Title}'. Available: {book.Stock}, Requested: {cartItem.Quantity}.");
                }

                var orderItem = new OrderItem(
                    book.Id,
                    book.Title,
                    book.Author,
                    book.SalePrice,
                    book.Cover,
                    cartItem.Quantity
                );
                orderItems.Add(orderItem);

                book.DecreaseStock(cartItem.Quantity);
                await _bookRepository.UpdateAsync(book);
            }

            
            var order = new Order(
                command.UserClientId,
                command.UserEmail,
                command.UserPhone,
                command.UserFullName,
                command.RecipientName,
                command.IsDelivery,
                command.ShippingDetails,
                orderItems,
                command.Status 
            );

            await _orderRepository.AddAsync(order);

            foreach (var cartItem in cartItems)
            {
                await _cartItemRepository.DeleteAsync(cartItem);
            }

            await _unitOfWork.CompleteAsync(); 
            
            await _notificationCommandService.Handle(new CreateNotificationCommand(
                command.UserClientId, 
                ENotificationType.Order, 
                DateTime.UtcNow       
            ));

            return order;
        }

        /// <summary>
        /// Maneja el comando <see cref="UpdateOrderStatusCommand"/> para actualizar el estado de una orden existente.
        /// </summary>
        /// <param name="command">El comando que contiene el ID de la orden y el nuevo estado.</param>
        /// <returns>El objeto <see cref="Order"/> actualizado, o <c>null</c> si la orden no se encuentra.</returns>
        /// <exception cref="ArgumentException">Se lanza si el nuevo estado no es 'pending', 'in progress' o 'delivered'.</exception>
        public async Task<Order?> Handle(UpdateOrderStatusCommand command)
        {
            var order = await _orderRepository.GetByIdAsync(command.OrderId);
            if (order == null)
            {
                return null; 
            }
            
            order.UpdateStatus(command.Status);

            await _unitOfWork.CompleteAsync(); 
            return order;
        }
    }
}