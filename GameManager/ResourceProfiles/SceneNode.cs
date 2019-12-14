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

        private MusicProfile realAmbientMusicProfile;

        [SerializeField]
        private string backgroundMusicProfile;

        private MusicProfile realBackgroundMusicProfile;

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

        public string MySceneName { get => resourceName; set => resourceName = value; }
        public Vector3 MyDefaultSpawnPosition { get => defaultSpawnPosition; set => defaultSpawnPosition = value; }
        public bool MySuppressCharacterSpawn { get => suppressCharacterSpawn; set => suppressCharacterSpawn = value; }
        public bool MySuppressMainCamera { get => suppressMainCamera; set => suppressMainCamera = value; }
        public bool MyCutsceneViewed { get => cutsceneViewed; set => cutsceneViewed = value; }
        public bool MyIsCutScene { get => isCutScene; set => isCutScene = value; }
        public MusicProfile MyAmbientMusicProfile { get => realAmbientMusicProfile; set => realAmbientMusicProfile = value; }
        public MusicProfile MyBackgroundMusicProfile { get => realBackgroundMusicProfile; set => realBackgroundMusicProfile = value; }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();

            realAmbientMusicProfile = null;
            if (ambientMusicProfile != null && ambientMusicProfile != string.Empty) {
                realAmbientMusicProfile = SystemMusicProfileManager.MyInstance.GetResource(ambientMusicProfile);
            }/* else {
                Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find music profile : " + ambientMusicProfile + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
            }*/

            realBackgroundMusicProfile = null;
            if (backgroundMusicProfile != null && backgroundMusicProfile != string.Empty) {
                realBackgroundMusicProfile = SystemMusicProfileManager.MyInstance.GetResource(backgroundMusicProfile);
            }/* else {
                Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find music profile : " + ambientMusicProfile + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
            }*/
        }

    }

}