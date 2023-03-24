using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class DummyResourceManager
    {
        public string resourceClassName;
        private List<ResourceProfile> allResources = new List<ResourceProfile>();

        private System.Type type;

        private List<string> validFolders = new List<string> {
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
            typeof(EquipmentModelProfile).Name,
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

        public void LoadResourceList() {
            allResources.Clear();

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
        }

        private void GenericLoadList<T>(string folder) where T: ResourceProfile {

            // add the scriptable objects stored in the root of all Resources folders to the resource list
            allResources.AddRange(Resources.LoadAll<T>(folder));

            // find the system game manager to load specific resource subfolders for the current game (based on the open scene)
            SystemConfigurationManager systemConfigurationManager = GameObject.FindObjectOfType<SystemConfigurationManager>();
            if (systemConfigurationManager == null) {
                SceneConfig sceneConfig = GameObject.FindObjectOfType<SceneConfig>();
                if (sceneConfig != null) {
                    systemConfigurationManager = sceneConfig.systemConfigurationManager;
                }
            }
            if (systemConfigurationManager != null) {
                // add the scriptable objects stored specific subfolders of the Resources folders to the master list
                foreach (string resourceFolderName in systemConfigurationManager.LoadResourcesFolders) {
                    allResources.AddRange(Resources.LoadAll<T>(resourceFolderName + "/" + folder));
                }
            }
        }

        public List<ResourceProfile> GetResourceList() {
            return allResources;
        }

    }
}
