using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Discord;
using Discord.Interactions;
using SpottedBot.Services;
using SpottedBot.Utilities;

namespace SpottedBot.Modules;

public partial class PlayerModule(DatabaseService databaseService) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("info", "Get information about the rules, the bot, and more")]
    public async Task Info()
    {
        // TODO
    }

    [SlashCommand("spot", "Spot another player")]
    public async Task Spot(IAttachment image, IUser spotted0, IUser? spotted1 = null,
        IUser? spotted2 = null, IUser? spotted3 = null, IUser? spotted4 = null, IUser? spotted5 = null,
        IUser? spotted6 = null, IUser? spotted7 = null)
    {
        if (image.ContentType == null || !AvMimeRegex().IsMatch(image.ContentType))
        {
            await RespondAsync("Invalid attachment, need either an image or a video.", ephemeral: true);
            return;
        }

        var spotted = new[] { spotted0, spotted1, spotted2, spotted3, spotted4, spotted5, spotted6, spotted7 }
            .Where(u => u != null).Select(u => u!)
            .Where(u => u.IsValidSpot(Context.User))
            .Distinct()
            .ToImmutableArray();

        if (spotted.IsEmpty)
        {
            await RespondAsync("Invalid arguments, need at least one valid person to spot.", ephemeral: true);
            return;
        }

        await databaseService.LogSpots(Context.Guild, Context.User, spotted, image);

        await RespondAsync($"{Context.User} spotted {string.Join(", ", spotted.Select(s => s.Mention))}. {image.Url}");
    }

    [GeneratedRegex("(image|video).*")]
    private static partial Regex AvMimeRegex();
}