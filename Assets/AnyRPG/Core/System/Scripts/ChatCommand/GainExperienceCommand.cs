using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Gain Experience Command", menuName = "AnyRPG/Chat Commands/Gain Experience Command")]
    public class GainExperienceCommand : ChatCommand {

        [Header("Gain Experience Command")]

        [Tooltip("If true, all parameters will be ignored, and the amount given will be the amount listed below")]
        [SerializeField]
        private bool fixedExperience = false;

        [Tooltip("Only applies if fixedExperience is true")]
        [SerializeField]
        private int experienceAmount = 0;


        public override void ExecuteCommand(string commandParameters, int accountId) {
            //Debug.Log("GainCurrencyCommand.ExecuteCommand() Executing command " + DisplayName + " with parameters (" + commandParameters + ")");

            // add a fixed experience amount
            if (fixedExperience == true) {
                AddExperience(experienceAmount, accountId);
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
                AddExperience(i, accountId);
            }

        }

        private void AddExperience(int amount, int accountId) {
            //Debug.Log("GainCurrencyCommand.AddCurrency(" + currency.DisplayName + ", " + amount + ")");

            playerManagerServer.GainXP(amount, accountId);
        }

    }

}