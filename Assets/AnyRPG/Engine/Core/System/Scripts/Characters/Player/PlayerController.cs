using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AnyRPG {
    public class PlayerController : MonoBehaviour {

        public event System.Action<int> AbilityButtonPressedHandler = delegate { };
        public event System.Action<bool> ToggleRunHandler = delegate { };

        //Inputs.
        [HideInInspector] public bool inputJump;
        [HideInInspector] public bool inputStrafe;
        //[HideInInspector] public float inputAimVertical = 0;
        //[HideInInspector] public float inputAimHorizontal = 0;
        [HideInInspector] public float inputHorizontal = 0;
        [HideInInspector] public float inputTurn = 0;
        [HideInInspector] public float inputVertical = 0;

        //Variables
        [HideInInspector]
        public bool allowedInput = true;

        public bool canMove = false;
        public bool canAction = false;

        [HideInInspector] public Vector3 NormalizedMoveInput;
        [HideInInspector] public Vector3 TurnInput;
        [HideInInspector] public Vector2 aimInput;

        public LayerMask movementMask;

        public float tabTargetMaxDistance = 20f;

        private List<Interactable> interactables = new List<Interactable>();
        private Interactable mouseOverInteractable = null;

        private int tabTargetIndex = 0;

        private DateTime lastTabTargetTime;

        private RaycastHit mouseOverhit;

        public List<Interactable> MyInteractables { get => interactables; }
        public RaycastHit MyMouseOverhit { get => mouseOverhit; set => mouseOverhit = value; }

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
            ClearInteractables();
        }

        public void ClearInteractables() {
            //Debug.Log(gameObject.name + ".PlayerController.ClearInteractables()");
            interactables.Clear();
        }

        private void ResetMoveInput() {
            inputJump = false;
            inputStrafe = false;
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
            inputJump = InputManager.MyInstance.KeyBindWasPressed("JUMP");
            inputStrafe = InputManager.MyInstance.KeyBindWasPressedOrHeld("STRAFELEFT") || InputManager.MyInstance.KeyBindWasPressedOrHeld("STRAFERIGHT");
            //inputAimVertical = Input.GetAxisRaw("AimVertical");
            //inputAimHorizontal = Input.GetAxisRaw("AimHorizontal");
            inputHorizontal = (InputManager.MyInstance.KeyBindWasPressedOrHeld("STRAFELEFT") ? -1 : 0) + (InputManager.MyInstance.KeyBindWasPressedOrHeld("STRAFERIGHT") ? 1 : 0);
            inputTurn = (InputManager.MyInstance.KeyBindWasPressedOrHeld("TURNLEFT") ? -1 : 0) + (InputManager.MyInstance.KeyBindWasPressedOrHeld("TURNRIGHT") ? 1 : 0);
            inputVertical = (InputManager.MyInstance.KeyBindWasPressedOrHeld("BACK") ? -1 : 0) + (InputManager.MyInstance.KeyBindWasPressedOrHeld("FORWARD") ? 1 : 0);

            NormalizedMoveInput = NormalizedVelocity(new Vector3(inputHorizontal, 0, inputVertical));
            TurnInput = new Vector3(inputTurn, 0, 0);

            if (HasMoveInput()) {
                //Debug.Log("PlayerController.CollectMoveInput(): hasMoveInput");
                PlayerManager.MyInstance.ActiveUnitController.CommonMovementNotifier();
            }
        }

        /*
        private void CollectAimInput() {
            aimInput = new Vector2(inputAimHorizontal, inputAimVertical);
        }
        */

        protected void Update() {
            //Debug.Log("PlayerController.Update()");
            ResetMoveInput();

            if (PlayerManager.MyInstance.ActiveUnitController == null) {
                //Debug.Log(gameObject.name + ".PlayerController.Update(): Player Unit is not spawned. Exiting");
                return;
            }

            if (allowedInput == false) {
                //Debug.Log("Not allowed to Collect Move Input. Exiting PlayerController Update!");
                return;
            }

            //CollectAimInput();

            HandleCancelButtonPressed();

            HandleMouseOver();

            if (PlayerManager.MyInstance?.MyCharacter?.CharacterStats?.IsAlive == false) {
                // can't interact, perform abilities or handle movement when dead
                return;
            }

            // test move this below death check to prevent player getting up after death
            ToggleRun();

            HandleLeftMouseClick();

            RegisterTab();

            // everything below this point cannot be done while control locked
            if (PlayerManager.MyInstance?.ActiveUnitController != null && PlayerManager.MyInstance.ActiveUnitController.ControlLocked == true) {
                return;
            }
            CollectMoveInput();

            HandleRightMouseClick();

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
            if (InputManager.MyInstance.KeyBindWasPressed("TOGGLERUN")) {
                EventParamProperties eventParamProperties = new EventParamProperties();
                if (PlayerManager.MyInstance.ActiveUnitController.Walking == false) {
                    PlayerManager.MyInstance.ActiveUnitController.Walking = true;
                    eventParamProperties.simpleParams.BoolParam = true;
                } else {
                    PlayerManager.MyInstance.ActiveUnitController.Walking = false;
                    eventParamProperties.simpleParams.BoolParam = false;
                }
                SystemEventManager.TriggerEvent("OnToggleRun", eventParamProperties);
                MessageFeedManager.MyInstance.WriteMessage("Walk: " + PlayerManager.MyInstance.ActiveUnitController.Walking.ToString());
                ToggleRunHandler(PlayerManager.MyInstance.ActiveUnitController.Walking);
            }
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
            if (CameraManager.MyInstance.MyActiveMainCamera == null) {
                // we are in a cutscene and shouldn't be dealing with mouseover
                return;
            }
            Ray ray = CameraManager.MyInstance.MyActiveMainCamera.ScreenPointToRay(Input.mousePosition);
            int playerMask = 1 << LayerMask.NameToLayer("Player");
            int ignoreMask = 1 << LayerMask.NameToLayer("Ignore Raycast");
            int spellMask = 1 << LayerMask.NameToLayer("SpellEffects");
            int waterMask = 1 << LayerMask.NameToLayer("Water");
            int layerMask = ~(playerMask | ignoreMask | spellMask | waterMask);

            bool disableMouseOver = false;
            bool mouseOverNamePlate = false;
            if (NamePlateManager.MyInstance != null) {
                mouseOverNamePlate = NamePlateManager.MyInstance.MouseOverNamePlate();
            }

            if (!EventSystem.current.IsPointerOverGameObject() && !mouseOverNamePlate) {
                if (Physics.Raycast(ray, out mouseOverhit, 100, layerMask)) {
                    // prevent clicking on mount
                    if (mouseOverhit.collider.gameObject != PlayerManager.MyInstance.ActiveUnitController.gameObject) {
                        Interactable newInteractable = mouseOverhit.collider.GetComponent<Interactable>();
                        if (newInteractable == null) {
                            newInteractable = mouseOverhit.collider.GetComponentInParent<Interactable>();
                        }
                        //Debug.Log("We hit " + mouseOverhit.collider.name + " " + mouseOverhit.point + "; old: " + (mouseOverInteractable != null ? mouseOverInteractable.MyName : "null") + "; new: " + (newInteractable != null ? newInteractable.MyName : "null"));

                        if (mouseOverInteractable != null && mouseOverInteractable != newInteractable) {
                            // since we hit something, and our existing thing was not null, we have to exit the old one
                            //Debug.Log("We hit " + mouseOverhit.collider.name + " " + mouseOverhit.point + "; old: " + (mouseOverInteractable != null ? mouseOverInteractable.MyName : "null")+ "; new: " + (newInteractable != null ? newInteractable.MyName : "null" ));
                            mouseOverInteractable.IsMouseOverUnit = false;
                            mouseOverInteractable.OnMouseOut();
                        }

                        if (newInteractable != null && mouseOverInteractable != newInteractable) {
                            // we have a new interactable, activate mouseover
                            //Debug.Log("We hit " + mouseOverhit.collider.name + " " + mouseOverhit.point + " and it had an interactable.  activating mouseover");
                            newInteractable.IsMouseOverUnit = true;
                            newInteractable.OnMouseIn();
                        }
                        mouseOverInteractable = newInteractable;
                    }
                }
            } else {
                disableMouseOver = true;
                //Debug.Log(gameObject.name + ".PlayerController.HandleMouseOver(): mouseovernameplate: " + NamePlateManager.MyInstance.MouseOverNamePlate() + "; pointerovergameobject: " + EventSystem.current.IsPointerOverGameObject());
            }

            if (disableMouseOver) {
                // we did not hit any interactable, check if a current interactable is set and unset it
                if (mouseOverInteractable != null) {
                    mouseOverInteractable.IsMouseOverUnit = false;
                    mouseOverInteractable.OnMouseOut();
                    mouseOverInteractable = null;
                }
            }

        }

        /*
        public void HandleMouseOver(Interactable newInteractable) {
            //Debug.Log(gameObject.name + ".PlayerController.HandleMouseOver()");
            if (CameraManager.MyInstance.MyActiveMainCamera == null) {
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
            if (InputManager.MyInstance.rightMouseButtonClicked && !EventSystem.current.IsPointerOverGameObject()) {
                //Debug.Log(gameObject.name + ".PlayerController.HandleRightMouseClick(): !EventSystem.current.IsPointerOverGameObject() == true!!!");


                if (mouseOverInteractable != null && mouseOverInteractable.IsTrigger == false) {
                    //Debug.Log("setting interaction target to " + hit.collider.gameObject.name);
                    //interactionTarget = hit.collider.gameObject;
                    InterActWithTarget(mouseOverInteractable);
                }
                //Debug.Log("We hit " + hit.collider.name + " " + hit.point);
            }
        }

        private void HandleLeftMouseClick() {
            //Debug.Log("PlayerController.HandleLeftMouseClick()");
            // Check if the left mouse button clicked on an interactable and focus it
            if (!InputManager.MyInstance.leftMouseButtonClicked) {
                return;
            }

            if (CameraManager.MyInstance.MyActiveMainCamera == null) {
                // probably in a cutscene.  don't respond to clicks on objects if there is no camera following the player
                return;
            }

            if (EventSystem.current.IsPointerOverGameObject() && !NamePlateManager.MyInstance.MouseOverNamePlate()) {
                //Debug.Log("PlayerController.HandleLeftMouseClick(): clicked over UI and not nameplate.  exiting");
                return;
            }

            //if (InputManager.MyInstance.leftMouseButtonClicked && !EventSystem.current.IsPointerOverGameObject()) {
            if (mouseOverInteractable == null && !NamePlateManager.MyInstance.MouseOverNamePlate()) {
                // Stop focusing any object
                //RemoveFocus();
                PlayerManager.MyInstance.UnitController.ClearTarget();
            } else if (mouseOverInteractable != null) {
                PlayerManager.MyInstance.UnitController.SetTarget(mouseOverInteractable);
            }
            //}

            Ray ray = CameraManager.MyInstance.MyActiveMainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100, movementMask)) {
                if (PlayerManager.MyInstance.ActiveCharacter.CharacterAbilityManager.WaitingForTarget()) {
                    PlayerManager.MyInstance.ActiveCharacter.CharacterAbilityManager.SetGroundTarget(hit.point);
                }
            }
        }

        /// <summary>
        /// if an interactable is set, try to interact with it if it's in range.
        /// </summary>
        private void CheckForInteraction() {
            Debug.Log(gameObject.name + ".PlayerController.CheckForInteraction()");

            if (PlayerManager.MyInstance.UnitController == null) {
                return;
            }
            if (PlayerManager.MyInstance.UnitController.Target == null) {
                return;
            }
            if (InteractionSucceeded()) {
                if (PlayerManager.MyInstance.ActiveUnitController != null && PlayerManager.MyInstance.ActiveUnitController.UnitMotor != null) {
                    PlayerManager.MyInstance.ActiveUnitController.UnitMotor.StopFollowingTarget();
                }
            }
        }

        private bool InteractionSucceeded() {
            Debug.Log(gameObject.name + ".PlayerController.InteractionSucceeded()");

            if (PlayerManager.MyInstance.UnitController == null) {
                return false;
            }
            if (PlayerManager.MyInstance.UnitController.Target == null) {
                //Debug.Log(gameObject.name + ".PlayerController.InteractionSucceeded(): target is null. return false.");
                return false;
            }
            //if (IsTargetInHitBox(target)) {
            // get reference to name now since interactable could change scene and then target reference is lost
            string targetDisplayName = PlayerManager.MyInstance.UnitController.Target.DisplayName;
            if (PlayerManager.MyInstance.UnitController.Target.Interact(PlayerManager.MyInstance.ActiveUnitController.CharacterUnit, true)) {
                //Debug.Log(gameObject.name + ".PlayerController.InteractionSucceeded(): Interaction Succeeded.  Setting interactable to null");
                SystemEventManager.MyInstance.NotifyOnInteractionStarted(targetDisplayName);
                return true;
            }
            //Debug.Log(gameObject.name + ".PlayerController.InteractionSucceeded(): returning false");

            return false;
            //}
            //return false;
        }

        private void RegisterTab() {
            if (InputManager.MyInstance.KeyBindWasPressed("NEXTTARGET")) {
                //Debug.Log("Tab Target Registered");
                Interactable oldTarget = PlayerManager.MyInstance.UnitController.Target;
                // moving this inside getnexttabtarget
                //PlayerManager.MyInstance.UnitController.ClearTarget();
                GetNextTabTarget(oldTarget);
            }
        }

        private void GetNextTabTarget(Interactable oldTarget) {
            //Debug.Log("PlayerController.GetNextTabTarget(): maxDistance: " + tabTargetMaxDistance);
            DateTime currentTime = DateTime.Now;
            TimeSpan timeSinceLastTab = currentTime - lastTabTargetTime;
            lastTabTargetTime = DateTime.Now;
            int validMask = 1 << LayerMask.NameToLayer("CharacterUnit");
            Collider[] hitColliders = Physics.OverlapSphere(PlayerManager.MyInstance.ActiveUnitController.transform.position, tabTargetMaxDistance, validMask);
            int i = 0;
            //Debug.Log("GetNextTabTarget(): collider length: " + hitColliders.Length + "; index: " + tabTargetIndex);
            int preferredTargetIndex = -1;
            int closestTargetIndex = -1;

            // although the layermask on the collider should have only delivered us valid characterUnits, they may be dead or friendly.  We need to put all the valid attack targets in a list first
            List<UnitController> characterUnitList = new List<UnitController>();
            foreach (Collider hitCollider in hitColliders) {
                //Debug.Log("GetNextTabTarget(): collider length: " + hitColliders.Length);
                GameObject collidedGameObject = hitCollider.gameObject;
                UnitController targetCharacterUnit = collidedGameObject.GetComponent<UnitController>();
                if (targetCharacterUnit != null && targetCharacterUnit.CharacterUnit.BaseCharacter.CharacterStats.IsAlive == true && Faction.RelationWith(targetCharacterUnit.CharacterUnit.BaseCharacter, PlayerManager.MyInstance.MyCharacter.Faction) <= -1) {

                    // check if the unit is actually in front of our character.
                    // not doing any cone or angles for now, anywhere in front will do.  might adjust this a bit later to prevent targetting units nearly adjacent to us and far away
                    Vector3 transformedPosition = PlayerManager.MyInstance.ActiveUnitController.transform.InverseTransformPoint(collidedGameObject.transform.position);
                    if (transformedPosition.z > 0f) {
                        characterUnitList.Add(targetCharacterUnit);

                    }
                }
            }

            if (characterUnitList.Count == 0) {
                // no valid characters in range
                //Debug.Log("PlayerController.GetNextTabTarget(): no valid characters in range, returning");
                return;
            } else {
                //Debug.Log("PlayerController.GetNextTabTarget(): valid character count: " + characterUnitList.Count);
            }

            // now that we have all valid attack targets, we need to process the list a bit before choosing a target
            i = 0;
            foreach (UnitController collidedGameObject in characterUnitList) {
                //Debug.Log("PlayerController.GetNextTabTarget(): processing target: " + i + "; " + collidedGameObject.name);
                if (closestTargetIndex == -1) {
                    closestTargetIndex = i;
                }
                if (Vector3.Distance(PlayerManager.MyInstance.ActiveUnitController.transform.position, collidedGameObject.transform.position) < Vector3.Distance(PlayerManager.MyInstance.ActiveUnitController.transform.position, characterUnitList[closestTargetIndex].transform.position)) {
                    closestTargetIndex = i;
                }
                // this next variable shouldn't actually be needed.  i think it was a logic error with not tracking the target index properly
                if (preferredTargetIndex == -1) {
                    preferredTargetIndex = i;
                }
                i++;
            }


            tabTargetIndex++;
            if (tabTargetIndex >= characterUnitList.Count) {
                tabTargetIndex = 0;
            }
            //Debug.Log("PlayerController.GetNextTabTarget(): processing complete: closestTargetIndex: " + closestTargetIndex + "; target: " + (target == null ? "null" : target.name) + "; closestTargetName: " + characterUnitList[closestTargetIndex]);

            // reset to closest unit every 3 seconds if starting a new round of tabbing.
            // otherwise, just keep going through the index
            if (timeSinceLastTab.TotalSeconds > 3f) {
                //Debug.Log("PlayerController.GetNextTabTarget(): More than 3 seconds since last tab");
                if (closestTargetIndex != -1 && characterUnitList[closestTargetIndex] != PlayerManager.MyInstance.UnitController.Target) {
                    // prevent a tab from re-targetting the same unit just because it's closest to us
                    // we only want to clear the target if we are actually setting a new target
                    PlayerManager.MyInstance.UnitController.ClearTarget();
                    PlayerManager.MyInstance.UnitController.SetTarget(characterUnitList[closestTargetIndex]);
                    // we need to manually set this here, otherwise our tab target index won't match our actual target, resulting in the next tab possibly not switching to a new target
                    tabTargetIndex = closestTargetIndex;
                    //} else if (preferredTarget != null) {
                } else {
                    if (characterUnitList[tabTargetIndex] != PlayerManager.MyInstance.UnitController.Target) {
                        // we only want to clear the target if we are actually setting a new target
                        PlayerManager.MyInstance.UnitController.ClearTarget();
                        PlayerManager.MyInstance.UnitController.SetTarget(characterUnitList[tabTargetIndex]);
                    }
                }
            } else {
                //Debug.Log("PlayerController.GetNextTabTarget(): Less than 3 seconds since last tab, using index: " + tabTargetIndex);
                // we only want to clear the target if we are actually setting a new target
                if (characterUnitList[tabTargetIndex] != PlayerManager.MyInstance.UnitController.Target) {
                    PlayerManager.MyInstance.UnitController.ClearTarget();
                    PlayerManager.MyInstance.UnitController.SetTarget(characterUnitList[tabTargetIndex]);
                }
            }
        }

        public void InterActWithTarget(Interactable interactable) {
            Debug.Log(gameObject.name + ".InterActWithTarget(" + interactable.gameObject.name + ")");
            if (PlayerManager.MyInstance.UnitController.Target != interactable) {
                PlayerManager.MyInstance.UnitController.ClearTarget();
                PlayerManager.MyInstance.UnitController.SetTarget(interactable);
            }
            if (InteractionSucceeded()) {
                //Debug.Log("We were able to interact with the target");
                // not actually stopping interacting.  just clearing target if this was a trigger interaction and we are not interacting with a focus
                StopInteract();
            } else {
                //Debug.Log("we were out of range and must move toward the target to be able to interact with it");
                if (PlayerManager.MyInstance.PlayerUnitMovementController.useMeshNav) {
                    //Debug.Log("Nav Mesh Agent is enabled. Setting follow target: " + target.name);
                    PlayerManager.MyInstance.ActiveUnitController.UnitMotor.FollowTarget(PlayerManager.MyInstance.UnitController.Target);
                } else {
                    //Debug.Log("Nav Mesh Agent is disabled and you are out of range");
                }
            }
        }

        public void InterActWithInteractableOption(Interactable interactable, InteractableOptionComponent interactableOption) {
            //Debug.Log(gameObject.name + ".InterActWithTarget(" + interactable.MyName + ", " + _gameObject.name.ToString() + ")");
            PlayerManager.MyInstance.UnitController.SetTarget(interactable);
            if (interactable == null) {
                //Debug.Log(gameObject.name + ".PlayerController.InteractWithTarget(): interactable is null!!!");
            }
            if (InteractionWithOptionSucceeded(interactableOption)) {
                //Debug.Log("We were able to interact with the target");
                // not actually stopping interacting.  just clearing target if this was a trigger interaction and we are not interacting with a focus
                StopInteract();
            } else {
                //Debug.Log("we were out of range and must move toward the target to be able to interact with it");
                if (PlayerManager.MyInstance.PlayerUnitMovementController.useMeshNav) {
                    //Debug.Log("Nav Mesh Agent is enabled. Setting follow target: " + target.name);
                    PlayerManager.MyInstance.ActiveUnitController.UnitMotor.FollowTarget(PlayerManager.MyInstance.UnitController.Target);
                } else {
                    //Debug.Log("Nav Mesh Agent is disabled and you are out of range");
                }
            }
        }

        private bool InteractionWithOptionSucceeded(InteractableOptionComponent interactableOption) {
            //Debug.Log(gameObject.name + ".PlayerController.InteractionSucceeded()");
            //if (IsTargetInHitBox(target)) {
            if (interactableOption.Interact(PlayerManager.MyInstance.ActiveUnitController.CharacterUnit)) {
                //Debug.Log(gameObject.name + ".PlayerController.InteractionSucceeded(): Interaction Succeeded.  Setting interactable to null");
                SystemEventManager.MyInstance.NotifyOnInteractionStarted(PlayerManager.MyInstance.UnitController.Target.DisplayName);
                SystemEventManager.MyInstance.NotifyOnInteractionWithOptionStarted(interactableOption);
                // no longer needed since targeting is changed and we don't want to lose target in the middle of attacking
                //PlayerManager.MyInstance.ActiveUnitController.SetTarget(null);
                return true;
            }
            //Debug.Log(gameObject.name + ".PlayerController.InteractionSucceeded(): returning false");
            return false;
            //}
            //return false;
        }



        /// <summary>
        /// Remove an interactable from the list of interactables in range
        /// </summary>
        /// <param name="_interactable"></param>
        public void RemoveInteractable(Interactable _interactable) {
            if (interactables.Contains(_interactable)) {
                interactables.Remove(_interactable);
            }
        }

        private void HandleCancelButtonPressed() {
            //Debug.Log("HandleCancelButtonPressed()");
            if (InputManager.MyInstance.KeyBindWasPressed("CANCEL")) {
                PlayerManager.MyInstance.UnitController.ClearTarget();
                if (PlayerManager.MyInstance.ActiveCharacter.CharacterStats.IsAlive != false) {
                    // prevent character from swapping to third party controller while dead
                    PlayerManager.MyInstance.ActiveCharacter.CharacterAbilityManager.StopCasting();
                }
                PlayerManager.MyInstance.ActiveCharacter.CharacterAbilityManager.DeActivateTargettingMode();
            }
        }

        public void RegisterAbilityButtonPresses() {
            //Debug.Log("PlayerController.RegisterAbilityButtonPresses()");
            foreach (KeyBindNode keyBindNode in KeyBindManager.MyInstance.MyKeyBinds.Values) {
                //Debug.Log("PlayerController.RegisterAbilityButtonPresses() keyBindNode.GetKeyDown: " + keyBindNode.GetKeyDown);
                //Debug.Log("PlayerController.RegisterAbilityButtonPresses() keyBindNode.GetKeyDown: " + keyBindNode.GetKey);
                if (keyBindNode.MyKeyBindType == KeyBindType.Action && InputManager.MyInstance.KeyBindWasPressed(keyBindNode.MyKeyBindID) == true) {
                    //Debug.Log("PlayerController.RegisterAbilityButtonPresses(): key pressed: " + keyBindNode.MyKeyCode.ToString());
                    keyBindNode.MyActionButton.OnClick(true);
                }
            }
        }

        public void HandleClearTarget(Interactable oldTarget) {
            //Debug.Log("PlayerController.HandleClearTarget()");

            UIManager.MyInstance.FocusUnitFrameController.ClearTarget();
            NamePlateManager.MyInstance.ClearFocus();
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
                UIManager.MyInstance.FocusUnitFrameController.SetTarget(namePlateUnit.NamePlateController);
                NamePlateManager.MyInstance.SetFocus(namePlateUnit);
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
            if (PlayerManager.MyInstance.ActiveUnitController != null) {
                PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.SetMoving(false);

                // why do we do this?
                //baseCharacter.UnitController.MyCharacterAnimator.EnableRootMotion();

                if (PlayerManager.MyInstance.PlayerUnitMovementController != null) {
                    PlayerManager.MyInstance.PlayerUnitMovementController.currentMoveVelocity = new Vector3(0, 0, 0);
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
            if (UIManager.MyInstance.FocusUnitFrameController.UnitNamePlateController == null && PlayerManager.MyInstance.UnitController != null) {
                PlayerManager.MyInstance.UnitController.ClearTarget();
            }
        }

        public void SubscribeToUnitEvents() {
            //Debug.Log("PlayerController.SubscribeToUnitEvents()");
            
            // if player was agrod at spawn, they may have a target already since we subscribe on model ready
            PlayerManager.MyInstance.ActiveUnitController.OnSetTarget += HandleSetTarget;
            if (PlayerManager.MyInstance.ActiveUnitController.Target != null) {
                HandleSetTarget(PlayerManager.MyInstance.ActiveUnitController.Target);
            }

            PlayerManager.MyInstance.ActiveUnitController.OnClearTarget += HandleClearTarget;
            PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.OnStartCasting += HandleStartCasting;
            PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.OnEndCasting += HandleEndCasting;
            PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.OnStartAttacking += HandleStartAttacking;
            PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.OnEndAttacking += HandleEndAttacking;
            PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.OnStartLevitated += HandleStartLevitated;
            PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.OnEndLevitated += HandleEndLevitated;
            PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.OnStartStunned += HandleStartStunned;
            PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.OnEndStunned += HandleEndStunned;
            PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.OnStartRevive += HandleStartRevive;
            PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.OnDeath += HandleDeath;
            PlayerManager.MyInstance.ActiveUnitController.OnClassChange += HandleClassChange;
            PlayerManager.MyInstance.ActiveUnitController.OnFactionChange += HandleFactionChange;
            PlayerManager.MyInstance.ActiveUnitController.OnSpecializationChange += HandleSpecializationChange;
            PlayerManager.MyInstance.ActiveUnitController.OnActivateMountedState += HandleActivateMountedState;
            PlayerManager.MyInstance.ActiveUnitController.OnDeActivateMountedState += HandleDeActivateMountedState;
            PlayerManager.MyInstance.ActiveUnitController.OnMessageFeed += HandleMessageFeed;
            PlayerManager.MyInstance.ActiveUnitController.OnUnitDestroy += HandleUnitDestroy;

            // subscribe and call in case the namePlate is already spawned
            PlayerManager.MyInstance.ActiveUnitController.OnInitializeNamePlate += HandleInitializeNamePlate;
            HandleInitializeNamePlate();
        }

        public void UnsubscribeFromUnitEvents() {
            PlayerManager.MyInstance.ActiveUnitController.OnSetTarget -= HandleSetTarget;
            PlayerManager.MyInstance.ActiveUnitController.OnClearTarget -= HandleClearTarget;
            PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.OnStartCasting -= HandleStartCasting;
            PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.OnEndCasting -= HandleEndCasting;
            PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.OnStartAttacking -= HandleStartAttacking;
            PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.OnEndAttacking -= HandleEndAttacking;
            PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.OnStartLevitated -= HandleStartLevitated;
            PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.OnEndLevitated -= HandleEndLevitated;
            PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.OnStartStunned -= HandleStartStunned;
            PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.OnEndStunned -= HandleEndStunned;
            PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.OnStartRevive -= HandleStartRevive;
            PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.OnDeath -= HandleDeath;
            PlayerManager.MyInstance.ActiveUnitController.OnClassChange -= HandleClassChange;
            PlayerManager.MyInstance.ActiveUnitController.OnFactionChange -= HandleFactionChange;
            PlayerManager.MyInstance.ActiveUnitController.OnSpecializationChange -= HandleSpecializationChange;
            PlayerManager.MyInstance.ActiveUnitController.OnActivateMountedState -= HandleActivateMountedState;
            PlayerManager.MyInstance.ActiveUnitController.OnDeActivateMountedState -= HandleDeActivateMountedState;
            PlayerManager.MyInstance.ActiveUnitController.OnMessageFeed -= HandleMessageFeed;
            PlayerManager.MyInstance.ActiveUnitController.OnInitializeNamePlate -= HandleInitializeNamePlate;
            PlayerManager.MyInstance.ActiveUnitController.OnUnitDestroy -= HandleUnitDestroy;

        }

        public void HandleInitializeNamePlate() {
            //Debug.Log("PlayerController.HandleInitializeNamePlate()");
            if (PlayerManager.MyInstance?.ActiveUnitController?.NamePlateController?.NamePlate != null) {
                PlayerManager.MyInstance.ActiveUnitController.NamePlateController.NamePlate.SetPlayerOwnerShip();
            }
        }

        public void HandleUnitDestroy(UnitProfile unitProfile) {
            //Debug.Log("PlayerController.HandleUnitDestroy()");
            SystemEventManager.TriggerEvent("OnPlayerUnitDespawn", new EventParamProperties());
            UnsubscribeFromUnitEvents();
            PlayerManager.MyInstance.SetUnitController(null);
        }

        public void HandleMessageFeed(string message) {
            MessageFeedManager.MyInstance.WriteMessage(message);
        }

        public void HandleActivateMountedState(UnitController mountUnitController) {

            PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.SetDefaultOverrideController();

            PlayerManager.MyInstance.SetActiveUnitController(mountUnitController);

            CameraManager.MyInstance.SwitchToMainCamera();
            CameraManager.MyInstance.MainCameraController.InitializeCamera(PlayerManager.MyInstance.ActiveUnitController.transform);
            if (SystemConfigurationManager.Instance.UseThirdPartyMovementControl == true) {
                PlayerManager.MyInstance.EnableMovementControllers();
            }

            EventParamProperties eventParam = new EventParamProperties();
            SystemEventManager.TriggerEvent("OnStartRiding", eventParam);
        }

        public void HandleDeActivateMountedState() {

            if (SystemConfigurationManager.Instance.UseThirdPartyMovementControl == true) {
                PlayerManager.MyInstance.DisableMovementControllers();
            }
            PlayerManager.MyInstance.SetActiveUnitController(PlayerManager.MyInstance.UnitController);
            if (PlayerManager.MyInstance.UnitController != null) {
                PlayerManager.MyInstance.UnitController.UnitAnimator.SetCorrectOverrideController();
            }

            CameraManager.MyInstance.ActivateMainCamera();
            CameraManager.MyInstance.MainCameraController.InitializeCamera(PlayerManager.MyInstance.ActiveUnitController.transform);

            EventParamProperties eventParam = new EventParamProperties();
            SystemEventManager.TriggerEvent("OnEndRiding", eventParam);

        }

        public void HandleFactionChange(Faction newFaction, Faction oldFaction) {
            SystemEventManager.TriggerEvent("OnFactionChange", new EventParamProperties());
            MessageFeedManager.MyInstance.WriteMessage("Changed faction to " + newFaction.DisplayName);
        }

        public void HandleClassChange(CharacterClass newCharacterClass, CharacterClass oldCharacterClass) {
            SystemEventManager.MyInstance.NotifyOnClassChange(newCharacterClass, oldCharacterClass);
            MessageFeedManager.MyInstance.WriteMessage("Changed class to " + newCharacterClass.DisplayName);
        }

        public void HandleSpecializationChange(ClassSpecialization newSpecialization, ClassSpecialization oldSpecialization) {
            SystemEventManager.TriggerEvent("OnSpecializationChange", new EventParamProperties());
            if (newSpecialization != null) {
                MessageFeedManager.MyInstance.WriteMessage("Changed specialization to " + newSpecialization.DisplayName);
            }
        }


        public void HandleDeath() {
            //Debug.Log(gameObject.name + ".PlayerController.HandleDeath()");
            PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.SetDefaultOverrideController();
            SystemEventManager.TriggerEvent("OnDeath", new EventParamProperties());
        }

        public void HandleStartRevive() {
            PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.SetDefaultOverrideController();
        }

        public void HandleStartLevitated() {
            PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.SetDefaultOverrideController();
            EventParamProperties eventParam = new EventParamProperties();
            SystemEventManager.TriggerEvent("OnStartLevitated", eventParam);
        }

        public void HandleEndLevitated(bool swapAnimator) {
            if (swapAnimator) {
                PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.SetCorrectOverrideController();
                EventParamProperties eventParam = new EventParamProperties();
                SystemEventManager.TriggerEvent("OnEndLevitated", eventParam);
            }
        }

        public void HandleStartStunned() {
            PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.SetDefaultOverrideController();
            EventParamProperties eventParam = new EventParamProperties();
            SystemEventManager.TriggerEvent("OnStartStunned", eventParam);
        }

        public void HandleEndStunned(bool swapAnimator) {
            if (swapAnimator) {
                PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.SetCorrectOverrideController();
                EventParamProperties eventParam = new EventParamProperties();
                SystemEventManager.TriggerEvent("OnEndStunned", eventParam);
            }
        }

        public void HandleStartCasting(bool swapAnimator) {
            EventParamProperties eventParam = new EventParamProperties();
            if (swapAnimator == true) {
                PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.SetDefaultOverrideController();
            }
            SystemEventManager.TriggerEvent("OnStartCasting", eventParam);
        }

        public void HandleEndCasting(bool swapAnimator) {
            EventParamProperties eventParam = new EventParamProperties();
            if (swapAnimator) {
                PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.SetCorrectOverrideController();
                SystemEventManager.TriggerEvent("OnEndCasting", eventParam);
            }
        }

        public void HandleStartAttacking(bool swapAnimator) {
            EventParamProperties eventParam = new EventParamProperties();
            if (swapAnimator) {
                PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.SetDefaultOverrideController();
            }
            SystemEventManager.TriggerEvent("OnStartAttacking", eventParam);
        }

        public void HandleEndAttacking(bool swapAnimator) {
            EventParamProperties eventParam = new EventParamProperties();
            if (swapAnimator) {
                if (PlayerManager.MyInstance.ActiveUnitController != null) {
                    PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.SetCorrectOverrideController();
                }
                SystemEventManager.TriggerEvent("OnEndAttacking", eventParam);
            }
        }

    }

}