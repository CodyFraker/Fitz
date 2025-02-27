using System;

namespace Fitz.Features.Rename.Notify.Domain
{
    public class NotifyRenameCommand
    {
        public NotifyRenameCommand(int renameId)
        {
            if (renameId <= 0)
                throw new ArgumentException("Rename ID must be greater than zero", nameof(renameId));

            RenameId = renameId;
        }

        /// <summary>
        /// The ID of the rename to notify about.
        /// </summary>
        public int RenameId { get; }
    }
} 