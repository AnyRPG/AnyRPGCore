using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemItemManager : ConfiguredMonoBehaviour {

        // game manager references
        SystemDataFactory systemDataFactory = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            systemDataFactory = systemGameManager.SystemDataFactory;
        }

        /// <summary>
        /// Get a new copy of an item based on the factory template.  This is necessary so items can be deleted without deleting the entire item from the database
        /// </summary>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        public Item GetNewResource(string resourceName, ItemQuality usedItemQuality = null) {
            //Debug.Log(this.GetType().Name + ".GetNewResource(" + resourceName + ")");
            if (!SystemDataFactory.RequestIsEmpty(resourceName)) {
                string keyName = SystemDataFactory.PrepareStringForMatch(resourceName);
                Item itemTemplate = systemDataFactory.GetResource<Item>(keyName);
                if (itemTemplate != null) {
                    Item returnValue = ScriptableObject.Instantiate(itemTemplate) as Item;
                    returnValue.SetupScriptableObjects();
                    returnValue.InitializeNewItem(usedItemQuality);
                    return returnValue;
                }
            }
            return null;
        }


    }

}