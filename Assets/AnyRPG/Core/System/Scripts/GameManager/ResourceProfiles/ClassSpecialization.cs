using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Class Specialization", menuName = "AnyRPG/CharacterClassSpecialization")]
    public class ClassSpecialization : DescribableResource, IStatProvider, ICapabilityProvider {

        //[Header("Class Specialization")]

        [Header("NewGame")]

        [Tooltip("If true, this faction is available for Players to choose on the new game menu")]
        [SerializeField]
        private bool newGameOption = false;

        [Header("Start Equipment")]

        [Tooltip("The names of the equipment that will be worn by this class when a new game is started")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(Equipment))]
        private List<string> equipmentNames = new List<string>();

        private List<Equipment> equipmentList = new List<Equipment>();

        [Header("Character Classes")]

        [Tooltip("The list of class names that have access to this specialization")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(CharacterClass))]
        private List<string> classNames = new List<string>();

        private List<CharacterClass> characterClasses = new List<CharacterClass>();

        [Header("Capabilities")]

        [Tooltip("Capabilities that apply to all characters of this specialization")]
        [SerializeField]
        private CapabilityProps capabilities = new CapabilityProps();

        [Header("Stats and Scaling")]

        [Tooltip("Stats available to this unit, in addition to the stats defined at the system level that all character use")]
        [FormerlySerializedAs("statScaling")]
        [SerializeField]
        private List<StatScalingNode> primaryStats = new List<StatScalingNode>();

        [Header("Power Resources")]

        [Tooltip("Power Resources used by this unit.  The first resource is considered primary and will show on the unit frame.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(PowerResource))]
        private List<string> powerResources = new List<string>();

        // reference to the actual power resources
        private List<PowerResource> powerResourceList = new List<PowerResource>();

        [SerializeField]
        private List<PowerEnhancerNode> powerEnhancerStats = new List<PowerEnhancerNode>();

        public List<PowerResource> PowerResourceList { get => powerResourceList; set => powerResourceList = value; }
        public List<StatScalingNode> PrimaryStats { get => primaryStats; set => primaryStats = value; }
        public List<PowerEnhancerNode> PowerEnhancerStats { get => powerEnhancerStats; set => powerEnhancerStats = value; }
        public bool NewGameOption { get => newGameOption; set => newGameOption = value; }
        public List<CharacterClass> CharacterClasses { get => characterClasses; set => characterClasses = value; }
        public List<Equipment> EquipmentList { get => equipmentList; set => equipmentList = value; }

        public CapabilityProps GetFilteredCapabilities(ICapabilityConsumer capabilityConsumer, bool returnAll = true) {
            return capabilities;
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
                        Debug.LogError("CharacterClass.SetupScriptableObjects(): Could not find equipment : " + equipmentName + " while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
                    }
                }
            }

            if (classNames != null) {
                foreach (string className in classNames) {
                    CharacterClass tmpClass = systemDataFactory.GetResource<CharacterClass>(className);
                    if (tmpClass != null) {
                        characterClasses.Add(tmpClass);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find character class : " + className + " while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
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
                        Debug.LogError("CharacterClass.SetupScriptableObjects(): Could not find power resource : " + powerResourcename + " while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
                    }
                }
            }

            capabilities.SetupScriptableObjects(systemDataFactory);

        }

    }

}