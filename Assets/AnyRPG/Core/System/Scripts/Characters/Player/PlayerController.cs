using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;

namespace AnyRPG {
    public class PlayerController : ConfiguredClass {

        public event System.Action<int> AbilityButtonPressedHandler = delegate { };
        public event System.Action<bool> ToggleRunHandler = delegate { };

        private MovementData movementData = new MovementData();

        //Variables
        [HideInInspector]
        public bool allowedInput = true;

        [HideInInspector]
        public bool autorunActive = false;

        [HideInInspector]
        public bool mouseLookActive = false;

        [HideInInspector]
        public bool strafeModeActive = true;

        private List<Interactable> interactables = new List<Interactable>();
        private Interactable mouseOverInteractable = null;

        //private int tabTargetIndex = 0;

        private int crossBarIndex = 0;

        private DateTime lastTabTargetTime;

        private RaycastHit mouseOverhit;

        // game manager references
        protected InputManager inputManager = null;
        protected PlayerManagerClient playerManagerClient = null;
        protected MessageFeedManager messageFeedManager = null;
        protected NamePlateManager namePlateManager = null;
        protected CameraManager cameraManager = null;
        protected KeyBindManager keyBindManager = null;
        protected CraftingManager craftingManager = null;
        protected UIManager uIManager = null;
        protected WindowManager windowManager = null;
        protected ControlsManager controlsManager = null;
        protected ActionBarManager actionBarManager = null;
        protected CastTargettingManager castTargettingManager = null;
        protected SaveManager saveManager = null;
        protected InteractionManagerClient interactionManagerClient = null;
        protected ContextMenuService contextMenuService = null;
        protected SystemEventManager systemEventManager = null;

        public List<Interactable> Interactables { get => interactables; }
        public RaycastHit MouseOverhit { get => mouseOverhit; set => mouseOverhit = value; }
        public MovementData MovementData { get => movementData; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            inputManager = systemGameManager.InputManager;
            playerManagerClient = systemGameManager.PlayerManagerClient;
            messageFeedManager = systemGameManager.UIManager.MessageFeedManager;
            namePlateManager = systemGameManager.UIManager.NamePlateManager;
            cameraManager = systemGameManager.CameraManager;
            keyBindManager = systemGameManager.KeyBindManager;
            craftingManager = systemGameManager.CraftingManager;
            uIManager = systemGameManager.UIManager;
            windowManager = systemGameManager.WindowManager;
            controlsManager = systemGameManager.ControlsManager;
            actionBarManager = uIManager.ActionBarManager;
            castTargettingManager = systemGameManager.CastTargettingManager;
            saveManager = systemGameManager.SaveManager;
            interactionManagerClient = systemGameManager.InteractionManagerClient;
            contextMenuService = systemGameManager.ContextMenuService;
            systemEventManager = systemGameManager.SystemEventManager;
        }

        public void AddInteractable(Interactable interactable) {
            //Debug.Log($"PlayerController.AddInteractable({interactable.gameObject.name})");

            if (interactables.Contains(interactable) == false) {
                interactables.Add(interactable);
            }

            ShowHideInteractionPopup();
        }

        /// <summary>
        /// Remove an interactable from the list of interactables in range
        /// </summary>
        /// <param name="_interactable"></param>
        public void RemoveInteractable(Interactable interactable) {
            //Debug.Log($"PlayerController.RemoveInteractable({interactable.gameObject.name})");

            if (interactables.Contains(interactable)) {
                interactables.Remove(interactable);
            }
            ShowHideInteractionPopup();
        }

        public void ShowHideInteractionPopup() {
            //Debug.Log("PlayerController.ShowHideInteractionPopup() count: " + interactables.Count);

            if (interactables.Count > 0
                && interactables[interactables.Count - 1].GetCurrentInteractables(playerManagerClient.UnitController).Count > 0) {
                uIManager.ShowInteractionTooltip(interactables[interactables.Count - 1]);
            } else {
                uIManager.HideInteractionToolTip();
            }
        }

        protected void OnEnable() {
            // put this in player spawn
            allowedInput = true;
            lastTabTargetTime = DateTime.Now;

            // moved from Start(). monitor for breakage
            // testing : disabled since this would only do something if the actual keybind was pressed, which it could not have been before Start() anyway
            // run by default
            //ToggleRun();
        }

        public void ProcessLevelUnload() {
            //Debug.Log($"{gameObject.name}.PlayerController.ProcessLevelUnload()");
            ClearInteractables();
        }

        public void ClearInteractables() {
            //Debug.Log($"{gameObject.name}.PlayerController.ClearInteractables()");
            interactables.Clear();
        }

