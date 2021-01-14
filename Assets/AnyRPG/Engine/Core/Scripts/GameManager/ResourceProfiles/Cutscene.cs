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

        [Tooltip("If this cutscene plays in a separate scene, this should be set to the scene name.")]
        [SerializeField]
        private string loadSceneName = string.Empty;

        private SceneNode loadScene = null;

        [Tooltip("Set to true if at the end of this cutscene, the previous scene should be loaded.")]
        [SerializeField]
        private bool unloadSceneOnEnd = false;

        [Tooltip("Set this to true to ignore the currently loaded player faction and just use faction default relationship colors.")]
        [SerializeField]
        private bool useDefaultFactionColors = false;

        [Tooltip("The name of a dialog to use for the subtitles")]
        [SerializeField]
        private string dialogName = string.Empty;

        [Tooltip("A timeline to play with the cutscene")]
        [SerializeField]
        private string timelineName = string.Empty;

        [Tooltip("Does this cutscene require the player unit to be spawned to play")]
        [SerializeField]
        private bool requirePlayerUnitSpawn = false;

        private Dialog dialog;

        public bool Viewed {
            get {
                return SaveManager.MyInstance.GetCutsceneSaveData(this).isCutSceneViewed;
            }
            set {
                //Debug.Log(DisplayName + ".Viewed: setting to: " + value);
                //SaveManager.MyInstance.GetCutsceneSaveData(this).IsCutSceneViewed = value;
                CutsceneSaveData saveData = SaveManager.MyInstance.GetCutsceneSaveData(this);
                saveData.isCutSceneViewed = value;
                SaveManager.MyInstance.CutsceneSaveDataDictionary[saveData.MyName] = saveData;
            }
        }

        public bool MyUseDefaultFactionColors { get => useDefaultFactionColors; set => useDefaultFactionColors = value; }
        public Dialog MyDialog { get => dialog; set => dialog = value; }
        public bool MyUnloadSceneOnEnd { get => unloadSceneOnEnd; set => unloadSceneOnEnd = value; }
        public SceneNode MyLoadScene { get => loadScene; set => loadScene = value; }
        public string TimelineName { get => timelineName; set => timelineName = value; }
        public bool RequirePlayerUnitSpawn { get => requirePlayerUnitSpawn; set => requirePlayerUnitSpawn = value; }
        public bool Repeatable { get => repeatable; set => repeatable = value; }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();

            if (dialogName != null && dialogName != string.Empty) {
                Dialog tmpDialog = SystemDialogManager.MyInstance.GetResource(dialogName);
                if (tmpDialog != null) {
                    dialog = tmpDialog;
                } else {
                    Debug.LogError("SceneNode.SetupScriptableObjects(): Could not find dialog : " + dialogName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

            if (loadSceneName != null && loadSceneName != string.Empty) {
                SceneNode tmpSceneNode = SystemSceneNodeManager.MyInstance.GetResource(loadSceneName);
                if (tmpSceneNode != null) {
                    loadScene = tmpSceneNode;
                } else {
                    Debug.LogError("SceneNode.SetupScriptableObjects(): Could not find sceneNode : " + loadSceneName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

        }
    }

}