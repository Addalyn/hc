public class AppState_InGameEnding : AppStateInGame
{
	private static AppState_InGameEnding s_instance;

	public static AppState_InGameEnding Get()
	{
		return s_instance;
	}

	public static void Create()
	{
		AppState.Create<AppState_InGameEnding>();
	}

	private void Awake()
	{
		s_instance = this;
	}

	protected override void OnEnter()
	{
		if (HUD_UI.Get() != null)
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
			HUD_UI.Get().m_mainScreenPanel.SetVisible(false);
		}
		if (UIGameOverScreen.Get() != null)
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
			UIGameOverScreen.Get().SetVisible(true);
			AudioManager.GetMixerSnapshotManager().SetMix_GameOver();
		}
		RegisterGameStoppedHandler();
	}

	protected override void OnLeave()
	{
		UnregisterGameStoppedHandler();
	}
}