        private void CollectMoveInput() {
            //Debug.Log("PlayerController.CollectMoveInput()");
            movementData.ResetMoveInput();

            if (allowedInput == false) {
                //Debug.Log("Not allowed to Collect Move Input. Exiting PlayerController.CollectMoveInput()");
                return;
            }
            
            if (windowManager.CurrentWindow == null) {
                movementData.RightAnalogHorizontal = Input.GetAxis("RightAnalogHorizontal");
            }

            movementData.CameraWantedDirection = cameraManager.MainCameraController.WantedDirection;
            movementData.CameraLocalEulerAngleX = cameraManager.MainCamera.transform.localEulerAngles.x;

            //movementData.GamepadModeActive = controlsManager.GamepadModeActive;
            if (strafeModeActive == false
                && cameraManager.MainCameraController.FirstPersonView == false) {
                //Debug.Log("PlayerController.CollectMoveInput() setting RotateModelMode to true because strafe mode is off and we are not in first person view");
                movementData.RotateModelMode = true;
            } else {
                //Debug.Log("PlayerController.CollectMoveInput() setting RotateModelMode to false because strafe mode is on or we are in first person view");
                movementData.RotateModelMode = false;
            }

            // don't allow jump or crouch while activating action bars
            if (controlsManager.LeftTriggerDown == false && controlsManager.RightTriggerDown == false) {
                if (inputManager.KeyBindWasPressed("JUMP")) {
                    movementData.InputJump = true;
                }
                if (inputManager.KeyBindWasPressedOrHeld("JUMP")) {
                    movementData.InputFly = true;
                }
                if (inputManager.KeyBindWasPressedOrHeld("CROUCH")) {
                    movementData.InputSink = true;
                }
                if (inputManager.KeyBindWasPressed("CROUCH")) {
                    movementData.InputCrouch = true;
                }
            }

            movementData.InputStrafe = inputManager.KeyBindWasPressedOrHeld("STRAFELEFT") || inputManager.KeyBindWasPressedOrHeld("STRAFERIGHT");

            // gather joystick move input
            movementData.InputHorizontal = Input.GetAxis("LeftAnalogHorizontal");
            movementData.InputVertical = Input.GetAxis("LeftAnalogVertical");

            // gather keyboard move input
            movementData.InputHorizontal += (inputManager.KeyBindWasPressedOrHeld("STRAFELEFT") ? -1 : 0) + (inputManager.KeyBindWasPressedOrHeld("STRAFERIGHT") ? 1 : 0);
            movementData.InputVertical += (inputManager.KeyBindWasPressedOrHeld("BACK") ? -1 : 0) + (inputManager.KeyBindWasPressedOrHeld("FORWARD") ? 1 : 0);

            // gather keyboard turn input
            movementData.InputTurn += (inputManager.KeyBindWasPressedOrHeld("TURNLEFT") ? -1 : 0) + (inputManager.KeyBindWasPressedOrHeld("TURNRIGHT") ? 1 : 0);

            // turn off autorun if there is any movement input
            if (autorunActive
                && ((movementData.InputHorizontal != 0f) || (movementData.InputVertical != 0f) || movementData.InputJump || movementData.InputFly || movementData.InputSink || movementData.InputStrafe || movementData.InputCrouch)) {
                ToggleAutorun();
            }

            if (autorunActive) {
                movementData.InputVertical = 1;
            }

            movementData.NormalizedMoveInput = NormalizedVelocity(new Vector3(movementData.InputHorizontal, 0, movementData.InputVertical));
            movementData.TurnInput = new Vector3(movementData.InputTurn, 0, 0);

            if (inputManager.rightMouseButtonDown
                && (inputManager.rightMouseButtonClickedOverUI == false || (namePlateManager != null ? namePlateManager.MouseOverNamePlate() : false))) {
                movementData.RightMouseButtonDown = true;
                // we will pretend the right mouse was dragged if we have move input so the character will run away from the screen if the camera was pointing
                // behind them at the start of the right mouse down, which is a common situation when trying to run away from something attacking you.
                // Otherwise, the player would have to drag the mouse in a direction before the character would start moving, which could be frustrating in a combat situation.
                if (inputManager.rightMouseButtonDownPosition != Input.mousePosition || movementData.HasMoveInput()) {
                    if (movementData.RotateModelMode == false
                        || cameraManager.MainCameraController.FirstPersonView == true) {
                        movementData.FaceCameraDirection = true;
                    }
                }
            }

            if (cameraManager.MainCameraController.FirstPersonView == true
                && inputManager.leftMouseButtonDown
                && (inputManager.leftMouseButtonClickedOverUI == false || (namePlateManager != null ? namePlateManager.MouseOverNamePlate() : false))
                && (inputManager.leftMouseButtonDownPosition != Input.mousePosition || movementData.HasMoveInput())) {
                movementData.FaceCameraDirection = true;
            }

            // if we are in first person view, we want to face the camera direction anytime we have move input, even if the mouse is not being used, because the player will expect that pushing forward will move them in the direction they are looking, and pushing back will move them backwards relative to the direction they are looking, etc.  This is a common behavior in first person games, and not having it would be frustrating.
            if (cameraManager.MainCameraController.FirstPersonView == true && movementData.HasMoveInput() && movementData.HasTurnInput() == false) {
                movementData.FaceCameraDirection = true;
            }

            if (mouseLookActive
                && (movementData.RotateModelMode == false || cameraManager.MainCameraController.FirstPersonView == true)
                && (Input.GetAxis("Mouse X") != 0f || Input.GetAxis("Mouse Y") != 0f)) { 
                movementData.FaceCameraDirection = true;
            }

            if (movementData.HasAnyInput()) {
                // turn off the projector, so it has to be done client side
                playerManagerClient.ActiveUnitController.CommonMovementNotifier();
            }

            playerManagerClient.ActiveUnitController.UnitMovementController.AddMovementData(movementData);
        }

        public void ProcessInput() {
            //Debug.Log("PlayerController.ProcessInput()");
            //ResetMoveInput();

            if (playerManagerClient.ActiveUnitController == null) {
                //Debug.Log($"{gameObject.name}.PlayerController.ProcessInput(): Player Unit is not spawned. Exiting");
                return;
            }

            if (playerManagerClient.ActiveUnitController.CameraTargetReady == false) {
                //Debug.Log($"{gameObject.name}.PlayerController.ProcessInput(): Camera Target is not ready. Exiting");
                return;
            }

            if (allowedInput == false) {
                //Debug.Log("Not allowed to Collect Move Input. Exiting PlayerController ProcessInput!");
                return;
            }

            HandleCancelButtonPressed();

            HandleMouseOver();

            if (playerManagerClient.ActiveUnitController?.CharacterStats.IsAlive == false) {
                // can't interact, perform abilities or handle movement when dead
                return;
            }

            // test move this below death check to prevent player getting up after death
            ToggleRun();

            CheckToggleStrafe();
            CheckToggleAutorun();
            CheckToggleMouseLook();

            HandleLeftMouseClick();

            RegisterTab();

            // everything below this point cannot be done while control locked
            if (playerManagerClient.ActiveUnitController.ControlLocked == true) {
                return;
            }

            if (systemConfigurationManager.AllowFreeMove == true) {
                CollectMoveInput();
            }

            HandleRightMouseClick();

            ProcessGamepadButtonClicks();

            RegisterAbilityButtonPresses();
        }

        private void CheckToggleStrafe() {
            //Debug.Log("PlayerController.CheckToggleStrafe()");

            if (inputManager.KeyBindWasPressed("TOGGLESTRAFE") && playerManagerClient.ActiveUnitController.UnitProfile.UnitPrefabProps.ForceRotateModelMode == false) {
                ToggleStrafe();
            }
        }

        private void ToggleStrafe() {
            strafeModeActive = !strafeModeActive;
            messageFeedManager.WriteMessage($"Strafe Mode: {(strafeModeActive ? "On" : "Off")}");
        }

        private void CheckToggleMouseLook() {
            if (inputManager.KeyBindWasPressed("TOGGLEMOUSELOOK")) {
                ToggleMouseLook();
            }
        }

