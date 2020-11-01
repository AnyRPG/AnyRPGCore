using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class MusicPlayerComponent : InteractableOptionComponent {

        public override event System.Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        private MusicPlayerProps interactableOptionProps = null;

        public override Sprite Icon { get => interactableOptionProps.Icon; }
        public override Sprite NamePlateImage { get => interactableOptionProps.NamePlateImage; }

        private List<AudioProfile> musicProfileList = new List<AudioProfile>();

        public List<AudioProfile> MyMusicProfileList { get => musicProfileList; set => musicProfileList = value; }

        public MusicPlayerComponent(Interactable interactable, MusicPlayerProps interactableOptionProps) : base(interactable) {
            this.interactableOptionProps = interactableOptionProps;
            if (interactionPanelTitle == string.Empty) {
                //Debug.Log("SkillTrainer.Start(): interactionPanelTitle is empty: setting to default (Train Me)!!!");
                interactionPanelTitle = "Music Player";
            }
        }

        public override void Cleanup() {
            base.Cleanup();
            CleanupWindowEventSubscriptions();
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
            CleanupWindowEventSubscriptions();
        }

        public void CleanupWindowEventSubscriptions() {
            if (PopupWindowManager.MyInstance != null && PopupWindowManager.MyInstance.musicPlayerWindow != null && PopupWindowManager.MyInstance.musicPlayerWindow.MyCloseableWindowContents != null) {
                PopupWindowManager.MyInstance.musicPlayerWindow.MyCloseableWindowContents.OnOpenWindow -= InitWindow;
                PopupWindowManager.MyInstance.musicPlayerWindow.MyCloseableWindowContents.OnCloseWindow -= CleanupEventSubscriptions;
            }
        }

        public override void HandlePrerequisiteUpdates() {
            //Debug.Log(gameObject.name + ".SkillTrainer.HandlePrerequisiteUpdates()");
            base.HandlePrerequisiteUpdates();
            MiniMapStatusUpdateHandler(this);
        }

        public override void HandlePlayerUnitSpawn() {
            base.HandlePlayerUnitSpawn();
            MiniMapStatusUpdateHandler(this);
        }


        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            musicProfileList = new List<AudioProfile>();
            if (interactableOptionProps.MusicProfileNames != null) {
                foreach (string musicProfileName in interactableOptionProps.MusicProfileNames) {
                    AudioProfile tmpMusicProfile = SystemAudioProfileManager.MyInstance.GetResource(musicProfileName);
                    if (tmpMusicProfile != null) {
                        musicProfileList.Add(tmpMusicProfile);
                    } else {
                        Debug.LogError("MusicPlayerCompoennt.SetupScriptableObjects(): COULD NOT FIND AUDIO PROFILE: " + interactableOptionProps.MusicProfileNames + " while initializing");
                    }
                }
            }
        }
    }

}