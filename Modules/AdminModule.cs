using Discord;
using Discord.Interactions;
using SpottedBot.Services;

namespace SpottedBot.Modules;

[RequireOwner(Group = "Permission")]
[RequireRole("spotted-admin", Group = "Permission")]
[RequireOwner(Group = "Permission")]
public class AdminModule(DatabaseService databaseService) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("dump", "Dump the spots in this guild to a file")]
    public async Task Dump()
    {
        var dump = await databaseService.Dump(Context.Guild);
        if (dump == null)
        {
            await RespondAsync("Could not dump from database.", ephemeral: true);
            return;
        }

        await RespondWithFileAsync(new FileAttachment(dump));

        try
        {
            File.Delete(dump);
        }
        catch
        {
            // ignored
        }
    }

    [SlashCommand("new_season", "Start a new spotted season in this guild")]
    public async Task NewSeason(string startTime)
    {
        // TODO
    }

    [SlashCommand("set_channel", "Set the channel that spotted commands are allowed to be sent in")]
    public async Task SetChannel()
    {
        // TODO
    }

    [SlashCommand("set_role", "Set the role that spotted participants are required to have")]
    public async Task SetRole()
    {
        // TODO
    }
}