using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AnyRPG {
    public class PlayerController : ConfiguredMonoBehaviour {

        public event System.Action<int> AbilityButtonPressedHandler = delegate { };
        public event System.Action<bool> ToggleRunHandler = delegate { };

        //Inputs.
        [HideInInspector] public bool inputJump;
        // inputFly is inputJump but true if held or pressed
        [HideInInspector] public bool inputFly;
        [HideInInspector] public bool inputSink;
        [HideInInspector] public bool inputStrafe;
        [HideInInspector] public bool inputCrouch;
        //[HideInInspector] public float inputAimVertical = 0;
        //[HideInInspector] public float inputAimHorizontal = 0;
        [HideInInspector] public float inputHorizontal = 0;
        [HideInInspector] public float inputTurn = 0;
        [HideInInspector] public float inputVertical = 0;

        //Variables
        [HideInInspector]
        public bool allowedInput = true;

        [HideInInspector]
        public bool autorunActive = false;

        public bool canMove = false;
        public bool canAction = false;

        [HideInInspector] public Vector3 NormalizedMoveInput;
        [HideInInspector] public Vector3 TurnInput;
        [HideInInspector] public Vector2 aimInput;

        public LayerMask movementMask;

        public float tabTargetMaxDistance = 20f;

        private List<Interactable> interactables = new List<Interactable>();
        private Interactable mouseOverInteractable = null;

        //private int tabTargetIndex = 0;

        private int crossBarIndex = 0;

        private DateTime lastTabTargetTime;

        private RaycastHit mouseOverhit;

        // game manager references
        protected InputManager inputManager = null;
        protected PlayerManager playerManager = null;
        protected MessageFeedManager messageFeedManager = null;
        protected NamePlateManager namePlateManager = null;
        protected CameraManager cameraManager = null;
        protected SystemEventManager systemEventManager = null;
        protected KeyBindManager keyBindManager = null;
        protected CraftingManager craftingManager = null;
        protected UIManager uIManager = null;
        protected WindowManager windowManager = null;
        protected ControlsManager controlsManager = null;
        protected ActionBarManager actionBarManager = null;
        protected CastTargettingManager castTargettingManager = null;

        public List<Interactable> Interactables { get => interactables; }
        public RaycastHit MouseOverhit { get => mouseOverhit; set => mouseOverhit = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            inputManager = systemGameManager.InputManager;
            playerManager = systemGameManager.PlayerManager;
            messageFeedManager = systemGameManager.UIManager.MessageFeedManager;
            namePlateManager = systemGameManager.UIManager.NamePlateManager;
            cameraManager = systemGameManager.CameraManager;
            systemEventManager = systemGameManager.SystemEventManager;
            keyBindManager = systemGameManager.KeyBindManager;
            craftingManager = systemGameManager.CraftingManager;
            uIManager = systemGameManager.UIManager;
            windowManager = systemGameManager.WindowManager;
            controlsManager = systemGameManager.ControlsManager;
            actionBarManager = uIManager.ActionBarManager;
            castTargettingManager = systemGameManager.CastTargettingManager;
        }

        public void AddInteractable(Interactable interactable) {
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
            if (interactables.Contains(interactable)) {
                interactables.Remove(interactable);
            }
            ShowHideInteractionPopup();
        }

        public void ShowHideInteractionPopup() {
            if (interactables.Count > 0
                && interactables[interactables.Count - 1].PrerequisitesMet == true
                && interactables[interactables.Count - 1].GetCurrentInteractables().Count > 0) {
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
            //Debug.Log(gameObject.name + ".PlayerController.ProcessLevelUnload()");
            ClearInteractables();
        }

        public void ClearInteractables() {
            //Debug.Log(gameObject.name + ".PlayerController.ClearInteractables()");
            interactables.Clear();
        }

        public void ResetMoveInput() {
            inputJump = false;
            inputFly = false;
            inputSink = false;
            inputStrafe = false;
            inputCrouch = false;
            //inputAimVertical = 0f;
            //inputAimHorizontal = 0f;
            inputHorizontal = 0;
            inputVertical = 0;
            inputTurn = 0;

            NormalizedMoveInput = NormalizedVelocity(new Vector3(inputHorizontal, 0, inputVertical));
            TurnInput = new Vector3(inputTurn, 0, 0);
        }


        private void CollectMoveInput() {
            //Debug.Log("PlayerController.CollectMoveInput()");

            // don't allow jump or crouch while activating action bars
            if (controlsManager.LeftTriggerDown == false && controlsManager.RightTriggerDown == false) {
                inputJump = inputManager.KeyBindWasPressed("JUMP");
                inputFly = inputManager.KeyBindWasPressedOrHeld("JUMP");
                inputSink = inputManager.KeyBindWasPressedOrHeld("CROUCH");
                inputCrouch = inputManager.KeyBindWasPressed("CROUCH");
            }

            inputStrafe = inputManager.KeyBindWasPressedOrHeld("STRAFELEFT") || inputManager.KeyBindWasPressedOrHeld("STRAFERIGHT");
            //inputAimVertical = Input.GetAxisRaw("AimVertical");
            //inputAimHorizontal = Input.GetAxisRaw("AimHorizontal");

            // gather joystick move input
            inputHorizontal = Input.GetAxis("LeftAnalogHorizontal");
            inputVertical = Input.GetAxis("LeftAnalogVertical");
            //Debug.Log("Joystick inputHorizontal: " + inputHorizontal + "; vertical: " + inputVertical);

            // gather keyboard move input
            inputHorizontal += (inputManager.KeyBindWasPressedOrHeld("STRAFELEFT") ? -1 : 0) + (inputManager.KeyBindWasPressedOrHeld("STRAFERIGHT") ? 1 : 0);
            inputVertical += (inputManager.KeyBindWasPressedOrHeld("BACK") ? -1 : 0) + (inputManager.KeyBindWasPressedOrHeld("FORWARD") ? 1 : 0);

            // only gather gamepad turn input while moving
            // temporarily disabled because gamepad movement turning is done by rotating the camera since everything is camera relative
            /*
            if (inputHorizontal != 0f || inputVertical != 0f) {
                inputTurn = Input.GetAxis("RightAnalogHorizontal");
            }
            */

            // gather keyboard turn input
            inputTurn += (inputManager.KeyBindWasPressedOrHeld("TURNLEFT") ? -1 : 0) + (inputManager.KeyBindWasPressedOrHeld("TURNRIGHT") ? 1 : 0);

            // turn off autorun if there is any movement input
            if (autorunActive
                && ((inputHorizontal != 0f) || (inputVertical != 0f) || inputJump || inputFly || inputSink || inputStrafe || inputCrouch)) {
                ToggleAutorun();
            }

            if (autorunActive) {
                inputVertical = 1;
            }

            NormalizedMoveInput = NormalizedVelocity(new Vector3(inputHorizontal, 0, inputVertical));
            TurnInput = new Vector3(inputTurn, 0, 0);

            if (HasMoveInput()) {
                //Debug.Log("PlayerController.CollectMoveInput(): hasMoveInput");
                playerManager.ActiveUnitController.CommonMovementNotifier();
            }
        }

        /*
        private void CollectAimInput() {
            aimInput = new Vector2(inputAimHorizontal, inputAimVertical);
        }
        */

        public void ProcessInput() {
            //Debug.Log("PlayerController.Update()");
            //ResetMoveInput();

            if (playerManager.ActiveUnitController == null) {
                //Debug.Log(gameObject.name + ".PlayerController.Update(): Player Unit is not spawned. Exiting");
                return;
            }

            if (uIManager.nameChangeWindow.IsOpen) {
                //Debug.Log("Not allowing movement during name change");
                return;
            }

            if (allowedInput == false) {
                //Debug.Log("Not allowed to Collect Move Input. Exiting PlayerController Update!");
                return;
            }

            //CollectAimInput();

            HandleCancelButtonPressed();

            HandleMouseOver();

            if (playerManager?.MyCharacter?.CharacterStats?.IsAlive == false) {
                // can't interact, perform abilities or handle movement when dead
                return;
            }

            // test move this below death check to prevent player getting up after death
            ToggleRun();

            CheckToggleAutorun();

            HandleLeftMouseClick();

            RegisterTab();

            // everything below this point cannot be done while control locked
            if (playerManager?.ActiveUnitController != null && playerManager.ActiveUnitController.ControlLocked == true) {
                return;
            }
            CollectMoveInput();

            HandleRightMouseClick();

            ProcessGamepadButtonClicks();

            RegisterAbilityButtonPresses();


        }


        private Vector3 NormalizedVelocity(Vector3 inputVelocity) {
            if (inputVelocity.magnitude > 1) {
                inputVelocity.Normalize();
            }
            return inputVelocity;
        }

        public bool HasAnyInput() {
            if (allowedInput && (NormalizedMoveInput != Vector3.zero || TurnInput != Vector3.zero || aimInput != Vector2.zero || inputJump != false)) {
                return true;
            } else {
                return false;
            }
        }

        public bool HasWaterMoveInput() {
            if (allowedInput && (NormalizedMoveInput != Vector3.zero || TurnInput != Vector3.zero || inputSink != false || inputFly != false)) {
                return true;
            } else {
                return false;
            }
        }

        public bool HasFlyMoveInput() {
            if (allowedInput && (NormalizedMoveInput != Vector3.zero || TurnInput != Vector3.zero || inputSink != false || inputFly != false)) {
                return true;
            } else {
                return false;
            }
        }

        public bool HasMoveInput() {
            if (allowedInput && NormalizedMoveInput != Vector3.zero) {
                return true;
            } else {
                return false;
            }
        }

        public bool HasTurnInput() {
            if (allowedInput && TurnInput != Vector3.zero) {
                return true;
            } else {
                return false;
            }
        }

        public bool HasAimInput() {
            if (allowedInput && (aimInput.x < -0.8f || aimInput.x > 0.8f) || (aimInput.y < -0.8f || aimInput.y > 0.8f)) {
                return true;
            } else {
                return false;
            }
        }

        private void ToggleRun() {
            if (inputManager.KeyBindWasPressed("TOGGLERUN")) {
                EventParamProperties eventParamProperties = new EventParamProperties();
                if (playerManager.ActiveUnitController.Walking == false) {
                    playerManager.ActiveUnitController.Walking = true;
                    eventParamProperties.simpleParams.BoolParam = true;
                } else {
                    playerManager.ActiveUnitController.Walking = false;
                    eventParamProperties.simpleParams.BoolParam = false;
                }
                SystemEventManager.TriggerEvent("OnToggleRun", eventParamProperties);
                messageFeedManager.WriteMessage("Walk: " + playerManager.ActiveUnitController.Walking.ToString());
                ToggleRunHandler(playerManager.ActiveUnitController.Walking);
            }
        }

        private void CheckToggleAutorun() {
            if (inputManager.KeyBindWasPressed("TOGGLEAUTORUN")) {
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

        protected void FixedUpdate() {
            // TODO : work out click to move and delayed interaction logic in a way that properly tracks the target you are trying to get to
            // instead of just using the generic target variable
            // disabled until click-to-move is re-enabled in a consistent way
            //CheckForInteraction();
        }

        /// <summary>
        /// this code is necessary because the only other solution to mouseover through the player is to set the player to layer Ignore Raycast
        /// which breaks the invector controller
        /// </summary>
        private void HandleMouseOver() {
            //Debug.Log(gameObject.name + ".PlayerController.HandleMouseOver()");
            if (cameraManager.ActiveMainCamera == null) {
                // we are in a cutscene and shouldn't be dealing with mouseover
                return;
            }

            // gamepad mode can hide the cursor.  Mouseover should not be activated when the cursor is hidden
            if (Cursor.visible == false) {
                return;
            }

            Ray ray = cameraManager.ActiveMainCamera.ScreenPointToRay(Input.mousePosition);
            int playerMask = 1 << LayerMask.NameToLayer("Player");
            int ignoreMask = 1 << LayerMask.NameToLayer("Ignore Raycast");
            int spellMask = 1 << LayerMask.NameToLayer("SpellEffects");
            int waterMask = 1 << LayerMask.NameToLayer("Water");
            int layerMask = ~(playerMask | ignoreMask | spellMask | waterMask);

            bool disableMouseOver = false;
            bool mouseOverNamePlate = false;
            mouseOverNamePlate = namePlateManager.MouseOverNamePlate();

            if (!EventSystem.current.IsPointerOverGameObject() && !mouseOverNamePlate) {
                if (Physics.Raycast(ray, out mouseOverhit, 100, layerMask)) {
                    // prevent clicking on mount
                    if (mouseOverhit.collider.gameObject != playerManager.ActiveUnitController.gameObject) {
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
                //Debug.Log(gameObject.name + ".PlayerController.HandleMouseOver(): mouseovernameplate: " + namePlateManager.MouseOverNamePlate() + "; pointerovergameobject: " + EventSystem.current.IsPointerOverGameObject());
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
            //Debug.Log(gameObject.name + ".PlayerController.HandleMouseOver()");
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
            //Debug.Log(gameObject.name + ".PlayerController.HandleRightMouseClick()");
            // check if the right mouse button clicked on something and interact with it
            if (inputManager.rightMouseButtonClicked && !EventSystem.current.IsPointerOverGameObject()) {
                //Debug.Log(gameObject.name + ".PlayerController.HandleRightMouseClick(): !EventSystem.current.IsPointerOverGameObject() == true!!!");


                if (mouseOverInteractable != null && mouseOverInteractable.IsTrigger == false) {
                    //Debug.Log("setting interaction target to " + hit.collider.gameObject.name);
                    //interactionTarget = hit.collider.gameObject;
                    InterActWithTarget(mouseOverInteractable);
                }
                //Debug.Log("We hit " + hit.collider.name + " " + hit.point);
            }
        }

        private void ProcessGamepadButtonClicks() {
            //Debug.Log(gameObject.name + ".PlayerController.ProcessGamepadButtonClicks()");

            // if a window is open, all button clicks will be processed by that window instead of the player
            if (windowManager.WindowStack.Count > 0) {
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
                if (playerManager.ActiveCharacter?.CharacterAbilityManager?.WaitingForTarget() == false) {

                    if (interactables.Count > 0) {
                        // range interactables are priority
                        InterActWithTarget(interactables[interactables.Count - 1]);
                    } else {
                        // allow friendly target when nothing is targeted
                        if (playerManager.UnitController.Target == null) {
                            GetNextTabTarget(playerManager.UnitController.Target, true, true);
                        } else {
                            InterActWithTarget(playerManager.UnitController.Target);
                        }
                    }
                } else {
                    FinishGroundTarget(castTargettingManager.CastTargetController.VirtualCursor);
                }
            } else if (controlsManager.DPadRightPressed) {
                GetNextTabTarget(playerManager.UnitController.Target, true, false);
            } else if (controlsManager.DPadLeftPressed) {
                GetNextTabTarget(playerManager.UnitController.Target, true, false, false);
            }
        }

        private void FinishGroundTarget(Vector3 targetPosition) {
            Ray ray = cameraManager.ActiveMainCamera.ScreenPointToRay(targetPosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100, movementMask)) {
                if (playerManager.ActiveCharacter.CharacterAbilityManager.WaitingForTarget()) {
                    playerManager.ActiveCharacter.CharacterAbilityManager.SetGroundTarget(hit.point);
                }
            }
        }

        private void HandleLeftMouseClick() {
            //Debug.Log("PlayerController.HandleLeftMouseClick()");
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

            //if (inputManager.leftMouseButtonClicked && !EventSystem.current.IsPointerOverGameObject()) {
            if (mouseOverInteractable == null && !namePlateManager.MouseOverNamePlate()) {
                // Stop focusing any object
                //RemoveFocus();
                playerManager.UnitController.ClearTarget();
            } else if (mouseOverInteractable != null) {
                playerManager.UnitController.SetTarget(mouseOverInteractable);
            }
            //}

            FinishGroundTarget(Input.mousePosition);
        }

        /// <summary>
        /// if an interactable is set, try to interact with it if it's in range.
        /// </summary>
        private void CheckForInteraction() {
            //Debug.Log(gameObject.name + ".PlayerController.CheckForInteraction()");

            if (playerManager.UnitController == null) {
                return;
            }
            if (playerManager.UnitController.Target == null) {
                return;
            }
            if (InteractionSucceeded()) {
                if (playerManager.ActiveUnitController != null && playerManager.ActiveUnitController.UnitMotor != null) {
                    playerManager.ActiveUnitController.UnitMotor.StopFollowingTarget();
                }
            }
        }

        private bool InteractionSucceeded() {
            //Debug.Log(gameObject.name + ".PlayerController.InteractionSucceeded()");

            if (playerManager.UnitController == null) {
                return false;
            }
            if (playerManager.UnitController.Target == null) {
                //Debug.Log(gameObject.name + ".PlayerController.InteractionSucceeded(): target is null. return false.");
                return false;
            }
            //if (IsTargetInHitBox(target)) {
            // get reference to name now since interactable could change scene and then target reference is lost
            string targetDisplayName = playerManager.UnitController.Target.DisplayName;
            if (playerManager.UnitController.Target.Interact(playerManager.ActiveUnitController.CharacterUnit, true)) {
                //Debug.Log(gameObject.name + ".PlayerController.InteractionSucceeded(): Interaction Succeeded.  Setting interactable to null");
                systemEventManager.NotifyOnInteractionStarted(targetDisplayName);
                return true;
            }
            //Debug.Log(gameObject.name + ".PlayerController.InteractionSucceeded(): returning false");

            return false;
            //}
            //return false;
        }

        private void RegisterTab() {

            //Interactable oldTarget = playerManager.UnitController.Target;

            // register keyboard tab target, only allow enemy target
            if (inputManager.KeyBindWasPressed("NEXTTARGET")) {
                //Debug.Log("Tab Target Registered");
                GetNextTabTarget(playerManager.UnitController.Target, false, false);
            }
        }

        /*
        private bool ValidFriendlyTarget(Interactable interactable) {
            if (interactable != null) {
                return true;
            }
            return false;
        }
        */

        private bool ValidEnemyTarget(Interactable interactable) {
            UnitController targetCharacterUnit = interactable.GetComponent<UnitController>();
            if (targetCharacterUnit != null
                && targetCharacterUnit.CharacterUnit.BaseCharacter.CharacterStats.IsAlive == true
                && Faction.RelationWith(targetCharacterUnit.CharacterUnit.BaseCharacter, playerManager.MyCharacter.Faction) <= -1) {
                return true;
            }
            return false;
        }

        private List<Interactable> GetTabTargets(Interactable oldTarget, bool includeFriendly, bool includeInteractable) {
            List<Interactable> allTabTargets = new List<Interactable>();
            int validMask = 0;
            if (includeInteractable) {
                validMask = (1 << LayerMask.NameToLayer("CharacterUnit")) | (1 << LayerMask.NameToLayer("Interactable"));
            } else {
                validMask = 1 << LayerMask.NameToLayer("CharacterUnit");
            }
            Collider[] hitColliders = Physics.OverlapSphere(playerManager.ActiveUnitController.transform.position, tabTargetMaxDistance, validMask);
            //Debug.Log("GetNextTabTarget(): collider length: " + hitColliders.Length + "; index: " + tabTargetIndex);

            // although the layermask on the collider should have only delivered us valid characterUnits, they may be dead or friendly.  We need to put all the valid attack targets in a list first
            foreach (Collider hitCollider in hitColliders) {
                //Debug.Log("GetNextTabTarget(): collider length: " + hitColliders.Length);
                //GameObject collidedGameObject = hitCollider.gameObject;
                Interactable targetCharacterUnit = hitCollider.gameObject.GetComponent<Interactable>();
                if (targetCharacterUnit != null
                    && targetCharacterUnit != oldTarget
                    //&& (includeFriendly == true || ValidEnemyTarget(targetCharacterUnit))) {
                    && (includeFriendly == true || ValidEnemyTarget(targetCharacterUnit))) {

                    // check if the unit is actually in front of our character.
                    // not doing any cone or angles for now, anywhere in front will do.  might adjust this a bit later to prevent targetting units nearly adjacent to us and far away
                    Vector3 transformedPosition = playerManager.ActiveUnitController.transform.InverseTransformPoint(targetCharacterUnit.transform.position);
                    if (transformedPosition.z > 0f) {
                        allTabTargets.Add(targetCharacterUnit);

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
                currentx = playerManager.ActiveUnitController.transform.InverseTransformPoint(oldTarget.transform.position).x;
            }

            int i = 0;
            foreach (Interactable collidedGameObject in allTabTargets) {
                //Debug.Log("PlayerController.GetNextTabTarget(): processing target: " + i + "; " + collidedGameObject.name);
                targetDistance = Vector3.Distance(playerManager.ActiveUnitController.transform.position, collidedGameObject.transform.position);
                if (closestTargetIndex == -1) {
                    closestTargetIndex = i;
                    closestTargetDistance = targetDistance;
                }
                if (targetDistance < closestTargetDistance) {
                    closestTargetIndex = i;
                    closestTargetDistance = targetDistance;
                }
                xPosition = playerManager.ActiveUnitController.transform.InverseTransformPoint(collidedGameObject.transform.position).x;
                if (closestLeftIndex == -1 && xPosition <= currentx ) {
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
                playerManager.UnitController.ClearTarget();
                if (oldTarget == null) {
                    //Debug.Log("DPadRightPressed : setting closest Target Index: " + closestTargetIndex + "; " + allTabTargets[closestTargetIndex]);
                    playerManager.UnitController.SetTarget(allTabTargets[closestTargetIndex]);
                    return;
                }
                if (closestRightIndex != -1) {
                    //Debug.Log("DPadRightPressed : setting closest Right Index: " + closestRightIndex + "; " + allTabTargets[closestRightIndex]);
                    playerManager.UnitController.SetTarget(allTabTargets[closestRightIndex]);
                } else {
                    //Debug.Log("DPadRightPressed : setting farthest Left Index: " + farthestLeftIndex + "; " + allTabTargets[farthestLeftIndex]);
                    playerManager.UnitController.SetTarget(allTabTargets[farthestLeftIndex]);
                }
                return;
            } else if (controlsManager.DPadLeftPressed == true) {
                playerManager.UnitController.ClearTarget();
                if (oldTarget == null) {
                    playerManager.UnitController.SetTarget(allTabTargets[closestTargetIndex]);
                    return;
                }
                if (closestLeftIndex != -1) {
                    playerManager.UnitController.SetTarget(allTabTargets[closestLeftIndex]);
                } else {
                    playerManager.UnitController.SetTarget(allTabTargets[farthestRightIndex]);
                }
                return;
            }
            if (timeSinceLastTab.TotalSeconds > 3f || oldTarget == null) {
                //Debug.Log("PlayerController.GetNextTabTarget(): More than 3 seconds since last tab");
                playerManager.UnitController.ClearTarget();
                playerManager.UnitController.SetTarget(allTabTargets[closestTargetIndex]);
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
                    playerManager.UnitController.SetTarget(allTabTargets[closestRightIndex]);
                } else {
                    playerManager.UnitController.SetTarget(allTabTargets[farthestLeftIndex]);
                }
            }
        }

        public void InterActWithTarget(Interactable interactable) {
            //Debug.Log(gameObject.name + ".InterActWithTarget(" + interactable.gameObject.name + ")");
            if (playerManager.UnitController.Target != interactable) {
                playerManager.UnitController.ClearTarget();
                playerManager.UnitController.SetTarget(interactable);
            }
            if (InteractionSucceeded()) {
                //Debug.Log("We were able to interact with the target");
                // not actually stopping interacting.  just clearing target if this was a trigger interaction and we are not interacting with a focus
                StopInteract();
            } else {
                //Debug.Log("we were out of range and must move toward the target to be able to interact with it");
                if (playerManager.PlayerUnitMovementController.useMeshNav) {
                    //Debug.Log("Nav Mesh Agent is enabled. Setting follow target: " + target.name);
                    playerManager.ActiveUnitController.UnitMotor.FollowTarget(playerManager.UnitController.Target);
                } else {
                    //Debug.Log("Nav Mesh Agent is disabled and you are out of range");
                }
            }
        }

        public void InterActWithInteractableOption(Interactable interactable, InteractableOptionComponent interactableOption) {
            playerManager.UnitController.SetTarget(interactable);
            if (InteractionWithOptionSucceeded(interactableOption)) {
                // not actually stopping interacting.  just clearing target if this was a trigger interaction and we are not interacting with a focus
                StopInteract();
            } else {
                //Debug.Log("we were out of range and must move toward the target to be able to interact with it");
                if (playerManager.PlayerUnitMovementController.useMeshNav) {
                    //Debug.Log("Nav Mesh Agent is enabled. Setting follow target: " + target.name);
                    playerManager.ActiveUnitController.UnitMotor.FollowTarget(playerManager.UnitController.Target);
                } else {
                    //Debug.Log("Nav Mesh Agent is disabled and you are out of range");
                }
            }
        }

        private bool InteractionWithOptionSucceeded(InteractableOptionComponent interactableOption) {
            //Debug.Log(gameObject.name + ".PlayerController.InteractionSucceeded()");
            //if (IsTargetInHitBox(target)) {
            if (interactableOption.Interact(playerManager.ActiveUnitController.CharacterUnit)) {
                //Debug.Log(gameObject.name + ".PlayerController.InteractionSucceeded(): Interaction Succeeded.  Setting interactable to null");
                systemEventManager.NotifyOnInteractionStarted(playerManager.UnitController.Target.DisplayName);
                systemEventManager.NotifyOnInteractionWithOptionStarted(interactableOption);
                // no longer needed since targeting is changed and we don't want to lose target in the middle of attacking
                //playerManager.ActiveUnitController.SetTarget(null);
                return true;
            }
            //Debug.Log(gameObject.name + ".PlayerController.InteractionSucceeded(): returning false");
            return false;
            //}
            //return false;
        }

        private void HandleCancelButtonPressed() {
            //Debug.Log("HandleCancelButtonPressed()");
            if (inputManager.KeyBindWasPressed("CANCEL")
                || inputManager.KeyBindWasPressed("CANCELALL")
                || (inputManager.KeyBindWasPressed("JOYSTICKBUTTON1") && controlsManager.RightTriggerDown == false && controlsManager.LeftTriggerDown == false)) {
                playerManager.UnitController.ClearTarget();
                if (playerManager.ActiveCharacter.CharacterStats.IsAlive != false) {
                    // prevent character from swapping to third party controller while dead
                    playerManager.ActiveCharacter.CharacterAbilityManager.StopCasting();
                }
                playerManager.ActiveCharacter.CharacterAbilityManager.DeActivateTargettingMode();
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
                uIManager.FocusUnitFrameController.ClearTarget(false);
            } else {
                uIManager.FocusUnitFrameController.ClearTarget();
            }
            namePlateManager.ClearFocus();
            oldTarget?.UnitComponentController?.HighlightController?.HandleClearTarget();
        }

        public void HandleSetTarget(Interactable newTarget) {
            //Debug.Log("PlayerController.HandleSetTarget(" + (newTarget == null ? "null" : newTarget.name) + ")");
            if (newTarget == null) {
                return;
            }
            NamePlateUnit namePlateUnit = (newTarget as NamePlateUnit);
            if (namePlateUnit?.NamePlateController != null && namePlateUnit.NamePlateController.SuppressNamePlate == false) {
                //Debug.Log("PlayerController.SetTarget(): InamePlateUnit is not null");
                uIManager.FocusUnitFrameController.SetTarget(namePlateUnit.NamePlateController);
                namePlateManager.SetFocus(namePlateUnit);
            } else {
                //Debug.Log("PlayerController.SetTarget(): InamePlateUnit is null ???!?");
            }
            newTarget?.UnitComponentController?.HighlightController?.HandleSetTarget();
        }

        /// <summary>
        /// Keep character from doing actions.
        /// </summary>
        void LockAction() {
            canAction = false;
        }

        /// <summary>
        /// Let character move and act again.
        /// </summary>
        void UnLock(bool movement, bool actions) {
            if (movement) {
                UnlockMovement();
            }
            if (actions) {
                canAction = true;
            }
        }

        /// <summary>
        /// Lock character movement and/or action, on a delay for a set time.
        /// </summary>
        /// <param name="lockMovement">If set to <c>true</c> lock movement.</param>
        /// <param name="lockAction">If set to <c>true</c> lock action.</param>
        /// <param name="timed">If set to <c>true</c> timed.</param>
        /// <param name="delayTime">Delay time.</param>
        /// <param name="lockTime">Lock time.</param>
        public void Lock(bool lockMovement, bool lockAction, bool timed, float delayTime, float lockTime) {
            StopCoroutine("_Lock");
            StartCoroutine(_Lock(lockMovement, lockAction, timed, delayTime, lockTime));
        }

        //Timed -1 = infinite, 0 = no, 1 = yes.
        public IEnumerator _Lock(bool lockMovement, bool lockAction, bool timed, float delayTime, float lockTime) {
            if (delayTime > 0) {
                yield return new WaitForSeconds(delayTime);
            }
            if (lockMovement) {
                LockMovement();
            }
            if (lockAction) {
                LockAction();
            }
            if (timed) {
                if (lockTime > 0) {
                    yield return new WaitForSeconds(lockTime);
                }
                UnLock(lockMovement, lockAction);
            }
        }

        public void HandleDie(CharacterStats characterStats) {
            //Debug.Log(gameObject.name + ".PlayerController.HandleDeath()");
            Lock(true, true, false, 0.1f, 0f);
        }

        public void HandleReviveBegin() {
            Lock(true, true, true, 0f, 8.0f);
        }

        //Keep character from moving.
        public void LockMovement() {
            //Debug.Log(gameObject.name + ".PlayerController.LockMovement()");
            canMove = false;
            if (playerManager.ActiveUnitController != null) {
                playerManager.ActiveUnitController.UnitAnimator.SetMoving(false);

                // why do we do this?
                //baseCharacter.UnitController.MyCharacterAnimator.EnableRootMotion();

                if (playerManager.PlayerUnitMovementController != null) {
                    playerManager.PlayerUnitMovementController.localMoveVelocity = new Vector3(0, 0, 0);
                }
            }
        }

        public void UnlockMovement() {
            //Debug.Log(gameObject.name + ".PlayerController.UnlockMovement()");
            canMove = true;

            // why do we do this?
            // is it because this function is never really called ?
            //baseCharacter.UnitController.MyCharacterAnimator.DisableRootMotion();
        }

        public void StopInteract() {
            // the idea of this code is that it will allow us to keep an NPC focused if we back out of range while its interactable popup closes
            // if we don't have anything focused, then we were interacting with someting environmental and definitely want to clear that because it can lead to a hidden target being set
            if (uIManager.FocusUnitFrameController.UnitNamePlateController == null && playerManager.UnitController != null) {
                playerManager.UnitController.ClearTarget();
            }
        }

        public void SubscribeToUnitEvents() {
            //Debug.Log(gameObject.name + ".PlayerController.SubscribeToUnitEvents()");
            
            // if player was agrod at spawn, they may have a target already since we subscribe on model ready
            playerManager.ActiveUnitController.OnSetTarget += HandleSetTarget;
            if (playerManager.ActiveUnitController.Target != null) {
                HandleSetTarget(playerManager.ActiveUnitController.Target);
            }

            playerManager.ActiveUnitController.OnClearTarget += HandleClearTarget;
            playerManager.ActiveUnitController.UnitAnimator.OnStartCasting += HandleStartCasting;
            playerManager.ActiveUnitController.UnitAnimator.OnEndCasting += HandleEndCasting;
            playerManager.ActiveUnitController.UnitAnimator.OnStartAttacking += HandleStartAttacking;
            playerManager.ActiveUnitController.UnitAnimator.OnEndAttacking += HandleEndAttacking;
            playerManager.ActiveUnitController.UnitAnimator.OnStartLevitated += HandleStartLevitated;
            playerManager.ActiveUnitController.UnitAnimator.OnEndLevitated += HandleEndLevitated;
            playerManager.ActiveUnitController.UnitAnimator.OnStartStunned += HandleStartStunned;
            playerManager.ActiveUnitController.UnitAnimator.OnEndStunned += HandleEndStunned;
            playerManager.ActiveUnitController.UnitAnimator.OnStartRevive += HandleStartRevive;
            playerManager.ActiveUnitController.UnitAnimator.OnDeath += HandleDeath;
            playerManager.ActiveUnitController.OnClassChange += HandleClassChange;
            playerManager.ActiveUnitController.OnFactionChange += HandleFactionChange;
            playerManager.ActiveUnitController.OnSpecializationChange += HandleSpecializationChange;
            playerManager.ActiveUnitController.OnActivateMountedState += HandleActivateMountedState;
            playerManager.ActiveUnitController.OnDeActivateMountedState += HandleDeActivateMountedState;
            playerManager.ActiveUnitController.OnMessageFeed += HandleMessageFeed;
            playerManager.ActiveUnitController.OnUnitDestroy += HandleUnitDestroy;
            playerManager.ActiveUnitController.OnCastCancel += HandleCastCancel;

            // subscribe and call in case the namePlate is already spawned
            playerManager.ActiveUnitController.OnInitializeNamePlate += HandleInitializeNamePlate;
            HandleInitializeNamePlate();
        }

        public void UnsubscribeFromUnitEvents() {
            //Debug.Log(gameObject.name + ".PlayerController.UnsubscribeFromUnitEvents()");
            playerManager.ActiveUnitController.OnSetTarget -= HandleSetTarget;
            playerManager.ActiveUnitController.OnClearTarget -= HandleClearTarget;
            playerManager.ActiveUnitController.UnitAnimator.OnStartCasting -= HandleStartCasting;
            playerManager.ActiveUnitController.UnitAnimator.OnEndCasting -= HandleEndCasting;
            playerManager.ActiveUnitController.UnitAnimator.OnStartAttacking -= HandleStartAttacking;
            playerManager.ActiveUnitController.UnitAnimator.OnEndAttacking -= HandleEndAttacking;
            playerManager.ActiveUnitController.UnitAnimator.OnStartLevitated -= HandleStartLevitated;
            playerManager.ActiveUnitController.UnitAnimator.OnEndLevitated -= HandleEndLevitated;
            playerManager.ActiveUnitController.UnitAnimator.OnStartStunned -= HandleStartStunned;
            playerManager.ActiveUnitController.UnitAnimator.OnEndStunned -= HandleEndStunned;
            playerManager.ActiveUnitController.UnitAnimator.OnStartRevive -= HandleStartRevive;
            playerManager.ActiveUnitController.UnitAnimator.OnDeath -= HandleDeath;
            playerManager.ActiveUnitController.OnClassChange -= HandleClassChange;
            playerManager.ActiveUnitController.OnFactionChange -= HandleFactionChange;
            playerManager.ActiveUnitController.OnSpecializationChange -= HandleSpecializationChange;
            playerManager.ActiveUnitController.OnActivateMountedState -= HandleActivateMountedState;
            playerManager.ActiveUnitController.OnDeActivateMountedState -= HandleDeActivateMountedState;
            playerManager.ActiveUnitController.OnMessageFeed -= HandleMessageFeed;
            playerManager.ActiveUnitController.OnInitializeNamePlate -= HandleInitializeNamePlate;
            playerManager.ActiveUnitController.OnUnitDestroy -= HandleUnitDestroy;
            playerManager.ActiveUnitController.OnCastCancel -= HandleCastCancel;

        }

        public void HandleCastCancel(BaseCharacter baseCharacter) {
            //Debug.Log("PlayerController.HandleCastCancel()");
            craftingManager.ClearCraftingQueue();
        }

        public void HandleInitializeNamePlate() {
            //Debug.Log("PlayerController.HandleInitializeNamePlate()");
            if (playerManager?.ActiveUnitController?.NamePlateController?.NamePlate != null) {
                playerManager.ActiveUnitController.NamePlateController.NamePlate.SetPlayerOwnerShip();
            }
        }

        public void HandleUnitDestroy(UnitProfile unitProfile) {
            //Debug.Log(gameObject.name + ".PlayerController.HandleUnitDestroy()");
            SystemEventManager.TriggerEvent("OnPlayerUnitDespawn", new EventParamProperties());
            UnsubscribeFromUnitEvents();
            playerManager.SetUnitController(null);
        }

        public void HandleMessageFeed(string message) {
            messageFeedManager.WriteMessage(message);
        }

        public void HandleActivateMountedState(UnitController mountUnitController) {

            playerManager.ActiveUnitController.UnitAnimator.SetDefaultOverrideController();

            playerManager.SetActiveUnitController(mountUnitController);

            cameraManager.SwitchToMainCamera();
            cameraManager.MainCameraController.InitializeCamera(playerManager.ActiveUnitController.transform);
            if (systemConfigurationManager.UseThirdPartyMovementControl == true) {
                playerManager.EnableMovementControllers();
            }

            EventParamProperties eventParam = new EventParamProperties();
            SystemEventManager.TriggerEvent("OnStartRiding", eventParam);
        }

        public void HandleDeActivateMountedState() {

            if (systemConfigurationManager.UseThirdPartyMovementControl == true) {
                playerManager.DisableMovementControllers();
            }
            playerManager.SetActiveUnitController(playerManager.UnitController);
            if (playerManager.UnitController != null) {
                playerManager.UnitController.UnitAnimator.SetCorrectOverrideController();
            }

            cameraManager.ActivateMainCamera();
            cameraManager.MainCameraController.InitializeCamera(playerManager.ActiveUnitController.transform);

            EventParamProperties eventParam = new EventParamProperties();
            SystemEventManager.TriggerEvent("OnEndRiding", eventParam);

        }

        public void HandleFactionChange(Faction newFaction, Faction oldFaction) {
            SystemEventManager.TriggerEvent("OnFactionChange", new EventParamProperties());
            messageFeedManager.WriteMessage("Changed faction to " + newFaction.DisplayName);
        }

        public void HandleClassChange(CharacterClass newCharacterClass, CharacterClass oldCharacterClass) {
            systemEventManager.NotifyOnClassChange(newCharacterClass, oldCharacterClass);
            messageFeedManager.WriteMessage("Changed class to " + newCharacterClass.DisplayName);
        }

        public void HandleSpecializationChange(ClassSpecialization newSpecialization, ClassSpecialization oldSpecialization) {
            SystemEventManager.TriggerEvent("OnSpecializationChange", new EventParamProperties());
            if (newSpecialization != null) {
                messageFeedManager.WriteMessage("Changed specialization to " + newSpecialization.DisplayName);
            }
        }


        public void HandleDeath() {
            //Debug.Log(gameObject.name + ".PlayerController.HandleDeath()");
            playerManager.ActiveUnitController.UnitAnimator.SetDefaultOverrideController();
            SystemEventManager.TriggerEvent("OnDeath", new EventParamProperties());
        }

        public void HandleStartRevive() {
            playerManager.ActiveUnitController.UnitAnimator.SetDefaultOverrideController();
        }

        public void HandleStartLevitated() {
            playerManager.ActiveUnitController.UnitAnimator.SetDefaultOverrideController();
            EventParamProperties eventParam = new EventParamProperties();
            SystemEventManager.TriggerEvent("OnStartLevitated", eventParam);
        }

        public void HandleEndLevitated(bool swapAnimator) {
            if (swapAnimator) {
                playerManager.ActiveUnitController.UnitAnimator.SetCorrectOverrideController();
                EventParamProperties eventParam = new EventParamProperties();
                SystemEventManager.TriggerEvent("OnEndLevitated", eventParam);
            }
        }

        public void HandleStartStunned() {
            playerManager.ActiveUnitController.UnitAnimator.SetDefaultOverrideController();
            EventParamProperties eventParam = new EventParamProperties();
            SystemEventManager.TriggerEvent("OnStartStunned", eventParam);
        }

        public void HandleEndStunned(bool swapAnimator) {
            if (swapAnimator) {
                playerManager.ActiveUnitController.UnitAnimator.SetCorrectOverrideController();
                EventParamProperties eventParam = new EventParamProperties();
                SystemEventManager.TriggerEvent("OnEndStunned", eventParam);
            }
        }

        public void HandleStartCasting(bool swapAnimator) {
            EventParamProperties eventParam = new EventParamProperties();
            if (swapAnimator == true) {
                playerManager.ActiveUnitController.UnitAnimator.SetDefaultOverrideController();
            }
            SystemEventManager.TriggerEvent("OnStartCasting", eventParam);
        }

        public void HandleEndCasting(bool swapAnimator) {
            EventParamProperties eventParam = new EventParamProperties();
            if (swapAnimator) {
                playerManager.ActiveUnitController.UnitAnimator.SetCorrectOverrideController();
                SystemEventManager.TriggerEvent("OnEndCasting", eventParam);
            }
        }

        public void HandleStartAttacking(bool swapAnimator) {
            EventParamProperties eventParam = new EventParamProperties();
            if (swapAnimator) {
                playerManager.ActiveUnitController.UnitAnimator.SetDefaultOverrideController();
            }
            SystemEventManager.TriggerEvent("OnStartAttacking", eventParam);
        }

        public void HandleEndAttacking(bool swapAnimator) {
            EventParamProperties eventParam = new EventParamProperties();
            if (swapAnimator) {
                if (playerManager.ActiveUnitController != null) {
                    playerManager.ActiveUnitController.UnitAnimator.SetCorrectOverrideController();
                }
                SystemEventManager.TriggerEvent("OnEndAttacking", eventParam);
            }
        }

        public void OnSendObjectToPool() {
            ClearInteractables();
        }

    }

}