using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Unit Profile", menuName = "AnyRPG/UnitProfile")]
    public class UnitProfile : DescribableResource, IStatProvider, ICapabilityProvider, ISerializationCallbackReceiver, IUUID {

        [Header("Unit Prefab")]

        [Tooltip("If true, the unit prefab is loaded by searching for a UnitPrefabProfile with the same name as this resource.")]
        [SerializeField]
        private bool automaticPrefabProfile = true;

        [Tooltip("The name of the prefab profile that contains the gameObject that represents the unit.  Only used if Automatic Prefab Profile, and use Inline props are not checked.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(UnitPrefabProfile))]
        private string prefabProfileName = string.Empty;

        [Tooltip("If true, the unit prefab is loaded from the inline prefab settings below, instead of the shared prefab profile above.")]
        [SerializeField]
        private bool useInlinePrefabProps = false;

        [Tooltip("If useInlinePrefabProps is true, these values will be used instead of the shared prefab profile above")]
        [SerializeField]
        private UnitPrefabProps unitPrefabProps = new UnitPrefabProps();

        private UnitPrefabProps unitPrefabProfileProps = null;

        [Header("Unit Settings")]

        [Tooltip("Mark this if true is the unit is an UMA unit")]
        [SerializeField]
        private bool isUMAUnit = false;

        [Tooltip("If true, this unit can be charmed and made into a pet")]
        [SerializeField]
        private bool isPet = false;

        [Tooltip("If true, this unit can fly")]
        [SerializeField]
        private bool flightCapable = false;

        [Tooltip("If true, this unit can glide")]
        [SerializeField]
        private bool glideCapable = false;

        [Header("Character")]

        [Tooltip("The name that will show over the character head and in unit frames")]
        [SerializeField]
        protected string characterName;

        [Tooltip("If set, this will show in the nameplate instead of the faction")]
        [SerializeField]
        protected string title = string.Empty;

        [Tooltip("The name of the faction this character belongs to")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(Faction))]
        protected string factionName;

        protected Faction faction;

        [Tooltip("The name of the unit type")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(UnitType))]
        protected string unitTypeName;

        protected UnitType unitType;

        [Tooltip("The race of the unit type")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(CharacterRace))]
        protected string characterRaceName;

        protected CharacterRace characterRace;

        [Tooltip("The name of the character class")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(CharacterClass))]
        protected string characterClassName;

        protected CharacterClass characterClass;

        [Tooltip("The name of the class specialization")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(ClassSpecialization))]
        protected string classSpecializationName;

        protected ClassSpecialization classSpecialization;

        [Tooltip("should this character start the game dead")]
        [SerializeField]
        protected bool spawnDead = false;

        [Tooltip("If true, the character will not despawn when dead")]
        [SerializeField]
        private bool preventAutoDespawn = false;

        [Tooltip("If this is set, when the unit spawns, it will use this toughness")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(UnitToughness))]
        private string defaultToughness = string.Empty;

        protected UnitToughness unitToughness = null;

        // disabled for now.  This should be an emergent property of the learned abilities
        /*
        [Tooltip("When no weapons are equippped to learn auto-attack abilities from, this auto-attack ability will be used")]
        [SerializeField]
        private string defaultAutoAttackAbilityName = string.Empty;
        */

        [Header("Voice")]

        [Tooltip("A voice profile that contains voice audio clips to play.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(VoiceProfile))]
        private string voiceProfile = string.Empty;

        [Tooltip("If true, the voice profile is loaded from the inline settings below, instead of the shared voice profile above.")]
        [SerializeField]
        private bool useInlineVoiceProps = false;

        [Tooltip("If useInlineVoiceProps is true, these values will be used instead of the shared voice profile above.")]
        [SerializeField]
        private VoiceProps voiceProps = new VoiceProps();

        private VoiceProps voiceProfileProps = new VoiceProps();


        [Header("Configuration")]

        [Tooltip("Capabilities that apply to this unit")]
        [SerializeField]
        private CapabilityProps capabilities = new CapabilityProps();

        private BaseAbilityProperties defaultAutoAttackAbility = null;

        [Tooltip("Stats available to this unit, in addition to the stats defined at the system level that all character use")]
        [FormerlySerializedAs("statScaling")]
        [SerializeField]
        private List<StatScalingNode> primaryStats = new List<StatScalingNode>();

        [Tooltip("Power Resources used by this unit.  The first resource is considered primary and will show on the unit frame.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(PowerResource))]
        private List<string> powerResources = new List<string>();

        // reference to the actual power resources
        private List<PowerResource> powerResourceList = new List<PowerResource>();

        [Tooltip("By default, NPCS only equip the specific equipment in the list below.  If this box is checked, NPCs will equip all default equipment from their character class, specialization, faction, etc.")]
        [SerializeField]
        bool useProviderEquipment = false;

        [Tooltip("This equipment will be equipped by default on this unit")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(Equipment))]
        private List<string> equipmentNameList = new List<string>();

        private List<Equipment> equipmentList = new List<Equipment>();

        [Header("Combat")]

        [Tooltip("If true, the unit will attack anything in its aggro radius based on faction relationship")]
        [SerializeField]
        private bool isAggressive = true;

        [Tooltip("The radius of the aggro sphere around the unit.  If any hostile characters enter this range, the character will attack them. Set to 0 to disable aggro")]
        [FormerlySerializedAs("aggroRange")]
        [SerializeField]
        private float aggroRadius = 20f;

        [Tooltip("If true, a combat strategy matching the unit name will be looked up and used if found")]
        [SerializeField]
        private bool automaticCombatStrategy = false;

        [Tooltip("The strategy that will be used when this unit is in combat")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(CombatStrategy))]
        private string combatStrategyName = string.Empty;

        // reference to the actual combat strategy
        private CombatStrategy combatStrategy;

        [Tooltip("Ability effects to cast on the target when the performing an unarmed standard (auto) attack.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AbilityEffect))]
        private List<string> defaultHitEffects = new List<string>();

        private List<AbilityEffectProperties> defaultHitEffectList = new List<AbilityEffectProperties>();

        [Header("Movement")]

        [Tooltip("If false, the unit will not have the Nav Mesh Agent enabled, and gravity will be disabled.")]
        [SerializeField]
        private bool isMobile = true;

        [Tooltip("None = Do not play footsteps.  Unit = Play the footsteps configured below.  Environment = Play the footsteps based on the terrain. UnitFallback = Try environment, then fallback to Unit if no environment sound available. Both = Play environment and unit sounds at the same time.")]
        [SerializeField]
        private FootstepType footstepType = FootstepType.UnitFallback;

        [Tooltip("If true, the movement sounds are played on footstep hit instead of in a continuous track.")]
        [SerializeField]
        private bool playOnFootstep = false;

        [Tooltip("These profiles will be played when the unit is in motion.  If footsteps are used, the next sound on the list will be played on every footstep.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AudioProfile))]
        private List<string> movementAudioProfileNames = new List<string>();

        private List<AudioProfile> movementAudioProfiles = new List<AudioProfile>();

        [Header("Patrol")]

        [Tooltip("If true, the patrol configuration below will be used for this unit.")]
        [SerializeField]
        private bool useInlinePatrol = false;

        [Tooltip("Inline patrol configuration.  Useful if no other unit will need to re-use this configuration.")]
        [SerializeField]
        private PatrolProperties patrolConfig = new PatrolProperties();

        [Tooltip("Lookup and use these named patrols that can be shared among units")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(PatrolProfile))]
        private List<string> patrolNames = new List<string>();

        [Header("Interaction")]

        [Tooltip("If true, this unit will turn to face any target that interacts with it.")]
        [SerializeField]
        private bool faceInteractionTarget = true;

        [Tooltip("The maximum range at which interacables on this unit can be interacted with")]
        [SerializeField]
        private float interactionMaxRange = 3f;

        [Tooltip("The names of the interactable options available on this character")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(InteractableOptionConfig))]
        private List<string> interactableOptions = new List<string>();

        [Tooltip("The configs of the interactable options available on this character")]
        [SerializeReference]
        [SerializeReferenceButton]
        private List<InteractableOptionProps> inlineInteractableOptions = new List<InteractableOptionProps>();

        private List<InteractableOptionProps> interactableOptionProps = new List<InteractableOptionProps>();

        [Header("Object Persistence")]

        [Tooltip("If true, the object position is saved based on selected settings. NOTE: at least one save option must be chosen (below or in patrol etc)")]
        [SerializeField]
        private bool persistObjectPosition = false;

        [Tooltip("If true, this object will save it's position when switching from one scene to another (including the main menu).  It will not save if the game is quit directly from the main menu.")]
        [SerializeField]
        private bool saveOnLevelUnload = false;

        [Tooltip("If true, this object will save it's position when the player saves the game.")]
        [SerializeField]
        private bool saveOnGameSave = false;

        [Tooltip("If true, this UUID will overwrite any UUID on the spawned unit.  Only use this for unique units")]
        [SerializeField]
        private bool overwriteUnitUUID = false;

        [Tooltip("This is an automatically generated unique string")]
        [SerializeField]
        private string m_UUID = null;

        //[Tooltip("If true, this object will overwrite any references to any other objects with the same UUID in the UUID manager.  This option should be true for non static (spawned) objects")]
        //[SerializeField]
        private bool forceUpdateUUID = false;

        // prevent this UUID from overwriting itself as soon as it's instantiated in the factory
        // this option only applies at runtime
        private bool ignoreDuplicateUUID = true;

        private string m_IDBackup = null;

        // game manager references

        private ObjectPooler objectPooler = null;

        public string ID { get => m_UUID; set => m_UUID = value; }
        public string IDBackup { get => m_IDBackup; set => m_IDBackup = value; }

        public void OnAfterDeserialize() {
            if (m_UUID == null || m_UUID != m_IDBackup) {
                UUIDManager.RegisterUUID(this);
            }
        }
        public void OnBeforeSerialize() {
            if (m_UUID == null || m_UUID != m_IDBackup) {
                UUIDManager.RegisterUUID(this);
            }
        }

        void OnDestroy() {
            UUIDManager.UnregisterUUID(this);
            m_UUID = null;
        }

        public UnitToughness DefaultToughness { get => unitToughness; set => unitToughness = value; }
        public BaseAbilityProperties DefaultAutoAttackAbility { get => defaultAutoAttackAbility; set => defaultAutoAttackAbility = value; }
        public bool IsUMAUnit { get => isUMAUnit; set => isUMAUnit = value; }
        public bool IsPet { get => isPet; set => isPet = value; }
        public FootstepType FootstepType { get => footstepType; set => footstepType = value; }
        public bool PlayOnFootstep { get => playOnFootstep; set => playOnFootstep = value; }
        public List<AudioProfile> MovementAudioProfiles { get => movementAudioProfiles; set => movementAudioProfiles = value; }
        public List<StatScalingNode> PrimaryStats { get => primaryStats; set => primaryStats = value; }
        public List<PowerResource> PowerResourceList { get => powerResourceList; set => powerResourceList = value; }
        public CombatStrategy CombatStrategy { get => combatStrategy; set => combatStrategy = value; }
        public string CharacterName { get => characterName; set => characterName = value; }
        public string Title { get => title; set => title = value; }
        public CharacterClass CharacterClass { get => characterClass; set => characterClass = value; }
        public CharacterRace CharacterRace { get => characterRace; set => characterRace = value; }
        public Faction Faction { get => faction; set => faction = value; }
        public UnitType UnitType { get => unitType; set => unitType = value; }
        public bool SpawnDead { get => spawnDead; }
        public ClassSpecialization ClassSpecialization { get => classSpecialization; set => classSpecialization = value; }
        public bool PreventAutoDespawn { get => preventAutoDespawn; set => preventAutoDespawn = value; }
        public List<string> PatrolNames { get => patrolNames; set => patrolNames = value; }
        public float AggroRadius { get => aggroRadius; set => aggroRadius = value; }
        //public BehaviorProps BehaviorProps { get => behaviorConfig; set => behaviorConfig = value; }
        public UnitPrefabProps UnitPrefabProps {
            get {
                if (useInlinePrefabProps) {
                    return unitPrefabProps;
                }
                return unitPrefabProfileProps;
            }
        }
        public VoiceProps VoiceProps {
            get {
                if (useInlineVoiceProps) {
                    return voiceProps;
                }
                return voiceProfileProps;
            }
        }


        public CapabilityProps GetFilteredCapabilities(ICapabilityConsumer capabilityConsumer, bool returnAll = true) {
            return capabilities;
        }

        public CapabilityProps Capabilities { get => capabilities; set => capabilities = value; }
        public List<Equipment> EquipmentList { get => equipmentList; set => equipmentList = value; }
        public List<InteractableOptionProps> InteractableOptionConfigs { get => interactableOptionProps; set => interactableOptionProps = value; }
        public bool IsAggressive { get => isAggressive; set => isAggressive = value; }
        public bool IsMobile { get => isMobile; set => isMobile = value; }
        public float InteractionMaxRange { get => interactionMaxRange; set => interactionMaxRange = value; }
        public bool ForceUpdateUUID { get => forceUpdateUUID; set => forceUpdateUUID = value; }
        public bool OverwriteUnitUUID { get => overwriteUnitUUID; set => overwriteUnitUUID = value; }
        public bool IgnoreDuplicateUUID { get => ignoreDuplicateUUID; set => ignoreDuplicateUUID = value; }
        public bool UseInlinePatrol { get => useInlinePatrol; set => useInlinePatrol = value; }
        public PatrolProperties PatrolConfig { get => patrolConfig; set => patrolConfig = value; }
        public bool UseProviderEquipment { get => useProviderEquipment; set => useProviderEquipment = value; }
        public bool PersistObjectPosition { get => persistObjectPosition; set => persistObjectPosition = value; }
        public bool SaveOnLevelUnload { get => saveOnLevelUnload; set => saveOnLevelUnload = value; }
        public bool SaveOnGameSave { get => saveOnGameSave; set => saveOnGameSave = value; }
        public bool FlightCapable { get => flightCapable; }
        public bool GlideCapable { get => glideCapable; }
        public bool UseInlinePrefabProps { get => useInlinePrefabProps; set => useInlinePrefabProps = value; }
        public bool UseInlineVoiceProps { get => useInlineVoiceProps; set => useInlineVoiceProps = value; }
        public bool AutomaticPrefabProfile { get => automaticPrefabProfile; set => automaticPrefabProfile = value; }
        public List<string> MovementAudioProfileNames { get => movementAudioProfileNames; set => movementAudioProfileNames = value; }
        public bool FaceInteractionTarget { get => faceInteractionTarget; set => faceInteractionTarget = value; }
        public List<AbilityEffectProperties> DefaultHitEffectList { get => defaultHitEffectList; set => defaultHitEffectList = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            objectPooler = systemGameManager.ObjectPooler;
        }

        // disabled because it was too high maintenance
        /*
        public bool UseLootableCharacter { get => useLootableCharacter; set => useLootableCharacter = value; }
        public bool UseDialog { get => useDialog; set => useDialog = value; }
        public bool UseQuestGiver { get => useQuestGiver; set => useQuestGiver = value; }
        public bool UseVendor { get => useVendor; set => useVendor = value; }
        */

        /*
    /// <summary>
    /// This will retrieve a unit profile from the system unit profile manager
    /// </summary>
    public static UnitProfile GetUnitProfileReference(string unitProfileName) {
        if (unitProfileName != null && unitProfileName != string.Empty) {
            UnitProfile tmpUnitProfile = systemDataFactory.GetResource<UnitProfile>(unitProfileName);
            if (tmpUnitProfile != null) {
                return tmpUnitProfile;
            } else {
                Debug.LogError("GetUnitProfileReference(): Unit Profile " + unitProfileName + " could not be found.");
            }
        }
        return null;
    }
    */

        /// <summary>
        /// spawn unit with parent. rotation and position from settings
        /// </summary>
        /// <param name="parentTransform"></param>
        /// <param name="settingsTransform"></param>
        /// <returns></returns>
        public UnitController SpawnUnitPrefab(Transform parentTransform, Vector3 position, Vector3 forward, UnitControllerMode unitControllerMode, int unitLevel = -1) {
            GameObject prefabObject = SpawnPrefab(UnitPrefabProps.UnitPrefab, parentTransform, position, forward);
            UnitController unitController = null;
            if (prefabObject != null) {
                unitController = prefabObject.GetComponent<UnitController>();
                if (unitController != null) {
                    
                    // give this unit a unique name
                    unitController.gameObject.name = DisplayName.Replace(" ", "") + systemGameManager.GetSpawnCount();
                    unitController.Configure(systemGameManager);
                    // test - set unitprofile first so we don't overwrite players baseCharacter settings
                    unitController.SetUnitProfile(this, unitControllerMode, unitLevel);
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
            return SpawnPrefab(UnitPrefabProps.ModelPrefab, parentTransform, position, forward);
        }

        public GameObject SpawnPrefab(GameObject spawnPrefab, Transform parentTransform, Vector3 position, Vector3 forward) {
            if (spawnPrefab == null) {
                return null;
            }
            
            GameObject prefabObject = objectPooler.GetPooledObject(spawnPrefab, position, (forward == Vector3.zero ? Quaternion.identity : Quaternion.LookRotation(forward)), parentTransform);

            return prefabObject;
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);
            /*
            defaultAutoAttackAbility = null;
            if (defaultAutoAttackAbilityName != null && defaultAutoAttackAbilityName != string.Empty) {
                defaultAutoAttackAbility = systemDataFactory.GetResource<BaseAbility>(defaultAutoAttackAbilityName);
            }*/

            if (unitToughness == null && defaultToughness != null && defaultToughness != string.Empty) {
                UnitToughness tmpToughness = systemDataFactory.GetResource<UnitToughness>(defaultToughness);
                if (tmpToughness != null) {
                    unitToughness = tmpToughness;
                } else {
                    Debug.LogError("Unit Toughness: " + defaultToughness + " not found while initializing Unit Profiles.  Check Inspector!");
                }
            }

            if (movementAudioProfileNames != null) {
                foreach (string movementAudioProfileName in movementAudioProfileNames) {
                    AudioProfile tmpAudioProfile = systemDataFactory.GetResource<AudioProfile>(movementAudioProfileName);
                    if (tmpAudioProfile != null) {
                        movementAudioProfiles.Add(tmpAudioProfile);
                    } else {
                        Debug.LogError("UnitProfile.SetupScriptableObjects(): Could not find audio profile : " + movementAudioProfileName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            if (equipmentNameList != null) {
                foreach (string equipmentName in equipmentNameList) {
                    Equipment tmpEquipment = systemDataFactory.GetResource<Item>(equipmentName) as Equipment;
                    if (tmpEquipment != null) {
                        equipmentList.Add(tmpEquipment);
                    } else {
                        Debug.LogError("UnitProfile.SetupScriptableObjects(): Could not find equipment : " + equipmentName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }


            powerResourceList = new List<PowerResource>();
            if (powerResources != null) {
                foreach (string powerResourcename in powerResources) {
                    PowerResource tmpPowerResource = systemDataFactory.GetResource<PowerResource>(powerResourcename);
                    if (tmpPowerResource != null) {
                        powerResourceList.Add(tmpPowerResource);
                    } else {
                        Debug.LogError("UnitProfile.SetupScriptableObjects(): Could not find power resource : " + powerResourcename + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            foreach (StatScalingNode statScalingNode in primaryStats) {
                statScalingNode.SetupScriptableObjects(systemDataFactory);
            }

            if (automaticCombatStrategy == true) {
                combatStrategyName = ResourceName;
            }
            if (combatStrategyName != null && combatStrategyName != string.Empty) {
                CombatStrategy tmpCombatStrategy = systemDataFactory.GetResource<CombatStrategy>(combatStrategyName);
                if (tmpCombatStrategy != null) {
                    combatStrategy = tmpCombatStrategy;
                } else {
                    Debug.LogError("UnitProfile.SetupScriptableObjects(): Could not find combat strategy : " + combatStrategyName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

            if (defaultHitEffects != null) {
                foreach (string defaultHitEffectName in defaultHitEffects) {
                    if (defaultHitEffectName != null && defaultHitEffectName != string.Empty) {
                        AbilityEffect abilityEffect = systemDataFactory.GetResource<AbilityEffect>(defaultHitEffectName);
                        if (abilityEffect != null) {
                            defaultHitEffectList.Add(abilityEffect.AbilityEffectProperties);
                        } else {
                            Debug.LogError("UnitProfile.SetupScriptableObjects(): Could not find ability effect : " + defaultHitEffectName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                        }
                    } else {
                        Debug.LogError("UnitProfile.SetupScriptableObjects(): null or empty default hit effect found while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            if (faction == null && factionName != null && factionName != string.Empty) {
                Faction tmpFaction = systemDataFactory.GetResource<Faction>(factionName);
                if (tmpFaction != null) {
                    faction = tmpFaction;
                } else {
                    Debug.LogError("UnitProfile.SetupScriptableObjects(): Could not find faction : " + factionName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

            if (characterClass == null && characterClassName != null && characterClassName != string.Empty) {
                CharacterClass tmpCharacterClass = systemDataFactory.GetResource<CharacterClass>(characterClassName);
                if (tmpCharacterClass != null) {
                    characterClass = tmpCharacterClass;
                } else {
                    Debug.LogError("UnitProfile.SetupScriptableObjects(): Could not find class : " + characterClassName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

            if (classSpecializationName != null && classSpecializationName != string.Empty) {
                ClassSpecialization tmpSpecialization = systemDataFactory.GetResource<ClassSpecialization>(classSpecializationName);
                if (tmpSpecialization != null) {
                    classSpecialization = tmpSpecialization;
                } else {
                    Debug.LogError("UnitProfile.SetupScriptableObjects(): Could not find specialization : " + classSpecializationName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

            if (characterRace == null && characterRaceName != null && characterRaceName != string.Empty) {
                CharacterRace tmpCharacterRace = systemDataFactory.GetResource<CharacterRace>(characterRaceName);
                if (tmpCharacterRace != null) {
                    characterRace = tmpCharacterRace;
                } else {
                    Debug.LogError("UnitProfile.SetupScriptableObjects(): Could not find race : " + characterRaceName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

            if (unitType == null && unitTypeName != null && unitTypeName != string.Empty) {
                UnitType tmpUnitType = systemDataFactory.GetResource<UnitType>(unitTypeName);
                if (tmpUnitType != null) {
                    unitType = tmpUnitType;
                } else {
                    Debug.LogError("UnitProfile.SetupScriptableObjects(): Could not find unit type : " + unitTypeName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

            if (automaticPrefabProfile == true) {
                prefabProfileName = ResourceName;
            }
            if (prefabProfileName != null && prefabProfileName != string.Empty) {
                UnitPrefabProfile tmpPrefabProfile = systemDataFactory.GetResource<UnitPrefabProfile>(prefabProfileName);
                if (tmpPrefabProfile != null) {
                    unitPrefabProfileProps = tmpPrefabProfile.UnitPrefabProps;
                } else {
                    Debug.LogError("UnitProfile.SetupScriptableObjects(): Could not find prefab profile : " + prefabProfileName + " while inititalizing " + name + ".  CHECK INSPECTOR");
                }
            }

            if (voiceProfile != null && voiceProfile != string.Empty) {
                VoiceProfile tmpVoiceProfile = systemDataFactory.GetResource<VoiceProfile>(voiceProfile);
                if (tmpVoiceProfile != null) {
                    voiceProfileProps = tmpVoiceProfile.VoiceProps;
                } else {
                    Debug.LogError("UnitProfile.SetupScriptableObjects(): Could not find voice profile : " + voiceProfile + " while inititalizing " + name + ".  CHECK INSPECTOR");
                }
            }

            // named interactables
            if (interactableOptions != null) {
                foreach (string interactableOptionName in interactableOptions) {
                    if (interactableOptionName != null && interactableOptionName != string.Empty) {
                        InteractableOptionConfig interactableOptionConfig = systemDataFactory.GetResource<InteractableOptionConfig>(interactableOptionName);
                        if (interactableOptionConfig != null && interactableOptionConfig.InteractableOptionProps != null) {
                            interactableOptionProps.Add(interactableOptionConfig.InteractableOptionProps);
                        } else {
                            Debug.LogError("UnitProfile.SetupScriptableObjects(): Could not find interactableOptionConfig: " + interactableOptionName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                        }
                    }
                }
            }

            // inline interactables
            foreach (InteractableOptionProps interactableOptionProp in inlineInteractableOptions) {
                if (interactableOptionProp != null) {
                    //Debug.Log(resourceName + " adding interactable option " + interactableOptionProp.GetType().ToString());
                    interactableOptionProp.SetupScriptableObjects(systemGameManager);
                    interactableOptionProps.Add(interactableOptionProp);
                }
            }

            unitPrefabProps.SetupScriptableObjects(systemDataFactory);

            capabilities.SetupScriptableObjects(systemDataFactory);

            // controller configs
            // patrolConfig doesn't need setup ?
            //behaviorConfig.SetupScriptableObjects(systemGameManager);

        }

    }

    public enum FootstepType { None, Unit, Environment, UnitFallback, Both }
}