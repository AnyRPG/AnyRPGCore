using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AnyRPG {

    /// <summary>
    /// meant to be inherited from by actual network implementations like fish-net, etc.
    /// </summary>
    public interface INetworkConnector {
        
        // client functions
        public virtual bool Login(string username, string password, string server) {
            return false;
        }
        public void RequestLogout();
        public void RequestSpawnPlayerUnit(string sceneName);
        public void RequestRespawnPlayerUnit();
        public void RequestDespawnPlayerUnit();
        public void RequestRevivePlayerUnit();
        public GameObject RequestSpawnModelPrefab(GameObject prefab, Transform parentTransform, Vector3 position, Vector3 forward);
        public void RequestReturnFromCutscene();
        public bool CanSpawnCharacterOverNetwork();
        public bool OwnPlayer(UnitController unitController);
        public void RequestCreatePlayerCharacter(CharacterSaveData saveData);
        public void DeletePlayerCharacter(int playerCharacterId);
        public void LoadCharacterList();
        public void RequestCreateLobbyGame(string sceneResourceName, bool allowLateJoin);
        public void CancelLobbyGame(int gameId);
        public void JoinLobbyGame(int gameId);
        public void LeaveLobbyGame(int gameId);
        public int GetClientId();
        public void SendLobbyChatMessage(string messageText);
        public void SendLobbyGameChatMessage(string messageText, int gameId);
        public void SendSceneChatMessage(string chatMessage);
        public void RequestLobbyGameList();
        public void RequestLobbyPlayerList();
        public void ChooseLobbyGameCharacter(string unitProfileName, int gameId, string appearanceString, List<SwappableMeshSaveData> swappableMeshSaveData);
        public void RequestStartLobbyGame(int gameId);
        public void RequestJoinLobbyGameInProgress(int gameId);
        public void ToggleLobbyGameReadyStatus(int gameId);
        public void InteractWithOption(UnitController sourceUnitController, Interactable targetInteractable, int componentIndex, int choiceIndex);
        public void RequestSetPlayerCharacterClass(Interactable interactable, int componentIndex);
        public void SetPlayerCharacterSpecialization(Interactable interactable, int componentIndex);
        public void RequestSetPlayerFaction(Interactable interactable, int componentIndex);
        public void RequestLearnSkill(Interactable interactable, int componentIndex, int skillId);
        public void RequestAcceptQuest(Interactable interactable, int componentIndex, Quest quest);
        public void RequestCompleteQuest(Interactable interactable, int componentIndex, Quest quest, QuestRewardChoices questRewardChoices);
        public void SellVendorItem(Interactable interactable, int componentIndex, long itemInstanceId);
        public void BuyItemFromVendor(Interactable interactable, int componentIndex, int collectionIndex, int itemIndex, string resourceName);
        public void RequestSpawnUnit(Interactable interactable, int componentIndex, int unitLevel, int extraLevels, bool useDynamicLevel, string unitProfileName, string unitToughnessName);
        public void RequestTurnInDialog(Interactable interactable, int componentIndex, Dialog dialog);
        public void RequestTurnInQuestDialog(Dialog dialog);
        public void TakeAllLoot();
        public void RequestTakeLoot(int lootDropId);
        public void RequestBeginCrafting(Recipe recipe, int craftAmount);
        public void RequestCancelCrafting();
        public void RequestUpdatePlayerAppearance(Interactable interactable, int componentIndex, string unitProfileName, string appearanceString, List<SwappableMeshSaveData> swappableMeshSaveData);
        public void RequestChangePlayerName(Interactable interactable, int componentIndex, string newName);
        public void RequestSpawnPet(UnitProfile unitProfile);
        public void RequestDespawnPet(UnitProfile unitProfile);
        public void RequestSceneWeather();
        public void RequestLoadPlayerCharacter(int playerCharacterId);
        public void AcceptCharacterGroupInvite(int inviteGroupId);
        public void DeclineCharacterGroupInvite();
        public void RequestLeaveCharacterGroup();
        public void RequestRemoveCharacterFromGroup(int playerCharacterId);
        public void RequestInviteCharacterToGroup(int playerCharacterId);
        public void RequestDisbandCharacterGroup(int characterGroupId);
        public void RequestPromoteCharacterToLeader(int characterId);
        public void RequestBeginTrade(int characterId);
        public void RequestDeclineTrade();


        // server functions
        public void StartServer(ushort port);
        public void StopServer();
        public void KickPlayer(int accountId);
        public string GetClientIPAddress(int accountId);
        public void AdvertiseCreateLobbyGame(LobbyGame lobbyGame);
        public void AdvertiseCancelLobbyGame(int gameId);
        public void AdvertiseAccountJoinLobbyGame(int gameId, int accountId, string userName);
        public void AdvertiseAccountLeaveLobbyGame(int gameId, int accountId);
        public void AdvertiseSendLobbyChatMessage(string messageText);
        public void AdvertiseSendLobbyGameChatMessage(string messageText, int gameId);
        public void AdvertiseSendSceneChatMessage(string messageText, int accountId);
        public void AdvertiseLobbyLogin(int accountId, string userName);
        public void AdvertiseLobbyLogout(int accountId);
        public void SetLobbyGameList(int accountId, List<LobbyGame> lobbyGames);
        public void SetLobbyPlayerList(int accountId, Dictionary<int, string> lobbyPlayers);
        public void AdvertiseChooseLobbyGameCharacter(int gameId, int accountId, string unitProfileName);
        public void StartLobbyGame(int gameId);
        public void AdvertiseJoinLobbyGameInProgress(int gameId, int accountId, string sceneResourceName);
        public void AdvertiseSetLobbyGameReadyStatus(int gameId, int accountId, bool ready);
        public int GetServerPort();
        public void AdvertiseLoadScene(string sceneResourceName, int accountId);
        public void ReturnObjectToPool(GameObject returnedObject);
        public UnitController SpawnCharacterPrefab(CharacterRequestData characterRequestData, Transform parentTransform, Vector3 position, Vector3 forward, Scene scene);
        public GameObject SpawnModelPrefabServer(GameObject prefab, Transform parentTransform, Vector3 position, Vector3 forward);
        public void AdvertiseMessageFeedMessage(int accountId, string message);
        public void AdvertiseSystemMessage(int accountId, string message);
        public void AdvertiseAddToBuyBackCollection(UnitController sourceUnitController, int accountId, Interactable interactable, int componentIndex, InstantiatedItem newInstantiatedItem);
        public void AdvertiseSellItemToPlayer(UnitController sourceUnitController, Interactable interactable, int componentIndex, int collectionIndex, int itemIndex, string resourceName, int quantity);
        public void AddAvailableDroppedLoot(int accountId, List<LootDrop> items);
        public void AddLootDrop(int accountId, int lootDropId, long itemInstanceId);
        public void AdvertiseTakeLoot(int accountId, int lootDropId);
        public void SpawnPlayer(int accountId, CharacterRequestData characterRequestData, Vector3 position, Vector3 forward, string sceneName);
        public Scene GetAccountScene(int accountId, string sceneName);
        public void AdvertiseAddSpawnRequest(int accountId, SpawnPlayerRequest loadSceneRequest);
        public void AdvertiseEndWeather(int sceneHandle, WeatherProfile profile, bool immediate);
        public void AdvertiseChooseWeather(int sceneHandle, WeatherProfile profile);
        public void AdvertiseStartWeather(int sceneHandle);
        public void AdvertiseLoadCutscene(Cutscene cutscene, int accountId);
        public void AdvertiseLoadPlayerCharacter(int accountId, string sceneName);
        public void AdvertiseAddCharacterToGroup(int playerCharacterId, CharacterGroup characterGroup);
        public void AdvertiseCharacterGroup(int accountId, CharacterGroup characterGroup);
        public void AdvertiseRemoveCharacterFromGroup(int characterId, CharacterGroup characterGroup);
        public void AdvertiseCharacterGroupInvite(int invitedAccountId, CharacterGroup characterGroup, string leaderName);
        public void AdvertiseDisbandCharacterGroup(CharacterGroup characterGroup);
        public void AdvertisePlayerNameNotAvailable(int accountId);
        public void AdvertiseLoadCharacterList(int accountId, List<PlayerCharacterSaveData> playerCharacterSaveDataList);
        public void AdvertiseDeletePlayerCharacter(int accountId);
        public void AdvertiseDeclineCharacterGroupInvite(int leaderAccountId, string decliningPlayerName);
        public void AdvertisePromoteGroupLeader(CharacterGroup characterGroup, int newLeaderCharacterId);
        public void AdvertiseRenameCharacterInGroup(CharacterGroup characterGroup, int characterId, string newName);
        public void AdvertiseGroupMessage(CharacterGroup characterGroup, string messageText);
        public void AdvertisePrivateMessage(int targetAccountId, string messageText);
        public void AdvertiseAcceptTradeInvite(int sourceAccountId, int targetCharacterId);
        public void AdvertiseDeclineTradeInvite(int sourceAccountId);
        public void AdvertiseRequestBeginTrade(int targetAccountId, int sourceCharacterId);
    }

}