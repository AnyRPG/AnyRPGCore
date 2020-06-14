using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Unit Profile", menuName = "AnyRPG/UnitProfile")]
    [System.Serializable]
    public class UnitProfile : DescribableResource {

        [SerializeField]
        private GameObject unitPrefab = null;

        [SerializeField]
        private string defaultToughness = string.Empty;

        protected UnitToughness unitToughness = null;

        [SerializeField]
        private string defaultAutoAttackAbilityName = string.Empty;

        /*
        [SerializeField]
        private string defaultAutoAttackAbility;
        */

        private BaseAbility realDefaultAutoAttackAbility = null;

        [SerializeField]
        private bool isUMAUnit = false;

        // this unit can be charmed and made into a pet
        [SerializeField]
        private bool isPet = false;

        [SerializeField]
        private List<string> learnedAbilityNames = new List<string>();

        //[SerializeField]
        private List<BaseAbility> learnedAbilities = new List<BaseAbility>();

        [Header("Movement")]

        [Tooltip("If true, the movement sounds are played on footstep hit instead of in a continuous track.")]
        [SerializeField]
        private bool playOnFootstep = false;

        [Tooltip("These profiles will be played when the unit is in motion.  If footsteps are used, the next sound on the list will be played on every footstep.")]
        [SerializeField]
        private List<string> movementAudioProfileNames = new List<string>();

        private List<AudioProfile> movementAudioProfiles = new List<AudioProfile>();

        public GameObject MyUnitPrefab { get => unitPrefab; set => unitPrefab = value; }
        public UnitToughness MyDefaultToughness { get => unitToughness; set => unitToughness = value; }
        public BaseAbility MyDefaultAutoAttackAbility { get => realDefaultAutoAttackAbility; set => realDefaultAutoAttackAbility = value; }
        public bool MyIsUMAUnit { get => isUMAUnit; set => isUMAUnit = value; }
        public bool MyIsPet { get => isPet; set => isPet = value; }
        public List<BaseAbility> MyLearnedAbilities { get => learnedAbilities; set => learnedAbilities = value; }
        public bool PlayOnFootstep { get => playOnFootstep; set => playOnFootstep = value; }
        public List<AudioProfile> MovementAudioProfiles { get => movementAudioProfiles; set => movementAudioProfiles = value; }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            realDefaultAutoAttackAbility = null;
            if (defaultAutoAttackAbilityName != null && defaultAutoAttackAbilityName != string.Empty) {
                realDefaultAutoAttackAbility = SystemAbilityManager.MyInstance.GetResource(defaultAutoAttackAbilityName);
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
                        Debug.LogError("UnitProfile.SetupScriptableObjects(): Could not find ability : " + baseAbilityName + " while inititalizing " + MyDisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            if (movementAudioProfileNames != null) {
                foreach (string movementAudioProfileName in movementAudioProfileNames) {
                    AudioProfile tmpAudioProfile = SystemAudioProfileManager.MyInstance.GetResource(movementAudioProfileName);
                    if (tmpAudioProfile != null) {
                        movementAudioProfiles.Add(tmpAudioProfile);
                    } else {
                        Debug.LogError("UnitProfile.SetupScriptableObjects(): Could not find audio profile : " + movementAudioProfileName + " while inititalizing " + MyDisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }


        }
    }

}