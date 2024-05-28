using Discord;

namespace SpottedBot.Utilities;

public static class Extensions
{
    public static bool IsValidSpot(this IUser user, IUser? spotter = null)
    {
        return true;
        return !(user.IsBot || user.IsWebhook || user.Equals(spotter));
    }
}