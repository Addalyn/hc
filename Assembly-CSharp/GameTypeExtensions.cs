public static class GameTypeExtensions
{
	public static bool IsHumanVsHumanGame(this GameType gameType)
	{
		int result;
		if (gameType != GameType.PvP)
		{
			while (true)
			{
				switch (6)
				{
				case 0:
					continue;
				}
				break;
			}
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			if (gameType != GameType.Ranked)
			{
				while (true)
				{
					switch (6)
					{
					case 0:
						continue;
					}
					break;
				}
				result = ((gameType == GameType.NewPlayerPvP) ? 1 : 0);
				goto IL_002e;
			}
		}
		result = 1;
		goto IL_002e;
		IL_002e:
		return (byte)result != 0;
	}

	public static bool IsQueueable(this GameType gameType)
	{
		int result;
		if (gameType != GameType.Coop)
		{
			while (true)
			{
				switch (3)
				{
				case 0:
					continue;
				}
				break;
			}
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			if (gameType != GameType.PvP)
			{
				while (true)
				{
					switch (7)
					{
					case 0:
						continue;
					}
					break;
				}
				if (gameType != GameType.Ranked)
				{
					while (true)
					{
						switch (1)
						{
						case 0:
							continue;
						}
						break;
					}
					result = ((gameType == GameType.NewPlayerPvP) ? 1 : 0);
					goto IL_003c;
				}
			}
		}
		result = 1;
		goto IL_003c;
		IL_003c:
		return (byte)result != 0;
	}

	public static bool IsAutoLaunchable(this GameType gameType)
	{
		int result;
		if (gameType != 0 && gameType != GameType.Practice)
		{
			while (true)
			{
				switch (1)
				{
				case 0:
					continue;
				}
				break;
			}
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			result = ((gameType == GameType.Tutorial) ? 1 : 0);
		}
		else
		{
			result = 1;
		}
		return (byte)result != 0;
	}

	public static bool TracksElo(this GameType gameType)
	{
		int result;
		if (gameType != GameType.PvP)
		{
			while (true)
			{
				switch (3)
				{
				case 0:
					continue;
				}
				break;
			}
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			if (gameType != GameType.Ranked)
			{
				while (true)
				{
					switch (6)
					{
					case 0:
						continue;
					}
					break;
				}
				result = ((gameType == GameType.NewPlayerPvP) ? 1 : 0);
				goto IL_002e;
			}
		}
		result = 1;
		goto IL_002e;
		IL_002e:
		return (byte)result != 0;
	}

	public static bool AllowsLockedCharacters(this GameType gameType)
	{
		int result;
		if (gameType != GameType.Practice)
		{
			while (true)
			{
				switch (2)
				{
				case 0:
					continue;
				}
				break;
			}
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			if (gameType != GameType.Tutorial)
			{
				while (true)
				{
					switch (3)
					{
					case 0:
						continue;
					}
					break;
				}
				result = ((gameType == GameType.NewPlayerSolo) ? 1 : 0);
				goto IL_002d;
			}
		}
		result = 1;
		goto IL_002d;
		IL_002d:
		return (byte)result != 0;
	}

	public static bool AllowsReconnect(this GameType gameType)
	{
		int result;
		if (gameType != GameType.Coop)
		{
			while (true)
			{
				switch (7)
				{
				case 0:
					continue;
				}
				break;
			}
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			if (gameType != GameType.PvP)
			{
				while (true)
				{
					switch (1)
					{
					case 0:
						continue;
					}
					break;
				}
				if (gameType != GameType.Ranked)
				{
					while (true)
					{
						switch (4)
						{
						case 0:
							continue;
						}
						break;
					}
					if (gameType != GameType.NewPlayerPvP)
					{
						while (true)
						{
							switch (3)
							{
							case 0:
								continue;
							}
							break;
						}
						result = ((gameType == GameType.Custom) ? 1 : 0);
						goto IL_004a;
					}
				}
			}
		}
		result = 1;
		goto IL_004a;
		IL_004a:
		return (byte)result != 0;
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
			return gameType.ToString() + "#NotLocalized";
		}
	}
}
