using LobbyGameClientMessages;
using System;

internal class GroupRequestWindow
{
    private GroupConfirmationRequest m_request;
    private UIPartyInvitePopDialogBox m_dialogBox;
    private bool m_done;
    private DateTime m_expiration;

    internal GroupRequestWindow(GroupConfirmationRequest request)
    {
        m_request = request;
        m_expiration = DateTime.UtcNow + request.ExpirationTime;
        SpawnNewPopup();
    }

    private void SpawnNewPopup()
    {
        switch (m_request.Type)
        {
            case GroupConfirmationRequest.JoinType.InviteToFormGroup:
            {
                m_dialogBox = UIDialogPopupManager.OpenPartyInviteDialog(
                    StringUtil.TR("GroupRequest", "Global"),
                    string.Format(StringUtil.TR("InviteToFormGroup", "Global"), m_request.LeaderName),
                    StringUtil.TR("Join", "Global"),
                    StringUtil.TR("Reject", "Global"),
                    delegate { BlockPlayerFromGroupRequest(); },
                    delegate { Join(); },
                    delegate { Reject(); });
                return;
            }
            case GroupConfirmationRequest.JoinType.RequestToJoinGroup:
            {
                m_dialogBox = UIDialogPopupManager.OpenPartyInviteDialog(
                    StringUtil.TR("GroupRequest", "Global"),
                    string.Format(StringUtil.TR("RequestToJoinGroup", "Invite"), m_request.JoinerName),
                    StringUtil.TR("Approve", "Global"),
                    StringUtil.TR("Reject", "Global"),
                    delegate { BlockPlayerFromGroupRequest(); },
                    delegate { Join(); },
                    delegate { Reject(); });
                break;
            }
        }
    }

    public void CleanupWindow()
    {
        if (m_done)
        {
            return;
        }

        MarkDone();
        m_dialogBox.Close();
        GroupJoinManager.Get().SendGroupConfirmation(GroupInviteResponseType.OfferExpired, m_request);
    }

    private void MarkDone()
    {
        m_done = true;
        GroupJoinManager.Get().RemoveRequest(m_request);
    }

    private void Join()
    {
        MarkDone();
        GroupJoinManager.Get().SendGroupConfirmation(GroupInviteResponseType.PlayerAccepted, m_request);
    }

    private void Reject()
    {
        MarkDone();
        GroupJoinManager.Get().SendGroupConfirmation(GroupInviteResponseType.PlayerRejected, m_request);
    }

    private void BlockPlayerFromGroupRequest()
    {
        UIDialogPopupManager.OpenTwoButtonDialog(
            StringUtil.TR("BlockPlayer", "FriendList"),
            string.Format(StringUtil.TR("DoYouWantToBlock", "FriendList"), m_request.LeaderName),
            StringUtil.TR("Yes", "Global"),
            StringUtil.TR("No", "Global"),
            delegate
            {
                MarkDone();
                GroupJoinManager.Get().SendGroupConfirmation(GroupInviteResponseType.PlayerRejected, m_request);
                SlashCommands.Get().RunSlashCommand("/block", m_request.LeaderFullHandle);
            },
            delegate { SpawnNewPopup(); });
    }

    public bool HasExpired(DateTime time)
    {
        return !m_done && m_expiration < time;
    }
}