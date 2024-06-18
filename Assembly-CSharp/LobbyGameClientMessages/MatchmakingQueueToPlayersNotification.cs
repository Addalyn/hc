using System;

namespace LobbyGameClientMessages
{
    [Serializable]
    public class MatchmakingQueueToPlayersNotification : WebSocketMessage
    {
        public enum MatchmakingQueueMessage
        {
            // names are custom
            None,
            QueueConfirmed,
            ReQueued,
            RuinedGameStartSoThrownOutOfQueue
        }

        public long AccountId;
        public MatchmakingQueueMessage MessageToSend;
        public GameType GameType;
        public ushort SubTypeMask;
    }
}