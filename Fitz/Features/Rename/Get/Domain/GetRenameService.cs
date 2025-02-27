using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fitz.Features.Rename.Common;
using Fitz.Features.Rename.Get.Persistance;

namespace Fitz.Features.Rename.Get.Domain
{
    public class GetRenameService
    {
        private readonly GetRenameRepository _repository;

        public GetRenameService(GetRenameRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<Common.Rename> GetRenameByIdAsync(GetRenameCommand command)
        {
            if (command.RenameId.HasValue)
            {
                var rename = await _repository.GetRenameByIdAsync(command.RenameId.Value);
                return rename;
            }
            else if (command.UserId.HasValue)
            {
                var rename = await _repository.GetActiveRenameByUserIdAsync(command.UserId.Value);
                return rename;
            }

            throw new ArgumentException("Either RenameId or UserId must be provided");
        }

        public async Task<IEnumerable<Common.Rename>> GetRenameHistoryByUserIdAsync(ulong userId)
        {
            var renames = await _repository.GetRenameHistoryByUserIdAsync(userId);
            if (renames == null || !renames.Any())
            {
                return new List<Common.Rename>();
            }
            return renames;
        }

        public async Task<IEnumerable<Common.Rename>> GetPendingRenamesAsync()
        {
            var pendingRenames = await _repository.GetRenamesByStatusAsync(RenameStatus.Pending);
            return pendingRenames ?? new List<Common.Rename>();
        }

        public async Task<IEnumerable<Common.Rename>> GetActiveRenamesAsync()
        {
            var activeRenames = await _repository.GetRenamesByStatusAsync(RenameStatus.Active);
            return activeRenames ?? new List<Common.Rename>();
        }

        public async Task<IEnumerable<Common.Rename>> GetExpiringRenamesAsync(int daysUntilExpiration)
        {
            if (daysUntilExpiration < 0)
            {
                throw new ArgumentException("Days until expiration must be a non-negative value", nameof(daysUntilExpiration));
            }

            var expirationDate = DateTime.UtcNow.AddDays(daysUntilExpiration);
            var expiringRenames = await _repository.GetExpiringRenamesAsync(expirationDate);
            return expiringRenames ?? new List<Common.Rename>();
        }
    }
} 