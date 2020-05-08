using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New SceneNode", menuName = "AnyRPG/SceneNodes/SceneNode")]
    [System.Serializable]
    public class SceneNode : DescribableResource {

        [SerializeField]
        private Vector3 defaultSpawnPosition = Vector3.zero;

        [SerializeField]
        private string ambientMusicProfile = string.Empty;

        private AudioProfile realAmbientMusicProfile;

        [SerializeField]
        private string backgroundMusicProfile = string.Empty;

        private AudioProfile realBackgroundMusicProfile;

        [SerializeField]
        private bool suppressCharacterSpawn = false;

        [SerializeField]
        private bool suppressMainCamera = false;

        [SerializeField]
        private string autoPlayCutsceneName = string.Empty;

        private Cutscene autoPlayCutscene = null;

        private bool visited = false;

        [SerializeField]
        private List<string> environmentStateNames = new List<string>();

        private List<EnvironmentStateProfile> environmentStates = new List<EnvironmentStateProfile>();

        private Dictionary<string, PersistentObjectSaveData> persistentObjects = new Dictionary<string, PersistentObjectSaveData>();

        public string MySceneName { get => resourceName; set => resourceName = value; }
        public Vector3 MyDefaultSpawnPosition { get => defaultSpawnPosition; set => defaultSpawnPosition = value; }
        public bool MySuppressCharacterSpawn { get => suppressCharacterSpawn; set => suppressCharacterSpawn = value; }
        public bool MySuppressMainCamera { get => suppressMainCamera; set => suppressMainCamera = value; }
        public AudioProfile MyAmbientMusicProfile { get => realAmbientMusicProfile; set => realAmbientMusicProfile = value; }
        public AudioProfile MyBackgroundMusicProfile { get => realBackgroundMusicProfile; set => realBackgroundMusicProfile = value; }
        public Dictionary<string, PersistentObjectSaveData> MyPersistentObjects { get => persistentObjects; set => persistentObjects = value; }
        public List<EnvironmentStateProfile> MyEnvironmentStates { get => environmentStates; set => environmentStates = value; }
        public Cutscene MyAutoPlayCutscene { get => autoPlayCutscene; set => autoPlayCutscene = value; }
        public bool Visited { get => visited; }

        public void Visit() {
            if (visited == false) {
                visited = true;
            }
        }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();

            realAmbientMusicProfile = null;
            if (ambientMusicProfile != null && ambientMusicProfile != string.Empty) {
                AudioProfile tmpAmbientMusicProfile = SystemAudioProfileManager.MyInstance.GetResource(ambientMusicProfile);
                if (tmpAmbientMusicProfile != null) {
                    realAmbientMusicProfile = tmpAmbientMusicProfile;
                } else {
                    Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find music profile : " + ambientMusicProfile + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                }
            }

            realBackgroundMusicProfile = null;
            if (backgroundMusicProfile != null && backgroundMusicProfile != string.Empty) {
                AudioProfile tmpBackgroundMusicProfile = SystemAudioProfileManager.MyInstance.GetResource(backgroundMusicProfile);
                if (tmpBackgroundMusicProfile != null) {
                    realBackgroundMusicProfile = tmpBackgroundMusicProfile;
                } else {
                    Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find music profile : " + ambientMusicProfile + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                }
            }

            if (autoPlayCutsceneName != null && autoPlayCutsceneName != string.Empty) {
                Cutscene tmpCutscene = SystemCutsceneManager.MyInstance.GetResource(autoPlayCutsceneName);
                if (tmpCutscene != null) {
                    autoPlayCutscene = tmpCutscene;
                } else {
                    Debug.LogError("SceneNode.SetupScriptableObjects(): Could not find cutscene : " + autoPlayCutsceneName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                }
            }

            if (environmentStateNames != null) {
                foreach (string environmentStateName in environmentStateNames) {
                    EnvironmentStateProfile tmpProfile = SystemEnvironmentStateProfileManager.MyInstance.GetResource(environmentStateName);
                    if (tmpProfile != null) {
                        environmentStates.Add(tmpProfile);
                    } else {
                        Debug.LogError("SceneNode.SetupScriptableObjects(): Could not find environment state : " + environmentStateName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                    }
                }
            }

        }

    }

}