using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    /// <summary>
    /// allow us to query scriptable objects for equivalence by storing a template ID on all instantiated objects
    /// </summary>
    public class SystemDataFactory : MonoBehaviour {

        #region Singleton
        private static SystemDataFactory instance;

        public static SystemDataFactory Instance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemDataFactory>();
                }

                return instance;
            }
        }
        #endregion

        private Dictionary<Type, FactoryDataAccess> dataDictionary = new Dictionary<Type, FactoryDataAccess>();

        public void SetupFactory() {
            FactoryDataAccess factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<BaseAbility>("BaseAbility");
            dataDictionary.Add(typeof(BaseAbility), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<Skill>("Skill");
            dataDictionary.Add(typeof(Skill), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<Recipe>("Recipe");
            dataDictionary.Add(typeof(Recipe), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<Faction>("Faction");
            dataDictionary.Add(typeof(Faction), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<Item>("Item");
            dataDictionary.Add(typeof(Item), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<Currency>("Currency");
            dataDictionary.Add(typeof(Currency), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<AbilityEffect>("AbilityEffect");
            dataDictionary.Add(typeof(AbilityEffect), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<Quest>("Quest");
            dataDictionary.Add(typeof(Quest), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<Dialog>("Dialog");
            dataDictionary.Add(typeof(Dialog), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<SceneNode>("SceneNode");
            dataDictionary.Add(typeof(SceneNode), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<PrefabProfile>("PrefabProfile");
            dataDictionary.Add(typeof(PrefabProfile), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<UnitProfile>("UnitProfile");
            dataDictionary.Add(typeof(UnitProfile), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<AudioProfile>("AudioProfile");
            dataDictionary.Add(typeof(AudioProfile), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<CombatStrategy>("CombatStrategy");
            dataDictionary.Add(typeof(CombatStrategy), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<ItemQuality>("ItemQuality");
            dataDictionary.Add(typeof(ItemQuality), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<VendorCollection>("VendorCollection");
            dataDictionary.Add(typeof(VendorCollection), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<CurrencyGroup>("CurrencyGroup");
            dataDictionary.Add(typeof(CurrencyGroup), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<CharacterClass>("CharacterClass");
            dataDictionary.Add(typeof(CharacterClass), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<ArmorClass>("ArmorClass");
            dataDictionary.Add(typeof(ArmorClass), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<WeaponSkill>("WeaponSkill");
            dataDictionary.Add(typeof(WeaponSkill), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<EquipmentSlotProfile>("EquipmentSlotProfile");
            dataDictionary.Add(typeof(EquipmentSlotProfile), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<EquipmentSlotType>("EquipmentSlotType");
            dataDictionary.Add(typeof(EquipmentSlotType), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<LootTable>("LootTable");
            dataDictionary.Add(typeof(LootTable), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<AnimationProfile>("AnimationProfile");
            dataDictionary.Add(typeof(AnimationProfile), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<QuestGiverProfile>("QuestGiverProfile");
            dataDictionary.Add(typeof(QuestGiverProfile), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<ResourceDescription>("ResourceDescription");
            dataDictionary.Add(typeof(ResourceDescription), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<UMARecipeProfile>("UMARecipeProfile");
            dataDictionary.Add(typeof(UMARecipeProfile), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<MaterialProfile>("MaterialProfile");
            dataDictionary.Add(typeof(MaterialProfile), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<ClassSpecialization>("ClassSpecialization");
            dataDictionary.Add(typeof(ClassSpecialization), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<UnitType>("UnitType");
            dataDictionary.Add(typeof(UnitType), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<PatrolProfile>("PatrolProfile");
            dataDictionary.Add(typeof(PatrolProfile), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<BehaviorProfile>("BehaviorProfile");
            dataDictionary.Add(typeof(BehaviorProfile), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<UnitToughness>("UnitToughness");
            dataDictionary.Add(typeof(UnitToughness), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<EnvironmentStateProfile>("EnvironmentStateProfile");
            dataDictionary.Add(typeof(EnvironmentStateProfile), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<CreditsCategory>("CreditsCategory");
            dataDictionary.Add(typeof(CreditsCategory), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<Cutscene>("Cutscene");
            dataDictionary.Add(typeof(Cutscene), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<EquipmentSet>("EquipmentSet");
            dataDictionary.Add(typeof(EquipmentSet), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<StatusEffectType>("StatusEffectType");
            dataDictionary.Add(typeof(StatusEffectType), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<PowerResource>("PowerResource");
            dataDictionary.Add(typeof(PowerResource), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<CharacterStat>("CharacterStat");
            dataDictionary.Add(typeof(CharacterStat), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<AttachmentProfile>("AttachmentProfile");
            dataDictionary.Add(typeof(AttachmentProfile), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<UnitPrefabProfile>("UnitPrefabProfile");
            dataDictionary.Add(typeof(UnitPrefabProfile), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<InteractableOptionConfig>("InteractableOptionConfig");
            dataDictionary.Add(typeof(InteractableOptionConfig), factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<CharacterRace>("CharacterRace");
            dataDictionary.Add(typeof(CharacterRace), factoryDataAccess);

            //setup scriptable objects
            foreach (FactoryDataAccess dataAccess in dataDictionary.Values) {
                dataAccess.SetupScriptableObjects();
            }
        }

        public TDataType GetResource<TDataType>(string resourceName) where TDataType : ResourceProfile {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!SystemResourceManager.RequestIsEmpty(resourceName)) {
                //string keyName = SystemResourceManager.prepareStringForMatch(resourceName);
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


    }

}