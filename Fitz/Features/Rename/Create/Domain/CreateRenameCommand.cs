using System;

namespace Fitz.Features.Rename.Create.Domain
{
    public class CreateRenameCommand
    {
        public CreateRenameCommand(
            ulong affectedUserId, 
            ulong requestedUserId, 
            string oldName, 
            string newName, 
            int? days, 
            int cost)
        {
            if (string.IsNullOrWhiteSpace(oldName))
                throw new ArgumentException("Old name cannot be empty", nameof(oldName));
            
            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("New name cannot be empty", nameof(newName));
            
            if (cost < 0)
                throw new ArgumentException("Cost cannot be negative", nameof(cost));
            
            if (days.HasValue && days.Value <= 0)
                throw new ArgumentException("Days must be greater than zero", nameof(days));

            AffectedUserId = affectedUserId;
            RequestedUserId = requestedUserId;
            OldName = oldName;
            NewName = newName;
            Days = days;
            Cost = cost;
        }

        /// <summary>
        /// User whose name has been requested to be changed.
        /// </summary>
        public ulong AffectedUserId { get; }

        /// <summary>
        /// The user who is paying to change a user's name.
        /// </summary>
        public ulong RequestedUserId { get; }

        /// <summary>
        /// The affected user's old name.
        /// </summary>
        public string OldName { get; }

        /// <summary>
        /// The affected user's new name.
        /// </summary>
        public string NewName { get; }

        /// <summary>
        /// Amount of days the name should be changed for. If null, the rename is permanent.
        /// </summary>
        public int? Days { get; }

        /// <summary>
        /// The total cost of the rename.
        /// </summary>
        public int Cost { get; }
    }
} 