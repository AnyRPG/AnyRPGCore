using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CraftingManager : ConfiguredMonoBehaviour {

        public event System.Action OnCraftAmountUpdated = delegate { };
        public event System.Action<CraftAbility> OnSetCraftAbility = delegate { };
        public event System.Action<Recipe> OnSelectRecipe = delegate { };

        private CraftAbility craftAbility = null;

        // the number of items to craft
        private int craftAmount = 1;

        private List<Recipe> craftingQueue = new List<Recipe>();

        private Recipe currentRecipe = null;

        private Coroutine waitCoroutine = null;

        // game manager references
        private UIManager uIManager = null;
        private PlayerManager playerManager = null;
        //private InventoryManager inventoryManager = null;
        private SystemItemManager systemItemManager = null;

        public List<Recipe> CraftingQueue { get => craftingQueue; set => craftingQueue = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            playerManager = systemGameManager.PlayerManager;
            //inventoryManager = systemGameManager.InventoryManager;
            systemItemManager = systemGameManager.SystemItemManager;
        }

        public void TriggerCraftAmountUpdated() {
            OnCraftAmountUpdated();
        }

        public void SetAbility(CraftAbility craftAbility) {
            this.craftAbility = craftAbility;
            uIManager.craftingWindow.OpenWindow();
            OnSetCraftAbility(this.craftAbility);
        }

        public void ClearSelectedRecipe() {
            currentRecipe = null;
        }

        public void SetSelectedRecipe(Recipe recipe) {
            if (currentRecipe != recipe) {
                currentRecipe = recipe;
                OnSelectRecipe(currentRecipe);
            }
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

        private bool CanCraft(Recipe recipe) {
            for (int i = 0; i < recipe.CraftingMaterials.Count; i++) {
                if (playerManager.MyCharacter.CharacterInventoryManager.GetItemCount(recipe.CraftingMaterials[i].Item.DisplayName) < recipe.CraftingMaterials[i].Count) {
                    return false;
                }
            }
            return true;
        }

        public int GetMaxCraftAmount(Recipe checkRecipe) {
            //Debug.Log("CraftingUI.GetMaxCraftAmount()");

            int maxAmount = -1;
            for (int i = 0; i < checkRecipe.CraftingMaterials.Count; i++) {
                int possibleAmount = playerManager.MyCharacter.CharacterInventoryManager.GetItemCount(checkRecipe.CraftingMaterials[i].Item.DisplayName) / checkRecipe.CraftingMaterials[i].Count;
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
                playerManager.MyCharacter.CharacterAbilityManager.BeginAbility(craftAbility);
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
            //Debug.Log("CraftingUI.CraftNextItem(): " + CraftingQueue.Count + " items in crafting queue");

            // PERFORM CHECK FOR MATERIALS IN INVENTORY FIRST IN CASE QUEUE GOT BIGGER THAN MATERIAL AMOUNT BY ACCIDENT / RACE CONDITION, also for bag space
            if (GetMaxCraftAmount(craftingQueue[0]) > 0) {
                Item tmpItem = systemItemManager.GetNewResource(craftingQueue[0].Output.DisplayName);
                tmpItem.DropLevel = playerManager.MyCharacter.CharacterStats.Level;
                if (playerManager.MyCharacter.CharacterInventoryManager.AddItem(tmpItem, false)) {
                    //Debug.Log("CraftingUI.CraftNextItem(): got an item successfully");
                    foreach (CraftingMaterial craftingMaterial in craftingQueue[0].CraftingMaterials) {
                        //Debug.Log("CraftingUI.CraftNextItem(): looping through crafting materials");
                        for (int i = 0; i < craftingMaterial.Count; i++) {
                            //Debug.Log("CraftingUI.CraftNextItem(): about to remove item from inventory");
                            playerManager.MyCharacter.CharacterInventoryManager.RemoveItem(playerManager.MyCharacter.CharacterInventoryManager.GetItems(craftingMaterial.Item.DisplayName, 1)[0]);
                        }
                    }
                    craftingQueue.RemoveAt(0);
                    //UpdateCraftAmountArea();
                    if (craftingQueue.Count > 0) {
                        //Debug.Log("CraftingUI.CraftNextItem(): count: " + craftingQueue.Count);
                        // because this gets called as the last part of the cast, which is still technically in progress, we have to stopcasting first or it will fail to start because the coroutine is not null
                        //SystemGameManager.Instance.PlayerManager.MyCharacter.MyCharacterAbilityManager.StopCasting();

                        playerManager.MyCharacter.CharacterAbilityManager.BeginAbility(craftAbility);
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
                waitCoroutine = systemGameManager.StartCoroutine(CraftNextItemDelay());
            }
        }

        private IEnumerator CraftNextItemDelay() {
            //Debug.Log("CraftingUI.CraftNextItemDelay()");
            yield return null;
            waitCoroutine = null;
            CraftNextItem();
        }

        public void CancelCrafting() {
            //Debug.Log("CraftingUI.CancelCrafting()");
            craftingQueue.Clear();
            playerManager.MyCharacter.CharacterAbilityManager.StopCasting();
        }

    }

}