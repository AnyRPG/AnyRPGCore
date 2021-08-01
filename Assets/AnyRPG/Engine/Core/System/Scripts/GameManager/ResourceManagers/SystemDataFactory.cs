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

        private Dictionary<string, FactoryDataAccess> dataDictionary = new Dictionary<string, FactoryDataAccess>();

        public Dictionary<string, FactoryDataAccess> DataDictionary { get => dataDictionary; set => dataDictionary = value; }

        public void SetupFactory() {
            /*
            FactoryData<BaseAbility> abilityFactory = new FactoryData<BaseAbility>("BaseAbility");
            abilityFactory.LoadResourceList();
            */
            FactoryDataAccess factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<BaseAbility>("BaseAbility");
            dataDictionary.Add("BaseAbility", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<Skill>("Skill");
            dataDictionary.Add("Skill", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<Recipe>("Recipe");
            dataDictionary.Add("Recipe", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<Faction>("Faction");
            dataDictionary.Add("Faction", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<Item>("Item");
            dataDictionary.Add("Item", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<Currency>("Currency");
            dataDictionary.Add("Currency", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<AbilityEffect>("AbilityEffect");
            dataDictionary.Add("AbilityEffect", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<Quest>("Quest");
            dataDictionary.Add("Quest", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<Dialog>("Dialog");
            dataDictionary.Add("Dialog", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<SceneNode>("SceneNode");
            dataDictionary.Add("SceneNode", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<PrefabProfile>("PrefabProfile");
            dataDictionary.Add("PrefabProfile", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<EquipmentProfile>("EquipmentProfile");
            dataDictionary.Add("EquipmentProfile", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<UnitProfile>("UnitProfile");
            dataDictionary.Add("UnitProfile", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<AudioProfile>("AudioProfile");
            dataDictionary.Add("AudioProfile", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<CombatStrategy>("CombatStrategy");
            dataDictionary.Add("CombatStrategy", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<ItemQuality>("ItemQuality");
            dataDictionary.Add("ItemQuality", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<VendorCollection>("VendorCollection");
            dataDictionary.Add("VendorCollection", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<CurrencyGroup>("CurrencyGroup");
            dataDictionary.Add("CurrencyGroup", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<CharacterClass>("CharacterClass");
            dataDictionary.Add("CharacterClass", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<ArmorClass>("ArmorClass");
            dataDictionary.Add("ArmorClass", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<WeaponSkill>("WeaponSkill");
            dataDictionary.Add("WeaponSkill", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<EquipmentSlotProfile>("EquipmentSlotProfile");
            dataDictionary.Add("EquipmentSlotProfile", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<EquipmentSlotType>("EquipmentSlotType");
            dataDictionary.Add("EquipmentSlotType", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<LootTable>("LootTable");
            dataDictionary.Add("LootTable", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<AnimationProfile>("AnimationProfile");
            dataDictionary.Add("AnimationProfile", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<QuestGiverProfile>("QuestGiverProfile");
            dataDictionary.Add("QuestGiverProfile", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<ResourceDescription>("ResourceDescription");
            dataDictionary.Add("ResourceDescription", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<UMARecipeProfile>("UMARecipeProfile");
            dataDictionary.Add("UMARecipeProfile", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<MaterialProfile>("MaterialProfile");
            dataDictionary.Add("MaterialProfile", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<ClassSpecialization>("ClassSpecialization");
            dataDictionary.Add("ClassSpecialization", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<UnitType>("UnitType");
            dataDictionary.Add("UnitType", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<PatrolProfile>("PatrolProfile");
            dataDictionary.Add("PatrolProfile", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<BehaviorProfile>("BehaviorProfile");
            dataDictionary.Add("BehaviorProfile", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<UnitToughness>("UnitToughness");
            dataDictionary.Add("UnitToughness", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<EnvironmentStateProfile>("EnvironmentStateProfile");
            dataDictionary.Add("EnvironmentStateProfile", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<CreditsCategory>("CreditsCategory");
            dataDictionary.Add("CreditsCategory", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<Cutscene>("Cutscene");
            dataDictionary.Add("Cutscene", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<EquipmentSet>("EquipmentSet");
            dataDictionary.Add("EquipmentSet", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<StatusEffectType>("StatusEffectType");
            dataDictionary.Add("StatusEffectType", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<PowerResource>("PowerResource");
            dataDictionary.Add("PowerResource", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<CharacterStat>("CharacterStat");
            dataDictionary.Add("CharacterStat", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<AttachmentProfile>("AttachmentProfile");
            dataDictionary.Add("AttachmentProfile", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<UnitPrefabProfile>("UnitPrefabProfile");
            dataDictionary.Add("UnitPrefabProfile", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<InteractableOptionConfig>("InteractableOptionConfig");
            dataDictionary.Add("InteractableOptionConfig", factoryDataAccess);

            factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<CharacterRace>("CharacterRace");
            dataDictionary.Add("CharacterRace", factoryDataAccess);

            //setup scriptable objects
            foreach (FactoryDataAccess dataAccess in dataDictionary.Values) {
                dataAccess.SetupScriptableObjects();
            }

        }


    }

}