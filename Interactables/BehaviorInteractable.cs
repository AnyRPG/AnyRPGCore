using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class BehaviorInteractable : InteractableOption {

        public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        public override Sprite MyIcon { get => (SystemConfigurationManager.MyInstance.MyDialogInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyDialogInteractionPanelImage : base.MyIcon); }
        public override Sprite MyNamePlateImage { get => (SystemConfigurationManager.MyInstance.MyDialogNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyDialogNamePlateImage : base.MyNamePlateImage); }

        private BoxCollider boxCollider;

        [SerializeField]
        private List<string> behaviorNames = new List<string>();

        // instantiate a new behavior profile or not when loading behavior profiles
        [SerializeField]
        private bool useBehaviorCopy = false;

        //[SerializeField]
        private List<BehaviorProfile> behaviorList = new List<BehaviorProfile>();

        private int behaviorIndex = 0;

        private float maxBehaviorTime = 300f;

        private Coroutine behaviorCoroutine = null;

        private bool suppressNameplateImage = false;

        public int MyBehaviorIndex { get => behaviorIndex; }
        public List<BehaviorProfile> MyDialogList { get => behaviorList; set => behaviorList = value; }


        protected override void Awake() {
            //Debug.Log("NameChangeInteractable.Awake()");
            base.Awake();
        }

        protected override void Start() {
            //Debug.Log(gameObject.name + ".BehaviorInteractable.Start()");
            base.Start();
            boxCollider = GetComponent<BoxCollider>();
            CreateEventSubscriptions();
            Spawn();
            HandlePrerequisiteUpdates();
        }

        private void CreateEventSubscriptions() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.MyInstance.OnPlayerUnitSpawn += HandlePrerequisiteUpdates;
            if (PlayerManager.MyInstance.MyPlayerUnitSpawned == true) {
                //Debug.Log(gameObject.name + ".BehaviorInteractable.CreateEventSubscriptions(): player unit is already spawned.");
                HandlePrerequisiteUpdates();
            } else {
                //Debug.Log(gameObject.name + ".BehaviorInteractable.CreateEventSubscriptions(): player unit is not yet spawned");
            }
            eventSubscriptionsInitialized = true;
        }

        public override void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            base.CleanupEventSubscriptions();
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.MyInstance.OnPlayerUnitSpawn -= HandlePrerequisiteUpdates;
            }
            eventSubscriptionsInitialized = false;
        }

        public override void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            base.OnDisable();
            CleanupEventSubscriptions();
            CleanupDialog();
        }

        public void CleanupEventSubscriptions(ICloseableWindowContents windowContents) {
            //Debug.Log(gameObject.name + ".NameChangeInteractable.CleanupEventSubscriptions(ICloseableWindowContents)");
            CleanupEventSubscriptions();
        }


        private void Spawn() {
            //Debug.Log(gameObject.name + ".BehaviorInteractable.Spawn()");
            if (boxCollider != null) {
                boxCollider.enabled = true;
            }
            //interactable.InitializeMaterials();
            MiniMapStatusUpdateHandler(this);
        }

        private void DestroySpawn() {
            //Debug.Log(gameObject.name + ".NameChangeInteractable.DestroySpawn()");
            boxCollider.enabled = false;
            MiniMapStatusUpdateHandler(this);
        }

        public List<BehaviorProfile> GetCurrentOptionList() {
            //Debug.Log("BehaviorInteractable.GetCurrentOptionList()");
            List<BehaviorProfile> currentList = new List<BehaviorProfile>();
            foreach (BehaviorProfile behaviorProfile in behaviorList) {
                if (behaviorProfile.MyPrerequisitesMet == true && behaviorProfile.MyCompleted == false) {
                    //Debug.Log("BehaviorInteractable.GetCurrentOptionList() adding behaviorProfile " + behaviorProfile.MyName + "; id: " + behaviorProfile.GetInstanceID());
                    currentList.Add(behaviorProfile);
                }
            }
            //Debug.Log("BehaviorInteractable.GetValidOptionList(): List Size: " + validList.Count);
            return currentList;
        }

        public override bool Interact(CharacterUnit source) {
            //Debug.Log(gameObject.name + ".BehaviorInteractable.Interact()");
            List<BehaviorProfile> currentList = GetCurrentOptionList();
            if (currentList.Count == 0) {
                return false;
            } else if (currentList.Count == 1) {
                TryPlayBehavior(currentList[0]);
                base.Interact(source);
                interactable.CloseInteractionWindow();
            } else {
                interactable.OpenInteractionWindow();
            }
            return true;
        }

        private void TryPlayBehavior(BehaviorProfile behaviorProfile) {
            //Debug.Log(gameObject.name + ".BehaviorInteractable.TryPlayBehavior()");
            if (behaviorCoroutine == null) {
                behaviorCoroutine = StartCoroutine(playBehavior(behaviorProfile));
            }
        }

        private void CleanupDialog() {
            //nameplate
            if (behaviorCoroutine != null) {
                StopCoroutine(behaviorCoroutine);
            }
            behaviorCoroutine = null;
            if (namePlateUnit != null && namePlateUnit.MyNamePlate != null) {
                namePlateUnit.MyNamePlate.HideSpeechBubble();
            }
        }

        public IEnumerator playBehavior(BehaviorProfile behaviorProfile) {
            //Debug.Log(gameObject.name + ".BehaviorInteractable.playBehavior(" + (behaviorProfile == null ? "null" : behaviorProfile.MyName) + ")");
            float elapsedTime = 0f;
            behaviorIndex = 0;
            BehaviorNode currentbehaviorNode = null;
            suppressNameplateImage = true;
            interactable.UpdateNamePlateImage();
            while (behaviorIndex < behaviorProfile.MyBehaviorNodes.Count) {
                foreach (BehaviorNode behaviorNode in behaviorProfile.MyBehaviorNodes) {
                    if (behaviorNode.MyStartTime <= elapsedTime && behaviorNode.MyCompleted == false) {
                        currentbehaviorNode = behaviorNode;

                        if (currentbehaviorNode.MyBehaviorActionNodes != null) {
                            foreach (BehaviorActionNode behaviorActionNode in currentbehaviorNode.MyBehaviorActionNodes) {
                                if (behaviorActionNode.MyBehaviorMethod != null && behaviorActionNode.MyBehaviorMethod != string.Empty) {
                                    //Debug.Log(gameObject.name + ".BehaviorInteractable.playBehavior(): sending Message " + behaviorActionNode.MyBehaviorMethod + "(" + behaviorActionNode.MyBehaviorParameter + ")");
                                    if (behaviorActionNode.MyBehaviorParameter != null && behaviorActionNode.MyBehaviorParameter != string.Empty) {
                                        gameObject.SendMessage(behaviorActionNode.MyBehaviorMethod, behaviorActionNode.MyBehaviorParameter, SendMessageOptions.DontRequireReceiver);
                                    } else {
                                        gameObject.SendMessage(behaviorActionNode.MyBehaviorMethod, SendMessageOptions.DontRequireReceiver);
                                    }
                                }
                            }
                        }

                        behaviorNode.MyCompleted = true;
                        behaviorIndex++;
                    }
                }
                elapsedTime += Time.deltaTime;

                // circuit breaker
                if (elapsedTime >= maxBehaviorTime) {
                    break;
                }
                yield return null;
            }
            //Debug.Log(gameObject.name + ".BehaviorInteractable.playBehavior(" + (behaviorProfile == null ? "null" : behaviorProfile.MyName) + ") : END LOOP");
            behaviorCoroutine = null;
            suppressNameplateImage = false;
            behaviorProfile.MyCompleted = true;
            interactable.UpdateNamePlateImage();

        }

        public override bool CanInteract() {
            //Debug.Log(gameObject.name + ".BehaviorInteractable.CanInteract()");
            if (!base.CanInteract()) {
                return false;
            }
            if (GetCurrentOptionList().Count == 0 || suppressNameplateImage == true) {
                return false;
            }
            return true;

        }

        /// <summary>
        /// Pick an item up off the ground and put it in the inventory
        /// </summary>

        public override void StopInteract() {
            base.StopInteract();
            PopupWindowManager.MyInstance.dialogWindow.CloseWindow();
        }

        public override bool HasMiniMapText() {
            return true;
        }

        public override bool SetMiniMapText(Text text) {
            if (!base.SetMiniMapText(text)) {
                text.text = "";
                text.color = new Color32(0, 0, 0, 0);
                return false;
            }
            text.text = "o";
            text.fontSize = 50;
            text.color = Color.white;
            return true;
        }

        public override int GetCurrentOptionCount() {
            //Debug.Log(gameObject.name + ".BehaviorInteractable.GetCurrentOptionCount()");
            if (behaviorCoroutine == null) {
                return GetCurrentOptionList().Count;
            } else {
                return 0;
            }
        }

        public override void HandlePrerequisiteUpdates() {
            //Debug.Log(gameObject.name + ".BehaviorInteractable.HandlePrerequisiteUpdates()");
            base.HandlePrerequisiteUpdates();
            MiniMapStatusUpdateHandler(this);
            PlayAutomaticBehaviors();
        }

        public void PlayAutomaticBehaviors() {
            //Debug.Log(gameObject.name + ".BehaviorInteractable.PlayAutomaticBehaviors()");
            foreach (BehaviorProfile behaviorProfile in GetCurrentOptionList()) {
                if (behaviorProfile.MyAutomatic == true && behaviorProfile.MyCompleted == false) {
                    TryPlayBehavior(behaviorProfile);
                }
            }
        }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            behaviorList = new List<BehaviorProfile>();
            if (behaviorNames != null) {
                foreach (string behaviorName in behaviorNames) {
                    BehaviorProfile tmpBehaviorProfile = null;
                    if (useBehaviorCopy == true) {
                        tmpBehaviorProfile = SystemBehaviorProfileManager.MyInstance.GetNewResource(behaviorName);
                    } else {
                        tmpBehaviorProfile = SystemBehaviorProfileManager.MyInstance.GetResource(behaviorName);
                    }
                    if (tmpBehaviorProfile != null) {
                        behaviorList.Add(tmpBehaviorProfile);
                    }
                }
            }
        }

        public void StopBackgroundMusic() {
            AudioManager.MyInstance.StopMusic();
        }

        public void StartBackgroundMusic() {
            LevelManager.MyInstance.PlayLevelSounds();
        }


    }

}