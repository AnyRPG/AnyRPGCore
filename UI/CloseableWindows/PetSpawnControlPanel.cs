using AnyRPG;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public class PetSpawnControlPanel : WindowContentController {

        #region Singleton
        private static PetSpawnControlPanel instance;

        public static PetSpawnControlPanel MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<PetSpawnControlPanel>();
                }

                return instance;
            }
        }

        #endregion

        public event System.Action OnConfirmAction = delegate { };
        public override event Action<ICloseableWindowContents> OnCloseWindow = delegate { };

        private PetSpawnButton selectedPetSpawnButton;

        [SerializeField]
        private GameObject buttonPrefab = null;

        [SerializeField]
        private GameObject buttonArea = null;

        [SerializeField]
        private PetPreviewCameraController previewCameraController = null;

        [SerializeField]
        private Button spawnButton = null;

        [SerializeField]
        private Button despawnButton = null;

        [SerializeField]
        private TextMeshProUGUI classText = null;

        private List<UnitProfile> unitProfileList = new List<UnitProfile>();

        private List<PetSpawnButton> petSpawnButtons = new List<PetSpawnButton>();

        //private DynamicCharacterAvatar umaAvatar;

        private int unitLevel;

        private int unitToughness;

        public PetPreviewCameraController MyPreviewCameraController { get => previewCameraController; set => previewCameraController = value; }
        public PetSpawnButton MySelectedPetSpawnButton { get => selectedPetSpawnButton; set => selectedPetSpawnButton = value; }
        //public List<UnitProfile> MyUnitProfileList { get => unitProfileList; set => unitProfileList = value; }

        protected void Start() {
        }

        public void ShowUnit(PetSpawnButton petSpawnButton) {
            //Debug.Log("LoadGamePanel.ShowSavedGame()");

            selectedPetSpawnButton = petSpawnButton;

            ClearPreviewTarget();
            SetPreviewTarget();
            UpdateButtons(petSpawnButton);
            UpdateUnitInformation();
        }

        public void UpdateUnitInformation() {
            BaseCharacter baseCharacter = PetPreviewManager.MyInstance.PreviewUnitController.GetComponent<BaseCharacter>();
            if (baseCharacter != null && baseCharacter.CharacterClass != null) {
                classText.text = baseCharacter.CharacterClass.DisplayName;
            }
        }

        public void UpdateButtons(PetSpawnButton petSpawnButton) {
            if (petSpawnButton.MyUnitProfile != null) {
                if (PlayerManager.MyInstance.MyCharacter.CharacterPetManager.MyActiveUnitProfiles.ContainsKey(petSpawnButton.MyUnitProfile)) {
                    spawnButton.interactable = false;
                    despawnButton.interactable = true;
                } else {
                    spawnButton.interactable = true;
                    despawnButton.interactable = false;
                }
            }
        }

        public void ClearPreviewTarget() {
            //Debug.Log("LoadGamePanel.ClearPreviewTarget()");
            // not really close window, but it will despawn the preview unit
            PetPreviewManager.MyInstance.HandleCloseWindow();
        }

        public void SetPreviewTarget() {
            //Debug.Log("CharacterPanel.SetPreviewTarget()");
            if (PetPreviewManager.MyInstance.PreviewUnitController != null) {
                //Debug.Log("CharacterPanel.SetPreviewTarget() UMA avatar is already spawned!");
                return;
            }
            //spawn correct preview unit
            PetPreviewManager.MyInstance.HandleOpenWindow();

            if (CameraManager.MyInstance != null && CameraManager.MyInstance.UnitPreviewCamera != null) {
                //Debug.Log("CharacterPanel.SetPreviewTarget(): preview camera was available, setting target");
                if (MyPreviewCameraController != null) {
                    MyPreviewCameraController.InitializeCamera(PetPreviewManager.MyInstance.PreviewUnitController);
                    //Debug.Log("CharacterPanel.SetPreviewTarget(): preview camera was available, setting Target Ready Callback");
                    MyPreviewCameraController.OnTargetReady += TargetReadyCallback;
                } else {
                    Debug.LogError("UnitSpawnController.SetPreviewTarget(): Character Preview Camera Controller is null. Please set it in the inspector");
                }
            }
        }

        public void TargetReadyCallback() {
            //Debug.Log("CharacterCreatorPanel.TargetReadyCallback()");
            MyPreviewCameraController.OnTargetReady -= TargetReadyCallback;

        }

        public void ClosePanel() {
            //Debug.Log("CharacterCreatorPanel.ClosePanel()");
            SystemWindowManager.MyInstance.petSpawnWindow.CloseWindow();
            PopupWindowManager.MyInstance.interactionWindow.CloseWindow();
        }

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("LoadGamePanel.OnCloseWindow()");
            base.RecieveClosedWindowNotification();
            previewCameraController.ClearTarget();
            PetPreviewManager.MyInstance.HandleCloseWindow();
            //SaveManager.MyInstance.ClearSharedData();
            OnCloseWindow(this);
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("PetSpawnControlPanel.ReceiveOpenWindowNotification()");

            if (PlayerManager.MyInstance.MyCharacter.CharacterPetManager != null) {
                unitProfileList = PlayerManager.MyInstance.MyCharacter.CharacterPetManager.MyUnitProfiles;
            }
            ShowPreviewButtonsCommon();
        }

        public void ShowPreviewButtonsCommon() {
            //Debug.Log("LoadGamePanel.ShowLoadButtonsCommon()");
            ClearPreviewTarget();
            ClearPreviewButtons();

            foreach (UnitProfile unitProfile in unitProfileList) {
                //Debug.Log("LoadGamePanel.ShowLoadButtonsCommon(): setting a button with saved game data");
                GameObject go = Instantiate(buttonPrefab, buttonArea.transform);
                PetSpawnButton petSpawnButton = go.GetComponent<PetSpawnButton>();
                if (petSpawnButton != null) {
                    petSpawnButton.AddUnitProfile(unitProfile);
                    petSpawnButtons.Add(petSpawnButton);
                }

            }
            if (petSpawnButtons.Count > 0) {
                petSpawnButtons[0].Select();
            }
            //SetPreviewTarget();
        }

        public void ClearPreviewButtons() {
            // clear the quest list so any quests left over from a previous time opening the window aren't shown
            //Debug.Log("LoadGamePanel.ClearLoadButtons()");
            foreach (PetSpawnButton petSpawnButton in petSpawnButtons) {
                if (petSpawnButton != null) {
                    Destroy(petSpawnButton.gameObject);
                }
            }
            petSpawnButtons.Clear();
            MySelectedPetSpawnButton = null;
        }

        public void SpawnUnit() {
            PlayerManager.MyInstance.MyCharacter.CharacterPetManager.SpawnPet(MySelectedPetSpawnButton.MyUnitProfile);
            ClosePanel();
        }

        public void DespawnUnit() {
            PlayerManager.MyInstance.MyCharacter.CharacterPetManager.DespawnPet(MySelectedPetSpawnButton.MyUnitProfile);
            UpdateButtons(selectedPetSpawnButton);
            //ClosePanel();
        }

    }

}