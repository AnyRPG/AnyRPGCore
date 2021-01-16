using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class Interactable : Spawnable, IDescribable {

        public event System.Action OnPrerequisiteUpdates = delegate { };
        public event System.Action OnInteractableDestroy = delegate { };

        // this field does not do anything, but is needed to satisfy the IDescribable interface
        protected Sprite interactableIcon = null;

        [Header("Mouse Over")]

        [Tooltip("Show a tooltip when the mouse is over the object")]
        [SerializeField]
        protected bool showTooltip = true;

        [Tooltip("This value will show in the mouseover tooltip.")]
        [SerializeField]
        protected string interactableName = string.Empty;

        [Tooltip("Should the object glow when the mouse is over it or its nameplate")]
        [SerializeField]
        protected bool glowOnMouseOver = true;

        [Tooltip("If true, the glow emits light on objects around it.")]
        [SerializeField]
        protected bool lightEmission = false;

        [Tooltip("The time period in seconds between high and low intensity of the glow strength")]
        [SerializeField]
        protected float glowFlashSpeed = 1.5f;

        [Tooltip("The minimum intensity the object should glow with")]
        [SerializeField]
        protected float glowMinIntensity = 4.5f;

        [Tooltip("The maximum intensity the object should glow with")]
        [SerializeField]
        protected float glowMaxIntensity = 6f;

        [Tooltip("The color of light to emit when glowing")]
        [SerializeField]
        protected Color glowColor = Color.yellow;

        [Tooltip("The glow material that should replace any normal materials on this object while glowing")]
        [SerializeField]
        protected Material temporaryMaterial = null;

        [Header("Interaction Options")]

        [Tooltip("Set this value to prevent direct interaction from the player.  This can be useful for interactables that only need to be activated with control switches.")]
        [SerializeField]
        protected bool notInteractable = false;

        [Tooltip("Set this to true to allow triggering interaction with anything that has a collider, not just players.")]
        [SerializeField]
        protected bool interactWithAny = false;

        [Tooltip("Set this to true to cause the interaction to trigger on when something exits the collider.")]
        [SerializeField]
        protected bool interactOnExit = false;

        [Tooltip("If true, interaction is triggered by a collider, and not by clicking with the mouse")]
        [SerializeField]
        protected bool isTrigger = false;

        [Tooltip("Set this to true to automatically activate the first interactable instead of opening the interaction window and presenting the player with interaction options.")]
        [SerializeField]
        protected bool suppressInteractionWindow = false;

        [Tooltip("For everything except character unit interactions, the interactor must be within this range of this objects collider. This does not apply to interactions triggered by switches.")]
        [SerializeField]
        private float interactionMaxRange = 2f;

        [Header("Additional Spawn Options")]

        [Tooltip("If set to true, all interactable options must have prerequisites met, in addition to the interactable prerequisites, in order to spawn")]
        [SerializeField]
        protected bool checkOptionsToSpawn = false;

        [Tooltip("Require a valid interactable option in addition to any preqrequisites.  For example, quests on a questgiver, a class changer, and dialogs.")]
        [SerializeField]
        protected bool spawnRequiresValidOption = false;

        [Tooltip("require no valid interactable options in addition to any preqrequisites. For example, quests on a questgiver, a class changer, and dialogs.")]
        [SerializeField]
        protected bool despawnRequiresNoValidOption = false;

        [Tooltip("Reference to local component controller prefab with nameplate target, speakers, etc")]
        [SerializeField]
        protected UnitComponentController unitComponentController = null;

        protected List<InteractableOptionComponent> interactables = new List<InteractableOptionComponent>();

        protected Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

        protected Material[] temporaryMaterials = null;
        protected Renderer[] meshRenderers = null;

        protected List<Shader> shaderList = new List<Shader>();
        protected List<Color> emissionColorList = new List<Color>();
        protected List<Texture> emissionTextureList = new List<Texture>();
        protected List<bool> emissionEnabledList = new List<bool>();

        // state
        protected bool isInteracting = false;
        protected bool isFlashing = false;
        protected bool hasMeshRenderer = false;
        protected bool miniMapIndicatorReady = false;
        protected bool isMouseOverUnit = false;
        protected bool isMouseOverNameplate = false;

        // attached components
        protected Collider myCollider;
        protected GameObject miniMapIndicator = null;

        // created components
        protected CharacterUnit characterUnit = null;
        protected DialogController dialogController = null;

        // properties
        public bool IsInteracting { get => isInteracting; }
        public List<InteractableOptionComponent> Interactables { get => interactables; set => interactables = value; }

        public Sprite Icon { get => interactableIcon; }

        public UnitComponentController UnitComponentController { get => unitComponentController; set => unitComponentController = value; }

        public virtual string DisplayName {
            get {
                if (interactableName != null && interactableName != string.Empty) {
                    return interactableName;
                }
                if (characterUnit != null) {
                    return characterUnit.BaseCharacter.CharacterName;
                }
                return gameObject.name;
            }
        }
        public bool NotInteractable { get => notInteractable; set => notInteractable = value; }
        public Collider Collider { get => myCollider; }
        public virtual float InteractionMaxRange { get => interactionMaxRange; set => interactionMaxRange = value; }
        public bool IsTrigger { get => isTrigger; set => isTrigger = value; }
        public CharacterUnit CharacterUnit { get => characterUnit; set => characterUnit = value; }
        public DialogController DialogController { get => dialogController; }
        public virtual bool CombatOnly { get => false; }
        public virtual bool NonCombatOptionsAvailable { get => true; }

        public override bool MyPrerequisitesMet {
            get {
                bool returnResult = base.MyPrerequisitesMet;
                if (returnResult != true) {
                    return returnResult;
                }
                if (checkOptionsToSpawn == true) {
                    //Debug.Log(gameObject.name + ".Interactable.MyPrerequisitesMet()");
                    if (CanInteract() == false) {
                        return false;
                    }
                }
                return returnResult;
            }
        }

        public virtual GameObject InteractableGameObject {
            get {
                return gameObject;
            }
        }

        public bool IsMouseOverUnit { get => isMouseOverUnit; set => isMouseOverUnit = value; }
        public bool IsMouseOverNameplate { get => isMouseOverNameplate; set => isMouseOverNameplate = value; }

        protected override void Awake() {
            base.Awake();
            dialogController = new DialogController(this);
            DisableInteraction();
            temporaryMaterials = null;
            if (temporaryMaterial == null) {
                if (SystemConfigurationManager.MyInstance == null) {
                    Debug.LogError(gameObject.name + ": SystemConfigurationManager not found. Is the GameManager in the scene?");
                    return;
                } else {
                    temporaryMaterial = SystemConfigurationManager.MyInstance.TemporaryMaterial;
                }
            }
            if (temporaryMaterial == null) {
                //Debug.Log("No glow materials available. overrideing glowOnMouseover to false");
                glowOnMouseOver = false;
            }

        }

        public override void ProcessInit() {
            base.ProcessInit();
            // moved to processInit()
            /*
            foreach (InteractableOptionComponent interactable in interactables) {
                //Debug.Log(gameObject.name + ".Interactable.Awake(): Found InteractableOptionComponent: " + interactable.ToString());
                if (interactable != null) {
                    // in rare cases where a script is missing or has been made abstract, but not updated, this can return a null interactable option
                    interactable.Init();
                }
            }
            */
        }

        public override void GetComponentReferences() {
            //Debug.Log(gameObject.name + ".Interactable.InitializeComponents()");

            if (componentReferencesInitialized == true) {
                return;
            }
            base.GetComponentReferences();

            myCollider = GetComponent<Collider>();

            // get monobehavior interactables
            InteractableOption[] interactableOptionMonoList = GetComponents<InteractableOption>();
            foreach (InteractableOption interactableOption in interactableOptionMonoList) {
                if (interactableOption.InteractableOptionProps != null) {
                    interactableOption.SetupScriptableObjects();
                    interactables.Add(interactableOption.InteractableOptionProps.GetInteractableOption(this, interactableOption));
                }
            }
        }

        public override void CleanupEverything() {
            //Debug.Log(gameObject.name + ".Interactable.CleanupEverything()");
            base.CleanupEverything();
            ClearFromPlayerRangeTable();
            if (dialogController != null) {
                dialogController.Cleanup();
            }
        }



        /// <summary>
        /// get a list of interactable options by type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<InteractableOptionComponent> GetInteractableOptionList(Type type) {
            List<InteractableOptionComponent> returnList = new List<InteractableOptionComponent>();

            foreach (InteractableOptionComponent interactableOption in interactables) {
                if (interactableOption.GetType() == type) {
                    returnList.Add(interactableOption);
                }
            }

            return returnList;
        }

        /// <summary>
        /// get the first interactable option by type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public InteractableOptionComponent GetFirstInteractableOption(Type type) {

            foreach (InteractableOptionComponent interactableOption in interactables) {
                if (interactableOption.GetType() == type) {
                    return interactableOption;
                }
            }

            return null;
        }

        public void AddInteractable(InteractableOptionComponent interactableOption) {
            //Debug.Log(gameObject.name + ".Interactable.AddInteractable()");
            interactables.Add(interactableOption);
        }

        protected virtual void Update() {
            // if the item is highlighted, we will continue a pulsing glow
            //return;
            if (isFlashing) {
                //Debug.Log("Interactable.Update(): isflashing == true");
                float emission = glowMinIntensity + Mathf.PingPong(Time.time * glowFlashSpeed, glowMaxIntensity - glowMinIntensity);
                //Debug.Log("Interactable.Update(): emission: " + emission);
                foreach (Renderer renderer in meshRenderers) {
                    //Debug.Log("Interactable.Update(): renderer: " + renderer.name);
                    if (renderer != null) {
                        // added this condition because of infestor effect adding extra renderers as child objects under the character unit
                        foreach (Material flashingMaterial in renderer.materials) {
                            //Debug.Log("Interactable.Update(): flashingmaterial: " + flashingMaterial.name + "; color: " + (glowColor * emission) + "; enabled? " + flashingMaterial.IsKeywordEnabled("_EMISSION"));
                            Color usedColor = glowColor;
                            if (lightEmission) {
                                usedColor = glowColor * emission;
                                flashingMaterial.SetColor("_EmissionColor", usedColor);
                            }
                            flashingMaterial.SetColor("_Color", usedColor);
                        }
                    }
                }
            }
        }

        public override bool CanSpawn() {
            //Debug.Log(gameObject.name + ".Interactable.CanSpawn()");
            bool returnResult = base.CanSpawn();
            if (returnResult == true && spawnRequiresValidOption) {
                if (GetCurrentInteractables().Count == 0) {
                    return false;
                }
            }
            return returnResult;
        }

        protected override bool CanDespawn() {
            bool returnResult = base.CanDespawn();
            if (returnResult == true && despawnRequiresNoValidOption) {
                if (GetCurrentInteractables().Count > 0) {
                    return false;
                }
            }
            return returnResult;
        }

        public override bool UpdateOnPlayerUnitSpawn() {
            //Debug.Log(gameObject.name + ".Interactable.UpdateOnPlayerUnitSpawn()");

            foreach (InteractableOptionComponent _interactable in interactables) {
                _interactable.HandlePlayerUnitSpawn();
            }
            bool preRequisitesUpdated = false;
            foreach (InteractableOptionComponent _interactable in interactables) {
                if (_interactable.MyPrerequisitesMet == true) {
                    preRequisitesUpdated = true;
                }
            }

            // calling this last intentionally because it can call handleprerequisiteupdates before we have set our prerequisite values properly
            bool updated = base.UpdateOnPlayerUnitSpawn();

            if (updated) {
                return true;
            }

            // calling this because our base will not have inititalized its prerequisites earlier
            if (preRequisitesUpdated) {
                HandlePrerequisiteUpdates();
                return true;
            }

            return false;
        }

        public override void HandlePrerequisiteUpdates() {
            //Debug.Log(gameObject.name + ".Interactable.HandlePrerequisiteUpdates()");

            base.HandlePrerequisiteUpdates();
            if (!PlayerManager.MyInstance.PlayerUnitSpawned) {
                return;
            }
            if (spawnReference == null && MyPrerequisitesMet == false) {
                DisableInteraction();
            } else {
                EnableInteraction();
            }

            // give interaction panel a chance to update or close
            OnPrerequisiteUpdates();

            InstantiateMiniMapIndicator();
        }

        public override void Spawn() {
            //Debug.Log(gameObject.name + ".Interactable.Spawn()");
            base.Spawn();

            EnableInteraction();
        }

        // meant to be overwritten on characters
        public virtual void EnableInteraction() {
            //Debug.Log(gameObject.name + ".Interactable.EnableInteraction()");
            if (myCollider != null) {
                myCollider.enabled = true;
            }
        }

        // meant to be overwritten on characters
        public virtual void DisableInteraction() {
            //Debug.Log(gameObject.name + ".Interactable.DisableInteraction()");
            if (myCollider != null) {
                myCollider.enabled = false;
            }
        }

        public override void DestroySpawn() {
            //Debug.Log(gameObject.name + ".Interactable.DestroySpawn()");
            base.DestroySpawn();

            originalMaterials.Clear();
            DisableInteraction();
            //MiniMapStatusUpdateHandler(this);
        }

        public bool InstantiateMiniMapIndicator() {
            //Debug.Log(gameObject.name + ".Interactable.InstantiateMiniMapIndicator()");
            if (!PlayerManager.MyInstance.PlayerUnitSpawned) {
                //Debug.Log(gameObject.name + ".Interactable.InstantiateMiniMapIndicator(): player unit not spawned yet.  returning");
                return false;
            }

            List<InteractableOptionComponent> validInteractables = GetValidInteractables();
            if (validInteractables.Count == 0) {
                //if (GetValidInteractables(PlayerManager.MyInstance.MyCharacter.MyCharacterUnit).Count == 0) {
                //Debug.Log(gameObject.name + ".Interactable.InstantiateMiniMapIndicator(): No valid Interactables.  Not spawning indicator.");
                return false;
            }

            if (!miniMapIndicatorReady) {
                if (MiniMapController.MyInstance == null) {
                    //Debug.Log(gameObject.name + ".Interactable.InstantiateMiniMapIndicator(): MiniMapController.MyInstance is null");
                    return false;
                }
                if (MiniMapController.MyInstance.MyMiniMapIndicatorPrefab == null) {
                    //Debug.Log(gameObject.name + ".Interactable.InstantiateMiniMapIndicator(): indicator prefab is null");
                    return false;
                }
                if (interactables.Count > 0) {
                    //Debug.Log(gameObject.name + ".Interactable.InstantiateMiniMapIndicator(): interactables.length > 0");
                    Vector3 spawnPosition = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + 9, gameObject.transform.position.z);
                    //miniMapIndicator = Instantiate(MiniMapController.MyInstance.MyMiniMapIndicatorPrefab, spawnPosition, Quaternion.identity, gameObject.transform);
                    miniMapIndicator = Instantiate(MiniMapController.MyInstance.MyMiniMapIndicatorPrefab, spawnPosition, Quaternion.identity, UIManager.MyInstance.MiniMapCanvasParent.transform);
                    miniMapIndicator.GetComponent<MiniMapIndicatorController>().SetInteractable(this);
                    miniMapIndicator.transform.localScale = new Vector3(0.0390625f, 0.0390625f, 0.0390625f);
                    miniMapIndicator.transform.Rotate(90f, 0f, 0f);
                    miniMapIndicatorReady = true;
                    return true;
                } else {
                    //Debug.Log(gameObject.name + ".Interactable.InstantiateMiniMapIndicator(): interactables.length == 0!!!!!");
                }
            } else {
                //Debug.Log("Already Instantiated");
            }
            return false;
        }

        public void CleanupMiniMapIndicator() {
            //Debug.Log(gameObject.name + ".Interactable.CleanupMiniMapIndicator()");
            if (miniMapIndicator != null) {
                Debug.Log(gameObject.name + ".Interactable.CleanupMiniMapIndicator(): " + miniMapIndicator.name);
                Destroy(miniMapIndicator);

                // keeping this set to true so any other update can't respawn it
                // if there is a situation where we re-enable interactables, then we should set it to false in OnEnable instead
                // miniMapIndicatorReady = false;
            }
        }

        /*
        public Transform GetInteractionTransform() {
            return interactionTransform;
        }
        */

        public void OpenInteractionWindow() {
            //Debug.Log(gameObject.name + ".Interactable.OpenInteractionWindow");
            if (InteractionPanelUI.MyInstance != null) {
                InteractionPanelUI.MyInstance.MyInteractable = this;
            }
            if (PopupWindowManager.MyInstance != null) {
                PopupWindowManager.MyInstance.interactionWindow.OpenWindow();
            }
        }

        public void CloseInteractionWindow() {
            InteractionPanelUI.MyInstance.MyInteractable = null;
            PopupWindowManager.MyInstance.interactionWindow.CloseWindow();
        }

        public bool CanInteract() {
            //Debug.Log(gameObject.name + ".Interactable.CanInteract()");
            if (notInteractable == true) {
                return false;
            }
            //Debug.Log(gameObject.name + ".Interactable.CanInteract()");
            if (PlayerManager.MyInstance == null || PlayerManager.MyInstance.PlayerUnitSpawned == false) {
                return false;
            }
            List<InteractableOptionComponent> validInteractables = GetValidInteractables();
            //List<InteractableOptionComponent> validInteractables = GetValidInteractables(PlayerManager.MyInstance.MyCharacter.MyCharacterUnit);
            if (validInteractables.Count > 0) {
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Return true if the object trying to interact is in the trigger list (it is inside the collider and allowed to interact)
        /// </summary>
        /// <returns></returns>
        public virtual bool Interact(CharacterUnit source, bool processRangeCheck = false) {
            //Debug.Log(gameObject.name + ".Interactable.Interact(" + source.DisplayName + ", " + processRangeCheck + ")");
            if (notInteractable == true) {
                return false;
            }

            // perform range check
            bool passedRangeCheck = false;
            if (processRangeCheck) {
                Collider[] colliders = new Collider[0];
                int playerMask = 1 << LayerMask.NameToLayer("Player");
                int characterMask = 1 << LayerMask.NameToLayer("CharacterUnit");
                int interactableMask = 1 << LayerMask.NameToLayer("Interactable");
                int triggerMask = 1 << LayerMask.NameToLayer("Triggers");
                int validMask = (playerMask | characterMask | interactableMask | triggerMask);
                Vector3 bottomPoint = new Vector3(source.Interactable.Collider.bounds.center.x,
                    source.Interactable.Collider.bounds.center.y - source.Interactable.Collider.bounds.extents.y,
                    source.Interactable.Collider.bounds.center.z);
                Vector3 topPoint = new Vector3(source.Interactable.Collider.bounds.center.x,
                    source.Interactable.Collider.bounds.center.y + source.Interactable.Collider.bounds.extents.y,
                    source.Interactable.Collider.bounds.center.z);
                colliders = Physics.OverlapCapsule(bottomPoint, topPoint, InteractionMaxRange, validMask);
                foreach (Collider collider in colliders) {
                    if (collider.gameObject == gameObject) {
                        passedRangeCheck = true;
                        break;
                    }
                }
            }

            float factionValue = PerformFactionCheck(source.BaseCharacter);

            // get a list of valid interactables to determine if there is an action we can treat as default
            List<InteractableOptionComponent> validInteractables = GetCurrentInteractables(source.BaseCharacter, true, factionValue);
            List<InteractableOptionComponent> finalInteractables = new List<InteractableOptionComponent>();
            if (processRangeCheck) {
                foreach (InteractableOptionComponent validInteractable in validInteractables) {
                    //Debug.Log(gameObject.name + ".Interactable.Interact(" + source.name + "): valid interactable name: " + validInteractable);
                    if (validInteractable.CanInteract(processRangeCheck, passedRangeCheck, factionValue)) {
                        finalInteractables.Add(validInteractable);
                    }
                }
            } else {
                finalInteractables = validInteractables;
            }
            // perform default interaction or open a window if there are multiple valid interactions
            //Debug.Log(gameObject.name + ".Interactable.Interact(): validInteractables.Count: " + validInteractables.Count);
            // changed code, window will always be opened, and it will decide if to pop another one or not
            if (finalInteractables.Count > 0) {
                if (suppressInteractionWindow == true || validInteractables.Count == 1) {
                    if (validInteractables[0].GetCurrentOptionCount() > 1) {
                        OpenInteractionWindow();
                    } else {
                        validInteractables[0].Interact(PlayerManager.MyInstance.ActiveUnitController.CharacterUnit);
                    }
                } else {
                    OpenInteractionWindow();
                }
                return true;
            }
            if (validInteractables.Count > 0 && finalInteractables.Count == 0) {
                if (processRangeCheck == true && passedRangeCheck == false) {
                    source.BaseCharacter.UnitController.NotifyOnMessageFeed(DisplayName + " is out of range");
                }
            }
            return false;
        }

        /*
        public bool CheckForInteractableObjectives(string questName) {
            Quest quest = SystemQuestManager.MyInstance.GetResource(questName);
            foreach (QuestObjective questObjective in quest.MyUseInteractableObjectives) {
                foreach (InteractableOption interactableOption in MyInteractables) {
                    if (SystemResourceManager.MatchResource(questObjective.MyType, interactableOption.MyName)) {
                        //Debug.Log("auto open interactable on questgiver to complete interactable objective");
                        if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.MyCharacter != null && PlayerManager.MyInstance.MyCharacter.MyCharacterController != null) {
                            Interactable _interactable = this;
                            GameObject _gameObject = gameObject;
                            PlayerManager.MyInstance.MyCharacter.MyCharacterController.InterActWithInteractableOption(_interactable, interactableOption, _gameObject);
                        } else {
                            //Debug.Log("player something is null");
                        }
                        return true;
                    }
                }
            }
            return false;
        }
        */


        public List<InteractableOptionComponent> GetValidInteractables() {
            //Debug.Log(gameObject.name + ".Interactable.GetValidInteractables(" + processRangeCheck + ", " + passedRangeCheck + ")");

            List<InteractableOptionComponent> validInteractables = new List<InteractableOptionComponent>();

            if (notInteractable == true) {
                return validInteractables;
            }

            if (interactables == null) {
                //Debug.Log(gameObject.name + ".Interactable.GetValidInteractables(): interactables is null.  returning null!");
                return validInteractables;
            }

            foreach (InteractableOptionComponent _interactable in interactables) {
                if (_interactable != null && !_interactable.Equals(null)) {
                    if (_interactable.GetValidOptionCount() > 0
                        && _interactable.MyPrerequisitesMet
                        //&& (processRangeCheck == false || _interactable.CanInteract())
                        ) {

                        // HAD TO REMOVE THE FIRST CONDITION BECAUSE IT WAS BREAKING MINIMAP UPDATES - MONITOR FOR WHAT REMOVING THAT BREAKS...
                        //if (_interactable.CanInteract(source) && _interactable.GetValidOptionCount() > 0 && _interactable.MyPrerequisitesMet) {

                        //Debug.Log(gameObject.name + ".Interactable.GetValidInteractables(): Adding valid interactable: " + _interactable.ToString());
                        validInteractables.Add(_interactable);
                    }
                }
            }
            return validInteractables;
        }

        public virtual float PerformFactionCheck(BaseCharacter sourceCharacter) {
            // interactables allow everything to interact by default.
            // characters will override this
            return 0;
        }

        public List<InteractableOptionComponent> GetCurrentInteractables(BaseCharacter sourceCharacter = null, bool overrideFactionValue = false, float factionValue = 0f) {
            //Debug.Log(gameObject.name + ".Interactable.GetCurrentInteractables()");

            if (sourceCharacter == null) {
                sourceCharacter = PlayerManager.MyInstance.ActiveCharacter;
            }

            if (overrideFactionValue == false) {
                factionValue = PerformFactionCheck(sourceCharacter);
            }

            if (notInteractable == true) {
                return null;
            }

            if (interactables == null) {
                //Debug.Log(gameObject.name + ".Interactable.GetValidInteractables(): interactables is null.  returning null!");
                return null;
            }

            List<InteractableOptionComponent> currentInteractables = new List<InteractableOptionComponent>();
            foreach (InteractableOptionComponent _interactable in interactables) {
                if (_interactable.CanInteract(false, false, factionValue)) {
                    //Debug.Log(gameObject.name + ".Interactable.GetCurrentInteractables(): Adding interactable: " + _interactable.ToString());
                    currentInteractables.Add(_interactable);
                } else {
                    //Debug.Log(gameObject.name + ".Interactable.GetValidInteractables(): invalid interactable: " + _interactable.ToString());
                }
            }
            return currentInteractables;
        }

        public void OnMouseHover() {
            //Debug.Log(gameObject.name + ".Interactable.OnMouseHover()");
            // rename from onMouseOver to OnMouseHover to avoid unity senting it mouse events.
            if (!isActiveAndEnabled) {
                // this interactable is inactive, there is no reason to do anything
                return;
            }

            if (notInteractable == true) {
                return;
            }

            if (showTooltip == false) {
                return;
            }

            if (PlayerManager.MyInstance == null) {
                return;
            }
            if (PlayerManager.MyInstance.PlayerUnitSpawned == false) {
                return;
            }

            if (EventSystem.current.IsPointerOverGameObject() && !NamePlateManager.MyInstance.MouseOverNamePlate()) {
                // THIS CODE WILL STILL CAUSE THE GUY TO GLOW IF YOU MOUSE OVER HIS NAMEPLATE WHILE A WINDOW IS UP.  NOT A BIG DEAL FOR NOW
                // IT HAS TO BE THIS WAY BECAUSE THE MOUSEOVER WINDOW IS A GAMEOBJECT AND WE NEED TO BE ABLE TO GLOW WHEN A WINDOW IS NOT UP AND WE ARE OVER IT
                // THIS COULD BE POTENTIALLY FIXED BY BLOCKING MOUSEOVER THE SAME WAY WE BLOCK DRAG IN THE UIMANAGER BY RESTRICTING ON MOUSEENTER ON ANY CLOSEABLEWINDOW IF IT'S TOO DISTRACTING
                // ANOTHER WAY WOULD BE DETECT NAMEPLATE UNDER MOUSE IN PLAYERCONTROLLER AND REMOVE THE AUTOMATIC MOUSEOVER RECEIVERS FROM ALL INTERACTABLES AND NAMEPLATES
                //Debug.Log(gameObject.name + ".Interactable.OnMouseEnter(): should not activate mouseover when windows are in front of things");
                return;
            }
            /*
             * the above case should handle this now - delete this code if no crashes found
            if (PlayerManager.MyInstance.MyCharacter.MyCharacterUnit == null) {
                //Debug.Log(gameObject.name + ".Interactable.OnMouseEnter(): Player Unit is not active. Cannot glow.");
                return;
            }
            */
            if (PlayerManager.MyInstance.ActiveUnitController.gameObject == gameObject) {
                return;
            }

            if (MyPrerequisitesMet == false) {
                return;
            }

            // moved to before the return statement.  This is because we still want a tooltip even if there are no current interactions to perform
            // added pivot so the tooltip doesn't bounce around
            UIManager.MyInstance.ShowToolTip(new Vector2(0, 1), UIManager.MyInstance.MouseOverWindow.transform.position, this);

            if (GetCurrentInteractables().Count == 0) {
                //if (GetValidInteractables(PlayerManager.MyInstance.MyCharacter.MyCharacterUnit).Count == 0) {
                //Debug.Log(gameObject.name + ".Interactable.OnMouseEnter(): No current Interactables.  Not glowing.");
                return;
            }

            if (glowOnMouseOver) {
                //Debug.Log(gameObject.name + ".Interactable.OnMouseEnter(): hasMeshRenderer && glowOnMouseOver == true");
                if (isFlashing == false) {
                    isFlashing = true;
                    //InitializeMaterialsOld();
                    //InitializeMaterialsNew(temporaryMaterial);
                    InitializeMaterialsNew();
                } else {
                    //Debug.Log(gameObject.name + ".Interactable.OnMouseEnter(): This object is already flashing!!!  Try to get find what event we missed and clear materials list on that event");
                }

            } else {
                //Debug.Log(gameObject.name + ".Interactable.OnMouseEnter(): hasMeshRenderer: " + hasMeshRenderer + "; glowOnMouseOver: " + glowOnMouseOver);
            }

        }

        public void OnMouseOut() {
            // renamed from OnMouseOver to OnMouseOut to stop automatic events from being received
            //Debug.Log(gameObject.name + ".Interactable.OnMouseOut()");

            if (notInteractable == true) {
                return;
            }

            if (showTooltip == false) {
                return;
            }

            if (PlayerManager.MyInstance == null) {
                return;
            }
            if (PlayerManager.MyInstance.PlayerUnitSpawned == false) {
                return;
            }

            if (PlayerManager.MyInstance.ActiveUnitController.gameObject == gameObject) {
                return;
            }

            if (MyPrerequisitesMet == false) {
                return;
            }

            // prevent moving mouse from unit to namePlate from stopping glow or hiding tooltip
            if (isMouseOverNameplate || isMouseOverUnit) {
                return;
            }

            // new mouseover code
            UIManager.MyInstance.HideToolTip();

            if (!isFlashing) {
                // there was nothing to interact with on mouseover so just exit instead of trying to reset materials
                return;
            }
            if (hasMeshRenderer) {
                isFlashing = false;
                RevertMaterialChange();
                // return emission enabled, emission color, and emission texture to their previous values
            }
        }

        protected void OnMouseDown() {
            //Debug.Log(gameObject.name + ": OnMouseDown()");
        }

        /// <summary>
        /// putting this in InteractableOptionComponent for now also
        /// </summary>
        public virtual void StopInteract() {
            //Debug.Log(gameObject.name + ".Interactable.StopInteract()");
            foreach (InteractableOptionComponent interactable in interactables) {
                interactable.StopInteract();
            }
            CloseInteractionWindow();
            isInteracting = false;
            return;
        }

        public void OnTriggerEnter(Collider other) {
            //Debug.Log(gameObject.name + ".Interactable.OnTriggerEnter(" + other.gameObject.name + ")");

            if (notInteractable == true) {
                return;
            }

            if (isTrigger) {
                UnitController unitController = other.gameObject.GetComponent<UnitController>();
                // ensure ai don't accidentally trigger interactions
                if (unitController != null && unitController == PlayerManager.MyInstance.ActiveUnitController) {
                    //Debug.Log(gameObject.name + ".Interactable.OnTriggerEnter(): triggered by player");
                    PlayerManager.MyInstance.PlayerController.InterActWithTarget(this);
                    //Interact(otherCharacterUnit);
                } else if (interactWithAny && PlayerManager.MyInstance.ActiveUnitController.CharacterUnit != null) {
                    Interact(PlayerManager.MyInstance.ActiveUnitController.CharacterUnit);
                }
            }
        }

        public void OnTriggerExit(Collider other) {
            if (notInteractable == true) {
                return;
            }

            if (isTrigger == true && interactOnExit == true) {
                UnitController unitController = other.gameObject.GetComponent<UnitController>();
                // ensure ai don't accidentally trigger interactions
                if (unitController != null && unitController == PlayerManager.MyInstance.ActiveUnitController) {
                    //Debug.Log(gameObject.name + ".Interactable.OnTriggerEnter(): triggered by player");
                    PlayerManager.MyInstance.PlayerController.InterActWithTarget(this);
                    //Interact(otherCharacterUnit);
                } else if (interactWithAny && PlayerManager.MyInstance.ActiveUnitController.CharacterUnit != null) {
                    Interact(PlayerManager.MyInstance.ActiveUnitController.CharacterUnit);
                }
            }

        }

        public void ClearFromPlayerRangeTable() {
            //Debug.Log(gameObject.name + ".Interactable.ClearFromPlayerRangeTable()");
            // prevent bugs if a unit despawns before the player moves out of range of it
            if (PlayerManager.MyInstance != null
                && PlayerManager.MyInstance.PlayerController != null
                && PlayerManager.MyInstance.ActiveUnitController != null) {
                if (PlayerManager.MyInstance.PlayerController.MyInteractables.Contains(this)) {
                    PlayerManager.MyInstance.PlayerController.MyInteractables.Remove(this);
                }
            }
        }

        public virtual string GetDescription() {
            //Debug.Log(gameObject.name + ".Interactable.GetDescription()");

            string nameString = DisplayName;
            if (DisplayName == string.Empty) {
                CharacterUnit baseCharacter = CharacterUnit.GetCharacterUnit(this);
                if (baseCharacter != null) {
                    //Debug.Log(gameObject.name + ".Interactable.GetDescription(): MyName is empty and baseCharacter exists: " + baseCharacter.MyCharacterName);
                    nameString = baseCharacter.DisplayName;
                }
            }
            Color textColor = GetDescriptionColor();
            string titleString = GetTitleString();
            return string.Format("<color=#{0}>{1}{2}</color>\n{3}", ColorUtility.ToHtmlStringRGB(textColor), nameString, titleString, GetSummary());
            // this would be where quest tracker info goes if we want to add that in the future -eg: Kill 5 skeletons : 1/5
        }

        public virtual Color GetDescriptionColor() {
            return Color.white;
        }

        public virtual string GetTitleString() {
            return string.Empty;
        }

        public virtual string GetSummary() {
            //Debug.Log(gameObject.name + ".Interactable.GetSummary()");

            string returnString = string.Empty;


            // switched this to current interactables so that we don't see mouseover options that we can't current interact with
            //List<InteractableOptionComponent> validInteractables = GetValidInteractables(PlayerManager.MyInstance.MyCharacter.MyCharacterUnit);
            List<InteractableOptionComponent> currentInteractables = GetCurrentInteractables();

            // perform default interaction or open a window if there are multiple valid interactions
            List<string> returnStrings = new List<string>();
            foreach (InteractableOptionComponent _interactable in currentInteractables) {
                //if (!(_interactable is INamePlateUnit)) {
                    // we already put the character name in the description so skip it here
                    returnStrings.Add(_interactable.GetSummary());
                //}
            }
            returnString = string.Join("\n", returnStrings);
            return string.Format("{0}", returnString);
        }

        public virtual void ProcessBeginDialog() {
        }

        public virtual void ProcessEndDialog() {
        }

        public virtual void ProcessDialogTextUpdate(string newText) {
        }

        public virtual void ProcessShowQuestIndicator(string indicatorText, QuestGiverComponent questGiverComponent) {
        }

        public virtual void ProcessHideQuestIndicator() {
        }

        public virtual void ProcessStatusIndicatorSourceInit() {
            List<InteractableOptionComponent> currentInteractables = GetCurrentInteractables();
            foreach (InteractableOptionComponent _interactable in currentInteractables) {
                //if (!(_interactable is INamePlateUnit)) {
                // we already put the character name in the description so skip it here
                _interactable.ProcessStatusIndicatorSourceInit();
                //}
            }
        }

        #region MaterialChange

        public virtual Color GetGlowColor() {
            return Color.yellow;
        }

        public void InitializeMaterialsNew() {
            //public void InitializeMaterialsNew(Material temporaryMaterial) {
            //Debug.Log(gameObject.name + ".Interactable.InitializeMaterialsNew()");
            //this.temporaryMaterial = temporaryMaterial;
            glowColor = GetGlowColor();
            //Debug.Log(gameObject.name + ".Interactable.InitializeMaterialsNew(): glowcolor set to: " + glowColor);

            meshRenderers = GetComponentsInChildren<MeshRenderer>();

            List<Renderer> tempList = new List<Renderer>();
            if (meshRenderers == null || meshRenderers.Length == 0) {
                //Debug.Log(gameObject.name + ".Interactable.InitializeMaterialsNew(): Unable to find mesh renderer in target.");
            } else {
                //Debug.Log(gameObject.name + ".Interactable.InitializeMaterialsNew(): Found " + meshRenderers.Length + " Mesh Renderers");
                foreach (Renderer renderer in meshRenderers) {
                    if (renderer.gameObject.layer != LayerMask.NameToLayer("SpellEffects")) {
                        tempList.Add(renderer);
                    }
                }
            }
            Renderer[] skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            if (skinnedMeshRenderers == null || skinnedMeshRenderers.Length == 0) {
                //Debug.Log(gameObject.name + ".Interactable.InitializeMaterialsNew(): Unable to find skinned mesh renderer in target.");
            } else {
                //Debug.Log(gameObject.name + ".Interactable.InitializeMaterialsNew(): Found " + skinnedMeshRenderers.Length + " Skinned Mesh Renderers");
                foreach (Renderer renderer in skinnedMeshRenderers) {
                    if (renderer.gameObject.layer != LayerMask.NameToLayer("SpellEffects")) {
                        tempList.Add(renderer);
                    }
                }
            }
            meshRenderers = tempList.ToArray();
            if (meshRenderers.Length != 0) {
                hasMeshRenderer = true;
            }

            PerformMaterialChange();
        }

        public void PerformMaterialChange() {
            //Debug.Log(gameObject.name + ".Interactable.PerformMaterialChange()");

            if (meshRenderers == null) {
                //Debug.Log(gameObject.name + ".MaterialChangeController.PerformMaterialChange(): meshRender is null.  This shouldn't happen because we checked before instantiating this!");
                return;
            }
            foreach (Renderer renderer in meshRenderers) {
                originalMaterials.Add(renderer, renderer.materials);
                //Debug.Log("MaterialChangeController.PerformMaterialChange(): material length: " + originalMaterials[renderer].Length);
                temporaryMaterials = new Material[originalMaterials[renderer].Length];
                //Debug.Log("MaterialChangeController.PerformMaterialChange(): temporary materials length: " + temporaryMaterials.Length);
                for (int i = 0; i < originalMaterials[renderer].Length; i++) {
                    //temporaryMaterials[i] = originalMaterials[renderer][i];
                    temporaryMaterials[i] = temporaryMaterial;
                    //enable emission and set the emission texture to none in case this item already had some type of glow mask or effect
                    //Debug.Log("Interactable.Update(): flashingmaterial: " + temporaryMaterial.name + "; emission enabled? " + temporaryMaterial.IsKeywordEnabled("_EMISSION"));
                    if (lightEmission) {
                        Debug.Log(gameObject.name + ".Interactable.PerformMaterialChange(): enabling emission");
                        temporaryMaterials[i].EnableKeyword("_EMISSION");
                        temporaryMaterials[i].SetTexture("_EmissionMap", null);
                    }
                }
                renderer.materials = temporaryMaterials;
            }
        }

        public void RevertMaterialChange() {
            //Debug.Log(gameObject.name + ".Interactable.RevertMaterialChange()");

            if (meshRenderers == null || originalMaterials.Count == 0) {
                //Debug.Log("meshRender is null.  This shouldn't happen because we checked before instantiating this!");
                return;
            }

            foreach (Renderer renderer in meshRenderers) {
                if (renderer != null) {
                    // here to prevent infestor materials that are temporary from crashing the game
                    renderer.materials = originalMaterials[renderer];
                }
            }

            originalMaterials.Clear();

            //Destroy(this);
        }

        #endregion

        public override void SetupScriptableObjects() {
            //Debug.Log(gameObject.name + ".Interactable.SetupScriptableObjects()");
            base.SetupScriptableObjects();

            //Init functionality moved to constructor : monitor for breakage
            /*
            // moved here from processInit
            foreach (InteractableOptionComponent interactable in interactables) {
                //Debug.Log(gameObject.name + ".Interactable.Awake(): Found InteractableOptionComponent: " + interactable.ToString());
                if (interactable != null) {
                    // in rare cases where a script is missing or has been made abstract, but not updated, this can return a null interactable option
                    interactable.Init();
                }
            }
            */
        }

        public virtual void OnEnable() {
            // NOTE : any interactable that gets disabled and then enabled will not have subscriptions to events from prerequisites on its options anymore
            // this could be fixed if interactables were part of a pool by re-adding the Init() method to the interactable options or something similar
            // currently the only interactables that get disabled and then re-enabled are the LunaMechs in the ManaSeal cutscene
            // and they shouldn't require any specific interactable to be subscribed to anything since they just respond to cutscene timeline events
            // and the objects that carry out those actions are permanent controllers, not interactable components
        }

        public override void OnDisable() {
            base.OnDisable();
            foreach (InteractableOptionComponent interactableOptionComponent in interactables) {
                //Debug.Log(gameObject.name + ".Interactable.Awake(): Found InteractableOptionComponent: " + interactable.ToString());
                if (interactableOptionComponent != null) {
                    // in rare cases where a script is missing or has been made abstract, but not updated, this can return a null interactable option
                    interactableOptionComponent.Cleanup();
                }
            }
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            /*
            foreach (InteractableOptionComponent interactableOptionComponent in interactables) {
                //Debug.Log(gameObject.name + ".Interactable.Awake(): Found InteractableOptionComponent: " + interactable.ToString());
                if (interactableOptionComponent != null) {
                    // in rare cases where a script is missing or has been made abstract, but not updated, this can return a null interactable option
                    interactableOptionComponent.Cleanup();
                }
            }
            */
            OnInteractableDestroy();
        }


    }

}