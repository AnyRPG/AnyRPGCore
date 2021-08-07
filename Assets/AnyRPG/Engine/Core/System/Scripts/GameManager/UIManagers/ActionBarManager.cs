using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class ActionBarManager : ConfiguredMonoBehaviour {

        [SerializeField]
        private SystemBarController systemBarController = null;

        [SerializeField]
        private List<ActionBarController> actionBarControllers = new List<ActionBarController>();

        private bool abilityBarsPopulated = false;

        private ActionButton fromButton = null;

        protected bool eventSubscriptionsInitialized = false;

        private Coroutine targetRangeRoutine = null;

        // the action bar target for range checks
        private Interactable target = null;

        private PlayerManager playerManager = null;
        private KeyBindManager keyBindManager = null;
        private SystemEventManager systemEventManager = null;

        public ActionButton FromButton { get => fromButton; set => fromButton = value; }
        public List<ActionBarController> ActionBarControllers { get => actionBarControllers; set => actionBarControllers = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            playerManager = systemGameManager.PlayerManager;
            keyBindManager = systemGameManager.KeyBindManager;
            systemEventManager = systemGameManager.SystemEventManager;

            systemBarController.Configure(systemGameManager);
            InitializeActionbars();

            AssociateActionBarKeyBinds();
            CreateEventSubscriptions();
        }

        private void CreateEventSubscriptions() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StartListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
            SystemEventManager.StartListening("OnPlayerUnitDespawn", HandlePlayerUnitDespawn);
            SystemEventManager.StartListening("OnPlayerConnectionDespawn", HandlePlayerConnectionDespawn);
            systemEventManager.OnEquipmentChanged += HandleEquipmentChange;
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StopListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
            SystemEventManager.StopListening("OnPlayerUnitDespawn", HandlePlayerUnitDespawn);
            SystemEventManager.StopListening("OnPlayerConnectionDespawn", HandlePlayerConnectionDespawn);
            systemEventManager.OnEquipmentChanged -= HandleEquipmentChange;
            eventSubscriptionsInitialized = false;
        }

        public void InitializeActionbars() {
            foreach (ActionBarController actionBarController in actionBarControllers) {
                actionBarController.Configure(systemGameManager);
            }
        }

        public void HandlePlayerConnectionDespawn(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log("ActionBarManager.HandlePlayerConnectionDespawn()");
            ClearActionBars(true);
        }

        public void HandlePlayerUnitSpawn(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log(gameObject.name + ".InanimateUnit.HandlePlayerUnitSpawn()");
            ProcessPlayerUnitSpawn();
        }


        public void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            CleanupEventSubscriptions();
        }

        public void ProcessPlayerUnitSpawn() {
            //Debug.Log("ActionBarmanager.HandlePlayerUnitSpawn()");
            playerManager.UnitController.OnSetTarget += HandleSetTarget;
            playerManager.UnitController.OnClearTarget += HandleClearTarget;
        }

        public void HandlePlayerUnitDespawn(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log("ActionBarmanager.HandlePlayerUnitDespawn()");

            // this needs to be called manually here because if the character controller processes the player unit despawn after us, we will miss the event
            HandleClearTarget(null);

            playerManager.UnitController.OnSetTarget -= HandleSetTarget;
            playerManager.UnitController.OnClearTarget -= HandleClearTarget;
        }

        public void HandleSetTarget(Interactable target) {
            //Debug.Log("ActionBarmanager.HandleSetTarget()");
            this.target = target;
            if (targetRangeRoutine == null) {
                targetRangeRoutine = StartCoroutine(UpdateTargetRange());
            }
        }

        public void HandleClearTarget(Interactable oldTarget) {
            //Debug.Log("ActionBarmanager.HandleClearTarget()");
            if (targetRangeRoutine != null) {
                StopCoroutine(targetRangeRoutine);
                targetRangeRoutine = null;
            }
            ResetRangeColors();
            target = null;
        }

        public bool HasTarget() {
            return (target != null);
        }

        public void ResetRangeColors() {
            //Debug.Log("ActionBarmanager.ResetRangeColors()");
            foreach (ActionButton actionButton in GetActionButtons()) {
                if (actionButton.KeyBindText.color != Color.white) {
                    actionButton.KeyBindText.color = Color.white;
                }
            }
        }

        public IEnumerator UpdateTargetRange() {
            //Debug.Log("ActionBarmanager.UpdateTargetRange()");
            //float distanceToTarget = 0f;
            bool inRange = false;
            while (HasTarget()) {
                if (playerManager.MyCharacter == null || playerManager.ActiveUnitController == null) {
                    break;
                }
                //Debug.Log("ActionBarmanager.UpdateTargetRange(): still have target at distance: " + distanceToTarget);
                foreach (ActionButton actionButton in GetActionButtons()) {
                    if ((actionButton.Useable as BaseAbility) is BaseAbility) {
                        BaseAbility baseAbility = actionButton.Useable as BaseAbility;
                        //Debug.Log("ActionBarmanager.UpdateTargetRange(): actionbutton: " + baseAbility.MyName);

                        Interactable finalTarget = baseAbility.ReturnTarget(playerManager.MyCharacter, target, false);
                        //distanceToTarget = Vector3.Distance(playerManager.ActiveUnitController.transform.position, target.transform.position);
                        //Debug.Log("ActionBarmanager.UpdateTargetRange(): actionbutton: " + baseAbility.DisplayName + "; finalTarget: " + (finalTarget == null ? "null" : finalTarget.gameObject.name));

                        inRange = false;
                        if (finalTarget != null) {
                            inRange = playerManager.MyCharacter.CharacterAbilityManager.IsTargetInRange(finalTarget, baseAbility);
                        }
                        if (inRange) {
                            if (actionButton.KeyBindText.color != Color.white) {
                                actionButton.KeyBindText.color = Color.white;
                                //Debug.Log("ActionBarmanager.UpdateTargetRange(): setting color to white for ability " + baseAbility.MyName);
                            }
                        } else {
                            if (actionButton.KeyBindText.color != Color.red) {
                                actionButton.KeyBindText.color = Color.red;
                                //Debug.Log("ActionBarmanager.UpdateTargetRange(): setting color to red for ability " + baseAbility.MyName);
                            }
                        }
                    } else {
                        if (actionButton.KeyBindText.color != Color.white) {
                            actionButton.KeyBindText.color = Color.white;
                            //Debug.Log("ActionBarmanager.UpdateTargetRange(): setting color to white");
                        }
                    }
                }
                yield return null;
            }
            targetRangeRoutine = null;
            //Debug.Log("ActionBarmanager.UpdateTargetRange(): exiting coroutine");
        }

        public List<ActionButton> GetActionButtons() {
            //Debug.Log("ActionBarManager.GetActionButtons()");
            List<ActionButton> actionButtons = new List<ActionButton>();
            int count = 0;
            foreach (ActionBarController actionBarController in actionBarControllers) {
                foreach (ActionButton actionButton in actionBarController.ActionButtons) {
                    actionButtons.Add(actionButton);
                    //Debug.Log("ActionBarManager.GetActionButtons() count: " + count + "; actionbutton: " + actionButton.name + actionButton.GetInstanceID());
                    count++;
                }
                //actionButtons.AddRange(actionBarController.MyActionButtons);
            }
            return actionButtons;
        }

        private void AssociateActionBarKeyBinds() {
            //Debug.Log("ActionBarManager.AssociateActionBarKeyBinds()");
            int count = 1;
            foreach (ActionButton actionButton in GetActionButtons()) {
                if (keyBindManager.MyKeyBinds.Count >= count) {
                    //Debug.Log("ActionBarManager.AssociateActionBarKeyBinds(): associate count: ACT" + count + " with actionButton " + actionButton.name + actionButton.GetInstanceID());
                    if (keyBindManager.MyKeyBinds.ContainsKey("ACT" + count.ToString())) {
                        keyBindManager.MyKeyBinds["ACT" + count.ToString()].MyActionButton = actionButton;
                        count++;
                    } else {
                        //Debug.Log("ActionBarManager.AssociateActionBarKeyBinds(): ran out of keybinds to associate with available action buttons!");
                        return;
                    }
                } else {
                    //Debug.Log("ActionBarManager.AssociateActionBarKeyBinds(): ran out of keybinds to associate with available action buttons!");
                    return;
                }
            }
        }

        public bool AddNewAbility(BaseAbility newAbility) {
            //Debug.Log("ActionBarManager.AddNewAbility()");
            foreach (ActionBarController actionBarController in actionBarControllers) {
                //Debug.Log("ActionBarManager.AddNewAbility(): looping through a controller");
                if (actionBarController.AddSavedAbility(newAbility)) {
                    return true;
                }
            }
            foreach (ActionBarController actionBarController in actionBarControllers) {
                if (actionBarController.AddNewAbility(newAbility)) {
                    //Debug.Log("ActionBarManager.AddNewAbility(): we were able to add " + newAbility.name);
                    return true;
                }
            }
            return false;
        }

        public void ClearActionBars(bool clearSavedUseables = false) {
            //Debug.Log("ActionBarManager.ClearActionBars()");
            foreach (ActionBarController actionBarController in actionBarControllers) {
                //Debug.Log("ActionBarManager.AddNewAbility(): looping through a controller");
                actionBarController.ClearActionBar(clearSavedUseables);
            }
        }


        /*
        public bool AbilityIsOnBars() {

        }
        */


        public void PopulateAbilityBars() {
            //Debug.Log("ActionBarmanager.PopulateAbilityBars()");
            if (abilityBarsPopulated) {
                //Debug.Log("ActionBarmanager.PopulateAbilityBars(): bars are already populated.  Doing nothing!");
                return;
            }
            // TODO: set maximum size of loop to less of abilitylist count or button count
            int abilityListCount = playerManager.MyCharacter.CharacterAbilityManager.AbilityList.Count;
            //Debug.Log("Updating ability bar with " + abilityListCount.ToString() + " abilities");
            foreach (BaseAbility newAbility in playerManager.MyCharacter.CharacterAbilityManager.AbilityList.Values) {
                AddNewAbility(newAbility);
            }
            abilityBarsPopulated = true;
        }

        public void UpdateVisuals(bool removeStaleActions = false) {
            foreach (ActionBarController actionBarController in actionBarControllers) {
                //Debug.Log("ActionBarManager.AddNewAbility(): looping through a controller");
                actionBarController.UpdateVisuals(removeStaleActions);
            }
        }

        public void HandleEquipmentChange(Equipment newEquipment, Equipment oldEquipment) {
            UpdateVisuals();
        }

    }

}