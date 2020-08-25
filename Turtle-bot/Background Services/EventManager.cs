namespace Fitz.BackgroundServices
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Fitz.BackgroundServices.Models;
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;

    public static class EventManager
    {
        public static void RegisterEventHandlers(IServiceProvider services)
        {
            foreach (Type type in Assembly.GetEntryAssembly()
                .GetTypes()
                .Where(t => typeof(IEventHandler).IsAssignableFrom(t) && t.GetInterfaces().Contains(typeof(IEventHandler))))
            {
                IEventHandler handler = ActivatorUtilities.CreateInstance(services, type) as IEventHandler;
                handler.RegisterListeners();
            }

            Log.Information($"EventHandlers processed");
        }
    }
}
