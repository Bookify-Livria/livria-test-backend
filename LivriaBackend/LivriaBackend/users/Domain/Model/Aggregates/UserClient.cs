﻿using LivriaBackend.commerce.Domain.Model.Aggregates; 
using LivriaBackend.communities.Domain.Model.Aggregates;
using System.Collections.Generic;
using System.Linq;
using System;

namespace LivriaBackend.users.Domain.Model.Aggregates
{
    /// <summary>
    /// Representa un cliente de usuario en el sistema.
    /// Hereda de la clase base <see cref="User"/> y añade propiedades específicas del cliente,
    /// como su icono, frase personal, suscripción, y colecciones relacionadas.
    /// </summary>
    public class UserClient : User
    {
        /// <summary>
        /// Obtiene el URL o identificador del icono/avatar del usuario.
        /// </summary>
        public string Icon { get; private set; }

        /// <summary>
        /// Obtiene una frase o estado personal del usuario.
        /// </summary>
        public string Phrase { get; private set; }

        /// <summary>
        /// Obtiene el plan de suscripción actual del usuario (ej. "freeplan", "communityplan").
        /// </summary>
        public string Subscription { get; private set; }

        /// <summary>
        /// Obtiene la colección de órdenes realizadas por este usuario.
        /// </summary>
        public ICollection<Order> Orders { get; private set; } = new List<Order>();

        /// <summary>
        /// Obtiene la colección de comunidades a las que este usuario se ha unido.
        /// </summary>
        public ICollection<UserCommunity> UserCommunities { get; private set; } = new List<UserCommunity>();

        /// <summary>
        /// Lista interna de libros favoritos del usuario.
        /// </summary>
        private readonly List<Book> _favoriteBooks = new List<Book>();

        /// <summary>
        /// Obtiene una colección de solo lectura de los libros favoritos del usuario.
        /// </summary>
        public IReadOnlyCollection<Book> FavoriteBooks => _favoriteBooks.AsReadOnly();


        /// <summary>
        /// Constructor protegido sin parámetros, típicamente utilizado por ORMs como Entity Framework Core.
        /// Inicializa las colecciones de navegación.
        /// </summary>
        protected UserClient() : base()
        {
            Orders = new List<Order>(); 
            UserCommunities = new List<UserCommunity>();
        }

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="UserClient"/> con las propiedades especificadas.
        /// Inicializa las colecciones de órdenes y comunidades.
        /// </summary>
        /// <param name="display">El nombre visible o alias del cliente.</param>
        /// <param name="username">El nombre de usuario único del cliente.</param>
        /// <param name="email">La dirección de correo electrónico del cliente.</param>
        /// <param name="password">La contraseña del cliente (asumida como segura).</param>
        /// <param name="icon">El URL o identificador del icono/avatar del cliente.</param>
        /// <param name="phrase">La frase o estado personal del cliente.</param>
        /// <param name="subscription">El plan de suscripción del cliente.</param>
        public UserClient(string display, string username, string email, string password, string icon, string phrase, string subscription)
            : base(display, username, email, password)
        {
            Icon = icon;
            Phrase = phrase;
            Subscription = subscription; 
            Orders = new List<Order>();
            UserCommunities = new List<UserCommunity>();
        }

        /// <summary>
        /// Actualiza las propiedades de un cliente de usuario.
        /// Utiliza el método base <see cref="User.UpdateUserProperties"/> para actualizar las propiedades de usuario comunes.
        /// </summary>
        /// <param name="display">El nuevo nombre visible o alias.</param>
        /// <param name="username">El nuevo nombre de usuario.</param>
        /// <param name="email">La nueva dirección de correo electrónico.</param>
        /// <param name="password">La nueva contraseña.</param>
        /// <param name="icon">El nuevo URL o identificador del icono.</param>
        /// <param name="phrase">La nueva frase o estado personal.</param>
        /// <param name="subscription">El nuevo plan de suscripción.</param>
        public void Update(string display, string username, string email, string password, string icon, string phrase, string subscription)
        {
            base.UpdateUserProperties(display, username, email, password);
            Icon = icon;
            Phrase = phrase;
            Subscription = subscription;
        }

        /// <summary>
        /// Actualiza el plan de suscripción del usuario cliente.
        /// </summary>
        /// <param name="newSubscriptionPlan">El nuevo plan de suscripción (ej. "freeplan", "communityplan").</param>
        /// <exception cref="ArgumentException">Se lanza si el nuevo plan de suscripción es nulo o vacío.</exception>
        public void UpdateSubscription(string newSubscriptionPlan)
        {
            if (string.IsNullOrWhiteSpace(newSubscriptionPlan))
            {
                throw new ArgumentException("Subscription plan cannot be empty.", nameof(newSubscriptionPlan));
            }
            
            Subscription = newSubscriptionPlan;
        }
        
        /// <summary>
        /// Permite al usuario unirse a una comunidad específica.
        /// Añade una nueva entrada <see cref="UserCommunity"/> si el usuario no está ya en la comunidad.
        /// </summary>
        /// <param name="communityId">El ID de la comunidad a la que el usuario se unirá.</param>
        public void JoinCommunity(int communityId)
        {
            if (!UserCommunities.Any(uc => uc.CommunityId == communityId))
            {
                UserCommunities.Add(new UserCommunity(this.Id, communityId));
            }
        }

        /// <summary>
        /// Permite al usuario dejar una comunidad específica.
        /// Elimina la entrada <see cref="UserCommunity"/> correspondiente si el usuario está en la comunidad.
        /// </summary>
        /// <param name="communityId">El ID de la comunidad que el usuario dejará.</param>
        public void LeaveCommunity(int communityId)
        {
                var userCommunity = UserCommunities.FirstOrDefault(uc => uc.CommunityId == communityId);
                if (userCommunity != null)
                {
                    UserCommunities.Remove(userCommunity);
                }
        }
        
        /// <summary>
        /// Añade un libro a la colección de libros favoritos del usuario.
        /// El libro no se añade si ya está presente en la colección.
        /// </summary>
        /// <param name="book">El objeto <see cref="Book"/> a añadir a favoritos.</param>
        /// <exception cref="ArgumentNullException">Se lanza si el libro proporcionado es nulo.</exception>
        public void AddFavoriteBook(Book book)
        {
            if (book == null)
            {
                throw new ArgumentNullException(nameof(book));
            }

            if (!_favoriteBooks.Contains(book))
            {
                _favoriteBooks.Add(book);
            }
        }
        
        /// <summary>
        /// Elimina un libro de la colección de libros favoritos del usuario.
        /// </summary>
        /// <param name="book">El objeto <see cref="Book"/> a eliminar de favoritos.</param>
        /// <exception cref="ArgumentNullException">Se lanza si el libro proporcionado es nulo.</exception>
        public void RemoveFavoriteBook(Book book)
        {
            if (book == null)
            {
                throw new ArgumentNullException(nameof(book));
            }
            _favoriteBooks.Remove(book);
        }
    }
}