using Fitz.Core.Services.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fitz.Features.Rename
{
    public sealed class RenameFeature : Feature
    {
        public override string Name => "User Renaming";

        public override string Description => "Users can use their beer to rename other users within the guild.";
    }
}