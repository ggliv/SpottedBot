using Discord;

namespace SpottedBot.Utilities;

public static class Extensions
{
    public static bool IsValidSpot(this IUser user, IUser? spotter = null)
    {
        // TODO
        return !(user.IsBot || user.IsWebhook || user.Equals(spotter));
    }
}