using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    public class PlayerManagerServer : ConfiguredClass, ICharacterRequestor {


        /// <summary>
        /// accountId, playerCharacterMonitor
        /// </summary>
        private Dictionary<int, PlayerCharacterMonitor> playerCharacterMonitors = new Dictionary<int, PlayerCharacterMonitor>();

        /// <summary>
        /// playerCharacterId, accountId
        /// </summary>
        private Dictionary<int, int> playerCharacterAccountIdLookup = new Dictionary<int, int>();

        /// <summary>
        /// accountId, UnitController
        /// </summary>
        private Dictionary<int, UnitController> activeUnitControllersByAccountId = new Dictionary<int, UnitController>();

        /// <summary>
        /// playerCharacterid, UnitController
        /// </summary>
        private Dictionary<int, UnitController> activeUnitControllersByPlayerCharacterId = new Dictionary<int, UnitController>();

        /// <summary>
        /// gameobject, accountId
        /// </summary>
        private Dictionary<GameObject, int> activePlayerGameObjects = new Dictionary<GameObject, int>();

        /// <summary>
        /// unitController, accountId
        /// </summary>
        private Dictionary<UnitController, int> activeUnitControllerLookup = new Dictionary<UnitController, int>();

        /// <summary>
        /// accountId, LoadSceneRequest pairs for spawn requests
        /// </summary>
        private Dictionary<int, SpawnPlayerRequest> spawnRequests = new Dictionary<int, SpawnPlayerRequest>();

        private string defaultSpawnLocationTag = "DefaultSpawnLocation";

        protected bool eventSubscriptionsInitialized = false;

        private Coroutine monitorPlayerCharactersCoroutine = null;

        // game manager references
        protected SaveManager saveManager = null;
        protected LevelManager levelManager = null;
        protected InteractionManager interactionManager = null;
        protected PlayerManager playerManager = null;
        protected SystemAchievementManager systemAchievementManager = null;
        protected QuestGiverManagerClient questGiverManager = null;
        protected MessageFeedManager messageFeedManager = null;
        protected CharacterManager characterManager = null;
        protected SystemEventManager systemEventManager = null;
        protected CharacterGroupServiceServer characterGroupServiceServer = null;
        protected TradeServiceServer tradeServiceServer = null;
        protected GuildServiceServer guildServiceServer = null;

        /// <summary>
        /// accountId, PlayerCharacterMonitor
        /// </summary>
        public Dictionary<int, PlayerCharacterMonitor> PlayerCharacterMonitors { get => playerCharacterMonitors; }
        
        /// <summary>
        /// accountId, UnitController
        /// </summary>
        public Dictionary<int, UnitController> ActiveUnitControllers { get => activeUnitControllersByAccountId; }

        /// <summary>
        /// unitController, accountId
        /// </summary>
        public Dictionary<UnitController, int> ActiveUnitControllerLookup { get => activeUnitControllerLookup; }

        /// <summary>
        /// gameObject, accountId
        /// </summary>
        public Dictionary<GameObject, int> ActivePlayerGameObjects { get => activePlayerGameObjects; set => activePlayerGameObjects = value; }

        /// <summary>
        /// accountId, LoadSceneRequest
        /// </summary>
        public Dictionary<int, SpawnPlayerRequest> SpawnRequests { get => spawnRequests; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            CreateEventSubscriptions();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            saveManager = systemGameManager.SaveManager;
            levelManager = systemGameManager.LevelManager;
            interactionManager = systemGameManager.InteractionManager;
            playerManager = systemGameManager.PlayerManager;
            systemAchievementManager = systemGameManager.SystemAchievementManager;
            questGiverManager = systemGameManager.QuestGiverManagerClient;
            messageFeedManager = systemGameManager.UIManager.MessageFeedManager;
            characterManager = systemGameManager.CharacterManager;
            systemEventManager = systemGameManager.SystemEventManager;
            characterGroupServiceServer = systemGameManager.CharacterGroupServiceServer;
            tradeServiceServer = systemGameManager.TradeServiceServer;
            guildServiceServer = systemGameManager.GuildServiceServer;
        }


        private void CreateEventSubscriptions() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            networkManagerServer.OnStartServer += HandleStartServer;
            networkManagerServer.OnStopServer += HandleStopServer;
            eventSubscriptionsInitialized = true;
        }

        private void HandleStartServer() {
            if (monitorPlayerCharactersCoroutine == null) {
                monitorPlayerCharactersCoroutine = systemGameManager.StartCoroutine(MonitorPlayerCharacters());
            }
        }

        private void HandleStopServer() {
            if (monitorPlayerCharactersCoroutine != null) {
                systemGameManager.StopCoroutine(monitorPlayerCharactersCoroutine);
            }
        }

        private void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            /*
            systemEventManager.OnLevelUnload -= HandleLevelUnload;
            systemEventManager.OnLevelLoad -= HandleLevelLoad;
            systemEventManager.OnLevelChanged -= PlayLevelUpEffects;
            SystemEventManager.StopListening("OnPlayerDeath", HandlePlayerDeath);
            */
            eventSubscriptionsInitialized = false;
        }

        public void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            CleanupEventSubscriptions();
        }

        public int GetPlayerCharacterId(int accountId) {
            //Debug.Log($"PlayerManagerServer.GetPlayerCharacterId(accountId: {accountId})");

            if (playerCharacterMonitors.ContainsKey(accountId)) {
                return playerCharacterMonitors[accountId].characterSaveData.CharacterId;
            }
            return -1;
        }


        private void SavePlayerCharacter(PlayerCharacterMonitor playerCharacterMonitor) {
            //Debug.Log($"PlayerManagerServer.SavePlayerCharacter()");

            playerCharacterService.SavePlayerCharacter(playerCharacterMonitor);
        }

        public void AddActivePlayer(int accountId, UnitController unitController) {
            //Debug.Log($"PlayerManagerServer.AddActivePlayer(accountId: {accountId}, {unitController.gameObject.name})");

            int playerCharacterId = GetPlayerCharacterId(accountId);
            activeUnitControllersByPlayerCharacterId.Add(playerCharacterId, unitController);
            activeUnitControllersByAccountId.Add(accountId, unitController);
            activeUnitControllerLookup.Add(unitController, accountId);
            activePlayerGameObjects.Add(unitController.gameObject, accountId);

        }

        public void MonitorPlayer(UnitController unitController) {
            //Debug.Log($"PlayerManagerServer.MonitorPlayer({unitController.gameObject.name})");

            if (activeUnitControllerLookup.ContainsKey(unitController) == false) {
                return;
            }
            SubscribeToPlayerEvents(unitController);

            if (levelManager.SceneDictionary.ContainsKey(unitController.gameObject.scene.name) == false) {
                return;
            }
            // commented out for now because it happens too early and the save data is overwritten later
            // this is now handled in the unitcontroller at the correct time
            /*
            SceneNode sceneNode = levelManager.SceneDictionary[unitController.gameObject.scene.name];
            if (sceneNode != null) {
                sceneNode.Visit(unitController);
            }
            */
        }

        public void RemoveActivePlayer(int accountId) {
            //Debug.Log($"PlayerManagerServer.RemoveActivePlayer({accountId})");

            if (ActiveUnitControllers.ContainsKey(accountId) == false) {
                return;
            }
            UnsubscribeFromPlayerEvents(activeUnitControllersByAccountId[accountId]);
            activePlayerGameObjects.Remove(activeUnitControllersByAccountId[accountId].gameObject);
            activeUnitControllerLookup.Remove(activeUnitControllersByAccountId[accountId]);
            activeUnitControllersByAccountId.Remove(accountId);
            int playerCharacterId = GetPlayerCharacterId(accountId);
            activeUnitControllersByPlayerCharacterId.Remove(playerCharacterId);
        }

        public void SubscribeToPlayerEvents(UnitController unitController) {
            //Debug.Log($"PlayerManagerServer.SubscribeToPlayerEvents({unitController.gameObject.name})");

            unitController.UnitEventController.OnKillEvent += HandleKillEvent;
            unitController.UnitEventController.OnEnterInteractableTrigger += HandleEnterInteractableTrigger;
            unitController.UnitEventController.OnExitInteractableTrigger += HandleExitInteractableTrigger;
        }

        public void UnsubscribeFromPlayerEvents(UnitController unitController) {
            //Debug.Log($"PlayerManagerServer.UnsubscribeFromPlayerEvents({unitController.gameObject.name})");

            unitController.UnitEventController.OnKillEvent -= HandleKillEvent;
            unitController.UnitEventController.OnEnterInteractableTrigger -= HandleEnterInteractableTrigger;
            unitController.UnitEventController.OnExitInteractableTrigger -= HandleExitInteractableTrigger;
        }

        private void HandleEnterInteractableTrigger(UnitController unitController, Interactable interactable) {
            //Debug.Log($"PlayerManagerServer.HandleEnterInteractableTrigger({unitController.gameObject.name})");

            if (networkManagerServer.ServerModeActive || systemGameManager.GameMode == GameMode.Local) {
                interactionManager.InteractWithTrigger(unitController, interactable);
            }
        }

        private void HandleExitInteractableTrigger(UnitController unitController, Interactable interactable) {
            //Debug.Log($"PlayerManagerServer.HandleEnterInteractableTrigger({unitController.gameObject.name})");

            if (networkManagerServer.ServerModeActive || systemGameManager.GameMode == GameMode.Local) {
                interactionManager.InteractWithTrigger(unitController, interactable);
            }
        }

        public void HandleKillEvent(UnitController unitController, UnitController killedUnitController, float creditPercent) {
            if (creditPercent == 0) {
                return;
            }
            //Debug.Log($"{gameObject.name}: About to gain xp from kill with creditPercent: " + creditPercent);
            GainXP(unitController, (int)(LevelEquations.GetXPAmountForKill(unitController.CharacterStats.Level, killedUnitController, systemConfigurationManager) * creditPercent));
        }

        public void GainXP(int amount, int accountId) {
            if (activeUnitControllersByAccountId.ContainsKey(accountId) == true) {
                GainXP(activeUnitControllersByAccountId[accountId], amount);
            }
        }

        public void GainXP(UnitController unitController, int amount) {
            unitController.CharacterStats.GainXP(amount);
        }

        public void AddCurrency(Currency currency, int amount, int accountId) {
            if (activeUnitControllersByAccountId.ContainsKey(accountId) == false) {
                return;
            }
            activeUnitControllersByAccountId[accountId].CharacterCurrencyManager.AddCurrency(currency, amount);

        }

        public void AddItem(string itemName, int accountId) {
            if (activeUnitControllersByAccountId.ContainsKey(accountId) == false) {
                return;
            }

            InstantiatedItem tmpItem = activeUnitControllersByAccountId[accountId].CharacterInventoryManager.GetNewInstantiatedItem(itemName);
            if (tmpItem != null) {
                activeUnitControllersByAccountId[accountId].CharacterInventoryManager.AddItem(tmpItem, false);
            }
        }

        public void BeginAction(AnimatedAction animatedAction, int accountId) {
            if (activeUnitControllersByAccountId.ContainsKey(accountId) == false) {
                return;
            }
            activeUnitControllersByAccountId[accountId].UnitActionManager.BeginAction(animatedAction);

        }

        public void LearnAbility(string abilityName, int accountId) {
            if (activeUnitControllersByAccountId.ContainsKey(accountId) == false) {
                return;
            }
            Ability tmpAbility = systemDataFactory.GetResource<Ability>(abilityName);
            if (tmpAbility != null) {
                activeUnitControllersByAccountId[accountId].CharacterAbilityManager.LearnAbility(tmpAbility.AbilityProperties);
            }

        }

        public void SetLevel(int newLevel, int accountId) {
            if (activeUnitControllersByAccountId.ContainsKey(accountId) == false) {
                return;
            }
            CharacterStats characterStats = activeUnitControllersByAccountId[accountId].CharacterStats;
            newLevel = Mathf.Clamp(newLevel, characterStats.Level, systemConfigurationManager.MaxLevel);
            if (newLevel > characterStats.Level) {
                while (characterStats.Level < newLevel) {
                    characterStats.GainLevel();
                }
            }
        }

        public void LoadScene(string sceneName, UnitController sourceUnitController) {
            //Debug.Log($"PlayerManagerServer.LoadScene({sceneName}, {sourceUnitController.gameObject.name})");

            if (activeUnitControllerLookup.ContainsKey(sourceUnitController) == false) {
                return;
            }
            LoadScene(sceneName, sourceUnitController, activeUnitControllerLookup[sourceUnitController]);
        }

        public void LoadScene(string sceneName, int accountId) {
            //Debug.Log($"PlayerManagerServer.LoadScene({sceneName}, {accountId})");

            if (activeUnitControllersByAccountId.ContainsKey(accountId) == false) {
                return;
            }
            LoadScene(sceneName, activeUnitControllersByAccountId[accountId], accountId);
        }

        public void LoadScene(string sceneName, UnitController sourceUnitController, int accountId) {
            //Debug.Log($"PlayerManagerServer.LoadScene({sceneName}, {accountId})");
            
            if (systemGameManager.GameMode == GameMode.Local) {
                levelManager.LoadLevel(sceneName);
            } else if (networkManagerServer.ServerModeActive) {
                networkManagerServer.AdvertiseLoadScene(sourceUnitController, sceneName, accountId);
            }
        }

        public void Teleport(UnitController unitController, TeleportEffectProperties teleportEffectProperties) {
            systemGameManager.StartCoroutine(TeleportDelay(unitController, teleportEffectProperties));
        }


        // delay the teleport by one frame so abilities can finish up without the source character becoming null
        public IEnumerator TeleportDelay (UnitController unitController, TeleportEffectProperties teleportEffectProperties) {
            yield return null;
            if (unitController != null) {
                TeleportInternal(unitController, teleportEffectProperties);
            }
        }

        private void TeleportInternal(UnitController unitController, TeleportEffectProperties teleportEffectProperties) {
            //Debug.Log($"PlayerManagerServer.TeleportInternal({unitController.gameObject.name}, {teleportEffectProperties.levelName})");

            if (activeUnitControllerLookup.ContainsKey(unitController) == false) {
                return;
            }
            int accountId = activeUnitControllerLookup[unitController];

            SpawnPlayerRequest loadSceneRequest = new SpawnPlayerRequest();
            if (teleportEffectProperties.overrideSpawnDirection == true) {
                loadSceneRequest.overrideSpawnDirection = true;
                loadSceneRequest.spawnForwardDirection = teleportEffectProperties.spawnForwardDirection;
            }
            if (teleportEffectProperties.overrideSpawnLocation == true) {
                loadSceneRequest.overrideSpawnLocation = true;
                loadSceneRequest.spawnLocation = teleportEffectProperties.spawnLocation;
            }
            if (teleportEffectProperties.locationTag != null && teleportEffectProperties.locationTag != string.Empty) {
                loadSceneRequest.locationTag = teleportEffectProperties.locationTag;
            }
            AddSpawnRequest(accountId, loadSceneRequest);

            // if the scene is already loaded, then just respawn the player
            if (unitController.gameObject.scene.name == teleportEffectProperties.levelName) {
                //Debug.Log($"PlayerManagerServer.TeleportInternal({unitController.gameObject.name}, {teleportEffectProperties.levelName}) - already in scene, respawning");

                RespawnPlayerUnit(accountId);
                return;
            }

            if (networkManagerServer.ServerModeActive == true) {
                networkManagerServer.AdvertiseTeleport(accountId, unitController, teleportEffectProperties);
                return;
            }

            // local mode active, continue with teleport
            if (teleportEffectProperties.levelName != null) {
                DespawnPlayerUnit(accountId);
                levelManager.LoadLevel(teleportEffectProperties.levelName);
            }
        }

        public void DespawnPlayerUnit(int accountId) {
            //Debug.Log($"PlayerManagerServer.DespawnPlayerUnit({accountId})");

            if (activeUnitControllersByAccountId.ContainsKey(accountId) == false) {
                return;
            }
            playerCharacterMonitors[accountId].ProcessBeforeDespawn();

            activeUnitControllersByAccountId[accountId].Despawn(0, false, true);
            RemoveActivePlayer(accountId);
        }

        public void AddSpawnRequest(UnitController unitController, SpawnPlayerRequest loadSceneRequest) {
            if (activeUnitControllerLookup.ContainsKey(unitController)) {
                //if (networkManagerServer.ServerModeActive == true) {
                //    networkManagerServer.AdvertiseAddSpawnRequest(activePlayerLookup[unitController], loadSceneRequest);
                //} else {
                    AddSpawnRequest(activeUnitControllerLookup[unitController], loadSceneRequest);
                //}
            }
        }

        public void SetPlayerCharacterClass(CharacterClass characterClass, int accountId) {
            if (activeUnitControllersByAccountId.ContainsKey(accountId) == false) {
                return;
            }
            activeUnitControllersByAccountId[accountId].BaseCharacter.ChangeCharacterClass(characterClass);
        }

        public void SetPlayerCharacterSpecialization(ClassSpecialization classSpecialization, int accountId) {
            if (activeUnitControllersByAccountId.ContainsKey(accountId) == false) {
                return;
            }
            activeUnitControllersByAccountId[accountId].BaseCharacter.ChangeClassSpecialization(classSpecialization);
        }

        public void SetPlayerFaction(Faction faction, int accountId) {
            if (activeUnitControllersByAccountId.ContainsKey(accountId) == false) {
                return;
            }
            activeUnitControllersByAccountId[accountId].BaseCharacter.ChangeCharacterFaction(faction);
        }


        public void LearnSkill(Skill skill, int accountId) {
            if (activeUnitControllersByAccountId.ContainsKey(accountId) == false) {
                return;
            }
            activeUnitControllersByAccountId[accountId].CharacterSkillManager.LearnSkill(skill);
        }

        public void AddSpawnRequest(int accountId, SpawnPlayerRequest loadSceneRequest) {
            //Debug.Log($"PlayerManagerServer.AddSpawnRequest({accountId})");

            AddSpawnRequest(accountId, loadSceneRequest, true);
        }

        public void AddSpawnRequest(int accountId, SpawnPlayerRequest loadSceneRequest, bool advertise) {
            //Debug.Log($"PlayerManagerServer.AddSpawnRequest({accountId}, {advertise})");

            if (spawnRequests.ContainsKey(accountId)) {
                //Debug.Log($"PlayerManagerServer.AddSpawnRequest({accountId}, {advertise}) replacing request spawn location: {loadSceneRequest.spawnLocation}, forward direction: {loadSceneRequest.spawnForwardDirection}");
                spawnRequests[accountId] = loadSceneRequest;
            } else {
                //Debug.Log($"PlayerManagerServer.AddSpawnRequest({accountId}, {advertise}) adding new request spawn location: {loadSceneRequest.spawnLocation}, forward direction: {loadSceneRequest.spawnForwardDirection}");
                spawnRequests.Add(accountId, loadSceneRequest);
            }
            if (networkManagerServer.ServerModeActive == true && advertise == true) {
                networkManagerServer.AdvertiseAddSpawnRequest(accountId, loadSceneRequest);
            } else {
                //Debug.Log($"PlayerManagerServer.AddSpawnRequest({accountId}) not in server mode, not advertising");
            }
        }

        public void RemoveSpawnRequest(int accountId) {
            //Debug.Log($"PlayerManagerServer.RemoveSpawnRequest({accountId})");

            spawnRequests.Remove(accountId);
        }
        public SpawnPlayerRequest GetSpawnPlayerRequest(int accountId, string sceneName) {
            return GetSpawnPlayerRequest(accountId, sceneName, false);
        }

        public SpawnPlayerRequest GetSpawnPlayerRequest(int accountId, string sceneName, bool keepRequest) {
            //Debug.Log($"PlayerManagerServer.GetSpawnPlayerRequest({accountId}, {sceneName})");

            SpawnPlayerRequest inputLoadSceneRequest = null;
            SpawnPlayerRequest outputLoadSceneRequest = new SpawnPlayerRequest();

            if (spawnRequests.ContainsKey(accountId)) {
                //Debug.Log($"PlayerManagerServer.GetSpawnPlayerRequest({accountId}) found request");
                inputLoadSceneRequest = spawnRequests[accountId];
            } else {
                //Debug.Log($"PlayerManagerServer.GetSpawnPlayerRequest({accountId}) making new request");
                inputLoadSceneRequest = new SpawnPlayerRequest();
            }
            outputLoadSceneRequest.overrideSpawnLocation = inputLoadSceneRequest.overrideSpawnLocation;
            outputLoadSceneRequest.overrideSpawnDirection = inputLoadSceneRequest.overrideSpawnDirection;
            outputLoadSceneRequest.xOffset = inputLoadSceneRequest.xOffset;
            outputLoadSceneRequest.zOffset = inputLoadSceneRequest.zOffset;

            Scene accountScene;
            if (networkManagerServer.ServerModeActive == true ) {
                accountScene = networkManagerServer.GetAccountScene(accountId, sceneName);
            } else {
                accountScene = SceneManager.GetSceneByName(sceneName);
            }
            if (inputLoadSceneRequest.overrideSpawnLocation == true) {
                //Debug.Log("Levelmanager.GetSpawnLocation(). SpawnLocationOverride is set.  returning " + spawnLocationOverride);
                outputLoadSceneRequest.spawnLocation = inputLoadSceneRequest.spawnLocation;
            } else {
                GameObject spawnLocationMarker = null;
                if (inputLoadSceneRequest.locationTag != string.Empty) {
                    spawnLocationMarker = GetSceneObjectByTag(inputLoadSceneRequest.locationTag, accountScene);
                    if (spawnLocationMarker != null) {
                        outputLoadSceneRequest.spawnLocation = spawnLocationMarker.transform.position;
                        outputLoadSceneRequest.spawnForwardDirection = spawnLocationMarker.transform.forward;
                    }
                }
                if (spawnLocationMarker == null) {
                    spawnLocationMarker = GetSceneObjectByTag(defaultSpawnLocationTag, accountScene);
                    if (spawnLocationMarker != null) {
                        outputLoadSceneRequest.spawnLocation = spawnLocationMarker.transform.position;
                        outputLoadSceneRequest.spawnForwardDirection = spawnLocationMarker.transform.forward;
                    }
                }
            }

            if (inputLoadSceneRequest.overrideSpawnDirection == true) {
                outputLoadSceneRequest.spawnForwardDirection = inputLoadSceneRequest.spawnForwardDirection;
            }

            if (keepRequest == false) {
                RemoveSpawnRequest(accountId);
            }

            // debug print the spawn location
            //Debug.Log($"PlayerManagerServer.GetSpawnPlayerRequest({accountId}, {sceneName}) spawn location: {outputLoadSceneRequest.spawnLocation}, forward direction: {outputLoadSceneRequest.spawnForwardDirection}");

            return outputLoadSceneRequest;
        }

        public GameObject GetSceneObjectByTag(string locationTag, Scene scene) {
            //Debug.Log($"PlayerManagerServer.GetSpawnLocationMarker({locationTag})");

            List<GameObject> spawnLocationMarkers = new List<GameObject>();
            spawnLocationMarkers = GameObject.FindGameObjectsWithTag(locationTag).ToList();
            foreach (GameObject spawnLocationMarker in spawnLocationMarkers) {
                if (spawnLocationMarker.scene.handle == scene.handle) {
                    return spawnLocationMarker;
                }
            }
            return null;
        }

        public void RequestSpawnPlayerUnit(int accountId, string sceneName) {
            //Debug.Log($"PlayerManagerServer.RequestSpawnPlayerUnit(accountId: {accountId}, sceneName: {sceneName})");

            SpawnPlayerRequest spawnPlayerRequest = GetSpawnPlayerRequest(accountId, sceneName);

            if (systemGameManager.GameMode == GameMode.Local) {
                // load local player
                CharacterConfigurationRequest characterConfigurationRequest = new CharacterConfigurationRequest(systemDataFactory, playerCharacterMonitors[accountId].characterSaveData);
                characterConfigurationRequest.unitControllerMode = UnitControllerMode.Player;
                CharacterRequestData characterRequestData = new CharacterRequestData(playerManager,
                    systemGameManager.GameMode,
                    characterConfigurationRequest);
                characterRequestData.characterId = playerCharacterMonitors[accountId].characterSaveData.CharacterId;
                characterRequestData.isOwner = true;
                characterRequestData.saveData = playerCharacterMonitors[accountId].characterSaveData;
                UnitController unitController = characterManager.SpawnCharacterPrefab(characterRequestData, null, spawnPlayerRequest.spawnLocation, spawnPlayerRequest.spawnForwardDirection);
                playerCharacterMonitors[accountId].SetUnitController(unitController);
            } else {
                CharacterConfigurationRequest characterConfigurationRequest = new CharacterConfigurationRequest(systemDataFactory, playerCharacterMonitors[accountId].characterSaveData);
                characterConfigurationRequest.unitControllerMode = UnitControllerMode.Player;
                CharacterRequestData characterRequestData = new CharacterRequestData(this, GameMode.Network, characterConfigurationRequest);
                characterRequestData.characterId = playerCharacterMonitors[accountId].characterSaveData.CharacterId;
                characterRequestData.characterGroupId = characterGroupServiceServer.GetCharacterGroupIdFromCharacterId(characterRequestData.characterId);
                Guild guild = guildServiceServer.GetGuildFromCharacterId(characterRequestData.characterId);
                if (guild != null) {
                    //Debug.Log($"PlayerManagerServer.RequestSpawnPlayerUnit: found guild {guild.guildName} for characterId {characterRequestData.characterId}");
                    characterRequestData.characterGuildId = guild.GuildId;
                    characterRequestData.characterGuildName = guild.GuildName;
                }
                characterRequestData.saveData = playerCharacterMonitors[accountId].characterSaveData;

                if (spawnPlayerRequest.overrideSpawnLocation == false) {
                    // we were loading the default location, so randomize the spawn position a bit so players don't all spawn in the same place
                    spawnPlayerRequest.spawnLocation = new Vector3(spawnPlayerRequest.spawnLocation.x + spawnPlayerRequest.xOffset, spawnPlayerRequest.spawnLocation.y, spawnPlayerRequest.spawnLocation.z + spawnPlayerRequest.zOffset);
                }
                networkManagerServer.SpawnPlayer(accountId, characterRequestData, spawnPlayerRequest.spawnLocation, spawnPlayerRequest.spawnForwardDirection, sceneName);
            }
        }

        public IEnumerator MonitorPlayerCharacters() {
            //Debug.Log($"PlayerManagerServer.MonitorPlayerCharacters()");

            while (systemGameManager.GameMode == GameMode.Network) {
                foreach (PlayerCharacterMonitor playerCharacterMonitor in playerCharacterMonitors.Values) {
                    if (playerCharacterMonitor.unitController != null) {
                        SavePlayerCharacter(playerCharacterMonitor);
                    }
                }
                yield return new WaitForSeconds(10);
            }
        }

        public void AddPlayerMonitor(int accountId, CharacterSaveData characterSaveData) {
            //Debug.Log($"PlayerManagerServer.AddPlayerMonitor(accountId: {accountId}, characterId: {characterSaveData.CharacterId})");

            if (playerCharacterMonitors.ContainsKey(accountId)) {
                return;
            } else {
                PlayerCharacterMonitor playerCharacterMonitor = new PlayerCharacterMonitor(
                    systemGameManager,
                    accountId,
                    characterSaveData,
                    null
                );
                playerCharacterMonitors.Add(accountId, playerCharacterMonitor);
                playerCharacterAccountIdLookup.Add(characterSaveData.CharacterId, accountId);

                // this should not be needed.  Check for breakage before deleting.
                //AddSpawnRequest(accountId, new SpawnPlayerRequest());
            }
        }

        public void MonitorPlayerUnit(int accountId, UnitController unitController) {
            //Debug.Log($"PlayerManagerServer.MonitorPlayerUnit({accountId}, {unitController.gameObject.name})");

            if (playerCharacterMonitors.ContainsKey(accountId) == false) {
                return;
            }
            playerCharacterMonitors[accountId].SetUnitController(unitController);
            AddActivePlayer(accountId, unitController);
        }

        /*
        public void StopMonitoringPlayerUnit(UnitController unitController) {
            //Debug.Log($"PlayerManagerServer.StopMonitoringPlayerUnit({unitController.gameObject.name})");

            if (activePlayerLookup.ContainsKey(unitController)) {
                StopMonitoringPlayerUnit(activePlayerLookup[unitController]);
            }
        }
        */

        public void StopMonitoringPlayerUnit(int accountId) {
            //Debug.Log($"PlayerManagerServer.StopMonitoringPlayerUnit(accountId: {accountId})");

            if (playerCharacterMonitors.ContainsKey(accountId)) {
                PauseMonitoringPlayerUnit(accountId);

                //activePlayerCharactersByAccount.Remove(activePlayerCharacters[playerCharacterId].accountId);
                playerCharacterAccountIdLookup.Remove(playerCharacterMonitors[accountId].characterSaveData.CharacterId);
                playerCharacterMonitors.Remove(accountId);
            }
        }

        public void ProcessDisconnect(int accountId) {
            //Debug.Log($"PlayerManagerServer.ProcessDisconnect({accountId})");

            if (playerCharacterMonitors.ContainsKey(accountId) && playerCharacterMonitors[accountId].unitController != null) {
                playerCharacterMonitors[accountId].SetDisconnected();
            }
            PauseMonitoringPlayerUnit(accountId);
        }

        public void PauseMonitoringPlayerUnit(int accountId) {
            //Debug.Log($"PlayerManagerServer.PauseMonitoringPlayerUnit({accountId})");

            // give the trade service a chance to cancel any trades
            tradeServiceServer.RequestCancelTrade(accountId);

            //if (playerCharacterMonitors.ContainsKey(accountId) && playerCharacterMonitors[accountId].unitController != null) {
                RemoveActivePlayer(accountId);

                // flush data to database before stop monitoring
                if (systemGameManager.GameMode == GameMode.Network && playerCharacterMonitors.ContainsKey(accountId) == true) {
                    SavePlayerCharacter(playerCharacterMonitors[accountId]);
                }
            //}
        }

        public void RevivePlayerUnit(int accountId) {

            // get lobby game id, unitprofile, and scene name from the player character save data
            if (activeUnitControllersByAccountId.ContainsKey(accountId) == false) {
                //Debug.LogError($"PlayerManagerServer.RequestRespawnPlayerUnit: activePlayerCharacters does not contain accountId {accountId}");
                return;
            }
            activeUnitControllersByAccountId[accountId].CharacterStats.StatusEffectRevive();
        }

        public void RespawnPlayerUnit(int accountId) {
            
            // get lobby game id, unitprofile, and scene name from the player character save data
            if (playerCharacterMonitors.ContainsKey(accountId) == false) {
                //Debug.LogError($"PlayerManagerServer.RequestRespawnPlayerUnit: activePlayerCharacters does not contain accountId {accountId}");
                return;
            }
            string sceneName = playerCharacterMonitors[accountId].unitController.gameObject.scene.name;

            DespawnPlayerUnit(accountId);
            playerCharacterMonitors[accountId].characterSaveData.IsDead = false;
            playerCharacterMonitors[accountId].characterSaveData.InitializeResourceAmounts = true;
            RequestSpawnPlayerUnit(accountId, sceneName);
        }

        public void UpdatePlayerAppearance(int accountId, string unitProfileName, string appearanceString, List<SwappableMeshSaveData> swappableMeshSaveData) {
            //Debug.Log($"PlayerManagerServer.UpdatePlayerAppearance({accountId}, {unitProfileName},swappableMeshSaveData.Count: {swappableMeshSaveData?.Count})");

            // Always despawn units if their appearance changes.
            SpawnPlayerRequest loadSceneRequest = new SpawnPlayerRequest() {
                overrideSpawnDirection = true,
                spawnForwardDirection = playerCharacterMonitors[accountId].unitController.transform.forward,
                overrideSpawnLocation = true,
                spawnLocation = playerCharacterMonitors[accountId].unitController.transform.position
            };
            string sceneName = playerCharacterMonitors[accountId].unitController.gameObject.scene.name;
            AddSpawnRequest(accountId, loadSceneRequest);
            DespawnPlayerUnit(accountId);
            playerCharacterMonitors[accountId].characterSaveData.AppearanceString = appearanceString;
            playerCharacterMonitors[accountId].characterSaveData.SwappableMeshSaveData = swappableMeshSaveData;
            playerCharacterMonitors[accountId].characterSaveData.UnitProfileName = unitProfileName;
            RequestSpawnPlayerUnit(accountId, sceneName);
        }

        public void ConfigureSpawnedCharacter(UnitController unitController) {
        }

        public void PostInit(UnitController unitController) {
            //Debug.Log($"PlayerManagerServer.PostInit({unitController.gameObject.name}, account: {unitController.CharacterRequestData.accountId})");

            /*
            // load player data from the active player characters dictionary
            if (!playerCharacterMonitors.ContainsKey(unitController.CharacterRequestData.accountId)) {
                //Debug.LogError($"PlayerManagerServer.PostInit: activePlayerCharacters does not contain accountId {characterRequestData.accountId}");
                return;
            }
            */
        }

        public void RequestSpawnPet(int accountId, UnitProfile unitProfile) {
            //Debug.Log($"PlayerManagerServer.RequestSpawnPet({accountId}, {unitProfile.ResourceName})");

            if (!playerCharacterMonitors.ContainsKey(accountId)) {
                //Debug.LogError($"NetworkManagerServer.PostInit: activePlayerCharacters does not contain accountId {characterRequestData.accountId}");
                return;
            }
            SpawnPet(playerCharacterMonitors[accountId].unitController, unitProfile);
        }

        public void SpawnPet(UnitController unitController, UnitProfile unitProfile) {
            //Debug.Log($"PlayerManagerServer.SpawnPet({unitController.gameObject.name}, {unitProfile.ResourceName})");

            unitController.CharacterPetManager.SpawnPet(unitProfile);
        }

        public void RequestDespawnPet(int accountId, UnitProfile unitProfile) {

            if (!playerCharacterMonitors.ContainsKey(accountId)) {
                //Debug.LogError($"NetworkManagerServer.PostInit: activePlayerCharacters does not contain accountId {characterRequestData.accountId}");
                return;
            }
            DespawnPet(playerCharacterMonitors[accountId].unitController, unitProfile);
        }

        public void DespawnPet(UnitController unitController, UnitProfile unitProfile) {
            unitController.CharacterPetManager.DespawnPet(unitProfile);
        }

        public void RequestSpawnRequest(int accountId) {
            //Debug.Log($"PlayerManagerServer.RequestSpawnRequest({accountId})");

            if (spawnRequests.ContainsKey(accountId)) {
                networkManagerServer.AdvertiseAddSpawnRequest(accountId, spawnRequests[accountId]);
            }
        }

        public void LoadCutsceneWithDelay(Cutscene cutscene, UnitController sourceUnitController) {
            //Debug.Log($"PlayerManagerServer.LoadCutscene({cutscene.ResourceName}, {sourceUnitController?.gameObject.name})");

            systemGameManager.StartCoroutine(LoadCutsceneWithDelayCoroutine(cutscene, sourceUnitController));
        }

        private IEnumerator LoadCutsceneWithDelayCoroutine(Cutscene cutscene, UnitController sourceUnitController) {
            yield return null;
            LoadCutscene(cutscene, sourceUnitController);
        }

        private void LoadCutscene(Cutscene cutscene, UnitController sourceUnitController) {
            //Debug.Log($"PlayerManagerServer.LoadCutscene({cutscene.ResourceName}, {sourceUnitController?.gameObject.name})");

            if (activeUnitControllerLookup.ContainsKey(sourceUnitController)) {
                int accountId = activeUnitControllerLookup[sourceUnitController];
                SpawnPlayerRequest spawnPlayerRequest = new SpawnPlayerRequest() {
                    overrideSpawnDirection = true,
                    spawnForwardDirection = sourceUnitController.transform.forward,
                    overrideSpawnLocation = true,
                    spawnLocation = sourceUnitController.transform.position
                };
                // debug print the spawn location
                //Debug.Log($"PlayerManagerServer.LoadCutscene({cutscene.ResourceName}, {accountId}) spawn location: {spawnPlayerRequest.spawnLocation}, forward direction: {spawnPlayerRequest.spawnForwardDirection}");
                DespawnPlayerUnit(accountId);
                AddSpawnRequest(accountId, spawnPlayerRequest, true);
                if (networkManagerServer.ServerModeActive) {
                    networkManagerServer.AdvertiseLoadCutscene(cutscene, accountId);
                } else {
                    // local mode active, continue with cutscene
                    levelManager.LoadCutSceneWithDelay(cutscene);
                }
            }
        }

        /// <summary>
        /// return zero if not found
        /// </summary>
        /// <param name="playerCharacterId"></param>
        /// <returns></returns>
        public int GetAccountIdFromPlayerCharacterId(int playerCharacterId) {
            if (playerCharacterAccountIdLookup.ContainsKey(playerCharacterId)) {
                return playerCharacterAccountIdLookup[playerCharacterId];
            }
            return -1;
        }

        public int GetAccountIdFromUnitController(UnitController unitController) {
            if (activeUnitControllerLookup.ContainsKey(unitController)) {
                return activeUnitControllerLookup[unitController];
            }
            return -1;
        }

        public string GetPlayerName(int leaderAccountId) {
            if (activeUnitControllersByAccountId.ContainsKey(leaderAccountId)) {
                return activeUnitControllersByAccountId[leaderAccountId].BaseCharacter.CharacterName;
            }
            return string.Empty;

        }

        public UnitController GetUnitControllerFromAccountId(int sourceAccountId) {
            if (activeUnitControllersByAccountId.ContainsKey(sourceAccountId)) {
                return activeUnitControllersByAccountId[sourceAccountId];
            }
            return null;
        }

        public UnitController GetUnitControllerFromPlayerCharacterId(int playerCharacterId) {
            //Debug.Log($"PlayerManagerServer.GetUnitControllerFromPlayerCharacterId({playerCharacterId})");

            if (activeUnitControllersByPlayerCharacterId.ContainsKey(playerCharacterId)) {
                return activeUnitControllersByPlayerCharacterId[playerCharacterId];
            }
            return null;
        }
    }

}