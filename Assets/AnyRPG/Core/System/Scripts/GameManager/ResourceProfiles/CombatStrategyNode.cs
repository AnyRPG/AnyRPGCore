using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class CombatStrategyNode : ConfiguredClass {

        [SerializeField]
        private int maxHealthPercent = 100;

        [SerializeField]
        private int minHealthPercent = 0;

        [SerializeField]
        [ResourceSelector(resourceType = typeof(AudioProfile))]
        private string phaseMusicProfileName;

        private AudioProfile phaseMusicProfile;

        [SerializeField]
        [ResourceSelector(resourceType = typeof(Ability))]
        private List<string> maintainBuffNames = new List<string>();

        /*
        [SerializeField]
        private List<string> maintainBuffs = new List<string>();
        */

        private List<AbilityProperties> maintainBuffList = new List<AbilityProperties>();

        [SerializeField]
        [ResourceSelector(resourceType = typeof(Ability))]
        private List<string> attackAbilityNames = new List<string>();

        /*
        [SerializeField]
        private List<string> attackAbilities = new List<string>();
        */

        private List<AbilityProperties> attackAbilityList = new List<AbilityProperties>();

        // game manager references
        private AudioManager audioManager = null;

        public int MaxHealthPercent { get => maxHealthPercent; set => maxHealthPercent = value; }
        public int MinHealthPercent { get => minHealthPercent; set => minHealthPercent = value; }
        public string PhaseMusicProfileName { get => phaseMusicProfileName; set => phaseMusicProfileName = value; }
        /*
        public List<string> MyMaintainBuffs { get => maintainBuffs; set => maintainBuffs = value; }
        public List<string> MyAttackAbilities { get => attackAbilities; set => attackAbilities = value; }
        */
        public List<AbilityProperties> AttackAbilityList { get => attackAbilityList; set => attackAbilityList = value; }
        public List<AbilityProperties> MaintainBuffList { get => maintainBuffList; set => maintainBuffList = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            audioManager = systemGameManager.AudioManager;
        }

        public void StartPhase() {
            if (networkManagerServer.ServerModeActive == true) {
                return;
            }
            if (phaseMusicProfile != null) {
                if (phaseMusicProfile.AudioClip != null) {
                    audioManager.PlayMusic(phaseMusicProfile.AudioClip);
                }
            }
        }

        public void SetupScriptableObjects(SystemGameManager systemGameManager, IDescribable describable) {

            Configure(systemGameManager);

            attackAbilityList = new List<AbilityProperties>();
            if (attackAbilityNames != null) {
                foreach (string baseAbilityName in attackAbilityNames) {
                    Ability baseAbility = systemDataFactory.GetResource<Ability>(baseAbilityName);
                    if (baseAbility != null) {
                        attackAbilityList.Add(baseAbility.AbilityProperties);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find attack ability : " + baseAbilityName + " while inititalizing a combat strategy node for " + describable.DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            maintainBuffList = new List<AbilityProperties>();
            if (maintainBuffNames != null) {
                foreach (string baseAbilityName in maintainBuffNames) {
                    Ability baseAbility = systemDataFactory.GetResource<Ability>(baseAbilityName);
                    if (baseAbility != null) {
                        maintainBuffList.Add(baseAbility.AbilityProperties);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find buff ability : " + baseAbilityName + " while inititalizing a combat strategy node.  CHECK INSPECTOR");
                    }
                }
            }

            phaseMusicProfile = null;
            if (phaseMusicProfileName != null && phaseMusicProfileName != string.Empty) {
                AudioProfile musicProfile = systemDataFactory.GetResource<AudioProfile>(phaseMusicProfileName);
                if (musicProfile != null) {
                    phaseMusicProfile = musicProfile;
                } else {
                    Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find music profile : " + phaseMusicProfileName + " while inititalizing a combat strategy node.  CHECK INSPECTOR");
                }

            }

        }
    }

}