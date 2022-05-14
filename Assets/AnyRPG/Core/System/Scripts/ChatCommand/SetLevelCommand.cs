using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Set Level Command", menuName = "AnyRPG/Chat Commands/Set Level Command")]
    public class SetLevelCommand : ChatCommand {

        [Header("Set Level Command")]

        [Tooltip("If true, all parameters will be ignored, and the level set will be the number listed below")]
        [SerializeField]
        private bool fixedLevel = false;

        [Tooltip("Only applies if Fixed Level is true")]
        [SerializeField]
        private int levelNumber = 0;


        public override void ExecuteCommand(string commandParameters) {
            //Debug.Log("SetLevelCommand.ExecuteCommand() Executing command " + DisplayName + " with parameters (" + commandParameters + ")");

            // add a fixed experience amount
            if (fixedLevel == true) {
                SetLevel(levelNumber);
                return;
            }

            // the currency or amount comes from parameters, but none were given
            if (commandParameters == string.Empty) {
                return;
            }

            // amount is not fixed
            // try to get the amount
            int i = 0;
            bool result = int.TryParse(commandParameters, out i);
            if (result == true) {
                SetLevel(i);
            }

        }

        private void SetLevel(int newLevel) {
            //Debug.Log("SetLevelCommand.SetLevel(" + newLevel + ")");

            newLevel = Mathf.Clamp(newLevel, playerManager.ActiveCharacter.CharacterStats.Level, systemConfigurationManager.MaxLevel);
            if (newLevel > playerManager.ActiveCharacter.CharacterStats.Level) {
                while (playerManager.ActiveCharacter.CharacterStats.Level < newLevel) {
                    playerManager.ActiveCharacter.CharacterStats.GainLevel();
                }
            }
        }

    }

}