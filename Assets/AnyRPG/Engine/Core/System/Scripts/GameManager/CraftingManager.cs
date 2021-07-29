using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CraftingManager  {

        public event System.Action OnCraftAmountUpdated = delegate { };
        public event System.Action<CraftAbility> OnSetCraftAbility = delegate { };
        public event System.Action<Recipe> OnSelectRecipe = delegate { };

        private CraftAbility craftAbility = null;

        // the number of items to craft
        private int craftAmount = 1;

        private List<Recipe> craftingQueue = new List<Recipe>();

        private Recipe currentRecipe = null;

        public List<Recipe> CraftingQueue { get => craftingQueue; set => craftingQueue = value; }

        private Coroutine waitCoroutine = null;

        public void TriggerCraftAmountUpdated() {
            OnCraftAmountUpdated();
        }

        public void SetAbility(CraftAbility craftAbility) {
            this.craftAbility = craftAbility;
            SystemGameManager.Instance.UIManager.PopupWindowManager.craftingWindow.OpenWindow();
            OnSetCraftAbility(this.craftAbility);
        }

        public void SetSelectedRecipe(Recipe recipe) {
            currentRecipe = recipe;
            OnSelectRecipe(currentRecipe);
        }

        public List<Recipe> GetRecipes() {
            //Debug.Log("CraftAbility.GetRecipes() this: " + this.name);
            List<Recipe> returnList = new List<Recipe>();
            foreach (Recipe recipe in SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterRecipeManager.RecipeList.Values) {
                if (craftAbility == recipe.CraftAbility) {
                    returnList.Add(recipe);
                }
            }
            return returnList;
        }

        private bool CanCraft(Recipe recipe) {
            //Debug.Log("CraftingUI.CanCraft(" + recipe.MyOutput.MyName + ")");
            for (int i = 0; i < recipe.MyCraftingMaterials.Count; i++) {
                if (SystemGameManager.Instance.InventoryManager.GetItemCount(recipe.MyCraftingMaterials[i].MyItem.DisplayName) < recipe.MyCraftingMaterials[i].MyCount) {
                    return false;
                }
            }
            return true;
        }

        public int GetMaxCraftAmount(Recipe checkRecipe) {
            //Debug.Log("CraftingUI.GetMaxCraftAmount()");

            int maxAmount = -1;
            for (int i = 0; i < checkRecipe.MyCraftingMaterials.Count; i++) {
                int possibleAmount = SystemGameManager.Instance.InventoryManager.GetItemCount(checkRecipe.MyCraftingMaterials[i].MyItem.DisplayName) / checkRecipe.MyCraftingMaterials[i].MyCount;
                if (maxAmount == -1) {
                    maxAmount = possibleAmount;
                }
                if (possibleAmount < maxAmount) {
                    maxAmount = possibleAmount;
                }
            }
            return maxAmount;
        }

        public void ClearCraftingQueue() {
            craftingQueue.Clear();
        }

        public void CraftAll() {
            //Debug.Log("CraftingUI.CraftAll()");
            if (currentRecipe != null) {
                craftAmount = GetMaxCraftAmount(currentRecipe);
                OnCraftAmountUpdated();
                BeginCrafting();
            } else {
                //Debug.Log("MySelectedRecipeScript is null!");
            }
        }

        public void BeginCrafting() {
            //Debug.Log("CraftingUI.BeginCrafting()");
            if (currentRecipe != null) {
                for (int i = 0; i < craftAmount; i++) {
                    craftingQueue.Add(currentRecipe);
                }
                SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterAbilityManager.BeginAbility(craftAbility);
            } else {
                //Debug.Log("MySelectedRecipeScript is null!");
            }
        }

        public void CraftNextItem() {
            Debug.Log("CraftingUI.CraftNextItem()");
            if (craftingQueue.Count == 0) {
                Debug.Log("CraftingUI.CraftNextItem(): no more items to craft");
                return;
            }
            Debug.Log("CraftingUI.CraftNextItem(): " + CraftingQueue.Count + " items in crafting queue");

            // PERFORM CHECK FOR MATERIALS IN INVENTORY FIRST IN CASE QUEUE GOT BIGGER THAN MATERIAL AMOUNT BY ACCIDENT / RACE CONDITION, also for bag space
            if (GetMaxCraftAmount(craftingQueue[0]) > 0) {
                Item tmpItem = SystemItemManager.Instance.GetNewResource(craftingQueue[0].MyOutput.DisplayName);
                tmpItem.DropLevel = SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.Level;
                if (SystemGameManager.Instance.InventoryManager.AddItem(tmpItem)) {
                    //Debug.Log("CraftingUI.CraftNextItem(): got an item successfully");
                    foreach (CraftingMaterial craftingMaterial in craftingQueue[0].MyCraftingMaterials) {
                        //Debug.Log("CraftingUI.CraftNextItem(): looping through crafting materials");
                        for (int i = 0; i < craftingMaterial.MyCount; i++) {
                            //Debug.Log("CraftingUI.CraftNextItem(): about to remove item from inventory");
                            SystemGameManager.Instance.InventoryManager.RemoveItem(SystemGameManager.Instance.InventoryManager.GetItems(craftingMaterial.MyItem.DisplayName, 1)[0]);
                        }
                    }
                    craftingQueue.RemoveAt(0);
                    //UpdateCraftAmountArea();
                    if (craftingQueue.Count > 0) {
                        //Debug.Log("CraftingUI.CraftNextItem(): count: " + craftingQueue.Count);
                        // because this gets called as the last part of the cast, which is still technically in progress, we have to stopcasting first or it will fail to start because the coroutine is not null
                        //SystemGameManager.Instance.PlayerManager.MyCharacter.MyCharacterAbilityManager.StopCasting();

                        SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterAbilityManager.BeginAbility(craftAbility);
                    }
                }
            } else {
                // empty the queue to prevent repeated loop trying to craft something you don't have materials for
                craftingQueue.Clear();
            }
        }

        public void CraftNextItemWait() {
            Debug.Log("CraftingUI.CraftNextItemWait()");
            // add delay to avoid issues with cast in progress from current crafting item
            if (waitCoroutine == null) {
                waitCoroutine = SystemGameManager.Instance.StartCoroutine(CraftNextItemDelay());
            }
        }

        private IEnumerator CraftNextItemDelay() {
            Debug.Log("CraftingUI.CraftNextItemDelay()");
            yield return null;
            waitCoroutine = null;
            CraftNextItem();
        }

        public void CancelCrafting() {
            //Debug.Log("CraftingUI.CancelCrafting()");
            craftingQueue.Clear();
            SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterAbilityManager.StopCasting();
        }

    }

}