        private void ToggleMouseLook() {
            mouseLookActive = !mouseLookActive;
            messageFeedManager.WriteMessage($"Mouse Look: {(mouseLookActive ? "On" : "Off")}");
        }

        private Vector3 NormalizedVelocity(Vector3 inputVelocity) {
            if (inputVelocity.magnitude > 1) {
                inputVelocity.Normalize();
            }
            return inputVelocity;
        }

        private void ToggleRun() {
            //Debug.Log("PlayerController.ToggleRun()");
            if (inputManager.KeyBindWasPressed("TOGGLERUN")
                || (controlsManager.DPadDownPressed == true && controlsManager.LeftTriggerDown == false && controlsManager.RightTriggerDown == false)) {
                EventParamProperties eventParamProperties = new EventParamProperties();
                if (playerManagerClient.ActiveUnitController.Walking == false) {
                    playerManagerClient.ActiveUnitController.Walking = true;
                    eventParamProperties.simpleParams.BoolParam = true;
                } else {
                    playerManagerClient.ActiveUnitController.Walking = false;
                    eventParamProperties.simpleParams.BoolParam = false;
                }
                SystemEventManager.TriggerEvent("OnToggleRun", eventParamProperties);
                messageFeedManager.WriteMessage("Walk: " + playerManagerClient.ActiveUnitController.Walking.ToString());
                ToggleRunHandler(playerManagerClient.ActiveUnitController.Walking);
            }
        }

        private void CheckToggleAutorun() {
            if (inputManager.KeyBindWasPressed("TOGGLEAUTORUN")
                || inputManager.KeyBindWasPressed("JOYSTICKBUTTON8") == true) {
                ToggleAutorun();
            }
        }

        private void ToggleAutorun() {
            EventParamProperties eventParamProperties = new EventParamProperties();
            if (autorunActive == false) {
                autorunActive = true;
                eventParamProperties.simpleParams.BoolParam = true;
            } else {
                autorunActive = false;
                eventParamProperties.simpleParams.BoolParam = false;
            }
            SystemEventManager.TriggerEvent("OnToggleAutorun", eventParamProperties);
            messageFeedManager.WriteMessage("Autorun: " + autorunActive.ToString());
        }

