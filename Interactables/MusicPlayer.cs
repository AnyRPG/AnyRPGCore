using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class MusicPlayer : InteractableOption {

        public override event System.Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        public override Sprite MyIcon { get => (SystemConfigurationManager.MyInstance.MyMusicPlayerInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyMusicPlayerInteractionPanelImage : base.MyIcon); }
        public override Sprite MyNamePlateImage { get => (SystemConfigurationManager.MyInstance.MyMusicPlayerNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyMusicPlayerNamePlateImage : base.MyNamePlateImage); }

        [SerializeField]
        private List<string> musicProfileNames = new List<string>();

        [SerializeField]
        private List<AudioProfile> musicProfileList = new List<AudioProfile>();

        /*
        [SerializeField]
        private CharacterUnit characterUnit;
        */

        public List<AudioProfile> MyMusicProfileList { get => musicProfileList; set => musicProfileList = value; }


        protected override void Awake() {
            base.Awake();
            //Debug.Log(gameObject.name + ".SkillTrainer.Awake()");
            //characterUnit = GetComponent<CharacterUnit>();
        }

        protected override void Start() {
            //Debug.Log(gameObject.name + ".SkillTrainer.Start()");
            base.Start();
            if (interactionPanelTitle == string.Empty) {
                //Debug.Log("SkillTrainer.Start(): interactionPanelTitle is empty: setting to default (Train Me)!!!");
                interactionPanelTitle = "Player Music";
            }
        }

        public void InitWindow(ICloseableWindowContents musicPlayerUI) {
            //Debug.Log(gameObject.name + ".SkillTrainer.InitWindow()");
            (musicPlayerUI as MusicPlayerUI).ShowMusicProfiles(this);
        }

        public override bool Interact(CharacterUnit source) {
            //Debug.Log(gameObject.name + ".SkillTrainer.Interact(" + source + ")");
            base.Interact(source);
            if (!PopupWindowManager.MyInstance.musicPlayerWindow.IsOpen) {
                //Debug.Log(source + " interacting with " + gameObject.name);
                //vendorWindow.MyVendorUI.CreatePages(items);
                PopupWindowManager.MyInstance.musicPlayerWindow.MyCloseableWindowContents.OnOpenWindow += InitWindow;
                PopupWindowManager.MyInstance.musicPlayerWindow.MyCloseableWindowContents.OnCloseWindow += CleanupEventSubscriptions;
                PopupWindowManager.MyInstance.musicPlayerWindow.OpenWindow();
                return true;
            }
            return false;
        }

        public override void StopInteract() {
            //Debug.Log(gameObject.name + ".SkillTrainer.StopInteract()");
            base.StopInteract();
            //vendorUI.ClearPages();
            PopupWindowManager.MyInstance.musicPlayerWindow.CloseWindow();
        }

        public void CleanupEventSubscriptions(ICloseableWindowContents windowContents) {
            //Debug.Log(gameObject.name + ".SkillTrainer.CleanupEventSubscriptions(windowContents)");
            CleanupEventSubscriptions();
        }

        public override void CleanupEventSubscriptions() {
            //Debug.Log(gameObject.name + ".SkillTrainer.CleanupEventSubscriptions()");
            base.CleanupEventSubscriptions();
            if (PopupWindowManager.MyInstance != null && PopupWindowManager.MyInstance.musicPlayerWindow != null && PopupWindowManager.MyInstance.musicPlayerWindow.MyCloseableWindowContents != null) {
                PopupWindowManager.MyInstance.musicPlayerWindow.MyCloseableWindowContents.OnOpenWindow -= InitWindow;
                PopupWindowManager.MyInstance.musicPlayerWindow.MyCloseableWindowContents.OnCloseWindow -= CleanupEventSubscriptions;
            }
        }

        public override void OnDisable() {
            //Debug.Log(gameObject.name + ".SkillTrainer.OnDisable()");
            base.OnDisable();
            CleanupEventSubscriptions();
        }

        public override void HandlePrerequisiteUpdates() {
            //Debug.Log(gameObject.name + ".SkillTrainer.HandlePrerequisiteUpdates()");
            base.HandlePrerequisiteUpdates();
            MiniMapStatusUpdateHandler(this);
        }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            musicProfileList = new List<AudioProfile>();
            if (musicProfileNames != null) {
                foreach (string musicProfileName in musicProfileNames) {
                    AudioProfile tmpMusicProfile = SystemAudioProfileManager.MyInstance.GetResource(musicProfileName);
                    if (tmpMusicProfile != null) {
                        musicProfileList.Add(tmpMusicProfile);
                    } else {
                        Debug.LogError(gameObject.name + "UnitSpawnControllerInteractable.SetupScriptableObjects(): COULD NOT FIND AUDIO PROFILE: " + musicProfileName + " while initializing " + gameObject.name);
                    }
                }
            }
        }
    }

}