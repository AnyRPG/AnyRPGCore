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

        private const string pathToDefaultCharacterUnitPrefab = "/AnyRPG/Core/System/Prefabs/Character/Unit/DefaultCharacterUnit.prefab";
        private const string pathToUnitSpawnNodeTemplate = "/AnyRPG/Core/Templates/Prefabs/UnitSpawner/UnitSpawnNode.prefab";
        private const string pathToLeftHandAttachment = "/AnyRPG/Core/Templates/Prefabs/Attachments/LeftHandAttachment.prefab";
        private const string pathToRightHandAttachment = "/AnyRPG/Core/Templates/Prefabs/Attachments/RightHandAttachment.prefab";
        private const string pathToLeftArmAttachment = "/AnyRPG/Core/Templates/Prefabs/Attachments/ShieldAttachment.prefab";
        private const string pathToHipAttachments = "/AnyRPG/Core/Templates/Prefabs/Attachments/HipAttachments.prefab";
        private const string pathToSpineAttachments = "/AnyRPG/Core/Templates/Prefabs/Attachments/SpineAttachments.prefab";
        private const string defaultFootstepLoop = "Default Footstep Loop";

        // the height above the highest transform in the unit the nameplate will be set to
        private const float namePlateHeightAdd = 0.2f;

        // the highest y value of any transform found in the model, for the purposes of determining nameplate height
        private float highestYTransform = 0f;

        private float modelScale = 0f;

        // a reference to the systemConfigurationManager found in the currently open scene, for automatic determination of the game name
        // and setting the newly created unit profile as the default if necessary
        private SystemConfigurationManager systemConfigurationManager = null;

        // Will be a subfolder of Application.dataPath and should start with "/"
        private string gameParentFolder = "/Games/";

        // the character model will be searched for bones with these names to try to auto-configure the bones
        private string[] defaultHeadBones = { "Head" };
        private string[] defaultLeftHandBones = { "LeftHand" };
        private string[] defaultRightHandBones = { "RightHand" };
        private string[] defaultLeftArmBones = { "arm_lower_L", "LeftForeArm" };
        private string[] defaultHipBones = { "Hips" };
        private string[] defaultSpineBones = { "Spine1", "Spine", "Torso" };

        // the used file path name for the game
        private string fileSystemGameName = string.Empty;

        // the used asset path for the Unit Profile
        private string scriptableObjectAssetPath = string.Empty;

        // the unit prefab to be used
        //private GameObject unitPrefab = null;

        // user modified variables
        [Header("Game")]
        public string gameName = string.Empty;

        [Header("Options")]

        [Tooltip("Create a unit spawn node that can be placed in a scene to spawn this character.  Useful for NPCs")]
        public bool createUnitSpawnNode = false;

        [Tooltip("If true, this character will be the default player character for the specified game")]
        public bool setAsDefaultPlayerCharacter = false;

        [Header("Prefab")]

        [Tooltip("The prefab with an animator attached that will be used as the character model")]
        public GameObject characterModel = null;

        [Header("Unit Frame")]

        [Tooltip("The head bone to be used for unit frame snapshots")]
        public string headBone = string.Empty;

        [Header("Weapon Attachments")]

        [Tooltip("Defines the attachment points for weapons. For non UMA characters, manually ensure that the HipAttachments, SpineAttachments, etc prefabs have been attached to the bones of the model.")]
        [ResourceSelector(resourceType = typeof(AttachmentProfile))]
        public string attachmentProfile = string.Empty;

        [Tooltip("If true, attachment points will be added to the model to allow it to equip weapon models.")]
        public bool addWeaponAttachments = false;

        [Tooltip("The left hand bone for holding off hand weapons")]
        public string leftHandBone = string.Empty;

        [Tooltip("The right hand bone for holding main hand weapons")]
        public string rightHandBone = string.Empty;

        [Tooltip("The left forearm bone for holding shields")]
        public string leftArmBone = string.Empty;

        [Tooltip("The hip bone for sheathed weapons that hang off the hips")]
        public string hipBone = string.Empty;

        [Tooltip("The spine bone for sheathed weapons that hang off the back")]
        public string spineBone = string.Empty;

        [Header("Details")]

        [Tooltip("Separate from the player name, this is the name of the unit profile that will be shown in lists of unit profiles the player can choose from")]
        public string characterName = string.Empty;

        [Tooltip("The image shown on the character button when selecting this character from the new game menu")]
        public Sprite portraitImage = null;

        [Header("Animation")]

        [Tooltip("If the character is not a humanoid, animations should be set here")]
        public AnimationProps animations = new AnimationProps();

        [MenuItem("Tools/AnyRPG/Wizard/New Character Wizard")]
        public static void CreateWizard() {
            ScriptableWizard.DisplayWizard<NewCharacterWizard>("New Character Wizard", "Create");
        }

        void OnEnable() {

            systemConfigurationManager = WizardUtilities.GetSystemConfigurationManager();
            gameName = WizardUtilities.GetGameName(systemConfigurationManager);
            gameParentFolder = WizardUtilities.GetGameParentFolder(systemConfigurationManager, gameName);
        }

        void OnWizardCreate() {

            try {
                CreateCharacter();
            } catch {
                Debug.LogWarning("Error detected while running wizard");

                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("New Character Wizard", "New Character Wizard encountered an error.  Check the console log for details.", "OK");
                throw;
            }

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("New Character Wizard", "New Character Wizard Complete! The character UnitProfile can be found at " + scriptableObjectAssetPath, "OK");

        }

        private void CreateCharacter() {
            EditorUtility.DisplayProgressBar("New Character Wizard", "Checking parameters...", 0.1f);

            // check for presence of template prefabs and resources
            if (CheckFilesExist() == false) {
                return;
            }

            EditorUtility.DisplayProgressBar("New Character Wizard", "Creating Resources Subfolder...", 0.2f);
            // Create root game folder
            string gameFileSystemFolder = WizardUtilities.GetGameFileSystemFolder(gameParentFolder, fileSystemGameName);
            string resourcesFolder = gameFileSystemFolder + "/Resources/" + fileSystemGameName + "/UnitProfile";

            // create resources folder
            WizardUtilities.CreateFolderIfNotExists(resourcesFolder);

            AssetDatabase.Refresh();

            // create model copy with attachments
            if (addWeaponAttachments == true) {
                EditorUtility.DisplayProgressBar("New Character Wizard", "Adding Attachments To Model...", 0.25f);
                characterModel = AddAttachmentsToModel();
            }

            EditorUtility.DisplayProgressBar("New Character Wizard", "Creating Unit Profile...", 0.3f);
            UnitProfile asset = ScriptableObject.CreateInstance("UnitProfile") as UnitProfile;

            EditorUtility.DisplayProgressBar("New Character Wizard", "Configuring Unit Profile...", 0.4f);
            // setup unit profile properties
            asset.ResourceName = characterName;
            asset.Icon = portraitImage;
            asset.CharacterName = characterName;
            asset.AutomaticPrefabProfile = false;
            asset.UseInlinePrefabProps = true;

            // setup unit prefab properties
            GameObject unitPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets" + pathToDefaultCharacterUnitPrefab);
            asset.UnitPrefabProps.UnitPrefab = unitPrefab;
            asset.UnitPrefabProps.ModelPrefab = characterModel;
            asset.UnitPrefabProps.RotateModel = true;
            asset.UnitPrefabProps.UseInlineAnimationProps = true;
            asset.UnitPrefabProps.AttachmentProfileName = attachmentProfile;

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

            scriptableObjectAssetPath = "Assets" + gameParentFolder + fileSystemGameName + "/Resources/" + fileSystemGameName + "/UnitProfile/" + WizardUtilities.GetScriptableObjectFileSystemName(characterName) + "Unit.asset";
            AssetDatabase.CreateAsset(asset, scriptableObjectAssetPath);

            AssetDatabase.Refresh();

            // set default player in SystemConfigurationManager
            EditorUtility.DisplayProgressBar("New Character Wizard", "Checking Default Player Unit Setting...", 0.6f);
            if (setAsDefaultPlayerCharacter == true && systemConfigurationManager != null) {

                systemConfigurationManager.DefaultPlayerUnitProfileName = characterName;
                EditorUtility.SetDirty(systemConfigurationManager);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            // create unit spawn node
            EditorUtility.DisplayProgressBar("New Character Wizard", "Optionally creating unit spawn node...", 0.7f);
            if (createUnitSpawnNode == true) {
                // create unit spawn node prefab folder
                string unitSpawnNodeFileSystemFolder = gameFileSystemFolder + "/Prefab/UnitSpawnNode";
                WizardUtilities.CreateFolderIfNotExists(unitSpawnNodeFileSystemFolder);

                string unitSpawnNodeAssetPath = "Assets" + gameParentFolder + fileSystemGameName + "/Prefab/UnitSpawnNode/" + WizardUtilities.GetScriptableObjectFileSystemName(characterName) + "UnitSpawnNode.prefab";
                AssetDatabase.CopyAsset("Assets" + pathToUnitSpawnNodeTemplate, unitSpawnNodeAssetPath);
                GameObject unitSpawnNodePrefab = (GameObject)AssetDatabase.LoadMainAssetAtPath(unitSpawnNodeAssetPath);
                if (unitSpawnNodePrefab != null) {
                    UnitSpawnNode unitSpawnNode = unitSpawnNodePrefab.GetComponent<UnitSpawnNode>();
                    if (unitSpawnNode != null) {
                        unitSpawnNode.UnitProfileNames = new List<string>() { characterName };

                        EditorUtility.SetDirty(unitSpawnNode);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                }
            }
        }

        private GameObject AddAttachmentsToModel() {

            // create new model folder if it does not exist
            string newModelFolder = gameParentFolder + fileSystemGameName + "/Prefab/Character";
            WizardUtilities.CreateFolderIfNotExists(Application.dataPath + newModelFolder);

            // copy existing model to new path
            string pathToModelPrefab = AssetDatabase.GetAssetPath(characterModel);
            string pathToModelCopy = "Assets" + newModelFolder + "/" + WizardUtilities.GetScriptableObjectFileSystemName(characterModel.name) + ".prefab";
            //Debug.Log("Copy " + pathToModelPrefab + " to " + pathToModelCopy);
            //AssetDatabase.CopyAsset(pathToModelPrefab, pathToModelCopy);
            //AssetDatabase.Refresh();

            // this next bit kind of sucks.  Unity does not seem to have a way to load an FBX directly the way that a 
            // prefab can be loaded, so in case an FBX is provided, it must first be turned into a prefab.
            // this leaves behind the original object in the scene, and to get rid of it, the easiest way seems to be to
            // perform an undo, which means any user changes that happened before the instantiation will be lost
            GameObject modelOriginal = (GameObject)AssetDatabase.LoadMainAssetAtPath(pathToModelPrefab);
            GameObject modelCopyObject = (GameObject)PrefabUtility.InstantiatePrefab(modelOriginal);
            PrefabUtility.UnpackPrefabInstance(modelCopyObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            PrefabUtility.SaveAsPrefabAsset(modelCopyObject, pathToModelCopy);
            GameObject.DestroyImmediate(modelCopyObject);

            modelCopyObject = PrefabUtility.LoadPrefabContents(pathToModelCopy);
            if (modelCopyObject != null) {
                if (leftHandBone != string.Empty) {
                    Transform parentTransform = modelCopyObject.transform.FindChildByRecursive(leftHandBone);
                    if (parentTransform != null) {
                        GameObject attachmentTemplate = (GameObject)AssetDatabase.LoadMainAssetAtPath("Assets" + pathToLeftHandAttachment);
                        GameObject attachmentObject = PrefabUtility.InstantiatePrefab(attachmentTemplate) as GameObject;
                        attachmentObject.transform.SetParent(parentTransform);
                        attachmentObject.transform.localScale = new Vector3(1f / modelScale, 1f / modelScale, 1f / modelScale);
                        attachmentObject.transform.localPosition = Vector3.zero;
                        attachmentObject.transform.localEulerAngles = Vector3.zero;
                        PrefabUtility.RecordPrefabInstancePropertyModifications(attachmentObject.GetComponent<Transform>());
                    }
                }
                if (rightHandBone != string.Empty) {
                    Transform parentTransform = modelCopyObject.transform.FindChildByRecursive(rightHandBone);
                    if (parentTransform != null) {
                        GameObject attachmentTemplate = (GameObject)AssetDatabase.LoadMainAssetAtPath("Assets" + pathToRightHandAttachment);
                        GameObject attachmentObject = PrefabUtility.InstantiatePrefab(attachmentTemplate) as GameObject;
                        attachmentObject.transform.SetParent(parentTransform);
                        attachmentObject.transform.localScale = new Vector3(1f / modelScale, 1f / modelScale, 1f / modelScale);
                        attachmentObject.transform.localPosition = Vector3.zero;
                        attachmentObject.transform.localEulerAngles = Vector3.zero;
                        PrefabUtility.RecordPrefabInstancePropertyModifications(attachmentObject.GetComponent<Transform>());
                    }
                }
                if (leftArmBone != string.Empty) {
                    Transform parentTransform = modelCopyObject.transform.FindChildByRecursive(leftArmBone);
                    if (parentTransform != null) {
                        GameObject attachmentTemplate = (GameObject)AssetDatabase.LoadMainAssetAtPath("Assets" + pathToLeftArmAttachment);
                        GameObject attachmentObject = PrefabUtility.InstantiatePrefab(attachmentTemplate) as GameObject;
                        attachmentObject.transform.SetParent(parentTransform);
                        attachmentObject.transform.localScale = new Vector3(1f / modelScale, 1f / modelScale, 1f / modelScale);
                        attachmentObject.transform.localPosition = Vector3.zero;
                        attachmentObject.transform.localEulerAngles = Vector3.zero;
                        PrefabUtility.RecordPrefabInstancePropertyModifications(attachmentObject.GetComponent<Transform>());
                    }
                }
                if (hipBone != string.Empty) {
                    Transform parentTransform = modelCopyObject.transform.FindChildByRecursive(hipBone);
                    if (parentTransform != null) {
                        GameObject attachmentTemplate = (GameObject)AssetDatabase.LoadMainAssetAtPath("Assets" + pathToHipAttachments);
                        GameObject attachmentObject = PrefabUtility.InstantiatePrefab(attachmentTemplate) as GameObject;
                        attachmentObject.transform.SetParent(parentTransform);
                        attachmentObject.transform.localScale = new Vector3(1f / modelScale, 1f / modelScale, 1f / modelScale);
                        attachmentObject.transform.localPosition = Vector3.zero;
                        attachmentObject.transform.localEulerAngles = Vector3.zero;
                        PrefabUtility.RecordPrefabInstancePropertyModifications(attachmentObject.GetComponent<Transform>());
                    }
                }
                if (spineBone != string.Empty) {
                    Transform parentTransform = modelCopyObject.transform.FindChildByRecursive(spineBone);
                    if (parentTransform != null) {
                        GameObject attachmentTemplate = (GameObject)AssetDatabase.LoadMainAssetAtPath("Assets" + pathToSpineAttachments);
                        GameObject attachmentObject = PrefabUtility.InstantiatePrefab(attachmentTemplate) as GameObject;
                        attachmentObject.transform.SetParent(parentTransform);
                        attachmentObject.transform.localScale = new Vector3(1f / modelScale, 1f / modelScale, 1f / modelScale);
                        attachmentObject.transform.localPosition = Vector3.zero;
                        attachmentObject.transform.localEulerAngles = Vector3.zero;
                        PrefabUtility.RecordPrefabInstancePropertyModifications(attachmentObject.GetComponent<Transform>());
                    }
                }

                PrefabUtility.SaveAsPrefabAsset(modelCopyObject, pathToModelCopy);

                // clean up
                PrefabUtility.UnloadPrefabContents(modelCopyObject);

                AssetDatabase.Refresh();
            }

            return modelCopyObject;
        }

        private bool CheckFilesExist() {

            // Check for presence of unit spawn node template
            if (WizardUtilities.CheckFileExists(pathToUnitSpawnNodeTemplate, "Unit Spawn Node Template") == false) {
                return false;
            }

            // Check for presence of default character unit prefab
            if (WizardUtilities.CheckFileExists(pathToDefaultCharacterUnitPrefab, "Default Character Unit Prefab") == false) {
                return false;
            }

            // Check for presence of attachments
            if (WizardUtilities.CheckFileExists(pathToLeftHandAttachment, "Left Hand Attachment Prefab") == false) {
                return false;
            }
            if (WizardUtilities.CheckFileExists(pathToRightHandAttachment, "Right Hand Attachment Prefab") == false) {
                return false;
            }
            if (WizardUtilities.CheckFileExists(pathToLeftArmAttachment, "Left Arm Attachment Prefab") == false) {
                return false;
            }
            if (WizardUtilities.CheckFileExists(pathToHipAttachments, "Hip Attachments Prefab") == false) {
                return false;
            }
            if (WizardUtilities.CheckFileExists(pathToSpineAttachments, "Spine Attachments Prefab") == false) {
                return false;
            }

            return true;
        }

        void OnWizardUpdate() {
            helpString = "Creates a new character unit profile";

            fileSystemGameName = WizardUtilities.GetFileSystemGameName(gameName);
            SetDefaultCharacterName();
            SetDefaultHeadBone();
            SetDefaultLeftHandBone();
            SetDefaultRightHandBone();
            SetDefaultLeftArmBone();
            SetDefaultHipBone();
            SetDefaultSpineBone();
            DetermineModelScale();
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
                //Debug.Log("Highest transform Y value in model : " + highestY);
                highestYTransform = highestY;
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
                    Transform headBoneTransform = characterModel.transform.FindChildByRecursive(defaultHeadBones[i], true, true);
                    if (headBoneTransform != null) {
                        headBone = headBoneTransform.name;
                        break;
                    }
                }
            }
        }

        private void SetDefaultLeftHandBone() {
            if (leftHandBone == string.Empty && characterModel != null) {
                for (int i = 0; i < defaultLeftHandBones.Length; i++) {
                    Transform boneTransform = characterModel.transform.FindChildByRecursive(defaultLeftHandBones[i], true, true);
                    if (boneTransform != null) {
                        leftHandBone = boneTransform.name;
                        break;
                    }
                }
            }
        }

        private void SetDefaultRightHandBone() {
            if (rightHandBone == string.Empty && characterModel != null) {
                for (int i = 0; i < defaultRightHandBones.Length; i++) {
                    Transform boneTransform = characterModel.transform.FindChildByRecursive(defaultRightHandBones[i], true, true);
                    if (boneTransform != null) {
                        rightHandBone = boneTransform.name;
                        break;
                    }
                }
            }
        }

        private void SetDefaultLeftArmBone() {
            if (leftArmBone == string.Empty && characterModel != null) {
                for (int i = 0; i < defaultLeftArmBones.Length; i++) {
                    Transform boneTransform = characterModel.transform.FindChildByRecursive(defaultLeftArmBones[i], true, true);
                    if (boneTransform != null) {
                        leftArmBone = boneTransform.name;
                        break;
                    }
                }
            }
        }

        private void SetDefaultHipBone() {
            if (hipBone == string.Empty && characterModel != null) {
                for (int i = 0; i < defaultHipBones.Length; i++) {
                    Transform boneTransform = characterModel.transform.FindChildByRecursive(defaultHipBones[i], true, true);
                    if (boneTransform != null) {
                        hipBone = boneTransform.name;
                        break;
                    }
                }
            }
        }

        private void SetDefaultSpineBone() {
            if (spineBone == string.Empty && characterModel != null) {
                for (int i = 0; i < defaultSpineBones.Length; i++) {
                    Transform boneTransform = characterModel.transform.FindChildByRecursive(defaultSpineBones[i], true, true);
                    if (boneTransform != null) {
                        spineBone = boneTransform.name;
                        break;
                    }
                }
            }
        }

        private void DetermineModelScale() {
            if (modelScale == 0f && characterModel != null) {
                modelScale = FindScaleByRecursive(characterModel.transform, modelScale);
            }
        }

        private static float FindScaleByRecursive(Transform aParent, float initialScale) {
            //Debug.Log("FindScaleByRecursive(" + aParent.name + ", " + initialScale + ")");
            float result = initialScale;
            if (aParent.localScale.x > initialScale) {
                result = aParent.localScale.x;
            }
            foreach (Transform child in aParent) {
                //Debug.Log("searching " + child.name + " for " + aName);

                float recursiveResult = FindScaleByRecursive(child, result);
                if (recursiveResult > result) {
                    result = recursiveResult;
                }
            }

            //Debug.Log("FindScaleByRecursive(" + aParent.name + ", " + initialScale + ") largest scale was " + result);
            return result;
        }


        string Validate() {

            // check for empty game name
            if (gameName == null || gameName.Trim() == "") {
                return "Game name must not be empty";
            }

            // check for game folder existing
            string gameFileSystemFolder = WizardUtilities.GetGameFileSystemFolder(gameParentFolder, fileSystemGameName);
            if (System.IO.Directory.Exists(gameFileSystemFolder) == false) {
                return "The folder " + gameFileSystemFolder + "does not exist.  Please run the new game wizard first to create the game folder structure";
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

    }

}
