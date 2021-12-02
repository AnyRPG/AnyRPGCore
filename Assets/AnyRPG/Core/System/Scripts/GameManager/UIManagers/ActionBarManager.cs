using AnyRPG;
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
        protected List<ActionButtonNode> gamepadActionButtons = new List<ActionButtonNode>();

        private bool abilityBarsPopulated = false;

        private ActionButton fromButton = null;

        protected bool eventSubscriptionsInitialized = false;

        private Coroutine targetRangeRoutine = null;

        protected IUseable assigningUseable = null;

        // the action bar target for range checks
        private Interactable target = null;

        protected PlayerManager playerManager = null;
        protected KeyBindManager keyBindManager = null;
        protected SystemEventManager systemEventManager = null;
        protected ControlsManager controlsManager = null;
        protected InputManager inputManager = null;
        protected UIManager uIManager = null;


        public ActionButton FromButton { get => fromButton; set => fromButton = value; }
        public List<ActionBarController> ActionBarControllers { get => actionBarControllers; set => actionBarControllers = value; }
        public List<GamepadActionBarController> GamepadActionBarControllers { get => gamepadActionBarControllers; set => gamepadActionBarControllers = value; }
        public SystemBarController SystemBarController { get => systemBarController; }
        public IUseable AssigningUseable { get => assigningUseable; }
        public List<ActionButtonNode> GamepadActionButtons { get => gamepadActionButtons; }
        public int CurrentActionBarSet { get => currentActionBarSet; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            playerManager = systemGameManager.PlayerManager;
            keyBindManager = systemGameManager.KeyBindManager;
            systemEventManager = systemGameManager.SystemEventManager;
            controlsManager = systemGameManager.ControlsManager;
            inputManager = systemGameManager.InputManager;
            uIManager = systemGameManager.UIManager;

            systemBarController.Configure(systemGameManager);

            InitializeGamepadActionButtonNodes();
            InitializeActionbars();

            AssociateActionBarKeyBinds();
            CreateEventSubscriptions();

        }

        private void InitializeGamepadActionButtonNodes() {
            //Debug.Log("ActionBarManager.InitializeGamepadActionButtonNodes()");
            for (int i = 0; i < gamepadActionButtonCount; i++) {
                gamepadActionButtons.Add(new ActionButtonNode());
            }
        }

        private void ClearGamepadActionButtonNodes() {
            //Debug.Log("ActionBarManager.ClearGamepadActionButtonNodes()");
            for (int i = 0; i < gamepadActionButtonCount; i++) {
                gamepadActionButtons[i] = new ActionButtonNode();
            }
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

        public void StartUseableAssignment(IUseable useable, int moveIndex = -1) {
            this.moveIndex = moveIndex;
            assigningUseable = useable;
        }

        public void ClearUseableAssignment() {
            assigningUseable = null;
        }

        public void AssignUseableByIndex(int index) {
            //Debug.Log("ActionBarManager.AssignUseableByIndex(" + index + ")");
            int controllerIndex = Mathf.FloorToInt((float)index / 8f);
            int buttonIndex = index % 8;

            IUseable oldUseable = null;
            if (moveIndex > -1) {
                oldUseable = gamepadActionButtons[(currentActionBarSet * 16) + index].Useable;
            }

            gamepadActionButtons[(currentActionBarSet * 16) + index].Useable = assigningUseable;
            gamepadActionBarControllers[controllerIndex].ActionButtons[buttonIndex].SetUseable(assigningUseable);

            if (moveIndex > -1) {
                if (oldUseable == null) {
                    // the spot where the useable was placed was empty, clear the original slot
                    ClearUseableByIndex(moveIndex);
                } else {
                    // the spot where the useable was placed was not empty, put the replaced useable in the old position (swap)
                    controllerIndex = Mathf.FloorToInt((float)moveIndex / 8f);
                    buttonIndex = moveIndex % 8;
                    gamepadActionButtons[(currentActionBarSet * 16) + moveIndex].Useable = oldUseable;
                    gamepadActionBarControllers[controllerIndex].ActionButtons[buttonIndex].SetUseable(oldUseable);
                }

            }
        }

        public void ClearUseableByIndex(int index) {
            //Debug.Log("ActionBarManager.AssignUseableByIndex(" + index + ")");
            int controllerIndex = Mathf.FloorToInt((float)index / 8f);
            int buttonIndex = index % 8;
            gamepadActionButtons[(currentActionBarSet * 16) + index].Useable = null;
            gamepadActionBarControllers[controllerIndex].ActionButtons[buttonIndex].ClearUseable(); ;
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

            if (inputManager.KeyBindWasPressed("JOYSTICKBUTTON4")) {
                // LB
                if (currentActionBarSet > 0) {
                    SetGamepadActionButtonSet(currentActionBarSet -1);
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
                actionButton.RangeIndicator.color = hiddenColor;
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
                            if (actionButton.RangeIndicator.color != hiddenColor) {
                                actionButton.RangeIndicator.color = hiddenColor;
                            }
                        } else {
                            /*
                            if (actionButton.KeyBindText.color != Color.red) {
                                actionButton.KeyBindText.color = Color.red;
                            }
                            */
                            if (actionButton.RangeIndicator.color != Color.red && actionButton.Useable != null) {
                                actionButton.RangeIndicator.color = Color.red;
                            }
                        }
                    } else {
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
                    }
                }
                yield return null;
            }

            targetRangeRoutine = null;
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
            return actionButtons;
        }

        public List<ActionButton> GetMouseActionButtons() {
            //Debug.Log("ActionBarManager.GetActionButtons()");
            
            return GetActionButtons(actionBarControllers);
        }

        public List<ActionButton> GetGamepadActionButtons() {
            //Debug.Log("ActionBarManager.GetActionButtons()");

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

        public bool AddGamepadSavedAbility(BaseAbility newAbility) {
            //Debug.Log("AbilityBarController.AddNewAbility(" + newAbility + ")");
            for (int i = 0; i < 16; i++) {
                if (gamepadActionButtons[i + (currentActionBarSet * 16)].Useable == null
                    && gamepadActionButtons[i + (currentActionBarSet * 16)].SavedUseable != null
                    && gamepadActionButtons[i + (currentActionBarSet * 16)].SavedUseable.DisplayName == newAbility.DisplayName) {
                    //Debug.Log("Adding ability: " + newAbility + " to empty action button " + i);
                    //gamepadActionButtons[i + (currentActionBarSet * 16)].SetUseable(newAbility);
                    assigningUseable = newAbility;
                    AssignUseableByIndex(i);
                    return true;
                } else if (gamepadActionButtons[i + (currentActionBarSet * 16)].Useable == (newAbility as IUseable)) {
                    //Debug.Log("Ability exists on bars already!");
                    return true;
                }
            }
            return false;
        }

        public bool AddGamepadNewAbility(BaseAbility newAbility) {
            //Debug.Log("AbilityBarController.AddNewAbility(" + newAbility + ")");
            for (int i = 0; i < 16; i++) {
                if (gamepadActionButtons[i + (currentActionBarSet * 16)].Useable == null) {
                    //Debug.Log("Adding ability: " + newAbility + " to empty action button " + i);
                    //gamepadActionButtons[i + (currentActionBarSet * 16)].SetUseable(newAbility);
                    assigningUseable = newAbility;
                    AssignUseableByIndex(i);
                    return true;
                } else if (gamepadActionButtons[i + (currentActionBarSet * 16)].Useable == (newAbility as IUseable)) {
                    //Debug.Log("Ability exists on bars already!");
                    return true;
                }
            }
            return false;
        }

        public bool AddNewAbility(BaseAbility newAbility) {
            //Debug.Log("ActionBarManager.AddNewAbility()");
            bool returnValue = false;
            bool foundSlot = false;
            if (AddGamepadSavedAbility(newAbility)) {
                if (controlsManager.GamePadModeActive) {
                    returnValue = true;
                }
                foundSlot = true;
            }
            /*
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
            */
            if (foundSlot != true) {
                if (AddGamepadNewAbility(newAbility)) {
                    if (controlsManager.GamePadModeActive) {
                        returnValue = true;
                    }
                }
                /*
                foreach (ActionBarController actionBarController in gamepadActionBarControllers) {
                    if (actionBarController.AddNewAbility(newAbility)) {
                        //Debug.Log("ActionBarManager.AddNewAbility(): we were able to add " + newAbility.name);
                        if (controlsManager.GamePadModeActive) {
                            returnValue = true;
                        }
                        break;
                    }
                }
                */
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
            ClearGamepadActionButtonNodes();

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

        public void SetGamepadActionButtonSet(int actionButtonSet, bool updateVisuals = true) {
            //Debug.Log("ActionBarmanager.SetGamepadActionButtonSet(" + actionButtonSet + ")");
            currentActionBarSet = actionButtonSet;
            gamepadPanel.SetGamepadActionButtonSet(actionButtonSet);
            UpdateGamepadActionButtons(actionButtonSet, GetGamepadActionButtons());
            if (updateVisuals) {
                UpdateVisuals();
            }
        }

        private void UpdateGamepadActionButtons(int actionButtonSet, List<ActionButton> actionButtons) {
            //Debug.Log("ActionBarmanager.UpdateGamepadActionButtons(" + actionButtonSet + ")");
            for (int i = 0; i < 16; i++) {
                //Debug.Log("ActionBarmanager.UpdateGamepadActionButtons(" + actionButtonSet + ") checking: " + (i + (actionButtonSet * 16)));
                if (gamepadActionButtons[i + (actionButtonSet * 16)].Useable == null) {
                    actionButtons[i].ClearUseable();
                } else {
                    actionButtons[i].SetUseable(gamepadActionButtons[i + (actionButtonSet * 16)].Useable, false);
                }
            }
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
            //Debug.Log("ActionBarmanager.UpdateActionBars()");

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

                // show gamepad
                //gamepadPanel.ShowGamepad();
                //gamepadPanel.gameObject.SetActive(true);

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

                // hide gamepad
                //gamepadPanel.HideGamepad();
                //gamepadPanel.gameObject.SetActive(false);

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