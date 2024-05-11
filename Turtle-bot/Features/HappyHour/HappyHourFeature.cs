using Fitz.Core.Services.Features;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.HappyHour
{
    public class HappyHourFeature : Feature
    {
        public override string Name => "Happy Hour";

        public override string Description => "Double the amount of beer when happy hour is active. (8PM-11PM EST)";

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