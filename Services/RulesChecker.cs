using Discord;

namespace SpottedBot.Services;

public class RulesChecker(DatabaseService databaseService)
{
    private static readonly List<Func<IUser, IUser, bool>> ActiveRules = [IsTagBack, IsAlreadySpottedToday];

    public static bool PassesRules(IUser spotter, IUser spotted)
    {
        return ActiveRules.All(r => !r(spotter, spotted));
    }

    private static bool IsTagBack(IUser spotter, IUser spotted)
    {
        // TODO
        return false;
    }

    private static bool IsAlreadySpottedToday(IUser spotter, IUser spotted)
    {
        // TODO
        return false;
    }
}