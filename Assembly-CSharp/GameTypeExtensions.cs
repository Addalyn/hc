public static class GameTypeExtensions
{
    public static bool IsHumanVsHumanGame(this GameType gameType)
    {
        return gameType == GameType.PvP
               || gameType == GameType.Ranked
               || gameType == GameType.NewPlayerPvP;
    }

    public static bool IsQueueable(this GameType gameType)
    {
        return gameType == GameType.Coop
               || gameType == GameType.PvP
               || gameType == GameType.Ranked
               || gameType == GameType.NewPlayerPvP;
    }

    public static bool IsAutoLaunchable(this GameType gameType)
    {
        return gameType == GameType.Custom
               || gameType == GameType.Practice
               || gameType == GameType.Tutorial;
    }

    public static bool TracksElo(this GameType gameType)
    {
        return gameType == GameType.PvP
               || gameType == GameType.Ranked
               || gameType == GameType.NewPlayerPvP;
    }

    public static bool AllowsLockedCharacters(this GameType gameType)
    {
        return gameType == GameType.Practice
               || gameType == GameType.Tutorial
               || gameType == GameType.NewPlayerSolo;
    }

    public static bool AllowsReconnect(this GameType gameType)
    {
        return gameType == GameType.Coop
               || gameType == GameType.PvP
               || gameType == GameType.Ranked
               || gameType == GameType.NewPlayerPvP
               || gameType == GameType.Custom;
    }

    public static string GetDisplayName(this GameType gameType)
    {
        switch (gameType)
        {
            case GameType.Custom:
                return StringUtil.TR("Custom", "Global");
            case GameType.Practice:
                return StringUtil.TR("Practice", "Global");
            case GameType.Tutorial:
                return StringUtil.TR("Tutorial", "Global");
            case GameType.Coop:
                return StringUtil.TR("VersusBots", "Global");
            case GameType.PvP:
            case GameType.NewPlayerPvP:
                return StringUtil.TR("PVP", "Global");
            case GameType.Duel:
                return StringUtil.TR("Duel", "Global");
            case GameType.Solo:
            case GameType.PvE:
            case GameType.NewPlayerSolo:
                return StringUtil.TR("PvE", "Global");
            case GameType.QuickPlay:
                return StringUtil.TR("QuickPlay", "Global");
            case GameType.Ranked:
                return StringUtil.TR("Ranked", "Global");
            default:
                return gameType + "#NotLocalized";
        }
    }
}