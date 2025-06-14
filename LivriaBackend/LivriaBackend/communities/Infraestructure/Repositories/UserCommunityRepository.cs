﻿using LivriaBackend.communities.Domain.Model.Aggregates;
using LivriaBackend.communities.Domain.Repositories;
using LivriaBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using LivriaBackend.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore; // For FirstOrDefaultAsync and Composite Key query
using System.Threading.Tasks;

namespace LivriaBackend.communities.Infrastructure.Repositories
{
    /// <summary>
    /// EF Core implementation of IUserCommunityRepository.
    /// </summary>
    public class UserCommunityRepository : BaseRepository<UserCommunity>, IUserCommunityRepository
    {
        public UserCommunityRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<UserCommunity> GetByUserAndCommunityIdsAsync(int userClientId, int communityId)
        {
            return await Context.UserCommunities
                .FirstOrDefaultAsync(uc => uc.UserClientId == userClientId && uc.CommunityId == communityId);
        }
    }
}