using System;
using System.Threading.Tasks;
using Fitz.Features.Rename.Common;
using Fitz.Features.Rename.Update.Persistance;

namespace Fitz.Features.Rename.Update.Domain
{
    public class UpdateRenameService
    {
        private readonly UpdateRenameRepository _repository;

        public UpdateRenameService(UpdateRenameRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<Common.Rename> UpdateRenameStatusAsync(UpdateRenameCommand command)
        {
            // Get the rename from the database
            var rename = await _repository.GetRenameByIdAsync(command.RenameId);
            if (rename == null)
            {
                throw new InvalidOperationException($"Rename with ID {command.RenameId} not found");
            }

            // Validate the status transition
            ValidateStatusTransition(rename.Status, command.NewStatus);

            // Update the rename status
            rename.Status = command.NewStatus;

            // If activating a rename, set the start date and calculate expiration
            if (command.NewStatus == RenameStatus.Active && rename.StartDate == null)
            {
                rename.StartDate = DateTime.UtcNow;
                
                if (rename.Days.HasValue)
                {
                    rename.Expiration = rename.StartDate.Value.AddDays(rename.Days.Value);
                }
            }

            // Save the updated rename
            var updatedRename = await _repository.UpdateRenameAsync(rename);

            return updatedRename;
        }

        private void ValidateStatusTransition(RenameStatus currentStatus, RenameStatus newStatus)
        {
            // Define valid status transitions
            bool isValidTransition = (currentStatus, newStatus) switch
            {
                // From Pending
                (RenameStatus.Pending, RenameStatus.Active) => true,
                (RenameStatus.Pending, RenameStatus.Expired) => true,

                // From Active
                (RenameStatus.Active, RenameStatus.Expired) => true,
                (RenameStatus.Active, RenameStatus.BoughtOut) => true,
                (RenameStatus.Active, RenameStatus.Permanent) => true,

                // No other transitions are valid
                _ => false
            };

            if (!isValidTransition)
            {
                throw new InvalidOperationException($"Invalid status transition from {currentStatus} to {newStatus}");
            }
        }
    }
} 