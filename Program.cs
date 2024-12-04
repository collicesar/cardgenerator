using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace CardGenerator;

class Program
{
    private static readonly ServiceProvider serviceProvider;
    static Program()
    {
        var services = new ServiceCollection();
        services.AddSingleton<Function>();

        var startup = new Startup();
        startup.ConfigureServices(services);
        serviceProvider = services.BuildServiceProvider();
    }

     public static async Task Main(string[] args)
    {
        // Create a scope for every request,
        // this allows creating scoped dependencies without creating a scope manually.
        using var scope = serviceProvider.CreateScope();
        var function = scope.ServiceProvider.GetRequiredService<Function>();

        await function.FunctionHandler(cardsToGenerate:80000);
    }
}