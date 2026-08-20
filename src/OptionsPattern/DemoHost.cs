using Microsoft.Extensions.Hosting;

namespace OptionsPattern;

/// <summary>
/// Builds a <see cref="HostApplicationBuilder"/> the way every demo in this project needs it.
/// </summary>
/// <remarks>
/// <see cref="Host.CreateApplicationBuilder()"/>'s default <c>appsettings.json</c> loader is
/// rooted at <see cref="Directory.GetCurrentDirectory"/> -- the process's working directory --
/// not the built assembly's directory. Run this project with
/// <c>dotnet run --project OptionsPattern</c> from anywhere other than the
/// <c>OptionsPattern</c> folder itself (for example, from the solution's <c>src/</c> directory,
/// which is how every other project in this repo is documented to be run) and the file is
/// silently not found -- every demo would then report the class defaults instead of the
/// bound values, exactly the "appsettings.json never binds" bug this project used to have for
/// an entirely different reason. Explicitly rooting the content path at
/// <see cref="AppContext.BaseDirectory"/> (where <c>appsettings.json</c> is copied to on build)
/// makes the demos work the same way regardless of the caller's current directory.
/// </remarks>
internal static class DemoHost
{
    /// <summary>Creates a <see cref="HostApplicationBuilder"/> whose configuration reliably finds this project's <c>appsettings.json</c>.</summary>
    public static HostApplicationBuilder CreateBuilder() =>
        Host.CreateApplicationBuilder(new HostApplicationBuilderSettings { ContentRootPath = AppContext.BaseDirectory });
}
