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
        private string ambientMusicProfile;

        private AudioProfile realAmbientMusicProfile;

        [SerializeField]
        private string backgroundMusicProfile = string.Empty;

        private AudioProfile realBackgroundMusicProfile;

        [SerializeField]
        private bool suppressCharacterSpawn;

        [SerializeField]
        private bool suppressMainCamera;

        [SerializeField]
        private bool isCutScene;

        [SerializeField]
        private bool allowCutSceneNamePlates;

        [SerializeField]
        private bool cutsceneViewed;

        // only applies if this is a cutscene
        [SerializeField]
        private bool useDefaultFactionColors = false;

        [SerializeField]
        private List<string> environmentStateNames = new List<string>();

        private List<EnvironmentStateProfile> environmentStates = new List<EnvironmentStateProfile>();

        [SerializeField]
        private string dialogName;

        private Dialog dialog;

        private Dictionary<string, PersistentObjectSaveData> persistentObjects = new Dictionary<string, PersistentObjectSaveData>();

        public string MySceneName { get => resourceName; set => resourceName = value; }
        public Vector3 MyDefaultSpawnPosition { get => defaultSpawnPosition; set => defaultSpawnPosition = value; }
        public bool MySuppressCharacterSpawn { get => suppressCharacterSpawn; set => suppressCharacterSpawn = value; }
        public bool MySuppressMainCamera { get => suppressMainCamera; set => suppressMainCamera = value; }
        public bool MyCutsceneViewed { get => cutsceneViewed; set => cutsceneViewed = value; }
        public bool MyIsCutScene { get => isCutScene; set => isCutScene = value; }
        public AudioProfile MyAmbientMusicProfile { get => realAmbientMusicProfile; set => realAmbientMusicProfile = value; }
        public AudioProfile MyBackgroundMusicProfile { get => realBackgroundMusicProfile; set => realBackgroundMusicProfile = value; }
        public Dictionary<string, PersistentObjectSaveData> MyPersistentObjects { get => persistentObjects; set => persistentObjects = value; }
        public Dialog MyDialog { get => dialog; set => dialog = value; }
        public bool MyUseDefaultFactionColors { get => useDefaultFactionColors; set => useDefaultFactionColors = value; }
        public List<EnvironmentStateProfile> MyEnvironmentStates { get => environmentStates; set => environmentStates = value; }

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

            if (dialogName != null && dialogName != string.Empty) {
                Dialog tmpDialog = SystemDialogManager.MyInstance.GetResource(dialogName);
                if (tmpDialog != null) {
                    dialog = tmpDialog;
                } else {
                    Debug.LogError("SceneNode.SetupScriptableObjects(): Could not find dialog : " + dialogName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
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