        private bool MouseOutsideScreen() {
            if (Input.mousePosition.x < 0f
                            || Input.mousePosition.x > Screen.width
                            || Input.mousePosition.y < 0f
                            || Input.mousePosition.y > Screen.height) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// this code is necessary because the only other solution to mouseover through the player is to set the player to layer Ignore Raycast
        /// which breaks the invector controller
        /// </summary>
        private void HandleMouseOver() {
            //Debug.Log($"{gameObject.name}.PlayerController.HandleMouseOver()");
            if (cameraManager.ActiveMainCamera == null) {
                // we are in a cutscene and shouldn't be dealing with mouseover
                return;
            }

            // don't do anything if the mouse is outside the screen bounds
            if (MouseOutsideScreen()) {
                DisableMouseOver();
                return;
            }

            // gamepad mode can hide the cursor.  Mouseover should not be activated when the cursor is hidden
            if (controlsManager.MouseDisabled == true) {
                return;
            }

            Ray ray = cameraManager.ActiveMainCamera.ScreenPointToRay(Input.mousePosition);
            int playerMask = 1 << LayerMask.NameToLayer("Player");
            int ignoreMask = 1 << LayerMask.NameToLayer("Ignore Raycast");
            int spellMask = 1 << LayerMask.NameToLayer("SpellEffects");
            int waterMask = 1 << LayerMask.NameToLayer("Water");
            int layerMask = ~(playerMask | ignoreMask | spellMask | waterMask);
            //int layerMask = ~( ignoreMask | spellMask | waterMask);

            bool disableMouseOver = false;
            bool mouseOverNamePlate = false;
            mouseOverNamePlate = namePlateManager.MouseOverNamePlate();

            if (!EventSystem.current.IsPointerOverGameObject() && !mouseOverNamePlate) {
                if (Physics.Raycast(ray, out mouseOverhit, 100, layerMask)) {
                    // prevent clicking on mount
                    if (mouseOverhit.collider.gameObject != playerManagerClient.ActiveUnitController.gameObject
                        && mouseOverhit.collider.gameObject != playerManagerClient.UnitController.gameObject) {
                        Interactable newInteractable = mouseOverhit.collider.GetComponent<Interactable>();
                        if (newInteractable == null) {
                            newInteractable = mouseOverhit.collider.GetComponentInParent<Interactable>();
                        }

                        if (mouseOverInteractable != null && mouseOverInteractable != newInteractable) {
                            // since we hit something, and our existing thing was not null, we have to exit the old one
                            mouseOverInteractable.IsMouseOverUnit = false;
                            mouseOverInteractable.OnMouseOut();
                        }

                        if (newInteractable != null && mouseOverInteractable != newInteractable) {
                            // we have a new interactable, activate mouseover
                            newInteractable.IsMouseOverUnit = true;
                            newInteractable.OnMouseIn();
                        }
                        mouseOverInteractable = newInteractable;
                    }
                }
            } else {
                disableMouseOver = true;
                //Debug.Log($"{gameObject.name}.PlayerController.HandleMouseOver(): mouseovernameplate: " + namePlateManager.MouseOverNamePlate() + "; pointerovergameobject: " + EventSystem.current.IsPointerOverGameObject());
            }

            if (disableMouseOver) {
                // we did not hit any interactable, check if a current interactable is set and unset it
                DisableMouseOver();
            }

        }

        public void DisableMouseOver() {
            if (mouseOverInteractable != null) {
                mouseOverInteractable.IsMouseOverUnit = false;
                mouseOverInteractable.OnMouseOut();
                mouseOverInteractable = null;
            }
        }

        /*
        public void HandleMouseOver(Interactable newInteractable) {
            //Debug.Log($"{gameObject.name}.PlayerController.HandleMouseOver()");
            if (cameraManager.MyActiveMainCamera == null) {
                // we are in a cutscene and shouldn't be dealing with mouseover
                return;
            }

            mouseOverInteractable = newInteractable;
        }

        public void HandleMouseOut(Interactable oldInteractable) {

            if (mouseOverInteractable == oldInteractable) {
                mouseOverInteractable = null;
            }
        }
        */

        private void HandleRightMouseClick() {
            //Debug.Log($"{gameObject.name}.PlayerController.HandleRightMouseClick()");

            if (MouseOutsideScreen()) {
                return;
            }

            // Check if the left mouse button clicked on an interactable and focus it
            if (!inputManager.rightMouseButtonClicked) {
                return;
            }

            //Debug.Log($"{gameObject.name}.PlayerController.HandleRightMouseClick() mouse is in screen");

            // check if the right mouse button clicked on something and interact with it
            if (EventSystem.current.IsPointerOverGameObject() == false) {
                //Debug.Log($"{gameObject.name}.PlayerController.HandleRightMouseClick() right mouse button clicked and not over UI");
                contextMenuService.CloseContextMenu();

                if (mouseOverInteractable != null && mouseOverInteractable.IsTrigger == false) {
                    //Debug.Log("setting interaction target to " + hit.collider.gameObject.name);
                    //interactionTarget = hit.collider.gameObject;
                    RightMouseInteraction(mouseOverInteractable.CharacterTarget);
                }
                //Debug.Log("We hit " + hit.collider.name + " " + hit.point);
            }
        }

        public void RightMouseInteraction(Interactable interactable) {

            if (interactable.IsMouseOverBlocked() == true) {
                //Debug.Log("PlayerController.InterActWithTarget(): mouseover blocked");
                return;
            }

            if (playerManagerClient.UnitController.Target == null || playerManagerClient.UnitController.Target != interactable) {
                playerManagerClient.UnitController.ClearTarget();
                playerManagerClient.UnitController.SetTarget(interactable);
            }

            Dictionary<int, InteractableOptionComponent> inRangeInteractables = interactable.GetInRangeInteractables(playerManagerClient.UnitController);
            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.GetCurrentInteractables(playerManagerClient.UnitController);

            // there are no options to interact with
            if (currentInteractables.Count == 0) {
                //Debug.Log($"{gameObject.name}.InterActWithTarget({interactable.gameObject.name}) no interactables available");
                return;
            }

            // the player is trying to interact with something that is out of range, but there are interactables available, so we should move toward the target
            if (inRangeInteractables.Count == 0) {
                if (playerManagerClient.ActiveUnitController.UnitMovementController.useMeshNav && systemConfigurationManager.AllowClickToMove) {
                    //Debug.Log($"{gameObject.name}.InterActWithTarget({interactable.gameObject.name}) out of range, following target");
                    if (systemGameManager.GameMode == GameMode.Local) {
                        playerManagerClient.ActiveUnitController.UnitMotor.FollowInteractionTarget(interactable);
                    } else {
                        playerManagerClient.ActiveUnitController.UnitEventController.NotifyOnRequestFollowInteractionTarget(interactable);
                    }
                }
                return;
            }

            // There is already an interactable in range, and it is of type attack.
            // Attack interactions are always valid at any range, so we need to check if within attack range for the currently equipped weapon.
            // If not, move toward the target.
            if (inRangeInteractables.Count == 1 && inRangeInteractables.First().Value.InteractionType == InteractionType.Attack) {
                if (playerManagerClient.UnitController.CharacterAbilityManager.AutoAttackAbility != null) {
                    float attackRange = playerManagerClient.UnitController.CharacterAbilityManager.AutoAttackAbility.GetTargetOptions(playerManagerClient.UnitController).MaxRange;
                    float distanceToTarget = Vector3.Distance(playerManagerClient.ActiveUnitController.transform.position, interactable.transform.position);
                    if (distanceToTarget > attackRange) {
                        if (playerManagerClient.ActiveUnitController.UnitMovementController.useMeshNav && systemConfigurationManager.AllowClickToMove) {
                            //Debug.Log($"{gameObject.name}.InterActWithTarget({interactable.gameObject.name}) out of range for attack, following target");
                            if (systemGameManager.GameMode == GameMode.Local) {
                                playerManagerClient.ActiveUnitController.UnitMotor.FollowAttackTarget(interactable, attackRange);
                            } else {
                                playerManagerClient.ActiveUnitController.UnitEventController.NotifyOnRequestFollowAttackTarget(interactable, attackRange);
                            }

                        }
                        //return;
                    }
                }
            }

            // we are within range.  Go ahead and interact (or attack)
            InterActWithTarget(interactable, false);
        }

        private void ProcessGamepadButtonClicks() {
            //Debug.Log($"{gameObject.name}.PlayerController.ProcessGamepadButtonClicks()");

            // if a window is open, all button clicks will be processed by that window instead of the player
            if (windowManager.CurrentWindow != null) {
                return;
            }

            // determine which crossbar, if any, is active
            if (controlsManager.RightTriggerDown) {
                crossBarIndex = 1;
            } else if (controlsManager.LeftTriggerDown) {
                crossBarIndex = 0;
            } else {
                crossBarIndex = -1;
            }

            // if a crossbar is activated, send the input to it
            if (crossBarIndex > -1) {
                if (controlsManager.DPadDownPressed) {
                    actionBarManager.GamepadActionBarControllers[crossBarIndex].ActionButtons[0].OnClick(false);
                } else if (controlsManager.DPadRightPressed) {
                    actionBarManager.GamepadActionBarControllers[crossBarIndex].ActionButtons[1].OnClick(false);
                } else if (controlsManager.DPadLeftPressed) {
                    actionBarManager.GamepadActionBarControllers[crossBarIndex].ActionButtons[2].OnClick(false);
                } else if (controlsManager.DPadUpPressed) {
                    actionBarManager.GamepadActionBarControllers[crossBarIndex].ActionButtons[3].OnClick(false);
                } else if (inputManager.KeyBindWasPressed("JOYSTICKBUTTON0")) {
                    actionBarManager.GamepadActionBarControllers[crossBarIndex].ActionButtons[4].OnClick(false);
                } else if (inputManager.KeyBindWasPressed("JOYSTICKBUTTON1")) {
                    actionBarManager.GamepadActionBarControllers[crossBarIndex].ActionButtons[5].OnClick(false);
                } else if (inputManager.KeyBindWasPressed("JOYSTICKBUTTON2")) {
                    actionBarManager.GamepadActionBarControllers[crossBarIndex].ActionButtons[6].OnClick(false);
                } else if (inputManager.KeyBindWasPressed("JOYSTICKBUTTON3")) {
                    actionBarManager.GamepadActionBarControllers[crossBarIndex].ActionButtons[7].OnClick(false);
                }

                return;
            }

            // no crossbar was activated, buttons will perform their native functions
            if (inputManager.KeyBindWasPressed("ACCEPT")) {
                // accept button when targeting should just confirm target and nothing else
                if (playerManagerClient.ActiveUnitController?.CharacterAbilityManager.WaitingForTarget() == false) {

                    if (interactables.Count > 0) {
                        // range interactables are priority
                        InterActWithTarget(interactables[interactables.Count - 1]);
                    } else {
                        // allow friendly target when nothing is targeted
                        if (playerManagerClient.UnitController.Target == null) {
                            GetNextTabTarget(playerManagerClient.UnitController.Target, true, true);
                        } else {
                            InterActWithTarget(playerManagerClient.UnitController.Target);
                        }
                    }
                } else {
                    if (playerManagerClient.ActiveUnitController.CharacterAbilityManager.WaitingForTarget()) {
                        FinishGroundTarget(castTargettingManager.CastTargetController.VirtualCursor);
                    }
                }
            } else if (controlsManager.DPadRightPressed) {
                GetNextTabTarget(playerManagerClient.UnitController.Target, true, false);
            } else if (controlsManager.DPadLeftPressed) {
                GetNextTabTarget(playerManagerClient.UnitController.Target, true, false, false);
            }
        }

        private void FinishGroundTarget(Vector3 targetPosition) {
            Ray ray = cameraManager.ActiveMainCamera.ScreenPointToRay(targetPosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100, systemConfigurationManager.DefaultGroundMask)) {
                playerManagerClient.ActiveUnitController.CharacterAbilityManager.SetGroundTargetClient(hit.point);
            }
        }

