﻿using LivriaBackend.commerce.Domain.Model.Aggregates;
using LivriaBackend.commerce.Domain.Repositories;
using LivriaBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using LivriaBackend.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LivriaBackend.commerce.Infrastructure.Repositories
{
    /// <summary>
    /// Implementación concreta del repositorio de <see cref="Book"/> utilizando Entity Framework Core.
    /// Hereda de <see cref="BaseRepository{TEntity}"/> y <see cref="IBookRepository"/>.
    /// </summary>
    public class BookRepository : BaseRepository<Book>, IBookRepository
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="BookRepository"/>.
        /// </summary>
        /// <param name="context">El contexto de la base de datos de la aplicación.</param>
        public BookRepository(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Obtiene un libro de forma asíncrona por su identificador único, incluyendo sus reseñas.
        /// Sobrescribe el método base para incluir la carga ansiosa de la colección <see cref="Book.Reviews"/>.
        /// </summary>
        /// <param name="id">El identificador único del libro.</param>
        /// <returns>Una tarea que representa la operación asíncrona. El resultado de la tarea es el <see cref="Book"/> encontrado con sus reseñas, o null si no existe.</returns>
        public new async Task<Book> GetByIdAsync(int id)
        {
            return await this.Context.Books
                .Include(b => b.Reviews)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        /// <summary>
        /// Obtiene todos los libros de forma asíncrona, incluyendo sus reseñas.
        /// Sobrescribe el método base para incluir la carga ansiosa de la colección <see cref="Book.Reviews"/>.
        /// </summary>
        /// <returns>Una tarea que representa la operación asíncrona. El resultado de la tarea es una colección de todos los <see cref="Book"/> con sus reseñas.</returns>
        public new async Task<IEnumerable<Book>> GetAllAsync()
        {
            return await this.Context.Books
                .Include(b => b.Reviews)
                .ToListAsync();
        }

        /// <summary>
        /// Añade un nuevo libro de forma asíncrona al repositorio.
        /// </summary>
        /// <param name="book">El objeto <see cref="Book"/> a añadir.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        public async Task AddAsync(Book book)
        {
            await this.Context.Books.AddAsync(book);
        }

        /// <summary>
        /// Actualiza un libro existente de forma asíncrona en el repositorio.
        /// </summary>
        /// <param name="book">El objeto <see cref="Book"/> a actualizar.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        /// <remarks>
        /// Este método establece el estado de la entidad a <see cref="EntityState.Modified"/>,
        /// lo que indica a Entity Framework Core que la entidad ha sido modificada y debe ser guardada.
        /// No realiza una operación asíncrona de base de datos directa, sino que prepara el contexto.
        /// La persistencia se realiza con <c>UnitOfWork</c>.
        /// </remarks>
        public async Task UpdateAsync(Book book) 
        {
            this.Context.Entry(book).State = EntityState.Modified;
            await Task.CompletedTask; 
        }
    }
}