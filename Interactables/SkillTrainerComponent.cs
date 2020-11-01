using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SkillTrainerComponent : InteractableOptionComponent {

        public override event System.Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        private SkillTrainerProps interactableOptionProps = null;

        private List<Skill> skills = new List<Skill>();

        public List<Skill> MySkills { get => skills; }

        public override Sprite Icon { get => interactableOptionProps.Icon; }
        public override Sprite NamePlateImage { get => interactableOptionProps.NamePlateImage; }

        public SkillTrainerComponent(Interactable interactable, SkillTrainerProps interactableOptionProps) : base(interactable) {
            this.interactableOptionProps = interactableOptionProps;
            SetupScriptableObjects();
        }

        public override void Init() {
            base.Init();
            if (interactionPanelTitle == string.Empty) {
                //Debug.Log("SkillTrainer.Start(): interactionPanelTitle is empty: setting to default (Train Me)!!!");
                interactionPanelTitle = "Train Me";
            }
        }

        public void InitWindow(ICloseableWindowContents skillTrainerUI) {
            //Debug.Log(gameObject.name + ".SkillTrainer.InitWindow()");
            (skillTrainerUI as SkillTrainerUI).ShowSkills(this);
        }

        public override bool Interact(CharacterUnit source) {
            //Debug.Log(gameObject.name + ".SkillTrainer.Interact(" + source + ")");
            base.Interact(source);
            if (!PopupWindowManager.MyInstance.skillTrainerWindow.IsOpen) {
                //Debug.Log(source + " interacting with " + gameObject.name);
                //vendorWindow.MyVendorUI.CreatePages(items);
                PopupWindowManager.MyInstance.skillTrainerWindow.MyCloseableWindowContents.OnOpenWindow += InitWindow;
                PopupWindowManager.MyInstance.skillTrainerWindow.MyCloseableWindowContents.OnCloseWindow += CleanupEventSubscriptions;
                SystemEventManager.MyInstance.OnSkillListChanged += HandleSkillListChanged;
                PopupWindowManager.MyInstance.skillTrainerWindow.OpenWindow();
                return true;
            }
            return false;
        }

        public override void StopInteract() {
            //Debug.Log(gameObject.name + ".SkillTrainer.StopInteract()");
            base.StopInteract();
            //vendorUI.ClearPages();
            PopupWindowManager.MyInstance.skillTrainerWindow.CloseWindow();
        }

        public void CleanupEventSubscriptions(ICloseableWindowContents windowContents) {
            //Debug.Log(gameObject.name + ".SkillTrainer.CleanupEventSubscriptions(windowContents)");
            CleanupWindowEventSubscriptions();
        }

        public void CleanupWindowEventSubscriptions() {
            if (PopupWindowManager.MyInstance != null && PopupWindowManager.MyInstance.skillTrainerWindow != null && PopupWindowManager.MyInstance.skillTrainerWindow.MyCloseableWindowContents != null) {
                PopupWindowManager.MyInstance.skillTrainerWindow.MyCloseableWindowContents.OnOpenWindow -= InitWindow;
                PopupWindowManager.MyInstance.skillTrainerWindow.MyCloseableWindowContents.OnCloseWindow -= CleanupEventSubscriptions;
                SystemEventManager.MyInstance.OnSkillListChanged -= HandleSkillListChanged;
            }
        }

        public override void CleanupEventSubscriptions() {
            //Debug.Log(gameObject.name + ".SkillTrainer.CleanupEventSubscriptions()");
            base.CleanupEventSubscriptions();
            CleanupWindowEventSubscriptions();
        }

        public void HandleSkillListChanged(Skill skill) {
            // this is a special case.  since skill is not a prerequisites, we need to subscribe directly to the event to get notified things have changed
            if (skills.Contains(skill)) {
                HandlePrerequisiteUpdates();
            }
        }

        public override int GetValidOptionCount() {
            return GetCurrentOptionCount();
        }

        public override int GetCurrentOptionCount() {
            //Debug.Log(gameObject.name + ".SkillTrainerInteractable.GetCurrentOptionCount()");
            int optionCount = 0;
            foreach (Skill skill in skills) {
                if (!PlayerManager.MyInstance.MyCharacter.CharacterSkillManager.HasSkill(skill)) {
                    optionCount++;
                }
            }
            //Debug.Log(gameObject.name + ".SkillTrainerInteractable.GetCurrentOptionCount(); return: " + optionCount);
            return optionCount;
        }

        public override bool CanInteract() {
            //Debug.Log(gameObject.name + ".SkillTrainer.CanInteract()");
            bool returnValue = ((GetCurrentOptionCount() > 0 && MyPrerequisitesMet) ? true : false);
            //Debug.Log(gameObject.name + ".SkillTrainer.CanInteract(): return: " + returnValue);
            return returnValue;
        }

        public override bool CanShowMiniMapIcon() {
            return CanInteract();
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
            if (interactableOptionProps.SkillNames != null) {
                skills = new List<Skill>();
                foreach (string skillName in interactableOptionProps.SkillNames) {
                    Skill tmpSkill = SystemSkillManager.MyInstance.GetResource(skillName);
                    if (tmpSkill != null) {
                        skills.Add(tmpSkill);
                    } else {
                        Debug.LogError("SkillTrainerComponent.SetupScriptableObjects(): Could not find skill : " + skillName + " while inititalizing.  CHECK INSPECTOR");
                    }
                }
            }

        }
    }

}