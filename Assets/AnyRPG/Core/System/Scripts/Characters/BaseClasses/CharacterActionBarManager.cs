using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace AnyRPG {
    public class CharacterActionBarManager : ConfiguredClass {

        private UnitController unitController;

        private bool eventSubscriptionsInitialized = false;

        private List<ActionButtonNode> mouseActionButtons = new List<ActionButtonNode>();
        private List<ActionButtonNode> gamepadActionButtons = new List<ActionButtonNode>();

        // game manager references
        private ActionBarManager actionBarManager = null;

        public List<ActionButtonNode> MouseActionButtons { get => mouseActionButtons; }
        public List<ActionButtonNode> GamepadActionButtons { get => gamepadActionButtons; }

        public CharacterActionBarManager(UnitController unitController, SystemGameManager systemGameManager) {
            this.unitController = unitController;
            Configure(systemGameManager);
            CreateEventSubscriptions();
            InitializeActionButtons();
        }

        private void InitializeActionButtons() {
            int mouseActionButtonCount = actionBarManager.GetMouseActionButtons().Count;
            int gamepadActionButtonCount = actionBarManager.GetGamepadActionButtons().Count;
            for (int i = 0; i < mouseActionButtonCount; i++) {
                mouseActionButtons.Add(new ActionButtonNode());
            }
            for (int i = 0; i < gamepadActionButtonCount; i++) {
                gamepadActionButtons.Add(new ActionButtonNode());
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            actionBarManager = systemGameManager.UIManager.ActionBarManager;
        }

        public void CreateEventSubscriptions() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized == false) {
                unitController.UnitEventController.OnLearnAbility += HandleLearnAbility;
                unitController.UnitEventController.OnUnlearnAbility += HandleUnlearnAbility;
                eventSubscriptionsInitialized = true;
            }
        }

        private void HandleUnlearnAbility(AbilityProperties abilityProperties) {
            RemoveAbility(abilityProperties);
        }

        private void HandleLearnAbility(UnitController controller, AbilityProperties abilityProperties) {
            if (abilityProperties.AutoAddToBars == false) {
                //Debug.Log($"{unitController.gameObject.name}.CharacterActionBarManager.HandleLearnAbility({abilityProperties.ResourceName}) - AutoAddToBars is false, not adding ability to action bar");
                return;
            }
            AddAbility(abilityProperties);
        }

        private void RemoveAbility(AbilityProperties abilityProperties) {

            for (int i = 0; i < gamepadActionButtons.Count; i++) {
                if (gamepadActionButtons[i].Useable != null && gamepadActionButtons[i].Useable == abilityProperties) {
                    UnSetGamepadActionButton(i);
                }
            }
            for (int i = 0; i < mouseActionButtons.Count; i++) {
                if (mouseActionButtons[i].Useable != null && mouseActionButtons[i].Useable == abilityProperties) {
                    UnSetMouseActionButton(i);
                }
            }
        }


        public bool AddAbility(AbilityProperties newAbility) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterActionBarManager.AddNewAbility({newAbility.ResourceName})");

            bool returnValue = false;
            bool foundSlot = false;
            if (AddGamepadSavedAbility(newAbility)) {
                returnValue = true;
                foundSlot = true;
            }
            if (foundSlot != true) {
                if (AddGamepadNewAbility(newAbility)) {
                    returnValue = true;
                }
            }

            foundSlot = false;
            if (AddMouseSavedAbility(newAbility)) {
                returnValue = true;
                foundSlot = true;
            }
            if (foundSlot != true) {
                if (AddMouseNewAbility(newAbility)) {
                    returnValue = true;
                }
            }

            return returnValue;
        }

        public bool AddMouseSavedAbility(AbilityProperties newAbility) {
            //Debug.Log($"{gameObject.name}.ActionBarController.AddSavedAbility({newAbility.ResourceName})");

            for (int i = 0; i < mouseActionButtons.Count; i++) {
                if (mouseActionButtons[i].Useable == null && mouseActionButtons[i].SavedUseable != null && mouseActionButtons[i].SavedUseable.ResourceName == newAbility.ResourceName) {
                    //Debug.Log("Adding ability: " + newAbility + " to empty action button " + i);
                    SetMouseActionButton(newAbility, i);
                    return true;
                } else if (mouseActionButtons[i].Useable == (newAbility as IUseable)) {
                    //Debug.Log("Ability exists on bars already!");
                    return true;
                }
            }
            return false;
        }

        public void SetMouseActionButton(IUseable useable, int index) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterActionBarController.SetMouseActionButton({useable.ResourceName}, {index})");
            if (index < 0 || index >= mouseActionButtons.Count) {
                //Debug.LogError($"{unitController.gameObject.name}.CharacterActionBarController.SetMouseActionButton(): index {index} is out of range for mouse action buttons");
                return;
            }
            mouseActionButtons[index].Useable = useable;
            unitController.UnitEventController.NotifyOnSetMouseActionButton(useable, index);
        }

        public void SetGamepadActionButton(IUseable useable, int index) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterActionBarController.SetGamepadActionButton({useable.ResourceName}, {index})");
            if (index < 0 || index >= gamepadActionButtons.Count) {
                //Debug.LogError($"{unitController.gameObject.name}.CharacterActionBarController.SetGamepadActionButton(): index {index} is out of range for gamepad action buttons");
                return;
            }
            gamepadActionButtons[index].Useable = useable;
            unitController.UnitEventController.NotifyOnSetGamepadActionButton(useable, index);
        }

        public bool AddMouseNewAbility(AbilityProperties newAbility) {
            //Debug.Log($"{unitController.gameObject.name}.AddMouseNewAbility.AddNewAbility({newAbility.ResourceName})");

            for (int i = 0; i < mouseActionButtons.Count; i++) {
                if (mouseActionButtons[i].Useable == null) {
                    //Debug.Log("Adding ability: " + newAbility + " to empty action button " + i);
                    SetMouseActionButton(newAbility, i);
                    return true;
                } else if (mouseActionButtons[i].Useable == (newAbility as IUseable)) {
                    //Debug.Log("Ability exists on bars already!");
                    return true;
                }
            }
            return false;
        }

        public bool AddGamepadSavedAbility(AbilityProperties newAbility) {
            //Debug.Log("AbilityBarController.AddNewAbility(" + newAbility + ")");
            for (int i = 0; i < gamepadActionButtons.Count; i++) {
                if (gamepadActionButtons[i].Useable == null
                    && gamepadActionButtons[i].SavedUseable != null
                    && gamepadActionButtons[i].SavedUseable.ResourceName == newAbility.ResourceName) {
                    SetGamepadActionButton(newAbility, i);
                    return true;
                } else if (gamepadActionButtons[i].Useable == (newAbility as IUseable)) {
                    //Debug.Log("Ability exists on bars already!");
                    return true;
                }
            }
            return false;
        }

        public bool AddGamepadNewAbility(AbilityProperties newAbility) {
            //Debug.Log("ActionBarManager.AddGamepadNewAbility(" + newAbility + ")");
            for (int i = 0; i < gamepadActionButtons.Count; i++) {
                if (gamepadActionButtons[i].Useable == null) {
                    SetGamepadActionButton(newAbility, i);
                    return true;
                } else if (gamepadActionButtons[i].Useable == (newAbility as IUseable)) {
                    //Debug.Log("Ability exists on bars already!");
                    return true;
                }
            }
            return false;
        }

        public void UnSetGamepadActionButton(int buttonIndex) {
            gamepadActionButtons[buttonIndex].SavedUseable = gamepadActionButtons[buttonIndex].Useable;
            gamepadActionButtons[buttonIndex].Useable = null;
            unitController.UnitEventController.NotifyOnUnsetGamepadActionButton(buttonIndex);
        }

        public void UnSetMouseActionButton(int buttonIndex) {
            mouseActionButtons[buttonIndex].SavedUseable = mouseActionButtons[buttonIndex].Useable;
            mouseActionButtons[buttonIndex].Useable = null;
            unitController.UnitEventController.NotifyOnUnsetMouseActionButton(buttonIndex);
        }

        public void RequestMoveMouseUseable(int oldIndex, int newIndex) {
            if (systemGameManager.GameMode == GameMode.Local) {
                MoveMouseUseable(oldIndex, newIndex);
            } else {
                unitController.UnitEventController.NotifyOnRequestMoveMouseUseable(oldIndex, newIndex);
            }
        }

        public void RequestAssignMouseUseable(IUseable useable, int buttonIndex) {
            if (systemGameManager.GameMode == GameMode.Local) {
                SetMouseActionButton(useable, buttonIndex);
            } else {
                unitController.UnitEventController.NotifyOnRequestAssignMouseUseable(useable, buttonIndex);
            }
        }

        public void RequestClearMouseUseable(int buttonIndex) {
            if (systemGameManager.GameMode == GameMode.Local) {
                UnSetMouseActionButton(buttonIndex);
            } else {
                unitController.UnitEventController.NotifyOnRequestClearMouseUseable(buttonIndex);
            }
        }

        public void MoveMouseUseable(int oldIndex, int newIndex) {
            //Debug.Log($"ActionBarManager.MoveMouseUseable({oldIndex}, {newIndex})");

            IUseable oldUseable = mouseActionButtons[newIndex].Useable;

            if (mouseActionButtons[oldIndex].Useable != null) {
                SetMouseActionButton(mouseActionButtons[oldIndex].Useable, newIndex);
            } else {
                UnSetMouseActionButton(newIndex);
            }

            if (oldUseable != null) {
                SetMouseActionButton(oldUseable, oldIndex);
            } else {
                UnSetMouseActionButton(oldIndex);
            }

        }


        public void RequestMoveGamepadUseable(int oldIndex, int newIndex) {
            if (systemGameManager.GameMode == GameMode.Local) {
                MoveGamepadUseable(oldIndex, newIndex);
            } else {
                unitController.UnitEventController.NotifyOnRequestMoveGamepadUseable(oldIndex, newIndex);
            }
        }

        public void RequestAssignGamepadUseable(IUseable useable, int buttonIndex) {
            if (systemGameManager.GameMode == GameMode.Local) {
                //AssignGamepadUseable(useable, buttonIndex);
                SetGamepadActionButton(useable, buttonIndex);
            } else {
                unitController.UnitEventController.NotifyOnRequestAssignGamepadUseable(useable, buttonIndex);
            }
        }

        public void RequestClearGamepadUseable(int buttonIndex) {
            if (systemGameManager.GameMode == GameMode.Local) {
                UnSetGamepadActionButton(buttonIndex);
            } else {
                unitController.UnitEventController.NotifyOnRequestClearGamepadUseable(buttonIndex);
            }
        }

        /*
        public void AssignGamepadUseable(IUseable useable, int index) {
            //Debug.Log($"ActionBarManager.AssignUseableByIndex({index})");

            gamepadActionButtons[index].Useable = useable;
        }
        */

        public void MoveGamepadUseable(int oldIndex, int newIndex) {
            //Debug.Log($"ActionBarManager.MoveGamepadUseable({oldIndex}, {newIndex})");

            IUseable oldUseable = gamepadActionButtons[newIndex].Useable;

            if (gamepadActionButtons[oldIndex].Useable != null) {
                SetGamepadActionButton(gamepadActionButtons[oldIndex].Useable, newIndex);
            } else {
                UnSetGamepadActionButton(newIndex);
            }

            if (oldUseable != null) {
                SetGamepadActionButton(oldUseable, oldIndex);
            } else {
                UnSetGamepadActionButton(oldIndex);
            }
        }

    }

}