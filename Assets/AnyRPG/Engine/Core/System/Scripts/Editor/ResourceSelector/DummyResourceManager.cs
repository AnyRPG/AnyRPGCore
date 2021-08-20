using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class DummyResourceManager : FactoryResource
    {
        public string resourceClassName;
        //bool includeCoreContent;

        System.Type type;

        public DummyResourceManager(System.Type resourceType) {
        //public DummyResourceManager(System.Type resourceType, bool includeCoreContent) {
            this.type = resourceType;
            //this.includeCoreContent = includeCoreContent;
            resourceClassName = resourceType.Name;
        }

        public override void LoadResourceList() {
            List<string> mappedClassNames = new List<string>();
            if (resourceClassName == "Equipment") {
                mappedClassNames.Add("Item/Accessory");
                mappedClassNames.Add("Item/Armor");
                mappedClassNames.Add("Item/Weapon");
            } else {
                mappedClassNames.Add(resourceClassName);
            }
            foreach (string className in mappedClassNames) {
                masterList.Add(Resources.LoadAll<ResourceProfile>(className));
                SystemConfigurationManager systemConfigurationManager = GameObject.FindObjectOfType<SystemConfigurationManager>();
                if (systemConfigurationManager != null) {
                    foreach (string resourceFolderName in systemConfigurationManager.LoadResourcesFolders) {
                        masterList.Add(Resources.LoadAll<ResourceProfile>(resourceFolderName + "/" + className));
                    }
                }
            }
            /*
            if (includeCoreContent) {
                masterList.Add(Resources.LoadAll<ResourceProfile>("CoreContent/"+resourceClassName));
            }
            */
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
