﻿using System;
using System.ComponentModel.DataAnnotations;
using LivriaBackend.Shared.Resources;
using LivriaBackend.Shared.Validation;

namespace LivriaBackend.communities.Interfaces.REST.Resources
{
    public record UserCommunityResource(
        int UserClientId,
        
        int CommunityId,
        
        [DataType(DataType.DateTime)]
        [DateRangeToday(MinimumDate = "1900-01-01", ErrorResourceType = typeof(SharedResource), ErrorResourceName = "DateNotInRange")]
        DateTime JoinedDate
    );
}