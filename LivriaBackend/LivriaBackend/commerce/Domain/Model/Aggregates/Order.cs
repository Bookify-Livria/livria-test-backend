﻿using LivriaBackend.commerce.Domain.Model.Entities;
using LivriaBackend.commerce.Domain.Model.ValueObjects;
using LivriaBackend.users.Domain.Model.Aggregates; 
using System; 
using System.Collections.Generic;
using System.Linq; 

namespace LivriaBackend.commerce.Domain.Model.Aggregates
{
    /// <summary>
    /// Representa la entidad agregada 'Orden' en el dominio de comercio.
    /// Una <see cref="Order"/> es un objeto con una identidad global y es la raíz de un agregado,
    /// lo que significa que controla la vida útil de las entidades contenidas como <see cref="OrderItem"/>.
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Obtiene el identificador único de la orden.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Obtiene el código alfanumérico único de la orden, generado automáticamente.
        /// </summary>
        public string Code { get; private set; } 

        /// <summary>
        /// Obtiene el identificador del cliente de usuario que realizó la orden.
        /// </summary>
        public int UserClientId { get; private set; } 

        /// <summary>
        /// Obtiene el objeto <see cref="UserClient"/> asociado a esta orden.
        /// </summary>
        public UserClient UserClient { get; private set; } 
        
        /// <summary>
        /// Obtiene el correo electrónico del usuario en el momento de la orden.
        /// </summary>
        public string UserEmail { get; private set; }

        /// <summary>
        /// Obtiene el número de teléfono del usuario en el momento de la orden.
        /// </summary>
        public string UserPhone { get; private set; }

        /// <summary>
        /// Obtiene el nombre completo del usuario en el momento de la orden.
        /// </summary>
        public string UserFullName { get; private set; }

        /// <summary>
        /// Obtiene un valor que indica si la orden requiere envío a domicilio.
        /// </summary>
        public bool IsDelivery { get; private set; } 
        
        /// <summary>
        /// Obtiene los detalles de envío de la orden, si <see cref="IsDelivery"/> es verdadero.
        /// </summary>
        public Shipping Shipping { get; private set; } 

        /// <summary>
        /// Obtiene el monto total de la orden.
        /// </summary>
        public decimal Total { get; private set; }

        /// <summary>
        /// Obtiene la fecha y hora de creación de la orden (en UTC).
        /// </summary>
        public DateTime Date { get; private set; } 

        private readonly List<OrderItem> _items = new List<OrderItem>();
        /// <summary>
        /// Obtiene una colección de solo lectura de los ítems incluidos en esta orden.
        /// </summary>
        public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

        /// <summary>
        /// Constructor protegido para uso de frameworks ORM (como Entity Framework Core).
        /// No debe ser utilizado directamente para la creación de instancias de <see cref="Order"/>.
        /// </summary>
        protected Order() { }

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="Order"/> con los detalles especificados.
        /// </summary>
        /// <param name="userClientId">El identificador del cliente de usuario que realiza la orden.</param>
        /// <param name="userEmail">El correo electrónico del usuario.</param>
        /// <param name="userPhone">El número de teléfono del usuario.</param>
        /// <param name="userFullName">El nombre completo del usuario.</param>
        /// <param name="isDelivery">Indica si la orden requiere envío a domicilio.</param>
        /// <param name="shipping">Los detalles de envío (obligatorio si <paramref name="isDelivery"/> es verdadero, debe ser nulo si es falso).</param>
        /// <param name="orderItems">La lista de ítems que componen la orden.</param>
        /// <exception cref="ArgumentOutOfRangeException">Se lanza si el UserClient ID no es positivo.</exception>
        /// <exception cref="ArgumentNullException">Se lanza si UserEmail, UserPhone o UserFullName son nulos o vacíos.</exception>
        /// <exception cref="ArgumentException">
        /// Se lanza si la lista de ítems está vacía, o si los detalles de envío no corresponden al valor de <paramref name="isDelivery"/>.
        /// </exception>
        /// <remarks>
        /// Este constructor:
        /// 1. Realiza validaciones sobre los argumentos de entrada.
        /// 2. Genera un código único para la orden.
        /// 3. Establece la fecha de la orden como la fecha UTC actual.
        /// 4. Añade los ítems a la orden, calcula el total y establece la relación bidireccional entre Order y OrderItem.
        /// </remarks>
        public Order(
            int userClientId,
            string userEmail,
            string userPhone,
            string userFullName,
            bool isDelivery,
            Shipping shipping, 
            List<OrderItem> orderItems) 
        {
            if (userClientId <= 0) throw new ArgumentOutOfRangeException(nameof(userClientId), "UserClient ID must be positive.");
            if (string.IsNullOrWhiteSpace(userEmail)) throw new ArgumentNullException(nameof(userEmail), "User email cannot be empty.");
            if (string.IsNullOrWhiteSpace(userPhone)) throw new ArgumentNullException(nameof(userPhone), "User phone cannot be empty.");
            if (string.IsNullOrWhiteSpace(userFullName)) throw new ArgumentNullException(nameof(userFullName), "User full name cannot be empty.");
            if (orderItems == null || !orderItems.Any()) throw new ArgumentException("Order must contain at least one item.", nameof(orderItems));
            if (isDelivery && shipping == null) throw new ArgumentException("Shipping details are required for delivery orders.");
            if (!isDelivery && shipping != null) throw new ArgumentException("Shipping details should be null for non-delivery orders.");


            UserClientId = userClientId;
            UserEmail = userEmail;
            UserPhone = userPhone;
            UserFullName = userFullName;
            IsDelivery = isDelivery;
            Shipping = shipping; 

            Code = GenerateOrderCode();
            Date = DateTime.UtcNow; 

            foreach (var item in orderItems)
            {
                _items.Add(item);
                Total += item.ItemTotal;
                item.SetOrder(this); 
            }
        }

        /// <summary>
        /// Genera un código alfanumérico aleatorio para la orden.
        /// </summary>
        /// <param name="length">La longitud deseada del código (por defecto es 6).</param>
        /// <returns>Una cadena que representa el código de la orden.</returns>
        public static string GenerateOrderCode(int length = 6)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var result = new char[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = chars[random.Next(chars.Length)];
            }

            return new string(result);
        }

    }
}