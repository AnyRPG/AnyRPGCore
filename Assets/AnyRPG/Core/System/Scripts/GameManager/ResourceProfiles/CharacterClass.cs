using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Character Class", menuName = "AnyRPG/CharacterClass")]
    public class CharacterClass : DescribableResource, IStatProvider, ICapabilityProvider {

        [Header("NewGame")]

        [Tooltip("If true, this character class is available for players to choose on the new game menu")]
        [SerializeField]
        private bool newGameOption = false;

        [Header("Start Equipment")]

        [Tooltip("The names of the equipment that will be worn by this class when a new game is started")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(Equipment))]
        private List<string> equipmentNames = new List<string>();

        private List<Equipment> equipmentList = new List<Equipment>();

        [Header("Capabilities")]

        [Tooltip("Capabilities that apply to all characters of this class")]
        [SerializeField]
        private CapabilityProps capabilities = new CapabilityProps();

        [Tooltip("Capabilities that only apply to specific unit types")]
        [SerializeField]
        private List<UnitTypeCapabilityNode> unitTypeCapabilities = new List<UnitTypeCapabilityNode>();

        [Header("Stats and Scaling")]

        [Tooltip("Stats available to this character class, in addition to the stats defined at the system level that all character use")]
        [FormerlySerializedAs("statScaling")]
        [SerializeField]
        private List<StatScalingNode> primaryStats = new List<StatScalingNode>();

        [Header("Power Resources")]

        [Tooltip("Power Resources used by this class.  The first resource is considered primary and will show on the unit frame.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(PowerResource))]
        private List<string> powerResources = new List<string>();

        [Header("Pet Management")]

        [Tooltip("The names of the equipment that will be worn by this class when a new game is started")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(UnitType))]
        private List<string> validPetTypes = new List<string>();

        private List<UnitType> validPetTypeList = new List<UnitType>();


        // reference to the actual power resources
        private List<PowerResource> powerResourceList = new List<PowerResource>();

        public List<PowerResource> PowerResourceList { get => powerResourceList; set => powerResourceList = value; }
        public List<StatScalingNode> PrimaryStats { get => primaryStats; set => primaryStats = value; }
        public bool NewGameOption { get => newGameOption; set => newGameOption = value; }
        public List<Equipment> EquipmentList { get => equipmentList; set => equipmentList = value; }
        public CapabilityProps Capabilities { get => capabilities; set => capabilities = value; }
        public List<UnitType> ValidPetTypeList { get => validPetTypeList; set => validPetTypeList = value; }

        public CapabilityProps GetFilteredCapabilities(ICapabilityConsumer capabilityConsumer, bool returnAll = true) {
            CapabilityProps returnValue = new CapabilityProps();
            if (returnAll) {
                returnValue = capabilities;
            }
            foreach (UnitTypeCapabilityNode unitTypeCapabilityNode in unitTypeCapabilities) {
                if (capabilityConsumer != null && capabilityConsumer.UnitType != null && unitTypeCapabilityNode.UnitTypeList.Contains(capabilityConsumer.UnitType)) {
                    returnValue = returnValue.Join(unitTypeCapabilityNode.Capabilities);
                }
            }
            return returnValue;
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            if (equipmentNames != null) {
                foreach (string equipmentName in equipmentNames) {
                    Equipment tmpEquipment = null;
                    tmpEquipment = systemDataFactory.GetResource<Item>(equipmentName) as Equipment;
                    if (tmpEquipment != null) {
                        equipmentList.Add(tmpEquipment);
                    } else {
                        Debug.LogError("CharacterClass.SetupScriptableObjects(): Could not find equipment : " + equipmentName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            if (validPetTypes != null) {
                foreach (string petType in validPetTypes) {
                    UnitType tmpUnitType = systemDataFactory.GetResource<UnitType>(petType);
                    if (tmpUnitType != null) {
                        validPetTypeList.Add(tmpUnitType);
                    } else {
                        Debug.LogError("CharacterClass.SetupScriptableObjects(): Could not find pet type : " + petType + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
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
                        Debug.LogError("CharacterClass.SetupScriptableObjects(): Could not find power resource : " + powerResourcename + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            foreach (StatScalingNode statScalingNode in primaryStats) {
                statScalingNode.SetupScriptableObjects(systemDataFactory);
            }

            capabilities.SetupScriptableObjects(systemDataFactory);
            foreach (UnitTypeCapabilityNode unitTypeCapabilityNode in unitTypeCapabilities) {
                unitTypeCapabilityNode.SetupScriptableObjects(systemDataFactory);
            }

        }

    }


    [System.Serializable]
    public class CharacterStatToResourceNode {

        [Tooltip("The name of the resource that will receive points from the stat")]
        [SerializeField]
        private string resourceName = string.Empty;

        private PowerResource powerResource = null;

        [Tooltip("The amount of the resource to be gained per point of the stat")]
        [SerializeField]
        private float resourcePerPoint = 0;

        public float ResourcePerPoint { get => resourcePerPoint; set => resourcePerPoint = value; }
        public string ResourceName { get => resourceName; set => resourceName = value; }
        public PowerResource PowerResource { get => powerResource; set => powerResource = value; }

        public void SetupScriptableObjects(SystemDataFactory systemDataFactory) {

            if (resourceName != null && resourceName != string.Empty) {
                PowerResource tmpPowerResource = systemDataFactory.GetResource<PowerResource>(resourceName);
                if (tmpPowerResource != null) {
                    powerResource = tmpPowerResource;
                } else {
                    Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find power resource : " + resourceName + " while inititalizing statresourceNode.  CHECK INSPECTOR");
                }
            }

        }

    }
    /*
    [System.Serializable]
    public class StatToResourceNode {

        [Tooltip("Resource that will have its maximum amount increased by the stat")]
        [SerializeField]
        private string powerResourceName = string.Empty;

        private PowerResource powerResource = null;

        [Tooltip("List of stats that will contribute to this resource")]
        [SerializeField]
        private List<CharacterStatToResourceNode> statConversion = new List<CharacterStatToResourceNode>();

        public PowerResource PowerResource { get => powerResource; set => powerResource = value; }
        public List<CharacterStatToResourceNode> StatConversion { get => statConversion; set => statConversion = value; }

        public void SetupScriptableObjects(SystemDataFactory systemDataFactory) {

            if (powerResourceName != null && powerResourceName != string.Empty) {
                PowerResource tmpPowerResource = systemDataFactory.GetResource<PowerResource>(powerResourceName);
                if (tmpPowerResource != null) {
                    powerResource = tmpPowerResource;
                } else {
                    Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find power resource : " + powerResourceName + " while inititalizing statresourceNode.  CHECK INSPECTOR");
                }
            }

        }
    }
    */

    [System.Serializable]
    public class UnitTypeCapabilityNode {

        [Tooltip("The unit types that will have these capabilities")]
        [ResourceSelector(resourceType = typeof(UnitType))]
        [SerializeField]
        private List<string> unitTypes = new List<string>();

        private List<UnitType> unitTypeList = new List<UnitType>();

        [Tooltip("Traits are status effects which are automatically active at all times if the level requirement is met.")]
        [SerializeField]
        private CapabilityProps capabilities = new CapabilityProps();

        public List<UnitType> UnitTypeList { get => unitTypeList; set => unitTypeList = value; }
        public CapabilityProps Capabilities { get => capabilities; set => capabilities = value; }

        public void SetupScriptableObjects(SystemDataFactory systemDataFactory) {

            foreach (string unitTypeName in unitTypes) {
                if (unitTypeName != null && unitTypeName != string.Empty) {
                    UnitType tmpUnitType = systemDataFactory.GetResource<UnitType>(unitTypeName);
                    if (tmpUnitType != null) {
                        unitTypeList.Add(tmpUnitType);
                    } else {
                        Debug.LogError("UnitProfile.SetupScriptableObjects(): Could not find faction : " + unitTypeName + " while inititalizing characterClassAbilityNode.  CHECK INSPECTOR");
                    }
                } else {
                    Debug.LogError("UnitProfile.SetupScriptableObjects(): null or empty character class name while inititalizing characterClassAbilityNode.  CHECK INSPECTOR");
                }
            }

            capabilities.SetupScriptableObjects(systemDataFactory);
        }
    }

}