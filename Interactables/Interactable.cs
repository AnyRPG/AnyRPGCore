using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class Interactable : Spawnable, IDescribable {

        // the physical interactable to spawn
        [SerializeField]
        private string interactableName;

        [SerializeField]
        private Sprite interactableIcon;


        public bool glowOnMouseOver = true;
        public float glowFlashSpeed = 1.5f;
        public float glowMinIntensity = 4.5f;
        public float glowMaxIntensity = 6f;
        private Color glowColor = Color.yellow;

        [SerializeField]
        private bool notInteractable = false;

        // automatically triggered by walking into it
        public bool isTrigger = false;

        //public Transform interactionTransform;
        //private Transform interactionTransform;
        //private Transform avatar;

        private IInteractable[] interactables;
        private Component meshRenderer;
        private GameObject avatar;
        //private Material[] materialList = new Material[0];

        public Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

        private Material[] temporaryMaterials = null;

        [SerializeField]
        private Material temporaryMaterial = null;

        // require a valid interactable option in addition to any preqrequisites in the spawnbable base class
        [SerializeField]
        private bool spawnRequiresValidOption = false;

        // require no valid interactable options in addition to any preqrequisites in the spawnbable base class
        [SerializeField]
        private bool despawnRequiresNoValidOption = false;

        Renderer[] meshRenderers = null;

        private List<Shader> shaderList = new List<Shader>();
        private List<Color> emissionColorList = new List<Color>();
        private List<Texture> emissionTextureList = new List<Texture>();
        private List<bool> emissionEnabledList = new List<bool>();

        private GameObject miniMapIndicator = null;

        //private List<GameObject> triggeredList = new List<GameObject>();

        protected bool isInteracting = false;
        private bool isFlashing = false;
        //bool isFocus = false;

        //bool hasInteracted = false;
        bool hasMeshRenderer = false;

        private bool miniMapIndicatorReady = false;

        private BoxCollider boxCollider;

        private INamePlateUnit namePlateUnit = null;

        public bool IsInteracting { get => isInteracting; }

        public IInteractable[] MyInteractables { get => interactables; set => interactables = value; }

        public Sprite MyIcon { get => interactableIcon; }

        public string MyName { get => (interactableName != null && interactableName != string.Empty ? interactableName : (namePlateUnit != null ? namePlateUnit.MyDisplayName : "namePlateUnit.MyDisplayname is null!!")); }
        public bool NotInteractable { get => notInteractable; set => notInteractable = value; }

        protected override void Awake() {
            //Debug.Log(gameObject.name + ".Interactable.Awake()");
            base.Awake();
            temporaryMaterials = null;
            if (temporaryMaterial == null) {
                temporaryMaterial = SystemConfigurationManager.MyInstance.MyTemporaryMaterial;
            }
            if (temporaryMaterial == null) {
                //Debug.Log("No glow materials available. overrideing glowOnMouseover to false");
                glowOnMouseOver = false;
            }

        }

        public override void Start() {
            //Debug.Log(gameObject.name + ".Interactable.Start()");
            base.Start();
        }

        public override void CleanupEverything() {
            //Debug.Log(gameObject.name + ".Interactable.CleanupEverything()");
            base.CleanupEverything();
            CleanupMiniMapIndicator();
            ClearFromPlayerRangeTable();
        }

        public override void InitializeComponents() {
            //Debug.Log(gameObject.name + ".Interactable.InitializeComponents()");

            if (componentsInitialized == true) {
                return;
            }
            base.InitializeComponents();
            boxCollider = GetComponent<BoxCollider>();
            interactables = GetComponents<IInteractable>();

            // MOVED THIS HERE FROM START
            namePlateUnit = GetComponent<INamePlateUnit>();
            if (namePlateUnit != null) {
                if (namePlateUnit.MyDisplayName != null && namePlateUnit.MyDisplayName != string.Empty) {
                    interactableName = namePlateUnit.MyDisplayName;
                }
            } else {
                //things like mining nodes have no namePlateUnit.  That's ok.  we don't want names over top of them
                //Debug.Log(gameObject.name + ".Interactable.InitializeComponents(): namePlateUnit is null");
            }

            foreach (IInteractable interactable in interactables) {
                //Debug.Log(gameObject.name + ".Interactable.Awake(): Found IInteractable: " + interactable.ToString());
            }
        }

        private void Update() {
            // if the item is highlighted, we will continue a pulsing glow
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
                            flashingMaterial.SetColor("_EmissionColor", glowColor * emission);
                            flashingMaterial.SetColor("_Color", glowColor * emission);
                        }
                    }
                }
            }
        }

        protected override bool CanSpawn() {
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

        public override void HandlePrerequisiteUpdates() {
            //Debug.Log(gameObject.name + ".Interactable.HandlePrerequisiteUpdates()");
            base.HandlePrerequisiteUpdates();
            if (!PlayerManager.MyInstance.MyPlayerUnitSpawned) {
                //Debug.Log(gameObject.name + ".Interactable.HandlePrerequisiteUpdates(): player unit not spawned.  returning");
                return;
            }
            InstantiateMiniMapIndicator();
            foreach (IInteractable _interactable in interactables) {
                _interactable.HandlePrerequisiteUpdates();
            }
            UpdateNamePlateImage();
        }

        public void UpdateNamePlateImage() {

            //Debug.Log(gameObject.name + ".Interactable.UpdateNamePlateImage()");
            if (PlayerManager.MyInstance.MyCharacter == null || PlayerManager.MyInstance.MyCharacter.MyCharacterUnit == null) {
                //Debug.Log(gameObject.name + ".Interactable.UpdateNamePlateImage(): player has no character");
                return;
            }
            if (namePlateUnit == null || namePlateUnit.MyNamePlate == null) {
                //Debug.Log(gameObject.name + ".Interactable.UpdateNamePlateImage(): nameplateUnit: " + (namePlateUnit == null ? "null" : namePlateUnit.MyDisplayName) + "; namePlateUnit.myNamePlate: " + (namePlateUnit != null && namePlateUnit.MyNamePlate != null ? namePlateUnit.MyNamePlate.name : "null"));
                return;
            }
            int currentInteractableCount = GetCurrentInteractables().Count;
            //Debug.Log(gameObject.name + ".DialogInteractable.UpdateDialogStatus(): currentInteractableCount: " + currentInteractableCount);

            // determine if one of our current interactables is a questgiver
            bool questGiverCurrent = false;
            foreach (InteractableOption interactableOption in GetCurrentInteractables()) {
                if (interactableOption is QuestGiver) {
                    questGiverCurrent = true;
                }
            }
            //Debug.Log(gameObject.name + ".DialogInteractable.UpdateDialogStatus(): MADE IT PAST QUESTIVER CHECK!!");

            if (currentInteractableCount == 0 || questGiverCurrent == true) {
                // questgiver should override all other nameplate images since it's special and appears separately
                namePlateUnit.MyNamePlate.MyGenericIndicatorImage.gameObject.SetActive(false);
                //Debug.Log(gameObject.name + ".DialogInteractable.UpdateDialogStatus(): interactable count is zero or questgiver is true");
            } else {
                //Debug.Log(gameObject.name + ".Interactable.UpdateNamePlateImage(): Our count is 1 or more");
                if (currentInteractableCount == 1) {
                    //Debug.Log(gameObject.name + ".Interactable.UpdateNamePlateImage(): Our count is 1");
                    if (GetCurrentInteractables()[0].MyNamePlateImage != null) {
                        //Debug.Log(gameObject.name + ".Interactable.UpdateNamePlateImage(): Our count is 1 and image is not null");
                        namePlateUnit.MyNamePlate.MyGenericIndicatorImage.gameObject.SetActive(true);
                        namePlateUnit.MyNamePlate.MyGenericIndicatorImage.sprite = GetCurrentInteractables()[0].MyNamePlateImage;
                    }
                } else {
                    //Debug.Log(gameObject.name + ".Interactable.UpdateNamePlateImage(): Our count is MORE THAN 1");
                    namePlateUnit.MyNamePlate.MyGenericIndicatorImage.gameObject.SetActive(true);
                    namePlateUnit.MyNamePlate.MyGenericIndicatorImage.sprite = SystemConfigurationManager.MyInstance.MyMultipleInteractionNamePlateImage;
                }
            }
        }


        public override void Spawn() {
            //Debug.Log(gameObject.name + ".Interactable.Spawn()");
            base.Spawn();

            EnableInteraction();
        }

        public void EnableInteraction() {
            if (boxCollider != null) {
                boxCollider.enabled = true;
            }
        }

        public void DisableInteraction() {
            boxCollider.enabled = false;
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
            if (!PlayerManager.MyInstance.MyPlayerUnitSpawned) {
                //Debug.Log(gameObject.name + ".Interactable.InstantiateMiniMapIndicator(): player unit not spawned yet.  returning");
                return false;
            }

            List<IInteractable> validInteractables = GetValidInteractables();
            if (validInteractables == null || validInteractables.Count == 0) {
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
                if (interactables.Length > 0) {
                    //Debug.Log(gameObject.name + ".Interactable.InstantiateMiniMapIndicator(): interactables.length > 0");
                    Vector3 spawnPosition = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + 9, gameObject.transform.position.z);
                    //miniMapIndicator = Instantiate(MiniMapController.MyInstance.MyMiniMapIndicatorPrefab, spawnPosition, Quaternion.identity, gameObject.transform);
                    miniMapIndicator = Instantiate(MiniMapController.MyInstance.MyMiniMapIndicatorPrefab, spawnPosition, Quaternion.identity, UIManager.MyInstance.MyMiniMapCanvasParent.transform);
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
                //Debug.Log(gameObject.name + ".Interactable.CleanupMiniMapIndicator(): " + miniMapIndicator.name);
                Destroy(miniMapIndicator);
                miniMapIndicatorReady = false;
            }
        }

        /*
        public Transform GetInteractionTransform() {
            return interactionTransform;
        }
        */

        public void OpenInteractionWindow() {
            //Debug.Log(gameObject.name + ".Interactable.OpenInteractionWindow");
            InteractionPanelUI.MyInstance.MyInteractable = this;
            PopupWindowManager.MyInstance.interactionWindow.OpenWindow();
        }

        public void CloseInteractionWindow() {
            InteractionPanelUI.MyInstance.MyInteractable = null;
            PopupWindowManager.MyInstance.interactionWindow.CloseWindow();
        }

        public bool CanInteract() {
            if (notInteractable == true) {
                return false;
            }
            //Debug.Log(gameObject.name + ".Interactable.CanInteract()");
            if (PlayerManager.MyInstance == null || PlayerManager.MyInstance.MyPlayerUnitSpawned == false) {
                return false;
            }
            List<IInteractable> validInteractables = GetValidInteractables();
            //List<IInteractable> validInteractables = GetValidInteractables(PlayerManager.MyInstance.MyCharacter.MyCharacterUnit);
            int validInteractableCount = 0;
            if (validInteractables != null) {
                validInteractableCount = validInteractables.Count;
            }
            if (validInteractableCount > 0) {
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Return true if the object trying to interact is in the trigger list (it is inside the collider and allowed to interact)
        /// </summary>
        /// <returns></returns>
        public virtual bool Interact(CharacterUnit source) {
            //Debug.Log(gameObject.name + ".Interactable.Interact(" + source.name + ")");
            if (notInteractable == true) {
                return false;
            }

            // get a list of valid interactables to determine if there is an action we can treat as default
            List<IInteractable> validInteractables = GetValidInteractables();
            //List<IInteractable> validInteractables = GetValidInteractables(source);
            if (validInteractables == null) {
                return false;
            }
            foreach (IInteractable validInteractable in validInteractables) {
                //Debug.Log(gameObject.name + ".Interactable.Interact(" + source.name + "): valid interactable name: " + validInteractable);
            }
            // perform default interaction or open a window if there are multiple valid interactions
            //Debug.Log(gameObject.name + ".Interactable.Interact(): validInteractables.Count: " + validInteractables.Count);
            // changed code, window will always be opened, and it will decide if to pop another one or not
            if (validInteractables.Count > 0) {
                OpenInteractionWindow();
                return true;
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


        public List<IInteractable> GetValidInteractables() {

            if (notInteractable == true) {
                return null;
            }

            //public List<IInteractable> GetValidInteractables(CharacterUnit source) {
            //Debug.Log(gameObject.name + ".Interactable.GetValidInteractables()");
            InitializeComponents();

            /*
            if (source == null) {
                //Debug.Log(gameObject.name + ".Interactable.GetValidInteractables(): source is null.  returning null!");
                return null;
            }
            */

            if (interactables == null) {
                //Debug.Log(gameObject.name + ".Interactable.GetValidInteractables(): interactables is null.  returning null!");
                return null;
            }

            List<IInteractable> validInteractables = new List<IInteractable>();
            foreach (IInteractable _interactable in interactables) {
                if (_interactable.GetValidOptionCount() > 0 && _interactable.MyPrerequisitesMet && (_interactable as MonoBehaviour).enabled == true) {
                    // HAD TO REMOVE THE FIRST CONDITION BECAUSE IT WAS BREAKING MINIMAP UPDATES - MONITOR FOR WHAT REMOVING THAT BREAKS...
                    //if (_interactable.CanInteract(source) && _interactable.GetValidOptionCount() > 0 && _interactable.MyPrerequisitesMet && (_interactable as MonoBehaviour).enabled == true) {
                    //Debug.Log(gameObject.name + ".Interactable.GetValidInteractables(): Adding valid interactable: " + _interactable.ToString());
                    validInteractables.Add(_interactable);
                } else {
                    if (_interactable.GetValidOptionCount() <= 0) {
                        //Debug.Log(gameObject.name + ".Interactable.GetValidInteractables(): invalid interactable: " + _interactable.ToString() + "; optionCount: " + _interactable.GetValidOptionCount());
                    }
                    if (!_interactable.MyPrerequisitesMet) {
                        //Debug.Log(gameObject.name + ".Interactable.GetValidInteractables(): invalid interactable: " + _interactable.ToString() + "; prerequisitesmet: " + _interactable.MyPrerequisitesMet);
                    }
                    if ((_interactable as MonoBehaviour).enabled == false) {
                        //Debug.Log(gameObject.name + ".Interactable.GetValidInteractables(): invalid interactable: " + _interactable.ToString() + "; DISABLED");
                    }

                }
            }
            return validInteractables;
        }

        public List<IInteractable> GetCurrentInteractables() {
            //Debug.Log(gameObject.name + ".Interactable.GetCurrentInteractables()");

            if (notInteractable == true) {
                return null;
            }

            InitializeComponents();

            /*
            if (source == null) {
                //Debug.Log(gameObject.name + ".Interactable.GetValidInteractables(): source is null.  returning null!");
                return null;
            }
            */

            if (interactables == null) {
                //Debug.Log(gameObject.name + ".Interactable.GetValidInteractables(): interactables is null.  returning null!");
                return null;
            }

            List<IInteractable> currentInteractables = new List<IInteractable>();
            foreach (IInteractable _interactable in interactables) {
                if (_interactable.CanInteract() && (_interactable as MonoBehaviour).enabled == true) {
                    //Debug.Log(gameObject.name + ".Interactable.GetValidInteractables(): Adding valid interactable: " + _interactable.ToString());
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
            if (notInteractable == true) {
                return;
            }

            if (PlayerManager.MyInstance == null) {
                return;
            }
            if (PlayerManager.MyInstance.MyPlayerUnitSpawned == false) {
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
            if (PlayerManager.MyInstance.MyPlayerUnitObject == gameObject) {
                return;
            }

            // moved to before the return statement.  This is because we still want a tooltip even if there are no current interactions to perform
            // added pivot so the tooltip doesn't bounce around
            UIManager.MyInstance.ShowToolTip(new Vector2(0, 1), UIManager.MyInstance.MyMouseOverWindow.transform.position, this);

            if (GetCurrentInteractables().Count == 0) {
                //if (GetValidInteractables(PlayerManager.MyInstance.MyCharacter.MyCharacterUnit).Count == 0) {
                //Debug.Log(gameObject.name + ".Interactable.OnMouseEnter(): No valid Interactables.  Not glowing.");
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

            if (PlayerManager.MyInstance == null) {
                return;
            }
            if (PlayerManager.MyInstance.MyPlayerUnitSpawned == false) {
                return;
            }

            if (PlayerManager.MyInstance.MyPlayerUnitObject == gameObject) {
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
                //RevertMaterialChangeOld();

            }
        }

        private void OnMouseDown() {
            //Debug.Log(gameObject.name + ": OnMouseDown()");
        }

        /// <summary>
        /// putting this in IInteractable for now also
        /// </summary>
        public virtual void StopInteract() {
            //Debug.Log(gameObject.name + ".Interactable.StopInteract()");
            foreach (IInteractable interactable in interactables) {
                interactable.StopInteract();
            }
            CloseInteractionWindow();
            isInteracting = false;
            return;
        }

        public void OnTriggerEnter(Collider other) {
            //Debug.Log(gameObject.name + ".Interactable.OnTriggerEnter()");

            if (notInteractable == true) {
                return;
            }

            if (isTrigger) {
                //CharacterUnit otherCharacterUnit = other.gameObject.GetComponent<CharacterUnit>();
                // changed to player to ensure ai don't accidentally trigger interactions

                //PlayerUnit otherCharacterUnit = other.gameObject.GetComponent<PlayerUnit>();

                AnimatedPlayerUnit otherCharacterUnit = other.gameObject.GetComponent<AnimatedPlayerUnit>();
                if (otherCharacterUnit != null && otherCharacterUnit.MyCharacterUnit != null) {
                    //Debug.Log(gameObject.name + ".Interactable.OnTriggerEnter(): triggered by player");
                    (otherCharacterUnit.MyCharacterUnit.MyCharacter.MyCharacterController as PlayerController).InterActWithTarget(this, gameObject);
                    //Interact(otherCharacterUnit);
                }
            }
        }

        public void ClearFromPlayerRangeTable() {
            //Debug.Log(gameObject.name + ".Interactable.ClearFromPlayerRangeTable()");
            // prevent bugs if a unit despawns before the player moves out of range of it
            if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.MyCharacter != null && PlayerManager.MyInstance.MyCharacter.MyCharacterController != null) {
                if ((PlayerManager.MyInstance.MyCharacter.MyCharacterController as PlayerController).MyInteractables.Contains(this)) {
                    (PlayerManager.MyInstance.MyCharacter.MyCharacterController as PlayerController).MyInteractables.Remove(this);
                }
            }
        }

        public virtual string GetDescription() {
            //Debug.Log(gameObject.name + ".Interactable.GetDescription()");

            string nameString = MyName;
            if (MyName == string.Empty) {
                CharacterUnit baseCharacter = GetComponent<CharacterUnit>();
                if (baseCharacter != null) {
                    //Debug.Log(gameObject.name + ".Interactable.GetDescription(): MyName is empty and baseCharacter exists: " + baseCharacter.MyCharacterName);
                    nameString = baseCharacter.MyDisplayName;
                }
            }
            Color textColor = Color.white;
            string factionString = string.Empty;
            if (namePlateUnit != null && namePlateUnit.MyFaction != null) {
                //Debug.Log(gameObject.name + ".Interactable.GetDescription(): getting color for faction: " + namePlateUnit.MyFactionName);
                textColor = Faction.GetFactionColor(namePlateUnit);
                factionString = "\n" + namePlateUnit.MyFaction.MyName;
            } else {
                //Debug.Log(gameObject.name + ".Interactable.GetDescription():  namePlateUnit is null: " + (namePlateUnit == null));
            }
            return string.Format("<color=#{0}>{1}{2}</color>\n{3}", ColorUtility.ToHtmlStringRGB(textColor), nameString, factionString, GetSummary());
            // this would be where quest tracker info goes if we want to add that in the future -eg: Kill 5 skeletons : 1/5
        }

        public virtual string GetSummary() {
            //Debug.Log(gameObject.name + ".Interactable.GetSummary()");

            string returnString = string.Empty;


            // switched this to current interactables so that we don't see mouseover options that we can't current interact with
            //List<IInteractable> validInteractables = GetValidInteractables(PlayerManager.MyInstance.MyCharacter.MyCharacterUnit);
            List<IInteractable> currentInteractables = GetCurrentInteractables();

            // perform default interaction or open a window if there are multiple valid interactions
            List<string> returnStrings = new List<string>();
            foreach (IInteractable _interactable in currentInteractables) {
                if (!(_interactable is INamePlateUnit)) {
                    // we already put the character name in the description so skip it here
                    returnStrings.Add(_interactable.GetSummary());
                }
            }
            returnString = string.Join("\n", returnStrings);
            return string.Format("{0}", returnString);
        }

        public void InitializeMaterialsNew() {
            //public void InitializeMaterialsNew(Material temporaryMaterial) {
            //Debug.Log(gameObject.name + ".Interactable.InitializeMaterialsNew()");
            //this.temporaryMaterial = temporaryMaterial;
            if (namePlateUnit != null) {
                glowColor = Faction.GetFactionColor(namePlateUnit);
                //Debug.Log(gameObject.name + ".Interactable.InitializeMaterialsNew(): glowcolor set to: " + glowColor);
            }

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
                    temporaryMaterials[i].EnableKeyword("_EMISSION");
                    temporaryMaterials[i].SetTexture("_EmissionMap", null);
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

        public void RevertMaterialChangeOld() {
            //Debug.Log(gameObject.name + ".Interactable.RevertMaterialsChangeOld()");
            for (int i = 0; i < temporaryMaterials.Length; i++) {
                temporaryMaterials[i].shader = shaderList[i];
                temporaryMaterials[i].SetTexture("_EmissionMap", emissionTextureList[i]);
                temporaryMaterials[i].SetColor("_EmissionColor", emissionColorList[i]);
                if (!emissionEnabledList[i]) {
                    temporaryMaterials[i].DisableKeyword("_EMISSION");
                }
            }
        }

        public void InitializeMaterialsOld() {
            //Debug.Log(gameObject.name + ".Interactable.InitializeMaterialsOld()");
            //avatar = transform.GetChild(0);
            if (meshRenderer != null) {
                //Debug.Log(gameObject.name + ".Interactable.InitializeMaterials(). MeshRenderer was not null");
                hasMeshRenderer = true;
                avatar = meshRenderer.transform.gameObject;
                temporaryMaterials = avatar.GetComponent<Renderer>().materials;
            }
            for (int i = 0; i < temporaryMaterials.Length; i++) {
                //Debug.Log(gameObject.name + " shader: " + materialList[i].shader);
                shaderList.Add(temporaryMaterials[i].shader);
                temporaryMaterials[i].shader = Shader.Find("Standard");
                Color tempColor = temporaryMaterials[i].GetColor("_EmissionColor");
                emissionColorList.Add(tempColor);
                //Debug.Log("Got color" + emissionColorList[i].ToString() + " on object " + transform.name);
                Texture tempTexture = temporaryMaterials[i].GetTexture("_EmissionMap");
                emissionTextureList.Add(tempTexture);
                bool tempEnabled = temporaryMaterials[i].IsKeywordEnabled("_EMISSION");
                emissionEnabledList.Add(tempEnabled);

            }
        }


        public override void SetupScriptableObjects() {
            //Debug.Log(gameObject.name + ".Interactable.SetupScriptableObjects()");
            base.SetupScriptableObjects();

            if (interactables != null) {
                foreach (IInteractable interactable in interactables) {
                    if (interactable != null) {
                        interactable.SetupScriptableObjects();
                    }
                }
            }
        }


    }

}