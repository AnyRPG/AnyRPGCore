using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UMA;
using UMA.CharacterSystem;
using UMA.CharacterSystem.Examples;

namespace AnyRPG {

    public class UMAAppearanceEditorPanelController : AppearancePanel {

        [Header("UMA")]

        [SerializeField]
        protected GameObject mainButtonsArea = null;

        [SerializeField]
        protected UINavigationListVertical mainOptionsNavigationController = null;

        [Header("Main Buttons")]

        [SerializeField]
        protected HighlightButton hairButton = null;

        [SerializeField]
        protected HighlightButton eyesButton = null;

        [SerializeField]
        protected HighlightButton faceButton = null;

        [SerializeField]
        protected HighlightButton bodyButton = null;

        [Header("Option Areas")]

        [SerializeField]
        protected UINavigationListVertical hairOptionsArea = null;

        [SerializeField]
        protected GameObject hairColorLabel = null;

        [SerializeField]
        protected ColorSelectionController hairColorSelectionController = null;

        [SerializeField]
        protected GameObject hairStyleLabel = null;

        [SerializeField]
        protected UINavigationListVertical eyeOptionsArea = null;

        [SerializeField]
        protected GameObject eyeColorLabel = null;

        [SerializeField]
        protected ColorSelectionController eyeColorSelectionController = null;

        [SerializeField]
        protected UINavigationListVertical faceOptionsArea = null;

        [SerializeField]
        protected GameObject eyebrowsLabel = null;

        [SerializeField]
        protected GameObject beardLabel = null;

        [SerializeField]
        protected GameObject lipstickColorLabel = null;

        [SerializeField]
        protected ColorSelectionController lipstickColorSelectionController = null;

        [SerializeField]
        protected GameObject faceDnaLabel = null;

        [SerializeField]
        protected UINavigationListVertical bodyOptionsArea = null;

        [SerializeField]
        protected GameObject skinColorLabel = null;

        [SerializeField]
        protected ColorSelectionController skinColorSelectionController = null;

        [SerializeField]
        protected GameObject bodyDnaLabel = null;


        [Header("Image")]

        [SerializeField]
        protected Sprite noOptionImage = null;

        [Header("Prefabs")]

        /*
        [SerializeField]
        private GameObject imageButtonPrefab = null;
        */

        [SerializeField]
        private GameObject mixedButtonPrefab = null;

        [SerializeField]
        private GameObject textButtonPrefab = null;

        [SerializeField]
        private GameObject dnaSliderPrefab = null;

        [Header("Color Tables")]

        public SharedColorTable HairColor;
        public SharedColorTable EyesColor;
        public SharedColorTable LipstickColor;
        public SharedColorTable SkinColor;

        // hold data so changes are not reset on switch between male and female
        protected string maleRecipe = string.Empty;
        protected string femaleRecipe = string.Empty;

        protected Dictionary<string, List<UMATextRecipe>> allRecipes = new Dictionary<string, List<UMATextRecipe>>();
        protected List<UMATextRecipe> hairRecipes = new List<UMATextRecipe>();
        protected List<UMATextRecipe> eyebrowsRecipes = new List<UMATextRecipe>();
        protected List<UMATextRecipe> beardRecipes = new List<UMATextRecipe>();

        protected Dictionary<string, DNAInfoNode> dnaDictionary = new Dictionary<string, DNAInfoNode>();
        protected List<string> bodyDNA;
        protected List<string> faceDNA;

        // slots to not clear when viewing the character appearance
        protected List<string> keepPreviewSlots;

        protected UMAModelOptions umaModelOptions = null;
        protected UMAModelController umaModelController = null;
        protected DynamicCharacterAvatar dynamicCharacterAvatar = null;

        // game manager references
        protected UIManager uIManager = null;
        protected ObjectPooler objectPooler = null;
        protected SaveManager saveManager = null;

