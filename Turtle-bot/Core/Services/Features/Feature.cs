using System.Threading.Tasks;

namespace Fitz.Core.Services.Features
{
    public abstract class Feature
    {
        public abstract string Name { get; }

        public abstract string Description { get; }

        public bool Enabled { get; private set; } = false;

        public virtual bool Protected => false;

        public virtual Task Initialize()
        {
            return Task.CompletedTask;
        }

        public virtual Task Enable()
        {
            Enabled = true;
            return Task.CompletedTask;
        }

        public virtual Task Disable()
        {
            if (!Protected)
            {
                Enabled = false;
            }

            return Task.CompletedTask;
        }

        public async Task Reload()
        {
            await Disable();
            await Enable();
        }
    }
}