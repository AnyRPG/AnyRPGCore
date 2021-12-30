using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace AnyRPG {
    public class NewCharacterWizard : ScriptableWizard {

        private const string defaultUnitPrefabPath = "/AnyRPG/Core/System/Prefabs/Character/Unit/DefaultCharacterUnit.prefab";
        private const string defaultFootstepLoop = "Default Footstep Loop";
        
        // the height above the highest transform in the unit the nameplate will be set to
        private const float namePlateHeightAdd = 0.2f;

        // the highest y value of any transform found in the model, for the purposes of determining nameplate height
        private float highestYTransform = 0f;

        // a reference to the systemConfigurationManager found in the currently open scene, for automatic determination of the game name
        // and setting the newly created unit profile as the default if necessary
        private SystemConfigurationManager systemConfigurationManager = null;

        // Will be a subfolder of Application.dataPath and should start with "/"
        private const string newGameParentFolder = "/Games/";

        // the character model will be searched for bones with this name to try to auto-configure the head bone
        private string[] defaultHeadBones = { "Head", "head" };

        // the used file path name for the game
        private string fileSystemGameName = string.Empty;

        // the used asset path for the Unit Profile
        private string scriptableObjectPath = string.Empty;

        // the unit prefab to be used
        private GameObject unitPrefab = null;

        // user modified variables
        [Header("Game")]
        public string gameName = string.Empty;

        [Header("Character")]

        [Tooltip("If true, this character will be the default player character for the specified game")]
        public bool setAsDefaultPlayerCharacter = false;

        [Tooltip("Separate from the player name, this is the name of the unit profile that will be shown in lists of unit profiles the player can choose from")]
        public string characterName = string.Empty;

        [Tooltip("The prefab with an animator attached that will be used as the character model")]
        public GameObject characterModel = null;

        [Tooltip("The head bone to be used for unit frame snapshots")]
        public string headBone = string.Empty;

        [Tooltip("If the character is not a humanoid, animations should be set here")]
        public AnimationProps animations = new AnimationProps();

        [MenuItem("Tools/AnyRPG/Wizard/New Character Wizard")]
        public static void CreateWizard() {
            ScriptableWizard.DisplayWizard<NewCharacterWizard>("New Character Wizard", "Create");
        }

        void OnEnable() {
            systemConfigurationManager = GameObject.FindObjectOfType<SystemConfigurationManager>();
            if (systemConfigurationManager != null) {
                gameName = systemConfigurationManager.GameName;
            }
        }

        void OnWizardCreate() {

            EditorUtility.DisplayProgressBar("New Character Wizard", "Checking parameters...", 0.1f);

            SetUnitPrefab();

            EditorUtility.DisplayProgressBar("New Character Wizard", "Creating Resources Subfolder...", 0.2f);
            // Create root game folder
            string newGameFolder = GetNewGameFolder();
            string resourcesFolder = newGameFolder + "/Resources/" + fileSystemGameName + "/UnitProfile";

            // create resources folder
            CreateFolderIfNotExists(resourcesFolder);

            AssetDatabase.Refresh();

            EditorUtility.DisplayProgressBar("New Character Wizard", "Creating Unit Profile...", 0.3f);
            UnitProfile asset = ScriptableObject.CreateInstance("UnitProfile") as UnitProfile;

            EditorUtility.DisplayProgressBar("New Character Wizard", "Configuring Unit Profile...", 0.4f);
            // setup unit profile properties
            asset.ResourceName = characterName;
            asset.CharacterName = characterName;
            asset.AutomaticPrefabProfile = false;
            asset.UseInlinePrefabProps = true;

            // setup unit prefab properties
            asset.UnitPrefabProps.UnitPrefab = unitPrefab;
            asset.UnitPrefabProps.ModelPrefab = characterModel;
            asset.UnitPrefabProps.RotateModel = true;
            asset.UnitPrefabProps.UseInlineAnimationProps = true;

            // setup animation properties
            asset.UnitPrefabProps.AnimationProps = animations;

            // setup nameplate properties
            asset.UnitPrefabProps.NamePlateProps.UnitFrameTarget = headBone;
            asset.UnitPrefabProps.NamePlateProps.OverrideNameplatePosition = true;
            asset.UnitPrefabProps.NamePlateProps.NameplatePosition = new Vector3(0f, highestYTransform + namePlateHeightAdd, 0f);
            // set the look position at half the height
            asset.UnitPrefabProps.NamePlateProps.UnitPreviewCameraLookOffset = new Vector3(0f, highestYTransform / 2f, 0f);
            // zoom out to 1.25 times the height
            asset.UnitPrefabProps.NamePlateProps.UnitPreviewCameraPositionOffset = new Vector3(0f, highestYTransform / 2f, highestYTransform * 1.25f);

            // setup foootstep properties
            asset.MovementAudioProfileNames.Add(defaultFootstepLoop);

            EditorUtility.DisplayProgressBar("New Character Wizard", "Saving Unit Profile...", 0.5f);

            scriptableObjectPath = "Assets" + newGameParentFolder + fileSystemGameName + "/Resources/" + fileSystemGameName + "/UnitProfile/" + GetFileSystemCharactername(characterName) + "Unit.asset";
            AssetDatabase.CreateAsset(asset, scriptableObjectPath);

            AssetDatabase.Refresh();

            EditorUtility.DisplayProgressBar("New Character Wizard", "Checking Default Player Unit Setting...", 0.6f);
            if (setAsDefaultPlayerCharacter == true && systemConfigurationManager != null) {
                SystemConfigurationManager diskSystemConfigurationManager = PrefabUtility.GetCorrespondingObjectFromSource<SystemConfigurationManager>(systemConfigurationManager);
                if (diskSystemConfigurationManager != null) {
                    diskSystemConfigurationManager.DefaultPlayerUnitProfileName = characterName;
                    EditorUtility.SetDirty(diskSystemConfigurationManager);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    // this next bit is due to a unity bug? where scene objects are not reimported when modified through script
                    /*
                    Object realGO = PrefabUtility.GetCorrespondingObjectFromSource(systemConfigurationManager);
                    string selectedPath = AssetDatabase.GetAssetPath(realGO);
                    Debug.Log(selectedPath);
                    AssetDatabase.ImportAsset(selectedPath);
                    */

                    //EditorApplication.RepaintHierarchyWindow();
                    //SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
                    //Resources.UnloadUnusedAssets();
                    //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
                }
            }

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("New Character Wizard", "New Character Wizard Complete! The character UnitProfile can be found at " + scriptableObjectPath, "OK");

        }

        void OnWizardUpdate() {
            helpString = "Creates a new character unit profile";

            MakeFileSystemGameName();
            SetDefaultCharacterName();
            SetDefaultHeadBone();
            SetUnitPrefab();
            CheckHighestPoint();

            errorString = Validate();
            isValid = (errorString == null || errorString == "");
        }

        private void CheckHighestPoint() {
            float highestY = 0f;
            if (characterModel != null) {

                Transform[] childTransforms = characterModel.GetComponentsInChildren<Transform>();
                for (int i = 0; i < childTransforms.Length; i++) {
                    if (childTransforms[i].position.y - characterModel.transform.position.y > highestY) {
                        highestY = childTransforms[i].position.y - characterModel.transform.position.y;
                    }
                }
                Debug.Log("Highest transform Y value in model : " + highestY);
                highestYTransform = highestY;
            }
        }

        private void SetUnitPrefab() {
            //Debug.Log("NewCharacterWizard.SetUnitPrefab()");
            if (unitPrefab == null) {
                string unitPrefabFilePath = Application.dataPath + defaultUnitPrefabPath;
                if (System.IO.File.Exists(unitPrefabFilePath)) {
                    unitPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets" + defaultUnitPrefabPath);
                } else {
                    Debug.Log("Could not find unit prefab at " + unitPrefabFilePath);
                }
            }
        }

        private void SetDefaultCharacterName() {
            if (characterName == string.Empty && characterModel != null) {
                characterName = characterModel.name;
            }
        }

        private void SetDefaultHeadBone() {
            if (headBone == string.Empty && characterModel != null) {
                for (int i = 0; i < defaultHeadBones.Length; i++) {
                    Transform headBoneTransform = characterModel.transform.FindChildByRecursive(defaultHeadBones[i]);
                    if (headBoneTransform != null) {
                        headBone = headBoneTransform.name;
                        break;
                    }
                }
            }
            
        }

        private void MakeFileSystemGameName() {
            fileSystemGameName = gameName.Replace(" ", "");
        }

        private string GetFileSystemCharactername(string characterName) {
            return characterName.Replace(" ", "");
        }

        string GetNewGameFolder() {
            return Application.dataPath + newGameParentFolder + fileSystemGameName;
        }

        string Validate() {

            // check for empty game name
            if (gameName == null || gameName.Trim() == "") {
                return "Game name must not be empty";
            }
           
            // check for game folder existing
            string newGameFolder = GetNewGameFolder();
            if (System.IO.Directory.Exists(newGameFolder) == false) {
                return "The folder " + newGameFolder + "does not exist.  Please run the new game wizard first to create the game folder structure";
            }

            // check for animator on model
            if (characterModel != null) {
                Animator animator = characterModel.GetComponentInChildren<Animator>();
                if (animator == null) {
                    return "Could not find an animator on the character model";
                }

                // ensure no character controller is present
                CharacterController characterController = characterModel.GetComponent<CharacterController>();
                if (characterController != null) {
                    return "The character controller on " + characterModel.name + " must be removed.  It will interfere with the AnyRPG character controller.";
                }

                // warn if scripts found, as they are usually related to character controllers and could interfere with the built in character controller
                MonoBehaviour[] monoBehaviours = characterModel.GetComponents<MonoBehaviour>();
                if (monoBehaviours.Length > 0) {
                    Debug.LogWarning("There are scripts attached to the character model.  Please ensure there are no character controllers or other components that could interfere with AnyRPG control of the character.");
                }

                // check for head bone
                if (headBone == string.Empty) {
                    return "Head Bone should not be empty";
                } else {
                    Transform headBoneTransform = characterModel.transform.FindChildByRecursive(headBone);
                    if (headBoneTransform == null) {
                        return "Head Bone '" + headBone + "' not found";
                    }
                }
            } else {
                return "Character Model is required";
            }

            // check for empty character name
            if (characterName == null || characterName.Trim() == "") {
                return "Character Name is Required";
            }

            return null;
        }

        private void ShowError(string message) {
            EditorUtility.DisplayDialog("Error", message, "OK");
        }


        private void CreateFolderIfNotExists(string folderName) {
            if (!System.IO.Directory.Exists(folderName)) {
                Debug.Log("Create folder " + folderName);
                System.IO.Directory.CreateDirectory(folderName);
            }

            AssetDatabase.Refresh();
        }
    }

}
