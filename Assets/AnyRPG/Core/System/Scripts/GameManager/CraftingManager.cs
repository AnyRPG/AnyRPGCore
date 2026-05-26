namespace AnyRPG {
    public class CraftingManager : ConfiguredClass {

        public event System.Action<Recipe> OnSelectRecipe = delegate { };

        private Recipe currentRecipe = null;

        // game manager references
        private PlayerManagerClient playerManagerClient = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManagerClient = systemGameManager.PlayerManagerClient;
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
            //Debug.Log($"CraftingManager.RequestBeginCrafting({sourceUnitController.gameObject.name}, {recipe.ResourceName}, {craftAmount})");

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
                CancelCrafting(playerManagerClient.UnitController);
            } else if (systemGameManager.GameMode == GameMode.Network) {
                networkManagerClient.RequestCancelCrafting();
            }
        }

        public void CancelCrafting(UnitController sourceUnitController) {
            sourceUnitController.CharacterCraftingManager.CancelCrafting();
        }

    }

}