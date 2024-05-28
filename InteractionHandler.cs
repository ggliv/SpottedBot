using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace SpottedBot;

public class InteractionHandler(DiscordSocketClient client, InteractionService handler, IServiceProvider services)
{
    public async Task InitializeAsync()
    {
        client.Ready += async () => await handler.RegisterCommandsGloballyAsync();
        handler.Log += m =>
        {
            Console.WriteLine(m);
            return Task.CompletedTask;
        };

        await handler.AddModulesAsync(Assembly.GetEntryAssembly(), services);

        client.InteractionCreated += HandleInteraction;
        handler.InteractionExecuted += HandleInteractionExecute;
    }


    private async Task HandleInteraction(SocketInteraction interaction)
    {
        try
        {
            var result = await handler.ExecuteCommandAsync(new SocketInteractionContext(client, interaction), services);

            // Due to async nature of InteractionFramework, the result here may always be success.
            // That's why we also need to handle the InteractionExecuted event.
            if (!result.IsSuccess)
                switch (result.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        break;
                    case InteractionCommandError.UnknownCommand:
                        break;
                    case InteractionCommandError.ConvertFailed:
                        break;
                    case InteractionCommandError.BadArgs:
                        break;
                    case InteractionCommandError.Exception:
                        break;
                    case InteractionCommandError.Unsuccessful:
                        break;
                    case InteractionCommandError.ParseFailed:
                        break;
                    case null:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
        }
        catch
        {
            // If Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
            // response, or at least let the user know that something went wrong during the command execution.
            if (interaction.Type is InteractionType.ApplicationCommand)
                await interaction.GetOriginalResponseAsync()
                    .ContinueWith(async msg => await msg.Result.DeleteAsync());
        }
    }

    private Task HandleInteractionExecute(ICommandInfo commandInfo, IInteractionContext context, IResult result)
    {
        if (result.IsSuccess) return Task.CompletedTask;

        switch (result.Error)
        {
            case InteractionCommandError.UnmetPrecondition:
                break;
            case InteractionCommandError.UnknownCommand:
                break;
            case InteractionCommandError.ConvertFailed:
                break;
            case InteractionCommandError.BadArgs:
                break;
            case InteractionCommandError.Exception:
                break;
            case InteractionCommandError.Unsuccessful:
                break;
            case InteractionCommandError.ParseFailed:
                break;
            case null:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return Task.CompletedTask;
    }
}