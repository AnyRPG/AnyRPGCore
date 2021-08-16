using AnyRPG;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public class PetSpawnControlPanel : WindowContentController, ICapabilityConsumer {

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
        private TextMeshProUGUI nameText = null;

        [SerializeField]
        private GameObject classLabel = null;

        [SerializeField]
        private TextMeshProUGUI classText = null;

        [SerializeField]
        private HighlightButton returnButton = null;

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
            returnButton.Configure(systemGameManager);
            spawnButton.Configure(systemGameManager);
            despawnButton.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
            uIManager = systemGameManager.UIManager;
            objectPooler = systemGameManager.ObjectPooler;
        }

        public void ShowUnit(PetSpawnButton petSpawnButton) {
            //Debug.Log("LoadGamePanel.ShowSavedGame()");

            selectedPetSpawnButton = petSpawnButton;

            unitProfile = petSpawnButton.MyUnitProfile;

            // ensure the correct unit and character model is spawned
            characterPreviewPanel.ReloadUnit();

            UpdateButtons(petSpawnButton);
            UpdateUnitInformation();

            nameText.text = unitProfile.CharacterName;
        }
   
        public void UpdateUnitInformation() {
            classLabel.SetActive(true);
            if (unitProfile != null && unitProfile.CharacterClass != null) {
                classText.text = unitProfile.CharacterClass.DisplayName;
            }
            nameText.text = unitProfile.DisplayName;
        }

        public void UpdateButtons(PetSpawnButton petSpawnButton) {
            if (petSpawnButton.MyUnitProfile != null) {
                spawnButton.gameObject.SetActive(true);
                despawnButton.gameObject.SetActive(true);
                if (playerManager.MyCharacter.CharacterPetManager.ActiveUnitProfiles.ContainsKey(petSpawnButton.MyUnitProfile)) {
                    spawnButton.Button.interactable = false;
                    despawnButton.Button.interactable = true;
                } else {
                    spawnButton.Button.interactable = true;
                    despawnButton.Button.interactable = false;
                }
            }
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
            classText.text = string.Empty;
            nameText.text = "No Pets Available";
        }

        public void ClosePanel() {
            //Debug.Log("CharacterCreatorPanel.ClosePanel()");
            uIManager.petSpawnWindow.CloseWindow();
            uIManager.interactionWindow.CloseWindow();
        }

        /*
        public void HandleTargetReady() {
            //LoadUMARecipe();
            // not doing anything for now since pets don't have equipment yet
        }
        */

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("LoadGamePanel.OnCloseWindow()");
            base.RecieveClosedWindowNotification();
            //characterPreviewPanel.OnTargetReady -= HandleTargetReady;
            characterPreviewPanel.RecieveClosedWindowNotification();
            OnCloseWindow(this);
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("PetSpawnControlPanel.ReceiveOpenWindowNotification()");

            ClearPanel();

            if (playerManager.MyCharacter.CharacterPetManager != null) {
                unitProfileList = playerManager.MyCharacter.CharacterPetManager.UnitProfiles;
            }
            ShowPreviewButtonsCommon();

            // inform the preview panel so the character can be rendered
            //characterPreviewPanel.OnTargetReady += HandleTargetReady;
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
                //Debug.Log("PetSpawnControlPanel.ShowLoadButtonsCommon() unitprofile: " + unitProfile.DisplayName);
                if (playerManager.MyCharacter.CharacterClass.ValidPetTypeList.Contains(unitProfile.UnitType)) {
                    GameObject go = objectPooler.GetPooledObject(buttonPrefab, buttonArea.transform);
                    PetSpawnButton petSpawnButton = go.GetComponent<PetSpawnButton>();
                    if (petSpawnButton != null) {
                        petSpawnButton.Configure(systemGameManager);
                        petSpawnButton.PetSpawnControlPanel = this;
                        petSpawnButton.AddUnitProfile(unitProfile);
                        petSpawnButtons.Add(petSpawnButton);
                    }
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
                    petSpawnButton.DeSelect();
                    objectPooler.ReturnObjectToPool(petSpawnButton.gameObject);
                }
            }
            petSpawnButtons.Clear();
            SelectedPetSpawnButton = null;
            nameText.text = "";
        }

        public void SpawnUnit() {
            playerManager.MyCharacter.CharacterPetManager.SpawnPet(SelectedPetSpawnButton.MyUnitProfile);
            ClosePanel();
        }

        public void DespawnUnit() {
            playerManager.MyCharacter.CharacterPetManager.DespawnPet(SelectedPetSpawnButton.MyUnitProfile);
            UpdateButtons(selectedPetSpawnButton);
            //ClosePanel();
        }

    }

}