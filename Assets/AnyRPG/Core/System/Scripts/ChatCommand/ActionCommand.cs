using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Action Command", menuName = "AnyRPG/Chat Commands/Action Command")]
    public class ActionCommand : ChatCommand {

        [Header("Action Command")]

        [SerializeField]
        private AnimatedActionProperties actionProperties = new AnimatedActionProperties();


        public override void ExecuteCommand(string commandParameters) {
            Debug.Log("ActionCommand.ExecuteCommand() Executing command " + DisplayName + " with parameters (" + commandParameters + ")");

            playerManager.UnitController.UnitActionManager.BeginAction(actionProperties);
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            actionProperties.SetupScriptableObjects(systemGameManager, DisplayName);
        }

    }

}