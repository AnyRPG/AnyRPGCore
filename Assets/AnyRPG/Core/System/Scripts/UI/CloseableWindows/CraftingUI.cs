using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CraftingUI : WindowContentController {

        [Header("Crafting")]

        [SerializeField]
        private HighlightButton craftButton = null;

        [SerializeField]
        private HighlightButton craftAllButton = null;

        /*
        [SerializeField]
        private HighlightButton cancelButton = null;
        */

        [SerializeField]
        private Button lessButton = null;

        [SerializeField]
        private Button moreButton = null;

        [SerializeField]
        private Image leftButton = null;

        [SerializeField]
        private Image rightButton = null;

        //[SerializeField]
        //private GameObject cancelButton = null;

        [SerializeField]
        private TextMeshProUGUI craftAmountText = null;

        [SerializeField]
        private GameObject recipePrefab = null;

        [SerializeField]
        private Transform recipeParent = null;

        [SerializeField]
        private TextMeshProUGUI recipeDescription = null;

        [SerializeField]
        private GameObject materialsHeading = null;


        [SerializeField]
        private DescribableCraftingOutputIcon outputIcon = null;

        [SerializeField]
        private List<DescribableCraftingInputIcon> inputIcons = new List<DescribableCraftingInputIcon>();

        [SerializeField]
        private UINavigationController recipeListNavigationController = null;

        // holds all the recipes
        private CraftAbilityProperties craftAbility = null;


        // the number of items to craft
        private int craftAmount = 1;

        //private List<recipeNode> recipeNodes = new List<recipeNode>();

        private Dictionary<Recipe, RecipeScript> recipeScripts = new Dictionary<Recipe, RecipeScript>();

        private RecipeScript selectedRecipeScript;

        //private string currentRecipeName = null;

        private Recipe currentRecipe = null;

        // game manager references
        private CraftingManager craftingManager = null;
        private PlayerManager playerManager = null;
        private ObjectPooler objectPooler = null;
        private UIManager uIManager = null;

        public RecipeScript SelectedRecipeScript { get => selectedRecipeScript; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            /*
            craftButton.Configure(systemGameManager);
            craftAllButton.Configure(systemGameManager);
            cancelButton.Configure(systemGameManager);
            */

            
            foreach (DescribableCraftingInputIcon inputIcon in inputIcons) {
                inputIcon.SetToolTipTransform(rectTransform);
            }
            outputIcon.SetToolTipTransform(rectTransform);
            
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            craftingManager = systemGameManager.CraftingManager;
            playerManager = systemGameManager.PlayerManager;
            objectPooler = systemGameManager.ObjectPooler;
            uIManager = systemGameManager.UIManager;
        }

        protected override void ProcessCreateEventSubscriptions() {
            //Debug.Log("CraftingUI.CreateEventSubscriptions()");
            base.ProcessCreateEventSubscriptions();
            craftingManager.OnSelectRecipe += SelectRecipe;
            craftingManager.OnCraftAmountUpdated += UpdateCraftAmountArea;
            craftingManager.OnSetCraftAbility += ViewRecipes;
        }

        protected override void ProcessCleanupEventSubscriptions() {
            //Debug.Log("CraftingUI.CleanupEventSubscriptions()");
            base.ProcessCleanupEventSubscriptions();
            craftingManager.OnSelectRecipe -= SelectRecipe;
            craftingManager.OnCraftAmountUpdated -= UpdateCraftAmountArea;
            craftingManager.OnSetCraftAbility -= ViewRecipes;
        }

        public void SelectRecipe(Recipe recipe) {
            selectedRecipeScript = recipeScripts[recipe];
            //DeselectOtherRecipes();
            ShowDescription(recipe);
        }

        public void DeactivateButtons() {
            craftButton.Button.interactable = false;
            craftAllButton.Button.interactable = false;
            uINavigationControllers[1].UpdateNavigationList();
        }

        public void CancelCrafting() {
            //Debug.Log("CraftingUI.CancelCrafting()");
            craftingManager.CancelCrafting();
        }

        // meant to be called externally from craftingNode
        public void ViewRecipes(CraftAbilityProperties craftAbility) {
            this.craftAbility = craftAbility;
            ResetWindow();
            ShowRecipes(craftAbility);
        }

        public List<Recipe> GetRecipes() {
            //Debug.Log("CraftAbility.GetRecipes() this: " + this.name);
            List<Recipe> returnList = new List<Recipe>();
            foreach (Recipe recipe in playerManager.MyCharacter.CharacterRecipeManager.RecipeList.Values) {
                if (craftAbility == recipe.CraftAbility) {
                    returnList.Add(recipe);
                }
            }
            return returnList;
        }

        public void ShowRecipesCommon(CraftAbilityProperties craftAbility) {
            //Debug.Log($"CraftingUI.ShowRecipesCommon({craftAbility.ResourceName})");

            Clearrecipes();
            RecipeScript firstScript = null;
            foreach (Recipe recipe in GetRecipes()) {
                //Debug.Log("craftingUI.ShowRecipesCommon(" + craftAbility.name + ") : adding recipe:" + recipe.MyOutput.itemName);
                if (recipe.Output != null) {
                    GameObject go = objectPooler.GetPooledObject(recipePrefab, recipeParent);
                    RecipeScript qs = go.GetComponentInChildren<RecipeScript>();
                    qs.Configure(systemGameManager);
                    if (firstScript == null) {
                        firstScript = qs;
                    }
                    qs.Text.text = recipe.Output.DisplayName;
                    qs.SetRecipe(recipe);
                    recipeScripts.Add(recipe, qs);
                    recipeListNavigationController.AddActiveButton(qs);
                } else {
                    //Debug.Log("Recipe Output is null!");
                }
            }

            //if (MySelectedRecipeScript != null) {
            //MySelectedRecipeScript.Select();
            //} else {
            selectedRecipeScript = firstScript;
            /*
            if (firstScript != null) {
                firstScript.Select();
            }
            */
            SetNavigationController(recipeListNavigationController);

            //}
        }

        public void ShowRecipes() {
            //Debug.Log("craftingUI.Showrecipes()");
            ShowRecipesCommon(craftAbility);
        }

        public void ShowRecipes(CraftAbilityProperties craftAbility) {
            //Debug.Log($"craftingUI.Showrecipes({craftAbility.ResourceName})");

            this.craftAbility = craftAbility;
            ShowRecipesCommon(this.craftAbility);
        }

        public void UpdateSelected() {
            //Debug.Log("CraftingUI.UpdateSelected()");

            if (selectedRecipeScript != null) {
                craftAmount = 1;
                ShowDescription(selectedRecipeScript.Recipe);
            }
        }

        public void ShowDescription(Recipe newRecipe) {
            //Debug.Log($"CraftingUI.ShowDescription({newRecipe.ResourceName})");

            ClearDescription();

            if (newRecipe == null) {
                return;
            }
            currentRecipe = newRecipe;

            recipeDescription.text = string.Format("<b>{0}</b>", newRecipe.Output.DisplayName, newRecipe.Description);

            outputIcon.SetItem(newRecipe.Output, newRecipe.OutputCount);

            if (newRecipe.CraftingMaterials.Count > 0) {
                materialsHeading.gameObject.SetActive(true);
            }

            // show crafting materials
            for (int i = 0; i < newRecipe.CraftingMaterials.Count; i++) {
                inputIcons[i].MaterialSlot.SetActive(true);
                inputIcons[i].SetItem(newRecipe.CraftingMaterials[i].Item, newRecipe.CraftingMaterials[i].Count);
            }

            UpdateCraftAmountArea();

            uINavigationControllers[2].UpdateNavigationList();

            if (craftButton.Button.interactable == true && uINavigationControllers[1].CurrentIndex == 2) {
                uINavigationControllers[1].SetCurrentIndex(0);
            }
        }

        public void ClearDescription() {
            //Debug.Log("CraftingUI.ClearDescription()");
            craftAmount = 1;
            //recipeDetailsArea.SetActive(false);
            recipeDescription.text = string.Empty;
            //materialsHeading.gameObject.SetActive(false);
            ClearInputIcons();
            //DeselectRecipes();
            DeselectOtherRecipes();
        }

        public void DeselectOtherRecipes() {
            //Debug.Log("CraftingUI.DeselectRecipes()");

            foreach (Recipe recipe in recipeScripts.Keys) {
                if (recipe != selectedRecipeScript?.Recipe) {
                    recipeScripts[recipe].DeSelect();
                }
            }
            recipeListNavigationController.UnHightlightButtonBackgrounds(selectedRecipeScript);

        }

        public void Clearrecipes() {
            //Debug.Log("CraftingUI.ClearRecipes()");
            // clear the recipe list so any recipes left over from a previous time opening the window aren't shown
            //Debug.Log("running clear recipes in recipegiverUI; recipegiver: " + tradeSkill.name);
            foreach (RecipeScript recipeScript in recipeScripts.Values) {
                //Debug.Log("The recipenode has a gameobject we need to clear");
                recipeScript.gameObject.transform.SetParent(null);
                recipeScript.DeSelect();
                objectPooler.ReturnObjectToPool(recipeScript.gameObject);
            }
            recipeScripts.Clear();
            recipeListNavigationController.ClearActiveButtons();
        }

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("craftingUI.OnCloseWindow()");
            base.ReceiveClosedWindowNotification();
            //Debug.Log("craftingUI.OnCloseWindow(): nulling recipe script");
            selectedRecipeScript = null;
            craftingManager.ClearSelectedRecipe();
        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("CraftingUI.ProcessOpenWindowNotification()");
            base.ProcessOpenWindowNotification();
            SetBackGroundColor(new Color32(0, 0, 0, (byte)(int)(PlayerPrefs.GetFloat("PopupWindowOpacity") * 255)));

            DeactivateButtons();

            craftingManager.ClearCraftingQueue();

            if (controlsManager.GamePadModeActive == true) {
                leftButton.gameObject.SetActive(true);
                rightButton.gameObject.SetActive(true);
            } else {
                leftButton.gameObject.SetActive(false);
                rightButton.gameObject.SetActive(false);
            }
        }

        private void ResetWindow() {
            ClearDescription();
            UpdateCraftAmountArea();
            uIManager.craftingWindow.SetWindowTitle(craftAbility.DisplayName);
        }

        private void ClearInputIcons() {
            foreach (DescribableCraftingInputIcon inputIcon in inputIcons) {
                inputIcon.MaterialSlot.SetActive(false);
            }
        }

        public void CraftAll() {
            //Debug.Log("CraftingUI.CraftAll()");
            if (selectedRecipeScript != null) {
                craftAmount = craftingManager.GetMaxCraftAmount(selectedRecipeScript.Recipe);
                UpdateCraftAmountArea();
                BeginCrafting();
            } else {
                //Debug.Log("MySelectedRecipeScript is null!");
            }
        }

        public void BeginCrafting() {
            //Debug.Log("CraftingUI.BeginCrafting()");
            if (selectedRecipeScript != null) {
                for (int i = 0; i < craftAmount; i++) {
                    craftingManager.CraftingQueue.Add(selectedRecipeScript.Recipe);
                }
                playerManager.MyCharacter.CharacterAbilityManager.BeginAbility(craftAbility);
            } else {
                //Debug.Log("MySelectedRecipeScript is null!");
            }
        }


        public void UpdateCraftAmountArea() {
            //Debug.Log("CraftingUI.UpdateCraftAmountArea()");
            int maxAmount = 0;
            if (selectedRecipeScript != null) {
                maxAmount = craftingManager.GetMaxCraftAmount(selectedRecipeScript.Recipe);
                if (craftAmount == 0 && maxAmount > 0) {
                    craftAmount = 1;
                }
            }

            //Debug.Log("CraftingUI.UpdateCraftAmountArea(): maxAmount: " + maxAmount);
            if (craftAmount > maxAmount) {
                craftAmount = maxAmount;
            }

            if (craftAmount == 0) {
                craftButton.Button.interactable = false;
                craftAllButton.Button.interactable = false;
                lessButton.interactable = false;
                moreButton.interactable = false;
                if (maxAmount > 0) {
                    moreButton.interactable = true;
                }
            } else {
                lessButton.interactable = true;
                if (maxAmount > craftAmount) {
                    moreButton.interactable = true;
                } else {
                    moreButton.interactable = false;
                }
                craftButton.Button.interactable = true;
                craftAllButton.Button.interactable = true;
            }
            craftAmountText.text = craftAmount.ToString();


            uINavigationControllers[1].UpdateNavigationList();

            if (currentNavigationController ==  uINavigationControllers[1]) {
                currentNavigationController.FocusCurrentButton();
            }
        }

        public void IncreaseCraftAmount() {
            craftAmount++;
            UpdateCraftAmountArea();
        }

        public void DecreaseCraftAmount() {
            if (craftAmount > 0) {
                craftAmount--;
                UpdateCraftAmountArea();
            }
        }

        public override void LBButton() {
            base.LBButton();
            DecreaseCraftAmount();
        }

        public override void RBButton() {
            base.RBButton();
            IncreaseCraftAmount();
        }


    }

}