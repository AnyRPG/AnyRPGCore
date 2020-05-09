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

        [Tooltip("If this cutscene plays in a separate scene, this should be set to the scene name.")]
        [SerializeField]
        private string loadSceneName = string.Empty;

        private SceneNode loadScene = null;

        [Tooltip("Set to true if at the end of this cutscene, the previous scene should be loaded.")]
        [SerializeField]
        private bool unloadSceneOnEnd = false;

        //[SerializeField]
        //private bool allowCutSceneNamePlates = false;

        [SerializeField]
        private bool viewed = false;

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

        public bool MyViewed { get => viewed; set => viewed = value; }
        public bool MyUseDefaultFactionColors { get => useDefaultFactionColors; set => useDefaultFactionColors = value; }
        public Dialog MyDialog { get => dialog; set => dialog = value; }
        public bool MyUnloadSceneOnEnd { get => unloadSceneOnEnd; set => unloadSceneOnEnd = value; }
        public SceneNode MyLoadScene { get => loadScene; set => loadScene = value; }
        public string MyTimelineName { get => timelineName; set => timelineName = value; }
        public bool RequirePlayerUnitSpawn { get => requirePlayerUnitSpawn; set => requirePlayerUnitSpawn = value; }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();

            if (dialogName != null && dialogName != string.Empty) {
                Dialog tmpDialog = SystemDialogManager.MyInstance.GetResource(dialogName);
                if (tmpDialog != null) {
                    dialog = tmpDialog;
                } else {
                    Debug.LogError("SceneNode.SetupScriptableObjects(): Could not find dialog : " + dialogName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                }
            }

            if (loadSceneName != null && loadSceneName != string.Empty) {
                SceneNode tmpSceneNode = SystemSceneNodeManager.MyInstance.GetResource(loadSceneName);
                if (tmpSceneNode != null) {
                    loadScene = tmpSceneNode;
                } else {
                    Debug.LogError("SceneNode.SetupScriptableObjects(): Could not find sceneNode : " + loadSceneName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                }
            }

        }
    }

}