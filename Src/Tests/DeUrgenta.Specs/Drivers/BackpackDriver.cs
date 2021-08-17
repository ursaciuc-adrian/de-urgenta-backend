﻿using System;
using System.Linq;
using System.Threading.Tasks;
using DeUrgenta.Specs.Extensions;

namespace DeUrgenta.Specs.Drivers
{
    /// <summary>
    /// This class encapsulates backpack actions
    /// </summary>
    public class BackpackDriver
    {

        /// <summary>
        /// Adds a user as backpack contributor.
        /// </summary>
        public async Task AddToBackpackContributor(ApiClient backpackOwner, ApiClient targetUser, Guid backpackId)
        {
            var targetUserId = await targetUser.GetUserId();
            await backpackOwner.InviteToBackpackContributorsAsync(backpackId, targetUserId);

            var invites = await targetUser.GetBackpackInvitesAsync();
            await targetUser.AcceptBackpackInviteAsync(invites.First().InviteId);
        }
    }
}