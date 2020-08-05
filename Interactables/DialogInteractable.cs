using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class DialogInteractable : InteractableOption {

        public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        public override Sprite MyIcon { get => (SystemConfigurationManager.MyInstance.MyDialogInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyDialogInteractionPanelImage : base.MyIcon); }
        public override Sprite MyNamePlateImage { get => (SystemConfigurationManager.MyInstance.MyDialogNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyDialogNamePlateImage : base.MyNamePlateImage); }

        public override string InteractionPanelTitle {
            get {
                List<Dialog> currentList = GetCurrentOptionList();
                if (currentList.Count > 0) {
                    return currentList[0].DisplayName;
                }
                return base.InteractionPanelTitle;
            }
            set => base.InteractionPanelTitle = value;
        }

        [Header("Dialog")]

        [Tooltip("The names of the dialogs available to this interactable")]
        [SerializeField]
        private List<string> dialogNames = new List<string>();

        private List<Dialog> dialogList = new List<Dialog>();

        private int dialogIndex = 0;

        private float maxDialogTime = 300f;

        private Coroutine dialogCoroutine = null;

        private BoxCollider boxCollider;

        public int MyDialogIndex { get => dialogIndex; }
        public List<Dialog> MyDialogList { get => dialogList; set => dialogList = value; }


        protected override void Awake() {
            //Debug.Log("NameChangeInteractable.Awake()");
            base.Awake();
        }

        protected override void Start() {
            //Debug.Log("DialogInteractable.Start()");
            base.Start();
            boxCollider = GetComponent<BoxCollider>();
            CreateEventSubscriptions();
            AddUnitProfileSettings();

            Spawn();
        }

        public void AddUnitProfileSettings() {
            CharacterUnit characterUnit = GetComponent<CharacterUnit>();
            if (characterUnit != null && characterUnit.MyCharacter != null && characterUnit.MyCharacter.UnitProfile != null) {
                if (characterUnit.MyCharacter.UnitProfile.DialogList != null) {
                    foreach (Dialog dialog in characterUnit.MyCharacter.UnitProfile.DialogList) {
                        dialog.RegisterPrerequisiteOwner(this);
                        dialogList.Add(dialog);
                    }
                }
            }

            // testing - add handle prerequisiteupdates here
            // this is necessary because addUnitProfileSettings is called late in startup order
            HandlePrerequisiteUpdates();
        }

        public override void CreateEventSubscriptions() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            base.CreateEventSubscriptions();
        }

        public override void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            base.CleanupEventSubscriptions();
            CleanupConfirm();
        }

        public override void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            base.OnDisable();
            CleanupEventSubscriptions();
            CleanupDialog();
        }

        /*
        public void CleanupEventSubscriptions(ICloseableWindowContents windowContents) {
            //Debug.Log(gameObject.name + ".NameChangeInteractable.CleanupEventSubscriptions(ICloseableWindowContents)");
            CleanupEventSubscriptions();
        }
        */

        public override void HandleConfirmAction() {
            //Debug.Log(gameObject.name + ".NameChangeInteractable.HandleConfirmAction()");
            base.HandleConfirmAction();
            // just to be safe
            CleanupConfirm();

            // since the dialog completion status is itself a form of prerequisite, we should call the prerequisite update here
            HandlePrerequisiteUpdates();
        }

        public void CleanupConfirm() {
            if (PopupWindowManager.MyInstance != null && PopupWindowManager.MyInstance.dialogWindow != null && PopupWindowManager.MyInstance.dialogWindow.MyCloseableWindowContents != null) {
                (PopupWindowManager.MyInstance.dialogWindow.MyCloseableWindowContents as DialogPanelController).OnConfirmAction -= HandleConfirmAction;
                (PopupWindowManager.MyInstance.dialogWindow.MyCloseableWindowContents as DialogPanelController).OnCloseWindow -= CleanupConfirm;
            }
        }

        public void CleanupConfirm(ICloseableWindowContents contents) {
            CleanupConfirm();
        }

        private void Spawn() {
            //Debug.Log(gameObject.name + ".DialogInteractable.Spawn()");
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

        public List<Dialog> GetCurrentOptionList() {
            //Debug.Log(gameObject.name + ".DialogInteractable.GetCurrentOptionList()");
            List<Dialog> currentList = new List<Dialog>();
            foreach (Dialog dialog in dialogList) {
                if (dialog.MyPrerequisitesMet == true && (dialog.TurnedIn == false || dialog.Repeatable == true)) {
                    currentList.Add(dialog);
                }
            }
            //Debug.Log(gameObject.name + ".DialogInteractable.GetCurrentOptionList(): List Size: " + currentList.Count);
            return currentList;
        }

        public override bool Interact(CharacterUnit source) {
            //Debug.Log(gameObject.name + ".DialogInteractable.Interact()");
            List<Dialog> currentList = GetCurrentOptionList();
            if (currentList.Count == 0) {
                return false;
            } else if (currentList.Count == 1) {
                if (currentList[0].MyAutomatic) {
                    if (dialogCoroutine == null) {
                        dialogCoroutine = StartCoroutine(playDialog(currentList[0]));
                    }
                } else {
                    (PopupWindowManager.MyInstance.dialogWindow.MyCloseableWindowContents as DialogPanelController).Setup(currentList[0], this.interactable);
                    (PopupWindowManager.MyInstance.dialogWindow.MyCloseableWindowContents as DialogPanelController).OnConfirmAction += HandleConfirmAction;
                    (PopupWindowManager.MyInstance.dialogWindow.MyCloseableWindowContents as DialogPanelController).OnCloseWindow += CleanupConfirm;
                }
            } else {
                interactable.OpenInteractionWindow();
            }
            base.Interact(source);
            return true;
        }

        private void CleanupDialog() {
            //nameplate
            if (dialogCoroutine != null) {
                StopCoroutine(dialogCoroutine);
            }
            dialogCoroutine = null;
            if (namePlateUnit != null && namePlateUnit.MyNamePlate != null) {
                namePlateUnit.MyNamePlate.HideSpeechBubble();
            }
        }

        public void BeginDialog(string dialogName) {
            // is there a better way avoid runtime lookups like requiring the dialog to already be available on the character?
            Dialog tmpDialog = SystemDialogManager.MyInstance.GetResource(dialogName);
            if (tmpDialog != null) {
                dialogCoroutine = StartCoroutine(playDialog(tmpDialog));
            }
        }

        public IEnumerator playDialog(Dialog dialog) {
            if (namePlateUnit != null && namePlateUnit.MyNamePlate != null) {
                namePlateUnit.MyNamePlate.ShowSpeechBubble();
            }
            float elapsedTime = 0f;
            dialogIndex = 0;
            DialogNode currentdialogNode = null;

            // this needs to be reset to allow for repeatable dialogs to replay
            dialog.ResetStatus();

            while (dialog.TurnedIn == false) {
                foreach (DialogNode dialogNode in dialog.MyDialogNodes) {
                    if (dialogNode.MyStartTime <= elapsedTime && dialogNode.Shown == false) {
                        currentdialogNode = dialogNode;
                        if (namePlateUnit != null && namePlateUnit.MyNamePlate != null) {
                            namePlateUnit.MyNamePlate.SetSpeechText(dialogNode.MyDescription);
                        }
                        if (unitAudio != null && dialog.MyAudioProfile != null && dialog.MyAudioProfile.AudioClips != null && dialog.MyAudioProfile.AudioClips.Count > dialogIndex) {
                            unitAudio.PlayVoice(dialog.MyAudioProfile.AudioClips[dialogIndex]);
                        }
                        if (CombatLogUI.MyInstance != null) {
                            CombatLogUI.MyInstance.WriteChatMessage(dialogNode.MyDescription);
                        }

                        dialogNode.Shown = true;
                        dialogIndex++;
                    }
                }
                if (dialogIndex >= dialog.MyDialogNodes.Count) {
                    dialog.TurnedIn = true;
                    HandleConfirmAction();
                }
                elapsedTime += Time.deltaTime;

                // circuit breaker
                if (elapsedTime >= maxDialogTime) {
                    break;
                }
                yield return null;
                dialogCoroutine = null;
            }

            if (currentdialogNode != null) {
                yield return new WaitForSeconds(currentdialogNode.MyShowTime);
            }
            if (namePlateUnit != null && namePlateUnit.MyNamePlate != null) {
                namePlateUnit.MyNamePlate.HideSpeechBubble();
            }
        }

        public override bool CanInteract() {
            //Debug.Log(gameObject.name + ".DialogInteractable.CanInteract()");
            if (!base.CanInteract()) {
                return false;
            }
            if (GetCurrentOptionList().Count == 0) {
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

        public override bool SetMiniMapText(TextMeshProUGUI text) {
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
            //Debug.Log(gameObject.name + ".DialogInteractable.GetCurrentOptionCount(): " + GetCurrentOptionList().Count);
            return GetCurrentOptionList().Count;
        }

        public override void HandlePrerequisiteUpdates() {
            base.HandlePrerequisiteUpdates();
            MiniMapStatusUpdateHandler(this);
        }

        public override void HandlePlayerUnitSpawn() {
            base.HandlePlayerUnitSpawn();
            UpdateDialogStatuses();
            MiniMapStatusUpdateHandler(this);
        }

        public void UpdateDialogStatuses() {
            foreach (Dialog dialog in dialogList) {
                dialog.UpdatePrerequisites(false);
            }

            bool preRequisitesUpdated = false;
            foreach (Dialog dialog in dialogList) {
                if (dialog.MyPrerequisitesMet == true) {
                    preRequisitesUpdated = true;
                }
            }

            if (preRequisitesUpdated) {
                HandlePrerequisiteUpdates();
            }

        }


        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            dialogList = new List<Dialog>();
            if (dialogNames != null) {
                foreach (string dialogName in dialogNames) {
                    Dialog tmpDialog = SystemDialogManager.MyInstance.GetResource(dialogName);
                    if (tmpDialog != null) {
                        tmpDialog.RegisterPrerequisiteOwner(this);
                        dialogList.Add(tmpDialog);
                    } else {
                        Debug.LogError(gameObject.name + ".SetupScriptableObjects(): Could not find dialog " + dialogName + " while initializing Dialog Interactable.");
                    }
                }
            }
        }


    }

}