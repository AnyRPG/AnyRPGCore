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

        public event System.Action OnVisitZone = delegate { };

        [Header("Scene File")]

        [Tooltip("If true, look for the resource description with the same name as this resource, plus the string 'Scene' and use the Display Name field as the file name.")]
        [SerializeField]
        private bool useRegionalFile = false;

        [Tooltip("The name of the scene, without a path, as it is found in the Unity Build settings")]
        [SerializeField]
        private string sceneFile = string.Empty;

        [Header("Scene Audio")]

        [Tooltip("Ambient sounds to play in the background while this scene is active")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AudioProfile))]
        private string ambientMusicProfile = string.Empty;

        private AudioProfile realAmbientMusicProfile;

        [Tooltip("Music to play in the background while this scene is active")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AudioProfile))]
        private string backgroundMusicProfile = string.Empty;

        private AudioProfile realBackgroundMusicProfile;

        [Header("Movement Audio")]

        [Tooltip("This audio will override the movement sound loop for a character in this zone")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AudioProfile))]
        private string movementLoopProfileName = string.Empty;

        private AudioProfile movementLoopProfile;

        [Tooltip("This audio will override the movement hit (footstep) sound for a character in this zone")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AudioProfile))]
        private string movementHitProfileName = string.Empty;

        private AudioProfile movementHitProfile;

        [Header("Scene Options")]

        [Tooltip("If false, mounts cannot be used in this scene")]
        [SerializeField]
        private bool allowMount = true;

        [Tooltip("Prevent the player unit from spawning in this scene.  Useful for cutscenes that are separate scenes or menu / game over scenes.")]
        [SerializeField]
        private bool suppressCharacterSpawn = false;

        [Tooltip("Prevent the main camera from activating when this scene is loaded.  Useful for cutscenes.")]
        [SerializeField]
        private bool suppressMainCamera = false;

        [Tooltip("A Cutscene to play automatically when this level is loaded.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(Cutscene))]
        private string autoPlayCutsceneName = string.Empty;

        private Cutscene autoPlayCutscene = null;

        [Header("Environmental Settings")]

        [Tooltip("A list of environment state names available to this scene.  Used for swapping environment states with unity timeline.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(EnvironmentStateProfile))]
        private List<string> environmentStateNames = new List<string>();

        private List<EnvironmentStateProfile> environmentStates = new List<EnvironmentStateProfile>();

        // game manager referenes
        private SaveManager saveManager = null;

        private Dictionary<string, PersistentObjectSaveData> persistentObjects = new Dictionary<string, PersistentObjectSaveData>();

        public string SceneName { get => resourceName; set => resourceName = value; }
        public bool SuppressCharacterSpawn { get => suppressCharacterSpawn; set => suppressCharacterSpawn = value; }
        public bool SuppressMainCamera { get => suppressMainCamera; set => suppressMainCamera = value; }
        public AudioProfile AmbientMusicProfile { get => realAmbientMusicProfile; set => realAmbientMusicProfile = value; }
        public AudioProfile BackgroundMusicProfile { get => realBackgroundMusicProfile; set => realBackgroundMusicProfile = value; }
        public List<PersistentObjectSaveData> PersistentObjects {
            get {
                return saveManager.GetSceneNodeSaveData(this).persistentObjects;
            }
        }
        public List<EnvironmentStateProfile> EnvironmentStates { get => environmentStates; set => environmentStates = value; }
        public Cutscene AutoPlayCutscene { get => autoPlayCutscene; set => autoPlayCutscene = value; }

        public AudioProfile MovementLoopProfile { get => movementLoopProfile; set => movementLoopProfile = value; }
        public AudioProfile MovementHitProfile { get => movementHitProfile; set => movementHitProfile = value; }
        public string SceneFile { get => sceneFile; set => sceneFile = value; }

        public bool Visited {
            get {
                return saveManager.GetSceneNodeSaveData(this).visited;
            }
            set {
                SceneNodeSaveData saveData = saveManager.GetSceneNodeSaveData(this);
                saveData.visited = value;
                saveManager.SceneNodeSaveDataDictionary[saveData.SceneName] = saveData;
            }
        }

        public bool AllowMount { get => allowMount; set => allowMount = value; }
        public string BackgroundMusicProfileName { set => backgroundMusicProfile = value; }
        public string AmbientMusicProfileName { set => ambientMusicProfile = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            saveManager = systemGameManager.SaveManager;
        }

        public void SavePersistentObject(string UUID, PersistentObjectSaveData persistentObjectSaveData) {
            //Debug.Log(DisplayName + ".SceneNode.SavePersistentObject(" + UUID + ")");
            SceneNodeSaveData saveData = saveManager.GetSceneNodeSaveData(this);
            foreach (PersistentObjectSaveData _persistentObjectSaveData in saveData.persistentObjects) {
                if (_persistentObjectSaveData.UUID == UUID) {
                    saveData.persistentObjects.Remove(_persistentObjectSaveData);
                    saveManager.SceneNodeSaveDataDictionary[saveData.SceneName] = saveData;
                    break;
                }
            }
            saveData.persistentObjects.Add(persistentObjectSaveData);
            saveManager.SceneNodeSaveDataDictionary[saveData.SceneName] = saveData;
        }

        public PersistentObjectSaveData GetPersistentObject(string UUID) {
            foreach (PersistentObjectSaveData _persistentObjectSaveData in saveManager.GetSceneNodeSaveData(this).persistentObjects) {
                if (_persistentObjectSaveData.UUID == UUID) {
                    return _persistentObjectSaveData;
                }
            }
            return new PersistentObjectSaveData();
        }

        public void Visit() {
            if (Visited == false) {
                Visited = true;
            }
            OnVisitZone();
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            if (movementLoopProfileName != null && movementLoopProfileName != string.Empty) {
                AudioProfile tmpMovementLoop = systemDataFactory.GetResource<AudioProfile>(movementLoopProfileName);
                if (tmpMovementLoop != null) {
                    movementLoopProfile = tmpMovementLoop;
                } else {
                    Debug.LogError("SceneNode.SetupScriptableObjects(): Could not find audio profile : " + movementLoopProfileName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

            if (movementHitProfileName != null && movementHitProfileName != string.Empty) {
                AudioProfile tmpMovementHit = systemDataFactory.GetResource<AudioProfile>(movementHitProfileName);
                if (tmpMovementHit != null) {
                    movementHitProfile = tmpMovementHit;
                } else {
                    Debug.LogError("SceneNode.SetupScriptableObjects(): Could not find audio profile : " + movementHitProfileName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

            realAmbientMusicProfile = null;
            if (ambientMusicProfile != null && ambientMusicProfile != string.Empty) {
                AudioProfile tmpAmbientMusicProfile = systemDataFactory.GetResource<AudioProfile>(ambientMusicProfile);
                if (tmpAmbientMusicProfile != null) {
                    realAmbientMusicProfile = tmpAmbientMusicProfile;
                } else {
                    Debug.LogError("SceneNode.SetupScriptableObjects(): Could not find music profile : " + ambientMusicProfile + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }


            realBackgroundMusicProfile = null;
            if (backgroundMusicProfile != null && backgroundMusicProfile != string.Empty) {
                AudioProfile tmpBackgroundMusicProfile = systemDataFactory.GetResource<AudioProfile>(backgroundMusicProfile);
                if (tmpBackgroundMusicProfile != null) {
                    realBackgroundMusicProfile = tmpBackgroundMusicProfile;
                } else {
                    Debug.LogError("SceneNode.SetupScriptableObjects(): Could not find music profile : " + backgroundMusicProfile + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

            if (autoPlayCutsceneName != null && autoPlayCutsceneName != string.Empty) {
                Cutscene tmpCutscene = systemDataFactory.GetResource<Cutscene>(autoPlayCutsceneName);
                if (tmpCutscene != null) {
                    autoPlayCutscene = tmpCutscene;
                } else {
                    Debug.LogError("SceneNode.SetupScriptableObjects(): Could not find cutscene : " + autoPlayCutsceneName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

            if (environmentStateNames != null) {
                foreach (string environmentStateName in environmentStateNames) {
                    EnvironmentStateProfile tmpProfile = systemDataFactory.GetResource<EnvironmentStateProfile>(environmentStateName);
                    if (tmpProfile != null) {
                        environmentStates.Add(tmpProfile);
                    } else {
                        Debug.LogError("SceneNode.SetupScriptableObjects(): Could not find environment state : " + environmentStateName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            if (useRegionalFile == true) {
                ResourceDescription tmpResourceDescription = systemDataFactory.GetResource<ResourceDescription>(resourceName + "Scene");
                if (tmpResourceDescription != null) {
                    sceneFile = tmpResourceDescription.DisplayName;
                } else {
                    Debug.LogError("SceneNode.SetupScriptableObjects(): Could not find scene file resource description : " + resourceName + "Scene while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }


        }

    }

}