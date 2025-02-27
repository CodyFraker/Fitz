using System;
using System.Threading.Tasks;
using Fitz.Features.Bank;
using Fitz.Features.Rename.Common;
using Fitz.Features.Rename.Create.Persistance;

namespace Fitz.Features.Rename.Create.Domain
{
    public class CreateRenameService
    {
        private readonly CreateRenameRepository _repository;
        private readonly BankService _bankService;

        public CreateRenameService(
            CreateRenameRepository repository,
            BankService bankService)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _bankService = bankService ?? throw new ArgumentNullException(nameof(bankService));
        }

        public async Task<Common.Rename> CreateRenameAsync(CreateRenameCommand command)
        {
            // Check if the affected user already has an active rename
            var existingRename = await _repository.GetActiveRenameByUserIdAsync(command.AffectedUserId);
            if (existingRename != null)
            {
                throw new InvalidOperationException($"User already has an active rename that expires on {existingRename.Expiration:yyyy-MM-dd}");
            }

            // Deduct the cost from the requester's account
            try
            {
                await _bankService.PurchaseRenameAsync(command.RequestedUserId, command.Cost);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to process payment: {ex.Message}");
            }

            // Create the rename entity
            var rename = new Common.Rename
            {
                AffectedUserId = command.AffectedUserId,
                RequestedUserId = command.RequestedUserId,
                OldName = command.OldName,
                NewName = command.NewName,
                Days = command.Days,
                Cost = command.Cost,
                Status = RenameStatus.Pending,
                Timestamp = DateTime.UtcNow,
                Notified = false
            };

            // If days is specified, calculate the expiration date
            if (command.Days.HasValue)
            {
                // Start date will be set when the rename is activated
                rename.Expiration = null;
            }
            else
            {
                // Permanent rename
                rename.Status = RenameStatus.Permanent;
            }

            // Save the rename to the database
            var createdRename = await _repository.CreateRenameAsync(rename);

            return createdRename;
        }
    }
} 