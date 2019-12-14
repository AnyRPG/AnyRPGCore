using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    [System.Serializable]
    public class CombatStrategyNode {

        [SerializeField]
        private int maxHealthPercent = 100;

        [SerializeField]
        private int minHealthPercent = 0;

        [SerializeField]
        private string phaseMusicProfileName;

        private MusicProfile phaseMusicProfile;

        [SerializeField]
        private List<string> maintainBuffNames = new List<string>();

        /*
        [SerializeField]
        private List<string> maintainBuffs = new List<string>();
        */

        private List<BaseAbility> maintainBuffList = new List<BaseAbility>();

        [SerializeField]
        private List<string> attackAbilityNames = new List<string>();

        /*
        [SerializeField]
        private List<string> attackAbilities = new List<string>();
        */

        private List<BaseAbility> attackAbilityList = new List<BaseAbility>();

        // track phase for music etc
        private bool phaseStarted = false;

        public int MyMaxHealthPercent { get => maxHealthPercent; set => maxHealthPercent = value; }
        public int MyMinHealthPercent { get => minHealthPercent; set => minHealthPercent = value; }
        public string MyPhaseMusicProfileName { get => phaseMusicProfileName; set => phaseMusicProfileName = value; }
        /*
        public List<string> MyMaintainBuffs { get => maintainBuffs; set => maintainBuffs = value; }
        public List<string> MyAttackAbilities { get => attackAbilities; set => attackAbilities = value; }
        */
        public List<BaseAbility> MyAttackAbilityList { get => attackAbilityList; set => attackAbilityList = value; }
        public List<BaseAbility> MyMaintainBuffList { get => maintainBuffList; set => maintainBuffList = value; }

        public void StartPhase() {
            if (phaseStarted) {
                return;
            }
            if (phaseMusicProfile != null) {
                if (phaseMusicProfile.MyAudioClip != null) {
                    AudioManager.MyInstance.PlayMusic(phaseMusicProfile.MyAudioClip);
                }
            }
            phaseStarted = true;
        }

        public void SetupScriptableObjects() {

            attackAbilityList = new List<BaseAbility>();
            if (attackAbilityNames != null) {
                foreach (string baseAbilityName in attackAbilityNames) {
                    BaseAbility baseAbility = SystemAbilityManager.MyInstance.GetResource(baseAbilityName);
                    if (baseAbility != null) {
                        attackAbilityList.Add(baseAbility);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find ability : " + baseAbilityName + " while inititalizing a combat strategy node.  CHECK INSPECTOR");
                    }
                }
            }

            phaseMusicProfile = null;
            if (phaseMusicProfileName != null && phaseMusicProfileName != string.Empty) {
                MusicProfile musicProfile = SystemMusicProfileManager.MyInstance.GetResource(phaseMusicProfileName);
                if (musicProfile != null) {
                    phaseMusicProfile = musicProfile;
                } else {
                    Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find music profile : " + phaseMusicProfileName + " while inititalizing a combat strategy node.  CHECK INSPECTOR");
                }

            }

        }
    }

}