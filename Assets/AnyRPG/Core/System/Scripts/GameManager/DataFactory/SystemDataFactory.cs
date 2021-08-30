using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    /// <summary>
    /// allow us to query scriptable objects for equivalence by storing a template ID on all instantiated objects
    /// </summary>
    public class SystemDataFactory : ConfiguredMonoBehaviour {

        private Dictionary<Type, FactoryDataAccess> dataDictionary = new Dictionary<Type, FactoryDataAccess>();

        public override void Configure(SystemGameManager systemGameManager) {
            //Debug.Log("SystemDataFactory.Configure(" + (systemGameManager == null ? "null" : systemGameManager.gameObject.name) + ")");

            base.Configure(systemGameManager);

            SetupFactory();
        }

        public void SetupFactory() {
            FactoryDataAccess factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<BaseAbility>("BaseAbility", systemGameManager);
            dataDictionary.Add(typeof(BaseAbility), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<Skill>("Skill", systemGameManager);
            dataDictionary.Add(typeof(Skill), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<Recipe>("Recipe", systemGameManager);
            dataDictionary.Add(typeof(Recipe), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<Faction>("Faction", systemGameManager);
            dataDictionary.Add(typeof(Faction), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<Item>("Item", systemGameManager);
            dataDictionary.Add(typeof(Item), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<Currency>("Currency", systemGameManager);
            dataDictionary.Add(typeof(Currency), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<AbilityEffect>("AbilityEffect", systemGameManager);
            dataDictionary.Add(typeof(AbilityEffect), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<Quest>("Quest", systemGameManager);
            dataDictionary.Add(typeof(Quest), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<Dialog>("Dialog", systemGameManager);
            dataDictionary.Add(typeof(Dialog), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<SceneNode>("SceneNode", systemGameManager);
            dataDictionary.Add(typeof(SceneNode), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<PrefabProfile>("PrefabProfile", systemGameManager);
            dataDictionary.Add(typeof(PrefabProfile), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<UnitProfile>("UnitProfile", systemGameManager);
            dataDictionary.Add(typeof(UnitProfile), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<AudioProfile>("AudioProfile", systemGameManager);
            dataDictionary.Add(typeof(AudioProfile), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<CombatStrategy>("CombatStrategy", systemGameManager);
            dataDictionary.Add(typeof(CombatStrategy), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<ItemQuality>("ItemQuality", systemGameManager);
            dataDictionary.Add(typeof(ItemQuality), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<VendorCollection>("VendorCollection", systemGameManager);
            dataDictionary.Add(typeof(VendorCollection), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<CurrencyGroup>("CurrencyGroup", systemGameManager);
            dataDictionary.Add(typeof(CurrencyGroup), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<CharacterClass>("CharacterClass", systemGameManager);
            dataDictionary.Add(typeof(CharacterClass), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<ArmorClass>("ArmorClass", systemGameManager);
            dataDictionary.Add(typeof(ArmorClass), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<WeaponSkill>("WeaponSkill", systemGameManager);
            dataDictionary.Add(typeof(WeaponSkill), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<EquipmentSlotProfile>("EquipmentSlotProfile", systemGameManager);
            dataDictionary.Add(typeof(EquipmentSlotProfile), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<EquipmentSlotType>("EquipmentSlotType", systemGameManager);
            dataDictionary.Add(typeof(EquipmentSlotType), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<LootTable>("LootTable", systemGameManager);
            dataDictionary.Add(typeof(LootTable), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<AnimationProfile>("AnimationProfile", systemGameManager);
            dataDictionary.Add(typeof(AnimationProfile), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<QuestGiverProfile>("QuestGiverProfile", systemGameManager);
            dataDictionary.Add(typeof(QuestGiverProfile), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<ResourceDescription>("ResourceDescription", systemGameManager);
            dataDictionary.Add(typeof(ResourceDescription), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<UMARecipeProfile>("UMARecipeProfile", systemGameManager);
            dataDictionary.Add(typeof(UMARecipeProfile), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<MaterialProfile>("MaterialProfile", systemGameManager);
            dataDictionary.Add(typeof(MaterialProfile), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<ClassSpecialization>("ClassSpecialization", systemGameManager);
            dataDictionary.Add(typeof(ClassSpecialization), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<UnitType>("UnitType", systemGameManager);
            dataDictionary.Add(typeof(UnitType), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<PatrolProfile>("PatrolProfile", systemGameManager);
            dataDictionary.Add(typeof(PatrolProfile), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<BehaviorProfile>("BehaviorProfile", systemGameManager);
            dataDictionary.Add(typeof(BehaviorProfile), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<UnitToughness>("UnitToughness", systemGameManager);
            dataDictionary.Add(typeof(UnitToughness), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<EnvironmentStateProfile>("EnvironmentStateProfile", systemGameManager);
            dataDictionary.Add(typeof(EnvironmentStateProfile), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<CreditsCategory>("CreditsCategory", systemGameManager);
            dataDictionary.Add(typeof(CreditsCategory), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<Cutscene>("Cutscene", systemGameManager);
            dataDictionary.Add(typeof(Cutscene), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<EquipmentSet>("EquipmentSet", systemGameManager);
            dataDictionary.Add(typeof(EquipmentSet), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<StatusEffectType>("StatusEffectType", systemGameManager);
            dataDictionary.Add(typeof(StatusEffectType), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<PowerResource>("PowerResource", systemGameManager);
            dataDictionary.Add(typeof(PowerResource), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<CharacterStat>("CharacterStat", systemGameManager);
            dataDictionary.Add(typeof(CharacterStat), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<AttachmentProfile>("AttachmentProfile", systemGameManager);
            dataDictionary.Add(typeof(AttachmentProfile), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<UnitPrefabProfile>("UnitPrefabProfile", systemGameManager);
            dataDictionary.Add(typeof(UnitPrefabProfile), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<InteractableOptionConfig>("InteractableOptionConfig", systemGameManager);
            dataDictionary.Add(typeof(InteractableOptionConfig), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<CharacterRace>("CharacterRace", systemGameManager);
            dataDictionary.Add(typeof(CharacterRace), factoryDataAccess);

            //setup scriptable objects
            foreach (FactoryDataAccess dataAccess in dataDictionary.Values) {
                dataAccess.SetupScriptableObjects(systemGameManager);
            }
        }

        public TDataType GetResource<TDataType>(string resourceName) where TDataType : ResourceProfile {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                //string keyName = SystemDataFactory.PrepareStringForMatch(resourceName);
                if (dataDictionary.ContainsKey(typeof(TDataType))) {
                    return dataDictionary[typeof(TDataType)].GetResource<TDataType>(resourceName);
                }
            }
            return default(TDataType);
        }

        public List<TDataType> GetResourceList<TDataType>() where TDataType : ResourceProfile {
            if (dataDictionary.ContainsKey(typeof(TDataType))) {
                return dataDictionary[typeof(TDataType)].GetResourceList<TDataType>();
            }
            return new List<TDataType>();
        }

        public int GetResourceCount<TDataType>() where TDataType : ResourceProfile {
            if (dataDictionary.ContainsKey(typeof(TDataType))) {
                return dataDictionary[typeof(TDataType)].GetResourceCount();
            }
            return 0;
        }

        public static string PrepareStringForMatch(string oldString) {
            return oldString.ToLower().Replace(" ", string.Empty).Replace("'", string.Empty);
        }

        public static bool MatchResource(string resourceName, string resourceMatchName) {
            if (resourceName != null && resourceMatchName != null) {
                if (PrepareStringForMatch(resourceName) == PrepareStringForMatch(resourceMatchName)) {
                    return true;
                }
            } else {
                //Debug.Log("SystemGameManager.MatchResource(" + (resourceName == null ? "null" : resourceName) + ", " + (resourceMatchName == null ? "null" : resourceMatchName) + ")");
            }
            return false;
        }

        public static bool RequestIsEmpty(string resourceName) {
            if (resourceName == null || resourceName == string.Empty) {
                //Debug.Log("SystemDataFactory.RequestIsEmpty(" + resourceName + "): EMPTY RESOURCE REQUESTED.  FIX THIS! DO NOT COMMENT THIS LINE");
                return true;
            }
            return false;
        }


    }

}