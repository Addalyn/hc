using System;

namespace LobbyGameClientMessages
{
    [Serializable]
    public class ClientFeedbackReport : WebSocketMessage
    {
        public enum FeedbackReason
        {
            None,
            Suggestion,
            Bug,
            UnsportsmanlikeConduct,
            VerbalHarassment,
            LeavingTheGameAFK,
            HateSpeech,
            IntentionallyFeeding,
            SpammingAdvertising,
            OffensiveName,
            Other,
            Botting
        }

        public FeedbackReason Reason;
        public string ReportedPlayerHandle;
        public long ReportedPlayerAccountId;
        public string Message;

        public bool IsMutableReport()
        {
            return Reason == FeedbackReason.UnsportsmanlikeConduct
                   || Reason == FeedbackReason.VerbalHarassment
                   || Reason == FeedbackReason.HateSpeech
                   || Reason == FeedbackReason.SpammingAdvertising
                   || Reason == FeedbackReason.OffensiveName
                   || Reason == FeedbackReason.Other;
        }
    }
}