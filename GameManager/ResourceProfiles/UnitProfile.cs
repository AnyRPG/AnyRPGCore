using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Unit Profile", menuName = "AnyRPG/UnitProfile")]
    public class UnitProfile : DescribableResource, IStatProvider {

        [Header("Unit")]

        [Tooltip("If true, the unit prefab is loaded by searching for a UnitPrefabProfile with the same name as this resource.")]
        [SerializeField]
        private bool automaticPrefabProfile = true;

        [Tooltip("The name of the prefab profile that contains the gameObject that represents the unit")]
        [SerializeField]
        private string prefabProfileName = string.Empty;

        private UnitPrefabProfile unitPrefabProfile = null;

        // The physical game object to spawn for this unit
        private GameObject unitPrefab = null;

        // the physical game object to spawn for the model, if any (units can already have models if configured that way)
        private GameObject modelPrefab = null;

        [Tooltip("Mark this if true is the unit is an UMA unit")]
        [SerializeField]
        private bool isUMAUnit = false;

        [Tooltip("If true, this unit can be charmed and made into a pet")]
        [SerializeField]
        private bool isPet = false;

        [Tooltip("If this is set, when the unit spawns, it will use this toughness")]
        [SerializeField]
        private string defaultToughness = string.Empty;

        protected UnitToughness unitToughness = null;

        [Header("Character")]

        [Tooltip("The name that will show over the character head and in unit frames")]
        [SerializeField]
        protected string characterName;

        [Tooltip("The name of the faction this character belongs to")]
        [SerializeField]
        protected string factionName;

        protected Faction faction;

        [Tooltip("If set, this will show in the nameplate instead of the faction")]
        [SerializeField]
        protected string title = string.Empty;

        [Tooltip("The name of the character class")]
        [SerializeField]
        protected string characterClassName;

        protected CharacterClass characterClass;

        [Tooltip("The name of the unit type")]
        [SerializeField]
        protected string unitTypeName;

        protected UnitType unitType;

        [Tooltip("The name of the class specialization")]
        [SerializeField]
        protected string classSpecializationName;

        protected ClassSpecialization classSpecialization;

        [Tooltip("should this character start the game dead")]
        [SerializeField]
        protected bool spawnDead = false;

        [Tooltip("If true, the character will not despawn when dead")]
        [SerializeField]
        private bool preventAutoDespawn = false;

        [Header("Abilities")]

        [Tooltip("When no weapons are equippped to learn auto-attack abilities from, this auto-attack ability will be used")]
        [SerializeField]
        private string defaultAutoAttackAbilityName = string.Empty;

        private BaseAbility defaultAutoAttackAbility = null;

        [Tooltip("Abilities this unit will know")]
        [SerializeField]
        private List<string> learnedAbilityNames = new List<string>();

        private List<BaseAbility> learnedAbilities = new List<BaseAbility>();

        [Header("Capabilities")]

        [Tooltip("Weapon skills known by this class")]
        [FormerlySerializedAs("weaponSkillList")]
        [SerializeField]
        private List<string> weaponSkills = new List<string>();

        // reference to the actual weapon skills
        private List<WeaponSkill> weaponSkillList = new List<WeaponSkill>();

        [Header("Control")]

        [Tooltip("The radius of the aggro sphere around the unit.  If any hostile characters enter this range, the character will attack them.")]
        [FormerlySerializedAs("aggroRange")]
        [SerializeField]
        private float aggroRadius = 20f;

        [Header("Combat")]

        [Tooltip("If true, a combat strategy matching the unit name will be looked up and used if found")]
        [SerializeField]
        private bool automaticCombatStrategy = false;

        [Tooltip("The strategy that will be used when this unit is in combat")]
        [SerializeField]
        private string combatStrategyName = string.Empty;

        // reference to the actual combat strategy
        private CombatStrategy combatStrategy;

        [Header("Stats and Scaling")]

        [Tooltip("Stats available to this unit, in addition to the stats defined at the system level that all character use")]
        [FormerlySerializedAs("statScaling")]
        [SerializeField]
        private List<StatScalingNode> primaryStats = new List<StatScalingNode>();

        [Header("Power Resources")]

        [Tooltip("Power Resources used by this unit.  The first resource is considered primary and will show on the unit frame.")]
        [SerializeField]
        private List<string> powerResources = new List<string>();

        // reference to the actual power resources
        private List<PowerResource> powerResourceList = new List<PowerResource>();

        [Header("Equipment")]

        [Tooltip("This equipment will be equipped by default on this unit")]
        [SerializeField]
        private List<string> equipmentNameList = new List<string>();

        [Header("Movement")]

        [Tooltip("If true, the movement sounds are played on footstep hit instead of in a continuous track.")]
        [SerializeField]
        private bool playOnFootstep = false;

        [Tooltip("These profiles will be played when the unit is in motion.  If footsteps are used, the next sound on the list will be played on every footstep.")]
        [SerializeField]
        private List<string> movementAudioProfileNames = new List<string>();

        private List<AudioProfile> movementAudioProfiles = new List<AudioProfile>();

        [Header("Patrol")]

        [Tooltip("Inline patrol configuration.  Useful if no other unit will need to re-use this configuration.")]
        [SerializeField]
        private PatrolProps patrolConfig = new PatrolProps();

        [Tooltip("Lookup and use these named patrols that can be shared among units")]
        [SerializeField]
        private List<string> patrolNames = new List<string>();

        [Header("Behavior")]

        [Tooltip("Inline behavior configuration.  Useful if no other unit will need to re-use this configuration.")]
        [SerializeField]
        private BehaviorProps behaviorConfig = new BehaviorProps();

        [Header("Builtin Interactables")]

        [Tooltip("Inline loot configuration.  Useful if no other unit will need to re-use this configuration.")]
        [SerializeField]
        private LootableCharacterProps lootableCharacter = new LootableCharacterProps();

        [Tooltip("Inline dialog configuration.  Useful if no other unit will need to re-use this configuration.")]
        [SerializeField]
        private DialogProps dialogConfig = new DialogProps();

        [Tooltip("Inline questGiver configuration.  Useful if no other unit will need to re-use this configuration.")]
        [SerializeField]
        private QuestGiverProps questGiverConfig = new QuestGiverProps();

        [Tooltip("Inline vendor configuration.  Useful if no other unit will need to re-use this configuration.")]
        [SerializeField]
        private VendorProps vendorConfig = new VendorProps();

        [Header("Named Interactables")]

        [Tooltip("The names of the interactable options available on this character")]
        [SerializeField]
        private List<string> interactableOptions = new List<string>();


        public GameObject UnitPrefab { get => unitPrefab; set => unitPrefab = value; }
        public UnitToughness DefaultToughness { get => unitToughness; set => unitToughness = value; }
        public BaseAbility DefaultAutoAttackAbility { get => defaultAutoAttackAbility; set => defaultAutoAttackAbility = value; }
        public bool IsUMAUnit { get => isUMAUnit; set => isUMAUnit = value; }
        public bool IsPet { get => isPet; set => isPet = value; }
        public List<BaseAbility> LearnedAbilities { get => learnedAbilities; set => learnedAbilities = value; }
        public bool PlayOnFootstep { get => playOnFootstep; set => playOnFootstep = value; }
        public List<AudioProfile> MovementAudioProfiles { get => movementAudioProfiles; set => movementAudioProfiles = value; }
        public List<WeaponSkill> WeaponSkillList { get => weaponSkillList; set => weaponSkillList = value; }
        public List<StatScalingNode> PrimaryStats { get => primaryStats; set => primaryStats = value; }
        public List<PowerResource> PowerResourceList { get => powerResourceList; set => powerResourceList = value; }
        public List<string> EquipmentNameList { get => equipmentNameList; set => equipmentNameList = value; }
        public CombatStrategy CombatStrategy { get => combatStrategy; set => combatStrategy = value; }
        public string CharacterName { get => characterName; set => characterName = value; }
        public string Title { get => title; set => title = value; }
        public CharacterClass CharacterClass { get => characterClass; set => characterClass = value; }
        public Faction Faction { get => faction; set => faction = value; }
        public UnitType UnitType { get => unitType; set => unitType = value; }
        public bool SpawnDead { get => spawnDead; set => spawnDead = value; }
        public ClassSpecialization ClassSpecialization { get => classSpecialization; set => classSpecialization = value; }
        public bool PreventAutoDespawn { get => preventAutoDespawn; set => preventAutoDespawn = value; }
        public List<string> PatrolNames { get => patrolNames; set => patrolNames = value; }
        public float AggroRadius { get => aggroRadius; set => aggroRadius = value; }
        public UnitPrefabProfile UnitPrefabProfile { get => unitPrefabProfile; set => unitPrefabProfile = value; }
        public LootableCharacterProps LootableCharacterProps { get => lootableCharacter; set => lootableCharacter = value; }
        public BehaviorProps BehaviorProps { get => behaviorConfig; set => behaviorConfig = value; }
        public DialogProps DialogProps { get => dialogConfig; set => dialogConfig = value; }
        public QuestGiverProps QuestGiverProps { get => questGiverConfig; set => questGiverConfig = value; }
        public VendorProps VendorProps { get => vendorConfig; set => vendorConfig = value; }

        /// <summary>
        /// spawn unit with parent. rotation and position from settings
        /// </summary>
        /// <param name="parentTransform"></param>
        /// <param name="settingsTransform"></param>
        /// <returns></returns>
        public UnitController SpawnUnitPrefab(Transform parentTransform, Vector3 position, Vector3 forward) {
            GameObject prefabObject = SpawnPrefab(unitPrefab, parentTransform, position, forward);
            UnitController unitController = null;
            if (prefabObject != null) {
                unitController = prefabObject.GetComponent<UnitController>();
                if (unitController != null) {
                    unitController.SetUnitProfile(this);
                }
            }

            return unitController;
        }

        /// <summary>
        /// spawn unit with parent. rotation and position from settings
        /// </summary>
        /// <param name="parentTransform"></param>
        /// <param name="settingsTransform"></param>
        /// <returns></returns>
        public GameObject SpawnModelPrefab(Transform parentTransform, Vector3 position, Vector3 forward) {
            return SpawnPrefab(modelPrefab, parentTransform, position, forward);
        }

        public GameObject SpawnPrefab(GameObject spawnPrefab, Transform parentTransform, Vector3 position, Vector3 forward) {
            GameObject prefabObject = Instantiate(spawnPrefab, position, Quaternion.LookRotation(forward), parentTransform);

            return prefabObject;
        }


        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            defaultAutoAttackAbility = null;
            if (defaultAutoAttackAbilityName != null && defaultAutoAttackAbilityName != string.Empty) {
                defaultAutoAttackAbility = SystemAbilityManager.MyInstance.GetResource(defaultAutoAttackAbilityName);
            }/* else {
                Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find ability : " + defaultAutoAttackAbilityName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
            }*/

            if (unitToughness == null && defaultToughness != null && defaultToughness != string.Empty) {
                UnitToughness tmpToughness = SystemUnitToughnessManager.MyInstance.GetResource(defaultToughness);
                if (tmpToughness != null) {
                    unitToughness = tmpToughness;
                } else {
                    Debug.LogError("Unit Toughness: " + defaultToughness + " not found while initializing Unit Profiles.  Check Inspector!");
                }
            }

            learnedAbilities = new List<BaseAbility>();
            if (learnedAbilityNames != null) {
                foreach (string baseAbilityName in learnedAbilityNames) {
                    BaseAbility baseAbility = SystemAbilityManager.MyInstance.GetResource(baseAbilityName);
                    if (baseAbility != null) {
                        learnedAbilities.Add(baseAbility);
                    } else {
                        Debug.LogError("UnitProfile.SetupScriptableObjects(): Could not find ability : " + baseAbilityName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            if (movementAudioProfileNames != null) {
                foreach (string movementAudioProfileName in movementAudioProfileNames) {
                    AudioProfile tmpAudioProfile = SystemAudioProfileManager.MyInstance.GetResource(movementAudioProfileName);
                    if (tmpAudioProfile != null) {
                        movementAudioProfiles.Add(tmpAudioProfile);
                    } else {
                        Debug.LogError("UnitProfile.SetupScriptableObjects(): Could not find audio profile : " + movementAudioProfileName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            weaponSkillList = new List<WeaponSkill>();
            if (weaponSkills != null) {
                foreach (string weaponSkillName in weaponSkills) {
                    WeaponSkill weaponSkill = SystemWeaponSkillManager.MyInstance.GetResource(weaponSkillName);
                    if (weaponSkill != null) {
                        weaponSkillList.Add(weaponSkill);
                    } else {
                        Debug.LogError("UnitProfile.SetupScriptableObjects(): Could not find weapon Skill : " + weaponSkillName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            powerResourceList = new List<PowerResource>();
            if (powerResources != null) {
                foreach (string powerResourcename in powerResources) {
                    PowerResource tmpPowerResource = SystemPowerResourceManager.MyInstance.GetResource(powerResourcename);
                    if (tmpPowerResource != null) {
                        powerResourceList.Add(tmpPowerResource);
                    } else {
                        Debug.LogError("UnitProfile.SetupScriptableObjects(): Could not find power resource : " + powerResourcename + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            foreach (StatScalingNode statScalingNode in primaryStats) {
                statScalingNode.SetupScriptableObjects();
            }

            if (automaticCombatStrategy == true) {
                combatStrategyName = ResourceName;
            }
            if (combatStrategyName != null && combatStrategyName != string.Empty) {
                CombatStrategy tmpCombatStrategy = SystemCombatStrategyManager.MyInstance.GetNewResource(combatStrategyName);
                if (tmpCombatStrategy != null) {
                    combatStrategy = tmpCombatStrategy;
                } else {
                    Debug.LogError("UnitProfile.SetupScriptableObjects(): Could not find combat strategy : " + combatStrategyName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }

            }

            if (faction == null && factionName != null && factionName != string.Empty) {
                Faction tmpFaction = SystemFactionManager.MyInstance.GetResource(factionName);
                if (tmpFaction != null) {
                    faction = tmpFaction;
                } else {
                    Debug.LogError("UnitProfile.SetupScriptableObjects(): Could not find faction : " + factionName + " while inititalizing " + name + ".  CHECK INSPECTOR");
                }
            }

            if (characterClass == null && characterClassName != null && characterClassName != string.Empty) {
                CharacterClass tmpCharacterClass = SystemCharacterClassManager.MyInstance.GetResource(characterClassName);
                if (tmpCharacterClass != null) {
                    characterClass = tmpCharacterClass;
                } else {
                    Debug.LogError("UnitProfile.SetupScriptableObjects(): Could not find faction : " + factionName + " while inititalizing " + name + ".  CHECK INSPECTOR");
                }
            }

            if (unitType == null && unitTypeName != null && unitTypeName != string.Empty) {
                UnitType tmpUnitType = SystemUnitTypeManager.MyInstance.GetResource(unitTypeName);
                if (tmpUnitType != null) {
                    unitType = tmpUnitType;
                    //Debug.Log(gameObject.name + ".BaseCharacter.SetupScriptableObjects(): successfully set unit type to: " + unitType.MyName);
                } else {
                    Debug.LogError("UnitProfile.SetupScriptableObjects(): Could not find unit type : " + unitTypeName + " while inititalizing " + name + ".  CHECK INSPECTOR");
                }
            }

            if (automaticPrefabProfile == true) {
                prefabProfileName = ResourceName;
            }
            if (prefabProfileName != null && prefabProfileName != string.Empty) {
                UnitPrefabProfile tmpPrefabProfile = SystemUnitPrefabProfileManager.MyInstance.GetResource(prefabProfileName);
                if (tmpPrefabProfile != null) {
                    unitPrefabProfile = tmpPrefabProfile;
                    unitPrefab = tmpPrefabProfile.UnitPrefab;
                    modelPrefab = tmpPrefabProfile.ModelPrefab;
                } else {
                    Debug.LogError("UnitProfile.SetupScriptableObjects(): Could not find prefab profile : " + prefabProfileName + " while inititalizing " + name + ".  CHECK INSPECTOR");
                }
            }



        }

    }
}