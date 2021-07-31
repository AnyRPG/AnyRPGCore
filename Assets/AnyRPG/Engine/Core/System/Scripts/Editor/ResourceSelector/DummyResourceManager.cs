using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class DummyResourceManager : SystemResourceManager
    {
        public string resourceClassName;
        System.Type type;

        public DummyResourceManager(System.Type resourceType) {
            this.type = resourceType;
            resourceClassName = resourceType.Name;
        }

        public override void LoadResourceList() {
            masterList.Add(Resources.LoadAll(resourceClassName, type));
            base.LoadResourceList();
        }

        public List<ResourceProfile> GetResourceList() {
            List<ResourceProfile> returnList = new List<ResourceProfile>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as ResourceProfile);
            }
            return returnList;
        }

    }
}