        private void ClickToMove(Vector3 targetPosition) {
            //Debug.Log($"PlayerController.ClickToMove({targetPosition})");

            Ray ray = cameraManager.ActiveMainCamera.ScreenPointToRay(targetPosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100, systemConfigurationManager.DefaultGroundMask)) {
                uIManager.MovementTargetController.SetPosition(hit.point);
                if (systemGameManager.GameMode == GameMode.Local) {
                    playerManagerClient.ActiveUnitController.UnitMotor.ClickToMove(hit.point);
                } else {
                    playerManagerClient.ActiveUnitController.UnitEventController.NotifyOnRequestClickToMove(hit.point);
                }
            }
        }


        private void HandleLeftMouseClick() {
            //Debug.Log("PlayerController.HandleLeftMouseClick()");

            if (MouseOutsideScreen()) {
                return;
            }

            // Check if the left mouse button clicked on an interactable and focus it
            if (!inputManager.leftMouseButtonClicked) {
                return;
            }

            if (cameraManager.ActiveMainCamera == null) {
                // probably in a cutscene.  don't respond to clicks on objects if there is no camera following the player
                return;
            }

            if (EventSystem.current.IsPointerOverGameObject() && !namePlateManager.MouseOverNamePlate()) {
                //Debug.Log("PlayerController.HandleLeftMouseClick(): clicked over UI and not nameplate.  exiting");
                return;
            }

            contextMenuService.CloseContextMenu();

            if (mouseOverInteractable == null && !namePlateManager.MouseOverNamePlate()) {
                playerManagerClient.UnitController.ClearTarget();
            } else if (mouseOverInteractable != null && mouseOverInteractable.IsTrigger == false) {
                //Debug.Log($"PlayerController.HandleLeftMouseClick(): mouseover interactable: {mouseOverInteractable.gameObject.name}");
                if (mouseOverInteractable.IsMouseOverBlocked() == false) {
                    //Debug.Log("PlayerController.HandleLeftMouseClick(): mouseover not blocked");
                    playerManagerClient.UnitController.SetTarget(mouseOverInteractable.CharacterTarget);
                    return;
                }
            }

            if (namePlateManager.MouseOverNamePlate()) {
                return;
            }

            if (playerManagerClient.ActiveUnitController.CharacterAbilityManager.WaitingForTarget()) {
                FinishGroundTarget(Input.mousePosition);
            } else if (systemConfigurationManager.AllowClickToMove == true) {
                if (playerManagerClient?.ActiveUnitController != null && playerManagerClient.ActiveUnitController.ControlLocked == true) {
                    return;
                }
                ClickToMove(Input.mousePosition);
            }
        }

        private void RegisterTab() {

            // register keyboard tab target, only allow enemy target
            if (inputManager.KeyBindWasPressed("NEXTTARGET")) {
                //Debug.Log("Tab Target Registered");
                GetNextTabTarget(playerManagerClient.UnitController.Target, false, false);
            }
        }

        private bool ValidEnemyTarget(Interactable interactable) {
            UnitController targetCharacterUnit = interactable.GetComponent<UnitController>();
            if (targetCharacterUnit != null
                && targetCharacterUnit.CharacterStats.IsAlive == true
                && Faction.RelationWith(targetCharacterUnit, playerManagerClient.UnitController.BaseCharacter.Faction) <= -1) {
                return true;
            }
            return false;
        }

        private List<Interactable> GetTabTargets(Interactable oldTarget, bool includeFriendly, bool includeInteractable) {
            List<Interactable> allTabTargets = new List<Interactable>();
            int validMask = 0;
            if (includeInteractable) {
                validMask = (1 << LayerMask.NameToLayer("CharacterUnit")) | (1 << LayerMask.NameToLayer("Interactable")) | (1 << LayerMask.NameToLayer("Player"));
            } else {
                validMask = 1 << LayerMask.NameToLayer("CharacterUnit") | (1 << LayerMask.NameToLayer("Player"));
            }
            Collider[] hitColliders = new Collider[100];
            playerManagerClient.UnitController.PhysicsScene.OverlapSphere(playerManagerClient.ActiveUnitController.transform.position, systemConfigurationManager.TabTargetMaxDistance, hitColliders, validMask, QueryTriggerInteraction.UseGlobal);
            //Debug.Log("GetNextTabTarget(): collider length: " + hitColliders.Length + "; index: " + tabTargetIndex);

            // although the layermask on the collider should have only delivered us valid characterUnits, they may be dead or friendly.  We need to put all the valid attack targets in a list first
            foreach (Collider hitCollider in hitColliders) {
                //Debug.Log("GetNextTabTarget(): collider length: " + hitColliders.Length);
                //GameObject collidedGameObject = hitCollider.gameObject;
                if (hitCollider == null || hitCollider.gameObject == playerManagerClient.UnitController.gameObject) {
                    continue;
                }
                Interactable targetInteractable = hitCollider.gameObject.GetComponent<Interactable>();
                if (targetInteractable != null
                    && targetInteractable != oldTarget
                    && targetInteractable.IsMouseOverBlocked() == false
                    //&& (includeFriendly == true || ValidEnemyTarget(targetCharacterUnit))) {
                    && (includeFriendly == true || ValidEnemyTarget(targetInteractable.CharacterTarget))) {

                    // check if the unit is actually in front of our character.
                    // not doing any cone or angles for now, anywhere in front will do.  might adjust this a bit later to prevent targetting units nearly adjacent to us and far away
                    Vector3 transformedPosition = playerManagerClient.ActiveUnitController.transform.InverseTransformPoint(targetInteractable.transform.position);
                    if (transformedPosition.z > 0f) {
                        allTabTargets.Add(targetInteractable.CharacterTarget);

                    }
                }
            }

            return allTabTargets;
        }

