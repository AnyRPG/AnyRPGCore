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
            inputJump = InputManager.Instance.KeyBindWasPressed("JUMP");
            inputStrafe = InputManager.Instance.KeyBindWasPressedOrHeld("STRAFELEFT") || InputManager.Instance.KeyBindWasPressedOrHeld("STRAFERIGHT");
            //inputAimVertical = Input.GetAxisRaw("AimVertical");
            //inputAimHorizontal = Input.GetAxisRaw("AimHorizontal");
            inputHorizontal = (InputManager.Instance.KeyBindWasPressedOrHeld("STRAFELEFT") ? -1 : 0) + (InputManager.Instance.KeyBindWasPressedOrHeld("STRAFERIGHT") ? 1 : 0);
            inputTurn = (InputManager.Instance.KeyBindWasPressedOrHeld("TURNLEFT") ? -1 : 0) + (InputManager.Instance.KeyBindWasPressedOrHeld("TURNRIGHT") ? 1 : 0);
            inputVertical = (InputManager.Instance.KeyBindWasPressedOrHeld("BACK") ? -1 : 0) + (InputManager.Instance.KeyBindWasPressedOrHeld("FORWARD") ? 1 : 0);

            NormalizedMoveInput = NormalizedVelocity(new Vector3(inputHorizontal, 0, inputVertical));
            TurnInput = new Vector3(inputTurn, 0, 0);

            if (HasMoveInput()) {
                //Debug.Log("PlayerController.CollectMoveInput(): hasMoveInput");
                PlayerManager.Instance.ActiveUnitController.CommonMovementNotifier();
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

            if (PlayerManager.Instance.ActiveUnitController == null) {
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

            if (PlayerManager.Instance?.MyCharacter?.CharacterStats?.IsAlive == false) {
                // can't interact, perform abilities or handle movement when dead
                return;
            }

            // test move this below death check to prevent player getting up after death
            ToggleRun();

            HandleLeftMouseClick();

            RegisterTab();

            // everything below this point cannot be done while control locked
            if (PlayerManager.Instance?.ActiveUnitController != null && PlayerManager.Instance.ActiveUnitController.ControlLocked == true) {
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
            if (InputManager.Instance.KeyBindWasPressed("TOGGLERUN")) {
                EventParamProperties eventParamProperties = new EventParamProperties();
                if (PlayerManager.Instance.ActiveUnitController.Walking == false) {
                    PlayerManager.Instance.ActiveUnitController.Walking = true;
                    eventParamProperties.simpleParams.BoolParam = true;
                } else {
                    PlayerManager.Instance.ActiveUnitController.Walking = false;
                    eventParamProperties.simpleParams.BoolParam = false;
                }
                SystemEventManager.TriggerEvent("OnToggleRun", eventParamProperties);
                SystemGameManager.Instance.UIManager.MessageFeedManager.WriteMessage("Walk: " + PlayerManager.Instance.ActiveUnitController.Walking.ToString());
                ToggleRunHandler(PlayerManager.Instance.ActiveUnitController.Walking);
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
            if (SystemGameManager.Instance.CameraManager.ActiveMainCamera == null) {
                // we are in a cutscene and shouldn't be dealing with mouseover
                return;
            }
            Ray ray = SystemGameManager.Instance.CameraManager.ActiveMainCamera.ScreenPointToRay(Input.mousePosition);
            int playerMask = 1 << LayerMask.NameToLayer("Player");
            int ignoreMask = 1 << LayerMask.NameToLayer("Ignore Raycast");
            int spellMask = 1 << LayerMask.NameToLayer("SpellEffects");
            int waterMask = 1 << LayerMask.NameToLayer("Water");
            int layerMask = ~(playerMask | ignoreMask | spellMask | waterMask);

            bool disableMouseOver = false;
            bool mouseOverNamePlate = false;
            if (SystemGameManager.Instance.UIManager.NamePlateManager != null) {
                mouseOverNamePlate = SystemGameManager.Instance.UIManager.NamePlateManager.MouseOverNamePlate();
            }

            if (!EventSystem.current.IsPointerOverGameObject() && !mouseOverNamePlate) {
                if (Physics.Raycast(ray, out mouseOverhit, 100, layerMask)) {
                    // prevent clicking on mount
                    if (mouseOverhit.collider.gameObject != PlayerManager.Instance.ActiveUnitController.gameObject) {
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
                //Debug.Log(gameObject.name + ".PlayerController.HandleMouseOver(): mouseovernameplate: " + SystemGameManager.Instance.UIManager.NamePlateManager.MouseOverNamePlate() + "; pointerovergameobject: " + EventSystem.current.IsPointerOverGameObject());
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
            if (SystemGameManager.Instance.CameraManager.MyActiveMainCamera == null) {
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
            if (InputManager.Instance.rightMouseButtonClicked && !EventSystem.current.IsPointerOverGameObject()) {
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
            if (!InputManager.Instance.leftMouseButtonClicked) {
                return;
            }

            if (SystemGameManager.Instance.CameraManager.ActiveMainCamera == null) {
                // probably in a cutscene.  don't respond to clicks on objects if there is no camera following the player
                return;
            }

            if (EventSystem.current.IsPointerOverGameObject() && !SystemGameManager.Instance.UIManager.NamePlateManager.MouseOverNamePlate()) {
                //Debug.Log("PlayerController.HandleLeftMouseClick(): clicked over UI and not nameplate.  exiting");
                return;
            }

            //if (InputManager.Instance.leftMouseButtonClicked && !EventSystem.current.IsPointerOverGameObject()) {
            if (mouseOverInteractable == null && !SystemGameManager.Instance.UIManager.NamePlateManager.MouseOverNamePlate()) {
                // Stop focusing any object
                //RemoveFocus();
                PlayerManager.Instance.UnitController.ClearTarget();
            } else if (mouseOverInteractable != null) {
                PlayerManager.Instance.UnitController.SetTarget(mouseOverInteractable);
            }
            //}

            Ray ray = SystemGameManager.Instance.CameraManager.ActiveMainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100, movementMask)) {
                if (PlayerManager.Instance.ActiveCharacter.CharacterAbilityManager.WaitingForTarget()) {
                    PlayerManager.Instance.ActiveCharacter.CharacterAbilityManager.SetGroundTarget(hit.point);
                }
            }
        }

        /// <summary>
        /// if an interactable is set, try to interact with it if it's in range.
        /// </summary>
        private void CheckForInteraction() {
            //Debug.Log(gameObject.name + ".PlayerController.CheckForInteraction()");

            if (PlayerManager.Instance.UnitController == null) {
                return;
            }
            if (PlayerManager.Instance.UnitController.Target == null) {
                return;
            }
            if (InteractionSucceeded()) {
                if (PlayerManager.Instance.ActiveUnitController != null && PlayerManager.Instance.ActiveUnitController.UnitMotor != null) {
                    PlayerManager.Instance.ActiveUnitController.UnitMotor.StopFollowingTarget();
                }
            }
        }

        private bool InteractionSucceeded() {
            //Debug.Log(gameObject.name + ".PlayerController.InteractionSucceeded()");

            if (PlayerManager.Instance.UnitController == null) {
                return false;
            }
            if (PlayerManager.Instance.UnitController.Target == null) {
                //Debug.Log(gameObject.name + ".PlayerController.InteractionSucceeded(): target is null. return false.");
                return false;
            }
            //if (IsTargetInHitBox(target)) {
            // get reference to name now since interactable could change scene and then target reference is lost
            string targetDisplayName = PlayerManager.Instance.UnitController.Target.DisplayName;
            if (PlayerManager.Instance.UnitController.Target.Interact(PlayerManager.Instance.ActiveUnitController.CharacterUnit, true)) {
                //Debug.Log(gameObject.name + ".PlayerController.InteractionSucceeded(): Interaction Succeeded.  Setting interactable to null");
                SystemGameManager.Instance.EventManager.NotifyOnInteractionStarted(targetDisplayName);
                return true;
            }
            //Debug.Log(gameObject.name + ".PlayerController.InteractionSucceeded(): returning false");

            return false;
            //}
            //return false;
        }

        private void RegisterTab() {
            if (InputManager.Instance.KeyBindWasPressed("NEXTTARGET")) {
                //Debug.Log("Tab Target Registered");
                Interactable oldTarget = PlayerManager.Instance.UnitController.Target;
                // moving this inside getnexttabtarget
                //PlayerManager.Instance.UnitController.ClearTarget();
                GetNextTabTarget(oldTarget);
            }
        }

        private void GetNextTabTarget(Interactable oldTarget) {
            //Debug.Log("PlayerController.GetNextTabTarget(): maxDistance: " + tabTargetMaxDistance);
            DateTime currentTime = DateTime.Now;
            TimeSpan timeSinceLastTab = currentTime - lastTabTargetTime;
            lastTabTargetTime = DateTime.Now;
            int validMask = 1 << LayerMask.NameToLayer("CharacterUnit");
            Collider[] hitColliders = Physics.OverlapSphere(PlayerManager.Instance.ActiveUnitController.transform.position, tabTargetMaxDistance, validMask);
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
                if (targetCharacterUnit != null && targetCharacterUnit.CharacterUnit.BaseCharacter.CharacterStats.IsAlive == true && Faction.RelationWith(targetCharacterUnit.CharacterUnit.BaseCharacter, PlayerManager.Instance.MyCharacter.Faction) <= -1) {

                    // check if the unit is actually in front of our character.
                    // not doing any cone or angles for now, anywhere in front will do.  might adjust this a bit later to prevent targetting units nearly adjacent to us and far away
                    Vector3 transformedPosition = PlayerManager.Instance.ActiveUnitController.transform.InverseTransformPoint(collidedGameObject.transform.position);
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
                if (Vector3.Distance(PlayerManager.Instance.ActiveUnitController.transform.position, collidedGameObject.transform.position) < Vector3.Distance(PlayerManager.Instance.ActiveUnitController.transform.position, characterUnitList[closestTargetIndex].transform.position)) {
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
                if (closestTargetIndex != -1 && characterUnitList[closestTargetIndex] != PlayerManager.Instance.UnitController.Target) {
                    // prevent a tab from re-targetting the same unit just because it's closest to us
                    // we only want to clear the target if we are actually setting a new target
                    PlayerManager.Instance.UnitController.ClearTarget();
                    PlayerManager.Instance.UnitController.SetTarget(characterUnitList[closestTargetIndex]);
                    // we need to manually set this here, otherwise our tab target index won't match our actual target, resulting in the next tab possibly not switching to a new target
                    tabTargetIndex = closestTargetIndex;
                    //} else if (preferredTarget != null) {
                } else {
                    if (characterUnitList[tabTargetIndex] != PlayerManager.Instance.UnitController.Target) {
                        // we only want to clear the target if we are actually setting a new target
                        PlayerManager.Instance.UnitController.ClearTarget();
                        PlayerManager.Instance.UnitController.SetTarget(characterUnitList[tabTargetIndex]);
                    }
                }
            } else {
                //Debug.Log("PlayerController.GetNextTabTarget(): Less than 3 seconds since last tab, using index: " + tabTargetIndex);
                // we only want to clear the target if we are actually setting a new target
                if (characterUnitList[tabTargetIndex] != PlayerManager.Instance.UnitController.Target) {
                    PlayerManager.Instance.UnitController.ClearTarget();
                    PlayerManager.Instance.UnitController.SetTarget(characterUnitList[tabTargetIndex]);
                }
            }
        }

        public void InterActWithTarget(Interactable interactable) {
            //Debug.Log(gameObject.name + ".InterActWithTarget(" + interactable.gameObject.name + ")");
            if (PlayerManager.Instance.UnitController.Target != interactable) {
                PlayerManager.Instance.UnitController.ClearTarget();
                PlayerManager.Instance.UnitController.SetTarget(interactable);
            }
            if (InteractionSucceeded()) {
                //Debug.Log("We were able to interact with the target");
                // not actually stopping interacting.  just clearing target if this was a trigger interaction and we are not interacting with a focus
                StopInteract();
            } else {
                //Debug.Log("we were out of range and must move toward the target to be able to interact with it");
                if (PlayerManager.Instance.PlayerUnitMovementController.useMeshNav) {
                    //Debug.Log("Nav Mesh Agent is enabled. Setting follow target: " + target.name);
                    PlayerManager.Instance.ActiveUnitController.UnitMotor.FollowTarget(PlayerManager.Instance.UnitController.Target);
                } else {
                    //Debug.Log("Nav Mesh Agent is disabled and you are out of range");
                }
            }
        }

        public void InterActWithInteractableOption(Interactable interactable, InteractableOptionComponent interactableOption) {
            //Debug.Log(gameObject.name + ".InterActWithTarget(" + interactable.MyName + ", " + _gameObject.name.ToString() + ")");
            PlayerManager.Instance.UnitController.SetTarget(interactable);
            if (interactable == null) {
                //Debug.Log(gameObject.name + ".PlayerController.InteractWithTarget(): interactable is null!!!");
            }
            if (InteractionWithOptionSucceeded(interactableOption)) {
                //Debug.Log("We were able to interact with the target");
                // not actually stopping interacting.  just clearing target if this was a trigger interaction and we are not interacting with a focus
                StopInteract();
            } else {
                //Debug.Log("we were out of range and must move toward the target to be able to interact with it");
                if (PlayerManager.Instance.PlayerUnitMovementController.useMeshNav) {
                    //Debug.Log("Nav Mesh Agent is enabled. Setting follow target: " + target.name);
                    PlayerManager.Instance.ActiveUnitController.UnitMotor.FollowTarget(PlayerManager.Instance.UnitController.Target);
                } else {
                    //Debug.Log("Nav Mesh Agent is disabled and you are out of range");
                }
            }
        }

        private bool InteractionWithOptionSucceeded(InteractableOptionComponent interactableOption) {
            //Debug.Log(gameObject.name + ".PlayerController.InteractionSucceeded()");
            //if (IsTargetInHitBox(target)) {
            if (interactableOption.Interact(PlayerManager.Instance.ActiveUnitController.CharacterUnit)) {
                //Debug.Log(gameObject.name + ".PlayerController.InteractionSucceeded(): Interaction Succeeded.  Setting interactable to null");
                SystemGameManager.Instance.EventManager.NotifyOnInteractionStarted(PlayerManager.Instance.UnitController.Target.DisplayName);
                SystemGameManager.Instance.EventManager.NotifyOnInteractionWithOptionStarted(interactableOption);
                // no longer needed since targeting is changed and we don't want to lose target in the middle of attacking
                //PlayerManager.Instance.ActiveUnitController.SetTarget(null);
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
            if (InputManager.Instance.KeyBindWasPressed("CANCEL")) {
                PlayerManager.Instance.UnitController.ClearTarget();
                if (PlayerManager.Instance.ActiveCharacter.CharacterStats.IsAlive != false) {
                    // prevent character from swapping to third party controller while dead
                    PlayerManager.Instance.ActiveCharacter.CharacterAbilityManager.StopCasting();
                }
                PlayerManager.Instance.ActiveCharacter.CharacterAbilityManager.DeActivateTargettingMode();
            }
        }

        public void RegisterAbilityButtonPresses() {
            //Debug.Log("PlayerController.RegisterAbilityButtonPresses()");
            foreach (KeyBindNode keyBindNode in SystemGameManager.Instance.KeyBindManager.MyKeyBinds.Values) {
                //Debug.Log("PlayerController.RegisterAbilityButtonPresses() keyBindNode.GetKeyDown: " + keyBindNode.GetKeyDown);
                //Debug.Log("PlayerController.RegisterAbilityButtonPresses() keyBindNode.GetKeyDown: " + keyBindNode.GetKey);
                if (keyBindNode.MyKeyBindType == KeyBindType.Action && InputManager.Instance.KeyBindWasPressed(keyBindNode.MyKeyBindID) == true) {
                    //Debug.Log("PlayerController.RegisterAbilityButtonPresses(): key pressed: " + keyBindNode.MyKeyCode.ToString());
                    keyBindNode.MyActionButton.OnClick(true);
                }
            }
        }

        public void HandleClearTarget(Interactable oldTarget) {
            //Debug.Log("PlayerController.HandleClearTarget()");

            SystemGameManager.Instance.UIManager.FocusUnitFrameController.ClearTarget();
            SystemGameManager.Instance.UIManager.NamePlateManager.ClearFocus();
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
                SystemGameManager.Instance.UIManager.FocusUnitFrameController.SetTarget(namePlateUnit.NamePlateController);
                SystemGameManager.Instance.UIManager.NamePlateManager.SetFocus(namePlateUnit);
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
            if (PlayerManager.Instance.ActiveUnitController != null) {
                PlayerManager.Instance.ActiveUnitController.UnitAnimator.SetMoving(false);

                // why do we do this?
                //baseCharacter.UnitController.MyCharacterAnimator.EnableRootMotion();

                if (PlayerManager.Instance.PlayerUnitMovementController != null) {
                    PlayerManager.Instance.PlayerUnitMovementController.currentMoveVelocity = new Vector3(0, 0, 0);
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
            if (SystemGameManager.Instance.UIManager.FocusUnitFrameController.UnitNamePlateController == null && PlayerManager.Instance.UnitController != null) {
                PlayerManager.Instance.UnitController.ClearTarget();
            }
        }

        public void SubscribeToUnitEvents() {
            //Debug.Log("PlayerController.SubscribeToUnitEvents()");
            
            // if player was agrod at spawn, they may have a target already since we subscribe on model ready
            PlayerManager.Instance.ActiveUnitController.OnSetTarget += HandleSetTarget;
            if (PlayerManager.Instance.ActiveUnitController.Target != null) {
                HandleSetTarget(PlayerManager.Instance.ActiveUnitController.Target);
            }

            PlayerManager.Instance.ActiveUnitController.OnClearTarget += HandleClearTarget;
            PlayerManager.Instance.ActiveUnitController.UnitAnimator.OnStartCasting += HandleStartCasting;
            PlayerManager.Instance.ActiveUnitController.UnitAnimator.OnEndCasting += HandleEndCasting;
            PlayerManager.Instance.ActiveUnitController.UnitAnimator.OnStartAttacking += HandleStartAttacking;
            PlayerManager.Instance.ActiveUnitController.UnitAnimator.OnEndAttacking += HandleEndAttacking;
            PlayerManager.Instance.ActiveUnitController.UnitAnimator.OnStartLevitated += HandleStartLevitated;
            PlayerManager.Instance.ActiveUnitController.UnitAnimator.OnEndLevitated += HandleEndLevitated;
            PlayerManager.Instance.ActiveUnitController.UnitAnimator.OnStartStunned += HandleStartStunned;
            PlayerManager.Instance.ActiveUnitController.UnitAnimator.OnEndStunned += HandleEndStunned;
            PlayerManager.Instance.ActiveUnitController.UnitAnimator.OnStartRevive += HandleStartRevive;
            PlayerManager.Instance.ActiveUnitController.UnitAnimator.OnDeath += HandleDeath;
            PlayerManager.Instance.ActiveUnitController.OnClassChange += HandleClassChange;
            PlayerManager.Instance.ActiveUnitController.OnFactionChange += HandleFactionChange;
            PlayerManager.Instance.ActiveUnitController.OnSpecializationChange += HandleSpecializationChange;
            PlayerManager.Instance.ActiveUnitController.OnActivateMountedState += HandleActivateMountedState;
            PlayerManager.Instance.ActiveUnitController.OnDeActivateMountedState += HandleDeActivateMountedState;
            PlayerManager.Instance.ActiveUnitController.OnMessageFeed += HandleMessageFeed;
            PlayerManager.Instance.ActiveUnitController.OnUnitDestroy += HandleUnitDestroy;
            PlayerManager.Instance.ActiveUnitController.OnCastCancel += HandleCastCancel;

            // subscribe and call in case the namePlate is already spawned
            PlayerManager.Instance.ActiveUnitController.OnInitializeNamePlate += HandleInitializeNamePlate;
            HandleInitializeNamePlate();
        }

        public void UnsubscribeFromUnitEvents() {
            PlayerManager.Instance.ActiveUnitController.OnSetTarget -= HandleSetTarget;
            PlayerManager.Instance.ActiveUnitController.OnClearTarget -= HandleClearTarget;
            PlayerManager.Instance.ActiveUnitController.UnitAnimator.OnStartCasting -= HandleStartCasting;
            PlayerManager.Instance.ActiveUnitController.UnitAnimator.OnEndCasting -= HandleEndCasting;
            PlayerManager.Instance.ActiveUnitController.UnitAnimator.OnStartAttacking -= HandleStartAttacking;
            PlayerManager.Instance.ActiveUnitController.UnitAnimator.OnEndAttacking -= HandleEndAttacking;
            PlayerManager.Instance.ActiveUnitController.UnitAnimator.OnStartLevitated -= HandleStartLevitated;
            PlayerManager.Instance.ActiveUnitController.UnitAnimator.OnEndLevitated -= HandleEndLevitated;
            PlayerManager.Instance.ActiveUnitController.UnitAnimator.OnStartStunned -= HandleStartStunned;
            PlayerManager.Instance.ActiveUnitController.UnitAnimator.OnEndStunned -= HandleEndStunned;
            PlayerManager.Instance.ActiveUnitController.UnitAnimator.OnStartRevive -= HandleStartRevive;
            PlayerManager.Instance.ActiveUnitController.UnitAnimator.OnDeath -= HandleDeath;
            PlayerManager.Instance.ActiveUnitController.OnClassChange -= HandleClassChange;
            PlayerManager.Instance.ActiveUnitController.OnFactionChange -= HandleFactionChange;
            PlayerManager.Instance.ActiveUnitController.OnSpecializationChange -= HandleSpecializationChange;
            PlayerManager.Instance.ActiveUnitController.OnActivateMountedState -= HandleActivateMountedState;
            PlayerManager.Instance.ActiveUnitController.OnDeActivateMountedState -= HandleDeActivateMountedState;
            PlayerManager.Instance.ActiveUnitController.OnMessageFeed -= HandleMessageFeed;
            PlayerManager.Instance.ActiveUnitController.OnInitializeNamePlate -= HandleInitializeNamePlate;
            PlayerManager.Instance.ActiveUnitController.OnUnitDestroy -= HandleUnitDestroy;
            PlayerManager.Instance.ActiveUnitController.OnCastCancel -= HandleCastCancel;

        }

        public void HandleCastCancel(BaseCharacter baseCharacter) {
            //Debug.Log("PlayerController.HandleCastCancel()");
            SystemGameManager.Instance.CraftingManager.ClearCraftingQueue();
        }

        public void HandleInitializeNamePlate() {
            //Debug.Log("PlayerController.HandleInitializeNamePlate()");
            if (PlayerManager.Instance?.ActiveUnitController?.NamePlateController?.NamePlate != null) {
                PlayerManager.Instance.ActiveUnitController.NamePlateController.NamePlate.SetPlayerOwnerShip();
            }
        }

        public void HandleUnitDestroy(UnitProfile unitProfile) {
            //Debug.Log("PlayerController.HandleUnitDestroy()");
            SystemEventManager.TriggerEvent("OnPlayerUnitDespawn", new EventParamProperties());
            UnsubscribeFromUnitEvents();
            PlayerManager.Instance.SetUnitController(null);
        }

        public void HandleMessageFeed(string message) {
            SystemGameManager.Instance.UIManager.MessageFeedManager.WriteMessage(message);
        }

        public void HandleActivateMountedState(UnitController mountUnitController) {

            PlayerManager.Instance.ActiveUnitController.UnitAnimator.SetDefaultOverrideController();

            PlayerManager.Instance.SetActiveUnitController(mountUnitController);

            SystemGameManager.Instance.CameraManager.SwitchToMainCamera();
            SystemGameManager.Instance.CameraManager.MainCameraController.InitializeCamera(PlayerManager.Instance.ActiveUnitController.transform);
            if (SystemConfigurationManager.Instance.UseThirdPartyMovementControl == true) {
                PlayerManager.Instance.EnableMovementControllers();
            }

            EventParamProperties eventParam = new EventParamProperties();
            SystemEventManager.TriggerEvent("OnStartRiding", eventParam);
        }

        public void HandleDeActivateMountedState() {

            if (SystemConfigurationManager.Instance.UseThirdPartyMovementControl == true) {
                PlayerManager.Instance.DisableMovementControllers();
            }
            PlayerManager.Instance.SetActiveUnitController(PlayerManager.Instance.UnitController);
            if (PlayerManager.Instance.UnitController != null) {
                PlayerManager.Instance.UnitController.UnitAnimator.SetCorrectOverrideController();
            }

            SystemGameManager.Instance.CameraManager.ActivateMainCamera();
            SystemGameManager.Instance.CameraManager.MainCameraController.InitializeCamera(PlayerManager.Instance.ActiveUnitController.transform);

            EventParamProperties eventParam = new EventParamProperties();
            SystemEventManager.TriggerEvent("OnEndRiding", eventParam);

        }

        public void HandleFactionChange(Faction newFaction, Faction oldFaction) {
            SystemEventManager.TriggerEvent("OnFactionChange", new EventParamProperties());
            SystemGameManager.Instance.UIManager.MessageFeedManager.WriteMessage("Changed faction to " + newFaction.DisplayName);
        }

        public void HandleClassChange(CharacterClass newCharacterClass, CharacterClass oldCharacterClass) {
            SystemGameManager.Instance.EventManager.NotifyOnClassChange(newCharacterClass, oldCharacterClass);
            SystemGameManager.Instance.UIManager.MessageFeedManager.WriteMessage("Changed class to " + newCharacterClass.DisplayName);
        }

        public void HandleSpecializationChange(ClassSpecialization newSpecialization, ClassSpecialization oldSpecialization) {
            SystemEventManager.TriggerEvent("OnSpecializationChange", new EventParamProperties());
            if (newSpecialization != null) {
                SystemGameManager.Instance.UIManager.MessageFeedManager.WriteMessage("Changed specialization to " + newSpecialization.DisplayName);
            }
        }


        public void HandleDeath() {
            //Debug.Log(gameObject.name + ".PlayerController.HandleDeath()");
            PlayerManager.Instance.ActiveUnitController.UnitAnimator.SetDefaultOverrideController();
            SystemEventManager.TriggerEvent("OnDeath", new EventParamProperties());
        }

        public void HandleStartRevive() {
            PlayerManager.Instance.ActiveUnitController.UnitAnimator.SetDefaultOverrideController();
        }

        public void HandleStartLevitated() {
            PlayerManager.Instance.ActiveUnitController.UnitAnimator.SetDefaultOverrideController();
            EventParamProperties eventParam = new EventParamProperties();
            SystemEventManager.TriggerEvent("OnStartLevitated", eventParam);
        }

        public void HandleEndLevitated(bool swapAnimator) {
            if (swapAnimator) {
                PlayerManager.Instance.ActiveUnitController.UnitAnimator.SetCorrectOverrideController();
                EventParamProperties eventParam = new EventParamProperties();
                SystemEventManager.TriggerEvent("OnEndLevitated", eventParam);
            }
        }

        public void HandleStartStunned() {
            PlayerManager.Instance.ActiveUnitController.UnitAnimator.SetDefaultOverrideController();
            EventParamProperties eventParam = new EventParamProperties();
            SystemEventManager.TriggerEvent("OnStartStunned", eventParam);
        }

        public void HandleEndStunned(bool swapAnimator) {
            if (swapAnimator) {
                PlayerManager.Instance.ActiveUnitController.UnitAnimator.SetCorrectOverrideController();
                EventParamProperties eventParam = new EventParamProperties();
                SystemEventManager.TriggerEvent("OnEndStunned", eventParam);
            }
        }

        public void HandleStartCasting(bool swapAnimator) {
            EventParamProperties eventParam = new EventParamProperties();
            if (swapAnimator == true) {
                PlayerManager.Instance.ActiveUnitController.UnitAnimator.SetDefaultOverrideController();
            }
            SystemEventManager.TriggerEvent("OnStartCasting", eventParam);
        }

        public void HandleEndCasting(bool swapAnimator) {
            EventParamProperties eventParam = new EventParamProperties();
            if (swapAnimator) {
                PlayerManager.Instance.ActiveUnitController.UnitAnimator.SetCorrectOverrideController();
                SystemEventManager.TriggerEvent("OnEndCasting", eventParam);
            }
        }

        public void HandleStartAttacking(bool swapAnimator) {
            EventParamProperties eventParam = new EventParamProperties();
            if (swapAnimator) {
                PlayerManager.Instance.ActiveUnitController.UnitAnimator.SetDefaultOverrideController();
            }
            SystemEventManager.TriggerEvent("OnStartAttacking", eventParam);
        }

        public void HandleEndAttacking(bool swapAnimator) {
            EventParamProperties eventParam = new EventParamProperties();
            if (swapAnimator) {
                if (PlayerManager.Instance.ActiveUnitController != null) {
                    PlayerManager.Instance.ActiveUnitController.UnitAnimator.SetCorrectOverrideController();
                }
                SystemEventManager.TriggerEvent("OnEndAttacking", eventParam);
            }
        }

    }

}