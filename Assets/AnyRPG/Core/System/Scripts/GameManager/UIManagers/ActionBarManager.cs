using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class ActionBarManager : ConfiguredMonoBehaviour {

        [SerializeField]
        private SystemBarController systemBarController = null;

        [SerializeField]
        private List<ActionBarController> actionBarControllers = new List<ActionBarController>();

        [SerializeField]
        private List<ActionBarController> gamepadActionBarControllers = new List<ActionBarController>();

        [SerializeField]
        protected Image leftBackground;

        [SerializeField]
        protected Image rightBackground;

        [SerializeField]
        protected Color normalColor = new Color32(0, 0, 0, 0);

        [SerializeField]
        protected Color pressedColor = new Color32(255, 255, 255, 128);

        private bool abilityBarsPopulated = false;

        private ActionButton fromButton = null;

        protected bool eventSubscriptionsInitialized = false;

        private Coroutine targetRangeRoutine = null;

        // the action bar target for range checks
        private Interactable target = null;

        protected PlayerManager playerManager = null;
        protected KeyBindManager keyBindManager = null;
        protected SystemEventManager systemEventManager = null;
        protected ControlsManager controlsManager = null;

        public ActionButton FromButton { get => fromButton; set => fromButton = value; }
        public List<ActionBarController> ActionBarControllers { get => actionBarControllers; set => actionBarControllers = value; }
        public List<ActionBarController> GamepadActionBarControllers { get => gamepadActionBarControllers; set => gamepadActionBarControllers = value; }
        public SystemBarController SystemBarController { get => systemBarController; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            playerManager = systemGameManager.PlayerManager;
            keyBindManager = systemGameManager.KeyBindManager;
            systemEventManager = systemGameManager.SystemEventManager;
            controlsManager = systemGameManager.ControlsManager;

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

        public void ProcessInput() {
            if (controlsManager.LeftTriggerDown) {
                PressLeftTrigger();
            }
            if (controlsManager.LeftTriggerUp) {
                LiftLeftTrigger();
            }
            if (controlsManager.RightTriggerDown) {
                PressRightTrigger();
            }
            if (controlsManager.RightTriggerUp) {
                LiftRightTrigger();
            }
        }

        public void PressLeftTrigger() {
            //Debug.Log("ActionBarManager.PressLeftTrigger()");
            leftBackground.color = pressedColor;
        }

        public void LiftLeftTrigger() {
            //Debug.Log("ActionBarManager.LiftLeftTrigger()");
            leftBackground.color = normalColor;
        }

        public void PressRightTrigger() {
            //Debug.Log("ActionBarManager.PressRightTrigger()");
            rightBackground.color = pressedColor;
        }

        public void LiftRightTrigger() {
            //Debug.Log("ActionBarManager.LiftRightTrigger()");
            rightBackground.color = normalColor;
        }

        public void InitializeActionbars() {
            foreach (ActionBarController actionBarController in actionBarControllers) {
                actionBarController.Configure(systemGameManager);
            }
            foreach (ActionBarController actionBarController in gamepadActionBarControllers) {
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
            foreach (ActionButton actionButton in GetCurrentActionButtons()) {
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
                foreach (ActionButton actionButton in GetCurrentActionButtons()) {
                    if ((actionButton.Useable as BaseAbility) is BaseAbility) {
                        BaseAbility baseAbility = actionButton.Useable as BaseAbility;

                        Interactable finalTarget = baseAbility.ReturnTarget(playerManager.MyCharacter, target, false);

                        inRange = false;
                        if (finalTarget != null) {
                            inRange = playerManager.MyCharacter.CharacterAbilityManager.IsTargetInRange(finalTarget, baseAbility);
                        }
                        if (inRange) {
                            if (actionButton.KeyBindText.color != Color.white) {
                                actionButton.KeyBindText.color = Color.white;
                            }
                        } else {
                            if (actionButton.KeyBindText.color != Color.red) {
                                actionButton.KeyBindText.color = Color.red;
                            }
                        }
                    } else {
                        if (actionButton.KeyBindText.color != Color.white) {
                            actionButton.KeyBindText.color = Color.white;
                        }
                    }
                }
                yield return null;
            }
            targetRangeRoutine = null;
        }

        public List<ActionButton> GetCurrentActionButtons() {
            if (controlsManager.GamePadModeActive == true) {
                return GetActionButtons(gamepadActionBarControllers);
            } else {
                return GetActionButtons(actionBarControllers);
            }
        }

        public List<ActionButton> GetActionButtons(List<ActionBarController> barControllers) {
            //Debug.Log("ActionBarManager.GetActionButtons()");
            List<ActionButton> actionButtons = new List<ActionButton>();
            int count = 0;
            foreach (ActionBarController actionBarController in barControllers) {
                foreach (ActionButton actionButton in actionBarController.ActionButtons) {
                    actionButtons.Add(actionButton);
                    //Debug.Log("ActionBarManager.GetActionButtons() count: " + count + "; actionbutton: " + actionButton.name + actionButton.GetInstanceID());
                    count++;
                }
                //actionButtons.AddRange(actionBarController.MyActionButtons);
            }
            return actionButtons;
        }

        public List<ActionButton> GetMouseActionButtons() {
            //Debug.Log("ActionBarManager.GetActionButtons()");
            
            return GetActionButtons(actionBarControllers);
        }

        public List<ActionButton> GetGamepadActionButtons() {
            //Debug.Log("ActionBarManager.GetActionButtons()");

            return GetActionButtons(GamepadActionBarControllers);
        }

        private void AssociateActionBarKeyBinds() {
            //Debug.Log("ActionBarManager.AssociateActionBarKeyBinds()");
            int count = 1;
            foreach (ActionButton actionButton in GetMouseActionButtons()) {
                if (keyBindManager.KeyBinds.Count >= count) {
                    //Debug.Log("ActionBarManager.AssociateActionBarKeyBinds(): associate count: ACT" + count + " with actionButton " + actionButton.name + actionButton.GetInstanceID());
                    if (keyBindManager.KeyBinds.ContainsKey("ACT" + count.ToString())) {
                        keyBindManager.KeyBinds["ACT" + count.ToString()].ActionButton = actionButton;
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
            bool returnValue = false;
            bool foundSlot = false;
            foreach (ActionBarController actionBarController in gamepadActionBarControllers) {
                //Debug.Log("ActionBarManager.AddNewAbility(): looping through a controller");
                if (actionBarController.AddSavedAbility(newAbility)) {
                    if (controlsManager.GamePadModeActive) {
                        returnValue = true;
                    }
                    foundSlot = true;
                    break;
                }
            }
            if (foundSlot != true) {
                foreach (ActionBarController actionBarController in gamepadActionBarControllers) {
                    if (actionBarController.AddNewAbility(newAbility)) {
                        //Debug.Log("ActionBarManager.AddNewAbility(): we were able to add " + newAbility.name);
                        if (controlsManager.GamePadModeActive) {
                            returnValue = true;
                        }
                        break;
                    }
                }
            }

            foundSlot = false;
            foreach (ActionBarController actionBarController in actionBarControllers) {
                //Debug.Log("ActionBarManager.AddNewAbility(): looping through a controller");
                if (actionBarController.AddSavedAbility(newAbility)) {
                    if (controlsManager.GamePadModeActive == false) {
                        returnValue = true;
                    }
                    foundSlot = true;
                    break;
                }
            }
            if (foundSlot == false) {
                foreach (ActionBarController actionBarController in actionBarControllers) {
                    if (actionBarController.AddNewAbility(newAbility)) {
                        //Debug.Log("ActionBarManager.AddNewAbility(): we were able to add " + newAbility.name);
                        if (controlsManager.GamePadModeActive == false) {
                            returnValue = true;
                        }
                        break;
                    }
                }
            }

            return returnValue;
        }

        public void ClearActionBars(bool clearSavedUseables = false) {
            //Debug.Log("ActionBarManager.ClearActionBars()");
            foreach (ActionBarController actionBarController in actionBarControllers) {
                //Debug.Log("ActionBarManager.AddNewAbility(): looping through a controller");
                actionBarController.ClearActionBar(clearSavedUseables);
            }
            foreach (ActionBarController actionBarController in gamepadActionBarControllers) {
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
            if (controlsManager.GamePadModeActive == true) {
                foreach (ActionBarController actionBarController in gamepadActionBarControllers) {
                    //Debug.Log("ActionBarManager.AddNewAbility(): looping through a controller");
                    actionBarController.UpdateVisuals(removeStaleActions);
                }
            } else {
                foreach (ActionBarController actionBarController in actionBarControllers) {
                    //Debug.Log("ActionBarManager.AddNewAbility(): looping through a controller");
                    actionBarController.UpdateVisuals(removeStaleActions);
                }
            }
        }

        public void HandleEquipmentChange(Equipment newEquipment, Equipment oldEquipment) {
            UpdateVisuals();
        }

        public void UpdateActionBars() {
            //Debug.Log("UIManager.UpdateActionBars()");

            if (controlsManager.GamePadModeActive == true) {

                // hide system bar
                if (systemBarController.gameObject.activeSelf) {
                    systemBarController.gameObject.SetActive(false);
                }

                // deactivate action bar controllers
                for (int i = 0; i <= 6; i++) {
                    if (actionBarControllers[i].gameObject.activeSelf) {
                        actionBarControllers[i].gameObject.SetActive(false);
                    }
                }

                // show gamepad hints
                leftBackground.gameObject.SetActive(true);
                rightBackground.gameObject.SetActive(true);

                // activate gamepad action bar controllers
                for (int i = 0; i <= 1; i++) {
                    if (!gamepadActionBarControllers[i].gameObject.activeSelf) {
                        gamepadActionBarControllers[i].gameObject.SetActive(true);
                    }
                }

            } else {
                // hide gamepad controllers
                for (int i = 0; i <= 1; i++) {
                    if (gamepadActionBarControllers[i].gameObject.activeSelf) {
                        gamepadActionBarControllers[i].gameObject.SetActive(false);
                    }
                }

                // hide gamepad hints
                leftBackground.gameObject.SetActive(false);
                rightBackground.gameObject.SetActive(false);

                // show systembar
                if (!systemBarController.gameObject.activeSelf) {
                    systemBarController.gameObject.SetActive(true);
                }

                if (PlayerPrefs.GetInt("UseActionBar2") == 0) {
                    if (actionBarControllers[1].gameObject.activeSelf) {
                        actionBarControllers[1].gameObject.SetActive(false);
                    }
                } else if (PlayerPrefs.GetInt("UseActionBar2") == 1) {
                    if (!actionBarControllers[1].gameObject.activeSelf) {
                        actionBarControllers[1].gameObject.SetActive(true);
                    }
                }

                if (PlayerPrefs.GetInt("UseActionBar3") == 0) {
                    if (actionBarControllers[2].gameObject.activeSelf) {
                        actionBarControllers[2].gameObject.SetActive(false);
                    }
                } else if (PlayerPrefs.GetInt("UseActionBar3") == 1) {
                    if (!actionBarControllers[2].gameObject.activeSelf) {
                        actionBarControllers[2].gameObject.SetActive(true);
                    }
                }

                if (PlayerPrefs.GetInt("UseActionBar4") == 0) {
                    if (actionBarControllers[3].gameObject.activeSelf) {
                        actionBarControllers[3].gameObject.SetActive(false);
                    }
                } else if (PlayerPrefs.GetInt("UseActionBar4") == 1) {
                    if (!actionBarControllers[3].gameObject.activeSelf) {
                        actionBarControllers[3].gameObject.SetActive(true);
                    }
                }
                if (PlayerPrefs.GetInt("UseActionBar5") == 0) {
                    if (actionBarControllers[4].gameObject.activeSelf) {
                        actionBarControllers[4].gameObject.SetActive(false);
                    }
                } else if (PlayerPrefs.GetInt("UseActionBar5") == 1) {
                    if (!actionBarControllers[4].gameObject.activeSelf) {
                        actionBarControllers[4].gameObject.SetActive(true);
                    }
                }
                if (PlayerPrefs.GetInt("UseActionBar6") == 0) {
                    if (actionBarControllers[5].gameObject.activeSelf) {
                        actionBarControllers[5].gameObject.SetActive(false);
                    }
                } else if (PlayerPrefs.GetInt("UseActionBar6") == 1) {
                    if (!actionBarControllers[5].gameObject.activeSelf) {
                        actionBarControllers[5].gameObject.SetActive(true);
                    }
                }
                if (PlayerPrefs.GetInt("UseActionBar7") == 0) {
                    if (actionBarControllers[6].gameObject.activeSelf) {
                        actionBarControllers[6].gameObject.SetActive(false);
                    }
                } else if (PlayerPrefs.GetInt("UseActionBar7") == 1) {
                    if (!actionBarControllers[6].gameObject.activeSelf) {
                        actionBarControllers[6].gameObject.SetActive(true);
                    }
                }
            }

        }

    }

}