        private void GetNextTabTarget(Interactable oldTarget, bool includeFriendly, bool includeInteractable, bool right = true) {
            //Debug.Log("PlayerController.GetNextTabTarget(): maxDistance: " + tabTargetMaxDistance);
            DateTime currentTime = DateTime.Now;
            TimeSpan timeSinceLastTab = currentTime - lastTabTargetTime;
            lastTabTargetTime = DateTime.Now;
            //int preferredTargetIndex = -1;
            int closestTargetIndex = -1;
            float closestTargetDistance = 0f;
            float targetDistance = 0f;
            float xPosition = 0f;

            List<Interactable> allTabTargets = GetTabTargets(oldTarget, includeFriendly, includeInteractable);

            if (allTabTargets.Count == 0) {
                // no valid characters in range
                //Debug.Log("PlayerController.GetNextTabTarget(): no valid characters in range, returning");
                return;
            } else {
                //Debug.Log("PlayerController.GetNextTabTarget(): valid target count: " + allTabTargets.Count);
            }

            // now that we have all valid attack targets, we need to process the list a bit before choosing a target
            float currentx = 0f;
            float closestLeftDistance = 0f;
            int closestLeftIndex = -1;
            float farthestLeftDistance = 0f;
            int farthestLeftIndex = -1;
            float closestRightDistance = 0f;
            int closestRightIndex = -1;
            float farthestRightDistance = 0f;
            int farthestRightIndex = -1;

            if (oldTarget != null) {
                currentx = playerManagerClient.ActiveUnitController.transform.InverseTransformPoint(oldTarget.transform.position).x;
            }

            int i = 0;
            foreach (Interactable collidedGameObject in allTabTargets) {
                //Debug.Log("PlayerController.GetNextTabTarget(): processing target: " + i + "; " + collidedGameObject.name);
                targetDistance = Vector3.Distance(playerManagerClient.ActiveUnitController.transform.position, collidedGameObject.transform.position);
                if (closestTargetIndex == -1) {
                    closestTargetIndex = i;
                    closestTargetDistance = targetDistance;
                }
                if (targetDistance < closestTargetDistance) {
                    closestTargetIndex = i;
                    closestTargetDistance = targetDistance;
                }
                xPosition = playerManagerClient.ActiveUnitController.transform.InverseTransformPoint(collidedGameObject.transform.position).x;
                if (closestLeftIndex == -1 && xPosition <= currentx) {
                    //Debug.Log("no left index and x position " + xPosition + " < currentx " + currentx);
                    closestLeftDistance = xPosition;
                    farthestLeftDistance = xPosition;
                    closestLeftIndex = i;
                    farthestLeftIndex = i;
                }
                if (closestRightIndex == -1 && xPosition >= currentx) {
                    //Debug.Log("no right index and x position " + xPosition + " > currentx " + currentx);
                    closestRightDistance = xPosition;
                    farthestRightDistance = xPosition;
                    closestRightIndex = i;
                    farthestRightIndex = i;
                }
                // closer than current closest left
                if (closestLeftIndex != -1 && xPosition > closestLeftDistance && xPosition < currentx) {
                    closestLeftDistance = xPosition;
                    closestLeftIndex = i;
                }
                // farther than current farthest left
                if (farthestLeftIndex != -1 && xPosition < farthestLeftDistance) {
                    farthestLeftDistance = xPosition;
                    farthestLeftIndex = i;
                }
                // closer than current closest right
                if (closestRightIndex != -1 && xPosition < closestRightDistance && xPosition > currentx) {
                    closestRightDistance = xPosition;
                    closestRightIndex = i;
                }
                // farther than current farthest right
                if (farthestRightIndex != -1 && xPosition > farthestRightDistance) {
                    farthestRightDistance = xPosition;
                    farthestRightIndex = i;
                }

                /*
                // this next variable shouldn't actually be needed.  i think it was a logic error with not tracking the target index properly
                if (preferredTargetIndex == -1) {
                    preferredTargetIndex = i;
                }
                */
                i++;
            }

            /*
            tabTargetIndex++;
            if (tabTargetIndex >= allTabTargets.Count) {
                tabTargetIndex = 0;
            }
            */
            //Debug.Log("PlayerController.GetNextTabTarget(): processing complete: closestTargetIndex: " + closestTargetIndex + "; target: " + (target == null ? "null" : target.name) + "; closestTargetName: " + characterUnitList[closestTargetIndex]);

            // reset to closest unit every 3 seconds if starting a new round of tabbing.
            // otherwise, just keep going through the index
            if (controlsManager.DPadRightPressed == true) {
                playerManagerClient.UnitController.ClearTarget();
                if (oldTarget == null) {
                    //Debug.Log("DPadRightPressed : setting closest Target Index: " + closestTargetIndex + "; " + allTabTargets[closestTargetIndex]);
                    playerManagerClient.UnitController.SetTarget(allTabTargets[closestTargetIndex]);
                    return;
                }
                if (closestRightIndex != -1) {
                    //Debug.Log("DPadRightPressed : setting closest Right Index: " + closestRightIndex + "; " + allTabTargets[closestRightIndex]);
                    playerManagerClient.UnitController.SetTarget(allTabTargets[closestRightIndex]);
                } else {
                    //Debug.Log("DPadRightPressed : setting farthest Left Index: " + farthestLeftIndex + "; " + allTabTargets[farthestLeftIndex]);
                    playerManagerClient.UnitController.SetTarget(allTabTargets[farthestLeftIndex]);
                }
                return;
            } else if (controlsManager.DPadLeftPressed == true) {
                playerManagerClient.UnitController.ClearTarget();
                if (oldTarget == null) {
                    playerManagerClient.UnitController.SetTarget(allTabTargets[closestTargetIndex]);
                    return;
                }
                if (closestLeftIndex != -1) {
                    playerManagerClient.UnitController.SetTarget(allTabTargets[closestLeftIndex]);
                } else {
                    playerManagerClient.UnitController.SetTarget(allTabTargets[farthestRightIndex]);
                }
                return;
            }
            if (timeSinceLastTab.TotalSeconds > 3f || oldTarget == null) {
                //Debug.Log("PlayerController.GetNextTabTarget(): More than 3 seconds since last tab");
                playerManagerClient.UnitController.ClearTarget();
                playerManagerClient.UnitController.SetTarget(allTabTargets[closestTargetIndex]);
                /*
                if (closestTargetIndex != -1 && allTabTargets[closestTargetIndex] != playerManager.UnitController.Target) {
                    // prevent a tab from re-targetting the same unit just because it's closest to us
                    // we only want to clear the target if we are actually setting a new target
                    playerManager.UnitController.ClearTarget();
                    playerManager.UnitController.SetTarget(allTabTargets[closestTargetIndex]);
                    // we need to manually set this here, otherwise our tab target index won't match our actual target, resulting in the next tab possibly not switching to a new target
                    tabTargetIndex = closestTargetIndex;
                    //} else if (preferredTarget != null) {
                } else {
                    if (allTabTargets[tabTargetIndex] != playerManager.UnitController.Target) {
                        // we only want to clear the target if we are actually setting a new target
                        playerManager.UnitController.ClearTarget();
                        playerManager.UnitController.SetTarget(allTabTargets[tabTargetIndex]);
                    }
                }
                */
            } else {
                //Debug.Log("PlayerController.GetNextTabTarget(): Less than 3 seconds since last tab, using index: " + tabTargetIndex);
                // we only want to clear the target if we are actually setting a new target
                /*
                if (allTabTargets[tabTargetIndex] != playerManager.UnitController.Target) {
                    playerManager.UnitController.ClearTarget();
                    playerManager.UnitController.SetTarget(allTabTargets[tabTargetIndex]);
                }
                */
                if (closestRightIndex != -1) {
                    playerManagerClient.UnitController.SetTarget(allTabTargets[closestRightIndex]);
                } else {
                    playerManagerClient.UnitController.SetTarget(allTabTargets[farthestLeftIndex]);
                }
            }
        }

