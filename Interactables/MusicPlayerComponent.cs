using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class MusicPlayerComponent : InteractableOptionComponent {

        public override event System.Action<InteractableOptionComponent> MiniMapStatusUpdateHandler = delegate { };

        public MusicPlayerProps Props { get => interactableOptionProps as MusicPlayerProps; }

        public MusicPlayerComponent(Interactable interactable, MusicPlayerProps interactableOptionProps) : base(interactable, interactableOptionProps) {
            if (interactableOptionProps.InteractionPanelTitle == string.Empty) {
                //Debug.Log("SkillTrainer.Start(): interactionPanelTitle is empty: setting to default (Train Me)!!!");
                interactableOptionProps.InteractionPanelTitle = "Music Player";
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

        public override void CallMiniMapStatusUpdateHandler() {
            MiniMapStatusUpdateHandler(this);
        }

        public override void HandlePlayerUnitSpawn() {
            base.HandlePlayerUnitSpawn();
            MiniMapStatusUpdateHandler(this);
        }

    }

}