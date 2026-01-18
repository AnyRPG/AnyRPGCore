using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class ActionBarManager : ConfiguredMonoBehaviour {

        [SerializeField]
        private SystemBarController systemBarController = null;

        [SerializeField]
        private List<ActionBarController> actionBarControllers = new List<ActionBarController>();

        [SerializeField]
        private GamepadPanel gamepadPanel = null;

        [SerializeField]
        private List<GamepadActionBarController> gamepadActionBarControllers = new List<GamepadActionBarController>();

        protected Color hiddenColor = new Color32(0, 0, 0, 0);

        protected int numActionBarSets = 4;

        protected int currentActionBarSet = 0;

        protected int gamepadActionButtonCount = 64;

        // index of button that move started from, -1 = nothing
        protected int moveIndex = -1;

        //protected List<List<IUseable>> actionBarSet = new List<List<IUseable>>();
        //protected List<IUseable> actionButtons = new List<IUseable>(70);
        protected List<ActionButton> mouseActionButtons = new List<ActionButton>();
        protected List<ActionButton> gamepadActionButtons = new List<ActionButton>();

        private ActionButton fromButton = null;

        protected bool eventSubscriptionsInitialized = false;

        private Coroutine targetRangeRoutine = null;

        protected IUseable assigningUseable = null;

        // the action bar target for range checks
        private Interactable target = null;

        protected PlayerManager playerManager = null;
        protected KeyBindManager keyBindManager = null;
        protected ControlsManager controlsManager = null;
        protected InputManager inputManager = null;
        protected UIManager uIManager = null;


        public ActionButton FromButton { get => fromButton; set => fromButton = value; }
        public List<ActionBarController> ActionBarControllers { get => actionBarControllers; set => actionBarControllers = value; }
        public List<GamepadActionBarController> GamepadActionBarControllers { get => gamepadActionBarControllers; set => gamepadActionBarControllers = value; }
        public SystemBarController SystemBarController { get => systemBarController; }
        public IUseable AssigningUseable { get => assigningUseable; }
        public List<ActionButton> GamepadActionButtons { get => gamepadActionButtons; }
        public int CurrentActionBarSet { get => currentActionBarSet; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            playerManager = systemGameManager.PlayerManager;
            keyBindManager = systemGameManager.KeyBindManager;
            controlsManager = systemGameManager.ControlsManager;
            inputManager = systemGameManager.InputManager;
            uIManager = systemGameManager.UIManager;

            systemBarController.Configure(systemGameManager);

            InitializeActionbars();
            InitializeMouseActionButtons();
            InitializeGamepadActionButtons();

            AssociateActionBarKeyBinds();
            CreateEventSubscriptions();

        }

        private void InitializeMouseActionButtons() {
            //Debug.Log("ActionBarManager.InitializeGamepadActionButtonNodes()");
            mouseActionButtons.AddRange(GetMouseActionButtons());
            int counter = 0;
            foreach (ActionButton actionButton in mouseActionButtons) {
                actionButton.SetIndex(counter, false);
                counter++;
            }
        }

        private void InitializeGamepadActionButtons() {
            //Debug.Log("ActionBarManager.InitializeGamepadActionButtonNodes()");
            gamepadActionButtons.AddRange(GetGamepadActionButtons());
        }

        private void CreateEventSubscriptions() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            systemEventManager.OnPlayerUnitSpawn += HandlePlayerUnitSpawn;
            systemEventManager.OnPlayerUnitDespawn += HandlePlayerUnitDespawn;
            SystemEventManager.StartListening("OnPlayerConnectionDespawn", HandlePlayerConnectionDespawn);
            systemEventManager.OnAddEquipment += HandleAddEquipment;
            systemEventManager.OnRemoveEquipment += HandleRemoveEquipment;
            systemEventManager.OnSetMouseActionButton += HandleSetMouseActionButton;
            systemEventManager.OnUnsetMouseActionButton += HandleUnsetMouseActionButton;
            systemEventManager.OnSetGamepadActionButton += HandleSetGamepadActionButton;
            systemEventManager.OnUnsetGamepadActionButton += HandleUnsetGamepadActionButton;
            eventSubscriptionsInitialized = true;
        }

        public void HandleUnsetGamepadActionButton(int buttonIndex) {
            if (buttonIndex >= (currentActionBarSet * 16) && buttonIndex < ((currentActionBarSet * 16) + 16)) {
                gamepadActionButtons[buttonIndex].ClearUseable();
            }
        }

        public void HandleSetGamepadActionButton(IUseable useable, int buttonIndex) {
            if (buttonIndex >= (currentActionBarSet * 16) && buttonIndex < ((currentActionBarSet * 16) + 16)) {
                gamepadActionButtons[buttonIndex].SetUseable(useable);
            }
        }

        public void HandleSetMouseActionButton(IUseable useable, int buttonIndex) {
            mouseActionButtons[buttonIndex].SetUseable(useable);
        }

        public void HandleUnsetMouseActionButton(int buttonIndex) {
            mouseActionButtons[buttonIndex].ClearUseable();
        }

        private void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            systemEventManager.OnPlayerUnitSpawn -= HandlePlayerUnitSpawn;
            systemEventManager.OnPlayerUnitDespawn -= HandlePlayerUnitDespawn;
            SystemEventManager.StopListening("OnPlayerConnectionDespawn", HandlePlayerConnectionDespawn);
            systemEventManager.OnAddEquipment -= HandleAddEquipment;
            systemEventManager.OnRemoveEquipment -= HandleRemoveEquipment;
            eventSubscriptionsInitialized = false;
        }

        public void StartUseableAssignment(IUseable useable, int moveIndex = -1) {
            //Debug.Log("ActionBarManager.StartUseableAssignment(" + moveIndex + ")");

            this.moveIndex = moveIndex;
            assigningUseable = useable;
        }

        public void ClearUseableAssignment() {
            //Debug.Log("ActionBarManager.ClearUseableAssignment()");

            moveIndex = -1;
            assigningUseable = null;
        }

        public void RequestAssignUseableByIndex(int index) {
            //Debug.Log($"ActionBarManager.AssignUseableByIndex({index})");

            if (moveIndex > -1) {
                RequestMoveGamepadUseable((currentActionBarSet * 16) + moveIndex, (currentActionBarSet * 16) + index);
            } else {
                RequestAssignGamepadUseable(assigningUseable, (currentActionBarSet * 16) + index);
            }
        }

        public void RequestMoveGamepadUseable(int oldIndex, int newIndex) {
            playerManager.UnitController.CharacterActionBarManager.RequestMoveGamepadUseable(oldIndex, newIndex);
        }

        public void RequestAssignGamepadUseable(IUseable useable, int buttonIndex) {
            playerManager.UnitController.CharacterActionBarManager.RequestAssignGamepadUseable(useable, buttonIndex);
        }

        public void RequestClearGamepadUseable(int index) {
            //Debug.Log($"ActionBarManager.RequestClearGamepadUseable({index})");

            playerManager.UnitController.CharacterActionBarManager.RequestClearGamepadUseable((currentActionBarSet * 16) + index);
        }

        public void RequestMoveMouseUseable(int oldIndex, int newIndex) {
            playerManager.UnitController.CharacterActionBarManager.RequestMoveMouseUseable(oldIndex, newIndex);
        }

        public void RequestAssignMouseUseable(IUseable useable, int actionButtonIndex) {
            playerManager.UnitController.CharacterActionBarManager.RequestAssignMouseUseable(useable, actionButtonIndex);
        }

        public void RequestClearMouseUseable(int index) {
            //Debug.Log($"ActionBarManager.RequestClearGamepadUseable({index})");

            playerManager.UnitController.CharacterActionBarManager.RequestClearMouseUseable(index);
        }


        public void ProcessGamepadInput() {
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

            if (inputManager.KeyBindWasPressed("JOYSTICKBUTTON4")) {
                // LB
                if (currentActionBarSet > 0) {
                    SetGamepadActionButtonSet(currentActionBarSet - 1);
                }
            } else if (inputManager.KeyBindWasPressed("JOYSTICKBUTTON5")) {
                // RB
                if (currentActionBarSet < (numActionBarSets - 1)) {
                    SetGamepadActionButtonSet(currentActionBarSet + 1);
                }
            }
        }

        public void PressLeftTrigger() {
            //Debug.Log("ActionBarManager.PressLeftTrigger()");
            gamepadPanel.PressLeftTrigger();
        }

        public void LiftLeftTrigger() {
            //Debug.Log("ActionBarManager.LiftLeftTrigger()");
            gamepadPanel.LiftLeftTrigger();
        }

        public void PressRightTrigger() {
            //Debug.Log("ActionBarManager.PressRightTrigger()");
            gamepadPanel.PressRightTrigger();
        }

        public void LiftRightTrigger() {
            //Debug.Log("ActionBarManager.LiftRightTrigger()");
            gamepadPanel.LiftRightTrigger();
        }

        public void InitializeActionbars() {
            for (int i = 0; i < actionBarControllers.Count; i++) {
                actionBarControllers[i].Configure(systemGameManager);
                if (i <= 2) {
                    actionBarControllers[i].SetTooltipTransform(uIManager.BottomPanel.RectTransform);
                } else {
                    actionBarControllers[i].SetTooltipTransform(uIManager.SidePanel.RectTransform);
                }
            }
            foreach (ActionBarController actionBarController in actionBarControllers) {
            }
            foreach (ActionBarController actionBarController in gamepadActionBarControllers) {
                actionBarController.Configure(systemGameManager);
            }
        }

        public void HandlePlayerConnectionDespawn(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log("ActionBarManager.HandlePlayerConnectionDespawn()");
            ClearActionBars(true);
        }

        public void HandlePlayerUnitSpawn(UnitController sourceUnitController) {
            //Debug.Log($"ActionBarManager.HandlePlayerUnitSpawn({sourceUnitController.gameObject.name})");

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
            //Debug.Log("ActionBarmanager.ProcessPlayerUnitSpawn()");

            playerManager.UnitController.UnitEventController.OnSetTarget += HandleSetTarget;
            playerManager.UnitController.UnitEventController.OnClearTarget += HandleClearTarget;
        }

        public void HandlePlayerUnitDespawn(UnitController unitController) {
            //Debug.Log("ActionBarmanager.HandlePlayerUnitDespawn()");

            ClearActionBars(true);

            // this needs to be called manually here because if the character controller processes the player unit despawn after us, we will miss the event
            HandleClearTarget(null);

            playerManager.UnitController.UnitEventController.OnSetTarget -= HandleSetTarget;
            playerManager.UnitController.UnitEventController.OnClearTarget -= HandleClearTarget;
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
                /*
                if (actionButton.KeyBindText.color != Color.white) {
                    actionButton.KeyBindText.color = Color.white;
                }
                */
                /*
                if (actionButton.RangeIndicator.color != Color.white && actionButton.Useable != null) {
                    actionButton.RangeIndicator.color = Color.white;
                }
                */
                actionButton.HideRangeIndicator();
                //actionButton.RangeIndicator.color = hiddenColor;
            }
        }

        public IEnumerator UpdateTargetRange() {
            //Debug.Log("ActionBarmanager.UpdateTargetRange()");
            //float distanceToTarget = 0f;
            while (HasTarget()) {
                if (playerManager.UnitController == null || playerManager.ActiveUnitController == null) {
                    break;
                }
                //Debug.Log("ActionBarmanager.UpdateTargetRange(): still have target at distance: " + distanceToTarget);
                foreach (ActionButton actionButton in GetCurrentActionButtons()) {
                    actionButton.Useable?.UpdateTargetRange(this, actionButton);
                }
                yield return null;
            }

            targetRangeRoutine = null;
        }

        public void UpdateAbilityTargetRange(AbilityProperties baseAbilityProperties, ActionButton actionButton) {
            //Debug.Log($"ActionBarmanager.UpdateAbilityTargetRange({baseAbilityProperties.DisplayName})");

            Interactable finalTarget = baseAbilityProperties.ReturnTarget(playerManager.UnitController, target, false);

            if (finalTarget == null || playerManager.UnitController.CharacterAbilityManager.IsTargetInRange(finalTarget, baseAbilityProperties) == false) {
                //Debug.Log($"ActionBarmanager.UpdateAbilityTargetRange({baseAbilityProperties.DisplayName}) finalTarget: {(finalTarget == null ? "null" : finalTarget.gameObject.name)}");
                if (actionButton.RangeIndicator.color != Color.red && actionButton.Useable != null) {
                    actionButton.RangeIndicator.color = Color.red;
                }

            } else {
                //Debug.Log($"ActionBarmanager.UpdateAbilityTargetRange({baseAbilityProperties.DisplayName}) hiding range indicator");
                if (actionButton.RangeIndicator.color != hiddenColor) {
                    actionButton.HideRangeIndicator();
                }
            }

        }

        public List<ActionButton> GetCurrentActionButtons() {
            if (controlsManager.GamePadModeActive == true) {
                return GetActionButtons(gamepadActionBarControllers.Cast<ActionBarController>().ToList());
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
            //Debug.Log($"ActionBarManager.GetActionButtons(): returning {actionButtons.Count} action buttons.");
            return actionButtons;
        }

        public List<ActionButton> GetMouseActionButtons() {
            //Debug.Log("ActionBarManager.GetMouseActionButtons()");

            return GetActionButtons(actionBarControllers);
        }

        public List<ActionButton> GetGamepadActionButtons() {
            //Debug.Log("ActionBarManager.GetGamepadActionButtons()");

            return GetActionButtons(GamepadActionBarControllers.Cast<ActionBarController>().ToList());
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

        public void ClearActionBars(bool clearSavedUseables = false) {
            //Debug.Log($"ActionBarManager.ClearActionBars({clearSavedUseables})");

            foreach (ActionBarController actionBarController in actionBarControllers) {
                //Debug.Log("ActionBarManager.AddNewAbility(): looping through a controller");
                actionBarController.ClearActionBar(clearSavedUseables);
            }
            foreach (ActionBarController actionBarController in gamepadActionBarControllers) {
                //Debug.Log("ActionBarManager.AddNewAbility(): looping through a controller");
                actionBarController.ClearActionBar(clearSavedUseables);
            }
        }

        public void SetGamepadActionButtonSet(int actionButtonSet, bool updateVisuals = true) {
            //Debug.Log($"ActionBarmanager.SetGamepadActionButtonSet({actionButtonSet}, {updateVisuals})");

            currentActionBarSet = actionButtonSet;
            gamepadPanel.SetGamepadActionButtonSet(actionButtonSet);
            UpdateGamepadActionButtons(actionButtonSet, GetGamepadActionButtons());
            if (updateVisuals) {
                UpdateVisuals();
            }
        }

        private void UpdateGamepadActionButtons(int actionButtonSet, List<ActionButton> actionButtons) {
            //Debug.Log($"ActionBarmanager.UpdateGamepadActionButtons({actionButtonSet})");

            for (int i = 0; i < 16; i++) {
                //Debug.Log("ActionBarmanager.UpdateGamepadActionButtons(" + actionButtonSet + ") checking: " + (i + (actionButtonSet * 16)));
                if (gamepadActionButtons[i + (actionButtonSet * 16)].Useable == null) {
                    actionButtons[i].ClearUseable();
                } else {
                    actionButtons[i].SetUseable(gamepadActionButtons[i + (actionButtonSet * 16)].Useable, false);
                }
            }
        }

        public void UpdateVisuals() {
            //Debug.Log("ActionBarmanager.UpdateVisuals()");

            if (controlsManager.GamePadModeActive == true) {
                foreach (ActionBarController actionBarController in gamepadActionBarControllers) {
                    //Debug.Log("ActionBarManager.AddNewAbility(): looping through a controller");
                    actionBarController.UpdateVisuals();
                }
            } else {
                foreach (ActionBarController actionBarController in actionBarControllers) {
                    //Debug.Log("ActionBarManager.AddNewAbility(): looping through a controller");
                    actionBarController.UpdateVisuals();
                }
            }
        }

        /*
        private void RemoveStaleGamepadActions() {

            foreach (ActionButtonNode actionButtonNode in gamepadActionButtons) {
                if (actionButtonNode.Useable != null && actionButtonNode.Useable.IsUseableStale(playerManager.UnitController)) {
                    actionButtonNode.SavedUseable = actionButtonNode.Useable;
                    actionButtonNode.Useable = null;
                }
            }
            UpdateGamepadActionButtons(currentActionBarSet, GetGamepadActionButtons());
        }

        public void RemoveStaleActions() {
            //Debug.Log("ActionBarManager.RemoveStaleActions()");

            RemoveStaleGamepadActions();
            foreach (ActionBarController actionBarController in actionBarControllers) {
                //Debug.Log("ActionBarManager.AddNewAbility(): looping through a controller");
                actionBarController.RemoveStaleActions();
            }
        }
        */

        private void HandleAddEquipment(EquipmentSlotProfile profile, InstantiatedEquipment equipment) {
            UpdateVisuals();
        }

        private void HandleRemoveEquipment(EquipmentSlotProfile profile, InstantiatedEquipment equipment) {
            UpdateVisuals();
        }

        private void ActivateGamepadActionBars() {
            
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

            // show gamepad
            uIManager.GamepadWindow.OpenWindow();
        }

        private void ActivateMouseActionBars() {
            
            // hide gamepad
            uIManager.GamepadWindow.CloseWindow();

            // show systembar
            if (PlayerPrefs.GetInt("UseSystemBar") == 0) {
                if (systemBarController.gameObject.activeSelf == true) {
                    systemBarController.gameObject.SetActive(false);
                }
            } else if (PlayerPrefs.GetInt("UseSystemBar") == 1) {
                if (systemBarController.gameObject.activeSelf == false) {
                    systemBarController.gameObject.SetActive(true);
                }
            }

            // activate action bar controller 1
            if (PlayerPrefs.GetInt("UseActionBar1") == 0) {
                if (actionBarControllers[0].gameObject.activeSelf == true) {
                    actionBarControllers[0].gameObject.SetActive(false);
                }
            } else if (PlayerPrefs.GetInt("UseActionBar1") == 1) {
                if (actionBarControllers[0].gameObject.activeSelf == false) {
                    actionBarControllers[0].gameObject.SetActive(true);
                }
            }

            // activate action bar controller 2
            if (PlayerPrefs.GetInt("UseActionBar2") == 0) {
                if (actionBarControllers[1].gameObject.activeSelf == true) {
                    actionBarControllers[1].gameObject.SetActive(false);
                }
            } else if (PlayerPrefs.GetInt("UseActionBar2") == 1) {
                if (actionBarControllers[1].gameObject.activeSelf == false) {
                    actionBarControllers[1].gameObject.SetActive(true);
                }
            }

            // activate action bar controller 3
            if (PlayerPrefs.GetInt("UseActionBar3") == 0) {
                if (actionBarControllers[2].gameObject.activeSelf) {
                    actionBarControllers[2].gameObject.SetActive(false);
                }
            } else if (PlayerPrefs.GetInt("UseActionBar3") == 1) {
                if (!actionBarControllers[2].gameObject.activeSelf) {
                    actionBarControllers[2].gameObject.SetActive(true);
                }
            }

            // activate action bar controller 4
            if (PlayerPrefs.GetInt("UseActionBar4") == 0) {
                if (actionBarControllers[3].gameObject.activeSelf) {
                    actionBarControllers[3].gameObject.SetActive(false);
                }
            } else if (PlayerPrefs.GetInt("UseActionBar4") == 1) {
                if (!actionBarControllers[3].gameObject.activeSelf) {
                    actionBarControllers[3].gameObject.SetActive(true);
                }
            }

            // activate action bar controller 5
            if (PlayerPrefs.GetInt("UseActionBar5") == 0) {
                if (actionBarControllers[4].gameObject.activeSelf) {
                    actionBarControllers[4].gameObject.SetActive(false);
                }
            } else if (PlayerPrefs.GetInt("UseActionBar5") == 1) {
                if (!actionBarControllers[4].gameObject.activeSelf) {
                    actionBarControllers[4].gameObject.SetActive(true);
                }
            }

            // activate action bar controller 6
            if (PlayerPrefs.GetInt("UseActionBar6") == 0) {
                if (actionBarControllers[5].gameObject.activeSelf) {
                    actionBarControllers[5].gameObject.SetActive(false);
                }
            } else if (PlayerPrefs.GetInt("UseActionBar6") == 1) {
                if (!actionBarControllers[5].gameObject.activeSelf) {
                    actionBarControllers[5].gameObject.SetActive(true);
                }
            }

            // activate action bar controller 7
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

        public void ActivateCorrectActionBars() {
            //Debug.Log("ActionBarmanager.ActivateCorrectActionBars()");

            if (controlsManager.GamePadModeActive == true) {
                ActivateGamepadActionBars();
            } else {
                ActivateMouseActionBars();
            }

            UpdateVisuals();

        }

    }

}