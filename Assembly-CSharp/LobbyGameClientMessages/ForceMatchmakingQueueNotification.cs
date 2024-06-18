using System;

namespace LobbyGameClientMessages
{
    [Serializable]
    public class ForceMatchmakingQueueNotification : WebSocketMessage
    {
        public enum ActionType
        {
            Unknown,
            Join,
            Leave
        }

        public ActionType Action;
        public GameType GameType;
    }
}