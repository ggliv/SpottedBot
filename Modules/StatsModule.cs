using System.Text;
using Discord;
using Discord.Interactions;
using SpottedBot.Services;

namespace SpottedBot.Modules;

public class StatsModule(DatabaseService databaseService) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("stats", "Get the stats of a particular user")]
    public async Task Stats(IUser user)
    {
        var spotsCount = await databaseService.GetSpotterCount(Context.Guild, user);
        var spottedCount = await databaseService.GetSpottedCount(Context.Guild, user);

        await RespondAsync(embed: new EmbedBuilder
        {
            Author = new EmbedAuthorBuilder
            {
                Name = Context.Guild.Name,
                IconUrl = Context.Guild.IconUrl
            },
            Title = $"{user.Username}",
            ThumbnailUrl = user.GetDisplayAvatarUrl(),
            Fields =
            [
                new EmbedFieldBuilder
                {
                    Name = "Spots",
                    Value = spotsCount,
                    IsInline = true
                },

                new EmbedFieldBuilder
                {
                    Name = "Spotted",
                    Value = spottedCount,
                    IsInline = true
                },

                new EmbedFieldBuilder
                {
                    Name = "Score",
                    Value = spotsCount - spottedCount
                }
            ]
        }.Build());
    }

    [SlashCommand("leaderboard", "Show who's on top")]
    public async Task Leaderboard(int? limit = null)
    {
        var leaderboard = (await databaseService.GetScores(Context.Guild))
            .OrderBy(p => p.Value)
            .Reverse()
            .Take(Math.Max(1, limit ?? int.MaxValue))
            .ToList();
        var leaderboardMarkdown = new StringBuilder();
        var place = 1;
        foreach (var pair in leaderboard)
        {
            leaderboardMarkdown.Append($"{place}. <@{pair.Key}> ({pair.Value})\n");
            place += 1;
        }

        // TODO make this look nicer
        await RespondAsync(leaderboardMarkdown.ToString(), allowedMentions: AllowedMentions.None);
    }
}