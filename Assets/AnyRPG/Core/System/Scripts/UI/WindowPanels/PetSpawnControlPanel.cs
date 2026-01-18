using AnyRPG;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public class PetSpawnControlPanel : WindowPanel, ICapabilityConsumer, ICharacterConfigurationProvider {

        public event System.Action OnConfirmAction = delegate { };
        public override event Action<CloseableWindowContents> OnCloseWindow = delegate { };

        private PetSpawnButton selectedPetSpawnButton;

        [SerializeField]
        private GameObject buttonPrefab = null;

        [SerializeField]
        private GameObject buttonArea = null;

        [SerializeField]
        private CharacterPreviewPanel characterPreviewPanel = null;

        [SerializeField]
        private TextMeshProUGUI nameText = null;

        [SerializeField]
        private GameObject classLabel = null;

        [SerializeField]
        private GameObject classTextBox = null;

        [SerializeField]
        private TextMeshProUGUI classText = null;

        /*
        [SerializeField]
        private HighlightButton returnButton = null;
        */

        [SerializeField]
        private HighlightButton spawnButton = null;

        [SerializeField]
        private HighlightButton despawnButton = null;


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

        //private CapabilityConsumerSnapshot capabilityConsumerSnapshot = null;

        // game manager references
        private PlayerManager playerManager = null;
        private UIManager uIManager = null;
        private ObjectPooler objectPooler = null;
        protected CharacterCreatorManager characterCreatorManager = null;
        //protected NetworkManagerClient networkManagerClient = null;

        public UnitProfile UnitProfile { get => unitProfile; set => unitProfile = value; }
        public UnitType UnitType { get => unitType; set => unitType = value; }
        public CharacterRace CharacterRace { get => characterRace; set => characterRace = value; }
        public CharacterClass CharacterClass { get => characterClass; set => characterClass = value; }
        public ClassSpecialization ClassSpecialization { get => classSpecialization; set => classSpecialization = value; }
        public Faction Faction { get => faction; set => faction = value; }
        public CapabilityConsumerProcessor CapabilityConsumerProcessor { get => capabilityConsumerProcessor; }

        public PetSpawnButton SelectedPetSpawnButton { get => selectedPetSpawnButton; set => selectedPetSpawnButton = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            /*
            returnButton.Configure(systemGameManager);
            spawnButton.Configure(systemGameManager);
            despawnButton.Configure(systemGameManager);
            */
            characterPreviewPanel.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
            uIManager = systemGameManager.UIManager;
            objectPooler = systemGameManager.ObjectPooler;
            characterCreatorManager = systemGameManager.CharacterCreatorManager;
            //networkManagerClient = systemGameManager.NetworkManagerClient;
        }

        public void ShowUnit(PetSpawnButton petSpawnButton) {
            //Debug.Log($"PetSpawnControlPanel.ShowUnit({petSpawnButton.UnitProfile.ResourceName})");

            selectedPetSpawnButton = petSpawnButton;

            unitProfile = petSpawnButton.UnitProfile;

            // ensure the correct unit and character model is spawned
            characterPreviewPanel.ReloadUnit();

            UpdateButtons(petSpawnButton);
            UpdateUnitInformation();

            petSpawnButton.HighlightBackground();
            uINavigationControllers[0].UnHightlightButtonBackgrounds(petSpawnButton);
        }
   
        public void UpdateUnitInformation() {
            //Debug.Log($"PetSpawnControlPanel.UpdateUnitInformation()");

            nameText.text = (unitProfile.CharacterName != string.Empty ? unitProfile.CharacterName : unitProfile.DisplayName);

            if (unitProfile.CharacterClass == null) {
                classLabel.SetActive(false);
                classTextBox.SetActive(false);
                return;
            }

            classLabel.SetActive(true);
            classTextBox.SetActive(true);
            classText.text = unitProfile.CharacterClass.DisplayName;
        }

        public void UpdateButtons(PetSpawnButton petSpawnButton) {
            if (petSpawnButton.UnitProfile != null) {
                spawnButton.gameObject.SetActive(true);
                despawnButton.gameObject.SetActive(true);
                if (playerManager.UnitController.CharacterPetManager.ActiveUnitProfiles.ContainsKey(petSpawnButton.UnitProfile)) {
                    spawnButton.Button.interactable = false;
                    despawnButton.Button.interactable = true;
                } else {
                    spawnButton.Button.interactable = true;
                    despawnButton.Button.interactable = false;
                }
            }
            uINavigationControllers[1].UpdateNavigationList();
        }

        public void ClearPanel() {
            unitProfile = null;
            unitType = null;
            characterRace = null;
            characterClass = null;
            classSpecialization = null;
            faction = null;
            spawnButton.gameObject.SetActive(false);
            despawnButton.gameObject.SetActive(false);
            classLabel.SetActive(false);
            classTextBox.SetActive(false);
            classText.text = string.Empty;
            nameText.text = "No Pets Available";
        }

        public void ClosePanel() {
            //Debug.Log("CharacterCreatorPanel.ClosePanel()");
            uIManager.petSpawnWindow.CloseWindow();
            uIManager.interactionWindow.CloseWindow();
        }

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("LoadGamePanel.OnCloseWindow()");
            base.ReceiveClosedWindowNotification();
            //characterPreviewPanel.OnTargetReady -= HandleTargetReady;
            systemEventManager.OnRemoveActivePet -= HandleRemoveActivePet;
            characterPreviewPanel.ReceiveClosedWindowNotification();
            OnCloseWindow(this);
            characterCreatorManager.DisableLight();
        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("PetSpawnControlPanel.ProcessOpenWindowNotification()");

            base.ProcessOpenWindowNotification();
            ClearPanel();
            systemEventManager.OnRemoveActivePet += HandleRemoveActivePet;
            if (playerManager.UnitController.CharacterPetManager != null) {
                //Debug.Log($"PetSpawnControlPanel.ProcessOpenWindowNotification() setting unit profile list count : {playerManager.UnitController.CharacterPetManager.UnitProfiles.Count}");
                unitProfileList = playerManager.UnitController.CharacterPetManager.UnitProfiles;
            }

            // inform the preview panel so the character can be rendered
            //characterPreviewPanel.OnTargetReady += HandleTargetReady;
            characterPreviewPanel.CharacterConfigurationProvider = this;
            characterPreviewPanel.ReceiveOpenWindowNotification();
            characterCreatorManager.EnableLight();

            ShowPreviewButtonsCommon();
        }

        private void HandleRemoveActivePet(UnitProfile profile) {
            UpdateButtons(SelectedPetSpawnButton);
        }

        public void ShowPreviewButtonsCommon() {
            //Debug.Log("PetSpawnControlPanel.ShowPreviewButtonsCommon()");
            //ClearPreviewTarget();
            ClearPreviewButtons();

            foreach (UnitProfile unitProfile in unitProfileList) {
                //Debug.Log($"PetSpawnControlPanel.ShowPreviewButtonsCommon() unitprofile: {unitProfile.DisplayName}");
                if (playerManager.UnitController.CharacterPetManager.ValidPetTypeList.Contains(unitProfile.UnitType) == true) {
                    GameObject go = objectPooler.GetPooledObject(buttonPrefab, buttonArea.transform);
                    PetSpawnButton petSpawnButton = go.GetComponent<PetSpawnButton>();
                    if (petSpawnButton != null) {
                        petSpawnButton.Configure(systemGameManager);
                        petSpawnButton.PetSpawnControlPanel = this;
                        petSpawnButton.AddUnitProfile(unitProfile);
                        petSpawnButtons.Add(petSpawnButton);
                        uINavigationControllers[0].AddActiveButton(petSpawnButton);
                    }
                } else {
                    //Debug.Log($"PetSpawnControlPanel.ShowPreviewButtonsCommon() unitprofile: {unitProfile.DisplayName} NOT IN VALID PET TYPE LIST");
                }
            }
            if (petSpawnButtons.Count > 0) {
                SetNavigationController(uINavigationControllers[0]);

                //petSpawnButtons[0].Select();
            }
            //SetPreviewTarget();
        }

        public void ClearPreviewButtons() {
            // clear the quest list so any quests left over from a previous time opening the window aren't shown
            //Debug.Log("LoadGamePanel.ClearLoadButtons()");
            foreach (PetSpawnButton petSpawnButton in petSpawnButtons) {
                if (petSpawnButton != null) {
                    petSpawnButton.DeSelect();
                    objectPooler.ReturnObjectToPool(petSpawnButton.gameObject);
                }
            }
            petSpawnButtons.Clear();
            uINavigationControllers[0].ClearActiveButtons();
            SelectedPetSpawnButton = null;
            nameText.text = "";
        }

        public void SpawnUnit() {
            playerManager.RequestSpawnPet(SelectedPetSpawnButton.UnitProfile);
            ClosePanel();
        }

        public void DespawnUnit() {
            playerManager.RequestDespawnPet(SelectedPetSpawnButton.UnitProfile);
            //ClosePanel();
        }

        public CharacterConfigurationRequest GetCharacterConfigurationRequest() {
            CharacterConfigurationRequest characterConfigurationRequest = new CharacterConfigurationRequest(this);
            return characterConfigurationRequest;
        }
    }

}