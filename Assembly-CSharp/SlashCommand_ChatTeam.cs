﻿using System;

public class SlashCommand_ChatTeam : SlashCommand
{
	public SlashCommand_ChatTeam() : base("/team", SlashCommandType.Everywhere)
	{
	}

	public override void OnSlashCommand(string arguments)
	{
		if (arguments.IsNullOrEmpty() || ClientGameManager.Get() == null)
		{
			return;
		}
		ClientGameManager.Get().SendChatNotification(null, ConsoleMessageType.TeamChat, arguments);
	}
}
