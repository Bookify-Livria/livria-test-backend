﻿using System;
using System.ComponentModel.DataAnnotations;
using LivriaBackend.Shared.Resources;
using LivriaBackend.Shared.Validation;

namespace LivriaBackend.communities.Interfaces.REST.Resources
{
    public record PostResource(
        int Id,
        
        int CommunityId,
        
        int UserId,
        
        [StringLength(255, ErrorMessage = "MaxLengthError")]
        string Username,
        
        [StringLength(500, ErrorMessage = "MaxLengthError")]
        string Content,
        
        /* [Url(ErrorMessage = "UrlError")] */
        string Img,
        
        [DataType(DataType.DateTime)]
        [DateRangeToday(MinimumDate = "1900-01-01", ErrorResourceType = typeof(SharedResource), ErrorResourceName = "DateNotInRange")]
        DateTime CreatedAt 
    );
}