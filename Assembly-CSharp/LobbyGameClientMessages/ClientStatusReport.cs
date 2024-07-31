using System;

namespace LobbyGameClientMessages
{
    [Serializable]
    public class ClientStatusReport : WebSocketMessage
    {
        public enum ClientStatusReportType
        {
            Crash,
            Exception,
            CrashUserMessage,
            ExceptionUserMessage,
            BelowMinimumSpecComputer
        }

        public ClientStatusReportType Status;
        public string StatusDetails;
        public string DeviceIdentifier;
        public string UserMessage;
        public string FileDateTime;
    }
}