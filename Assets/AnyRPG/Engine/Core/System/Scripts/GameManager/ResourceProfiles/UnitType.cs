using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Unit Type", menuName = "AnyRPG/UnitType")]
    public class UnitType : DescribableResource, IStatProvider, ICapabilityProvider {

        [Header("Capabilities")]

        [Tooltip("Capabilities that apply to all characters of this unit type")]
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
        private List<string> powerResources = new List<string>();

        // reference to the actual power resources
        private List<PowerResource> powerResourceList = new List<PowerResource>();

        public List<PowerResource> PowerResourceList { get => powerResourceList; set => powerResourceList = value; }
        public List<StatScalingNode> PrimaryStats { get => primaryStats; set => primaryStats = value; }

        public CapabilityProps GetFilteredCapabilities(ICapabilityConsumer capabilityConsumer, bool returnAll = true) {
            return capabilities;
        }

        public override void SetupScriptableObjects() {

            base.SetupScriptableObjects();

            powerResourceList = new List<PowerResource>();
            if (powerResources != null) {
                foreach (string powerResourcename in powerResources) {
                    PowerResource tmpPowerResource = SystemDataFactory.Instance.GetResource<PowerResource>(powerResourcename);
                    if (tmpPowerResource != null) {
                        powerResourceList.Add(tmpPowerResource);
                    } else {
                        Debug.LogError("UnitProfile.SetupScriptableObjects(): Could not find power resource : " + powerResourcename + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            capabilities.SetupScriptableObjects();

            foreach (StatScalingNode statScalingNode in primaryStats) {
                statScalingNode.SetupScriptableObjects();
            }

        }
    }

}