        public void InterActWithTarget(Interactable interactable, bool resetTarget = true) {
            //Debug.Log($"{gameObject.name}.InterActWithTarget({interactable.gameObject.name})");

            if (interactable.IsMouseOverBlocked() == true) {
                //Debug.Log("PlayerController.InterActWithTarget(): mouseover blocked");
                return;
            }

            if ((playerManagerClient.UnitController.Target == null || playerManagerClient.UnitController.Target != interactable) && resetTarget == true) {
                playerManagerClient.UnitController.ClearTarget();
                playerManagerClient.UnitController.SetTarget(interactable);
            }

            interactionManagerClient.InteractWithInteractable(playerManagerClient.UnitController, interactable);

            if (resetTarget == true) {
                // not actually stopping interacting.  just clearing target if this was a trigger interaction and we are not interacting with a focus
                StopInteract();
            }
        }

        private void HandleCancelButtonPressed() {
            //Debug.Log("HandleCancelButtonPressed()");
            if (controlsManager.WindowStackCount > 0) {
                // escape / cancel key should go to the window, if one is open
                return;
            }

            if (inputManager.KeyBindWasPressed("CANCELALL")
                || (inputManager.KeyBindWasPressed("JOYSTICKBUTTON1") && controlsManager.RightTriggerDown == false && controlsManager.LeftTriggerDown == false)) {
                uIManager.MovementTargetController.DisableProjector();
                playerManagerClient.UnitController.ClearTarget();
                if (playerManagerClient.ActiveUnitController.CharacterStats.IsAlive != false) {
                    // that stuff is already done by ClearTarget()
                    /*
                    if (playerManager.ActiveUnitController.UnitMotor.HasDestination() == true) {
                        uIManager.MovementTargetController.DisableProjector();
                        playerManager.ActiveUnitController.UnitMotor.StopFollowingTarget();
                    }
                    */
                    playerManagerClient.ActiveUnitController.CharacterAbilityManager.TryToStopAnyAbility();
                    playerManagerClient.UnitController.UnitActionManager.TryToStopAction();
                }
                playerManagerClient.ActiveUnitController.CharacterAbilityManager.DeactivateTargetingMode();
            }
        }

        public void RegisterAbilityButtonPresses() {
            //Debug.Log("PlayerController.RegisterAbilityButtonPresses()");
            foreach (KeyBindNode keyBindNode in keyBindManager.KeyBinds.Values) {
                //Debug.Log("PlayerController.RegisterAbilityButtonPresses() keyBindNode.GetKeyDown: " + keyBindNode.GetKeyDown);
                //Debug.Log("PlayerController.RegisterAbilityButtonPresses() keyBindNode.GetKeyDown: " + keyBindNode.GetKey);
                if (keyBindNode.KeyBindType == KeyBindType.Action && inputManager.KeyBindWasPressed(keyBindNode.KeyBindID) == true) {
                    //Debug.Log("PlayerController.RegisterAbilityButtonPresses(): key pressed: " + keyBindNode.MyKeyCode.ToString());
                    keyBindNode.ActionButton.OnClick(true);
                }
            }
        }

        public void HandleClearTarget(Interactable oldTarget) {
            //Debug.Log("PlayerController.HandleClearTarget()");

            if (PlayerPrefs.HasKey("LockUI") == true && PlayerPrefs.GetInt("LockUI") == 0) {
                uIManager.FocusUnitFramePanel.ClearTarget(false);
            } else {
                uIManager.FocusUnitFramePanel.ClearTarget();
            }
            namePlateManager.ClearFocus();
            oldTarget?.PhysicalTarget.SetUnTargeted();
        }

