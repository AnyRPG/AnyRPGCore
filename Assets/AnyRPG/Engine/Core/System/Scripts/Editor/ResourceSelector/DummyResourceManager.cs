using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class DummyResourceManager : FactoryResource
    {
        public string resourceClassName;
        bool includeCoreContent;

        System.Type type;

        public DummyResourceManager(System.Type resourceType, bool includeCoreContent) {
            this.type = resourceType;
            this.includeCoreContent = includeCoreContent;
            resourceClassName = resourceType.Name;
        }

        public override void LoadResourceList() {
            masterList.Add(Resources.LoadAll<ResourceProfile>(resourceClassName));
            if (includeCoreContent) {
                masterList.Add(Resources.LoadAll<ResourceProfile>("CoreContent/"+resourceClassName));
            }
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
