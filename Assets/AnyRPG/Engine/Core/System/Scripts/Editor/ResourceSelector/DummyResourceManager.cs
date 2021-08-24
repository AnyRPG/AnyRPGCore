using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class DummyResourceManager : FactoryResource
    {
        public string resourceClassName;

        System.Type type;

        public DummyResourceManager(System.Type resourceType) {
            this.type = resourceType;
            resourceClassName = resourceType.Name;
        }

        public override void LoadResourceList() {
            List<string> mappedClassNames = new List<string>();
            if (resourceClassName == "ResourceProfile") {
                GenericLoadList<ResourceProfile>("");
            } else {
                if (resourceClassName == "Equipment") {
                    mappedClassNames.Add("Item/Accessory");
                    mappedClassNames.Add("Item/Armor");
                    mappedClassNames.Add("Item/Weapon");
                } else {
                    mappedClassNames.Add(resourceClassName);
                }
                foreach (string className in mappedClassNames) {
                    GenericLoadList<ResourceProfile>(className);
                }
            }
            base.LoadResourceList();
        }

        void GenericLoadList<T>(string folder) where T: ResourceProfile {
            masterList.Add(Resources.LoadAll<T>(folder));
            SystemConfigurationManager systemConfigurationManager = GameObject.FindObjectOfType<SystemConfigurationManager>();
            if (systemConfigurationManager != null) {
                foreach (string resourceFolderName in systemConfigurationManager.LoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<T>(resourceFolderName + "/" + folder));
                }
            }
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