        public void HandleSetTarget(Interactable newTarget) {
            //Debug.Log($"PlayerController.HandleSetTarget({(newTarget == null ? "null" : newTarget.gameObject.name)})");

            if (newTarget == null) {
                return;
            }
            contextMenuService.CloseContextMenu();
            if (newTarget is UnitController) {
                //Debug.Log("PlayerController.SetTarget(): InamePlateUnit is not null");
                uIManager.FocusUnitFramePanel.SetTarget(newTarget as UnitController);
                namePlateManager.SetFocus(newTarget);
            }
            newTarget?.PhysicalTarget.SetTargeted();
        }

        public void StopInteract() {
            // the idea of this code is that it will allow us to keep an NPC focused if we back out of range while its interactable popup closes
            // if we don't have anything focused, then we were interacting with someting environmental and definitely want to clear that because it can lead to a hidden target being set
            if (uIManager.FocusUnitFramePanel.UnitController == null && playerManagerClient.UnitController != null) {
                playerManagerClient.UnitController.ClearTarget();
            }
        }

        public void SubscribeToUnitEvents() {
            //Debug.Log($"PlayerController.SubscribeToUnitEvents() activeUnitController: {(playerManagerClient.ActiveUnitController == null ? "null" : playerManagerClient.ActiveUnitController.gameObject.name)}");

            // this one catches the initial player unit spawn
            if (playerManagerClient.ActiveUnitController.UnitProfile.UnitPrefabProps.ForceRotateModelMode == true) {
                //Debug.Log($"PlayerController.SubscribeToUnitEvents() force rotate model mode enabled, disabling strafe mode");
                strafeModeActive = false;
            }

            // if player was agrod at spawn, they may have a target already since we subscribe on model ready
            playerManagerClient.ActiveUnitController.UnitEventController.OnSetTarget += HandleSetTarget;
            if (playerManagerClient.ActiveUnitController.Target != null) {
                HandleSetTarget(playerManagerClient.ActiveUnitController.Target);
            }

            playerManagerClient.ActiveUnitController.UnitEventController.OnClearTarget += HandleClearTarget;
            playerManagerClient.ActiveUnitController.UnitEventController.OnActivateMountedState += HandleActivateMountedState;
            playerManagerClient.ActiveUnitController.UnitEventController.OnDeactivateMountedState += HandleDeactivateMountedState;
            systemEventManager.OnPlayerUnitDespawn += HandleUnitDespawn;
            playerManagerClient.ActiveUnitController.UnitEventController.OnCastCancel += HandleCastCancel;
            playerManagerClient.ActiveUnitController.UnitModelController.OnModelUpdated += HandleModelUpdated;

            // subscribe and call in case the namePlate is already spawned
            playerManagerClient.ActiveUnitController.OnInitializeNamePlate += HandleInitializeNamePlate;
            HandleInitializeNamePlate();
        }

        public void UnsubscribeFromUnitEvents() {
            //Debug.Log($"{gameObject.name}.PlayerController.UnsubscribeFromUnitEvents()");
            playerManagerClient.ActiveUnitController.UnitEventController.OnSetTarget -= HandleSetTarget;
            playerManagerClient.ActiveUnitController.UnitEventController.OnClearTarget -= HandleClearTarget;
            playerManagerClient.ActiveUnitController.UnitEventController.OnActivateMountedState -= HandleActivateMountedState;
            playerManagerClient.ActiveUnitController.UnitEventController.OnDeactivateMountedState -= HandleDeactivateMountedState;
            playerManagerClient.ActiveUnitController.OnInitializeNamePlate -= HandleInitializeNamePlate;
            systemEventManager.OnPlayerUnitDespawn -= HandleUnitDespawn;
            playerManagerClient.ActiveUnitController.UnitEventController.OnCastCancel -= HandleCastCancel;
            playerManagerClient.ActiveUnitController.UnitModelController.OnModelUpdated -= HandleModelUpdated;

        }

        public void HandleModelUpdated() {
            if (systemGameManager.GameMode == GameMode.Local) {
                playerManagerClient.UnitController.CharacterSaveManager.SaveAppearanceData();
            }
        }

        public void HandleCastCancel() {
            //Debug.Log("PlayerController.HandleCastCancel()");
            playerManagerClient.UnitController.CharacterCraftingManager.ClearCraftingQueue();
        }

        public void HandleInitializeNamePlate() {
            //Debug.Log("PlayerController.HandleInitializeNamePlate()");
            if (playerManagerClient?.ActiveUnitController?.NamePlateController?.NamePlate != null) {
                playerManagerClient.ActiveUnitController.NamePlateController.NamePlate.SetPlayerOwnerShip();
            }
        }

        public void HandleUnitDespawn(UnitController unitController) {
            //Debug.Log($"{gameObject.name}.PlayerController.HandleUnitDestroy()");
            UnsubscribeFromUnitEvents();
            NotifyInteractablesOnDespawn();
        }

        public void NotifyInteractablesOnDespawn() {
            foreach (Interactable interactable in interactables) {
                interactable.RegisterDespawn(playerManagerClient.ActiveUnitController.gameObject);
            }
        }

        public void HandleActivateMountedState(UnitController mountUnitController) {

            playerManagerClient.SetActiveUnitController(mountUnitController);

            cameraManager.SwitchToMainCamera();
            cameraManager.MainCameraController.InitializeCamera(playerManagerClient.ActiveUnitController.CameraTransform, playerManagerClient.ActiveUnitController.NameplateVector.y);
        }

        public void HandleDeactivateMountedState() {

            playerManagerClient.SetActiveUnitController(playerManagerClient.UnitController);

            cameraManager.ActivateMainCamera();
            cameraManager.MainCameraController.InitializeCamera(playerManagerClient.UnitController.CameraTransform, playerManagerClient.UnitController.NameplateVector.y);
        }

        public void OnSendObjectToPool() {
            ClearInteractables();
        }

        public void ProcessSetActiveUnitController() {
            //Debug.Log($"PlayerController.ProcessSetActiveUnitController() activeUnitController: {(playerManagerClient.ActiveUnitController == null ? "null" : playerManagerClient.ActiveUnitController.gameObject.name)}");
            
            // this one captures the switch between mounted and normal states
            if (playerManagerClient.ActiveUnitController.UnitProfile.UnitPrefabProps.ForceRotateModelMode == true) {
                //Debug.Log($"PlayerController.ProcessSetActiveUnitController() force rotate model mode enabled, disabling strafe mode");
                strafeModeActive = false;
            }
        }
    }

}