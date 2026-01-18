using Fitz.Core.Services.Features;
using System.Threading.Tasks;

namespace Fitz.Features.Favorability
{
    public class FavorabilityFeature : Feature
    {
        public override string Name => "Favorability";

        public override string Description => "Manages user favorability based on beer balance ratios.";

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
