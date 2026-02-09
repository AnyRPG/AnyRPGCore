using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public abstract class InstantiatedCastableItem : InstantiatedItem, IUseable {

        private CastableItem castableItem = null;

        // game manager references
        protected SystemAbilityController systemAbilityController = null;

        public InstantiatedCastableItem(SystemGameManager systemGameManager, long instanceId, CastableItem castableItem, ItemQuality itemQuality) : base(systemGameManager, instanceId, castableItem, itemQuality) {
            this.castableItem = castableItem;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            systemAbilityController = systemGameManager.SystemAbilityController;
        }

        public override bool Use(UnitController sourceUnitController) {
            //Debug.Log($"{ResourceName}.InstantiatedCastableItem.Use()");

            if (castableItem.Ability == null) {
                Debug.LogError(ResourceName + ".CastableItem.Use(): ability is null.  Please set it in the inspector!");
                return false;
            }
            bool returnValue = base.Use(sourceUnitController);
            if (returnValue == false) {
                return false;
            }
            if (sourceUnitController.CharacterAbilityManager.BeginAbility(castableItem.Ability)) {
                Remove();
            }
            return returnValue;
        }

        public override Coroutine ChooseMonitorCoroutine(ActionButton actionButton) {
            //Debug.Log(DisplayName + ".CastableItem.ChooseMonitorCoroutine()");
            if (castableItem.Ability == null) {
                return null;
            }
            return systemAbilityController.StartCoroutine(actionButton.MonitorAbility(castableItem.Ability.DisplayName));
        }

        public override string GetDescription() {
            //Debug.Log($"{item.ResourceName}.InstantiatedCurrencyItem.GetDescription()");

            return base.GetDescription() + castableItem.GetCastableInformation();
        }


    }

}