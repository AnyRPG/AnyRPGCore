using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CraftingUI : WindowContentController {

        // holds all the recipes
        private CraftAbility craftAbility = null;

        [SerializeField]
        private GameObject craftButton = null;

        [SerializeField]
        private GameObject craftAllButton = null;

        [SerializeField]
        private GameObject lessButton = null;

        [SerializeField]
        private GameObject moreButton = null;

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

        // the number of items to craft
        private int craftAmount = 1;

        //private List<recipeNode> recipeNodes = new List<recipeNode>();

        private Dictionary<Recipe, RecipeScript> recipeScripts = new Dictionary<Recipe, RecipeScript>();

        private RecipeScript selectedRecipeScript;

        //private string currentRecipeName = null;

        private Recipe currentRecipe = null;

        public RecipeScript SelectedRecipeScript { get => selectedRecipeScript; set => selectedRecipeScript = value; }

        protected override void CreateEventSubscriptions() {
            //Debug.Log("CraftingUI.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            base.CreateEventSubscriptions();
            //SystemEventManager.Instance.OnPrerequisiteUpdated += CheckPrerequisites;
            CraftingManager.Instance.OnSelectRecipe += SelectRecipe;
            CraftingManager.Instance.OnCraftAmountUpdated += UpdateCraftAmountArea;
            CraftingManager.Instance.OnSetCraftAbility += ViewRecipes;
        }

        protected override void CleanupEventSubscriptions() {
            //Debug.Log("CraftingUI.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            base.CleanupEventSubscriptions();
            CraftingManager.Instance.OnSelectRecipe -= SelectRecipe;
            CraftingManager.Instance.OnCraftAmountUpdated -= UpdateCraftAmountArea;
            CraftingManager.Instance.OnSetCraftAbility -= ViewRecipes;
            //SystemEventManager.Instance.OnPrerequisiteUpdated -= CheckPrerequisites;
        }

        public void SelectRecipe(Recipe recipe) {
            selectedRecipeScript = recipeScripts[recipe];
            ShowDescription(recipe);
        }

        public void DeactivateButtons() {
            Button craftButtonComponent = craftButton.GetComponent<Button>();
            if (craftButtonComponent != null) {
                craftButton.GetComponent<Button>().interactable = false;
            }
            Button craftAllButtonComponent = craftButton.GetComponent<Button>();
            if (craftAllButtonComponent != null) {
                craftAllButton.GetComponent<Button>().interactable = false;
            }
        }

        public void CancelCrafting() {
            //Debug.Log("CraftingUI.CancelCrafting()");
            CraftingManager.Instance.CancelCrafting();
        }


        // meant to be called externally from craftingNode
        public void ViewRecipes(CraftAbility craftAbility) {
            this.craftAbility = craftAbility;
            //PopupWindowManager.Instance.craftingWindow.OpenWindow();
            ResetWindow();
            ShowRecipes(craftAbility);
        }

        public List<Recipe> GetRecipes() {
            //Debug.Log("CraftAbility.GetRecipes() this: " + this.name);
            List<Recipe> returnList = new List<Recipe>();
            foreach (Recipe recipe in PlayerManager.Instance.MyCharacter.CharacterRecipeManager.RecipeList.Values) {
                if (craftAbility == recipe.CraftAbility) {
                    returnList.Add(recipe);
                }
            }
            return returnList;
        }

        public void ShowRecipesCommon(CraftAbility craftAbility) {
            //Debug.Log("craftingUI.ShowRecipesCommon(" + craftAbility.name + ")");
            Clearrecipes();
            RecipeScript firstScript = null;
            foreach (Recipe recipe in GetRecipes()) {
                //Debug.Log("craftingUI.ShowRecipesCommon(" + craftAbility.name + ") : adding recipe:" + recipe.MyOutput.itemName);
                if (recipe.MyOutput != null) {
                    GameObject go = ObjectPooler.Instance.GetPooledObject(recipePrefab, recipeParent);
                    RecipeScript qs = go.GetComponentInChildren<RecipeScript>();
                    if (firstScript == null) {
                        firstScript = qs;
                    }
                    qs.MyText.text = recipe.MyOutput.DisplayName;
                    qs.SetRecipe(recipe);
                    recipeScripts.Add(recipe, qs);
                } else {
                    //Debug.Log("Recipe Output is null!");
                }
            }

            //if (MySelectedRecipeScript != null) {
            //MySelectedRecipeScript.Select();
            //} else {
            SelectedRecipeScript = firstScript;
            if (firstScript != null) {
                firstScript.Select();
            }
            //}
        }

        public void ShowRecipes() {
            //Debug.Log("craftingUI.Showrecipes()");
            ShowRecipesCommon(craftAbility);
        }

        public void ShowRecipes(CraftAbility craftAbility) {
            //Debug.Log("craftingUI.Showrecipes(" + craftAbility.name + ")");
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
            Debug.Log("CraftingUI.ShowDescription(" + newRecipe.DisplayName + ")");
            ClearDescription();

            if (newRecipe == null) {
                return;
            }
            currentRecipe = newRecipe;

            recipeDescription.text = string.Format("<b>{0}</b>", newRecipe.MyOutput.DisplayName, newRecipe.MyDescription);

            outputIcon.SetDescribable(newRecipe.MyOutput, newRecipe.MyOutputCount);

            if (newRecipe.MyCraftingMaterials.Count > 0) {
                materialsHeading.gameObject.SetActive(true);
            }

            // show crafting materials
            for (int i = 0; i < newRecipe.MyCraftingMaterials.Count; i++) {
                inputIcons[i].MyMaterialSlot.SetActive(true);
                inputIcons[i].SetDescribable(newRecipe.MyCraftingMaterials[i].MyItem, newRecipe.MyCraftingMaterials[i].MyCount);
            }

            UpdateCraftAmountArea();

        }

        public void ClearDescription() {
            //Debug.Log("CraftingUI.ClearDescription()");
            craftAmount = 1;
            //recipeDetailsArea.SetActive(false);
            recipeDescription.text = string.Empty;
            //materialsHeading.gameObject.SetActive(false);
            ClearInputIcons();
            DeselectRecipes();
        }

        public void DeselectRecipes() {
            //Debug.Log("CraftingUI.DeselectRecipes()");
            foreach (Recipe recipe in recipeScripts.Keys) {
                if (recipe != SelectedRecipeScript.Recipe) {
                    recipeScripts[recipe].DeSelect();
                }
            }
        }

        public void Clearrecipes() {
            //Debug.Log("CraftingUI.ClearRecipes()");
            // clear the recipe list so any recipes left over from a previous time opening the window aren't shown
            //Debug.Log("running clear recipes in recipegiverUI; recipegiver: " + tradeSkill.name);
            foreach (RecipeScript recipeScript in recipeScripts.Values) {
                //Debug.Log("The recipenode has a gameobject we need to clear");
                recipeScript.gameObject.transform.SetParent(null);
                recipeScript.DeSelect();
                ObjectPooler.Instance.ReturnObjectToPool(recipeScript.gameObject);
            }
            recipeScripts.Clear();
        }

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("craftingUI.OnCloseWindow()");
            base.RecieveClosedWindowNotification();
            //Debug.Log("craftingUI.OnCloseWindow(): nulling recipe script");
            SelectedRecipeScript = null;
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("CraftingUI.ReceiveOpenWindowNotification()");
            base.ReceiveOpenWindowNotification();
            SetBackGroundColor(new Color32(0, 0, 0, (byte)(int)(PlayerPrefs.GetFloat("PopupWindowOpacity") * 255)));

            DeactivateButtons();

            CraftingManager.Instance.ClearCraftingQueue();
        }

        private void ResetWindow() {
            ClearDescription();
            UpdateCraftAmountArea();
            PopupWindowManager.Instance.craftingWindow.SetWindowTitle(craftAbility.DisplayName);
        }

        private void ClearInputIcons() {
            foreach (DescribableCraftingInputIcon inputIcon in inputIcons) {
                inputIcon.MyMaterialSlot.SetActive(false);
            }
        }

        public void CraftAll() {
            //Debug.Log("CraftingUI.CraftAll()");
            if (SelectedRecipeScript != null) {
                craftAmount = CraftingManager.Instance.GetMaxCraftAmount(SelectedRecipeScript.Recipe);
                UpdateCraftAmountArea();
                BeginCrafting();
            } else {
                //Debug.Log("MySelectedRecipeScript is null!");
            }
        }

        public void BeginCrafting() {
            //Debug.Log("CraftingUI.BeginCrafting()");
            if (SelectedRecipeScript != null) {
                for (int i = 0; i < craftAmount; i++) {
                    CraftingManager.Instance.CraftingQueue.Add(SelectedRecipeScript.Recipe);
                }
                PlayerManager.Instance.MyCharacter.CharacterAbilityManager.BeginAbility(craftAbility);
            } else {
                //Debug.Log("MySelectedRecipeScript is null!");
            }
        }


        public void UpdateCraftAmountArea() {
            //Debug.Log("CraftingUI.UpdateCraftAmountArea()");
            int maxAmount = 0;
            if (SelectedRecipeScript != null) {
                maxAmount = CraftingManager.Instance.GetMaxCraftAmount(SelectedRecipeScript.Recipe);
                if (craftAmount == 0 && maxAmount > 0) {
                    craftAmount = 1;
                }
            }

            //Debug.Log("CraftingUI.UpdateCraftAmountArea(): maxAmount: " + maxAmount);
            if (craftAmount > maxAmount) {
                craftAmount = maxAmount;
            }

            if (craftAmount == 0) {
                craftButton.GetComponent<Button>().interactable = false;
                craftAllButton.GetComponent<Button>().interactable = false;
                lessButton.GetComponent<Button>().interactable = false;
                moreButton.GetComponent<Button>().interactable = false;
                if (maxAmount > 0) {
                    moreButton.GetComponent<Button>().interactable = true;
                }
            } else {
                lessButton.GetComponent<Button>().interactable = true;
                if (maxAmount > craftAmount) {
                    moreButton.GetComponent<Button>().interactable = true;
                } else {
                    moreButton.GetComponent<Button>().interactable = false;
                }
                craftButton.GetComponent<Button>().interactable = true;
                craftAllButton.GetComponent<Button>().interactable = true;
            }
            craftAmountText.text = craftAmount.ToString();
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


    }

}