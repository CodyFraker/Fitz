using System;
using Microsoft.JSInterop;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Fitz.WebPortal.Shared
{
    public class ScreenSizeService
    {
        private readonly IJSRuntime _jsRuntime;
        public event Action<ScreenSize> ScreenSizeChanged;
        private bool _initialized = false;

        public ScreenSize CurrentScreenSize { get; private set; } = ScreenSize.Medium;

        public ScreenSizeService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task InitializeAsync()
        {
            try
            {
                // Only initialize if not already initialized
                if (!_initialized)
                {
                    await _jsRuntime.InvokeVoidAsync("screenSizeInterop.initialize", DotNetObjectReference.Create(this));
                    await UpdateScreenSizeAsync();
                    _initialized = true;
                }
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("prerendering"))
            {
                // Ignore JavaScript interop during prerendering
                // We'll initialize during OnAfterRender instead
            }
        }

        [JSInvokable]
        public void OnScreenSizeChanged(string screenSizeString)
        {
            if (Enum.TryParse<ScreenSize>(screenSizeString, true, out var screenSize))
            {
                CurrentScreenSize = screenSize;
                ScreenSizeChanged?.Invoke(screenSize);
            }
        }

        public async Task UpdateScreenSizeAsync()
        {
            try
            {
                var screenSizeString = await _jsRuntime.InvokeAsync<string>("screenSizeInterop.getScreenSize");
                if (Enum.TryParse<ScreenSize>(screenSizeString, true, out var screenSize))
                {
                    CurrentScreenSize = screenSize;
                    ScreenSizeChanged?.Invoke(screenSize);
                }
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("prerendering"))
            {
                // Ignore JavaScript interop during prerendering
            }
        }

        public bool IsSmallScreen => CurrentScreenSize == ScreenSize.ExtraSmall || CurrentScreenSize == ScreenSize.Small;
    }

    public enum ScreenSize
    {
        ExtraSmall,  // xs: 0px or larger
        Small,       // sm: 600px or larger
        Medium,      // md: 960px or larger
        Large,       // lg: 1280px or larger
        ExtraLarge   // xl: 1920px or larger
    }

    public static class ScreenSizeServiceExtensions
    {
        public static IServiceCollection AddScreenSizeService(this IServiceCollection services)
        {
            return services.AddScoped<ScreenSizeService>();
        }
    }
} 