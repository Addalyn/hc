using System;

public class SlashCommand_NameplateOvercon : SlashCommand
{
	public SlashCommand_NameplateOvercon()
		: base("/overcon", SlashCommandType.InGame)
	{
	}

	public override void OnSlashCommand(string arguments)
	{
		ActorData actorData = (!(GameFlowData.Get() != null)) ? null : GameFlowData.Get().activeOwnedActorData;
		if (!(actorData != null) || !(actorData.GetActorController() != null) || !(HUD_UI.Get() != null))
		{
			return;
		}
		while (true)
		{
			switch (3)
			{
			case 0:
				continue;
			}
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			if (!(UIOverconData.Get() != null))
			{
				return;
			}
			while (true)
			{
				switch (3)
				{
				case 0:
					continue;
				}
				if (!(ClientGameManager.Get() != null))
				{
					return;
				}
				while (true)
				{
					switch (2)
					{
					case 0:
						continue;
					}
					string[] array = arguments.Split((string[])null, StringSplitOptions.RemoveEmptyEntries);
					if (arguments.Length <= 0)
					{
						return;
					}
					while (true)
					{
						switch (7)
						{
						case 0:
							continue;
						}
						string text = array[0];
						int overconIdByName = UIOverconData.Get().GetOverconIdByName(text);
						if (overconIdByName > 0)
						{
							while (true)
							{
								switch (5)
								{
								case 0:
									break;
								default:
									if (ClientGameManager.Get().IsOverconUnlocked(overconIdByName))
									{
										while (true)
										{
											switch (5)
											{
											case 0:
												break;
											default:
												ClientGameManager.Get().SendUseOverconRequest(overconIdByName, text, actorData.ActorIndex, GameFlowData.Get().CurrentTurn);
												return;
											}
										}
									}
									TextConsole.Get().Write(string.Format(StringUtil.TR("OverconNotUnlocked", "SlashCommand"), text));
									return;
								}
							}
						}
						TextConsole.Get().Write(string.Format(StringUtil.TR("DidNotFindOvercon", "SlashCommand"), text));
						return;
					}
				}
			}
		}
	}
}
