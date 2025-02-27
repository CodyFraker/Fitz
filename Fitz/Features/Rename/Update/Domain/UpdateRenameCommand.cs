using System;
using Fitz.Features.Rename.Common;

namespace Fitz.Features.Rename.Update.Domain
{
    public class UpdateRenameCommand
    {
        public UpdateRenameCommand(int renameId, RenameStatus newStatus)
        {
            if (renameId <= 0)
                throw new ArgumentException("Rename ID must be greater than zero", nameof(renameId));

            RenameId = renameId;
            NewStatus = newStatus;
        }

        /// <summary>
        /// The ID of the rename to update.
        /// </summary>
        public int RenameId { get; }

        /// <summary>
        /// The new status to set for the rename.
        /// </summary>
        public RenameStatus NewStatus { get; }
    }
} 