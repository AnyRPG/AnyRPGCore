using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class DummyResourceManager : FactoryResource
    {
        public string resourceClassName;
        List<ResourceProfile> allResources = new List<ResourceProfile>();

        System.Type type;

        List<string> validFolders = new List<string> {
            typeof(AbilityEffect).Name,
            typeof(AnimationProfile).Name,
            typeof(ArmorClass).Name,
            typeof(AudioProfile).Name,
            typeof(BaseAbility).Name,
            typeof(BehaviorProfile).Name,
            typeof(CharacterClass).Name,
            typeof(CharacterRace).Name,
            typeof(ClassSpecialization).Name,
            typeof(CombatStrategy).Name,
            typeof(Currency).Name,
            typeof(CurrencyGroup).Name,
            typeof(Cutscene).Name,
            typeof(Dialog).Name,
            typeof(EnvironmentStateProfile).Name,
            typeof(EquipmentSet).Name,
            typeof(Faction).Name,
            typeof(InteractableOptionConfig).Name,
            typeof(Item).Name,
            typeof(ItemQuality).Name,
            typeof(LootTable).Name,
            typeof(MaterialProfile).Name,
            typeof(PatrolProfile).Name,
            typeof(PowerResource).Name,
            typeof(PrefabProfile).Name,
            typeof(Quest).Name,
            typeof(Recipe).Name,
            typeof(SceneNode).Name,
            typeof(Skill).Name,
            typeof(StatusEffectType).Name,
            typeof(UMARecipeProfile).Name,
            typeof(UnitProfile).Name,
            typeof(UnitToughness).Name,
            typeof(UnitType).Name,
            typeof(VendorCollection).Name,
            typeof(WeaponSkill).Name
        };

        public DummyResourceManager(System.Type resourceType) {
            this.type = resourceType;
            resourceClassName = resourceType.Name;
        }

        public override void LoadResourceList() {
            List<string> mappedClassNames = new List<string>();
            if (resourceClassName == "ResourceProfile") {
                foreach (string folder in validFolders)
                    GenericLoadList<ResourceProfile>(folder);
            } else {
                if (resourceClassName == "Equipment") {
                    mappedClassNames.Add("Item/Equipment");
                    mappedClassNames.Add("Item/Accessory");
                    mappedClassNames.Add("Item/Armor");
                    mappedClassNames.Add("Item/Weapon");
                } else if (resourceClassName == "StatusEffect") {
                    mappedClassNames.Add("AbilityEffect/StatusEffect");
                } else {
                    mappedClassNames.Add(resourceClassName);
                }
                foreach (string className in mappedClassNames) {
                    GenericLoadList<ResourceProfile>(className);
                }
            }
            //base.LoadResourceList();
            // other than the in-game resource managers we only need a list of all the resources
            allResources = new List<ResourceProfile>();
            foreach (ResourceProfile[] subList in masterList) {
                allResources.AddRange(subList);
            }
        }

        void GenericLoadList<T>(string folder) where T: ResourceProfile {
            masterList.Add(Resources.LoadAll<T>(folder));
            SystemConfigurationManager systemConfigurationManager = GameObject.FindObjectOfType<SystemConfigurationManager>();
            if (systemConfigurationManager == null) {
                SceneConfig sceneConfig = GameObject.FindObjectOfType<SceneConfig>();
                if (sceneConfig != null) {
                    systemConfigurationManager = sceneConfig.systemConfigurationManager;
                }
            }
            if (systemConfigurationManager != null) {
                foreach (string resourceFolderName in systemConfigurationManager.LoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<T>(resourceFolderName + "/" + folder));
                }
            }
        }

        public List<ResourceProfile> GetResourceList() {
            return allResources;
            //List<ResourceProfile> returnList = new List<ResourceProfile>();

            //foreach (UnityEngine.Object listItem in resourceList.Values) {
                //returnList.Add(listItem as ResourceProfile);
            //}
            //return returnList;
        }

    }
}
