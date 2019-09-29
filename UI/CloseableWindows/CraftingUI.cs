using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftingUI : WindowContentController {

    #region Singleton
    private static CraftingUI instance;

    public static CraftingUI MyInstance
    {
        get
        {
            if (instance == null) {
                instance = FindObjectOfType<CraftingUI>();
            }

            return instance;
        }
    }

    #endregion

    // holds all the recipes
    private CraftAbility craftAbility;

    // the actual craft spell
    [SerializeField]
    private InstantEffectAbility craftAction;

    [SerializeField]
    private GameObject craftButton, craftAllButton, lessButton, moreButton, cancelButton;

    [SerializeField]
    private Text craftAmountText;

    [SerializeField]
    private GameObject recipePrefab;

    [SerializeField]
    private Transform recipeParent;

    [SerializeField]
    private GameObject availableArea;

    [SerializeField]
    private GameObject hiddenHeading;

    [SerializeField]
    private GameObject hiddenArea;

    [SerializeField]
    private GameObject recipeDetailsArea;

    [SerializeField]
    private Text recipeDescription;

    [SerializeField]
    private GameObject materialsHeading;


    [SerializeField]
    private DescribableCraftingOutputIcon outputIcon;

    [SerializeField]
    private DescribableCraftingInputIcon[] inputIcons;

    // the number of items to craft
    private int craftAmount = 1;

    //private List<recipeNode> recipeNodes = new List<recipeNode>();

    private List<RecipeScript> recipeScripts = new List<RecipeScript>();

    private List<string> craftingQueue = new List<string>();

    private RecipeScript selectedRecipeScript;

    private string currentRecipeName = null;

    public override event System.Action<ICloseableWindowContents> OnOpenWindowHandler = delegate { };

    public RecipeScript MySelectedRecipeScript { get => selectedRecipeScript; set => selectedRecipeScript = value; }

    private void Start() {
        DeactivateButtons();
    }

    public void DeactivateButtons() {
        Button craftButtonComponent = craftButton.GetComponent<Button>();
        if (craftButtonComponent != null) {
            craftButton.GetComponent<Button>().enabled = false;
        }
        Button craftAllButtonComponent = craftButton.GetComponent<Button>();
        if (craftAllButtonComponent != null) {
            craftAllButton.GetComponent<Button>().enabled = false;
        }
    }

    public void ShowRecipesCommon(CraftAbility craftAbility) {
        //Debug.Log("craftingUI.ShowRecipesCommon(" + craftAbility.name + ")");
        Clearrecipes();
        RecipeScript firstScript = null;
        foreach (Recipe recipe in craftAbility.GetRecipes()) {
            //Debug.Log("craftingUI.ShowRecipesCommon(" + craftAbility.name + ") : adding recipe:" + recipe.MyOutput.itemName);
            if (recipe.MyOutput != null) {
                GameObject go = Instantiate(recipePrefab, recipeParent);
                RecipeScript qs = go.GetComponentInChildren<RecipeScript>();
                if (firstScript == null) {
                    firstScript = qs;
                }
                qs.MyText.text = recipe.MyOutput.MyName;
                qs.SetRecipeName(recipe.MyName);
                recipeScripts.Add(qs);
            } else {
                Debug.Log("Recipe Output is null!");
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
        Debug.Log("CraftingUI.UpdateSelected()");
        if (selectedRecipeScript != null) {
            craftAmount = 1;
            ShowDescription(selectedRecipeScript.MyRecipeName);
        }
    }

    public void ShowDescription(string recipeName) {
        Debug.Log("CraftingUI.ShowDescription(" + recipeName + ")");
        ClearDescription();

        if (recipeName == null && recipeName == string.Empty) {
            return;
        }
        currentRecipeName = recipeName;

        Recipe recipe = SystemRecipeManager.MyInstance.GetResource(recipeName);
        if (recipe == null) {
            Debug.Log("SkillTrainerUI.ShowDescription(" + recipeName + "): failed to get skill from SystemSkillManager");
        }

        recipeDescription.text = string.Format("<b>{0}</b>", recipe.MyOutput.MyName, recipe.MyDescription);

        outputIcon.SetDescribable(recipe.MyOutput, recipe.MyOutputCount);
        
        if (recipe.MyCraftingMaterials.Length > 0) {
            materialsHeading.gameObject.SetActive(true);
        }

        // show crafting materials
        for (int i = 0; i < recipe.MyCraftingMaterials.Length; i++) {
            inputIcons[i].MyMaterialSlot.SetActive(true);
            inputIcons[i].SetDescribable(recipe.MyCraftingMaterials[i].MyItem, recipe.MyCraftingMaterials[i].MyCount);
        }

        UpdateCraftAmountArea();

    }


    private bool CanCraft(Recipe recipe) {
        Debug.Log("CraftingUI.CanCraft(" + recipe.MyOutput.MyName + ")");
        for (int i = 0; i < recipe.MyCraftingMaterials.Length; i++) {
            if (InventoryManager.MyInstance.GetItemCount(recipe.MyCraftingMaterials[i].MyItem.MyName) < recipe.MyCraftingMaterials[i].MyCount) {
                return false;
            }
        }
        return true;
    }

    public void ClearDescription() {
        Debug.Log("CraftingUI.ClearDescription()");
        craftAmount = 1;
        //recipeDetailsArea.SetActive(false);
        recipeDescription.text = string.Empty;
        //materialsHeading.gameObject.SetActive(false);
        ClearInputIcons();
        DeselectRecipes();
    }

    public void DeselectRecipes() {
        Debug.Log("CraftingUI.DeselectRecipes()");
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

    public override void OnCloseWindow() {
        Debug.Log("craftingUI.OnCloseWindow()");
        base.OnCloseWindow();
        DeactivateButtons();
        Debug.Log("craftingUI.OnCloseWindow(): nulling recipe script");
        MySelectedRecipeScript = null;
    }

    public override void OnOpenWindow() {
        //Debug.Log("craftingUI.OnOpenWindow()");
        base.OnOpenWindow();

        craftingQueue.Clear();

        ClearDescription();
        OnOpenWindowHandler(this);
        UpdateCraftAmountArea();
        PopupWindowManager.MyInstance.craftingWindow.SetWindowTitle(craftAbility.MyName);

    }

    private void ClearInputIcons() {
        foreach (DescribableCraftingInputIcon inputIcon in inputIcons) {
            inputIcon.MyMaterialSlot.SetActive(false);
        }
    }

    private int GetMaxCraftAmount(string recipeName) {
        //Debug.Log("CraftingUI.GetMaxCraftAmount()");
        Recipe recipe = SystemRecipeManager.MyInstance.GetResource(recipeName);

        int maxAmount = -1;
        for (int i = 0; i < recipe.MyCraftingMaterials.Length; i++) {
            int possibleAmount = InventoryManager.MyInstance.GetItemCount(recipe.MyCraftingMaterials[i].MyItem.MyName) / recipe.MyCraftingMaterials[i].MyCount;
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
        if (MySelectedRecipeScript != null) {
            craftAmount = GetMaxCraftAmount(MySelectedRecipeScript.MyRecipeName);
            UpdateCraftAmountArea();
            BeginCrafting();
        } else {
            Debug.Log("MySelectedRecipeScript is null!");
        }
    }

    public void BeginCrafting() {
        //Debug.Log("CraftingUI.BeginCrafting()");
        if (MySelectedRecipeScript != null) {
            for (int i = 0; i < craftAmount; i++) {
                craftingQueue.Add(MySelectedRecipeScript.MyRecipeName);
            }
            PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.BeginAbility(craftAction);
        } else {
            //Debug.Log("MySelectedRecipeScript is null!");
        }
    }

    public void CraftNextItem() {
        //Debug.Log("CraftingUI.CraftNextItem()");
        if (craftingQueue.Count == 0) {
            return;
        }

        Recipe recipe = SystemRecipeManager.MyInstance.GetResource(craftingQueue[0]);
        // PERFORM CHECK FOR MATERIALS IN INVENTORY FIRST IN CASE QUEUE GOT BIGGER THAN MATERIAL AMOUNT BY ACCIDENT / RACE CONDITION, also for bag space
        if (GetMaxCraftAmount(craftingQueue[0]) > 0 && InventoryManager.MyInstance.AddItem(SystemItemManager.MyInstance.GetResource(recipe.MyOutput.MyName))) {
            foreach (CraftingMaterial craftingMaterial in recipe.MyCraftingMaterials) {
                //Debug.Log("CraftingUI.CraftNextItem(): looping through crafting materials");
                for (int i = 0; i < craftingMaterial.MyCount; i++) {
                    //Debug.Log("CraftingUI.CraftNextItem(): about to remove item from inventory");
                    InventoryManager.MyInstance.RemoveItem(InventoryManager.MyInstance.GetItems(craftingMaterial.MyItem.MyName, 1)[0]);
                }
            }
            craftingQueue.RemoveAt(0);
            //UpdateCraftAmountArea();
            if (craftingQueue.Count > 0) {
                if (craftAction == null) {
                    //Debug.Log("CraftingUI.CraftNextItem(). CraftAction is null!");
                }
                PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.BeginAbility(craftAction as IAbility);
            }
        } else {
            // empty the queue to prevent repeated loop trying to craft something you don't have materials for
            craftingQueue.Clear();
        }
    }

    public void UpdateCraftAmountArea() {
        //Debug.Log("CraftingUI.UpdateCraftAmountArea()");
        int maxAmount = 0;
        if (MySelectedRecipeScript != null) {
            maxAmount = GetMaxCraftAmount(MySelectedRecipeScript.MyRecipeName);
            if (craftAmount == 0 && maxAmount > 0) {
                craftAmount = 1;
            }
        }

        //Debug.Log("CraftingUI.UpdateCraftAmountArea(): maxAmount: " + maxAmount);
        if (craftAmount > maxAmount) {
            craftAmount = maxAmount;
        }

        if (craftAmount == 0) {
            craftButton.GetComponent<Button>().enabled = false;
            craftAllButton.GetComponent<Button>().enabled = false;
            lessButton.GetComponent<Button>().enabled = false;
            if (maxAmount > 0) {
                moreButton.GetComponent<Button>().enabled = true;
            }
        } else {
            lessButton.GetComponent<Button>().enabled = true;
            if (maxAmount > craftAmount) {
                moreButton.GetComponent<Button>().enabled = true;
            } else {
                moreButton.GetComponent<Button>().enabled = false;
            }
            craftButton.GetComponent<Button>().enabled = true;
            craftAllButton.GetComponent<Button>().enabled = true;
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
        Debug.Log("CraftingUI.CancelCrafting()");
        PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.StopCasting();
        craftingQueue.Clear();
    }

}
