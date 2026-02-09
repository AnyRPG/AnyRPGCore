using FishNet.Component.Transforming;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AnyRPG {
    public class FishNetUnitController : FishNetInteractable {

        public event System.Action OnCompleteCharacterRequest = delegate { };

        public readonly SyncVar<string> unitProfileName = new SyncVar<string>();

        public readonly SyncVar<UnitControllerMode> unitControllerMode = new SyncVar<UnitControllerMode>();
        public readonly SyncVar<int> characterId = new SyncVar<int>();

        private UnitProfile unitProfile = null;
        private UnitController unitController = null;
        private NetworkObject networkObject = null;

        public UnitController UnitController { get => unitController; }

        protected override void Awake() {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.Awake() position: { gameObject.transform.position}");
            base.Awake();

            unitControllerMode.Value = UnitControllerMode.Preview;
        }

        protected override void Configure() {
            base.Configure();
            unitController = GetComponent<UnitController>();
            networkObject = GetComponent<NetworkObject>();
        }


        public override void OnStartClient() {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.OnStartClient()");

            base.OnStartClient();

            Configure();
            if (systemGameManager == null) {
                return;
            }
            if (unitControllerMode.Value == UnitControllerMode.Player
                || unitControllerMode.Value == UnitControllerMode.Pet
                || unitControllerMode.Value == UnitControllerMode.AI) {
                BeginCharacterRequest();
                //GetClientSaveData();
            } else {
                BeginCharacterRequest();
                CompleteClientCharacterRequest(null, -1, -1, string.Empty);
            }
        }

        public override void OnStopClient() {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.OnStopClient()");

            base.OnStopClient();

            if (SystemGameManager.IsShuttingDown == true) {
                return;
            }

            UnsubscribeFromClientUnitEvents();
            systemGameManager.NetworkManagerClient.ProcessStopNetworkUnitClient(unitController);
        }

        public override void OnStartServer() {
            base.OnStartServer();
            //Debug.Log($"{gameObject.name}.FishNetUnitController.OnStartServer()");

            Configure();
            if (systemGameManager == null) {
                return;
            }
            BeginCharacterRequest();
            CompleteCharacterRequest(false, null, -1, -1, string.Empty);
            SubscribeToServerUnitEvents();
        }

        public override void OnStopServer() {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.OnStopServer() {GetInstanceID()}");

            base.OnStopServer();
            if (SystemGameManager.IsShuttingDown == true) {
                return;
            }
            UnsubscribeFromServerUnitEvents();
            if (unitController.CharacterConfigured == true) {
                //Debug.Log($"{gameObject.name}.FishNetUnitController.OnStopServer() setting IsDisconnected = true");
                unitController.IsDisconnected = true;
            }
            systemGameManager.NetworkManagerServer.ProcessStopNetworkUnitServer(unitController);
        }

        public override void OnSpawnServer(NetworkConnection connection) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.OnSpawnServer()");

            base.OnSpawnServer(connection);

            HandleSpawnServerClient(connection, new PlayerCharacterSaveData(unitController.CharacterSaveManager.SaveData, systemItemManager), unitController.CharacterGroupManager.GroupId, unitController.CharacterGuildManager.GuildId, unitController.CharacterGuildManager.GuildName);
        }

        [TargetRpc]
        private void HandleSpawnServerClient(NetworkConnection networkConnection, PlayerCharacterSaveData playerCharacterSaveData, int characterGroupId, int guildId, string guildName) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleSpawnServerClient() owner: {base.OwnerId}");

            if (unitControllerMode.Value == UnitControllerMode.Player
                || unitControllerMode.Value == UnitControllerMode.Pet
                || unitControllerMode.Value == UnitControllerMode.AI) {
                CompleteClientCharacterRequest(playerCharacterSaveData, characterGroupId, guildId, guildName);
            }
        }

        public void CompleteClientCharacterRequest(PlayerCharacterSaveData playerCharacterSaveData, int characterGroupId, int guildId, string guildName) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.CompleteClientCharacterRequest()");

            CompleteCharacterRequest(base.IsOwner, playerCharacterSaveData, characterGroupId, guildId, guildName);
            SubscribeToClientUnitEvents();
        }

        public void SubscribeToClientUnitEvents() {
            if (unitController == null) {
                // something went wrong
                return;
            }

            if (base.IsOwner) {
                unitController.UnitEventController.OnBeginAction += HandleBeginAction;
                unitController.UnitEventController.OnBeginAbility += HandleBeginAbilityLocal;
                unitController.UnitEventController.OnSetTarget += HandleSetTargetClient;
                unitController.UnitEventController.OnClearTarget += HandleClearTargetClient;
                unitController.UnitEventController.OnRequestEquipToSlot += HandleRequestEquipToSlot;
                //unitController.UnitEventController.OnRequestUnequipFromList += HandleRequestUnequipFromList;
                unitController.UnitEventController.OnRequestDropItemFromInventorySlot += HandleRequestDropItemFromInventorySlot;
                unitController.UnitEventController.OnRequestMoveFromBankToInventory += HandleRequestMoveFromBankToInventory;
                unitController.UnitEventController.OnRequestMoveFromInventoryToBank += HandleRequestMoveFromInventoryToBank;
                unitController.UnitEventController.OnRequestUseItem += HandleRequestUseItem;
                unitController.UnitEventController.OnRequestSwapInventoryEquipment += HandleRequestSwapInventoryEquipment;
                unitController.UnitEventController.OnRequestUnequipToSlot += HandleRequestUnequipToSlot;
                unitController.UnitEventController.OnRequestSwapBags += HandleRequestSwapBags;
                unitController.UnitEventController.OnRequestUnequipBagToSlot += HandleRequestUnequipBagToSlot;
                unitController.UnitEventController.OnRequestUnequipBag += HandleRequestUnequipBag;
                unitController.UnitEventController.OnRequestMoveBag += HandleRequestMoveBag;
                unitController.UnitEventController.OnRequestAddBag += HandleRequestAddBagFromInventory;
                unitController.UnitEventController.OnSetGroundTarget += HandleSetGroundTarget;
                unitController.UnitEventController.OnRequestCancelStatusEffect += HandleRequestCancelStatusEffect;
                unitController.UnitEventController.OnRequestMoveGamepadUseable += HandleRequestMoveGamepadUseable;
                unitController.UnitEventController.OnRequestAssignGamepadUseable += HandleRequestAssignGamepadUseable;
                unitController.UnitEventController.OnRequestClearGamepadUseable += HandleRequestClearGamepadUseable;
                unitController.UnitEventController.OnRequestMoveMouseUseable += HandleRequestMoveMouseUseable;
                unitController.UnitEventController.OnRequestAssignMouseUseable += HandleRequestAssignMouseUseable;
                unitController.UnitEventController.OnRequestClearMouseUseable += HandleRequestClearMouseUseable;
                unitController.UnitEventController.OnSetParent += HandleSetParent;
                unitController.UnitEventController.OnDeactivateMountedState += HandleDeactivateMountedStateOwner;
                unitController.UnitEventController.OnRequestAcceptQuestItemQuest += HandleRequestAcceptQuestItemQuest;
                unitController.UnitEventController.OnRequestCompleteQuestItemQuest += HandleRequestCompleteQuestItemQuest;
                unitController.UnitEventController.OnRequestDeleteItem += HandleRequestDeleteItem;
            }
            // all clients
            unitController.UnitEventController.OnUnsetParent += HandleUnsetParent;
            //unitController.UnitEventController.OnDespawn += HandleDespawnClient;
        }

        public void UnsubscribeFromClientUnitEvents() {
            if (unitController == null) {
                return;
            }
            if (base.IsOwner) {
                unitController.UnitEventController.OnBeginAction -= HandleBeginAction;
                unitController.UnitEventController.OnBeginAbility -= HandleBeginAbilityLocal;
                unitController.UnitEventController.OnSetTarget -= HandleSetTargetClient;
                unitController.UnitEventController.OnClearTarget -= HandleClearTargetClient;
                unitController.UnitEventController.OnRequestEquipToSlot -= HandleRequestEquipToSlot;
                //unitController.UnitEventController.OnRequestUnequipFromList -= HandleRequestUnequipFromList;
                unitController.UnitEventController.OnRequestDropItemFromInventorySlot -= HandleRequestDropItemFromInventorySlot;
                unitController.UnitEventController.OnRequestMoveFromBankToInventory -= HandleRequestMoveFromBankToInventory;
                unitController.UnitEventController.OnRequestMoveFromInventoryToBank -= HandleRequestMoveFromInventoryToBank;
                unitController.UnitEventController.OnRequestUseItem -= HandleRequestUseItem;
                unitController.UnitEventController.OnRequestSwapInventoryEquipment -= HandleRequestSwapInventoryEquipment;
                unitController.UnitEventController.OnRequestUnequipToSlot -= HandleRequestUnequipToSlot;
                unitController.UnitEventController.OnRequestSwapBags -= HandleRequestSwapBags;
                unitController.UnitEventController.OnRequestUnequipBagToSlot -= HandleRequestUnequipBagToSlot;
                unitController.UnitEventController.OnRequestUnequipBag -= HandleRequestUnequipBag;
                unitController.UnitEventController.OnRequestMoveBag -= HandleRequestMoveBag;
                unitController.UnitEventController.OnRequestAddBag -= HandleRequestAddBagFromInventory;
                unitController.UnitEventController.OnSetGroundTarget -= HandleSetGroundTarget;
                unitController.UnitEventController.OnRequestCancelStatusEffect -= HandleRequestCancelStatusEffect;
                unitController.UnitEventController.OnRequestMoveGamepadUseable -= HandleRequestMoveGamepadUseable;
                unitController.UnitEventController.OnRequestAssignGamepadUseable -= HandleRequestAssignGamepadUseable;
                unitController.UnitEventController.OnRequestClearGamepadUseable -= HandleRequestClearGamepadUseable;
                unitController.UnitEventController.OnRequestMoveMouseUseable -= HandleRequestMoveMouseUseable;
                unitController.UnitEventController.OnRequestAssignMouseUseable -= HandleRequestAssignMouseUseable;
                unitController.UnitEventController.OnRequestClearMouseUseable -= HandleRequestClearMouseUseable;
                unitController.UnitEventController.OnSetParent -= HandleSetParent;
                unitController.UnitEventController.OnDeactivateMountedState -= HandleDeactivateMountedStateOwner;
                unitController.UnitEventController.OnRequestAcceptQuestItemQuest -= HandleRequestAcceptQuestItemQuest;
                unitController.UnitEventController.OnRequestCompleteQuestItemQuest -= HandleRequestCompleteQuestItemQuest;
                unitController.UnitEventController.OnRequestDeleteItem -= HandleRequestDeleteItem;
            }
            // all clients
            unitController.UnitEventController.OnUnsetParent -= HandleUnsetParent;
            //unitController.UnitEventController.OnDespawn -= HandleDespawnClient;
        }

        public void SubscribeToServerUnitEvents() {
            if (unitController == null) {
                // something went wrong
                return;
            }

            unitController.UnitEventController.OnBeginChatMessage += HandleBeginChatMessageServer;
            unitController.UnitEventController.OnPerformAnimatedActionAnimation += HandlePerformAnimatedActionServer;
            unitController.UnitEventController.OnPerformAbilityCastAnimation += HandlePerformAbilityCastAnimationServer;
            unitController.UnitEventController.OnPerformAbilityActionAnimation += HandlePerformAbilityActionAnimationServer;
            unitController.UnitEventController.OnAnimatorClearAction += HandleClearActionClient;
            unitController.UnitEventController.OnAnimatorClearAbilityAction += HandleClearAnimatedAbilityClient;
            unitController.UnitEventController.OnAnimatorClearAbilityCast += HandleClearCastingClient;
            //unitController.UnitEventController.OnAnimatorDeath += HandleAnimatorDeathClient;
            unitController.UnitEventController.OnResourceAmountChanged += HandleResourceAmountChangedServer;
            unitController.UnitEventController.OnBeforeDie += HandleBeforeDieServer;
            unitController.UnitEventController.OnReviveBegin += HandleReviveBeginServer;
            unitController.UnitEventController.OnReviveComplete += HandleReviveCompleteServer;
            unitController.UnitEventController.OnEnterCombat += HandleEnterCombatServer;
            unitController.UnitEventController.OnDropCombat += HandleDropCombat;
            unitController.UnitEventController.OnSpawnAbilityObjects += HandleSpawnAbilityObjectsServer;
            unitController.UnitEventController.OnDespawnAbilityObjects += HandleDespawnAbilityObjects;
            unitController.UnitEventController.OnSpawnAbilityEffectPrefabs += HandleSpawnAbilityEffectPrefabsServer;
            unitController.UnitEventController.OnSpawnProjectileEffectPrefabs += HandleSpawnProjectileEffectPrefabsServer;
            unitController.UnitEventController.OnSpawnChanneledEffectPrefabs += HandleSpawnChanneledEffectPrefabsServer;
            unitController.UnitEventController.OnGainXP += HandleGainXPServer;
            unitController.UnitEventController.OnLevelChanged += HandleLevelChanged;
            unitController.UnitEventController.OnDespawn += HandleDespawn;
            //unitController.UnitEventController.OnEnterInteractableTrigger += HandleEnterInteractableTriggerServer;
            unitController.UnitEventController.OnClassChange += HandleClassChangeServer;
            unitController.UnitEventController.OnSpecializationChange += HandleSpecializationChangeServer;
            unitController.UnitEventController.OnFactionChange += HandleFactionChangeServer;
            unitController.UnitEventController.OnEnterInteractableRange += HandleEnterInteractableRangeServer;
            unitController.UnitEventController.OnExitInteractableRange += HandleExitInteractableRangeServer;
            unitController.UnitEventController.OnAcceptQuest += HandleAcceptQuestServer;
            unitController.UnitEventController.OnAcceptAchievement += HandleAcceptAchievementServer;
            unitController.UnitEventController.OnAbandonQuest += HandleAbandonQuestServer;
            unitController.UnitEventController.OnTurnInQuest += HandleTurnInQuestServer;
            unitController.UnitEventController.OnMarkQuestComplete += HandleMarkQuestCompleteServer;
            unitController.UnitEventController.OnMarkAchievementComplete += HandleMarkAchievementCompleteServer;
            //unitController.UnitEventController.OnRemoveQuest += HandleRemoveQuestServer;
            unitController.UnitEventController.OnLearnSkill += HandleLearnSkillServer;
            unitController.UnitEventController.OnUnLearnSkill += HandleUnLearnSkillServer;
            unitController.UnitEventController.OnSetQuestObjectiveCurrentAmount += HandleSetQuestObjectiveCurrentAmount;
            unitController.UnitEventController.OnSetAchievementObjectiveCurrentAmount += HandleSetAchievementObjectiveCurrentAmount;
            unitController.UnitEventController.OnQuestObjectiveStatusUpdated += HandleQuestObjectiveStatusUpdatedServer;
            unitController.UnitEventController.OnAchievementObjectiveStatusUpdated += HandleAchievementObjectiveStatusUpdatedServer;
            //unitController.UnitEventController.OnStartInteractWithOption += HandleStartInteractWithOption;
            unitController.UnitEventController.OnGetNewInstantiatedItem += HandleGetNewInstantiatedItem;
            //unitController.UnitEventController.OnDeleteItem += HandleDeleteItemServer;
            unitController.UnitEventController.OnAddEquipment += HandleAddEquipment;
            unitController.UnitEventController.OnRemoveEquipment += HandleRemoveEquipment;
            unitController.UnitEventController.OnAddItemToInventorySlot += HandleAddItemToInventorySlot;
            unitController.UnitEventController.OnRemoveItemFromInventorySlot += HandleRemoveItemFromInventorySlot;
            unitController.UnitEventController.OnAddItemToBankSlot += HandleAddItemToBankSlot;
            unitController.UnitEventController.OnRemoveItemFromBankSlot += HandleRemoveItemFromBankSlot;
            //unitController.UnitEventController.OnPlaceInEmpty += HandlePlaceInEmpty;
            unitController.UnitEventController.OnSetCraftAbility += HandleSetCraftAbilityServer;
            unitController.UnitEventController.OnCraftItem += HandleCraftItemServer;
            unitController.UnitEventController.OnRemoveFirstCraftingQueueItem += HandleRemoveFirstCraftingQueueItemServer;
            unitController.UnitEventController.OnClearCraftingQueue += HandleClearCraftingQueueServer;
            unitController.UnitEventController.OnAddToCraftingQueue += HandleAddToCraftingQueueServer;
            unitController.UnitEventController.OnCastTimeChanged += HandleCastTimeChanged;
            unitController.UnitEventController.OnCastComplete += HandleCastComplete;
            unitController.UnitEventController.OnCastCancel += HandleCastCancel;
            unitController.UnitEventController.OnRebuildModelAppearance += HandleRebuildModelAppearanceServer;
            unitController.UnitEventController.OnRemoveBag += HandleRemoveBagServer;
            unitController.UnitEventController.OnAddBag += HandleAddBagServer;
            unitController.UnitEventController.OnStatusEffectAdd += HandleStatusEffectAddServer;
            unitController.UnitEventController.OnAddStatusEffectStack += HandleAddStatusEffectStackServer;
            unitController.UnitEventController.OnCancelStatusEffect += HandleCancelStatusEffectServer;
            unitController.UnitEventController.OnCombatMessage += HandleCombatMessageServer;
            unitController.UnitEventController.OnReceiveCombatTextEvent += HandleReceiveCombatTextEventServer;
            unitController.UnitEventController.OnTakeDamage += HandleTakeDamageServer;
            unitController.UnitEventController.OnImmuneToEffect += HandleImmuneToEffectServer;
            unitController.UnitEventController.OnRecoverResource += HandleRecoverResourceServer;
            unitController.UnitEventController.OnCurrencyChange += HandleCurrencyChangeServer;
            unitController.UnitEventController.OnLearnRecipe += HandleLearnRecipe;
            unitController.UnitEventController.OnUnlearnRecipe += HandleUnlearnRecipe;
            unitController.UnitEventController.OnSetReputationAmount += HandleSetReputationAmountServer;
            unitController.UnitEventController.OnSetGamepadActionButton += HandleSetGamepadActionButton;
            unitController.UnitEventController.OnSetMouseActionButton += HandleSetMouseActionButton;
            unitController.UnitEventController.OnUnsetMouseActionButton += HandleUnsetMouseActionButton;
            unitController.UnitEventController.OnUnsetGamepadActionButton += HandleUnsetGamepadActionButton;
            unitController.UnitEventController.OnNameChange += HandleNameChangeServer;
            unitController.UnitEventController.OnAddPet += HandleAddPetServer;
            unitController.UnitEventController.OnAddActivePet += HandleAddActivePetServer;
            unitController.UnitEventController.OnBeginAbilityCoolDown += HandleBeginAbilityCoolDownServer;
            unitController.UnitEventController.OnBeginActionCoolDown += HandleBeginActionCoolDownServer;
            unitController.UnitEventController.OnInitiateGlobalCooldown += HandleInitiateGlobalCooldownServer;
            unitController.UnitEventController.OnActivateAutoAttack += HandleActivateAutoAttackServer;
            unitController.UnitEventController.OnDeactivateAutoAttack += HandleDeactivateAutoAttackServer;
            unitController.UnitEventController.OnSpawnActionObjects += HandleSpawnActionObjectsServer;
            unitController.UnitEventController.OnDespawnActionObjects += HandleDespawnActionObjectsServer;
            unitController.UnitEventController.OnSetMountedState += HandleSetMountedStateServer;
            unitController.UnitEventController.OnActivateMountedState += HandleActivateMountedStateServer;
            unitController.UnitEventController.OnDeactivateMountedState += HandleDeactivateMountedState;
            //unitController.UnitEventController.OnSetParent += HandleSetParent;
            unitController.UnitEventController.OnUnsetParent += HandleUnsetParent;
            //unitController.UnitEventController.OnMountUnitSpawn += HandleMountUnitSpawnServer;
            unitController.UnitEventController.OnDespawnMountUnit += HandleDespawnMountUnitServer;
            unitController.UnitEventController.OnWriteMessageFeedMessage += HandleWriteMessageFeedMessageServer;
            unitController.UnitEventController.OnDialogCompleted += HandleDialogCompletedServer;
            unitController.UnitEventController.OnInteractWithQuestStartItem += HandleInteractWithQuestStartItemServer;
            unitController.UnitEventController.OnNameChangeFail += HandleNameChangeFailServer;
            unitController.UnitEventController.OnSetGroupId += HandleSetGroupId;
            unitController.UnitEventController.OnSetGuildId += HandleSetGuildId;
        }


        public void UnsubscribeFromServerUnitEvents() {
            if (unitController == null) {
                return;
            }
            unitController.UnitEventController.OnBeginChatMessage -= HandleBeginChatMessageServer;
            unitController.UnitEventController.OnPerformAnimatedActionAnimation -= HandlePerformAnimatedActionServer;
            unitController.UnitEventController.OnPerformAbilityCastAnimation -= HandlePerformAbilityCastAnimationServer;
            unitController.UnitEventController.OnPerformAbilityActionAnimation -= HandlePerformAbilityActionAnimationServer;
            unitController.UnitEventController.OnAnimatorClearAction -= HandleClearActionClient;
            unitController.UnitEventController.OnAnimatorClearAbilityAction -= HandleClearAnimatedAbilityClient;
            unitController.UnitEventController.OnAnimatorClearAbilityCast -= HandleClearCastingClient;
            //unitController.UnitEventController.OnAnimatorDeath -= HandleAnimatorDeathClient;
            unitController.UnitEventController.OnResourceAmountChanged -= HandleResourceAmountChangedServer;
            unitController.UnitEventController.OnBeforeDie -= HandleBeforeDieServer;
            unitController.UnitEventController.OnReviveBegin -= HandleReviveBeginServer;
            unitController.UnitEventController.OnReviveComplete -= HandleReviveCompleteServer;
            unitController.UnitEventController.OnEnterCombat -= HandleEnterCombatServer;
            unitController.UnitEventController.OnDropCombat -= HandleDropCombat;
            unitController.UnitEventController.OnSpawnAbilityObjects -= HandleSpawnAbilityObjectsServer;
            unitController.UnitEventController.OnDespawnAbilityObjects -= HandleDespawnAbilityObjects;
            unitController.UnitEventController.OnSpawnAbilityEffectPrefabs -= HandleSpawnAbilityEffectPrefabsServer;
            unitController.UnitEventController.OnSpawnProjectileEffectPrefabs -= HandleSpawnProjectileEffectPrefabsServer;
            unitController.UnitEventController.OnSpawnChanneledEffectPrefabs -= HandleSpawnChanneledEffectPrefabsServer;
            unitController.UnitEventController.OnGainXP -= HandleGainXPServer;
            unitController.UnitEventController.OnLevelChanged -= HandleLevelChanged;
            unitController.UnitEventController.OnDespawn -= HandleDespawn;
            //unitController.UnitEventController.OnEnterInteractableTrigger -= HandleEnterInteractableTriggerServer;
            unitController.UnitEventController.OnClassChange -= HandleClassChangeServer;
            unitController.UnitEventController.OnSpecializationChange -= HandleSpecializationChangeServer;
            unitController.UnitEventController.OnFactionChange -= HandleFactionChangeServer;
            unitController.UnitEventController.OnEnterInteractableRange -= HandleEnterInteractableRangeServer;
            unitController.UnitEventController.OnExitInteractableRange -= HandleExitInteractableRangeServer;
            unitController.UnitEventController.OnAcceptQuest -= HandleAcceptQuestServer;
            unitController.UnitEventController.OnAbandonQuest -= HandleAbandonQuestServer;
            unitController.UnitEventController.OnTurnInQuest -= HandleTurnInQuestServer;
            unitController.UnitEventController.OnMarkQuestComplete -= HandleMarkQuestCompleteServer;
            //unitController.UnitEventController.OnRemoveQuest -= HandleRemoveQuestServer;
            unitController.UnitEventController.OnLearnSkill -= HandleLearnSkillServer;
            unitController.UnitEventController.OnUnLearnSkill -= HandleUnLearnSkillServer;
            unitController.UnitEventController.OnSetQuestObjectiveCurrentAmount -= HandleSetQuestObjectiveCurrentAmount;
            unitController.UnitEventController.OnSetAchievementObjectiveCurrentAmount -= HandleSetAchievementObjectiveCurrentAmount;
            unitController.UnitEventController.OnQuestObjectiveStatusUpdated -= HandleQuestObjectiveStatusUpdatedServer;
            //unitController.UnitEventController.OnStartInteractWithOption -= HandleStartInteractWithOptionServer;
            unitController.UnitEventController.OnGetNewInstantiatedItem -= HandleGetNewInstantiatedItem;
            //unitController.UnitEventController.OnDeleteItem -= HandleDeleteItemServer;
            unitController.UnitEventController.OnAddEquipment -= HandleAddEquipment;
            unitController.UnitEventController.OnRemoveEquipment -= HandleRemoveEquipment;
            unitController.UnitEventController.OnAddItemToInventorySlot -= HandleAddItemToInventorySlot;
            unitController.UnitEventController.OnRemoveItemFromInventorySlot -= HandleRemoveItemFromInventorySlot;
            unitController.UnitEventController.OnAddItemToBankSlot -= HandleAddItemToBankSlot;
            unitController.UnitEventController.OnRemoveItemFromBankSlot -= HandleRemoveItemFromBankSlot;
            unitController.UnitEventController.OnSetCraftAbility -= HandleSetCraftAbilityServer;
            unitController.UnitEventController.OnCraftItem -= HandleCraftItemServer;
            unitController.UnitEventController.OnRemoveFirstCraftingQueueItem -= HandleRemoveFirstCraftingQueueItemServer;
            unitController.UnitEventController.OnClearCraftingQueue -= HandleClearCraftingQueueServer;
            unitController.UnitEventController.OnAddToCraftingQueue -= HandleAddToCraftingQueueServer;
            unitController.UnitEventController.OnCastTimeChanged -= HandleCastTimeChanged;
            unitController.UnitEventController.OnCastComplete -= HandleCastComplete;
            unitController.UnitEventController.OnCastCancel -= HandleCastCancel;
            unitController.UnitEventController.OnRebuildModelAppearance -= HandleRebuildModelAppearanceServer;
            unitController.UnitEventController.OnRemoveBag -= HandleRemoveBagServer;
            unitController.UnitEventController.OnAddBag -= HandleAddBagServer;
            unitController.UnitEventController.OnStatusEffectAdd -= HandleStatusEffectAddServer;
            unitController.UnitEventController.OnAddStatusEffectStack -= HandleAddStatusEffectStackServer;
            unitController.UnitEventController.OnCancelStatusEffect -= HandleCancelStatusEffectServer;
            unitController.UnitEventController.OnCombatMessage -= HandleCombatMessageServer;
            unitController.UnitEventController.OnReceiveCombatTextEvent -= HandleReceiveCombatTextEventServer;
            unitController.UnitEventController.OnTakeDamage -= HandleTakeDamageServer;
            unitController.UnitEventController.OnImmuneToEffect -= HandleImmuneToEffectServer;
            unitController.UnitEventController.OnRecoverResource -= HandleRecoverResourceServer;
            unitController.UnitEventController.OnCurrencyChange -= HandleCurrencyChangeServer;
            unitController.UnitEventController.OnLearnRecipe -= HandleLearnRecipe;
            unitController.UnitEventController.OnUnlearnRecipe -= HandleUnlearnRecipe;
            unitController.UnitEventController.OnSetReputationAmount -= HandleSetReputationAmountServer;
            unitController.UnitEventController.OnSetGamepadActionButton -= HandleSetGamepadActionButton;
            unitController.UnitEventController.OnSetMouseActionButton -= HandleSetMouseActionButton;
            unitController.UnitEventController.OnUnsetMouseActionButton -= HandleUnsetMouseActionButton;
            unitController.UnitEventController.OnUnsetGamepadActionButton -= HandleUnsetGamepadActionButton;
            unitController.UnitEventController.OnNameChange -= HandleNameChangeServer;
            unitController.UnitEventController.OnAddPet -= HandleAddPetServer;
            unitController.UnitEventController.OnAddActivePet -= HandleAddActivePetServer;
            unitController.UnitEventController.OnBeginAbilityCoolDown -= HandleBeginAbilityCoolDownServer;
            unitController.UnitEventController.OnBeginActionCoolDown -= HandleBeginActionCoolDownServer;
            unitController.UnitEventController.OnInitiateGlobalCooldown -= HandleInitiateGlobalCooldownServer;
            unitController.UnitEventController.OnActivateAutoAttack -= HandleActivateAutoAttackServer;
            unitController.UnitEventController.OnDeactivateAutoAttack -= HandleDeactivateAutoAttackServer;
            unitController.UnitEventController.OnSpawnActionObjects -= HandleSpawnActionObjectsServer;
            unitController.UnitEventController.OnDespawnActionObjects -= HandleDespawnActionObjectsServer;
            unitController.UnitEventController.OnSetMountedState -= HandleSetMountedStateServer;
            unitController.UnitEventController.OnActivateMountedState -= HandleActivateMountedStateServer;
            unitController.UnitEventController.OnDeactivateMountedState -= HandleDeactivateMountedState;
            //unitController.UnitEventController.OnSetParent -= HandleSetParent;
            unitController.UnitEventController.OnUnsetParent -= HandleUnsetParent;
            //unitController.UnitEventController.OnMountUnitSpawn -= HandleMountUnitSpawnServer;
            unitController.UnitEventController.OnDespawnMountUnit -= HandleDespawnMountUnitServer;
            unitController.UnitEventController.OnWriteMessageFeedMessage -= HandleWriteMessageFeedMessageServer;
            unitController.UnitEventController.OnDialogCompleted -= HandleDialogCompletedServer;
            unitController.UnitEventController.OnInteractWithQuestStartItem -= HandleInteractWithQuestStartItemServer;
            unitController.UnitEventController.OnNameChangeFail -= HandleNameChangeFailServer;
            unitController.UnitEventController.OnSetGroupId -= HandleSetGroupId;
            unitController.UnitEventController.OnSetGuildId -= HandleSetGuildId;
        }



        [ObserversRpc]
        private void HandleSetGroupId(int newGroupId) {
            unitController.CharacterGroupManager.SetGroupId(newGroupId);
        }

        [ObserversRpc]
        private void HandleSetGuildId(int newGuildId, string guildName) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleSetGuildId({newGuildId},{guildName})");

            unitController.CharacterGuildManager.SetGuildId(newGuildId, guildName);
        }

        private void HandleNameChangeFailServer() {
            HandleNameChangeFailClient(base.Owner);
        }

        [TargetRpc]
        private void HandleNameChangeFailClient(NetworkConnection networkConnection) {
            unitController.UnitEventController.NotifyOnNameChangeFail();
        }

        private void HandleInteractWithQuestStartItemServer(Quest quest, int slotIndex, long itemInstanceId) {
            HandleInteractWithQuestStartItemClient(base.Owner, quest.ResourceName, slotIndex, itemInstanceId);
        }

        [TargetRpc]
        private void HandleInteractWithQuestStartItemClient(NetworkConnection networkConnection, string questResourceName, int slotIndex, long itemInstanceId) {
            Quest quest = systemDataFactory.GetResource<Quest>(questResourceName);
            if (quest == null) {
                return;
            }
            unitController.CharacterQuestLog.InteractWithQuestStartItem(quest, slotIndex, itemInstanceId);
        }

        private void HandleDialogCompletedServer(UnitController controller, Dialog dialog) {
            HandleDialogCompletedClient(dialog.ResourceName);
        }

        [ObserversRpc]
        private void HandleDialogCompletedClient(string dialogResourceName) {
            Dialog dialog = systemDataFactory.GetResource<Dialog>(dialogResourceName);
            if (dialog == null) {
                return;
            }
            unitController.CharacterDialogManager.TurnInDialog(dialog);
        }

        private void HandleWriteMessageFeedMessageServer(string messageText) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleWriteMessageFeedMessageServer({messageText})");

            HandleWriteMessageFeedMessageClient(messageText);
        }

        [ObserversRpc]
        private void HandleWriteMessageFeedMessageClient(string messageText) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleWriteMessageFeedMessageClient({messageText})");

            unitController.UnitEventController.NotifyOnWriteMessageFeedMessage(messageText);
        }

        private void HandleDespawnMountUnitServer() {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleDespawnMountUnitServer()");

            HandleDespawnMountUnitClient();
        }

        [ObserversRpc]
        public void HandleDespawnMountUnitClient() {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleDespawnMountUnitClient()");

            unitController.UnitMountManager.DespawnMountUnit();
        }

        /*
        private void HandleMountUnitSpawnServer() {
            //HandleMountUnitSpawnClient();
        }

        [ObserversRpc]
        private void HandleMountUnitSpawnClient() {
            if (base.IsOwner == true) {
                unitController.UnitMountManager.HandleMountUnitSpawn();
            }
            
        }
        */

        private void HandleSetParent(Transform parentTransform) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleSetParent({(parentTransform == null ? "null" : parentTransform.gameObject.name)})");

            if (networkObject != null && parentTransform != null) {
                //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleSetParent({(parentTransform == null ? "null" : parentTransform.gameObject.name)}) networkObject is not null");
                NetworkBehaviour nobParent = parentTransform.GetComponent<NetworkBehaviour>();
                if (nobParent == null) {
                    Debug.LogWarning($"{gameObject.name}.FishNetUnitController.HandleSetParent({parentTransform.gameObject.name}) No EmptyNetworkBehaviour found on parent.  Please check inspector!");
                } else {
                    //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleSetParent({parentTransform.gameObject.name}) setting parent transform");
                    networkObject.SetParent(nobParent);
                }
            }
        }

        private void HandleUnsetParent() {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleUnsetParent() setting parent transform to null");
            
            networkObject.UnsetParent();
            unitController.UnitMountManager.ProcessUnsetParent();
        }

        private void HandleDeactivateMountedStateOwner() {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleDeactivateMountedStateOwner()");

            HandleDeactivateMountedStateServer();
        }

        [ServerRpc]
        private void HandleDeactivateMountedStateServer() {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleDeactivateMountedStateServer() frame: {Time.frameCount}");

            unitController.UnitMountManager.DespawnMountUnit();
        }

        private void HandleDeactivateMountedState() {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleDeactivateMountedState()");

            HandleDeactivateMountedStateClient();
        }

        [ObserversRpc]
        public void HandleDeactivateMountedStateClient() {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleDeactivateMountedState()");

            unitController.UnitMountManager.DeactivateMountedState();
        }


        private void HandleActivateMountedStateServer(UnitController mountUnitController) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleActivateMountedStateServer({mountUnitController.gameObject.name})");

            HandleActiveateMountedStateClient();
        }

        [ObserversRpc]
        public void HandleActiveateMountedStateClient() {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleActivateMountedStateClient()");

            unitController.UnitMountManager.ActivateMountedState();
        }

        private void HandleSetMountedStateServer(UnitController sourceUnitController, UnitProfile unitProfile) {

            FishNetUnitController targetNetworkCharacterUnit = sourceUnitController.GetComponent<FishNetUnitController>();
            if (targetNetworkCharacterUnit == null) {
                return;
            }

            HandleSetMountedStateClient(targetNetworkCharacterUnit, unitProfile.ResourceName);
        }

        [ObserversRpc]
        private void HandleSetMountedStateClient(FishNetUnitController targetNetworkCharacterUnit, string unitProfileName) {
            UnitProfile unitProfile = systemDataFactory.GetResource<UnitProfile>(unitProfileName);
            if (unitProfile == null) {
                return;
            }
            unitController.UnitMountManager.SetMountedState(targetNetworkCharacterUnit.UnitController, unitProfile);
        }

        private void HandleDespawnActionObjectsServer() {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleDespawnActionObjectsServer()");

            HandleDespawnActionObjectsClient();
        }

        [ObserversRpc]
        private void HandleDespawnActionObjectsClient() {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleDespawnActionObjectsClient()");

            unitController.UnitActionManager.DespawnActionObjects();
        }

        private void HandleSpawnActionObjectsServer(AnimatedAction animatedAction) {
            HandleSpawnActionObjectsClient(animatedAction.ResourceName);
        }

        [ObserversRpc]
        private void HandleSpawnActionObjectsClient(string animatedActionResourceName) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleSpawnActionObjectsClient({animatedActionResourceName})");

            AnimatedAction animatedAction = systemDataFactory.GetResource<AnimatedAction>(animatedActionResourceName);
            if (animatedAction == null) {
                return;
            }
            unitController.UnitActionManager.SpawnActionObjectsInternal(animatedAction);
        }


        private void HandleDeactivateAutoAttackServer() {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleDeactivateAutoAttackServer()");

            HandleDeactivateAutoAttackClient();
        }

        [ObserversRpc]
        private void HandleDeactivateAutoAttackClient() {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleActivateAutoAttackClient()");

            unitController.CharacterCombat.DeactivateAutoAttack();
        }

        private void HandleActivateAutoAttackServer() {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleActivateAutoAttackServer()");

            HandleActivateAutoAttackClient();
        }

        [ObserversRpc]
        private void HandleActivateAutoAttackClient() {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleActivateAutoAttackClient()");

            unitController.CharacterCombat.ActivateAutoAttack();
        }

        public void HandleInitiateGlobalCooldownServer(float coolDownLength) {
            if (base.OwnerId != -1 && base.ServerManager.Clients.ContainsKey(base.OwnerId)) {
                NetworkConnection networkConnection = base.ServerManager.Clients[base.OwnerId];
                HandleInitiateGlobalCooldownClient(networkConnection, coolDownLength);
            }
        }

        [TargetRpc]
        public void HandleInitiateGlobalCooldownClient(NetworkConnection networkConnection, float coolDownLength) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleInitiateGlobalCooldownClient({coolDownLength})");
            unitController.CharacterAbilityManager.InitiateGlobalCooldown(coolDownLength);
        }

        public void HandleBeginAbilityCoolDownServer(AbilityProperties abilityProperties, float coolDownLength) {
            if (base.OwnerId != -1 && base.ServerManager.Clients.ContainsKey(base.OwnerId)) {
                NetworkConnection networkConnection = base.ServerManager.Clients[base.OwnerId];
                HandleBeginAbilityCoolDownClient(networkConnection, abilityProperties.ResourceName, coolDownLength);
            }
        }

        [TargetRpc]
        public void HandleBeginAbilityCoolDownClient(NetworkConnection networkConnection, string abilityResourceName, float coolDownLength) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleBeginAbilityCoolDownClient({abilityResourceName}, {coolDownLength})");
            Ability ability = systemDataFactory.GetResource<Ability>(abilityResourceName);
            if (ability == null) {
                return;
            }
            unitController.CharacterAbilityManager.BeginAbilityCoolDown(ability.AbilityProperties, coolDownLength);
        }

        private void HandleBeginActionCoolDownServer(InstantiatedActionItem item, float coolDownLength) {
            if (base.OwnerId != -1 && base.ServerManager.Clients.ContainsKey(base.OwnerId)) {
                NetworkConnection networkConnection = base.ServerManager.Clients[base.OwnerId];
                HandleBeginActionCoolDownClient(networkConnection, item.InstanceId, coolDownLength);
            }
        }

        [TargetRpc]
        private void HandleBeginActionCoolDownClient(NetworkConnection networkConnection, long itemInstanceId, float coolDownLength) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleBeginActionCoolDownClient({actionResourceName}, {coolDownLength})");
            if (systemItemManager.InstantiatedItems.ContainsKey(itemInstanceId) && systemItemManager.InstantiatedItems[itemInstanceId] is InstantiatedActionItem) {
                unitController.CharacterAbilityManager.BeginActionCoolDown(systemItemManager.InstantiatedItems[itemInstanceId] as InstantiatedActionItem, coolDownLength);
            }
        }



        public void HandleAddActivePetServer(UnitProfile profile, UnitController petUnitController) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleAddActivePetServer({profile?.ResourceName}, {petUnitController?.gameObject.name})");

            FishNetUnitController targetNetworkCharacterUnit = petUnitController.GetComponent<FishNetUnitController>();
            if (targetNetworkCharacterUnit == null) {
                //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleAddActivePetServer(): targetNetworkCharacterUnit is null for {petUnitController?.gameObject.name}");
                return;
            }

            HandleAddActivePetClient(profile.ResourceName, targetNetworkCharacterUnit);
        }

        [ObserversRpc]
        public void HandleAddActivePetClient(string petResourceName, FishNetUnitController targetNetworkCharacterUnit) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleAddActivePetClient({petResourceName}, {targetNetworkCharacterUnit?.gameObject.name})");
            
            if (targetNetworkCharacterUnit?.unitController == null) {
                //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleAddActivePetClient(): targetNetworkCharacterUnit is null");
                return;
            }
            UnitProfile unitProfile = systemDataFactory.GetResource<UnitProfile>(petResourceName);
            if (unitProfile == null) {
                return;
            }
            
            unitController.CharacterPetManager.AddActivePet(unitProfile, targetNetworkCharacterUnit.unitController);
        }

        public void HandleAddPetServer(UnitProfile profile) {
            HandleAddPetClient(profile.ResourceName);
        }

        [ObserversRpc]
        public void HandleAddPetClient(string petResourceName) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleAddPetClient({petResourceName})");
            
            UnitProfile unitProfile = systemDataFactory.GetResource<UnitProfile>(petResourceName);
            
            unitController.CharacterPetManager.AddPet(unitProfile);
        }

        public void HandleNameChangeServer(string newName) {
            HandleNameChangeClient(newName);
        }
        
        [ObserversRpc]
        public void HandleNameChangeClient(string newName) {
            unitController.BaseCharacter.ChangeCharacterName(newName);
        }

        private void HandleRequestAcceptQuestItemQuest(int slotIndex, long instanceId, Quest quest) {
            HandleRequestAcceptQuestItemQuestServer(slotIndex, instanceId, quest.ResourceName);
        }

        [ServerRpc]
        private void HandleRequestAcceptQuestItemQuestServer(int slotIndex, long instanceId, string questResourceName) {
            Quest quest = systemDataFactory.GetResource<Quest>(questResourceName);
            if (quest == null) {
                return;
            }
            if (unitController.CharacterInventoryManager.InventorySlots.Count <= slotIndex) {
                return;
            }
            InstantiatedItem instantiatedItem = unitController.CharacterInventoryManager.InventorySlots[slotIndex].InstantiatedItem;
            if (!(instantiatedItem is InstantiatedQuestStartItem)) {
                return;
            }
            if (instantiatedItem.InstanceId != instanceId) {
                return;
            }
            unitController.CharacterQuestLog.AcceptQuestItemQuest(instantiatedItem as InstantiatedQuestStartItem, quest);
        }

        private void HandleRequestDeleteItem(InstantiatedItem item) {
            HandleRequestDeleteItemServer(item.InstanceId);
        }

        [ServerRpc]
        private void HandleRequestDeleteItemServer(long itemInstanceId) {
            unitController.CharacterInventoryManager.DeleteItem(itemInstanceId);
        }


        private void HandleRequestCompleteQuestItemQuest(int slotIndex, long instanceId, Quest quest, QuestRewardChoices questRewardChoices) {
            HandleRequestCompleteQuestItemQuestServer(slotIndex, instanceId, quest.ResourceName, questRewardChoices);
        }

        [ServerRpc]
        private void HandleRequestCompleteQuestItemQuestServer(int slotIndex, long instanceId, string questResourceName, QuestRewardChoices questRewardChoices) {
            Quest quest = systemDataFactory.GetResource<Quest>(questResourceName);
            if (quest == null) {
                return;
            }
            if (unitController.CharacterInventoryManager.InventorySlots.Count <= slotIndex) {
                return;
            }
            InstantiatedItem instantiatedItem = unitController.CharacterInventoryManager.InventorySlots[slotIndex].InstantiatedItem;
            if (!(instantiatedItem is InstantiatedQuestStartItem)) {
                return;
            }
            if (instantiatedItem.InstanceId != instanceId) {
                return;
            }
            unitController.CharacterQuestLog.CompleteQuestItemQuest(instantiatedItem as InstantiatedQuestStartItem, quest, questRewardChoices);
        }



        public void HandleRequestClearMouseUseable(int buttonIndex) {
            RequestClearMouseUseableServer(buttonIndex);
        }

        [ServerRpc]
        public void RequestClearMouseUseableServer(int buttonIndex) {
            unitController.CharacterActionBarManager.UnSetMouseActionButton(buttonIndex);
        }

        public void HandleRequestAssignMouseUseable(IUseable useable, int buttonIndex) {
            RequestAssignMouseUseableServer(useable.ResourceName, useable is InstantiatedItem, buttonIndex);
        }

        [ServerRpc]
        public void RequestAssignMouseUseableServer(string useableName, bool isItem, int buttonIndex) {
            IUseable useable = null;
            if (isItem) {
                useable = unitController.CharacterInventoryManager.GetNewInstantiatedItem(useableName);
            } else {
                useable = systemDataFactory.GetResource<Ability>(useableName).AbilityProperties;
            }
            if (useable == null) {
                return;
            }
            unitController.CharacterActionBarManager.SetMouseActionButton(useable, buttonIndex);
        }

        public void HandleRequestMoveMouseUseable(int oldIndex, int newIndex) {
            RequestMoveMouseUseableServer(oldIndex, newIndex);
        }

        [ServerRpc]
        public void RequestMoveMouseUseableServer(int oldIndex, int newIndex) {
            unitController.CharacterActionBarManager.MoveMouseUseable(oldIndex, newIndex);
        }


        public void HandleRequestClearGamepadUseable(int buttonIndex) {
            RequestClearGamepadUseableServer(buttonIndex);
        }

        [ServerRpc]
        public void RequestClearGamepadUseableServer(int buttonIndex) {
            unitController.CharacterActionBarManager.UnSetGamepadActionButton(buttonIndex);
        }

        public void HandleRequestAssignGamepadUseable(IUseable useable, int buttonIndex) {
            RequestAssignGamepadUseableServer(useable.ResourceName, useable is InstantiatedItem, buttonIndex);
        }

        [ServerRpc]
        public void RequestAssignGamepadUseableServer(string useableName, bool isItem, int buttonIndex) {
            IUseable useable = null;
            if (isItem) {
                useable = unitController.CharacterInventoryManager.GetNewInstantiatedItem(useableName);
            } else {
                useable = systemDataFactory.GetResource<Ability>(useableName).AbilityProperties;
            }
            if (useable == null) {
                return;
            }
            //unitController.CharacterActionBarManager.AssignGamepadUseable(useable, buttonIndex);
            unitController.CharacterActionBarManager.SetGamepadActionButton(useable, buttonIndex);
        }

        public void HandleRequestMoveGamepadUseable(int oldIndex, int newIndex) {
            RequestMoveGamepadUseableServer(oldIndex, newIndex);
        }

        [ServerRpc]
        public void RequestMoveGamepadUseableServer(int oldIndex, int newIndex) {
            unitController.CharacterActionBarManager.MoveGamepadUseable(oldIndex, newIndex);
        }


        public void HandleUnsetGamepadActionButton(int buttonIndex) {
            UnSetGamepadActionButtonClient(buttonIndex);
        }

        [ObserversRpc]
        public void UnSetGamepadActionButtonClient(int buttonIndex) {
            unitController.CharacterActionBarManager.UnSetGamepadActionButton(buttonIndex);
        }

        public void HandleUnsetMouseActionButton(int buttonIndex) {
            UnSetMouseActionButtonClient(buttonIndex);
        }

        [ObserversRpc]
        public void UnSetMouseActionButtonClient(int buttonIndex) {
            unitController.CharacterActionBarManager.UnSetMouseActionButton(buttonIndex);
        }

        public void HandleSetMouseActionButton(IUseable useable, int buttonIndex) {
            SetMouseActionButtonClient(useable.ResourceName, useable is InstantiatedItem, buttonIndex);
        }

        [ObserversRpc]
        public void SetMouseActionButtonClient(string useableName, bool isItem, int buttonIndex) {
            IUseable useable = null;
            if (isItem) {
                useable = unitController.CharacterInventoryManager.GetNewInstantiatedItem(useableName);
            } else {
                useable = systemDataFactory.GetResource<Ability>(useableName)?.AbilityProperties;
            }
            if (useable == null) {
                return;
            }
            unitController.CharacterActionBarManager.SetMouseActionButton(useable, buttonIndex);
        }

        public void HandleSetGamepadActionButton(IUseable useable, int buttonIndex) {
            SetGamepadActionButtonClient(useable.ResourceName, useable is InstantiatedItem, buttonIndex);
        }

        [ObserversRpc]
        public void SetGamepadActionButtonClient(string useableName, bool isItem, int buttonIndex) {
            IUseable useable = null;
            if (isItem) {
                useable = unitController.CharacterInventoryManager.GetNewInstantiatedItem(useableName);
            } else {
                useable = systemDataFactory.GetResource<Ability>(useableName).AbilityProperties;
            }
            if (useable == null) {
                return;
            }
            unitController.CharacterActionBarManager.SetGamepadActionButton(useable, buttonIndex);
        }

        public void HandleSetReputationAmountServer(Faction faction, float amount) {
            HandleSetReputationAmountClient(faction.ResourceName, amount);
        }

        [ObserversRpc]
        public void HandleSetReputationAmountClient(string factionName, float amount) {
            Faction faction = systemDataFactory.GetResource<Faction>(factionName);
            if (faction == null) {
                return;
            }
            unitController.CharacterFactionManager.SetReputationAmount(faction, amount);
        }

        public void HandleUnlearnRecipe(Recipe recipe) {
            HandleUnlearnRecipeClient(recipe.ResourceName);
        }

        [ObserversRpc]
        public void HandleUnlearnRecipeClient(string recipeName) {
            Recipe recipe = systemDataFactory.GetResource<Recipe>(recipeName);
            if (recipe == null) {
                return;
            }
            unitController.CharacterRecipeManager.UnlearnRecipe(recipe);
        }

        public void HandleLearnRecipe(Recipe recipe) {
            HandleLearnRecipeClient(recipe.ResourceName);
        }

        [ObserversRpc]
        public void HandleLearnRecipeClient(string recipeName) {
            Recipe recipe = systemDataFactory.GetResource<Recipe>(recipeName);
            if (recipe == null) {
                return;
            }
            unitController.CharacterRecipeManager.LearnRecipe(recipe);
        }

        public void HandleCurrencyChangeServer(string currencyResourceName, int amount) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleCurrencyChangeServer({currencyResourceName}, {amount})");

            HandleCurrencyChangeClient(currencyResourceName, amount);
        }

        [ObserversRpc]
        public void HandleCurrencyChangeClient(string currencyResourceName, int amount) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleCurrencyChangeClient({currencyResourceName}, {amount})");

            Currency currency = systemDataFactory.GetResource<Currency>(currencyResourceName);
            if (currency == null) {
                return;
            }
            unitController.CharacterCurrencyManager.LoadCurrencyValue(currency, amount);
        }

        public void HandleRecoverResourceServer(PowerResource resource, int amount, CombatMagnitude magnitude, AbilityEffectContext context) {
            HandleRecoverResourceClient(resource.ResourceName, amount, magnitude, context.GetSerializableContext());
        }

        [ObserversRpc]
        public void HandleRecoverResourceClient(string resourceName, int amount, CombatMagnitude magnitude, SerializableAbilityEffectContext context) {
            PowerResource powerResource = systemDataFactory.GetResource<PowerResource>(resourceName);
            if (powerResource == null) {
                return;
            }
            unitController.UnitEventController.NotifyOnRecoverResource(powerResource, amount, magnitude, new AbilityEffectContext(unitController, null, context, systemGameManager));
        }

        public void HandleImmuneToEffectServer(AbilityEffectContext context) {
            HandleImmuneToEffectClient(context.GetSerializableContext());
        }

        [ObserversRpc]
        public void HandleImmuneToEffectClient(SerializableAbilityEffectContext context) {
            unitController.UnitEventController.NotifyOnImmuneToEffect(new AbilityEffectContext(unitController, null, context, systemGameManager));
        }

        public void HandleTakeDamageServer(IAbilityCaster sourceCaster, UnitController target, int amount, CombatTextType combatTextType, CombatMagnitude combatMagnitude, string abilityName, AbilityEffectContext context) {
            
            UnitController sourceUnitController = sourceCaster as UnitController;
            FishNetUnitController networkCharacterUnit = null;
            if (sourceUnitController != null) {
                networkCharacterUnit = sourceUnitController.GetComponent<FishNetUnitController>();
            }
            HandleTakeDamageClient(networkCharacterUnit, amount, combatTextType, combatMagnitude, abilityName, context.GetSerializableContext());
        }

        [ObserversRpc]
        public void HandleTakeDamageClient(FishNetUnitController sourceNetworkCharacterUnit, int amount, CombatTextType combatTextType, CombatMagnitude combatMagnitude, string abilityName, SerializableAbilityEffectContext context) {
            IAbilityCaster sourceCaster = null;
            if (sourceNetworkCharacterUnit == null) {
                sourceCaster = systemGameManager.SystemAbilityController;
            } else {
                sourceCaster = sourceNetworkCharacterUnit.UnitController;
            }
            unitController.UnitEventController.NotifyOnTakeDamage(sourceCaster, unitController, amount, combatTextType, combatMagnitude, abilityName, new AbilityEffectContext(unitController, null, context, systemGameManager));
        }

        public void HandleReceiveCombatTextEventServer(UnitController targetUnitController, int amount, CombatTextType type, CombatMagnitude magnitude, AbilityEffectContext context) {
            FishNetUnitController networkCharacterUnit = null;
            if (targetUnitController != null) {
                networkCharacterUnit = targetUnitController.GetComponent<FishNetUnitController>();
            }
            ReceiveCombatTextEventClient(networkCharacterUnit, amount, type, magnitude, context.GetSerializableContext());
        }

        [ObserversRpc]
        public void ReceiveCombatTextEventClient(FishNetUnitController targetNetworkCharacterUnit, int amount, CombatTextType type, CombatMagnitude magnitude, SerializableAbilityEffectContext context) {
            if (targetNetworkCharacterUnit != null) {
                unitController.UnitEventController.NotifyOnReceiveCombatTextEvent(targetNetworkCharacterUnit.unitController, amount, type, magnitude, new AbilityEffectContext(unitController, null, context, systemGameManager));
            }
        }

        public void HandleCombatMessageServer(string message) {
            HandleCombatMessageClient(message);
        }

        [ObserversRpc]
        public void HandleCombatMessageClient(string message) {
            unitController.UnitEventController.NotifyOnCombatMessage(message);
        }

        public void HandleCancelStatusEffectServer(StatusEffectProperties statusEffectProperties) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleCancelStatusEffectServer({statusEffectProperties.ResourceName})");

            CancelStatusEffectClient(statusEffectProperties.ResourceName);
        }

        [ObserversRpc]
        public void CancelStatusEffectClient(string resourceName) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.CancelStatusEffectClient({resourceName})");

            StatusEffectBase statusEffect = systemDataFactory.GetResource<AbilityEffect>(resourceName) as StatusEffectBase;
            if (statusEffect == null) {
                return;
            }
            unitController.CharacterStats.CancelStatusEffect(statusEffect.StatusEffectProperties);
        }

        public void HandleAddStatusEffectStackServer(string resourceName) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleAddStatusEffectStackServer({resourceName})");

            AddStatusEffectStackClient(resourceName);
        }

        [ObserversRpc]
        public void AddStatusEffectStackClient(string resourceName) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.AddStatusEffectStackClient({resourceName})");

            StatusEffectBase statusEffect = systemDataFactory.GetResource<AbilityEffect>(resourceName) as StatusEffectBase;
            if (statusEffect == null) {
                return;
            }
            unitController.CharacterStats.AddStatusEffectStack(statusEffect.StatusEffectProperties);
        }

        public void HandleStatusEffectAddServer(UnitController sourceUnitController, StatusEffectNode statusEffectNode) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleStatusEffectAddServer({statusEffectNode.StatusEffect.ResourceName})");

            FishNetUnitController sourceNetworkCharacterUnit = statusEffectNode.AbilityEffectContext.AbilityCaster?.AbilityManager.UnitGameObject.GetComponent<FishNetUnitController>();
            
            AddStatusEffectClient(statusEffectNode.StatusEffect.ResourceName, sourceNetworkCharacterUnit);
        }

        [ObserversRpc]
        public void AddStatusEffectClient(string resourceName, FishNetUnitController sourceNetworkCharacterUnit) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.AddStatusEffectClient({resourceName}, {sourceNetworkCharacterUnit?.gameObject.name})");

            StatusEffectBase statusEffect = systemDataFactory.GetResource<AbilityEffect>(resourceName) as StatusEffectBase;
            if (statusEffect == null) {
                return;
            }
            IAbilityCaster abilityCaster = null;
            if (sourceNetworkCharacterUnit != null) {
                abilityCaster = sourceNetworkCharacterUnit.UnitController;
            } else {
                abilityCaster = systemGameManager.SystemAbilityController;
            }
            unitController.CharacterStats.AddNewStatusEffect(statusEffect.StatusEffectProperties, abilityCaster, new AbilityEffectContext(abilityCaster));
        }

        public void HandleAddBagServer(InstantiatedBag instantiatedBag, BagNode node) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleAddBagServer({instantiatedBag.Bag.ResourceName}, {node.NodeIndex})");

            HandleAddBagClient(instantiatedBag.InstanceId, node.NodeIndex, node.IsBankNode);
        }

        [ObserversRpc]
        public void HandleAddBagClient(long itemInstanceId, int nodeIndex, bool isBankNode) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleAddBagClient({itemInstanceId}, {nodeIndex}, {isBankNode})");

            if (systemItemManager.InstantiatedItems.ContainsKey(itemInstanceId) && systemItemManager.InstantiatedItems[itemInstanceId] is InstantiatedBag) {
                BagNode bagNode = null;
                if (isBankNode && unitController.CharacterInventoryManager.BankNodes.Count > nodeIndex) {
                    bagNode = unitController.CharacterInventoryManager.BankNodes[nodeIndex];
                } else if (isBankNode == false && unitController.CharacterInventoryManager.BagNodes.Count > nodeIndex) {
                    bagNode = unitController.CharacterInventoryManager.BagNodes[nodeIndex];
                } else {
                    // invalid index
                    return;
                }
                unitController.CharacterInventoryManager.AddBag(systemItemManager.InstantiatedItems[itemInstanceId] as InstantiatedBag, bagNode);
            }
        }

        public void HandleRemoveBagServer(InstantiatedBag bag) {
            HandleRemoveBagClient(bag.InstanceId);
        }

        [ObserversRpc]
        public void HandleRemoveBagClient(long itemInstanceId) {
            if (systemItemManager.InstantiatedItems.ContainsKey(itemInstanceId) && systemItemManager.InstantiatedItems[itemInstanceId] is InstantiatedBag) {
                unitController.CharacterInventoryManager.RemoveBag(systemItemManager.InstantiatedItems[itemInstanceId] as InstantiatedBag, true);
            }
        }

        public void HandleRebuildModelAppearanceServer() {
            HandleRebuildModelAppearanceClient();
        }

        [ObserversRpc]
        public void HandleRebuildModelAppearanceClient() {
            unitController.UnitModelController.RebuildModelAppearance();
        }

        public void HandleCastTimeChanged(IAbilityCaster abilityCaster, AbilityProperties abilityProperties, float castPercent) {
            HandleCastTimeChangedClient(abilityProperties.ResourceName, castPercent);
        }

        [ObserversRpc]
        public void HandleCastTimeChangedClient(string abilityName, float castPercent) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleCastTimeChangedClient({abilityName}, {castPercent})");

            Ability ability = systemDataFactory.GetResource<Ability>(abilityName);
            unitController.UnitEventController.NotifyOnCastTimeChanged(unitController, ability.AbilityProperties, castPercent);
        }

        public void HandleCastComplete() {
            HandleCastCompleteClient();
        }

        [ObserversRpc]
        public void HandleCastCompleteClient() {
            unitController.UnitEventController.NotifyOnCastComplete();
        }

        public void HandleCastCancel() {
            HandleCastCancelClient();
        }

        [ObserversRpc]
        public void HandleCastCancelClient() {
            unitController.UnitEventController.NotifyOnCastCancel();
        }

        public void HandleAddToCraftingQueueServer(Recipe recipe) {
            HandleAddToCraftingQueueClient(recipe.ResourceName);
        }

        [ObserversRpc]
        public void HandleAddToCraftingQueueClient(string recipeName) {
            Recipe recipe = systemDataFactory.GetResource<Recipe>(recipeName);
            if (recipe == null) {
                return;
            }
            unitController.CharacterCraftingManager.AddToCraftingQueue(recipe);
        }

        public void HandleClearCraftingQueueServer() {
            HandleClearCraftingQueueClient();
        }

        [ObserversRpc]
        public void HandleClearCraftingQueueClient() {
            unitController.CharacterCraftingManager.ClearCraftingQueue();
        }

        public void HandleRemoveFirstCraftingQueueItemServer() {
            HandleRemoveFirstCraftingQueueItemClient();
        }

        [ObserversRpc]
        public void HandleRemoveFirstCraftingQueueItemClient() {
            unitController.CharacterCraftingManager.RemoveFirstQueueItem();
        }

        public void HandleCraftItemServer() {
            HandleCraftItemClient();
        }

        [ObserversRpc]
        public void HandleCraftItemClient() {
            unitController.UnitEventController.NotifyOnCraftItem();
        }

        public void HandleSetCraftAbilityServer(CraftAbilityProperties abilityProperties) {
            HandleSetCraftAbilityClient(abilityProperties.ResourceName);
        }

        [ObserversRpc]
        public void HandleSetCraftAbilityClient(string craftAbilityName) {
            CraftAbility craftAbility = systemDataFactory.GetResource<Ability>(craftAbilityName) as CraftAbility;
            if (craftAbility == null) {
                return;
            }
            unitController.CharacterCraftingManager.SetCraftAbility(craftAbility.CraftAbilityProperties);
        }

        public void HandleAddItemToInventorySlot(InventorySlot slot, InstantiatedItem item) {
            //Debug.Log($"{unitController.gameObject.name}.FishNetUnitController.HandleAddItemToInventorySlot({item.Item.ResourceName}({item.InstanceId}))");

            int slotIndex = slot.GetCurrentInventorySlotIndex(unitController);
            AddItemToInventorySlotClient(slotIndex, item.InstanceId);
        }

        [ObserversRpc]
        public void AddItemToInventorySlotClient(int slotIndex, long itemInstanceId) {
            //Debug.Log($"{unitController.gameObject.name}.FishNetUnitController.AddItemToInventorySlotClient({slotIndex}, {itemInstanceId})");

            if (systemItemManager.InstantiatedItems.ContainsKey(itemInstanceId)) {
                unitController.CharacterInventoryManager.AddInventoryItem(systemItemManager.InstantiatedItems[itemInstanceId], slotIndex);
            }
        }

        public void HandleRemoveItemFromInventorySlot(InventorySlot slot, InstantiatedItem item) {
            //Debug.Log($"{unitController.gameObject.name}.FishNetUnitController.HandleRemoveItemFromInventorySlot({item.Item.ResourceName})");

            RemoveItemFromInventorySlotClient(slot.GetCurrentInventorySlotIndex(unitController), item.InstanceId);

        }

        [ObserversRpc]
        public void RemoveItemFromInventorySlotClient(int slotIndex, long itemInstanceId) {
            //Debug.Log($"{unitController.gameObject.name}.FishNetUnitController.RemoveItemFromInventorySlotClient({slotIndex}, {itemInstanceId})");

            if (systemItemManager.InstantiatedItems.ContainsKey(itemInstanceId)) {
                unitController.CharacterInventoryManager.RemoveInventoryItem(systemItemManager.InstantiatedItems[itemInstanceId], slotIndex);
            }
        }

        public void HandleAddItemToBankSlot(InventorySlot slot, InstantiatedItem item) {
            AddItemToBankSlotClient(slot.GetCurrentBankSlotIndex(unitController), item.InstanceId);
        }

        [ObserversRpc]
        public void AddItemToBankSlotClient(int slotIndex, long itemInstanceId) {
            if (systemItemManager.InstantiatedItems.ContainsKey(itemInstanceId)) {
                unitController.CharacterInventoryManager.AddBankItem(systemItemManager.InstantiatedItems[itemInstanceId], slotIndex);
            }
        }

        public void HandleRemoveItemFromBankSlot(InventorySlot slot, InstantiatedItem item) {
            RemoveItemFromBankSlotClient(slot.GetCurrentBankSlotIndex(unitController), item.InstanceId);
        }

        [ObserversRpc]
        public void RemoveItemFromBankSlotClient(int slotIndex, long itemInstanceId) {
            if (systemItemManager.InstantiatedItems.ContainsKey(itemInstanceId)) {
                unitController.CharacterInventoryManager.RemoveBankItem(systemItemManager.InstantiatedItems[itemInstanceId], slotIndex);
            }
        }

        public void HandleRemoveEquipment(EquipmentSlotProfile profile, InstantiatedEquipment equipment) {
            HandleRemoveEquipmentClient(profile.ResourceName, equipment.InstanceId);
        }

        [ObserversRpc]
        public void HandleRemoveEquipmentClient(string equipmentSlotProfileName, long itemInstanceId) {
            if (systemItemManager.InstantiatedItems.ContainsKey(itemInstanceId) && systemItemManager.InstantiatedItems[itemInstanceId] is InstantiatedEquipment) {
                EquipmentSlotProfile equipmentSlotProfile = systemDataFactory.GetResource<EquipmentSlotProfile>(equipmentSlotProfileName);
                if (equipmentSlotProfile == null) {
                    return;
                }
                unitController.CharacterEquipmentManager.CurrentEquipment[equipmentSlotProfile].RemoveItem(systemItemManager.InstantiatedItems[itemInstanceId]);
            }

        }

        public void HandleAddEquipment(EquipmentSlotProfile profile, InstantiatedEquipment equipment) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleAddEquipment({profile.ResourceName}, {equipment.Equipment.ResourceName})");

            HandleAddEquipmentClient(profile.ResourceName, equipment.InstanceId);
        }

        [ObserversRpc]
        public void HandleAddEquipmentClient(string equipmentSlotProfileName, long itemInstanceId) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleAddEquipmentClient({equipmentSlotProfileName}, {itemInstanceId})");

            if (systemItemManager.InstantiatedItems.ContainsKey(itemInstanceId) && systemItemManager.InstantiatedItems[itemInstanceId] is InstantiatedEquipment) {
                EquipmentSlotProfile equipmentSlotProfile = systemDataFactory.GetResource<EquipmentSlotProfile>(equipmentSlotProfileName);
                if (equipmentSlotProfile == null) {
                    return;
                }
                //unitController.CharacterEquipmentManager.CurrentEquipment[equipmentSlotProfile].AddItem(systemItemManager.InstantiatedItems[itemInstanceId]);
                unitController.CharacterEquipmentManager.EquipToList(systemItemManager.InstantiatedItems[itemInstanceId] as InstantiatedEquipment, equipmentSlotProfile);
            }
        }

        /*
        public void HandleRequestUnequipFromList(EquipmentSlotProfile equipmentSlotProfile) {
            RequestUnequipFromList(equipmentSlotProfile.ResourceName);
        }

        [ServerRpc]
        public void RequestUnequipFromList(string equipmentSlotProfileName) {
            EquipmentSlotProfile equipmentSlotProfile = systemDataFactory.GetResource<EquipmentSlotProfile>(equipmentSlotProfileName);
            if (equipmentSlotProfile == null) {
                return;
            }
            unitController.CharacterEquipmentManager.UnequipFromList(equipmentSlotProfile);
        }
        */

        private void HandleRequestMoveFromBankToInventory(int slotIndex) {
            RequestMoveFromBankToInventory(slotIndex);
        }

        [ServerRpc]
        private void RequestMoveFromBankToInventory(int slotIndex) {
            unitController.CharacterInventoryManager.MoveFromBankToInventory(slotIndex);
        }

        public void HandleRequestUseItem(int slotIndex) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleRequestUseItemClient({slotIndex})");

            RequestUseItemClient(slotIndex);
        }

        [ServerRpc]
        private void RequestUseItemClient(int slotIndex) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.RequestUseItemClient({slotIndex})");

            unitController.CharacterInventoryManager.UseItem(slotIndex);
        }

        public void HandleRequestSwapInventoryEquipment(InstantiatedEquipment oldEquipment, InstantiatedEquipment newEquipment) {
            RequestSwapInventoryEquipment(oldEquipment.InstanceId, newEquipment.InstanceId);
        }

        [ServerRpc]
        public void RequestSwapInventoryEquipment(long oldEquipmentInstanceId, long newEquipmentInstanceId) {
            if (systemItemManager.InstantiatedItems.ContainsKey(oldEquipmentInstanceId) && systemItemManager.InstantiatedItems[oldEquipmentInstanceId] is InstantiatedEquipment) {
                unitController.CharacterEquipmentManager.SwapInventoryEquipment(systemItemManager.InstantiatedItems[oldEquipmentInstanceId] as InstantiatedEquipment, systemItemManager.InstantiatedItems[newEquipmentInstanceId] as InstantiatedEquipment);
            }
        }

        public void HandleRequestUnequipToSlot(InstantiatedEquipment equipment, int inventorySlotId) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleRequestUnequipToSlot({equipment.Equipment.ResourceName}, {inventorySlotId}) instanceId: {equipment.InstanceId}");
            
            RequestUnequipToSlot(equipment.InstanceId, inventorySlotId);
        }

        [ServerRpc]
        public void RequestUnequipToSlot(long itemInstanceId, int inventorySlotId) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.RequestUnequipToSlot({itemInstanceId}, {inventorySlotId})");

            if (systemItemManager.InstantiatedItems.ContainsKey(itemInstanceId) && systemItemManager.InstantiatedItems[itemInstanceId] is InstantiatedEquipment) {
                unitController.CharacterEquipmentManager.UnequipToSlot(systemItemManager.InstantiatedItems[itemInstanceId] as InstantiatedEquipment, inventorySlotId);
            }
        }

        public void HandleRequestSwapBags(InstantiatedBag oldBag, InstantiatedBag newBag) {
            RequestSwapBags(oldBag.InstanceId, newBag.InstanceId);
        }

        [ServerRpc]
        public void RequestSwapBags(long oldBagInstanceId, long newBagInstanceId) {
            if (systemItemManager.InstantiatedItems.ContainsKey(oldBagInstanceId)
                && systemItemManager.InstantiatedItems[oldBagInstanceId] is InstantiatedBag
                && systemItemManager.InstantiatedItems.ContainsKey(newBagInstanceId)
                && systemItemManager.InstantiatedItems[newBagInstanceId] is InstantiatedBag) {
                unitController.CharacterInventoryManager.SwapEquippedOrUnequippedBags(systemItemManager.InstantiatedItems[oldBagInstanceId] as InstantiatedBag, systemItemManager.InstantiatedItems[newBagInstanceId] as InstantiatedBag);
            }
        }

        public void HandleRequestUnequipBagToSlot(InstantiatedBag bag, int slotIndex, bool isBank) {
            RequestUnequipBagToSlot(bag.InstanceId, slotIndex, isBank);
        }

        [ServerRpc]
        public void RequestUnequipBagToSlot(long itemInstanceId, int slotIndex, bool isBank) {
            if (systemItemManager.InstantiatedItems.ContainsKey(itemInstanceId) && systemItemManager.InstantiatedItems[itemInstanceId] is InstantiatedBag) {
                unitController.CharacterInventoryManager.UnequipBagToSlot(systemItemManager.InstantiatedItems[itemInstanceId] as InstantiatedBag, slotIndex, isBank);
            }
        }

        public void HandleRequestUnequipBag(InstantiatedBag bag, bool isBank) {
            RequestUnequipBag(bag.InstanceId, isBank);
        }

        [ServerRpc]
        public void RequestUnequipBag(long itemInstanceId, bool isBank) {
            if (systemItemManager.InstantiatedItems.ContainsKey(itemInstanceId) && systemItemManager.InstantiatedItems[itemInstanceId] is InstantiatedBag) {
                unitController.CharacterInventoryManager.UnequipBag(systemItemManager.InstantiatedItems[itemInstanceId] as InstantiatedBag, isBank);
            }
        }

        public void HandleRequestMoveBag(InstantiatedBag bag, int nodeIndex, bool isBankNode) {
            RequestMoveBag(bag.InstanceId, nodeIndex, isBankNode);
        }

        [ServerRpc]
        public void RequestMoveBag(long itemInstanceId, int nodeIndex, bool isBankNode) {
            if (systemItemManager.InstantiatedItems.ContainsKey(itemInstanceId) && systemItemManager.InstantiatedItems[itemInstanceId] is InstantiatedBag) {
                unitController.CharacterInventoryManager.MoveBag(systemItemManager.InstantiatedItems[itemInstanceId] as InstantiatedBag, nodeIndex, isBankNode);
            }
        }

        public void HandleSetGroundTarget(Vector3 vector) {
            SetGroundTargetServer(vector);
        }

        [ServerRpc]
        public void SetGroundTargetServer(Vector3 vector) {
            unitController.CharacterAbilityManager.SetGroundTarget(vector);
        }

        public void HandleRequestAddBagFromInventory(InstantiatedBag instantiatedBag, int nodeIndex, bool isBankNode) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleRequestAddBagFromInventory({instantiatedBag.InstanceId}, {nodeIndex}, {isBankNode})");

            RequestAddBagFromInventory(instantiatedBag.InstanceId, nodeIndex, isBankNode);
        }

        [ServerRpc]
        public void RequestAddBagFromInventory(long itemInstanceId, int nodeIndex, bool isBankNode) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.RequestAddBagFromInventory({itemInstanceId}, {nodeIndex}, {isBankNode})");

            if (systemItemManager.InstantiatedItems.ContainsKey(itemInstanceId) && systemItemManager.InstantiatedItems[itemInstanceId] is InstantiatedBag) {
                unitController.CharacterInventoryManager.AddBagFromInventory(systemItemManager.InstantiatedItems[itemInstanceId] as InstantiatedBag, nodeIndex, isBankNode);
            }
        }

        public void HandleRequestCancelStatusEffect(StatusEffectProperties properties) {
            RequestCancelStatusEffect(properties.ResourceName);
        }

        [ServerRpc]
        public void RequestCancelStatusEffect(string resourceName) {
            StatusEffectBase statusEffect = systemDataFactory.GetResource<AbilityEffect>(resourceName) as StatusEffectBase;
            if (statusEffect == null) {
                return;
            }
            unitController.CharacterStats.CancelStatusEffect(statusEffect.StatusEffectProperties);
        }


        public void HandleRequestMoveFromInventoryToBank(int slotIndex) {
            RequestMoveFromInventoryToBank(slotIndex);
        }

        [ServerRpc]
        private void RequestMoveFromInventoryToBank(int slotIndex) {
            unitController.CharacterInventoryManager.MoveFromInventoryToBank(slotIndex);
        }

        private void HandleRequestDropItemFromInventorySlot(InventorySlot fromSlot, InventorySlot toSlot, bool fromSlotIsInventory, bool toSlotIsInventory) {
            int fromSlotIndex;
            if (fromSlotIsInventory) {
                fromSlotIndex = fromSlot.GetCurrentInventorySlotIndex(unitController);
            } else {
                fromSlotIndex = fromSlot.GetCurrentBankSlotIndex(unitController);
            }
            int toSlotIndex;
            if (toSlotIsInventory) {
                toSlotIndex = toSlot.GetCurrentInventorySlotIndex(unitController);
            } else {
                toSlotIndex = toSlot.GetCurrentBankSlotIndex(unitController);
            }
            RequestDropItemFromInventorySlot(fromSlotIndex, toSlotIndex, fromSlotIsInventory, toSlotIsInventory);
        }

        [ServerRpc]
        private void RequestDropItemFromInventorySlot(int fromSlotId, int toSlotId, bool fromSlotIsInventory, bool toSlotIsInventory) {
            unitController.CharacterInventoryManager.DropItemFromInventorySlot(fromSlotId, toSlotId, fromSlotIsInventory, toSlotIsInventory);
        }


        public void HandleRequestEquipToSlot(InstantiatedEquipment equipment, EquipmentSlotProfile profile) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleRequestEquipToSlot({equipment.Equipment.ResourceName}, {profile.ResourceName}) instanceId: {equipment.InstanceId}");

            RequestEquipToSlot(equipment.InstanceId, profile.ResourceName);
        }

        [ServerRpc]
        public void RequestEquipToSlot(long itemInstanceId, string equipmentSlotProfileName) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.RequestEquipToSlot({itemInstanceId}, {equipmentSlotProfileName})");

            if (systemItemManager.InstantiatedItems.ContainsKey(itemInstanceId) && systemItemManager.InstantiatedItems[itemInstanceId] is InstantiatedEquipment) {
                EquipmentSlotProfile equipmentSlotProfile = systemDataFactory.GetResource<EquipmentSlotProfile>(equipmentSlotProfileName);
                if (equipmentSlotProfile == null) {
                    return;
                }
                unitController.CharacterEquipmentManager.EquipToSlot(systemItemManager.InstantiatedItems[itemInstanceId] as InstantiatedEquipment, equipmentSlotProfile);
            }
        }

        /*
        public void HandleDeleteItemServer(InstantiatedItem item) {
            HandleDeleteItemClient(item.InstanceId);
        }
        */

        /*
        [ObserversRpc]
        public void HandleDeleteItemClient(long itemInstanceId) {
            unitController.CharacterInventoryManager.DeleteItem(itemInstanceId);
        }
        */

        public void HandleGetNewInstantiatedItem(InstantiatedItem instantiatedItem) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleGetNewInstantiatedItem({instantiatedItem.ResourceName}) instanceId: {instantiatedItem.InstanceId}");
            
            ItemInstanceSaveData itemInstanceSaveData = instantiatedItem.GetItemSaveData();
            HandleGetNewInstantiatedItemClient(itemInstanceSaveData);
        }

        [ObserversRpc]
        public void HandleGetNewInstantiatedItemClient(ItemInstanceSaveData itemInstanceSaveData) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleGetNewInstantiatedItemClient({itemInstanceSaveData.ItemInstanceId}, {itemInstanceSaveData.ItemName}");
            
            unitController.CharacterInventoryManager.GetNewInstantiatedItemFromSaveData(itemInstanceSaveData);
        }

        public void HandleMarkQuestCompleteServer(UnitController controller, Quest quest) {
            HandleMarkQuestCompleteClient(quest.ResourceName);
        }

        [ObserversRpc]
        public void HandleMarkQuestCompleteClient(string questName) {
            Quest quest = systemDataFactory.GetResource<Quest>(questName);
            if (quest == null) {
                return;
            }
            unitController.CharacterQuestLog.MarkQuestComplete(quest);
        }

        public void HandleMarkAchievementCompleteServer(UnitController controller, Achievement achievement) {
            HandleMarkAchievementCompleteClient(achievement.ResourceName);
        }

        [ObserversRpc]
        public void HandleMarkAchievementCompleteClient(string resourceName) {
            Achievement achievement = systemDataFactory.GetResource<Achievement>(resourceName);
            if (achievement == null) {
                return;
            }
            unitController.CharacterQuestLog.MarkAchievementComplete(achievement);
        }


        public void HandleQuestObjectiveStatusUpdatedServer(UnitController controller, QuestBase questBase) {
            HandleQuestObjectiveStatusUpdatedClient(questBase.ResourceName);
        }

        [ObserversRpc]
        public void HandleQuestObjectiveStatusUpdatedClient(string questName) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleQuestObjectiveStatusUpdatedClient({questName})");

            Quest quest = systemDataFactory.GetResource<Quest>(questName);
            if (quest == null) {
                return;
            }
            unitController.UnitEventController.NotifyOnQuestObjectiveStatusUpdated(quest);
            quest.StepsComplete(unitController, true);
        }

        public void HandleAchievementObjectiveStatusUpdatedServer(UnitController controller, Achievement achievement) {
            HandleAchievementObjectiveStatusUpdatedClient(achievement.ResourceName);
        }

        [ObserversRpc]
        public void HandleAchievementObjectiveStatusUpdatedClient(string resourceName) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleAchievementObjectiveStatusUpdatedClient({resourceName})");

            Achievement achievement = systemDataFactory.GetResource<Achievement>(resourceName);
            if (achievement == null) {
                return;
            }
            unitController.UnitEventController.NotifyOnAchievementObjectiveStatusUpdated(achievement);
            achievement.StepsComplete(unitController, true);
        }


        [ObserversRpc]
        public void HandleSetQuestObjectiveCurrentAmount(string questName, string objectiveType, string objectiveName, int amount) {
            unitController.CharacterQuestLog.SetQuestObjectiveCurrentAmount(questName, objectiveType, objectiveName, amount);
        }

        [ObserversRpc]
        public void HandleSetAchievementObjectiveCurrentAmount(string questName, string objectiveType, string objectiveName, int amount) {
            unitController.CharacterQuestLog.SetAchievementObjectiveCurrentAmount(questName, objectiveType, objectiveName, amount);
        }

        public void HandleLearnSkillServer(UnitController sourceUnitController, Skill skill) {
            HandleLearnSkillClient(skill.ResourceName);
        }

        [ObserversRpc]
        public void HandleLearnSkillClient(string skillName) {
            Skill skill = systemDataFactory.GetResource<Skill>(skillName);
            if (skill != null) {
                unitController.CharacterSkillManager.LearnSkill(skill);
            }
        }

        public void HandleUnLearnSkillServer(UnitController sourceUnitController, Skill skill) {
            HandleUnLearnSkillClient(skill.ResourceName);
        }

        [ObserversRpc]
        public void HandleUnLearnSkillClient(string skillName) {
            Skill skill = systemDataFactory.GetResource<Skill>(skillName);
            if (skill != null) {
                unitController.CharacterSkillManager.UnLearnSkill(skill);
            }
        }

        public void HandleAcceptQuestServer(UnitController sourceUnitController, Quest quest) {
            HandleAcceptQuestClient(quest.ResourceName);
        }

        [ObserversRpc]
        public void HandleAcceptQuestClient(string questName) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleAcceptQuestClient({questName})");

            Quest quest = systemDataFactory.GetResource<Quest>(questName);
            if (quest != null) {
                unitController.CharacterQuestLog.AcceptQuest(quest);
            }
        }

        public void HandleAcceptAchievementServer(UnitController sourceUnitController, Achievement achievement) {
            HandleAcceptAchievementClient(achievement.ResourceName);
        }

        [ObserversRpc]
        public void HandleAcceptAchievementClient(string resourceName) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleAcceptQuestClient({resourceName})");

            Achievement achievement = systemDataFactory.GetResource<Achievement>(resourceName);
            if (achievement != null) {
                unitController.CharacterQuestLog.AcceptAchievement(achievement);
            }
        }


        public void HandleAbandonQuestServer(UnitController sourceUnitController, QuestBase quest) {
            HandleAbandonQuestClient(quest.ResourceName);
        }

        [ObserversRpc]
        public void HandleAbandonQuestClient(string questName) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleAbandonQuestClient({questName})");

            Quest quest = systemDataFactory.GetResource<Quest>(questName);
            if (quest != null) {
                unitController.CharacterQuestLog.AbandonQuest(quest);
            }
        }

        public void HandleTurnInQuestServer(UnitController sourceUnitController, QuestBase quest) {
            HandleTurnInQuestClient(quest.ResourceName);
        }

        [ObserversRpc]
        public void HandleTurnInQuestClient(string questName) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleTurnInQuestClient({questName})");

            Quest quest = systemDataFactory.GetResource<Quest>(questName);
            if (quest != null) {
                unitController.CharacterQuestLog.TurnInQuest(quest);
            }
        }

        public void HandleRemoveQuestServer(UnitController sourceUnitController, QuestBase quest) {
            HandleRemoveQuestClient(quest.ResourceName);
        }

        [ObserversRpc]
        public void HandleRemoveQuestClient(string questName) {
            Quest quest = systemDataFactory.GetResource<Quest>(questName);
            if (quest != null) {
                unitController.CharacterQuestLog.RemoveQuest(quest);
            }
        }

        private void HandleEnterInteractableRangeServer(UnitController controller, Interactable interactable) {

            FishNetInteractable networkInteractable = null;
            if (interactable != null) {
                networkInteractable = interactable.GetComponent<FishNetInteractable>();
            }
            HandleEnterInteractableRangeClient(networkInteractable);
        }

        [ObserversRpc]
        private void HandleEnterInteractableRangeClient(FishNetInteractable networkInteractable) {
            Interactable interactable = null;
            if (networkInteractable != null) {
                interactable = networkInteractable.Interactable;
            }
            unitController.UnitEventController.NotifyOnEnterInteractableRange(interactable);
        }

        private void HandleExitInteractableRangeServer(UnitController controller, Interactable interactable) {

            FishNetInteractable networkInteractable = null;
            if (interactable != null) {
                networkInteractable = interactable.GetComponent<FishNetInteractable>();
            }
            HandleExitInteractableRangeClient(networkInteractable);
        }

        [ObserversRpc]
        private void HandleExitInteractableRangeClient(FishNetInteractable networkInteractable) {
            Interactable interactable = null;
            if (networkInteractable != null) {
                interactable = networkInteractable.Interactable;
            }
            unitController.UnitEventController.NotifyOnExitInteractableRange(interactable);
        }


        public void HandleSpecializationChangeServer(UnitController sourceUnitController, ClassSpecialization newSpecialization, ClassSpecialization oldSpecialization) {
            HandleSpecializationChangeClient(newSpecialization == null ? string.Empty : newSpecialization.ResourceName);
        }

        [ObserversRpc]
        public void HandleSpecializationChangeClient(string newSpecializationName) {
            ClassSpecialization newSpecialization = systemDataFactory.GetResource<ClassSpecialization>(newSpecializationName);
            unitController.BaseCharacter.ChangeClassSpecialization(newSpecialization);
        }

        public void HandleFactionChangeServer(Faction newFaction, Faction oldFaction) {
            HandleFactionChangeClient(newFaction.ResourceName);
        }

        [ObserversRpc]
        public void HandleFactionChangeClient(string newFactionName) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleFactionChangeClient({newFactionName})");

            Faction newFaction = systemDataFactory.GetResource<Faction>(newFactionName);
            if (newFaction == null) {
                return;
            }
            unitController.BaseCharacter.ChangeCharacterFaction(newFaction);
        }


        public void HandleClassChangeServer(UnitController sourceUnitController, CharacterClass newCharacterClass, CharacterClass oldCharacterClass) {
            HandleClassChangeClient(newCharacterClass.ResourceName);
        }

        [ObserversRpc]
        public void HandleClassChangeClient(string newCharacterClassName) {
            CharacterClass newCharacterClass = systemDataFactory.GetResource<CharacterClass>(newCharacterClassName);
            unitController.BaseCharacter.ChangeCharacterClass(newCharacterClass);
        }

        /*
        private void HandleEnterInteractableTriggerServer(Interactable triggerInteractable) {
            NetworkInteractable networkTarget = null;
            if (triggerInteractable != null) {
                networkTarget = triggerInteractable.GetComponent<NetworkInteractable>();
            }

        }
        */

        [ObserversRpc]
        public void HandleEnterInteractableTriggerClient(FishNetInteractable networkInteractable) {
            Interactable triggerInteractable = null;
            if (networkInteractable != null) {
                triggerInteractable = networkInteractable.Interactable;
            }
            unitController.UnitEventController.NotifyOnEnterInteractableTrigger(triggerInteractable);
        }

        /*
        public void HandleDespawnClient(UnitController controller) {
        }
        */


        public void HandleDespawn(UnitController controller) {
            HandleDespawnClient();
        }

        [ObserversRpc]
        public void HandleDespawnClient() {
            unitController.Despawn(0, false, true);
        }


        [ObserversRpc]
        public void HandleLevelChanged(int newLevel) {
            unitController.CharacterStats.SetLevel(newLevel);
        }

        public void HandleGainXPServer(UnitController controller, int gainedXP, int currentXP) {
            HandleGainXP(gainedXP, currentXP);
        }

        [ObserversRpc]
        private void HandleGainXP(int gainedXP, int currentXP) {
            unitController.CharacterStats.SetXP(currentXP);
            unitController.UnitEventController.NotifyOnGainXP(gainedXP, currentXP);
        }

        public void HandleSpawnAbilityEffectPrefabsServer(Interactable target, Interactable originalTarget, LengthEffectProperties lengthEffectProperties, AbilityEffectContext abilityEffectContext) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleSpawnAbilityEffectPrefabsServer()");

            FishNetInteractable networkTarget = null;
            if (target != null) {
                networkTarget = target.GetComponent<FishNetInteractable>();
            }
            FishNetInteractable networkOriginalTarget = null;
            if (originalTarget != null) {
                networkOriginalTarget = originalTarget.GetComponent<FishNetInteractable>();
            }
            HandleSpawnAbilityEffectPrefabsClient(networkTarget, networkOriginalTarget, lengthEffectProperties.ResourceName, abilityEffectContext.GetSerializableContext());
        }

        [ObserversRpc]
        public void HandleSpawnAbilityEffectPrefabsClient(FishNetInteractable networkTarget, FishNetInteractable networkOriginalTarget, string abilityEffectName, SerializableAbilityEffectContext serializableAbilityEffectContext) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleSpawnAbilityObjectsClient({networkTarget?.gameObject.name}, {networkOriginalTarget?.gameObject.name}, {abilityEffectName})");

            AbilityEffect abilityEffect = systemGameManager.SystemDataFactory.GetResource<AbilityEffect>(abilityEffectName);
            if (abilityEffect == null) {
                return;
            }
            FixedLengthEffectProperties fixedLengthEffectProperties = abilityEffect.AbilityEffectProperties as FixedLengthEffectProperties;
            if (fixedLengthEffectProperties == null) {
                return;
            }
            Interactable target = null;
            Interactable originalTarget = null;
            if (networkTarget != null) {
                target = networkTarget.Interactable;
            }
            if (networkOriginalTarget != null) {
                originalTarget = networkOriginalTarget.Interactable;
            }
            unitController.CharacterAbilityManager.SpawnAbilityEffectPrefabs(target, originalTarget, fixedLengthEffectProperties, new AbilityEffectContext(unitController, originalTarget, serializableAbilityEffectContext, systemGameManager));
        }

        public void HandleSpawnProjectileEffectPrefabsServer(Interactable target, Interactable originalTarget, ProjectileEffectProperties projectileEffectProperties, AbilityEffectContext abilityEffectContext) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleSpawnProjectileEffectPrefabsServer({target?.gameObject.name}, {originalTarget?.gameObject.name}, {projectileEffectProperties.ResourceName})");

            FishNetInteractable networkTarget = null;
            if (target != null) {
                networkTarget = target.GetComponent<FishNetInteractable>();
            }
            FishNetInteractable networkOriginalTarget = null;
            if (target != null) {
                networkOriginalTarget = originalTarget.GetComponent<FishNetInteractable>();
            }
            HandleSpawnProjectileEffectPrefabsClient(networkTarget, networkOriginalTarget, projectileEffectProperties.ResourceName, abilityEffectContext.GetSerializableContext());
        }

        [ObserversRpc]
        public void HandleSpawnProjectileEffectPrefabsClient(FishNetInteractable networkTarget, FishNetInteractable networkOriginalTarget, string abilityEffectName, SerializableAbilityEffectContext serializableAbilityEffectContext) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleSpawnProjectileEffectPrefabsClient({networkTarget?.gameObject.name}, {networkOriginalTarget?.gameObject.name}, {abilityEffectName})");

            ProjectileEffect abilityEffect = systemGameManager.SystemDataFactory.GetResource<AbilityEffect>(abilityEffectName) as ProjectileEffect;
            if (abilityEffect == null) {
                return;
            }
            ProjectileEffectProperties projectileEffectProperties = abilityEffect.AbilityEffectProperties as ProjectileEffectProperties;
            if (projectileEffectProperties == null) {
                return;
            }
            Interactable target = null;
            Interactable originalTarget = null;
            if (networkTarget != null) {
                target = networkTarget.Interactable;
            }
            if (networkOriginalTarget != null) {
                originalTarget = networkOriginalTarget.Interactable;
            }
            unitController.CharacterAbilityManager.SpawnProjectileEffectPrefabs(target, originalTarget, projectileEffectProperties, new AbilityEffectContext(unitController, originalTarget, serializableAbilityEffectContext, systemGameManager));
        }

        public void HandleSpawnChanneledEffectPrefabsServer(Interactable target, Interactable originalTarget, ChanneledEffectProperties channeledEffectProperties, AbilityEffectContext abilityEffectContext) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleSpawnChanneledEffectPrefabsServer({target?.gameObject.name}, {originalTarget?.gameObject.name}, {channeledEffectProperties.ResourceName})");

            FishNetInteractable networkTarget = null;
            if (target != null) {
                networkTarget = target.GetComponent<FishNetInteractable>();
            }
            FishNetInteractable networkOriginalTarget = null;
            if (target != null) {
                networkOriginalTarget = originalTarget.GetComponent<FishNetInteractable>();
            }
            HandleSpawnChanneledEffectPrefabsClient(networkTarget, networkOriginalTarget, channeledEffectProperties.ResourceName, abilityEffectContext.GetSerializableContext());
        }

        [ObserversRpc]
        public void HandleSpawnChanneledEffectPrefabsClient(FishNetInteractable networkTarget, FishNetInteractable networkOriginalTarget, string abilityEffectName, SerializableAbilityEffectContext serializableAbilityEffectContext) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleSpawnProjectileEffectPrefabsClient({networkTarget?.gameObject.name}, {networkOriginalTarget?.gameObject.name}, {abilityEffectName})");

            ChanneledEffect abilityEffect = systemGameManager.SystemDataFactory.GetResource<AbilityEffect>(abilityEffectName) as ChanneledEffect;
            if (abilityEffect == null) {
                return;
            }
            ChanneledEffectProperties channeledEffectProperties = abilityEffect.AbilityEffectProperties as ChanneledEffectProperties;
            if (channeledEffectProperties == null) {
                return;
            }
            Interactable target = null;
            Interactable originalTarget = null;
            if (networkTarget != null) {
                target = networkTarget.Interactable;
            }
            if (networkOriginalTarget != null) {
                originalTarget = networkOriginalTarget.Interactable;
            }
            unitController.CharacterAbilityManager.SpawnChanneledEffectPrefabs(target, originalTarget, channeledEffectProperties, new AbilityEffectContext(unitController, originalTarget, serializableAbilityEffectContext, systemGameManager));
        }


        public void HandleSpawnAbilityObjectsServer(AbilityProperties ability, int index) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleSpawnAbilityObjectsServer({ability.ResourceName}, {index})");

            HandleSpawnAbilityObjectsClient(ability.ResourceName, index);
        }

        [ObserversRpc]
        public void HandleSpawnAbilityObjectsClient(string abilityName, int index) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleSpawnAbilityObjectsClient()");

            Ability ability = systemGameManager.SystemDataFactory.GetResource<Ability>(abilityName);
            if (ability != null) {
                unitController.CharacterAbilityManager.SpawnAbilityObjectsInternal(ability.AbilityProperties, index);
            }
        }

        [ObserversRpc]
        public void HandleDespawnAbilityObjects() {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleDespawnAbilityObjects()");

            unitController.CharacterAbilityManager.DespawnAbilityObjects();
        }

        [ObserversRpc]
        public void HandleDropCombat() {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleDropCombat()");

            //unitController.CharacterCombat.TryToDropCombat();
            unitController.CharacterCombat.DropCombat(true);
        }

        private void HandleEnterCombatServer(Interactable targetInteractable) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleEnterCombatServer(" + (targetInteractable == null ? "null" : targetInteractable.gameObject.name) + ")");

            FishNetInteractable networkInteractable = null;
            if (targetInteractable != null) {
                networkInteractable = targetInteractable.GetComponent<FishNetInteractable>();
            }
            HandleEnterCombatClient(networkInteractable);
        }

        [ObserversRpc]
        public void HandleEnterCombatClient(FishNetInteractable networkInteractable) {
            //Debug.Log($"{gameObject.name}.HandleEnterCombatClient()");
            
            if (networkInteractable != null) {
                unitController.CharacterCombat.EnterCombat(networkInteractable.Interactable);
            }
        }


        private void HandleBeforeDieServer(UnitController targetUnitController) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleBeforeDieServer(" + (targetUnitController == null ? "null" : targetUnitController.gameObject.name) + ")");

            HandleBeforeDieClient();
        }

        [ObserversRpc]
        public void HandleBeforeDieClient() {
            //Debug.Log($"{gameObject.name}.HandleBeforeDieClient()");

            unitController.CharacterStats.Die();
        }

        private void HandleReviveBeginServer(float reviveTime) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleReviveBeginServer()");

            // we don't need to pass the time to the client, since it will be calculated there anyway
            HandleReviveBeginClient();
        }

        [ObserversRpc]
        public void HandleReviveBeginClient() {
            //Debug.Log($"{gameObject.name}.HandleBeforeDieClient()");

            unitController.CharacterStats.Revive();
        }

        private void HandleReviveCompleteServer(UnitController targetUnitController) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleReviveCompleteServer(" + (targetUnitController == null ? "null" : targetUnitController.gameObject.name) + ")");

            HandleReviveCompleteClient();
        }

        [ObserversRpc]
        public void HandleReviveCompleteClient() {
            //Debug.Log($"{gameObject.name}.HandleReviveCompleteClient()");

            unitController.CharacterStats.ReviveComplete();
        }


        private void HandleClearTargetClient(Interactable oldTarget) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleClearTargetClient(" + (oldTarget == null ? "null" : oldTarget.gameObject.name) + ")");

            /*
            NetworkInteractable networkInteractable = null;
            if (oldTarget != null) {
                networkInteractable = oldTarget.GetComponent<NetworkInteractable>();
            }
            HandleClearTargetServer(networkInteractable);
            */
            HandleClearTargetServer();
        }

        [ServerRpc]
        private void HandleClearTargetServer(/*NetworkInteractable networkInteractable*/) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleClearTargetServer(" + (networkInteractable == null ? "null" : networkInteractable.gameObject.name) + ")");
            
            //unitController.SetTarget((networkInteractable == null ? null : networkInteractable.interactable));
            
            unitController.ClearTarget();
        }


        private void HandleSetTargetClient(Interactable target) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleSetTargetClient({(target == null ? "null" : target.gameObject.name)})");

            FishNetInteractable networkInteractable = null;
            if (target != null) {
                networkInteractable = target.GetComponent<FishNetInteractable>();
            }
            HandleSetTargetServer(networkInteractable);
        }

        [ServerRpc]
        private void HandleSetTargetServer(FishNetInteractable networkInteractable) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleSetTargetServer(" + (networkInteractable == null ? "null" : networkInteractable.gameObject.name) + ")");

            unitController.SetTarget((networkInteractable == null ? null : networkInteractable.Interactable));
        }

        private void BeginCharacterRequest() {
            systemGameManager.CharacterManager.BeginCharacterRequest(unitController);
        }

        private void CompleteCharacterRequest(bool isOwner, PlayerCharacterSaveData playerCharacterSaveData, int characterGroupId, int guildId, string guildName) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.CompleteCharacterRequest({isOwner}, {(saveData == null ? "null" : "valid save data")})");

            /*
            if (base.Owner != null ) {
                //Debug.Log($"{gameObject.name}.FishNetUnitController.CompleteCharacterRequest({isOwner}) owner accountId: {base.OwnerId}");
            }
            */

            unitProfile = systemGameManager.SystemDataFactory.GetResource<UnitProfile>(unitProfileName.Value);
            if (networkManagerServer.ServerModeActive == true) {
                systemGameManager.CharacterManager.CompleteCharacterRequest(unitController);
            } else {
                // first load items
                systemItemManager.LoadItemInstanceListSaveData(playerCharacterSaveData.ItemInstanceListSaveData);

                CharacterConfigurationRequest characterConfigurationRequest;
                characterConfigurationRequest = new CharacterConfigurationRequest(unitProfile);
                characterConfigurationRequest.unitControllerMode = unitControllerMode.Value;
                CharacterRequestData characterRequestData = new CharacterRequestData(null, GameMode.Network, characterConfigurationRequest);
                characterRequestData.isOwner = isOwner;
                characterRequestData.characterId = characterId.Value;
                characterRequestData.characterGroupId = characterGroupId;
                characterRequestData.characterGuildId = guildId;
                characterRequestData.characterGuildName = guildName;
                if (isOwner == true && unitControllerMode.Value == UnitControllerMode.Player) {
                    characterRequestData.characterRequestor = systemGameManager.PlayerManager;
                }
                if (playerCharacterSaveData != null) {
                    characterRequestData.saveData = playerCharacterSaveData.CharacterSaveData;
                    //Debug.Log($"{gameObject.name}.FishNetUnitController.CompleteCharacterRequest({isOwner}, isMounted: {saveData.isMounted})");
                }
                unitController.SetCharacterRequestData(characterRequestData);
                systemGameManager.CharacterManager.CompleteNetworkCharacterRequest(unitController);
            }

            OnCompleteCharacterRequest();
        }

        public void HandleResourceAmountChangedServer(PowerResource powerResource, int oldValue, int newValue) {
            HandleResourceAmountChangedClient(powerResource.resourceName, oldValue, newValue);
        }

        [ObserversRpc]
        public void HandleResourceAmountChangedClient(string powerResourceName, int oldValue, int newValue) {
            unitController.CharacterStats.SetResourceAmount(powerResourceName, newValue);
        }

        public void HandleBeginChatMessageServer(string messageText) {
            HandleBeginChatMessageClient(messageText);
        }

        [ObserversRpc]
        public void HandleBeginChatMessageClient(string messageText) {
            unitController.BeginChatMessage(messageText);
        }

        public void HandlePerformAbilityCastAnimationServer(AbilityProperties baseAbility, int clipIndex) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandlePerformAbilityCastAnimationServer({baseAbility.ResourceName}, {clipIndex})");

            HandlePerformAbilityCastAnimationClient(baseAbility.ResourceName, clipIndex);
        }

        [ObserversRpc]
        public void HandlePerformAbilityCastAnimationClient(string abilityName, int clipIndex) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandlePerformAbilityCastAnimationClient({abilityName}, {clipIndex})");

            Ability baseAbility = systemDataFactory.GetResource<Ability>(abilityName);
            if (baseAbility == null) {
                return;
            }
            unitController.UnitAnimator.PerformAbilityCast(baseAbility.AbilityProperties, clipIndex);
        }

        public void HandlePerformAbilityActionAnimationServer(AbilityProperties baseAbility, int clipIndex) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandlePerformAbilityActionAnimationServer({baseAbility.ResourceName}, {clipIndex})");

            HandlePerformAbilityActionAnimationClient(baseAbility.ResourceName, clipIndex);
        }

        [ObserversRpc]
        public void HandlePerformAbilityActionAnimationClient(string abilityName, int clipIndex) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandlePerformAbilityActionAnimationClient({abilityName})");

            Ability baseAbility = systemDataFactory.GetResource<Ability>(abilityName);
            if (baseAbility == null) {
                return;
            }
            unitController.UnitAnimator.PerformAbilityAction(baseAbility.AbilityProperties, clipIndex);
        }


        public void HandlePerformAnimatedActionServer(AnimatedAction animatedAction) {
            HandlePerformAnimatedActionClient(animatedAction.ResourceName);
        }

        [ObserversRpc]
        public void HandlePerformAnimatedActionClient(string actionName) {
            AnimatedAction animatedAction = systemDataFactory.GetResource<AnimatedAction>(actionName);
            if (animatedAction == null) {
                return;
            }
            unitController.UnitAnimator.PerformAnimatedAction(animatedAction);
        }

        [ObserversRpc]
        public void HandleClearActionClient() {
            unitController.UnitAnimator.ClearAction();
        }

        [ObserversRpc]
        public void HandleClearAnimatedAbilityClient() {
            unitController.UnitAnimator.ClearAnimatedAbility();
        }

        [ObserversRpc]
        public void HandleClearCastingClient() {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleClearCastingClient()");

            unitController.UnitAnimator.ClearCasting();
        }

        /*
        [ObserversRpc]
        public void HandleAnimatorDeathClient() {
            unitController.UnitAnimator.HandleDie();
        }
        */

        public void HandleBeginAbilityLocal(AbilityProperties abilityProperties, Interactable target, bool playerInitiated) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleBeginAbilityLocal({abilityProperties.ResourceName})");


            FishNetInteractable targetNetworkInteractable = null;
            if (target != null) {
                targetNetworkInteractable = target.GetComponent<FishNetInteractable>();
            }
            HandleBeginAbilityServer(abilityProperties.ResourceName, targetNetworkInteractable, playerInitiated);
        }

        [ServerRpc]
        public void HandleBeginAbilityServer(string abilityName, FishNetInteractable targetNetworkInteractable, bool playerInitiated) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.HandleBeginAbilityServer({abilityName})");

            Ability baseAbility = systemDataFactory.GetResource<Ability>(abilityName);
            if (baseAbility == null) {
                return;
            }
            Interactable targetInteractable = null;
            if (targetNetworkInteractable != null) {
                targetInteractable = targetNetworkInteractable.GetComponent<Interactable>();
            }
            unitController.CharacterAbilityManager.BeginAbility(baseAbility.AbilityProperties, targetInteractable, playerInitiated);
        }

        [ServerRpc]
        public void HandleBeginAction(string actionName, bool playerInitiated) {
            AnimatedAction animatedAction = systemDataFactory.GetResource<AnimatedAction>(actionName);
            if (animatedAction == null) {
                return;
            }
            unitController.UnitActionManager.BeginActionInternal(animatedAction, playerInitiated);
        }

        /*
        [ServerRpc(RequireOwnership = false)]
        public void GetClientSaveData(NetworkConnection networkConnection = null) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.GetClientSaveData()");

            PutClientSaveData(networkConnection, unitController.CharacterSaveManager.SaveData);
        }

        [TargetRpc]
        public void PutClientSaveData(NetworkConnection networkConnection, AnyRPGSaveData saveData) {
            //Debug.Log($"{gameObject.name}.FishNetUnitController.PutClientSaveData()");

            CompleteClientCharacterRequest(saveData);
        }
        */


    }
}

