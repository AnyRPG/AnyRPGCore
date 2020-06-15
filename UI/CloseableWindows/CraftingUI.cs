using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CraftingUI : WindowContentController {

        #region Singleton
        private static CraftingUI instance;

        public static CraftingUI MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<CraftingUI>();
                }

                return instance;
            }
        }

        #endregion

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

        //[SerializeField]
        //private GameObject availableArea = null;

        //[SerializeField]
        //private GameObject hiddenHeading = null;

        //[SerializeField]
        //private GameObject hiddenArea = null;

        //[SerializeField]
        //private GameObject recipeDetailsArea = null;

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

        private List<RecipeScript> recipeScripts = new List<RecipeScript>();

        private List<Recipe> craftingQueue = new List<Recipe>();

        private RecipeScript selectedRecipeScript;

        //private string currentRecipeName = null;

        private Recipe currentRecipe = null;

        public override event System.Action<ICloseableWindowContents> OnOpenWindow = delegate { };

        public RecipeScript MySelectedRecipeScript { get => selectedRecipeScript; set => selectedRecipeScript = value; }
        public List<Recipe> CraftingQueue { get => craftingQueue; set => craftingQueue = value; }

        private Coroutine waitCoroutine = null;

        private void Start() {
            DeactivateButtons();
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

        // meant to be called externally from craftingNode
        public void ViewRecipes(CraftAbility craftAbility) {
            this.craftAbility = craftAbility;
            PopupWindowManager.MyInstance.craftingWindow.OpenWindow();
            ShowRecipes(craftAbility);
        }

        public List<Recipe> GetRecipes() {
            //Debug.Log("CraftAbility.GetRecipes() this: " + this.name);
            List<Recipe> returnList = new List<Recipe>();
            foreach (Recipe recipe in PlayerManager.MyInstance.MyCharacter.PlayerRecipeManager.RecipeList.Values) {
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
                    GameObject go = Instantiate(recipePrefab, recipeParent);
                    RecipeScript qs = go.GetComponentInChildren<RecipeScript>();
                    if (firstScript == null) {
                        firstScript = qs;
                    }
                    qs.MyText.text = recipe.MyOutput.MyDisplayName;
                    qs.SetRecipe(recipe);
                    recipeScripts.Add(qs);
                } else {
                    //Debug.Log("Recipe Output is null!");
                }
            }

            //if (MySelectedRecipeScript != null) {
            //MySelectedRecipeScript.Select();
            //} else {
            MySelectedRecipeScript = firstScript;
            firstScript.Select();
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
                ShowDescription(selectedRecipeScript.MyRecipe);
            }
        }

        public void ShowDescription(Recipe newRecipe) {
            //Debug.Log("CraftingUI.ShowDescription(" + recipeName + ")");
            ClearDescription();

            if (newRecipe == null) {
                return;
            }
            currentRecipe = newRecipe;

            recipeDescription.text = string.Format("<b>{0}</b>", newRecipe.MyOutput.MyDisplayName, newRecipe.MyDescription);

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


        private bool CanCraft(Recipe recipe) {
            //Debug.Log("CraftingUI.CanCraft(" + recipe.MyOutput.MyName + ")");
            for (int i = 0; i < recipe.MyCraftingMaterials.Count; i++) {
                if (InventoryManager.MyInstance.GetItemCount(recipe.MyCraftingMaterials[i].MyItem.MyDisplayName) < recipe.MyCraftingMaterials[i].MyCount) {
                    return false;
                }
            }
            return true;
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
            foreach (RecipeScript recipe in recipeScripts) {
                if (recipe != MySelectedRecipeScript) {
                    recipe.DeSelect();
                }
            }
        }

        public void Clearrecipes() {
            //Debug.Log("CraftingUI.ClearRecipes()");
            // clear the recipe list so any recipes left over from a previous time opening the window aren't shown
            //Debug.Log("running clear recipes in recipegiverUI; recipegiver: " + tradeSkill.name);
            foreach (RecipeScript recipeScript in recipeScripts) {
                //Debug.Log("The recipenode has a gameobject we need to clear");
                recipeScript.gameObject.transform.SetParent(null);
                Destroy(recipeScript.gameObject);
            }
            recipeScripts.Clear();
        }

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("craftingUI.OnCloseWindow()");
            base.RecieveClosedWindowNotification();
            DeactivateButtons();
            //Debug.Log("craftingUI.OnCloseWindow(): nulling recipe script");
            MySelectedRecipeScript = null;
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("craftingUI.OnOpenWindow()");
            base.ReceiveOpenWindowNotification();

            craftingQueue.Clear();

            ClearDescription();
            OnOpenWindow(this);
            UpdateCraftAmountArea();
            PopupWindowManager.MyInstance.craftingWindow.SetWindowTitle(craftAbility.MyDisplayName);

        }

        private void ClearInputIcons() {
            foreach (DescribableCraftingInputIcon inputIcon in inputIcons) {
                inputIcon.MyMaterialSlot.SetActive(false);
            }
        }

        private int GetMaxCraftAmount(Recipe checkRecipe) {
            //Debug.Log("CraftingUI.GetMaxCraftAmount()");

            int maxAmount = -1;
            for (int i = 0; i < checkRecipe.MyCraftingMaterials.Count; i++) {
                int possibleAmount = InventoryManager.MyInstance.GetItemCount(checkRecipe.MyCraftingMaterials[i].MyItem.MyDisplayName) / checkRecipe.MyCraftingMaterials[i].MyCount;
                if (maxAmount == -1) {
                    maxAmount = possibleAmount;
                }
                if (possibleAmount < maxAmount) {
                    maxAmount = possibleAmount;
                }
            }
            return maxAmount;
        }

        public void CraftAll() {
            //Debug.Log("CraftingUI.CraftAll()");
            if (MySelectedRecipeScript != null) {
                craftAmount = GetMaxCraftAmount(MySelectedRecipeScript.MyRecipe);
                UpdateCraftAmountArea();
                BeginCrafting();
            } else {
                //Debug.Log("MySelectedRecipeScript is null!");
            }
        }

        public void BeginCrafting() {
            //Debug.Log("CraftingUI.BeginCrafting()");
            if (MySelectedRecipeScript != null) {
                for (int i = 0; i < craftAmount; i++) {
                    craftingQueue.Add(MySelectedRecipeScript.MyRecipe);
                }
                PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.BeginAbility(craftAbility);
            } else {
                //Debug.Log("MySelectedRecipeScript is null!");
            }
        }

        public void CraftNextItem() {
            //Debug.Log("CraftingUI.CraftNextItem()");
            if (craftingQueue.Count == 0) {
                //Debug.Log("CraftingUI.CraftNextItem(): no more items to craft");
                return;
            }

            // PERFORM CHECK FOR MATERIALS IN INVENTORY FIRST IN CASE QUEUE GOT BIGGER THAN MATERIAL AMOUNT BY ACCIDENT / RACE CONDITION, also for bag space
            if (GetMaxCraftAmount(craftingQueue[0]) > 0) {
                Item tmpItem = SystemItemManager.MyInstance.GetNewResource(craftingQueue[0].MyOutput.MyDisplayName);
                tmpItem.DropLevel = PlayerManager.MyInstance.MyCharacter.CharacterStats.Level;
                if (InventoryManager.MyInstance.AddItem(tmpItem)) {
                    //Debug.Log("CraftingUI.CraftNextItem(): got an item successfully");
                    foreach (CraftingMaterial craftingMaterial in craftingQueue[0].MyCraftingMaterials) {
                        //Debug.Log("CraftingUI.CraftNextItem(): looping through crafting materials");
                        for (int i = 0; i < craftingMaterial.MyCount; i++) {
                            //Debug.Log("CraftingUI.CraftNextItem(): about to remove item from inventory");
                            InventoryManager.MyInstance.RemoveItem(InventoryManager.MyInstance.GetItems(craftingMaterial.MyItem.MyDisplayName, 1)[0]);
                        }
                    }
                    craftingQueue.RemoveAt(0);
                    //UpdateCraftAmountArea();
                    if (craftingQueue.Count > 0) {
                        //Debug.Log("CraftingUI.CraftNextItem(): count: " + craftingQueue.Count);
                        // because this gets called as the last part of the cast, which is still technically in progress, we have to stopcasting first or it will fail to start because the coroutine is not null
                        //PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.StopCasting();

                        PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.BeginAbility(craftAbility);
                    }
                }
            } else {
                // empty the queue to prevent repeated loop trying to craft something you don't have materials for
                craftingQueue.Clear();
            }
        }

        public void CraftNextItemWait() {
            //Debug.Log("CraftingUI.CraftNextItemWait()");
            // add delay to avoid issues with cast in progress from current crafting item
            if (waitCoroutine == null) {
                waitCoroutine = StartCoroutine(CraftNextItemDelay());
            }
        }

        private IEnumerator CraftNextItemDelay() {
            //Debug.Log("CraftingUI.CraftNextItemDelay()");
            yield return null;
            waitCoroutine = null;
            CraftNextItem();
        }

        public void UpdateCraftAmountArea() {
            //Debug.Log("CraftingUI.UpdateCraftAmountArea()");
            int maxAmount = 0;
            if (MySelectedRecipeScript != null) {
                maxAmount = GetMaxCraftAmount(MySelectedRecipeScript.MyRecipe);
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

        public void CancelCrafting() {
            //Debug.Log("CraftingUI.CancelCrafting()");
            craftingQueue.Clear();
            PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.StopCasting();
        }

    }

}