using System;

namespace Fitz.Features.Rename.Get.Domain
{
    public class GetRenameCommand
    {
        public GetRenameCommand(int? renameId = null, ulong? userId = null)
        {
            if (renameId == null && userId == null)
            {
                throw new ArgumentException("Either renameId or userId must be provided");
            }

            RenameId = renameId;
            UserId = userId;
        }

        /// <summary>
        /// The ID of the rename to retrieve. Optional if UserId is provided.
        /// </summary>
        public int? RenameId { get; }

        /// <summary>
        /// The Discord user ID to retrieve renames for. Optional if RenameId is provided.
        /// </summary>
        public ulong? UserId { get; }
    }
} 