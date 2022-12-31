using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New SceneNode", menuName = "AnyRPG/SceneNode")]
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

        [Tooltip("Ambient sounds to play in the background during the day while this scene is active")]
        [SerializeField]
        [FormerlySerializedAs("ambientMusicProfile")]
        [ResourceSelector(resourceType = typeof(AudioProfile))]
        private string dayAmbientSoundsProfile = string.Empty;

        [Tooltip("Ambient sounds to play in the background during the day while this scene is active.  This will override any audio profile chosen")]
        [SerializeField]
        private AudioClip dayAmbientSoundsAudio = null;

        private AudioProfile dayAmbientSoundsProfileReference;

        [Tooltip("Ambient sounds to play in the background while this scene is active")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AudioProfile))]
        private string nightAmbientSoundsProfile = string.Empty;

        [Tooltip("Ambient sounds to play in the background at night while this scene is active.  This will override any audio profile chosen")]
        [SerializeField]
        private AudioClip nightAmbientSoundsAudio = null;

        private AudioProfile nightAmbientSoundsProfileReference;

        [Tooltip("Music to play in the background while this scene is active")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AudioProfile))]
        private string backgroundMusicProfile = string.Empty;

        [Tooltip("Music to play in the background while this scene is active.  This will override any audio profile chosen")]
        [SerializeField]
        private AudioClip backgroundMusicAudio = null;

        private AudioProfile backgroundMusicProfileReference;

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

        [Tooltip("A list of audio profiles containing footstep sounds to play depending on what terrain layer the character is moving on.  The list index is matched to the terrain layers index.  Eg, list item one will be played when the character is over terrain layer one.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AudioProfile))]
        private List<string> footStepProfiles = new List<string>();

        private List<AudioProfile> footStepProfileReferences = new List<AudioProfile>();


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

        [Header("Time Of Day Settings")]

        [Tooltip("If true, the sun source in the scene will be rotated according to the time of day.")]
        [SerializeField]
        private bool rotateSunDirection = false;

        [Tooltip("If true, the default sun angle from the System Configuration Manager will be used instead of the value below.")]
        [SerializeField]
        private bool useDefaultSunAngle = false;

        [Tooltip("The angle of the sun as an offset from straight down. -90 is pointing directly North, +90 is pointing directly south.")]
        [SerializeField]
        private float sunAngle = 0f;

        [Tooltip("If true, the color of light the sun emits will be changed over time.")]
        [SerializeField]
        private bool rotateSunColor = false;

        [Tooltip("If true, the Sun Gradient from the System Configuration Manager will be used instead of the gradient below.")]
        [SerializeField]
        private bool useDefaultSunGradient = false;

        [Tooltip("A color gradient to use for the sun color.  The ends represent midnight, and the center is noon.")]
        [SerializeField]
        private Gradient sunGradient;

        [Tooltip("If true, the skybox is assumed to be using the BlendedSkybox shader, and will change based on the alpha property of the sunGradient over time.")]
        [SerializeField]
        private bool blendedSkybox = false;

        [Tooltip("If true, the skybox will be rotated as time passes.")]
        [SerializeField]
        private bool rotateSkybox = false;

        [Tooltip("The offset rotation required to position the skybox so the sun is in the correct position at midnight.")]
        [SerializeField]
        [Range(0, 360)]
        private float skyboxRotationOffset = 0f;

        [Tooltip("If true, the skybox will be rotated in the opposite of the default direction")]
        [SerializeField]
        private bool reverseSkyboxRotation = false;


        // game manager referenes
        private SaveManager saveManager = null;

        private Dictionary<string, PersistentObjectSaveData> persistentObjects = new Dictionary<string, PersistentObjectSaveData>();

        public string SceneName { get => resourceName; set => resourceName = value; }
        public bool SuppressCharacterSpawn { get => suppressCharacterSpawn; set => suppressCharacterSpawn = value; }
        public bool SuppressMainCamera { get => suppressMainCamera; set => suppressMainCamera = value; }
        //public AudioProfile AmbientMusicProfile { get => dayAmbientSoundsProfileReference; set => dayAmbientSoundsProfileReference = value; }
        public AudioClip DayAmbientSound {
            get {
                if (dayAmbientSoundsAudio != null) {
                    return dayAmbientSoundsAudio;
                }
                if (dayAmbientSoundsProfileReference != null) {
                    return dayAmbientSoundsProfileReference.RandomAudioClip;
                }
                return null;
            }
            set {
                dayAmbientSoundsAudio = value;
            }
        }
        public AudioClip NightAmbientSound {
            get {
                if (nightAmbientSoundsAudio != null) {
                    return nightAmbientSoundsAudio;
                }
                if (nightAmbientSoundsProfileReference != null) {
                    return nightAmbientSoundsProfileReference.RandomAudioClip;
                }
                return null;
            }
            set {
                nightAmbientSoundsAudio = value;
            }
        }
        public AudioClip BackgroundMusicAudio {
            get {
                if (backgroundMusicAudio != null) {
                    return backgroundMusicAudio;
                }
                if (backgroundMusicProfileReference != null) {
                    return backgroundMusicProfileReference.RandomAudioClip;
                }
                return null;
            }
            set {
                backgroundMusicAudio = value;
            }
        }

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
        public string DayAmbientSoundsProfileName { set => dayAmbientSoundsProfile = value; }
        public string NightAmbientSoundsProfileName { set => nightAmbientSoundsProfile = value; }
        public int FootStepProfilesCount { get => footStepProfileReferences.Count; }
        public bool RotateSunDirection { get => rotateSunDirection; set => rotateSunDirection = value; }
        public float SunAngle { get => sunAngle; set => sunAngle = value; }
        public bool RotateSunColor { get => rotateSunColor; set => rotateSunColor = value; }
        public Gradient SunGradient { get => sunGradient; set => sunGradient = value; }
        public bool BlendedSkybox { get => blendedSkybox; set => blendedSkybox = value; }
        public bool UseDefaultSunAngle { get => useDefaultSunAngle; set => useDefaultSunAngle = value; }
        public bool UseDefaultSunGradient { get => useDefaultSunGradient; set => useDefaultSunGradient = value; }
        public bool RotateSkybox { get => rotateSkybox; set => rotateSkybox = value; }
        public float SkyboxRotationOffset { get => skyboxRotationOffset; set => skyboxRotationOffset = value; }
        public bool ReverseSkyboxRotation { get => reverseSkyboxRotation; set => reverseSkyboxRotation = value; }
        //public AudioClip BackgroundMusicAudio { get => backgroundMusicAudio; set => backgroundMusicAudio = value; }
        //public AudioClip NightAmbientSoundsAudio { get => nightAmbientSoundsAudio; set => nightAmbientSoundsAudio = value; }

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
            PreloadFootStepAudio();
            OnVisitZone();
        }

        private void PreloadFootStepAudio() {
            foreach (AudioProfile audioProfile in footStepProfileReferences) {
                if (audioProfile != null) {
                    audioProfile.PreloadAudioClips();
                }
            }
        }

        public AudioProfile GetFootStepAudioProfile(int terrainTextureIndex) {
            //Debug.Log($"{DisplayName}.SceneNode.GetFootStepAudioProfile({terrainTextureIndex})");
            if (terrainTextureIndex >= footStepProfileReferences.Count) {
                return null;
            }

            return footStepProfileReferences[terrainTextureIndex];
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

            foreach (string audioProfileName in footStepProfiles) {
                if (audioProfileName != null && audioProfileName != string.Empty) {
                    AudioProfile tmpMovementHit = systemDataFactory.GetResource<AudioProfile>(audioProfileName);
                    if (tmpMovementHit == null) {
                        Debug.LogError("SceneNode.SetupScriptableObjects(): Could not find audio profile : " + audioProfileName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                    footStepProfileReferences.Add(tmpMovementHit);
                } else {
                    footStepProfileReferences.Add(null);
                }
            }
            //Debug.Log($"{DisplayName} has {footStepProfileReferences.Count} audio profiles");

            dayAmbientSoundsProfileReference = null;
            if (dayAmbientSoundsProfile != null && dayAmbientSoundsProfile != string.Empty) {
                AudioProfile tmpAmbientMusicProfile = systemDataFactory.GetResource<AudioProfile>(dayAmbientSoundsProfile);
                if (tmpAmbientMusicProfile != null) {
                    dayAmbientSoundsProfileReference = tmpAmbientMusicProfile;
                } else {
                    Debug.LogError("SceneNode.SetupScriptableObjects(): Could not find audio profile : " + dayAmbientSoundsProfile + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

            nightAmbientSoundsProfileReference = null;
            if (nightAmbientSoundsProfile != null && nightAmbientSoundsProfile != string.Empty) {
                AudioProfile tmpAmbientMusicProfile = systemDataFactory.GetResource<AudioProfile>(nightAmbientSoundsProfile);
                if (tmpAmbientMusicProfile != null) {
                    nightAmbientSoundsProfileReference = tmpAmbientMusicProfile;
                } else {
                    Debug.LogError("SceneNode.SetupScriptableObjects(): Could not find audio profile : " + nightAmbientSoundsProfile + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }


            backgroundMusicProfileReference = null;
            if (backgroundMusicProfile != null && backgroundMusicProfile != string.Empty) {
                AudioProfile tmpBackgroundMusicProfile = systemDataFactory.GetResource<AudioProfile>(backgroundMusicProfile);
                if (tmpBackgroundMusicProfile != null) {
                    backgroundMusicProfileReference = tmpBackgroundMusicProfile;
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