using System;
using System.Threading.Tasks;
using Fitz.Features.Rename.Common;
using Fitz.Features.Rename.Notify.Persistance;

namespace Fitz.Features.Rename.Notify.Domain
{
    public class NotifyRenameService
    {
        private readonly NotifyRenameRepository _repository;

        public NotifyRenameService(NotifyRenameRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<Common.Rename> NotifyRenameAsync(NotifyRenameCommand command)
        {
            // Get the rename from the database
            var rename = await _repository.GetRenameByIdAsync(command.RenameId);
            if (rename == null)
            {
                throw new InvalidOperationException($"Rename with ID {command.RenameId} not found");
            }

            // Check if the rename has already been notified
            if (rename.Notified)
            {
                return rename;
            }

            // Mark the rename as notified
            rename.Notified = true;

            // Save the updated rename
            var updatedRename = await _repository.UpdateRenameAsync(rename);

            return updatedRename;
        }

        public async Task<Common.Rename[]> GetUnnotifiedRenamesAsync()
        {
            var unnotifiedRenames = await _repository.GetUnnotifiedRenamesAsync();
            return unnotifiedRenames;
        }
    }
} 