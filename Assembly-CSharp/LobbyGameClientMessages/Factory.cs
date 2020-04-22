using System;
using System.Collections.Generic;

namespace LobbyGameClientMessages
{
	public static class Factory
	{
		private static WebSocketMessageFactory s_instance;

		public static IEnumerable<Type> MessageTypes => new Type[284]
		{
			typeof(AssignGameClientRequest),
			typeof(AssignGameClientResponse),
			typeof(RegisterGameClientRequest),
			typeof(RegisterGameClientResponse),
			typeof(LobbyServerReadyNotification),
			typeof(LobbyStatusNotification),
			typeof(LobbyGameplayOverridesNotification),
			typeof(LobbyCustomGamesNotification),
			typeof(SubscribeToCustomGamesRequest),
			typeof(UnsubscribeFromCustomGamesRequest),
			typeof(SetRegionRequest),
			typeof(SyncNotification),
			typeof(CreateGameRequest),
			typeof(CreateGameResponse),
			typeof(JoinGameRequest),
			typeof(JoinGameResponse),
			typeof(LeaveGameRequest),
			typeof(LeaveGameResponse),
			typeof(JoinMatchmakingQueueRequest),
			typeof(JoinMatchmakingQueueResponse),
			typeof(LeaveMatchmakingQueueRequest),
			typeof(LeaveMatchmakingQueueResponse),
			typeof(UpdateMatchmakingQueueRequest),
			typeof(ForceMatchmakingQueueNotification),
			typeof(MatchmakingQueueAssignmentNotification),
			typeof(MatchmakingQueueStatusNotification),
			typeof(GameAssignmentNotification),
			typeof(GameInfoNotification),
			typeof(GameStatusNotification),
			typeof(GameCheatUpdateRequest),
			typeof(GameCheatUpdateResponse),
			typeof(PlayerGroupInfoUpdateRequest),
			typeof(PlayerGroupInfoUpdateResponse),
			typeof(PlayerInfoUpdateRequest),
			typeof(PlayerInfoUpdateResponse),
			typeof(GameInfoUpdateRequest),
			typeof(GameInfoUpdateResponse),
			typeof(GameInvitationRequest),
			typeof(GameInvitationResponse),
			typeof(GameSpectatorRequest),
			typeof(GameSpectatorResponse),
			typeof(GameInviteConfirmationRequest),
			typeof(GameInviteConfirmationResponse),
			typeof(CrashReportArchiveNameRequest),
			typeof(CrashReportArchiveNameResponse),
			typeof(ClientStatusReport),
			typeof(ClientErrorReport),
			typeof(ClientErrorSummary),
			typeof(ErrorReportSummaryRequest),
			typeof(ErrorReportSummaryResponse),
			typeof(ClientFeedbackReport),
			typeof(ClientPerformanceReport),
			typeof(PlayerMatchDataRequest),
			typeof(PlayerMatchDataResponse),
			typeof(GameplayOverridesRequest),
			typeof(GameplayOverridesResponse),
			typeof(PlayerAccountDataUpdateNotification),
			typeof(ForcedCharacterChangeFromServerNotification),
			typeof(PlayerCharacterDataUpdateNotification),
			typeof(InventoryComponentUpdateNotification),
			typeof(PlayerCharacterFeedbackRequest),
			typeof(PlayerFeedbackData),
			typeof(PreviousGameInfoRequest),
			typeof(PreviousGameInfoResponse),
			typeof(RejoinGameRequest),
			typeof(RejoinGameResponse),
			typeof(DiscordGetRpcTokenRequest),
			typeof(DiscordGetRpcTokenResponse),
			typeof(DiscordGetAccessTokenRequest),
			typeof(DiscordGetAccessTokenResponse),
			typeof(DiscordJoinServerRequest),
			typeof(DiscordJoinServerResponse),
			typeof(DiscordLeaveServerRequest),
			typeof(DiscordLeaveServerResponse),
			typeof(FacebookGetUserTokenRequest),
			typeof(FacebookGetUserTokenResponse),
			typeof(FacebookAccessTokenNotification),
			typeof(PurchaseLoadoutSlotRequest),
			typeof(PurchaseLoadoutSlotResponse),
			typeof(PurchaseModRequest),
			typeof(PurchaseModResponse),
			typeof(PurchaseModTokenRequest),
			typeof(PurchaseModTokenResponse),
			typeof(BalancedTeamRequest),
			typeof(BalancedTeamResponse),
			typeof(RefreshBankDataRequest),
			typeof(RefreshBankDataResponse),
			typeof(BankBalanceChangeNotification),
			typeof(SeasonStatusNotification),
			typeof(SeasonStatusConfirmed),
			typeof(ChapterStatusNotification),
			typeof(ChatNotification),
			typeof(ChatSyncNotification),
			typeof(UIActionNotification),
			typeof(UseGGPackNotification),
			typeof(UseGGPackResponse),
			typeof(UseGGPackRequest),
			typeof(FactionCompetitionNotification),
			typeof(TrustBoostUsedNotification),
			typeof(PlayerFactionContributionChangeNotification),
			typeof(FactionLoginRewardNotification),
			typeof(UseOverconRequest),
			typeof(UseOverconResponse),
			typeof(SetDevTagRequest),
			typeof(SetDevTagResponse),
			typeof(UseOverconSyncNotification),
			typeof(UpdateRemoteCharacterRequest),
			typeof(UpdateRemoteCharacterResponse),
			typeof(SelectTitleRequest),
			typeof(SelectTitleResponse),
			typeof(SelectBannerRequest),
			typeof(SelectBannerResponse),
			typeof(SelectRibbonRequest),
			typeof(SelectRibbonResponse),
			typeof(LoadingScreenToggleRequest),
			typeof(LoadingScreenToggleResponse),
			typeof(UpdateUIStateRequest),
			typeof(UpdateUIStateResponse),
			typeof(UpdatePushToTalkKeyRequest),
			typeof(UpdatePushToTalkKeyResponse),
			typeof(GroupUpdateNotification),
			typeof(GroupChatRequest),
			typeof(GroupChatResponse),
			typeof(GroupInviteRequest),
			typeof(GroupInviteResponse),
			typeof(GroupJoinRequest),
			typeof(GroupJoinResponse),
			typeof(GroupPromoteRequest),
			typeof(GroupPromoteResponse),
			typeof(GroupConfirmationRequest),
			typeof(GroupConfirmationResponse),
			typeof(GroupSuggestionRequest),
			typeof(GroupSuggestionResponse),
			typeof(GroupLeaveRequest),
			typeof(GroupLeaveResponse),
			typeof(GroupKickRequest),
			typeof(GroupKickResponse),
			typeof(FriendUpdateRequest),
			typeof(FriendUpdateResponse),
			typeof(FriendStatusNotification),
			typeof(FriendListSyncNotification),
			typeof(FriendStatusSyncNotification),
			typeof(PlayerUpdateStatusRequest),
			typeof(PlayerUpdateStatusResponse),
			typeof(StoreOpenedMessage),
			typeof(PaymentMethodsRequest),
			typeof(PaymentMethodsResponse),
			typeof(PricesRequest),
			typeof(PricesResponse),
			typeof(SteamMtxConfirm),
			typeof(PurchaseLootMatrixPackRequest),
			typeof(PurchaseLootMatrixPackResponse),
			typeof(PurchaseGameRequest),
			typeof(PurchaseGameResponse),
			typeof(PurchaseGGPackRequest),
			typeof(PurchaseGGPackResponse),
			typeof(PurchaseCharacterRequest),
			typeof(PurchaseCharacterResponse),
			typeof(PurchaseCharacterForCashRequest),
			typeof(PurchaseCharacterForCashResponse),
			typeof(PurchaseSkinRequest),
			typeof(PurchaseSkinResponse),
			typeof(PurchaseTextureRequest),
			typeof(PurchaseTextureResponse),
			typeof(PurchaseTintRequest),
			typeof(PurchaseTintResponse),
			typeof(PurchaseTintForCashRequest),
			typeof(PurchaseTintForCashResponse),
			typeof(PurchaseStoreItemForCashRequest),
			typeof(PurchaseStoreItemForCashResponse),
			typeof(PurchaseTauntRequest),
			typeof(PurchaseTauntResponse),
			typeof(PurchaseTitleRequest),
			typeof(PurchaseTitleResponse),
			typeof(PurchaseChatEmojiRequest),
			typeof(PurchaseChatEmojiResponse),
			typeof(PurchaseBannerBackgroundRequest),
			typeof(PurchaseBannerBackgroundResponse),
			typeof(PurchaseBannerForegroundRequest),
			typeof(PurchaseBannerForegroundResponse),
			typeof(PurchaseAbilityVfxRequest),
			typeof(PurchaseAbilityVfxResponse),
			typeof(PurchaseOverconRequest),
			typeof(PurchaseOverconResponse),
			typeof(PurchaseLoadingScreenBackgroundRequest),
			typeof(PurchaseLoadingScreenBackgroundResponse),
			typeof(PlayerPanelUpdatedNotification),
			typeof(PurchaseInventoryItemRequest),
			typeof(PurchaseInventoryItemResponse),
			typeof(PendingPurchaseResult),
			typeof(QuestOfferNotification),
			typeof(CheckAccountStatusRequest),
			typeof(CheckAccountStatusResponse),
			typeof(PickDailyQuestRequest),
			typeof(PickDailyQuestResponse),
			typeof(AbandonDailyQuestRequest),
			typeof(AbandonDailyQuestResponse),
			typeof(QuestCompleteNotification),
			typeof(CheckRAFStatusRequest),
			typeof(CheckRAFStatusResponse),
			typeof(SendRAFReferralEmailsRequest),
			typeof(SendRAFReferralEmailsResponse),
			typeof(MatchResultsNotification),
			typeof(ActivateQuestTriggerRequest),
			typeof(ActivateQuestTriggerResponse),
			typeof(BeginQuestRequest),
			typeof(BeginQuestResponse),
			typeof(CompleteQuestRequest),
			typeof(CompleteQuestResponse),
			typeof(MarkTutorialSkippedRequest),
			typeof(MarkTutorialSkippedResponse),
			typeof(GetInventoryItemsRequest),
			typeof(GetInventoryItemsResponse),
			typeof(AddInventoryItemsRequest),
			typeof(AddInventoryItemsResponse),
			typeof(RemoveInventoryItemsRequest),
			typeof(RemoveInventoryItemsResponse),
			typeof(ConsumeInventoryItemRequest),
			typeof(ConsumeInventoryItemResponse),
			typeof(ConsumeInventoryItemsRequest),
			typeof(ConsumeInventoryItemsResponse),
			typeof(SeasonQuestActionRequest),
			typeof(SeasonQuestActionResponse),
			typeof(RankedBanRequest),
			typeof(RankedBanResponse),
			typeof(RankedSelectionRequest),
			typeof(RankedSelectionResponse),
			typeof(RankedTradeRequest),
			typeof(RankedTradeResponse),
			typeof(RankedHoverClickRequest),
			typeof(RankedHoverClickResponse),
			typeof(GameDestroyedByPlayerNotification),
			typeof(EnterFreelancerResolutionPhaseNotification),
			typeof(FreelancerUnavailableNotification),
			typeof(MatchmakingQueueToPlayersNotification),
			typeof(ServerQueueConfigurationUpdateNotification),
			typeof(FlushRankedDataNotification),
			typeof(RankedLeaderboardOverviewRequest),
			typeof(RankedLeaderboardOverviewResponse),
			typeof(RankedLeaderboardSpecificRequest),
			typeof(RankedLeaderboardSpecificResponse),
			typeof(RankedOverviewChangeNotification),
			typeof(OverrideSessionLanguageCodeNotification),
			typeof(CustomKeyBindNotification),
			typeof(OptionsNotification),
			typeof(DEBUG_AdminSlashCommandNotification),
			typeof(DEBUG_CharacterXPChangeRequest),
			typeof(DEBUG_CharacterXPChangeResponse),
			typeof(DEBUG_PlayerXPChangeRequest),
			typeof(DEBUG_PlayerXPChangeResponse),
			typeof(DEBUG_AccountCurrencyChangeRequest),
			typeof(DEBUG_ResetDailyQuestTimersRequest),
			typeof(DEBUG_AddFactionRequest),
			typeof(DEBUG_AddFactionResponse),
			typeof(DEBUG_UnlockAllModsForCharacterRequest),
			typeof(DEBUG_UnlockAllModsForCharacterResponse),
			typeof(DEBUG_UnlockAllSkinsForCharacterRequest),
			typeof(DEBUG_UnlockAllSkinsForCharacterResponse),
			typeof(DEBUG_SetEloRequest),
			typeof(DEBUG_SetEloResponse),
			typeof(DEBUG_SetTierRequest),
			typeof(DEBUG_SetTierResponse),
			typeof(DEBUG_ForceMatchmakingRequest),
			typeof(DEBUG_ForceMatchmakingResponse),
			typeof(DEBUG_TakeSnapshotRequest),
			typeof(DEBUG_TakeSnapshotResponse),
			typeof(DEBUG_UnlockAllRequest),
			typeof(DEBUG_UnlockAllResponse),
			typeof(DEBUG_RelockAllRequest),
			typeof(DEBUG_RelockAllResponse),
			typeof(DEBUG_ResetCompletedChaptersRequest),
			typeof(DEBUG_ResetCompletedChaptersResponse),
			typeof(DEBUG_ClearPenaltyRequest),
			typeof(DEBUG_ClearPenaltyResponse),
			typeof(DEBUG_QueryAccessRequest),
			typeof(DEBUG_QueryAccessResponse),
			typeof(SetGameSubTypeRequest),
			typeof(SetGameSubTypeResponse),
			typeof(SynchronizeWithClientOutOfGameRequest),
			typeof(SynchronizeWithClientOutOfGameResponse),
			typeof(CalculateFreelancerStatsRequest),
			typeof(CalculateFreelancerStatsResponse),
			typeof(LobbyAlertMissionDataNotification),
			typeof(LobbySeasonQuestDataNotification)
		};

		public static WebSocketMessageFactory Get()
		{
			if (s_instance == null)
			{
				s_instance = new WebSocketMessageFactory();
				s_instance.AddMessageTypes(MessageTypes);
				s_instance.AddMessageTypes(QueueRequirement.MessageTypes);
			}
			return s_instance;
		}
	}
}
