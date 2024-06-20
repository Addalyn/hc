using System;

namespace LobbyGameClientMessages
{
    [Serializable]
    public class GroupSuggestionResponse : WebSocketResponseMessage
    {
        public enum Status
        {
            // custom names
            Denied,
            Error,
            Success
        }

        public Status SuggestionStatus;
        public long SuggesterAccountId;
    }
}