using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpottedBot.Services;

namespace SpottedBot;

public class Program
{
    public static async Task Main()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables("SpottedBot_")
            .AddJsonFile("config.json", true)
            .Build();

        var services = new ServiceCollection()
            .AddSingleton(configuration)
            .AddSingleton<DiscordSocketConfig>()
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton(sp =>
                new InteractionService(sp.GetRequiredService<DiscordSocketClient>(), new InteractionServiceConfig()))
            .AddSingleton<InteractionHandler>()
            .AddSingleton<DatabaseService>()
            .AddSingleton<RulesChecker>()
            .BuildServiceProvider();

        var client = services.GetRequiredService<DiscordSocketClient>();

        client.Log += m =>
        {
            Console.WriteLine(m);
            return Task.CompletedTask;
        };

        services.GetRequiredService<DatabaseService>().EnsureDatabaseExists().Wait();

        await services.GetRequiredService<InteractionHandler>().InitializeAsync();

        await client.LoginAsync(TokenType.Bot, configuration["token"]);
        await client.StartAsync();

        await Task.Delay(Timeout.InfiniteTimeSpan);
    }
}