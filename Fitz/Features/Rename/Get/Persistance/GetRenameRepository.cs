using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fitz.Core.Contexts;
using Fitz.Features.Rename.Common;
using Fitz.Features.Rename.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Fitz.Features.Rename.Get.Persistance
{
    public class GetRenameRepository
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public GetRenameRepository(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        }

        public async Task<Common.Rename> GetRenameByIdAsync(int renameId)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<BotContext>();

            var renameEntity = await dbContext.Renames
                .FirstOrDefaultAsync(r => r.Id == renameId);
                
            return ConvertToRename(renameEntity);
        }

        public async Task<Common.Rename> GetActiveRenameByUserIdAsync(ulong userId)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<BotContext>();

            var renameEntity = await dbContext.Renames
                .Where(r => r.AffectedUserId == userId && r.Status == (Models.RenameStatus)Common.RenameStatus.Active)
                .OrderByDescending(r => r.StartDate)
                .FirstOrDefaultAsync();
                
            return ConvertToRename(renameEntity);
        }

        public async Task<IEnumerable<Common.Rename>> GetRenameHistoryByUserIdAsync(ulong userId)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<BotContext>();

            var renameEntities = await dbContext.Renames
                .Where(r => r.AffectedUserId == userId || r.RequestedUserId == userId)
                .OrderByDescending(r => r.Timestamp)
                .ToListAsync();
                
            return renameEntities.Select(ConvertToRename).Where(r => r != null).ToList();
        }

        public async Task<IEnumerable<Common.Rename>> GetRenamesByStatusAsync(Common.RenameStatus status)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<BotContext>();

            var renameEntities = await dbContext.Renames
                .Where(r => r.Status == (Models.RenameStatus)status)
                .OrderByDescending(r => r.Timestamp)
                .ToListAsync();
                
            return renameEntities.Select(ConvertToRename).Where(r => r != null).ToList();
        }

        public async Task<IEnumerable<Common.Rename>> GetExpiringRenamesAsync(DateTime expirationDate)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<BotContext>();

            var renameEntities = await dbContext.Renames
                .Where(r => r.Status == (Models.RenameStatus)Common.RenameStatus.Active && 
                           r.Expiration.HasValue && 
                           r.Expiration.Value <= expirationDate)
                .OrderBy(r => r.Expiration)
                .ToListAsync();
                
            return renameEntities.Select(ConvertToRename).Where(r => r != null).ToList();
        }
        
        private Common.Rename ConvertToRename(Renames renameEntity)
        {
            if (renameEntity == null)
                return null;

            return new Common.Rename
            {
                Id = renameEntity.Id,
                OldName = renameEntity.OldName,
                NewName = renameEntity.NewName,
                AffectedUserId = renameEntity.AffectedUserId,
                RequestedUserId = renameEntity.RequestedUserId,
                Days = renameEntity.Days,
                Cost = renameEntity.Cost,
                Notified = renameEntity.Notified,
                Status = (Common.RenameStatus)renameEntity.Status,
                StartDate = renameEntity.StartDate,
                Expiration = renameEntity.Expiration,
                Timestamp = renameEntity.Timestamp
            };
        }
    }
} 