        public class DNAInfoNode {
            public string name;
            public float value;
            public int index;
            public UMADnaBase dnaBase;
            public DNAInfoNode(string Name, float Value, int Index, UMADnaBase DNABase) {
                name = Name;
                value = Value;
                index = Index;
                dnaBase = DNABase;
            }
            /*
            #region IComparable implementation
            public int CompareTo(DNAHolder other) {
                return string.Compare(name, other.name);
            }
            #endregion
            */
        }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            //appearanceButton.Configure(systemGameManager);
            //colorsButton.Configure(systemGameManager);
            //hairColorsButton.Configure(systemGameManager);
            //skinColorsButton.Configure(systemGameManager);
            //eyesColorsButton.Configure(systemGameManager);
            //sexButton.Configure(systemGameManager);
            //maleButton.Configure(systemGameManager);
            //femaleButton.Configure(systemGameManager);
            bodyDNA = new List<string>() {
                "height",
                "skinGreenness",
                "skinBlueness",
                "skinRedness",
                "neckThickness",
                "upperMuscle",
                "upperWeight",
                "breastSize",
                "belly",
                "waist",
                "armLength",
                "armWidth",
                "forearmLength",
                "forearmWidth",
                "handsSize",
                "lowerMuscle",
                "lowerWeight",
                "gluteusSize",
                "legSeparation",
                "legsSize",
                "feetSize"
            };

            faceDNA = new List<string>() {
                "headSize",
                "headWidth",
                "foreheadSize",
                "foreheadPosition",
                "eyeRotation",
                "eyeSize",
                "eyeSpacing",
                "earsSize",
                "earsPosition",
                "earsRotation",
                "noseSize",
                "noseCurve",
                "noseWidth",
                "noseInclination",
                "nosePosition",
                "nosePronounced",
                "noseFlatten",
                "lipsSize",
                "mouthSize",
                "chinSize",
                "chinPronounced",
                "chinPosition",
                "mandibleSize",
                "jawsSize",
                "jawsPosition",
                "cheekSize",
                "cheekPosition",
                "lowCheekPronounced",
                "lowCheekPosition"
            };

            keepPreviewSlots = new List<string>() {
                "Physique",
                "Face",
                "Eyes",
                "Hair",
                "Body",
                "Complexion",
                "Tattoo",
                "Underwear",
                "Eyebrows",
                "Beard",
                "Ears"
            };

        }

        public override void SetGameManagerReferences() {
            //Debug.Log("CharacterAppearancePanel.SetGameManagerReferences()");
            base.SetGameManagerReferences();

            uIManager = systemGameManager.UIManager;
            objectPooler = systemGameManager.ObjectPooler;
            saveManager = systemGameManager.SaveManager;
        }

        public override void SetupOptions() {
            //Debug.Log("UMACharacterEditorPanelController.SetupOptions()");

            base.SetupOptions();

            CloseOptionsAreas();
            mainButtonsArea.SetActive(false);
            mainNoOptionsArea.SetActive(false);

            if (umaModelController == null) {
                // somehow this panel was opened but the preview model is not configured as an UMA model
                Debug.Log("UMACharacterEditorPanelController.SetupOptions() : no UMA model controller");
                mainNoOptionsArea.SetActive(true);
                return;
            }

            dynamicCharacterAvatar = umaModelController.DynamicCharacterAvatar;

            // there are no options to show if this is not an UMA
            if (dynamicCharacterAvatar == null) {
                Debug.Log("UMACharacterEditorPanelController.SetupOptions() : no dynamic character avatar");
                mainNoOptionsArea.SetActive(true);
                return;
            }

            GetAvatarConfiguration();
            ShowAppearanceOptions();

            //if (characterCreatorManager.PreviewUnitController?.UnitModelController?.ModelReady == true) {
                mainButtonsArea.SetActive(true);
                OpenHairOptionsArea();
                //appearanceButton.HighlightBackground();
                mainOptionsNavigationController.FocusCurrentButton();
            //}
        }

        private void GetAvatarConfiguration() {
            //Debug.Log("UMACharacterEditorPanelController.GetAvatarConfiguration()");

            if (dynamicCharacterAvatar == null) {
                Debug.Log("UMAAppearanceEditorPanel.CheckAppearance(): umaAvatar is null!!!!");
            }

            allRecipes = dynamicCharacterAvatar.AvailableRecipes;
            //Debug.Log("recipes: " + string.Join(',', allRecipes.Keys));
            GetDNAList();
        }

        public override void GetUnitModelController() {
            base.GetUnitModelController();
            umaModelController = characterCreatorManager.PreviewUnitController?.UnitModelController.ModelAppearanceController.GetModelAppearanceController<UMAModelController>();
            umaModelOptions = umaModelController.UMAModelOptions;
        }

