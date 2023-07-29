using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Learn Ability Command", menuName = "AnyRPG/Chat Commands/Learn Ability Command")]
    public class LearnAbilityCommand : ChatCommand {

        [Header("Learn Ability Command")]

        [Tooltip("If true, all parameters will be ignored, and the ability learned will be the ability listed below")]
        [SerializeField]
        private bool fixedAbility = false;

        [Tooltip("Only applies if Fixed Ability is true")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(BaseAbility))]
        private string abilityName = string.Empty;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            //systemItemManager = systemGameManager.SystemItemManager;
        }

        public override void ExecuteCommand(string commandParameters) {
            //Debug.Log("GainItemCommand.ExecuteCommand() Executing command " + DisplayName + " with parameters (" + commandParameters + ")");

            // add a fixed item
            if (fixedAbility == true) {
                LearnAbility(abilityName);
                return;
            }

            // the item comes from parameters, but none were given
            if (commandParameters == string.Empty) {
                return;
            }

            // add an item from parameters
            LearnAbility(commandParameters);
        }

        private void LearnAbility(string abilityName) {
            BaseAbility tmpAbility = systemDataFactory.GetResource<BaseAbility>(abilityName);
            if (tmpAbility != null) {
                playerManager.UnitController.CharacterAbilityManager.LearnAbility(tmpAbility.AbilityProperties);
            }
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            if (fixedAbility == true && abilityName != null && abilityName != string.Empty) {
                BaseAbility tmpAbility = systemDataFactory.GetResource<BaseAbility>(abilityName);
                if (tmpAbility == null) {
                    Debug.LogError("LearnAbilityCommand.SetupScriptableObjects(): Could not find ability : " + abilityName + " while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
                }
            }
        }

    }

}