﻿using LivriaBackend.commerce.Domain.Model.ValueObjects; 
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LivriaBackend.commerce.Domain.Model.Commands
{
    /// <summary>
    /// Representa un comando para crear una nueva orden de compra.
    /// </summary>
    /// <param name="UserClientId">El identificador único del cliente de usuario que realiza la orden.</param>
    /// <param name="UserEmail">El correo electrónico del usuario.</param>
    /// <param name="UserPhone">El número de teléfono del usuario.</param>
    /// <param name="UserFullName">El nombre completo del usuario.</param>
    /// <param name="IsDelivery">Indica si la orden requiere envío a domicilio (verdadero) o es para recoger (falso).</param>
    /// <param name="ShippingDetails">Los detalles de envío. Obligatorio si <paramref name="IsDelivery"/> es verdadero.</param>
    /// <param name="CartItemIds">Una lista de los identificadores de los ítems del carrito que se incluirán en la orden.</param>
    public record CreateOrderCommand(
        [Required]
        int UserClientId,
        
        [Required]
        [EmailAddress]
        string UserEmail,
        
        [Required]
        [Phone]
        string UserPhone,
        
        [Required]
        [StringLength(255)]
        string UserFullName,
        
        [Required]
        bool IsDelivery,
        
        Shipping ShippingDetails,
        
        [Required]
        List<int> CartItemIds 
    );
}