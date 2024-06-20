using System;

namespace LobbyGameClientMessages
{
    [Serializable]
    public class GroupConfirmationRequest : WebSocketMessage
    {
        public enum JoinType
        {
            InviteToFormGroup,
            RequestToJoinGroup
        }

        public long GroupId;
        public string LeaderName;
        public string LeaderFullHandle;
        public string JoinerName;
        public long JoinerAccountId;
        public long ConfirmationNumber;
        public TimeSpan ExpirationTime;
        public JoinType Type;
    }
}