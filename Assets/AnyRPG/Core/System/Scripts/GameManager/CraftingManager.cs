using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CraftingManager : ConfiguredClass {

        public event System.Action<Recipe> OnSelectRecipe = delegate { };

        private Recipe currentRecipe = null;

        // game manager references
        private PlayerManager playerManager = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
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

        public void RequestBeginCrafting(UnitController sourceUnitController, Recipe recipe, int craftAmount) {
            //Debug.Log($"CraftingManager.RequestBeginCrafting({sourceUnitController.gameObject.name}, {recipe.DisplayName}, {craftAmount})");

            if (systemGameManager.GameMode == GameMode.Local) {
                BeginCrafting(sourceUnitController, recipe, craftAmount);
            } else if (systemGameManager.GameMode == GameMode.Network) {
                networkManagerClient.RequestBeginCrafting(recipe, craftAmount);
            }
        }

        public void BeginCrafting(UnitController sourceUnitController, Recipe recipe, int craftAmount) {
            //Debug.Log($"CraftingManager.BeginCrafting({sourceUnitController.gameObject.name}, {recipe.DisplayName}, {craftAmount})");

            sourceUnitController.CharacterCraftingManager.BeginCrafting(recipe, craftAmount);
        }

        public void RequestCancelCrafting() {
            if (systemGameManager.GameMode == GameMode.Local) {
                CancelCrafting(playerManager.UnitController);
            } else if (systemGameManager.GameMode == GameMode.Network) {
                networkManagerClient.RequestCancelCrafting();
            }
        }

        public void CancelCrafting(UnitController sourceUnitController) {
            sourceUnitController.CharacterCraftingManager.CancelCrafting();
        }

    }

}