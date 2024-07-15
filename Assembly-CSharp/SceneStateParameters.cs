using System;

public class SceneStateParameters
{
    public UIManager.ClientState? NewClientGameState;

    public static bool IsHUDHidden => UIScreenManager.Get().GetHideHUDCompletely();

    public static bool IsGroupLeader =>
        ClientGameManager.Get().GroupInfo != null
        && ClientGameManager.Get().GroupInfo.InAGroup
        && ClientGameManager.Get().GroupInfo.IsLeader;

    public static bool IsGroupSubordinate =>
        ClientGameManager.Get().GroupInfo != null
        && ClientGameManager.Get().GroupInfo.InAGroup
        && !ClientGameManager.Get().GroupInfo.IsLeader;

    public static bool IsInQueue => GameManager.Get().QueueInfo != null;

    public static bool IsWaitingForGroup
    {
        get
        {
            if (GameManager.Get().QueueInfo != null
                || GameManager.Get().GameStatus == GameStatus.LoadoutSelecting
                || GameManager.Get().GameStatus == GameStatus.FreelancerSelecting
                || ClientGameManager.Get().GroupInfo == null
                || !ClientGameManager.Get().GroupInfo.InAGroup)
            {
                return false;
            }

            foreach (UpdateGroupMemberData member in ClientGameManager.Get().GroupInfo.Members)
            {
                if (member.AccountID != ClientGameManager.Get().GetPlayerAccountData().AccountId)
                {
                    continue;
                }

                if (AppState_GroupCharacterSelect.Get() == AppState.GetCurrent()
                    && AppState_GroupCharacterSelect.Get().IsReady())
                {
                    return true;
                }

                if (AppState_CharacterSelect.Get() == AppState.GetCurrent()
                    && AppState_CharacterSelect.IsReady())
                {
                    return true;
                }
            }

            return false;
        }
    }

    public static bool IsInGameLobby =>
        GameManager.Get().GameInfo != null
        && GameManager.Get().PlayerInfo != null
        && GameManager.Get().GameInfo.GameStatus != GameStatus.Stopped
        && GameManager.Get().GameStatus != GameStatus.None;

    public static bool IsInCustomGame =>
        GameManager.Get().GameInfo != null
        && GameManager.Get().GameInfo.GameStatus != GameStatus.Stopped
        && GameManager.Get().GameInfo.GameConfig != null
        && GameManager.Get().GameInfo.GameConfig.GameType == GameType.Custom;

    public static bool PracticeGameTypeSelectedForQueue =>
        !IsInGameLobby
        && ClientGameManager.Get().GroupInfo != null
        && ClientGameManager.Get().GroupInfo.SelectedQueueType == GameType.Practice;

    public static TimeSpan TimeInQueue => ClientGameManager.Get().QueueEntryTime == DateTime.MinValue
        ? TimeSpan.FromMinutes(0.0)
        : DateTime.UtcNow - ClientGameManager.Get().QueueEntryTime;

    public static CharacterType SelectedCharacterInGroup =>
        ClientGameManager.Get().GroupInfo != null && ClientGameManager.Get().GroupInfo.InAGroup
            ? ClientGameManager.Get().GroupInfo.ChararacterInfo.CharacterType
            : CharacterType.None;

    public static CharacterType SelectedCharacterFromPlayerData =>
        ClientGameManager.Get().IsPlayerAccountDataAvailable()
            ? ClientGameManager.Get().GetPlayerAccountData().AccountComponent.LastCharacter
            : CharacterType.None;

    public static CharacterType SelectedCharacterFromGameInfo =>
        GameManager.Get().GameInfo != null
        && GameManager.Get().GameInfo.GameStatus != GameStatus.Stopped
        && GameManager.Get().PlayerInfo != null
            ? GameManager.Get().PlayerInfo.CharacterType
            : CharacterType.None;
}