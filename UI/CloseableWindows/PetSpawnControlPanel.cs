using AnyRPG;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public class PetSpawnControlPanel : WindowContentController, ICapabilityConsumer {

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
        private CharacterPreviewPanelController characterPreviewPanel = null;

        [SerializeField]
        private Button spawnButton = null;

        [SerializeField]
        private Button despawnButton = null;

        [SerializeField]
        private TextMeshProUGUI nameText = null;

        [SerializeField]
        private GameObject classLabel = null;

        [SerializeField]
        private TextMeshProUGUI classText = null;

        private List<UnitProfile> unitProfileList = new List<UnitProfile>();

        private List<PetSpawnButton> petSpawnButtons = new List<PetSpawnButton>();

        //private DynamicCharacterAvatar umaAvatar;

        private UnitProfile unitProfile = null;
        private UnitType unitType = null;
        private CharacterRace characterRace = null;
        private CharacterClass characterClass = null;
        private ClassSpecialization classSpecialization = null;
        private Faction faction = null;

        private CapabilityConsumerProcessor capabilityConsumerProcessor = null;

        private int unitLevel;

        private int unitToughness;

        private CapabilityConsumerSnapshot capabilityConsumerSnapshot = null;

        public UnitProfile UnitProfile { get => unitProfile; set => unitProfile = value; }
        public UnitType UnitType { get => unitType; set => unitType = value; }
        public CharacterRace CharacterRace { get => characterRace; set => characterRace = value; }
        public CharacterClass CharacterClass { get => characterClass; set => characterClass = value; }
        public ClassSpecialization ClassSpecialization { get => classSpecialization; set => classSpecialization = value; }
        public Faction Faction { get => faction; set => faction = value; }
        public CapabilityConsumerProcessor CapabilityConsumerProcessor { get => capabilityConsumerProcessor; }

        public PetSpawnButton MySelectedPetSpawnButton { get => selectedPetSpawnButton; set => selectedPetSpawnButton = value; }
        //public List<UnitProfile> MyUnitProfileList { get => unitProfileList; set => unitProfileList = value; }

        protected void Start() {
        }

        public void ShowUnit(PetSpawnButton petSpawnButton) {
            //Debug.Log("LoadGamePanel.ShowSavedGame()");

            selectedPetSpawnButton = petSpawnButton;

            unitProfile = petSpawnButton.MyUnitProfile;
            /*
            unitType = unitProfile.UnitType;
            characterRace = unitProfile.CharacterRace;
            characterClass = unitProfile.CharacterClass;
            classSpecialization = unitProfile.ClassSpecialization;
            faction = unitProfile.Faction;
            */

            // ensure the correct unit and character model is spawned
            characterPreviewPanel.ReloadUnit();

            // testing get proper appearance
            //LoadUMARecipe();

            UpdateButtons(petSpawnButton);
            UpdateUnitInformation();
        }
   
        public void UpdateUnitInformation() {
            BaseCharacter baseCharacter = CharacterCreatorManager.MyInstance.PreviewUnitController.CharacterUnit.BaseCharacter;
            //BaseCharacter baseCharacter = PetPreviewManager.MyInstance.PreviewUnitController.GetComponent<BaseCharacter>();
            if (baseCharacter != null && baseCharacter.CharacterClass != null) {
                classText.text = baseCharacter.CharacterClass.DisplayName;
            }
            nameText.text = unitProfile.DisplayName;
        }

        public void UpdateButtons(PetSpawnButton petSpawnButton) {
            if (petSpawnButton.MyUnitProfile != null) {
                //spawnButton.enabled = true;
                //despawnButton.enabled = true;
                spawnButton.gameObject.SetActive(true);
                despawnButton.gameObject.SetActive(true);
                if (PlayerManager.MyInstance.MyCharacter.CharacterPetManager.MyActiveUnitProfiles.ContainsKey(petSpawnButton.MyUnitProfile)) {
                    spawnButton.interactable = false;
                    despawnButton.interactable = true;
                } else {
                    spawnButton.interactable = true;
                    despawnButton.interactable = false;
                }
            }
        }

        public void ClearPanel() {
            //spawnButton.enabled = false;
            //despawnButton.enabled = false;
            spawnButton.gameObject.SetActive(false);
            despawnButton.gameObject.SetActive(false);
            classLabel.SetActive(false);
            classText.text = string.Empty;
            nameText.text = "No Pets Available";
        }

        public void ClosePanel() {
            //Debug.Log("CharacterCreatorPanel.ClosePanel()");
            SystemWindowManager.MyInstance.petSpawnWindow.CloseWindow();
            PopupWindowManager.MyInstance.interactionWindow.CloseWindow();
        }

        public void HandleTargetReady() {
            //LoadUMARecipe();
            // not doing anything for now since pets don't have equipment yet
        }

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("LoadGamePanel.OnCloseWindow()");
            base.RecieveClosedWindowNotification();
            characterPreviewPanel.OnTargetReady -= HandleTargetReady;
            characterPreviewPanel.RecieveClosedWindowNotification();
            //SaveManager.MyInstance.ClearSharedData();
            OnCloseWindow(this);
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("PetSpawnControlPanel.ReceiveOpenWindowNotification()");

            ClearPanel();

            if (PlayerManager.MyInstance.MyCharacter.CharacterPetManager != null) {
                unitProfileList = PlayerManager.MyInstance.MyCharacter.CharacterPetManager.MyUnitProfiles;
            }
            ShowPreviewButtonsCommon();

            // inform the preview panel so the character can be rendered
            characterPreviewPanel.OnTargetReady += HandleTargetReady;
            characterPreviewPanel.CapabilityConsumer = this;
            characterPreviewPanel.ReceiveOpenWindowNotification();

            // this needs to be run here because the initial run in ShowLoadButtonsCommon will have done nothing because the preview panel wasn't open yet
            //LoadUMARecipe();
        }

        public void ShowPreviewButtonsCommon() {
            //Debug.Log("LoadGamePanel.ShowLoadButtonsCommon()");
            //ClearPreviewTarget();
            ClearPreviewButtons();

            foreach (UnitProfile unitProfile in unitProfileList) {
                Debug.Log("PetSpawnControlPanel.ShowLoadButtonsCommon() unitprofile: " + unitProfile.DisplayName);
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