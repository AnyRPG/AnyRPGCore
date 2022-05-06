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
        private string gameParentFolder = "/Games/";

        // path to weapon handle prefab template
        private const string weaponHandleTemplatePath = "/AnyRPG/Core/Templates/Prefabs/WeaponHandle/WeaponHandleTemplate.prefab";
        private const string projectileHandleTemplatePath = "/AnyRPG/Core/Templates/Prefabs/WeaponHandle/ProjectileHandleTemplate.prefab";
        private const string projectileTemplatePath = "/AnyRPG/Core/Templates/Prefabs/WeaponHandle/ProjectileTemplate.prefab";

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

        [Tooltip("If the weapon is not symmetrical, a separate prefab will be made for the left and right hands if AnyHand is selected")]
        public bool asymmetricalWeapon = false;

        [Header("Ranged Properties")]

        [Tooltip("If this is a ranged weapon, the name of the projectile")]
        public string projectileName = string.Empty;

        [Tooltip("The prefab of the projectile for the ranged weapon")]
        public GameObject projectilePrefab = null;

        [Tooltip("If true, the projectile will be shown during ranged attack animations")]
        public bool useProjectileInAnimations = false;

        [Tooltip("If true, the projectile will be shown when travelling to the target")]
        public bool useProjectileInAbilities = false;

        // global variables
        string newWeaponHandleAssetPath = string.Empty;


        [MenuItem("Tools/AnyRPG/Wizard/New Weapon Wizard")]
        public static void CreateWizard() {
            ScriptableWizard.DisplayWizard<NewWeaponWizard>("New Weapon Wizard", "Create");
        }

        void OnEnable() {

            systemConfigurationManager = WizardUtilities.GetSystemConfigurationManager();
            gameName = WizardUtilities.GetGameName(systemConfigurationManager);
            gameParentFolder = WizardUtilities.GetGameParentFolder(systemConfigurationManager, gameName);
        }

        void OnWizardCreate() {

            EditorUtility.DisplayProgressBar("New Weapon Wizard", "Checking parameters...", 0.1f);

            try {
                if (RunWizard() == false) {
                    PrintErrorMesage();
                    return;
                }
            } catch {
                PrintErrorMesage();
                throw;
            }

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("New Weapon Wizard", "New Weapon Wizard Complete! The handle prefab can be found at " + newWeaponHandleAssetPath, "OK");

        }

        private void PrintErrorMesage() {
            // do nothing
            Debug.LogWarning("An error was detected while running wizard.  See console log for details");
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Template Content Wizard",
                "Error!  See console log for details",
                "OK");
        }

        private bool RunWizard() {

            // check that templates exist
            if (CheckRequiredTemplatesExist() == false) {
                return false;
            }

            CreateWeaponHandle(gameName, weaponName, weaponPrefab, icon);
            return true;
        }

        private bool CheckRequiredTemplatesExist() {

            // Check for presence of weapon handle prefab template
            if (WizardUtilities.CheckFileExists(weaponHandleTemplatePath, "Weapon Handle Prefab Template") == false) {
                return false;
            }

            // check for presence of projectile handle prefab template
            if (WizardUtilities.CheckFileExists(projectileHandleTemplatePath, "Projectile Handle Prefab Template") == false) {
                return false;
            }

            // check for presence of projectile prefab template
            if (WizardUtilities.CheckFileExists(projectileTemplatePath, "Projectile Prefab Template") == false) {
                return false;
            }


            return true;
        }


        public string CreateWeaponHandle(string gameName, string weaponName, GameObject weaponPrefab, Sprite icon) {

            string fileSystemGameName = WizardUtilities.GetFileSystemGameName(gameName);

            // Determine root game folder
            string gameFileSystemFolder = WizardUtilities.GetGameFileSystemFolder(gameParentFolder, fileSystemGameName);

            // create resources folders if they doesn't already exist
            EditorUtility.DisplayProgressBar("New Weapon Wizard", "Create Resources Folders If Necessary...", 0.2f);
            WizardUtilities.CreateFolderIfNotExists(gameFileSystemFolder + "/Resources/" + fileSystemGameName + "/Item/Equipment/Weapon");
            WizardUtilities.CreateFolderIfNotExists(gameFileSystemFolder + "/Resources/" + fileSystemGameName + "/PrefabProfile/Weapon");

            // create prefab folder
            EditorUtility.DisplayProgressBar("New Weapon Wizard", "Create Prefab Folders If Necessary...", 0.3f);
            WizardUtilities.CreateFolderIfNotExists(gameFileSystemFolder + "/Prefab/Handle/Weapon");

            // create the weapon handle prefab
            EditorUtility.DisplayProgressBar("New Weapon Wizard", "Create Weapon Handle Prefab...", 0.4f);
            List<string> newWeaponHandleAssetPaths = new List<string>();
            string weaponFileSystemName = WizardUtilities.GetScriptableObjectFileSystemName(weaponName);

            if (weaponSlotType == WeaponSlotType.AnyHand && asymmetricalWeapon == true) {
                newWeaponHandleAssetPaths.Add(CreateWeaponHandleObjects(fileSystemGameName, weaponFileSystemName, "Main Hand"));
                newWeaponHandleAssetPaths.Add(CreateWeaponHandleObjects(fileSystemGameName, weaponFileSystemName, "Off Hand"));
            } else {
                newWeaponHandleAssetPaths.Add(CreateWeaponHandleObjects(fileSystemGameName, weaponFileSystemName, ""));
            }

            // create projectile prefabs
            if (projectilePrefab != null && projectileName != string.Empty) {
                EditorUtility.DisplayProgressBar("New Weapon Wizard", "Create Projectile Prefabs...", 0.5f);
                CreateProjectileHandleObjects(fileSystemGameName, projectileHandleTemplatePath, projectileName, "", "Handle", "");
                CreateProjectileHandleObjects(fileSystemGameName, projectileTemplatePath, projectileName, "Projectile", "", "SpellEffects");
            }

            // create item Scriptable Object
            EditorUtility.DisplayProgressBar("New Weapon Wizard", "Configuring Weapon Item...", 0.6f);
            CreateWeaponScriptableObjects(fileSystemGameName, weaponName, icon, newWeaponHandleAssetPaths, weaponSlotType, weaponType);

            // install weapon skill
            EditorUtility.DisplayProgressBar("New Weapon Wizard", "Installing Weapon Skill...", 0.7f);
            TemplateContentWizard.RunWizard(fileSystemGameName, gameParentFolder, new List<ScriptableContentTemplate>() { weaponType.WeaponSkillContentTemplate }, false, false);

            AssetDatabase.Refresh();

            newWeaponHandleAssetPath = newWeaponHandleAssetPaths[0];

            return newWeaponHandleAssetPaths[0];
        }

        private string CreateWeaponHandleObjects(string fileSystemGameName, string weaponFileSystemName, string suffix) {
            // copy the weapon handle prefab template
            string weaponHandleTemplateAssetPath = "Assets" + weaponHandleTemplatePath;
            string newWeaponHandleAssetPath = "Assets" + gameParentFolder + fileSystemGameName + "/Prefab/Handle/Weapon/" + WizardUtilities.GetScriptableObjectFileSystemName(weaponName) + WizardUtilities.GetScriptableObjectFileSystemName(suffix) + "Handle.prefab";
            Debug.Log("Copying Resource from '" + weaponHandleTemplateAssetPath + "' to '" + newWeaponHandleAssetPath + "'");
            AssetDatabase.CopyAsset(weaponHandleTemplateAssetPath, newWeaponHandleAssetPath);
            AssetDatabase.Refresh();


            // add weapon to weapon handle
            GameObject weaponHandleObject = PrefabUtility.LoadPrefabContents(newWeaponHandleAssetPath);
            if (weaponHandleObject != null) {
                GameObject weaponObject = PrefabUtility.InstantiatePrefab(weaponPrefab) as GameObject;
                weaponObject.transform.SetParent(weaponHandleObject.transform);
                WizardUtilities.SetLayerRecursive(weaponObject, LayerMask.NameToLayer("Equipment"));

                PrefabUtility.RecordPrefabInstancePropertyModifications(weaponObject.GetComponent<Transform>());
                PrefabUtility.SaveAsPrefabAsset(weaponHandleObject, newWeaponHandleAssetPath);

                // clean up
                PrefabUtility.UnloadPrefabContents(weaponHandleObject);

                AssetDatabase.Refresh();
            }

            // create the prefab profile for the newly created handle
            CreatePrefabProfile(fileSystemGameName, newWeaponHandleAssetPath, weaponName, suffix);

            return newWeaponHandleAssetPath;
        }

        private string CreateProjectileHandleObjects(string fileSystemGameName, string itemHandleTemplatePath, string itemName, string itemSuffix, string itemFileSystemSuffix, string layerName) {

            // copy the projectile handle prefab template
            string itemFileSystemName = WizardUtilities.GetScriptableObjectFileSystemName(itemName);
            string itemHandleTemplateAssetPath = "Assets" + itemHandleTemplatePath;
            string newItemHandleAssetPath = "Assets" + gameParentFolder + fileSystemGameName + "/Prefab/Handle/Weapon/" + itemFileSystemName + WizardUtilities.GetScriptableObjectFileSystemName(itemSuffix) + itemFileSystemSuffix + ".prefab";
            Debug.Log("Copying Resource from '" + itemHandleTemplateAssetPath + "' to '" + newItemHandleAssetPath + "'");
            AssetDatabase.CopyAsset(itemHandleTemplateAssetPath, newItemHandleAssetPath);
            AssetDatabase.Refresh();


            // add projectile to projectile handle
            GameObject itemHandleObject = PrefabUtility.LoadPrefabContents(newItemHandleAssetPath);
            if (itemHandleObject != null) {
                GameObject itemObject = PrefabUtility.InstantiatePrefab(projectilePrefab) as GameObject;
                itemObject.transform.SetParent(itemHandleObject.transform);
                if (layerName != string.Empty) {
                    WizardUtilities.SetLayerRecursive(itemObject, LayerMask.NameToLayer(layerName));
                }

                PrefabUtility.RecordPrefabInstancePropertyModifications(itemObject.GetComponent<Transform>());
                PrefabUtility.SaveAsPrefabAsset(itemHandleObject, newItemHandleAssetPath);

                // clean up
                PrefabUtility.UnloadPrefabContents(itemHandleObject);

                AssetDatabase.Refresh();
            }

            // create the prefab profile for the newly created handle
            CreatePrefabProfile(fileSystemGameName, newItemHandleAssetPath, itemName, itemSuffix);

            return newItemHandleAssetPath;
        }

        private void CreatePrefabProfile(string fileSystemGameName, string itemHandleAssetPath, string itemName, string suffix) {

            string itemFileSystemName = WizardUtilities.GetScriptableObjectFileSystemName(itemName);
            GameObject itemHandleObject = (GameObject)AssetDatabase.LoadMainAssetAtPath(itemHandleAssetPath);

            // create weapon handle prefab profile
            PrefabProfile prefabProfile = ScriptableObject.CreateInstance("PrefabProfile") as PrefabProfile;
            prefabProfile.ResourceName = itemName + (suffix == "" ? suffix : " " + suffix);
            prefabProfile.Prefab = itemHandleObject;

            string scriptableObjectPath = "Assets" + gameParentFolder + fileSystemGameName + "/Resources/" + fileSystemGameName + "/PrefabProfile/Weapon/" + itemFileSystemName + WizardUtilities.GetScriptableObjectFileSystemName(suffix) + "Prefab.asset";
            AssetDatabase.CreateAsset(prefabProfile, scriptableObjectPath);
        }

        private void CreateWeaponScriptableObjects(string fileSystemGameName, string weaponName, Sprite icon, List<string> weaponHandleAssetPaths, WeaponSlotType weaponSlotType, WeaponTypeConfigTemplate weaponType) {

            string weaponFileSystemName = WizardUtilities.GetScriptableObjectFileSystemName(weaponName);

            /*
            if (weaponSlotType == WeaponSlotType.AnyHand && asymmetricalWeapon == true) {
                CreatePrefabProfile(fileSystemGameName, weaponHandleAssetPaths[0], weaponFileSystemName);
                CreatePrefabProfile(fileSystemGameName, weaponHandleAssetPaths[1], weaponFileSystemName);
            } else {
                CreatePrefabProfile(fileSystemGameName, weaponHandleAssetPaths[0], weaponFileSystemName);
            }
            */


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
                    if (weaponSlotType == WeaponSlotType.AnyHand && asymmetricalWeapon == true) {
                        weaponItem.HoldableObjectList[0].AttachmentNodes[0].HoldableObjectName = weaponName + " Main Hand";
                        weaponItem.HoldableObjectList[0].AttachmentNodes[1].HoldableObjectName = weaponName + " Off Hand";
                    } else {
                        foreach (AttachmentNode attachmentNode in weaponItem.HoldableObjectList[0].AttachmentNodes) {
                            //foreach (AttachmentNode attachmentNode in holdableObjectAttachment.AttachmentNodes) {
                            attachmentNode.HoldableObjectName = weaponName;
                        }
                    }
                    // this is where you would do a quiver/ammo pouch as [1]
                    //}
                    break;
                }
            }

            if (projectilePrefab != null && projectileName != string.Empty) {
                weaponItem.UseWeaponTypeObjects = false;
                if (useProjectileInAnimations == true) {
                    AbilityAttachmentNode animationAttachmentNode = new AbilityAttachmentNode();
                    animationAttachmentNode.HoldableObjectName = projectileName;
                    animationAttachmentNode.AttachmentName = "Right Hand";
                    animationAttachmentNode.UseUniversalAttachment = true;
                    weaponItem.AbilityAnimationObjectList.Add(animationAttachmentNode);
                }
                if (useProjectileInAbilities == true) {
                    AbilityAttachmentNode animationAttachmentNode = new AbilityAttachmentNode();
                    animationAttachmentNode.HoldableObjectName = projectileName + " Projectile";
                    animationAttachmentNode.AttachmentName = "Right Hand";
                    animationAttachmentNode.UseUniversalAttachment = true;
                    weaponItem.AbilityObjectList.Add(animationAttachmentNode);
                }

            }

            string scriptableObjectPath = "Assets" + gameParentFolder + fileSystemGameName + "/Resources/" + fileSystemGameName + "/Item/Equipment/Weapon/" + weaponFileSystemName + "Item.asset";
            AssetDatabase.CreateAsset(weaponItem, scriptableObjectPath);
        }

        void OnWizardUpdate() {
            helpString = "Creates a new weapon handle prefab and weapon item";
            SetWeaponName();
            SetProjectileName();
            string fileSystemGameName = WizardUtilities.GetFileSystemGameName(gameName);
            errorString = Validate(fileSystemGameName);
            isValid = (errorString == null || errorString == "");
        }

        private void SetWeaponName() {
            if (weaponName == string.Empty && weaponPrefab != null) {
                weaponName = weaponPrefab.name;
            }
        }

        private void SetProjectileName() {
            if (projectileName == string.Empty && projectilePrefab != null) {
                projectileName = projectilePrefab.name;
            }
        }

        string Validate(string fileSystemGameName) {

            if (fileSystemGameName == "") {
                return "Game name must not be empty";
            }

            // check for game folder existing
            string newGameFolder = WizardUtilities.GetGameFileSystemFolder(gameParentFolder, fileSystemGameName);
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
