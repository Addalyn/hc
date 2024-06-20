using LobbyGameClientMessages;
using System;
using System.Collections.Generic;

internal class GroupJoinManager
{
    private static GroupJoinManager s_instance;
    private Dictionary<string, GroupRequestWindow> m_pendingWindows = new Dictionary<string, GroupRequestWindow>();
    private Dictionary<string, LeakyBucket> m_restrictSpammers = new Dictionary<string, LeakyBucket>();

    public static GroupJoinManager Get()
    {
        if (s_instance == null)
        {
            s_instance = new GroupJoinManager();
        }

        return s_instance;
    }

    public void Update()
    {
        DateTime utcNow = DateTime.UtcNow;
        List<GroupRequestWindow> expiredWindows = new List<GroupRequestWindow>();
        foreach (GroupRequestWindow value in m_pendingWindows.Values)
        {
            if (value.HasExpired(utcNow))
            {
                expiredWindows.Add(value);
            }
        }

        expiredWindows.ForEach(delegate(GroupRequestWindow p) { p.CleanupWindow(); });
    }

    internal void AddRequest(GroupConfirmationRequest request)
    {
        if (UIFrontEnd.Get() == null)
        {
            return;
        }

        if (UIFrontEnd.Get().m_landingPageScreen != null && UIFrontEnd.Get().m_landingPageScreen.m_inCustomGame)
        {
            SendGroupConfirmation(GroupInviteResponseType.PlayerInCustomMatch, request);
        }
        else if (m_pendingWindows.ContainsKey(request.LeaderFullHandle))
        {
            SendGroupConfirmation(GroupInviteResponseType.PlayerStillAwaitingPreviousQuery, request);
        }
        else if (request == null) // TODO CLIENT too late to check
        {
            throw new Exception("request is null");
        }
        else if (m_restrictSpammers.TryGetValue(request.LeaderFullHandle, out LeakyBucket spamBucket)
                 && spamBucket != null
                 && !spamBucket.CanAdd())
        {
            SendGroupConfirmation(GroupInviteResponseType.RequestorSpamming, request);
        }
        else
        {
            m_pendingWindows.Add(request.LeaderFullHandle, new GroupRequestWindow(request));
        }
    }

    internal void RemoveRequest(GroupConfirmationRequest request)
    {
        m_pendingWindows.Remove(request.LeaderFullHandle);
    }

    internal void SendGroupConfirmation(GroupInviteResponseType status, GroupConfirmationRequest request)
    {
        if (status == GroupInviteResponseType.PlayerRejected)
        {
            if (!m_restrictSpammers.TryGetValue(request.LeaderFullHandle, out LeakyBucket spamBucket))
            {
                spamBucket = new LeakyBucket(2.0, TimeSpan.FromMinutes(10.0));
                m_restrictSpammers.Add(request.LeaderFullHandle, spamBucket);
            }

            spamBucket.Add();
        }

        ClientGameManager.Get().LobbyInterface.SendMessage(
            new GroupConfirmationResponse
            {
                Acceptance = status,
                GroupId = request.GroupId,
                ConfirmationNumber = request.ConfirmationNumber,
                JoinerAccountId = request.JoinerAccountId
            });
    }
}