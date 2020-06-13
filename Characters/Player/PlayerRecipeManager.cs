using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class PlayerRecipeManager : CharacterRecipeManager {

        public override void CreateEventSubscriptions() {
            //Debug.Log(gameObject.name + ".PlayerRecipeManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            base.CreateEventSubscriptions();
            SystemEventManager.MyInstance.OnLevelChanged += UpdateRecipeList;

        }

        public override void CleanupEventSubscriptions() {
            //Debug.Log(gameObject.name + ".PlayerRecipeManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            base.CleanupEventSubscriptions();
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.MyInstance.OnLevelChanged -= UpdateRecipeList;
            }
        }

        public override void OnDisable() {
            //Debug.Log(gameObject.name + ".PlayerRecipeManager.OnDisable()");
            base.OnDisable();
            CleanupEventSubscriptions();
        }

        public override void UpdateRecipeList(int newLevel) {
            //Debug.Log(gameObject.name + ".PlayerRecipemanager.UpdateRecipeList()");
            base.UpdateRecipeList(newLevel);
        }



    }

}