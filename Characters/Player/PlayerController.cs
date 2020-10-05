using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AnyRPG {
    public class PlayerController : BaseController {
        public event System.Action<int> AbilityButtonPressedHandler = delegate { };
        public event System.Action<bool> ToggleRunHandler = delegate { };
        public override event System.Action<GameObject> OnSetTarget = delegate { };
        public override event System.Action OnClearTarget = delegate { };


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
        private Interactable interactable = null;
        private Interactable mouseOverInteractable = null;

        private int tabTargetIndex = 0;
        private CharacterCombat enemyCombat;

        private DateTime lastTabTargetTime;

        private RaycastHit mouseOverhit;

        public List<Interactable> MyInteractables { get => interactables; }
        public RaycastHit MyMouseOverhit { get => mouseOverhit; set => mouseOverhit = value; }

        protected override void Awake() {
            base.Awake();
            baseCharacter = GetComponent<PlayerCharacter>();
            // put this in player spawn
            allowedInput = true;
            lastTabTargetTime = DateTime.Now;
        }

        protected override void Start() {
            //Debug.Log(gameObject.name + ".PlayerController.Start()");
            base.Start();

            // run by default
            ToggleRun();
        }

        public override void CreateEventSubscriptions() {
            //Debug.Log(gameObject.name + ".PlayerController.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            base.CreateEventSubscriptions();
            if (baseCharacter != null && baseCharacter.CharacterStats != null) {
                //Debug.Log(gameObject.name + ".PlayerController.CreateEventSubscriptions(): subscribing to OnDie");
                baseCharacter.CharacterStats.OnDie += HandleDeath;
                baseCharacter.CharacterStats.OnReviveBegin += HandleRevive;
            }
        }

        public override void CleanupEventSubscriptions() {
            base.CleanupEventSubscriptions();
            if (baseCharacter != null && baseCharacter.CharacterStats != null) {
                baseCharacter.CharacterStats.OnDie -= HandleDeath;
                baseCharacter.CharacterStats.OnReviveBegin -= HandleRevive;
            }
        }

        public override void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            base.OnDisable();
            CleanupEventSubscriptions();
        }

        public override void ProcessLevelUnload() {
            base.ProcessLevelUnload();
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
                CommonMovementNotifier();
            }
        }

        /*
        private void CollectAimInput() {
            aimInput = new Vector2(inputAimHorizontal, inputAimVertical);
        }
        */

        protected override void Update() {
            //Debug.Log("PlayerController.Update()");
            ResetMoveInput();

            if (PlayerManager.MyInstance.MyPlayerUnitObject == null) {
                //Debug.Log(gameObject.name + ".PlayerController.Update(): Player Unit is not spawned. Exiting");
                return;
            }
            base.Update();

            if (allowedInput == false) {
                //Debug.Log("Not allowed to Collect Move Input. Exiting PlayerController Update!");
                return;
            }

            //CollectAimInput();

            HandleCancelButtonPressed();

            HandleMouseOver();

            if (baseCharacter.CharacterStats.IsAlive == false) {
                // can't interact, perform abilities or handle movement when dead
                return;
            }

            // test move this below death check to prevent player getting up after death
            ToggleRun();
            CollectMoveInput();

            HandleLeftMouseClick();

            HandleRightMouseClick();

            RegisterAbilityButtonPresses();

            RegisterTab();

            if (apparentVelocity > 0.1f) {
                baseCharacter.CharacterAbilityManager.HandleManualMovement();
            }

            HandleMovementAudio();
        }

        private void HandleMovementAudio() {
            //Debug.Log(gameObject.name + ".HandleMovementAudio(): " + apparentVelocity);
            if (baseCharacter.UnitProfile == null || baseCharacter.UnitProfile.MovementAudioProfiles == null || baseCharacter.UnitProfile.MovementAudioProfiles.Count == 0 || baseCharacter.UnitProfile.PlayOnFootstep == true) {
                //Debug.Log(gameObject.name + ".HandleMovementAudio(): nothing to do, returning");
                return;
            }

            if (apparentVelocity >= (baseCharacter.CharacterStats.RunSpeed / 2f)) {
                //Debug.Log(gameObject.name + ".HandleMovementAudio(): up to run speed");
                if (!baseCharacter.CharacterUnit.UnitAudio.MovementIsPlaying()) {
                    baseCharacter.CharacterUnit.UnitAudio.PlayMovement(baseCharacter.CharacterUnit.MovementLoopProfile.AudioClip, true);
                }
            } else {
                //Debug.Log(gameObject.name + ".HandleMovementAudio(): not up to run speed");
                if (baseCharacter.CharacterUnit.UnitAudio.MovementIsPlaying()) {
                    baseCharacter.CharacterUnit.UnitAudio.StopMovement();
                }
            }
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
                if (walking == false) {
                    walking = true;
                    eventParamProperties.simpleParams.BoolParam = true;
                } else {
                    walking = false;
                    eventParamProperties.simpleParams.BoolParam = false;
                }
                SystemEventManager.TriggerEvent("OnToggleRun", eventParamProperties);
                MessageFeedManager.MyInstance.WriteMessage("Walk: " + walking.ToString());
                ToggleRunHandler(walking);
            }
        }

        protected override void FixedUpdate() {
            base.FixedUpdate();

            CheckForInteraction();

        }

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

                    Interactable newInteractable = mouseOverhit.collider.GetComponent<Interactable>();
                    if (newInteractable == null) {
                        newInteractable = mouseOverhit.collider.GetComponentInParent<Interactable>();
                    }
                    //Debug.Log("We hit " + mouseOverhit.collider.name + " " + mouseOverhit.point + "; old: " + (mouseOverInteractable != null ? mouseOverInteractable.MyName : "null") + "; new: " + (newInteractable != null ? newInteractable.MyName : "null"));

                    if (mouseOverInteractable != null && mouseOverInteractable != newInteractable) {
                        // since we hit something, and our existing thing was not null, we have to exit the old one
                        //Debug.Log("We hit " + mouseOverhit.collider.name + " " + mouseOverhit.point + "; old: " + (mouseOverInteractable != null ? mouseOverInteractable.MyName : "null")+ "; new: " + (newInteractable != null ? newInteractable.MyName : "null" ));

                        mouseOverInteractable.OnMouseOut();
                    }

                    if (newInteractable != null && mouseOverInteractable != newInteractable) {
                        // we have a new interactable, activate mouseover

                        //Debug.Log("We hit " + mouseOverhit.collider.name + " " + mouseOverhit.point + " and it had an interactable.  activating mouseover");
                        newInteractable.OnMouseHover();
                    }
                    mouseOverInteractable = newInteractable;
                }
            } else {
                disableMouseOver = true;
                //Debug.Log(gameObject.name + ".PlayerController.HandleMouseOver(): mouseovernameplate: " + NamePlateManager.MyInstance.MouseOverNamePlate() + "; pointerovergameobject: " + EventSystem.current.IsPointerOverGameObject());
            }

            if (disableMouseOver) {
                // we did not hit any interactable, check if a current interactable is set and unset it
                if (mouseOverInteractable != null) {
                    mouseOverInteractable.OnMouseOut();
                    mouseOverInteractable = null;
                }
            }

        }


        private void HandleRightMouseClick() {
            //Debug.Log(gameObject.name + ".PlayerController.HandleRightMouseClick()");
            // check if the right mouse button clicked on something and interact with it
            if (InputManager.MyInstance.rightMouseButtonClicked && !EventSystem.current.IsPointerOverGameObject()) {
                //Debug.Log(gameObject.name + ".PlayerController.HandleRightMouseClick(): !EventSystem.current.IsPointerOverGameObject() == true!!!");


                if (mouseOverInteractable != null && mouseOverInteractable.IsTrigger == false) {
                    //Debug.Log("setting interaction target to " + hit.collider.gameObject.name);
                    //interactionTarget = hit.collider.gameObject;
                    InterActWithTarget(mouseOverInteractable, mouseOverInteractable.gameObject);
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
                ClearTarget();
            } else if (mouseOverInteractable != null) {
                SetTarget(mouseOverInteractable.gameObject);
            }
            //}

            Ray ray = CameraManager.MyInstance.MyActiveMainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100, movementMask)) {
                if ((MyBaseCharacter.CharacterAbilityManager as PlayerAbilityManager).WaitingForTarget()) {

                    (MyBaseCharacter.CharacterAbilityManager as PlayerAbilityManager).SetGroundTarget(hit.point);
                }
            }
        }

        /// <summary>
        /// if an interactable is set, try to interact with it if it's in range.
        /// </summary>
        private void CheckForInteraction() {
            if (interactable == null) {
                return;
            }
            if (InteractionSucceeded()) {
                if (MyBaseCharacter != null && MyBaseCharacter.AnimatedUnit != null && MyBaseCharacter.AnimatedUnit.MyCharacterMotor != null) {
                    MyBaseCharacter.AnimatedUnit.MyCharacterMotor.StopFollowingTarget();
                }
            }
        }

        private bool InteractionSucceeded() {
            //Debug.Log(gameObject.name + ".PlayerController.InteractionSucceeded()");

            if (PlayerManager.MyInstance.MyPlayerUnitSpawned == false) {
                return false;
            }
            //if (IsTargetInHitBox(target)) {
            if (interactable.Interact(baseCharacter.CharacterUnit)) {
                //Debug.Log(gameObject.name + ".PlayerController.InteractionSucceeded(): Interaction Succeeded.  Setting interactable to null");
                if (interactable != null) {
                    SystemEventManager.MyInstance.NotifyOnInteractionStarted(interactable.DisplayName);
                    interactable = null;
                }
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
                GameObject oldTarget = target;
                // moving this inside getnexttabtarget
                //ClearTarget();
                GetNextTabTarget(oldTarget);
            }
        }

        private void GetNextTabTarget(GameObject oldTarget) {
            //Debug.Log("PlayerController.GetNextTabTarget(): maxDistance: " + tabTargetMaxDistance);
            DateTime currentTime = DateTime.Now;
            TimeSpan timeSinceLastTab = currentTime - lastTabTargetTime;
            lastTabTargetTime = DateTime.Now;
            int validMask = 1 << LayerMask.NameToLayer("CharacterUnit");
            Collider[] hitColliders = Physics.OverlapSphere(baseCharacter.CharacterUnit.transform.position, tabTargetMaxDistance, validMask);
            int i = 0;
            //Debug.Log("GetNextTabTarget(): collider length: " + hitColliders.Length + "; index: " + tabTargetIndex);
            int preferredTargetIndex = -1;
            int closestTargetIndex = -1;

            // although the layermask on the collider should have only delivered us valid characterUnits, they may be dead or friendly.  We need to put all the valid attack targets in a list first
            List<GameObject> characterUnitList = new List<GameObject>();
            foreach (Collider hitCollider in hitColliders) {
                //Debug.Log("GetNextTabTarget(): collider length: " + hitColliders.Length);
                GameObject collidedGameObject = hitCollider.gameObject;
                CharacterUnit targetCharacterUnit = collidedGameObject.GetComponent<CharacterUnit>();
                if (targetCharacterUnit != null && targetCharacterUnit.MyCharacter.CharacterStats.IsAlive == true && Faction.RelationWith(targetCharacterUnit.MyCharacter, baseCharacter.Faction) <= -1) {

                    // check if the unit is actually in front of our character.
                    // not doing any cone or angles for now, anywhere in front will do.  might adjust this a bit later to prevent targetting units nearly adjacent to us and far away
                    Vector3 transformedPosition = baseCharacter.CharacterUnit.transform.InverseTransformPoint(collidedGameObject.transform.position);
                    if (transformedPosition.z > 0f) {
                        characterUnitList.Add(collidedGameObject);

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
            foreach (GameObject collidedGameObject in characterUnitList) {
                //Debug.Log("PlayerController.GetNextTabTarget(): processing target: " + i + "; " + collidedGameObject.name);
                if (closestTargetIndex == -1) {
                    closestTargetIndex = i;
                }
                if (Vector3.Distance(MyBaseCharacter.CharacterUnit.transform.position, collidedGameObject.transform.position) < Vector3.Distance(MyBaseCharacter.CharacterUnit.transform.position, characterUnitList[closestTargetIndex].transform.position)) {
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
                if (closestTargetIndex != -1 && characterUnitList[closestTargetIndex] != target) {
                    // prevent a tab from re-targetting the same unit just because it's closest to us
                    // we only want to clear the target if we are actually setting a new target
                    ClearTarget();
                    SetTarget(characterUnitList[closestTargetIndex]);
                    // we need to manually set this here, otherwise our tab target index won't match our actual target, resulting in the next tab possibly not switching to a new target
                    tabTargetIndex = closestTargetIndex;
                    //} else if (preferredTarget != null) {
                } else {
                    if (characterUnitList[tabTargetIndex] != target) {
                        // we only want to clear the target if we are actually setting a new target
                        ClearTarget();
                        SetTarget(characterUnitList[tabTargetIndex]);
                    }
                }
            } else {
                //Debug.Log("PlayerController.GetNextTabTarget(): Less than 3 seconds since last tab, using index: " + tabTargetIndex);
                // we only want to clear the target if we are actually setting a new target
                if (characterUnitList[tabTargetIndex] != target) {
                    ClearTarget();
                    SetTarget(characterUnitList[tabTargetIndex]);
                }
            }
        }

        public void InterActWithTarget(Interactable interactable, GameObject _gameObject) {
            //Debug.Log(gameObject.name + ".InterActWithTarget(" + interactable.MyName + ", " + _gameObject.name.ToString() + "); my current target: " + target);
            if (target != _gameObject) {
                ClearTarget();
                SetTarget(_gameObject);
            } else {
                //Debug.Log(gameObject.name + ".PlayerController.InteractWithTarget(): current target remains the same");
            }
            if (this.interactable != interactable) {
                this.interactable = interactable;
            } else {
                //Debug.Log(gameObject.name + ".PlayerController.InteractWithTarget(): current interactable remains the same");
            }
            if (interactable == null) {
                //Debug.Log(gameObject.name + ".PlayerController.InteractWithTarget(): interactable is null!!!");
            }
            if (baseCharacter == null) {
                //Debug.Log("BaseCharacter is null!!!");
            }
            if (baseCharacter.CharacterUnit == null) {
                //Debug.Log("BaseCharacter.MyCharacterUnit is null!!!");
            }
            if (InteractionSucceeded()) {
                //Debug.Log("We were able to interact with the target");
                // not actually stopping interacting.  just clearing target if this was a trigger interaction and we are not interacting with a focus
                StopInteract();
            } else if (MyBaseCharacter != null && MyBaseCharacter.AnimatedUnit != null && (MyBaseCharacter.AnimatedUnit as AnimatedPlayerUnit).MyPlayerUnitMovementController != null) {
                //Debug.Log("we were out of range and must move toward the target to be able to interact with it");
                if ((MyBaseCharacter.AnimatedUnit as AnimatedPlayerUnit).MyPlayerUnitMovementController.useMeshNav) {
                    //Debug.Log("Nav Mesh Agent is enabled. Setting follow target: " + target.name);
                    MyBaseCharacter.AnimatedUnit.MyCharacterMotor.FollowTarget(target);
                } else {
                    //Debug.Log("Nav Mesh Agent is disabled and you are out of range");
                }
            }
        }

        public void InterActWithInteractableOption(Interactable interactable, InteractableOption interactableOption, GameObject _gameObject) {
            //Debug.Log(gameObject.name + ".InterActWithTarget(" + interactable.MyName + ", " + _gameObject.name.ToString() + ")");
            SetTarget(_gameObject);
            this.interactable = interactable;
            if (interactable == null) {
                //Debug.Log(gameObject.name + ".PlayerController.InteractWithTarget(): interactable is null!!!");
            }
            if (baseCharacter == null) {
                //Debug.Log("BaseCharacter is null!!!");
            }
            if (baseCharacter.CharacterUnit == null) {
                //Debug.Log("BaseCharacter.MyCharacterUnit is null!!!");
            }
            if (InteractionWithOptionSucceeded(interactableOption)) {
                //Debug.Log("We were able to interact with the target");
                // not actually stopping interacting.  just clearing target if this was a trigger interaction and we are not interacting with a focus
                StopInteract();
            } else {
                //Debug.Log("we were out of range and must move toward the target to be able to interact with it");
                if ((MyBaseCharacter.AnimatedUnit as AnimatedPlayerUnit).MyPlayerUnitMovementController.useMeshNav) {
                    //Debug.Log("Nav Mesh Agent is enabled. Setting follow target: " + target.name);
                    MyBaseCharacter.AnimatedUnit.MyCharacterMotor.FollowTarget(target);
                } else {
                    //Debug.Log("Nav Mesh Agent is disabled and you are out of range");
                }
            }
        }

        private bool InteractionWithOptionSucceeded(InteractableOption interactableOption) {
            //Debug.Log(gameObject.name + ".PlayerController.InteractionSucceeded()");
            //if (IsTargetInHitBox(target)) {
            if (interactableOption.Interact(baseCharacter.CharacterUnit)) {
                //Debug.Log(gameObject.name + ".PlayerController.InteractionSucceeded(): Interaction Succeeded.  Setting interactable to null");
                SystemEventManager.MyInstance.NotifyOnInteractionStarted(interactable.DisplayName);
                SystemEventManager.MyInstance.NotifyOnInteractionWithOptionStarted(interactableOption);
                interactable = null;
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
                ClearTarget();
                MyBaseCharacter.CharacterAbilityManager.DeActivateTargettingMode();
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

        public override void ClearTarget() {
            //Debug.Log("PlayerController.ClearTarget()");
            base.ClearTarget();
            interactable = null;
            UIManager.MyInstance.MyFocusUnitFrameController.ClearTarget();
            NamePlateManager.MyInstance.ClearFocus();
            OnClearTarget();
        }

        public override void SetTarget(GameObject newTarget) {
            //Debug.Log("PlayerController.SetTarget(" + (newTarget == null ? "null" : newTarget.name) + ")");
            if (newTarget == null) {
                return;
            }
            base.SetTarget(newTarget);
            if (newTarget.GetComponent<INamePlateUnit>() != null) {
                //Debug.Log("PlayerController.SetTarget(): InamePlateUnit is not null");
                UIManager.MyInstance.MyFocusUnitFrameController.SetTarget(newTarget);
                NamePlateManager.MyInstance.SetFocus(newTarget.GetComponent<INamePlateUnit>());
                OnSetTarget(target);
            } else {
                //Debug.Log("PlayerController.SetTarget(): InamePlateUnit is null ???!?");
            }
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

        public void HandleDeath(CharacterStats characterStats) {
            //Debug.Log(gameObject.name + ".PlayerController.HandleDeath()");
            Lock(true, true, false, 0.1f, 0f);
        }

        public void HandleRevive() {
            Lock(true, true, true, 0f, 8.0f);
        }

        //Keep character from moving.
        public void LockMovement() {
            //Debug.Log(gameObject.name + ".PlayerController.LockMovement()");
            canMove = false;
            if (baseCharacter.AnimatedUnit != null) {
                baseCharacter.AnimatedUnit.MyCharacterAnimator.SetMoving(false);

                // why do we do this?
                baseCharacter.AnimatedUnit.MyCharacterAnimator.EnableRootMotion();

                if ((baseCharacter.AnimatedUnit as AnimatedPlayerUnit).MyPlayerUnitMovementController != null) {
                    (baseCharacter.AnimatedUnit as AnimatedPlayerUnit).MyPlayerUnitMovementController.currentMoveVelocity = new Vector3(0, 0, 0);
                }
            }
        }

        public void UnlockMovement() {
            //Debug.Log(gameObject.name + ".PlayerController.UnlockMovement()");
            canMove = true;

            // why do we do this?
            // is it because this function is never really called ?
            baseCharacter.AnimatedUnit.MyCharacterAnimator.DisableRootMotion();
        }

        public override void OnDestroy() {
            //Debug.Log(gameObject.name + ".PlayerController.OnDestroy()");
            base.OnDestroy();
            CleanupEventSubscriptions();
        }

        public void StopInteract() {
            // the idea of this code is that it will allow us to keep an NPC focused if we back out of range while its interactable popup closes
            // if we don't have anything focused, then we were interacting with someting environmental and definitely want to clear that because it can lead to a hidden target being set
            if (UIManager.MyInstance.MyFocusUnitFrameController.FollowGameObject == null) {
                ClearTarget();
            }
        }

    }

}