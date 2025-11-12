using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AnyRPG {

    /// <summary>
    /// meant to be inherited from by actual network implementations like fish-net, etc.
    /// </summary>
    public abstract class NetworkController : ConfiguredMonoBehaviour {
        
        // client functions
        public virtual bool Login(string username, string password, string server) {
            return false;
        }
        public abstract void RequestLogout();
        public abstract void Disconnect();
        public abstract void RequestSpawnPlayerUnit(string sceneName);
        public abstract void RequestRespawnPlayerUnit();
        public abstract void RequestDespawnPlayerUnit();
        public abstract void RequestRevivePlayerUnit();
        public abstract GameObject RequestSpawnModelPrefab(GameObject prefab, Transform parentTransform, Vector3 position, Vector3 forward);
        public abstract void RequestReturnFromCutscene();
        public abstract bool CanSpawnCharacterOverNetwork();
        public abstract bool OwnPlayer(UnitController unitController);
        public abstract void RequestCreatePlayerCharacter(AnyRPGSaveData saveData);
        public abstract void DeletePlayerCharacter(int playerCharacterId);
        public abstract void LoadCharacterList();
        public abstract void RequestCreateLobbyGame(string sceneResourceName, bool allowLateJoin);
        public abstract void CancelLobbyGame(int gameId);
        public abstract void JoinLobbyGame(int gameId);
        public abstract void LeaveLobbyGame(int gameId);
        public abstract int GetClientId();
        public abstract void SendLobbyChatMessage(string messageText);
        public abstract void SendLobbyGameChatMessage(string messageText, int gameId);
        public abstract void SendSceneChatMessage(string chatMessage);
        public abstract void RequestLobbyGameList();
        public abstract void RequestLobbyPlayerList();
        public abstract void ChooseLobbyGameCharacter(string unitProfileName, int gameId, string appearanceString, List<SwappableMeshSaveData> swappableMeshSaveData);
        public abstract void RequestStartLobbyGame(int gameId);
        public abstract void RequestJoinLobbyGameInProgress(int gameId);
        public abstract void ToggleLobbyGameReadyStatus(int gameId);
        public abstract void InteractWithOption(UnitController sourceUnitController, Interactable targetInteractable, int componentIndex, int choiceIndex);
        public abstract void RequestSetPlayerCharacterClass(Interactable interactable, int componentIndex);
        public abstract void SetPlayerCharacterSpecialization(Interactable interactable, int componentIndex);
        public abstract void RequestSetPlayerFaction(Interactable interactable, int componentIndex);
        public abstract void RequestLearnSkill(Interactable interactable, int componentIndex, int skillId);
        public abstract void RequestAcceptQuest(Interactable interactable, int componentIndex, Quest quest);
        public abstract void RequestCompleteQuest(Interactable interactable, int componentIndex, Quest quest, QuestRewardChoices questRewardChoices);
        public abstract void SellVendorItem(Interactable interactable, int componentIndex, int itemInstanceId);
        public abstract void BuyItemFromVendor(Interactable interactable, int componentIndex, int collectionIndex, int itemIndex, string resourceName);
        public abstract void RequestSpawnUnit(Interactable interactable, int componentIndex, int unitLevel, int extraLevels, bool useDynamicLevel, string unitProfileName, string unitToughnessName);
        public abstract void RequestTurnInDialog(Interactable interactable, int componentIndex, Dialog dialog);
        public abstract void RequestTurnInQuestDialog(Dialog dialog);
        public abstract void TakeAllLoot();
        public abstract void RequestTakeLoot(int lootDropId);
        public abstract void RequestBeginCrafting(Recipe recipe, int craftAmount);
        public abstract void RequestCancelCrafting();
        public abstract void RequestUpdatePlayerAppearance(Interactable interactable, int componentIndex, string unitProfileName, string appearanceString, List<SwappableMeshSaveData> swappableMeshSaveData);
        public abstract void RequestChangePlayerName(Interactable interactable, int componentIndex, string newName);
        public abstract void RequestSpawnPet(UnitProfile unitProfile);
        public abstract void RequestDespawnPet(UnitProfile unitProfile);
        public abstract void RequestSceneWeather();
        public abstract void RequestLoadPlayerCharacter(int playerCharacterId);
        public abstract void AcceptCharacterGroupInvite(int inviteGroupId);
        public abstract void DeclineCharacterGroupInvite();
        public abstract void RequestLeaveCharacterGroup();
        public abstract void RequestRemoveCharacterFromGroup(int playerCharacterId);
        public abstract void RequestInviteCharacterToGroup(int playerCharacterId);
        public abstract void RequestDisbandCharacterGroup(int characterGroupId);


        // server functions
        public abstract void StartServer(ushort port);
        public abstract void StopServer();
        public abstract void KickPlayer(int accountId);
        public abstract string GetClientIPAddress(int accountId);
        public abstract void AdvertiseCreateLobbyGame(LobbyGame lobbyGame);
        public abstract void AdvertiseCancelLobbyGame(int gameId);
        public abstract void AdvertiseAccountJoinLobbyGame(int gameId, int accountId, string userName);
        public abstract void AdvertiseAccountLeaveLobbyGame(int gameId, int accountId);
        public abstract void AdvertiseSendLobbyChatMessage(string messageText);
        public abstract void AdvertiseSendLobbyGameChatMessage(string messageText, int gameId);
        public abstract void AdvertiseSendSceneChatMessage(string messageText, int accountId);
        public abstract void AdvertiseLobbyLogin(int accountId, string userName);
        public abstract void AdvertiseLobbyLogout(int accountId);
        public abstract void SetLobbyGameList(int accountId, List<LobbyGame> lobbyGames);
        public abstract void SetLobbyPlayerList(int accountId, Dictionary<int, string> lobbyPlayers);
        public abstract void AdvertiseChooseLobbyGameCharacter(int gameId, int accountId, string unitProfileName);
        public abstract void StartLobbyGame(int gameId);
        public abstract void AdvertiseJoinLobbyGameInProgress(int gameId, int accountId, string sceneResourceName);
        public abstract void AdvertiseSetLobbyGameReadyStatus(int gameId, int accountId, bool ready);
        public abstract int GetServerPort();
        public abstract void AdvertiseLoadScene(string sceneResourceName, int accountId);
        public abstract void ReturnObjectToPool(GameObject returnedObject);
        //public abstract void AdvertiseAddSpawnRequest(int accountId, SpawnPlayerRequest loadSceneRequest);
        public abstract UnitController SpawnCharacterPrefab(CharacterRequestData characterRequestData, Transform parentTransform, Vector3 position, Vector3 forward, Scene scene);
        public abstract GameObject SpawnModelPrefabServer(GameObject prefab, Transform parentTransform, Vector3 position, Vector3 forward);
        public abstract void AdvertiseMessageFeedMessage(int accountId, string message);
        public abstract void AdvertiseSystemMessage(int accountId, string message);
        public abstract void AdvertiseAddToBuyBackCollection(UnitController sourceUnitController, int accountId, Interactable interactable, int componentIndex, InstantiatedItem newInstantiatedItem);
        public abstract void AdvertiseSellItemToPlayer(UnitController sourceUnitController, Interactable interactable, int componentIndex, int collectionIndex, int itemIndex, string resourceName, int quantity);
        public abstract void AddAvailableDroppedLoot(int accountId, List<LootDrop> items);
        public abstract void AddLootDrop(int accountId, int lootDropId, int itemId);
        public abstract void AdvertiseTakeLoot(int accountId, int lootDropId);
        public abstract void SpawnPlayer(int accountId, CharacterRequestData characterRequestData, Vector3 position, Vector3 forward, string sceneName);
        public abstract Scene GetAccountScene(int accountId, string sceneName);
        public abstract void AdvertiseAddSpawnRequest(int accountId, SpawnPlayerRequest loadSceneRequest);
        public abstract void AdvertiseEndWeather(int sceneHandle, WeatherProfile profile, bool immediate);
        public abstract void AdvertiseChooseWeather(int sceneHandle, WeatherProfile profile);
        public abstract void AdvertiseStartWeather(int sceneHandle);
        public abstract void AdvertiseLoadCutscene(Cutscene cutscene, int accountId);
        public abstract void AdvertiseLoadPlayerCharacter(int accountId, string sceneName);
        public abstract void AdvertiseAddCharacterToGroup(int playerCharacterId, CharacterGroup characterGroup);
        public abstract void AdvertiseCharacterGroup(int accountId, CharacterGroup characterGroup);
        public abstract void AdvertiseRemoveCharacterFromGroup(int characterId, CharacterGroup characterGroup);
        public abstract void AdvertiseCharacterGroupInvite(int invitedAccountId, CharacterGroup characterGroup, string leaderName);
        public abstract void AdvertiseDisbandCharacterGroup(CharacterGroup characterGroup);
        public abstract void AdvertisePlayerNameNotAvailable(int accountId);
        public abstract void AdvertiseLoadCharacterList(int accountId, List<PlayerCharacterSaveData> playerCharacterSaveDataList);
        public abstract void AdvertiseDeletePlayerCharacter(int accountId);
        public abstract void AdvertiseDeclineCharacterGroupInvite(int leaderAccountId, string decliningPlayerName);
    }

}