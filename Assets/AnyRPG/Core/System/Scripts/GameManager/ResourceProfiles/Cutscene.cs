using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Cutscene", menuName = "AnyRPG/Cutscene")]
    [System.Serializable]
    public class Cutscene : DescribableResource {

        [Header("Cutscene")]

        [Tooltip("Can this cutscene be viewed more than once.")]
        [SerializeField]
        private bool repeatable = false;

        [Tooltip("Does this cutscene require the player unit to be spawned to play")]
        [SerializeField]
        private bool requirePlayerUnitSpawn = false;

        [Tooltip("Set this to true to ignore the currently loaded player faction and just use faction default relationship colors.")]
        [SerializeField]
        private bool useDefaultFactionColors = false;

        [Tooltip("A timeline to play with the cutscene")]
        [SerializeField]
        private string timelineName = string.Empty;

        [Header("Scene Control")]

        [Tooltip("If this cutscene plays in a separate scene, this should be set to the scene name.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(SceneNode))]
        private string loadSceneName = string.Empty;

        private SceneNode loadScene = null;

        [Tooltip("Set to true if at the end of this cutscene, the previous scene should be loaded.")]
        [SerializeField]
        private bool unloadSceneOnEnd = false;

        [Header("Subtitles")]

        [Tooltip("Uncheck this option to allow a timeline to advance the subtitles")]
        [SerializeField]
        private bool autoAdvanceSubtitles = true;

        [SerializeField]
        private SubtitleProperties subtitleProperties = new SubtitleProperties();

        // game manager references
        protected SaveManager saveManager = null;

        public bool Viewed {
            get {
                return saveManager.GetCutsceneSaveData(this).isCutSceneViewed;
            }
            set {
                //Debug.Log(DisplayName + ".Viewed: setting to: " + value);
                //saveManager.GetCutsceneSaveData(this).IsCutSceneViewed = value;
                CutsceneSaveData saveData = saveManager.GetCutsceneSaveData(this);
                saveData.isCutSceneViewed = value;
                saveManager.CutsceneSaveDataDictionary[saveData.CutsceneName] = saveData;
            }
        }

        public bool UseDefaultFactionColors { get => useDefaultFactionColors; set => useDefaultFactionColors = value; }
        public bool UnloadSceneOnEnd { get => unloadSceneOnEnd; set => unloadSceneOnEnd = value; }
        public SceneNode LoadScene { get => loadScene; set => loadScene = value; }
        public string TimelineName { get => timelineName; set => timelineName = value; }
        public bool RequirePlayerUnitSpawn { get => requirePlayerUnitSpawn; set => requirePlayerUnitSpawn = value; }
        public bool Repeatable { get => repeatable; set => repeatable = value; }
        public bool AutoAdvanceSubtitles { get => autoAdvanceSubtitles; set => autoAdvanceSubtitles = value; }
        public SubtitleProperties SubtitleProperties { get => subtitleProperties; set => subtitleProperties = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            saveManager = systemGameManager.SaveManager;
        }

        public void ResetSubtitles() {
            foreach (SubtitleNode subtitleNode in subtitleProperties.SubtitleNodes) {
                subtitleNode.ResetStatus();
            }
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            if (loadSceneName != null && loadSceneName != string.Empty) {
                SceneNode tmpSceneNode = systemDataFactory.GetResource<SceneNode>(loadSceneName);
                if (tmpSceneNode != null) {
                    loadScene = tmpSceneNode;
                } else {
                    Debug.LogError("Cutscene.SetupScriptableObjects(): Could not find sceneNode : " + loadSceneName + " while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
                }
            }

            subtitleProperties.SetupScriptableObjects(systemGameManager);

        }
    }

}