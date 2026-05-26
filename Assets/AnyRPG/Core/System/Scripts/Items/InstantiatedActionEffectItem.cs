namespace AnyRPG {
    public class InstantiatedActionEffectItem : InstantiatedActionItem {

        private ActionEffectItem actionEffectItem = null;

        public InstantiatedActionEffectItem(SystemGameManager systemGameManager, long instanceId, ActionEffectItem actionEffectItem, ItemQuality itemQuality) : base(systemGameManager, instanceId, actionEffectItem, itemQuality) {
            this.actionEffectItem = actionEffectItem;
        }

        public override bool Use(UnitController sourceUnitController) {
            //Debug.Log(DisplayName + ".ActionEffectItem.Use()");

            bool returnValue = base.Use(sourceUnitController);
            if (returnValue == false) {
                return false;
            }

            // perform heal effect
            actionEffectItem.AbilityEffectProperties.Cast(sourceUnitController, sourceUnitController, null, null);

            return returnValue;

        }

        public override bool IsUseable() {
            return true;
        }

    }

}