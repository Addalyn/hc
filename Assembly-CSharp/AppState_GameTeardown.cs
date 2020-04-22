public class AppState_GameTeardown : AppState
{
	private static AppState_GameTeardown s_instance;

	private GameResult m_lastGameResult;

	private string m_lastLobbyErrorMessage;

	private LobbyGameInfo m_lastGameInfo;

	private UIDialogBox m_messageBox;

	public static AppState_GameTeardown Get()
	{
		return s_instance;
	}

	public static void Create()
	{
		AppState.Create<AppState_GameTeardown>();
	}

	private void Awake()
	{
		s_instance = this;
	}

	public void Enter(GameResult gameResult, string lastLobbyErrorMessage)
	{
		m_lastGameResult = gameResult;
		m_lastLobbyErrorMessage = lastLobbyErrorMessage;
		base.Enter();
	}

	protected override void OnEnter()
	{
		GameManager.Get().StopGame();
		m_lastGameInfo = GameManager.Get().GameInfo;
		ClientGameManager clientGameManager = ClientGameManager.Get();
		clientGameManager.LeaveGame(false, m_lastGameResult);
		if (HUD_UI.Get() != null)
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
			HUD_UI.Get().m_mainScreenPanel.SetVisible(false);
			UIManager.SetGameObjectActive(HUD_UI.Get().m_textConsole, false);
		}
		UILoadingScreenPanel.Get().SetVisible(false);
		GameEventManager.Get().FireEvent(GameEventManager.EventType.GameTeardown, null);
		if (HUD_UI.Get() != null)
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
			HUD_UI.Get().GameTeardown();
		}
		if (ClientGamePrefabInstantiator.Get() != null)
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
			ClientGamePrefabInstantiator.Get().DestroyInstantiations();
		}
		else
		{
			Log.Error("ClientGamePrefabInstantiator reference not set on game teardown");
		}
		if (m_lastGameResult == GameResult.ServerCrashed)
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
			m_messageBox = UIDialogPopupManager.OpenOneButtonDialog(string.Empty, StringUtil.TR("ServerShutDown", "Global"), StringUtil.TR("Ok", "Global"), delegate
			{
				GoToFrontend();
			});
		}
		else if (m_lastGameResult.IsConnectionErrorResult())
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
			if (!m_lastLobbyErrorMessage.IsNullOrEmpty())
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
				if (clientGameManager.AllowRelogin)
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
					string empty = string.Empty;
					string description = string.Format(StringUtil.TR("PressOkToReconnect", "Global"), m_lastLobbyErrorMessage);
					string leftButtonLabel = StringUtil.TR("Ok", "Global");
					string rightButtonLabel = StringUtil.TR("Cancel", "Global");
					UIDialogBox.DialogButtonCallback leftButtonCallback = delegate
					{
						GoToFrontend();
					};
					if (_003C_003Ef__am_0024cache0 == null)
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
						_003C_003Ef__am_0024cache0 = delegate
						{
							AppState_Shutdown.Get().Enter();
						};
					}
					m_messageBox = UIDialogPopupManager.OpenTwoButtonDialog(empty, description, leftButtonLabel, rightButtonLabel, leftButtonCallback, _003C_003Ef__am_0024cache0);
				}
				else
				{
					m_messageBox = UIDialogPopupManager.OpenOneButtonDialog(string.Empty, string.Format(StringUtil.TR("PressOkToExit", "Global"), m_lastLobbyErrorMessage), StringUtil.TR("Ok", "Global"), delegate
					{
						AppState_Shutdown.Get().Enter();
					});
				}
			}
			else
			{
				m_messageBox = UIDialogPopupManager.OpenOneButtonDialog(string.Empty, m_lastGameResult.GetErrorMessage(), StringUtil.TR("Ok", "Global"), delegate
				{
					GoToFrontend();
				});
			}
		}
		else
		{
			GoToFrontend();
		}
		m_lastLobbyErrorMessage = null;
		if (!clientGameManager.PlayerObjectStartedOnClient)
		{
			while (true)
			{
				switch (5)
				{
				case 0:
					continue;
				}
				break;
			}
			if (!clientGameManager.InGameUIActivated)
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
				if (!clientGameManager.VisualSceneLoaded)
				{
					if (!clientGameManager.DesignSceneStarted)
					{
						return;
					}
					while (true)
					{
						switch (5)
						{
						case 0:
							continue;
						}
						break;
					}
				}
			}
		}
		string message = $"CGM UI state in a bad state: {clientGameManager.PlayerObjectStartedOnClient} {clientGameManager.InGameUIActivated} {clientGameManager.VisualSceneLoaded} {clientGameManager.DesignSceneStarted}";
		clientGameManager.PlayerObjectStartedOnClient = false;
		clientGameManager.InGameUIActivated = false;
		clientGameManager.VisualSceneLoaded = false;
		clientGameManager.DesignSceneStarted = false;
		Log.Info(message);
	}

	private void GoToFrontend()
	{
		AppState_FrontendLoadingScreen.NextState nextState;
		if (m_lastGameInfo.GameConfig.GameType != GameType.Tutorial)
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
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			if (m_lastGameInfo.GameConfig.GameType != 0)
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
				if (m_lastGameInfo.GameConfig.GameType != GameType.NewPlayerSolo)
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
					if (m_lastGameResult != GameResult.ClientIdleTimeout && m_lastGameResult != GameResult.SkippedTurnsIdleTimeout)
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
						nextState = AppState_FrontendLoadingScreen.NextState.GoToCharacterSelect;
						goto IL_0087;
					}
				}
			}
		}
		nextState = AppState_FrontendLoadingScreen.NextState.GoToLandingPage;
		goto IL_0087;
		IL_0087:
		AppState_FrontendLoadingScreen.Get().Enter(m_lastLobbyErrorMessage, nextState);
	}

	protected override void OnLeave()
	{
		if (!(m_messageBox != null))
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
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			m_messageBox.Close();
			m_messageBox = null;
			return;
		}
	}

	private void Update()
	{
	}
}
