using System;
using System.Collections.Generic;

namespace AnyRPG {

    /// <summary>
    /// store and retrieve all scriptable objects from the Resources folders defined in the GameManager
    /// </summary>
    public class SystemDataFactory : ConfiguredMonoBehaviour {

        private Dictionary<Type, FactoryDataAccess> dataDictionary = new Dictionary<Type, FactoryDataAccess>();

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            LoadFactoryData();

            SetupScriptableObjects();
        }

        /// <summary>
        /// create data access objects, load their resources, and store them in the data dictionary
        /// </summary>
        private void LoadFactoryData() {
            SetupFactoryDataAccess<AbilityEffect>();
            SetupFactoryDataAccess<Achievement>();
            SetupFactoryDataAccess<AnimatedAction>();
            SetupFactoryDataAccess<AnimationProfile>();
            SetupFactoryDataAccess<AppearanceEditorProfile>();
            SetupFactoryDataAccess<ArmorClass>();
            SetupFactoryDataAccess<AttachmentProfile>();
            SetupFactoryDataAccess<AudioProfile>();
            SetupFactoryDataAccess<BaseAbility>();
            SetupFactoryDataAccess<BehaviorProfile>();
            SetupFactoryDataAccess<ChatCommand>();
            SetupFactoryDataAccess<CharacterClass>();
            SetupFactoryDataAccess<CharacterRace>();
            SetupFactoryDataAccess<CharacterStat>();
            SetupFactoryDataAccess<ClassSpecialization>();
            SetupFactoryDataAccess<CombatStrategy>();
            SetupFactoryDataAccess<CreditsCategory>();
            SetupFactoryDataAccess<Currency>();
            SetupFactoryDataAccess<CurrencyGroup>();
            SetupFactoryDataAccess<Cutscene>();
            SetupFactoryDataAccess<Dialog>();
            SetupFactoryDataAccess<EnvironmentStateProfile>();
            SetupFactoryDataAccess<EquipmentSet>();
            SetupFactoryDataAccess<EquipmentSlotProfile>();
            SetupFactoryDataAccess<EquipmentSlotType>();
            SetupFactoryDataAccess<Faction>();
            SetupFactoryDataAccess<InteractableOptionConfig>();
            SetupFactoryDataAccess<Item>();
            SetupFactoryDataAccess<ItemQuality>();
            SetupFactoryDataAccess<LootTable>();
            SetupFactoryDataAccess<MaterialProfile>();
            SetupFactoryDataAccess<PatrolProfile>();
            SetupFactoryDataAccess<PowerResource>();
            SetupFactoryDataAccess<PrefabProfile>();
            SetupFactoryDataAccess<Quest>();
            SetupFactoryDataAccess<QuestGiverProfile>();
            SetupFactoryDataAccess<Recipe>();
            SetupFactoryDataAccess<ResourceDescription>();
            SetupFactoryDataAccess<SceneNode>();
            SetupFactoryDataAccess<Skill>();
            SetupFactoryDataAccess<StatusEffectGroup>();
            SetupFactoryDataAccess<StatusEffectType>();
            SetupFactoryDataAccess<SwappableMeshModelProfile>();
            SetupFactoryDataAccess<UMARecipeProfile>();
            SetupFactoryDataAccess<UnitPrefabProfile>();
            SetupFactoryDataAccess<UnitProfile>();
            SetupFactoryDataAccess<UnitToughness>();
            SetupFactoryDataAccess<UnitType>();
            SetupFactoryDataAccess<VendorCollection>();
            SetupFactoryDataAccess<VoiceProfile>();
            SetupFactoryDataAccess<WeaponSkill>();
            SetupFactoryDataAccess<WeatherProfile>();
        }
        
        /// <summary>
        /// setup the scriptable objects for each data type in the factory data
        /// </summary>
        private void SetupScriptableObjects() {
            foreach (FactoryDataAccess dataAccess in dataDictionary.Values) {
                dataAccess.SetupScriptableObjects(systemGameManager);
            }
        }

        /// <summary>
        /// create a factory data access object and add it to the data dictionary
        /// </summary>
        /// <typeparam name="TDataType"></typeparam>
        private void SetupFactoryDataAccess<TDataType>() where TDataType : ResourceProfile {
            FactoryDataAccess factoryDataAccess = new FactoryDataAccess();
            factoryDataAccess.Setup<TDataType>(systemGameManager);
            dataDictionary.Add(typeof(TDataType), factoryDataAccess);
        }

        /// <summary>
        /// return the requested resource of the provided class from the data factory
        /// </summary>
        /// <typeparam name="TDataType"></typeparam>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        public TDataType GetResource<TDataType>(string resourceName) where TDataType : ResourceProfile {
            if (!SystemDataUtility.RequestIsEmpty(resourceName)) {
                if (dataDictionary.ContainsKey(typeof(TDataType))) {
                    return dataDictionary[typeof(TDataType)].GetResource<TDataType>(resourceName);
                }
            }
            return default(TDataType);
        }

        /// <summary>
        /// return all resources of the provided class in the data factory
        /// </summary>
        /// <typeparam name="TDataType"></typeparam>
        /// <returns></returns>
        public List<TDataType> GetResourceList<TDataType>() where TDataType : ResourceProfile {
            if (dataDictionary.ContainsKey(typeof(TDataType))) {
                return dataDictionary[typeof(TDataType)].GetResourceList<TDataType>();
            }
            return new List<TDataType>();
        }

        /// <summary>
        /// return the number of resources of the provided class in the data factory
        /// </summary>
        /// <typeparam name="TDataType"></typeparam>
        /// <returns></returns>
        public int GetResourceCount<TDataType>() where TDataType : ResourceProfile {
            if (dataDictionary.ContainsKey(typeof(TDataType))) {
                return dataDictionary[typeof(TDataType)].GetResourceCount();
            }
            return 0;
        }

    }

}