        /*
        public override void HandleModelCreated() {
            Debug.Log("UMAAppearanceEditorPanelController.HandleModelCreated()");

            base.HandleUnitCreated();

            SetupOptions();
        }
        */

        public virtual void OpenHairOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.OpenAppearanceOptions()");

            // reset areas and buttons
            hairButton.HighlightBackground();
            mainOptionsNavigationController.UnHightlightButtonBackgrounds(hairButton);
            CloseEyeOptionsArea();
            CloseFaceOptionsArea();
            CloseBodyOptionsArea();

            hairOptionsArea.gameObject.SetActive(true);
        }

        public void CloseHairOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.CloseAppearanceOptions()");
            hairOptionsArea.gameObject.SetActive(false);
        }

        public virtual void OpenEyeOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.OpenEyesOptionsArea()");

            // reset areas and buttons
            eyesButton.HighlightBackground();
            mainOptionsNavigationController.UnHightlightButtonBackgrounds(eyesButton);
            CloseHairOptionsArea();
            CloseFaceOptionsArea();
            CloseBodyOptionsArea();

            eyeOptionsArea.gameObject.SetActive(true);
        }

        public void CloseEyeOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.CloseColorsOptionsArea()");
            eyeOptionsArea.gameObject.SetActive(false);
        }

        public virtual void OpenFaceOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.OpenEyesOptionsArea()");

            // reset areas and buttons
            faceButton.HighlightBackground();
            mainOptionsNavigationController.UnHightlightButtonBackgrounds(faceButton);
            CloseEyeOptionsArea();
            CloseHairOptionsArea();
            CloseBodyOptionsArea();

            faceOptionsArea.gameObject.SetActive(true);
        }

        public void CloseFaceOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.CloseColorsOptionsArea()");
            faceOptionsArea.gameObject.SetActive(false);
        }

        public virtual void OpenBodyOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.OpenEyesOptionsArea()");

            // reset areas and buttons
            bodyButton.HighlightBackground();
            mainOptionsNavigationController.UnHightlightButtonBackgrounds(bodyButton);
            CloseEyeOptionsArea();
            CloseHairOptionsArea();
            CloseFaceOptionsArea();

            bodyOptionsArea.gameObject.SetActive(true);
        }

        public void CloseBodyOptionsArea() {
            //Debug.Log("CharacterCreatorPanel.CloseColorsOptionsArea()");
            bodyOptionsArea.gameObject.SetActive(false);
        }

        public void UnHighlightAllButtons() {
            //Debug.Log("CharacterCreatorPanel.UnHighlightAllButtons()");

            hairButton.UnHighlightBackground();
            eyesButton.UnHighlightBackground();
            faceButton.UnHighlightBackground();
            bodyButton.UnHighlightBackground();
        }

        public void ClosePanel() {
            //Debug.Log("CharacterCreatorPanel.ClosePanel()");
            uIManager.characterCreatorWindow.CloseWindow();
        }

        private bool HasSharedColor(string colorName) {
            foreach (DynamicCharacterAvatar.ColorValue colorValue in dynamicCharacterAvatar.characterColors.Colors) {
                if (colorValue.Name == colorName) {
                    return true;
                }
            }

            return false;
        }

        public void InitializeSkinColors() {
            //Debug.Log("CharacterCreatorPanel.InitializeSkinColors()");

            if (HasSharedColor("Skin")) {
                skinColorLabel.SetActive(true);
                skinColorSelectionController.Configure(systemGameManager);
                skinColorSelectionController.Setup(dynamicCharacterAvatar, "Skin", bodyOptionsArea.gameObject, skinColorLabel, SkinColor);
            } else {
                skinColorLabel.SetActive(false);
            }
        }

        public void InitializeHairColors() {
            //Debug.Log("CharacterCreatorPanel.InitializeHairColors()");
            
            if (HasSharedColor("Hair")) {
                hairColorLabel.SetActive(true);
                hairColorSelectionController.Configure(systemGameManager);
                hairColorSelectionController.Setup(dynamicCharacterAvatar, "Hair", hairOptionsArea.gameObject, hairColorLabel, HairColor);
            } else {
                hairColorLabel.SetActive(false);
            }
        }

        public void InitializeLipstickColors() {
            //Debug.Log("CharacterCreatorPanel.InitializeLipstickColors()");

            if (HasSharedColor("Lipstick")) {
                lipstickColorLabel.SetActive(true);
                lipstickColorSelectionController.Configure(systemGameManager);
                lipstickColorSelectionController.Setup(dynamicCharacterAvatar, "Lipstick", faceOptionsArea.gameObject, lipstickColorLabel, LipstickColor);
            } else {
                lipstickColorLabel.SetActive(false);
            }

        }

        public void InitializeEyeColors() {
            //Debug.Log("CharacterCreatorPanel.InitializeEyeColors()");

            if (HasSharedColor("Eyes")) {
                eyeColorLabel.SetActive(true);
                eyeColorSelectionController.Configure(systemGameManager);
                eyeColorSelectionController.Setup(dynamicCharacterAvatar, "Eyes", eyeOptionsArea.gameObject, eyeColorLabel, EyesColor);
            } else {
                eyeColorLabel.SetActive(false);
            }
        }

        public override void ProcessBeforeSetMale() {
            base.ProcessBeforeSetMale();
            femaleRecipe = umaModelController.GetAppearanceString();
        }

        public override void ProcessBeforeSetFemale() {
            base.ProcessBeforeSetFemale();
            maleRecipe = umaModelController.GetAppearanceString();
        }

        public override void ProcessSetMale() {
            //Debug.Log("CharacterCreatorPanel.ProcessSetMale()");

            base.ProcessSetMale();

            if (maleRecipe != string.Empty) {
                //Debug.Log("CharacterCreatorPanel.SetFemale(): maleRecipe != string.Empty");
                //dynamicCharacterAvatar.ChangeRace("HumanMale");
                //characterCreatorManager.PreviewUnitController.UnitModelController.LoadSavedAppearanceSettings(maleRecipe, true);
                umaModelController.SetAppearance(maleRecipe);
            } else {
                //Debug.Log("CharacterCreatorPanel.SetMale(): maleRecipe == string.Empty");
                //characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.DynamicCharacterAvatar.ChangeRace("HumanMale");
                //umaModelController.SetAvatarDefinitionRace("HumanMale");
                umaModelController.PreloadEquipmentModels(true);
                umaModelController.ReloadAvatarDefinition();
            }
        }

        public override void ProcessSetFemale() {
            //Debug.Log("UMAAppearancePanel.ProcessSetFemale()");

            base.ProcessSetFemale();

            if (femaleRecipe != string.Empty) {
                //Debug.Log("CharacterCreatorPanel.SetFemale(): femaleRecipe != string.Empty");
                //dynamicCharacterAvatar.ChangeRace("HumanFemale");
                //characterCreatorManager.PreviewUnitController.UnitModelController.LoadSavedAppearanceSettings(femaleRecipe, true);
                umaModelController.SetAppearance(femaleRecipe);
            } else {
                //Debug.Log("CharacterCreatorPanel.SetFemale(): femaleRecipe == string.Empty");
                //characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.DynamicCharacterAvatar.ChangeRace("HumanFemale");
                //umaModelController.SetAvatarDefinitionRace("HumanFemale");
                umaModelController.PreloadEquipmentModels(true);
                umaModelController.ReloadAvatarDefinition();
            }
        }

        public void CloseOptionsAreas() {
            //Debug.Log("UMAAppearanceEditorPanelController.CloseOptionsAreas()");

            CloseHairOptionsArea();
            CloseEyeOptionsArea();
            CloseFaceOptionsArea();
            CloseBodyOptionsArea();
        }

        private void GetDNAList() {
            //Debug.Log("UMAAppearanceEditorPanel.GetDNAList()");
            
            dnaDictionary.Clear();
            UMADnaBase[] DNA = dynamicCharacterAvatar.GetAllDNA();

            foreach (UMADnaBase dnaBase in DNA) {
                string[] names = dnaBase.Names;
                float[] values = dnaBase.Values;

                for (int i = 0; i < names.Length; i++) {
                    string name = names[i];
                    //Debug.Log("Found DNA: " + name);
                    dnaDictionary.Add(name, new DNAInfoNode(name, values[i], i, dnaBase));
                }
            }
        }

        private void ShowAppearanceOptions() {
            //Debug.Log("UMAAppearanceEditorPanelController.ShowAppearanceOptions()");

            if (dynamicCharacterAvatar == null) {
                Debug.Log("UMAAppearanceEditorPanelController.ShowAppearanceOptions(): umaAvatar is null!!!!");
            }

            // clear old buttons
            hairOptionsArea.DeleteActiveButtons();
            eyeOptionsArea.DeleteActiveButtons();
            faceOptionsArea.DeleteActiveButtons();
            bodyOptionsArea.DeleteActiveButtons();

            // Hair
            if (allRecipes.ContainsKey("Hair")) {
                hairStyleLabel.SetActive(true);
                hairRecipes = allRecipes["Hair"];
                PopulateHairOptions(hairRecipes);
            } else {
                hairStyleLabel.SetActive(false);
            }
            InitializeHairColors();
            //hairOptionsArea.UpdateNavigationList();

            // eyes
            InitializeEyeColors();
            //eyeOptionsArea.UpdateNavigationList();


            // Eyebrows
            if (allRecipes.ContainsKey("Eyebrows")) {
                eyebrowsLabel.SetActive(true);
                eyebrowsRecipes = allRecipes["Eyebrows"];
                PopulateEyebrowOptions(eyebrowsRecipes);
            } else {
                eyebrowsLabel.SetActive(false);
            }

            // Beard
            if (allRecipes.ContainsKey("Beard")) {
                beardLabel.SetActive(true);
                beardRecipes = allRecipes["Beard"];
                PopulateBeardOptions(beardRecipes);
            } else {
                beardLabel.SetActive(false);
            }

            InitializeLipstickColors();

            // face DNA
            PopulateDNAOptions(faceDnaLabel, faceOptionsArea, faceDNA);

            //faceOptionsArea.UpdateNavigationList();

            // body
            InitializeSkinColors();
            PopulateDNAOptions(bodyDnaLabel, bodyOptionsArea, bodyDNA);

            //bodyOptionsArea.UpdateNavigationList();

        }

        private void PopulateDNAOptions(GameObject label, UINavigationController navigationController, List<string> dnaOptions) {
            //Debug.Log("UMAAppearanceEditorPanelController.PopulateDNAOptions()");

            foreach (string dnaName in dnaOptions) {
                if (dnaDictionary.ContainsKey(dnaName)) {
                    DNAInfoNode dnaInfoNode = dnaDictionary[dnaName];
                    UMADNASlider dnaSlider = objectPooler.GetPooledObject(dnaSliderPrefab, navigationController.transform).GetComponent<UMADNASlider>();
                    dnaSlider.Configure(systemGameManager);
                    dnaSlider.Initialize(dnaInfoNode.name.BreakupCamelCase(), dnaInfoNode.index, dnaInfoNode.dnaBase, dynamicCharacterAvatar, dnaInfoNode.value);
                    dnaSlider.transform.SetAsLastSibling();
                    navigationController.AddActiveButton(dnaSlider);
                }
            }
        }

        private bool CanDisplayRecipe(string slotName, string recipeName) {
            if (umaModelOptions.SlotOptions.ContainsKey(slotName) == false) {
                return true;
            }

            if (umaModelOptions.SlotOptions[slotName].RecipeListType == UMARecipetListType.Allow) {
                // list type is allow
                if (umaModelOptions.SlotOptions[slotName].RecipeNames.Contains(recipeName) == true) {
                    // was allowed
                    return true;
                }
            } else {
                // list type is deny
                if (umaModelOptions.SlotOptions[slotName].RecipeNames.Contains(recipeName) == false) {
                    // was not denied
                    return true;
                }
            }

            // was not in allow list, or was in deny list
            return false;
        }

        private void PopulateHairOptions(List<UMATextRecipe> hairRecipes) {
            //Debug.Log("UMAAppearanceEditorPanelController.PopulateHairOptions()");

            int index = 2;
            UMAOptionChoiceButton optionChoiceButton = AddOptionListButton(hairOptionsArea, "Hair", noOptionImage, "None", "");
            optionChoiceButton.transform.SetSiblingIndex(1);
            foreach (UMATextRecipe textRecipe in hairRecipes) {
                if (CanDisplayRecipe("Hair", textRecipe.DisplayValue)) {
                    optionChoiceButton = AddOptionListButton(hairOptionsArea, "Hair", null, textRecipe.DisplayValue, textRecipe.DisplayValue);
                    optionChoiceButton.transform.SetSiblingIndex(index);
                    index++;
                }
            }
        }

        private void PopulateEyebrowOptions(List<UMATextRecipe> eyebrowRecipes) {
            //Debug.Log("UMAAppearanceEditorPanelController.PopulateEyebrowOptions()");

            int index = eyebrowsLabel.transform.GetSiblingIndex() + 1;
            UMAOptionChoiceButton optionChoiceButton = AddOptionListButton(faceOptionsArea, "Eyebrows", noOptionImage, "None", "");
            optionChoiceButton.transform.SetSiblingIndex(index);
            index++;
            foreach (UMATextRecipe textRecipe in eyebrowRecipes) {
                if (CanDisplayRecipe("Eyebrows", textRecipe.DisplayValue)) {
                    optionChoiceButton = AddOptionListButton(faceOptionsArea, "Eyebrows", null, textRecipe.DisplayValue, textRecipe.DisplayValue);
                    optionChoiceButton.transform.SetSiblingIndex(index);
                    index++;
                }
            }
        }

        private void PopulateBeardOptions(List<UMATextRecipe> beardRecipes) {
            int index = beardLabel.transform.GetSiblingIndex() + 1;
            UMAOptionChoiceButton optionChoiceButton = AddOptionListButton(faceOptionsArea, "Beard", noOptionImage, "None", "");
            optionChoiceButton.transform.SetSiblingIndex(index);
            index++;
            foreach (UMATextRecipe textRecipe in beardRecipes) {
                if (CanDisplayRecipe("Beard", textRecipe.DisplayValue)) {
                    optionChoiceButton = AddOptionListButton(faceOptionsArea, "Beard", null, textRecipe.DisplayValue, textRecipe.DisplayValue);
                    optionChoiceButton.transform.SetSiblingIndex(index);
                    index++;
                }
            }
        }

        private UMAOptionChoiceButton AddOptionListButton(UINavigationListVertical listOptionsArea, string groupName, Sprite optionImage, string displayName, string optionChoice) {
            //Debug.Log("SwappableMeshAppearancePanelController.AddOptionListButton()");

            UMAOptionChoiceButton optionChoiceButton = null;
            if (optionImage != null) {
                optionChoiceButton = objectPooler.GetPooledObject(mixedButtonPrefab, listOptionsArea.transform).GetComponent<UMAOptionChoiceButton>();
            } else {
                optionChoiceButton = objectPooler.GetPooledObject(textButtonPrefab, listOptionsArea.transform).GetComponent<UMAOptionChoiceButton>();
            }
            if (optionChoiceButton != null) {
                optionChoiceButton.Configure(systemGameManager);
                optionChoiceButton.ConfigureButton(this, groupName, optionImage, displayName, optionChoice);
                listOptionsArea.AddActiveButton(optionChoiceButton);

                // highlight the button if it is already the selected choice for the group
                if (dynamicCharacterAvatar.GetWardrobeItemName(groupName) == GetRecipeName(optionChoice, allRecipes[groupName])) {
                    optionChoiceButton.HighlightOutline();
                }
            }
            return optionChoiceButton;
        }

        public void SetRecipe(UMAOptionChoiceButton optionChoiceButton, string groupName, string optionChoice) {
            //Debug.Log("UMAAppearanceEditorPanel.SetRecipe(" + groupName + ", " + optionChoice + ")");

            if (optionChoice == string.Empty) {
                dynamicCharacterAvatar.ClearSlot(groupName);
            }
            dynamicCharacterAvatar.SetSlot(groupName, GetRecipeName(optionChoice, allRecipes[groupName]));
            RebuildUMA();
        }

        private string GetRecipeName(string recipeDisplayName, List<UMATextRecipe> recipeList) {
            //Debug.Log("UMAAppearanceEditorPanelController.GetRecipeName(" + recipeDisplayName + ")");

            foreach (UMATextRecipe umaTextRecipe in recipeList) {
                if (umaTextRecipe.DisplayValue == recipeDisplayName) {
                    //Debug.Log("UMAAppearanceEditorPanelController.GetRecipeName(" + recipeDisplayName + "): returning " + umaTextRecipe.name);
                    return umaTextRecipe.name;
                }
            }
            //Debug.Log("UMAAppearanceEditorPanelController.GetRecipeName(" + recipeDisplayName + "): Could not find recipe.  return string.Empty!!!");
            return string.Empty;
        }

        public void RebuildUMA() {
            //Debug.Log("UMAAppearanceEditorPanelController.RebuildUMA()");

            umaModelController.BuildModelAppearance();
        }
        

    }

}