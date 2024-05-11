using Fitz.Core.Services.Features;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.BeerHunt
{
    public class BeerHuntFeature : Feature
    {
        public override string Name => "Beer Hunt";

        public override string Description => "Post a beer reaction to a random message.";

        public override Task Disable()
        {
            return base.Disable();
        }

        public override Task Enable()
        {
            return base.Enable();
        }
    }
}