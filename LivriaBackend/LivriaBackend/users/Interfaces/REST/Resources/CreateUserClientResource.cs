﻿using System.ComponentModel.DataAnnotations; // Para validaciones

namespace LivriaBackend.users.Interfaces.REST.Resources
{
    // Cambiado a 'public record'
    public record CreateUserClientResource(
        [Required] [StringLength(100)] string Display,
        [Required] [StringLength(50)] string Username,
        [Required] [EmailAddress] [StringLength(100)] string Email,
        [Required] [MinLength(6)] string Password, // Ejemplo de validación para contraseña
        string Icon,
        string Phrase,
        string Subscription
    );
}