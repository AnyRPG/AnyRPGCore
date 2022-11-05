using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace AnyRPG.Editor {
    public class AnimationProfileWizard : ScriptableWizard {

        // a reference to the systemConfigurationManager found in the currently open scene, for automatic determination of the game name
        private SystemConfigurationManager systemConfigurationManager = null;

        // Will be a subfolder of Application.dataPath and should start with "/"
        private string gameParentFolder = "/Games/";

        // the used file path name for the game
        private string fileSystemGameName = string.Empty;

        // the used asset path for the Animation Profile
        private string scriptableObjectAssetPath = string.Empty;

        // user modified variables
        [Header("Game")]
        public string gameName = string.Empty;

        [Header("Details")]

        [Tooltip("The name of the animation profile that can be used from abilities, weapons, unit prefab properties, etc")]
        public string profileName = "New";

        [Header("Animation Auto Assign")]

        [Tooltip("Idle, jump loop, and movement animations will be assigned to this section")]
        public AssignAnimationType assignPriority = AssignAnimationType.OutOfCombat;

        [Tooltip("Drag clips here and the wizard will attempt to auto-assign them to the animation properties below based on their names.")]
        public List<AnimationClip> assignClips = new List<AnimationClip>();
        
        [Header("Animation Properties")]

        [Tooltip("If true, Hit events will be added to the attack animations if they don't already exist")]
        public bool addHitEvents = true;

        [Tooltip("Lists of animations to perform for specific actions")]
        public AnimationProps animations = new AnimationProps();

        [MenuItem("Tools/AnyRPG/Wizard/Animation Profile Wizard")]
        public static void CreateWizard() {
            ScriptableWizard.DisplayWizard<AnimationProfileWizard>("Animation Profile Wizard", "Create");
        }

        void OnEnable() {

            systemConfigurationManager = WizardUtilities.GetSystemConfigurationManager();
            gameName = WizardUtilities.GetGameName(systemConfigurationManager);
            gameParentFolder = WizardUtilities.GetGameParentFolder(systemConfigurationManager, gameName);
        }

        void OnWizardCreate() {

            try {
                CreateProfile();
            } catch {
                Debug.LogWarning("Error detected while running wizard");

                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Animation Profile Wizard", "Animation Profile Wizard encountered an error.  Check the console log for details.", "OK");
                throw;
            }

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Animation Profile Wizard", "Animation Profile Wizard Complete! The animation profile can be found at " + scriptableObjectAssetPath, "OK");

        }

        private void CreateProfile() {
            EditorUtility.DisplayProgressBar("Animation Profile Wizard", "Checking parameters...", 0.1f);

            // check for presence of template prefabs and resources
            if (CheckFilesExist() == false) {
                return;
            }

            EditorUtility.DisplayProgressBar("Animation Profile Wizard", "Creating Resources Subfolder...", 0.2f);
            // Create root game folder
            string gameFileSystemFolder = WizardUtilities.GetGameFileSystemFolder(gameParentFolder, fileSystemGameName);
            string resourcesFolder = gameFileSystemFolder + "/Resources/" + fileSystemGameName + "/AnimationProfile";

            // create resources folder
            WizardUtilities.CreateFolderIfNotExists(resourcesFolder);

            AssetDatabase.Refresh();
           

            EditorUtility.DisplayProgressBar("Animation Profile Wizard", "Creating Animation Profile...", 0.3f);
            AnimationProfile asset = ScriptableObject.CreateInstance("AnimationProfile") as AnimationProfile;

            EditorUtility.DisplayProgressBar("Animation Profile Wizard", "Configuring Animation Profile...", 0.4f);
            // setup animation profile properties
            asset.ResourceName = profileName;

            // setup animation properties
            asset.AnimationProps = animations;

            EditorUtility.DisplayProgressBar("Animation Profile Wizard", "Saving Animation Profile...", 0.5f);

            scriptableObjectAssetPath = "Assets" + gameParentFolder + fileSystemGameName + "/Resources/" + fileSystemGameName + "/AnimationProfile/" + WizardUtilities.GetScriptableObjectFileSystemName(profileName) + "AnimationProfile.asset";
            AssetDatabase.CreateAsset(asset, scriptableObjectAssetPath);

            AssetDatabase.Refresh();

            // add hit events
            EditorUtility.DisplayProgressBar("Animation Profile Wizard", "Optionally adding hit events to attack animations...", 0.6f);
            if (addHitEvents == true) {
                AddHitEvents(animations);
            }
        }

        public static void AddHitEvents(AnimationProps animationProps) {
            bool hitEventExists = false;
            foreach (AnimationClip animationClip in animationProps.AttackClips) {
                hitEventExists = false;

                ModelImporter modelImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(animationClip)) as ModelImporter;
                SerializedObject serializedObject = new SerializedObject(modelImporter);
                SerializedProperty clipAnimations = serializedObject.FindProperty("m_ClipAnimations");
                AnimationClipInfoProperties clipInfoProperties = GetClipInfoProperties(clipAnimations, animationClip.name);
                AnimationEvent[] animationEvents = null;
                if (clipInfoProperties != null) {
                    // this should never be null because we actually got the name of the animation clip from a serialized property in the first place
                    animationEvents = clipInfoProperties.GetEvents();
                    Debug.Log("Event array for the animation clip named " + animationClip.name + " has " + animationEvents.Length + " events");
                    foreach (AnimationEvent animationEvent in animationEvents) {
                        if (animationEvent.functionName == "Hit") {
                            hitEventExists = true;
                            Debug.Log("Hit() event exists on animation clip named " + animationClip.name + " at " + animationEvent.time);
                            break;
                        }
                    }
                }

                if (hitEventExists == false) {
                    AnimationEvent newEvent = new AnimationEvent();
                    // by default, put the hit in the middle of the animation.  This value is a scaled float with range 0 - 1
                    newEvent.time = 0.5f;
                    newEvent.functionName = "Hit";
                    Debug.Log("Adding Hit() event to animation clip named " + animationClip.name + " at " + newEvent.time);
                    animationEvents = animationEvents.Append(newEvent).ToArray();
                    clipInfoProperties.SetEvents(animationEvents);
                    serializedObject.ApplyModifiedProperties();
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(animationClip));

                    AssetDatabase.Refresh();
                }
            }
        }

        private static AnimationClipInfoProperties GetClipInfoProperties(SerializedProperty clipAnimations, string animationClipName) {
            for (int i = 0; i < clipAnimations.arraySize; i++) {
                AnimationClipInfoProperties clipInfoProperties = new AnimationClipInfoProperties(clipAnimations.GetArrayElementAtIndex(i));
                if (clipInfoProperties.name == animationClipName) {
                    return clipInfoProperties;
                }
            }
            return null;
        }

        private bool CheckFilesExist() {

            // Check for presence of unit spawn node template
            /*
            if (WizardUtilities.CheckFileExists(pathToUnitSpawnNodeTemplate, "Unit Spawn Node Template") == false) {
                return false;
            }
            */

            return true;
        }

        void OnWizardUpdate() {
            helpString = "Creates a new animation profile";

            fileSystemGameName = WizardUtilities.GetFileSystemGameName(gameName);
            AutoAssignAnimations(assignPriority, animations, assignClips);

            errorString = Validate();
            isValid = (errorString == null || errorString == "");
        }

        public static void AutoAssignAnimations(AssignAnimationType assignAnimationType, AnimationProps animationProps, List<AnimationClip> assignClips) {
            if (assignClips.Count > 0) {
                while (assignClips.Count > 0) {
                    AssignAnimation(assignAnimationType, animationProps, assignClips[0]);
                    assignClips.RemoveAt(0);
                }
            }
        }

        private static void AssignAnimation(AssignAnimationType assignAnimationType, AnimationProps animationProps, AnimationClip animationClip) {
            
            // attempt attack clips
            if (animationClip.name.ToLowerInvariant().Contains("attack") == true) {
                if (animationProps.AttackClips.Contains(animationClip) == false) {
                    animationProps.AttackClips.Add(animationClip);
                    return;
                }
            }

            // attempt death clip
            if (animationProps.DeathClip == null
                && (
                    animationClip.name.ToLowerInvariant().Contains("death")
                    || animationClip.name.ToLowerInvariant().Contains("die")
                    || animationClip.name.ToLowerInvariant().Contains("knockdown")
                    )
                ) {
                animationProps.DeathClip = animationClip;
                return;
            }

            // attempt revive clip
            if (animationProps.ReviveClip == null
                && (animationClip.name.ToLowerInvariant().Contains("getup") || animationClip.name.ToLowerInvariant().Contains("revive"))) {
                animationProps.ReviveClip = animationClip;
                return;
            }


            if (assignAnimationType == AssignAnimationType.OutOfCombat) {
                AssignOutOfCombatAnimation(animationProps, animationClip);
            } else {
                AssignInCombatAnimation(animationProps, animationClip);
            }

        }

        private static void AssignOutOfCombatAnimation(AnimationProps animationProps, AnimationClip animationClip) {
            
            // attemp jump clip
            if (animationProps.JumpClip == null
                && animationClip.name.ToLowerInvariant().Contains("jump")) {
                animationProps.JumpClip = animationClip;
                return;
            }

            // attemp fall clip
            if (animationProps.FallClip == null
                && animationClip.name.ToLowerInvariant().Contains("fall")) {
                animationProps.FallClip = animationClip;
                return;
            }

            // attemp land clip
            if (animationProps.LandClip == null
                && animationClip.name.ToLowerInvariant().Contains("land")) {
                animationProps.LandClip = animationClip;
                return;
            }

            // attemp idle clip
            if (animationProps.IdleClip == null
                && animationClip.name.ToLowerInvariant().Contains("idle")) {
                animationProps.IdleClip = animationClip;
                return;
            }

            // attemp walk clip
            if (animationProps.WalkClip == null
                && animationClip.name.ToLowerInvariant().Contains("walk") == true
                && animationClip.name.ToLowerInvariant().Contains("back") == false) {
                animationProps.WalkClip = animationClip;
                return;
            }

            // attemp walk back clip
            if (animationProps.WalkBackClip == null
                && animationClip.name.ToLowerInvariant().Contains("walk") == true
                && animationClip.name.ToLowerInvariant().Contains("back") == true) {
                animationProps.WalkBackClip = animationClip;
                return;
            }

            // attemp run clip
            if (animationProps.RunClip == null
                && animationClip.name.ToLowerInvariant().Contains("run") == true
                && animationClip.name.ToLowerInvariant().Contains("back") == false) {
                animationProps.RunClip = animationClip;
                return;
            }

            // attemp run back clip
            if (animationProps.RunBackClip == null
                && animationClip.name.ToLowerInvariant().Contains("run") == true
                && animationClip.name.ToLowerInvariant().Contains("back") == true) {
                animationProps.RunBackClip = animationClip;
                return;
            }

            // attemp strafe back left clip
            if (animationProps.StrafeBackLeftClip == null
                && animationClip.name.ToLowerInvariant().Contains("strafe") == true
                && animationClip.name.ToLowerInvariant().Contains("back") == true
                && animationClip.name.ToLowerInvariant().Contains("left") == true) {
                animationProps.StrafeBackLeftClip = animationClip;
                return;
            }

            // attemp strafe back right clip
            if (animationProps.StrafeBackRightClip == null
                && animationClip.name.ToLowerInvariant().Contains("strafe") == true
                && animationClip.name.ToLowerInvariant().Contains("back") == true
                && animationClip.name.ToLowerInvariant().Contains("right") == true) {
                animationProps.StrafeBackRightClip = animationClip;
                return;
            }

            // attemp strafe forward left clip
            if (animationProps.StrafeForwardLeftClip == null
                && animationClip.name.ToLowerInvariant().Contains("strafe") == true
                && animationClip.name.ToLowerInvariant().Contains("forward") == true
                && animationClip.name.ToLowerInvariant().Contains("left") == true) {
                animationProps.StrafeForwardLeftClip = animationClip;
                return;
            }

            // attemp strafe forward right clip
            if (animationProps.StrafeForwardRightClip == null
                && animationClip.name.ToLowerInvariant().Contains("strafe") == true
                && animationClip.name.ToLowerInvariant().Contains("forward") == true
                && animationClip.name.ToLowerInvariant().Contains("right") == true) {
                animationProps.StrafeForwardRightClip = animationClip;
                return;
            }

            // attemp strafe left clip
            if (animationProps.StrafeLeftClip == null
                && animationClip.name.ToLowerInvariant().Contains("strafe") == true
                && animationClip.name.ToLowerInvariant().Contains("forward") == false
                && animationClip.name.ToLowerInvariant().Contains("back") == false
                && animationClip.name.ToLowerInvariant().Contains("left") == true) {
                animationProps.StrafeLeftClip = animationClip;
                return;
            }

            // attemp strafe right clip
            if (animationProps.StrafeRightClip == null
                && animationClip.name.ToLowerInvariant().Contains("strafe") == true
                && animationClip.name.ToLowerInvariant().Contains("forward") == false
                && animationClip.name.ToLowerInvariant().Contains("back") == false
                && animationClip.name.ToLowerInvariant().Contains("right") == true) {
                animationProps.StrafeRightClip = animationClip;
                return;
            }


            // attemp Jog strafe back left clip
            if (animationProps.JogStrafeBackLeftClip == null
                && animationClip.name.ToLowerInvariant().Contains("run") == true
                && animationClip.name.ToLowerInvariant().Contains("back") == true
                && animationClip.name.ToLowerInvariant().Contains("left") == true) {
                animationProps.JogStrafeBackLeftClip = animationClip;
                return;
            }

            // attemp Jog strafe back right clip
            if (animationProps.JogStrafeBackRightClip == null
                && animationClip.name.ToLowerInvariant().Contains("run") == true
                && animationClip.name.ToLowerInvariant().Contains("back") == true
                && animationClip.name.ToLowerInvariant().Contains("right") == true) {
                animationProps.JogStrafeBackRightClip = animationClip;
                return;
            }

            // attemp Jog strafe forward left clip
            if (animationProps.JogStrafeForwardLeftClip == null
                && animationClip.name.ToLowerInvariant().Contains("run") == true
                && animationClip.name.ToLowerInvariant().Contains("forward") == true
                && animationClip.name.ToLowerInvariant().Contains("left") == true) {
                animationProps.JogStrafeForwardLeftClip = animationClip;
                return;
            }

            // attemp Jog strafe forward right clip
            if (animationProps.JogStrafeForwardRightClip == null
                && animationClip.name.ToLowerInvariant().Contains("run") == true
                && animationClip.name.ToLowerInvariant().Contains("forward") == true
                && animationClip.name.ToLowerInvariant().Contains("right") == true) {
                animationProps.JogStrafeForwardRightClip = animationClip;
                return;
            }

            // attemp Jog strafe left clip
            if (animationProps.JogStrafeLeftClip == null
                && animationClip.name.ToLowerInvariant().Contains("run") == true
                && animationClip.name.ToLowerInvariant().Contains("forward") == false
                && animationClip.name.ToLowerInvariant().Contains("back") == false
                && animationClip.name.ToLowerInvariant().Contains("left") == true) {
                animationProps.JogStrafeLeftClip = animationClip;
                return;
            }

            // attemp Jog strafe right clip
            if (animationProps.JogStrafeRightClip == null
                && animationClip.name.ToLowerInvariant().Contains("run") == true
                && animationClip.name.ToLowerInvariant().Contains("forward") == false
                && animationClip.name.ToLowerInvariant().Contains("back") == false
                && animationClip.name.ToLowerInvariant().Contains("right") == true) {
                animationProps.JogStrafeRightClip = animationClip;
                return;
            }
        }

        private static void AssignInCombatAnimation(AnimationProps animationProps, AnimationClip animationClip) {

            // attemp jump clip
            if (animationProps.CombatJumpClip == null
                && animationClip.name.ToLowerInvariant().Contains("jump")) {
                animationProps.CombatJumpClip = animationClip;
                return;
            }

            // attemp fall clip
            if (animationProps.CombatFallClip == null
                && animationClip.name.ToLowerInvariant().Contains("fall")) {
                animationProps.CombatFallClip = animationClip;
                return;
            }

            // attemp land clip
            if (animationProps.CombatLandClip == null
                && animationClip.name.ToLowerInvariant().Contains("land")) {
                animationProps.CombatLandClip = animationClip;
                return;
            }

            // attemp idle clip
            if (animationProps.CombatIdleClip == null
                && animationClip.name.ToLowerInvariant().Contains("idle")) {
                animationProps.CombatIdleClip = animationClip;
                return;
            }

            // attemp walk clip
            if (animationProps.CombatWalkClip == null
                && animationClip.name.ToLowerInvariant().Contains("walk") == true
                && animationClip.name.ToLowerInvariant().Contains("back") == false) {
                animationProps.CombatWalkClip = animationClip;
                return;
            }

            // attemp walk back clip
            if (animationProps.CombatWalkBackClip == null
                && animationClip.name.ToLowerInvariant().Contains("walk") == true
                && animationClip.name.ToLowerInvariant().Contains("back") == true) {
                animationProps.CombatWalkBackClip = animationClip;
                return;
            }

            // attemp run clip
            if (animationProps.CombatRunClip == null
                && animationClip.name.ToLowerInvariant().Contains("run") == true
                && animationClip.name.ToLowerInvariant().Contains("back") == false) {
                animationProps.CombatRunClip = animationClip;
                return;
            }

            // attemp run back clip
            if (animationProps.CombatRunBackClip == null
                && animationClip.name.ToLowerInvariant().Contains("run") == true
                && animationClip.name.ToLowerInvariant().Contains("back") == true) {
                animationProps.CombatRunBackClip = animationClip;
                return;
            }

            // attemp strafe back left clip
            if (animationProps.CombatStrafeBackLeftClip == null
                && animationClip.name.ToLowerInvariant().Contains("strafe") == true
                && animationClip.name.ToLowerInvariant().Contains("back") == true
                && animationClip.name.ToLowerInvariant().Contains("left") == true) {
                animationProps.CombatStrafeBackLeftClip = animationClip;
                return;
            }

            // attemp strafe back right clip
            if (animationProps.CombatStrafeBackRightClip == null
                && animationClip.name.ToLowerInvariant().Contains("strafe") == true
                && animationClip.name.ToLowerInvariant().Contains("back") == true
                && animationClip.name.ToLowerInvariant().Contains("right") == true) {
                animationProps.CombatStrafeBackRightClip = animationClip;
                return;
            }

            // attemp strafe forward left clip
            if (animationProps.CombatStrafeForwardLeftClip == null
                && animationClip.name.ToLowerInvariant().Contains("strafe") == true
                && animationClip.name.ToLowerInvariant().Contains("forward") == true
                && animationClip.name.ToLowerInvariant().Contains("left") == true) {
                animationProps.CombatStrafeForwardLeftClip = animationClip;
                return;
            }

            // attemp strafe forward right clip
            if (animationProps.CombatStrafeForwardRightClip == null
                && animationClip.name.ToLowerInvariant().Contains("strafe") == true
                && animationClip.name.ToLowerInvariant().Contains("forward") == true
                && animationClip.name.ToLowerInvariant().Contains("right") == true) {
                animationProps.CombatStrafeForwardRightClip = animationClip;
                return;
            }

            // attemp strafe left clip
            if (animationProps.CombatStrafeLeftClip == null
                && animationClip.name.ToLowerInvariant().Contains("strafe") == true
                && animationClip.name.ToLowerInvariant().Contains("forward") == false
                && animationClip.name.ToLowerInvariant().Contains("back") == false
                && animationClip.name.ToLowerInvariant().Contains("left") == true) {
                animationProps.CombatStrafeLeftClip = animationClip;
                return;
            }

            // attemp strafe right clip
            if (animationProps.CombatStrafeRightClip == null
                && animationClip.name.ToLowerInvariant().Contains("strafe") == true
                && animationClip.name.ToLowerInvariant().Contains("forward") == false
                && animationClip.name.ToLowerInvariant().Contains("back") == false
                && animationClip.name.ToLowerInvariant().Contains("right") == true) {
                animationProps.CombatStrafeRightClip = animationClip;
                return;
            }


            // attemp Jog strafe back left clip
            if (animationProps.CombatJogStrafeBackLeftClip == null
                && animationClip.name.ToLowerInvariant().Contains("run") == true
                && animationClip.name.ToLowerInvariant().Contains("back") == true
                && animationClip.name.ToLowerInvariant().Contains("left") == true) {
                animationProps.CombatJogStrafeBackLeftClip = animationClip;
                return;
            }

            // attemp Jog strafe back right clip
            if (animationProps.CombatJogStrafeBackRightClip == null
                && animationClip.name.ToLowerInvariant().Contains("run") == true
                && animationClip.name.ToLowerInvariant().Contains("back") == true
                && animationClip.name.ToLowerInvariant().Contains("right") == true) {
                animationProps.CombatJogStrafeBackRightClip = animationClip;
                return;
            }

            // attemp Jog strafe forward left clip
            if (animationProps.CombatJogStrafeForwardLeftClip == null
                && animationClip.name.ToLowerInvariant().Contains("run") == true
                && animationClip.name.ToLowerInvariant().Contains("forward") == true
                && animationClip.name.ToLowerInvariant().Contains("left") == true) {
                animationProps.CombatJogStrafeForwardLeftClip = animationClip;
                return;
            }

            // attemp Jog strafe forward right clip
            if (animationProps.CombatJogStrafeForwardRightClip == null
                && animationClip.name.ToLowerInvariant().Contains("run") == true
                && animationClip.name.ToLowerInvariant().Contains("forward") == true
                && animationClip.name.ToLowerInvariant().Contains("right") == true) {
                animationProps.CombatJogStrafeForwardRightClip = animationClip;
                return;
            }

            // attemp Jog strafe left clip
            if (animationProps.CombatJogStrafeLeftClip == null
                && animationClip.name.ToLowerInvariant().Contains("run") == true
                && animationClip.name.ToLowerInvariant().Contains("forward") == false
                && animationClip.name.ToLowerInvariant().Contains("back") == false
                && animationClip.name.ToLowerInvariant().Contains("left") == true) {
                animationProps.CombatJogStrafeLeftClip = animationClip;
                return;
            }

            // attemp Jog strafe right clip
            if (animationProps.CombatJogStrafeRightClip == null
                && animationClip.name.ToLowerInvariant().Contains("run") == true
                && animationClip.name.ToLowerInvariant().Contains("forward") == false
                && animationClip.name.ToLowerInvariant().Contains("back") == false
                && animationClip.name.ToLowerInvariant().Contains("right") == true) {
                animationProps.CombatJogStrafeRightClip = animationClip;
                return;
            }
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

            // check for empty character name
            if (profileName == null || profileName.Trim() == "") {
                return "Profile Name is Required";
            }

            return null;
        }

        
        protected override bool DrawWizardGUI() {

            /*
            if (attachmentProfile == string.Empty) {
                EditorGUILayout.HelpBox("The attachment profile is not set.  If this character is a humanoid, it will not be able to equip weapon models.", MessageType.Warning);
            }
            */

            bool returnResult = base.DrawWizardGUI();


            return returnResult;
        }
        

    }

    public enum AssignAnimationType { OutOfCombat, InCombat }

}
