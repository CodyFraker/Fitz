using System;
using System.Threading.Tasks;
using Fitz.Core.Contexts;
using Fitz.Features.Rename.Common;
using Fitz.Features.Rename.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Fitz.Features.Rename.Update.Persistance
{
    public class UpdateRenameRepository
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public UpdateRenameRepository(IServiceScopeFactory serviceScopeFactory)
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

        public async Task<Common.Rename> UpdateRenameAsync(Common.Rename rename)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<BotContext>();

            var renameEntity = ConvertToRenamesEntity(rename);
            dbContext.Renames.Update(renameEntity);
            await dbContext.SaveChangesAsync();

            return rename;
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

        private Renames ConvertToRenamesEntity(Common.Rename rename)
        {
            if (rename == null)
                return null;

            return new Renames
            {
                Id = rename.Id,
                OldName = rename.OldName,
                NewName = rename.NewName,
                AffectedUserId = rename.AffectedUserId,
                RequestedUserId = rename.RequestedUserId,
                Days = rename.Days,
                Cost = rename.Cost,
                Notified = rename.Notified,
                Status = (Models.RenameStatus)rename.Status,
                StartDate = rename.StartDate,
                Expiration = rename.Expiration,
                Timestamp = rename.Timestamp
            };
        }
    }
} 