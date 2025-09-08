using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CharacterCraftingManager : ConfiguredClass {

        protected UnitController unitController;

        private CraftAbilityProperties craftAbility = null;

        private List<Recipe> craftingQueue = new List<Recipe>();

        private Coroutine waitCoroutine = null;

        public List<Recipe> CraftingQueue { get => craftingQueue; set => craftingQueue = value; }

        public CharacterCraftingManager(UnitController unitController, SystemGameManager systemGameManager) {
            this.unitController = unitController;
            Configure(systemGameManager);
        }

        public void SetCraftAbility(CraftAbilityProperties craftAbility) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterCraftingManager.SetCraftAbility({craftAbility.DisplayName})");

            this.craftAbility = craftAbility;
            ClearCraftingQueue();
            unitController.UnitEventController.NotifyOnSetCraftAbility(this.craftAbility);
        }

        public List<Recipe> GetRecipes() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterCraftingManager.GetRecipes()");

            List<Recipe> returnList = new List<Recipe>();
            foreach (Recipe recipe in unitController.CharacterRecipeManager.RecipeList.Values) {
                if (craftAbility == recipe.CraftAbility) {
                    returnList.Add(recipe);
                }
            }
            return returnList;
        }

        public int GetMaxCraftAmount(Recipe checkRecipe) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterCraftingManager.GetMaxCraftAmount({checkRecipe.DisplayName})");

            int maxAmount = -1;
            for (int i = 0; i < checkRecipe.CraftingMaterials.Count; i++) {
                int possibleAmount = unitController.CharacterInventoryManager.GetItemCount(checkRecipe.CraftingMaterials[i].Item.ResourceName) / checkRecipe.CraftingMaterials[i].Count;
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
            //Debug.Log($"{unitController.gameObject.name}.CharacterCraftingManager.ClearCraftingQueue()");

            craftingQueue.Clear();
            unitController.UnitEventController.NotifyOnClearCraftingQueue();
        }

        public void CraftNextItem() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterCraftingManager.CraftNextItem()");

            if (craftingQueue.Count == 0) {
                //Debug.Log("CraftingUI.CraftNextItem(): no more items to craft");
                return;
            }
            //Debug.Log("CraftingUI.CraftNextItem(): " + CraftingQueue.Count + " items in crafting queue");

            // PERFORM CHECK FOR MATERIALS IN INVENTORY FIRST IN CASE QUEUE GOT BIGGER THAN MATERIAL AMOUNT BY ACCIDENT / RACE CONDITION, also for bag space
            if (GetMaxCraftAmount(craftingQueue[0]) > 0) {
                InstantiatedItem tmpItem = unitController.CharacterInventoryManager.GetNewInstantiatedItem(craftingQueue[0].Output.ResourceName);
                if (unitController.CharacterInventoryManager.AddItem(tmpItem, false)) {
                    //Debug.Log("CraftingUI.CraftNextItem(): got an item successfully");
                    foreach (CraftingMaterial craftingMaterial in craftingQueue[0].CraftingMaterials) {
                        //Debug.Log("CraftingUI.CraftNextItem(): looping through crafting materials");
                        for (int i = 0; i < craftingMaterial.Count; i++) {
                            //Debug.Log("CraftingUI.CraftNextItem(): about to remove item from inventory");
                            unitController.CharacterInventoryManager.RemoveInventoryItem(unitController.CharacterInventoryManager.GetItems(craftingMaterial.Item.ResourceName, 1)[0]);
                        }
                    }
                    unitController.UnitEventController.NotifyOnCraftItem();
                    RemoveFirstQueueItem();
                    if (craftingQueue.Count > 0) {
                        //Debug.Log("CraftingUI.CraftNextItem(): count: " + craftingQueue.Count);
                        // because this gets called as the last part of the cast, which is still technically in progress, we have to stopcasting first or it will fail to start because the coroutine is not null
                        //SystemGameManager.Instance.PlayerManager.MyCharacter.MyCharacterAbilityManager.StopCasting();

                        unitController.CharacterAbilityManager.BeginAbility(craftAbility);
                    }
                }
            } else {
                // empty the queue to prevent repeated loop trying to craft something you don't have materials for
                ClearCraftingQueue();
            }
        }

        public void RemoveFirstQueueItem() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterCraftingManager.RemoveFirstQueueItem()");
            if (craftingQueue.Count > 0) {
                craftingQueue.RemoveAt(0);
            }
            unitController.UnitEventController.NotifyOnRemoveFirstCraftingQueueItem();

        }

        public void CraftNextItemWait() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterCraftingManager.CraftNextItemWait()");

            // add delay to avoid issues with cast in progress from current crafting item
            if (waitCoroutine == null) {
                waitCoroutine = unitController.StartCoroutine(CraftNextItemDelay());
            }
        }

        private IEnumerator CraftNextItemDelay() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterCraftingManager.CraftNextItemDelay()");

            yield return null;
            waitCoroutine = null;
            CraftNextItem();
        }

        public void CancelCrafting() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterCraftingManager.CancelCrafting()");

            ClearCraftingQueue();
            unitController.CharacterAbilityManager.TryToStopCasting();
        }

        public void BeginCrafting(Recipe recipe, int craftAmount) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterCraftingManager.BeginCrafting({recipe.DisplayName}, {craftAmount})");

            for (int i = 0; i < craftAmount; i++) {
                AddToCraftingQueue(recipe);
            }
            unitController.CharacterAbilityManager.BeginAbility(craftAbility);
        }

        public void AddToCraftingQueue(Recipe recipe) {
            craftingQueue.Add(recipe);
            unitController.UnitEventController.NotifyOnAddToCraftingQueue(recipe);
        }
    }

}