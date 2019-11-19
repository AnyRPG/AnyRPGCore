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

        [SerializeField]
        private List<string> maintainBuffs = new List<string>();

        [SerializeField]
        private List<string> attackAbilities = new List<string>();

        // track phase for music etc
        private bool phaseStarted = false;

        public int MyMaxHealthPercent { get => maxHealthPercent; set => maxHealthPercent = value; }
        public int MyMinHealthPercent { get => minHealthPercent; set => minHealthPercent = value; }
        public string MyPhaseMusicProfileName { get => phaseMusicProfileName; set => phaseMusicProfileName = value; }
        public List<string> MyMaintainBuffs { get => maintainBuffs; set => maintainBuffs = value; }
        public List<string> MyAttackAbilities { get => attackAbilities; set => attackAbilities = value; }

        public void StartPhase() {
            if (phaseStarted) {
                return;
            }
            if (phaseMusicProfileName != null && phaseMusicProfileName != string.Empty) {
                MusicProfile musicProfile = SystemMusicProfileManager.MyInstance.GetResource(phaseMusicProfileName);
                if (musicProfile != null) {
                    if (musicProfile.MyAudioClip != null) {
                        AudioManager.MyInstance.PlayMusic(musicProfile.MyAudioClip);
                    }
                }
            }
            phaseStarted = true;
        }
    }

}