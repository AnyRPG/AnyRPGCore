using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace AnyRPG {
    public class NewWeaponWizard : ScriptableWizard {

        SystemConfigurationManager systemConfigurationManager = null;

        // Will be a subfolder of Application.dataPath and should start with "/"
        private const string gameParentFolder = "/Games/";

        // path to weapon handle prefab template
        private const string weaponHandleTemplatePath = "/AnyRPG/Core/Templates/Prefabs/WeaponHandle/WeaponHandleTemplate.prefab";

        // user modified variables
        [Header("Game")]
        public string gameName = string.Empty;

        [Header("Prefab")]
        public GameObject weaponPrefab = null;

        [Header("Scriptable Object")]
        public string weaponName = "";
        public Sprite icon = null;
        public WeaponSlotType weaponSlotType = WeaponSlotType.MainHandOnly;
        public WeaponTypeConfigTemplate weaponType = null;


        [MenuItem("Tools/AnyRPG/Wizard/New Weapon Wizard")]
        public static void CreateWizard() {
            ScriptableWizard.DisplayWizard<NewWeaponWizard>("New Weapon Wizard", "Create");
        }

        void OnEnable() {

            systemConfigurationManager = WizardUtilities.GetSystemConfigurationManager();
            gameName = WizardUtilities.GetGameName(systemConfigurationManager);
        }

        void OnWizardCreate() {

            EditorUtility.DisplayProgressBar("New Weapon Wizard", "Checking parameters...", 0.1f);

            string fileSystemGameName = WizardUtilities.GetFileSystemGameName(gameName);

            // Check for presence of weapon handle prefab template
            if (CheckWeaponHandlePrefabTemplateExists(fileSystemGameName) == false) {
                return;
            }

            string newWeaponHandleAssetPath = CreateWeaponHandle(gameName, weaponName, weaponPrefab, icon);

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("New Weapon Wizard", "New Weapon Wizard Complete! The handle prefab can be found at " + newWeaponHandleAssetPath, "OK");

        }

        private static bool CheckWeaponHandlePrefabTemplateExists(string fileSystemGameName) {

            string templateAssetPath = "Assets" + weaponHandleTemplatePath;
            string templateFileSystemPath = Application.dataPath + weaponHandleTemplatePath;
            if (System.IO.File.Exists(templateFileSystemPath) == false) {
                WizardUtilities.ShowError("Missing Weapon Handle Prefab Template at " + templateAssetPath + ".  Aborting...");
                return false;
            }

            return true;
        }

        public string CreateWeaponHandle(string gameName, string weaponName, GameObject weaponPrefab, Sprite icon) {

            string fileSystemGameName = WizardUtilities.GetFileSystemGameName(gameName);

            // Determine root game folder
            string gameFileSystemFolder = WizardUtilities.GetGameFolder(gameParentFolder, fileSystemGameName);

            // create resources folders if they doesn't already exist
            EditorUtility.DisplayProgressBar("New Weapon Wizard", "Create Resources Folders If Necessary...", 0.2f);
            WizardUtilities.CreateFolderIfNotExists(gameFileSystemFolder + "/Resources/" + fileSystemGameName + "/Item/Equipment/Weapon");
            WizardUtilities.CreateFolderIfNotExists(gameFileSystemFolder + "/Resources/" + fileSystemGameName + "/PrefabProfile/Weapon");

            // create prefab folder
            EditorUtility.DisplayProgressBar("New Weapon Wizard", "Create Prefab Folders If Necessary...", 0.3f);
            WizardUtilities.CreateFolderIfNotExists(gameFileSystemFolder + "/Prefab/Handle/Weapon");

            // copy the weapon handle prefab template
            EditorUtility.DisplayProgressBar("New Weapon Wizard", "Create Weapon Handle Prefab Template...", 0.4f);
            string weaponHandleTemplateAssetPath = "Assets" + weaponHandleTemplatePath;
            string newWeaponHandleAssetPath = "Assets" + gameParentFolder + fileSystemGameName + "/Prefab/Handle/Weapon/" + WizardUtilities.GetScriptableObjectFileSystemName(weaponName) + "Handle.prefab";
            Debug.Log("Copying Resource from '" + weaponHandleTemplateAssetPath + "' to '" + newWeaponHandleAssetPath + "'");
            AssetDatabase.CopyAsset(weaponHandleTemplateAssetPath, newWeaponHandleAssetPath);
            AssetDatabase.Refresh();


            // add weapon to weapon handle
            EditorUtility.DisplayProgressBar("New Weapon Wizard", "Nest Weapon Prefab in Handle Prefab...", 0.5f);
            GameObject weaponHandleObject = PrefabUtility.LoadPrefabContents(newWeaponHandleAssetPath);
            if (weaponHandleObject != null) {
                GameObject weaponObject = PrefabUtility.InstantiatePrefab(weaponPrefab) as GameObject;
                weaponObject.transform.SetParent(weaponHandleObject.transform);

                PrefabUtility.RecordPrefabInstancePropertyModifications(weaponObject.GetComponent<Transform>());
                PrefabUtility.SaveAsPrefabAsset(weaponHandleObject, newWeaponHandleAssetPath);

                // clean up
                PrefabUtility.UnloadPrefabContents(weaponHandleObject);

                AssetDatabase.Refresh();
            }

            // create item Scriptable Object
            EditorUtility.DisplayProgressBar("New Weapon Wizard", "Configuring Weapon Item...", 0.6f);
            CreateWeaponScriptableObjects(fileSystemGameName, weaponName, icon, newWeaponHandleAssetPath, weaponSlotType, weaponType);

            // install weapon skill
            EditorUtility.DisplayProgressBar("New Weapon Wizard", "Installing Weapon Skill...", 0.7f);
            TemplateContentWizard.RunWizard(fileSystemGameName, gameParentFolder, new List<ScriptableContentTemplate>() { weaponType.WeaponSkillContentTemplate }, false, false);

            AssetDatabase.Refresh();

            return newWeaponHandleAssetPath;
        }


        private void CreateWeaponScriptableObjects(string fileSystemGameName, string weaponName, Sprite icon, string weaponHandleAssetPath, WeaponSlotType weaponSlotType, WeaponTypeConfigTemplate weaponType) {

            string weaponFileSystemName = WizardUtilities.GetScriptableObjectFileSystemName(weaponName);
            GameObject weaponHandleObject = (GameObject)AssetDatabase.LoadMainAssetAtPath(weaponHandleAssetPath);

            // create weapon handle prefab profile
            PrefabProfile prefabProfile = ScriptableObject.CreateInstance("PrefabProfile") as PrefabProfile;
            prefabProfile.ResourceName = weaponName;
            prefabProfile.Prefab = weaponHandleObject;
            
            string scriptableObjectPath = "Assets" + gameParentFolder + fileSystemGameName + "/Resources/" + fileSystemGameName + "/PrefabProfile/Weapon/" + weaponFileSystemName + "Prefab.asset";
            AssetDatabase.CreateAsset(prefabProfile, scriptableObjectPath);

            // create weapon item
            Weapon weaponItem = ScriptableObject.CreateInstance("Weapon") as Weapon;
            weaponItem.ResourceName = weaponName;
            weaponItem.Icon = icon;
            weaponItem.WeaponType = weaponType.WeaponType;
            switch (weaponSlotType) {
                case WeaponSlotType.MainHandOnly:
                    weaponItem.EquipmentSlotTypeName = "Main Hand";
                    break;
                case WeaponSlotType.OffHandOnly:
                    weaponItem.EquipmentSlotTypeName = "Off Hand";
                    break;
                case WeaponSlotType.AnyHand:
                    weaponItem.EquipmentSlotTypeName = "One Hand";
                    break;
                case WeaponSlotType.TwoHand:
                    weaponItem.EquipmentSlotTypeName = "Two Hand";
                    break;
                default:
                    break;
            }

            foreach (WeaponSlotConfig weaponSlotConfig in weaponType.WeaponSlotConfigs) {
                if (weaponSlotConfig.weaponSlotType == weaponSlotType) {
                    weaponItem.HoldableObjectList = weaponSlotConfig.holdableObjectList;
                    //foreach (HoldableObjectAttachment holdableObjectAttachment in weaponItem.HoldableObjectList) {
                        foreach (AttachmentNode attachmentNode in weaponItem.HoldableObjectList[0].AttachmentNodes) {
                            //foreach (AttachmentNode attachmentNode in holdableObjectAttachment.AttachmentNodes) {
                            attachmentNode.HoldableObjectName = weaponName;
                        }
                        // this is where you would do a quiver/ammo pouch as [1]
                    //}
                    break;
                }
            }

            scriptableObjectPath = "Assets" + gameParentFolder + fileSystemGameName + "/Resources/" + fileSystemGameName + "/Item/Equipment/Weapon/" + weaponFileSystemName + "Item.asset";
            AssetDatabase.CreateAsset(weaponItem, scriptableObjectPath);


        }

        void OnWizardUpdate() {
            helpString = "Creates a new weapon handle prefab and weapon item";
            SetWeaponName();
            string fileSystemGameName = WizardUtilities.GetFileSystemGameName(gameName);
            errorString = Validate(fileSystemGameName);
            isValid = (errorString == null || errorString == "");
        }

        private void SetWeaponName() {
            if (weaponName == string.Empty && weaponPrefab != null) {
                weaponName = weaponPrefab.name;
            }
        }

        string Validate(string fileSystemGameName) {

            if (fileSystemGameName == "") {
                return "Game name must not be empty";
            }

            // check for game folder existing
            string newGameFolder = WizardUtilities.GetGameFolder(gameParentFolder, fileSystemGameName);
            if (System.IO.Directory.Exists(newGameFolder) == false) {
                return "The folder " + newGameFolder + "does not exist.  Please run the new game wizard first to create the game folder structure";
            }

            // check that weapon name is not empty
            if (weaponName == "") {
                return "Weapon Name must not be empty";
            }

            // check that weapon name is not empty
            if (weaponPrefab == null) {
                return "Weapon Prefab must be assigned";
            }

            if (weaponType == null) {
                return "Weapon Type must be assigned";
            }

            return null;
        }


        /*
        protected override bool DrawWizardGUI() {
            //return base.DrawWizardGUI();

            //NewGameWizard myScript = target as NewGameWizard;

            EditorGUILayout.LabelField("Game Options", EditorStyles.boldLabel);

            gameName = EditorGUILayout.TextField("Game Name", gameName);

            EditorGUILayout.LabelField("Scene Options", EditorStyles.boldLabel);

            weaponName = EditorGUILayout.TextField("Scene Name", weaponName);
            copyExistingScene = EditorGUILayout.Toggle("Copy Existing Scene", copyExistingScene);

            if (copyExistingScene) {
                existingScene = EditorGUILayout.ObjectField("Existing Scene", existingScene, typeof(SceneAsset), false) as SceneAsset;
            }

            newSceneAmbientSounds = EditorGUILayout.ObjectField("First Scene Ambient Sounds", newSceneAmbientSounds, typeof(AudioClip), false) as AudioClip;
            newSceneMusic = EditorGUILayout.ObjectField("First Scene Music", newSceneMusic, typeof(AudioClip), false) as AudioClip;

            return true;
        }
        */
        
    }

   


}
