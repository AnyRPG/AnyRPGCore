using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Gain Currency Command", menuName = "AnyRPG/Chat Commands/Gain Currency Command")]
    public class GainCurrencyCommand : ChatCommand {

        [Header("Gain Currency Command")]

        [Tooltip("If true, all parameters will be ignored, and the item provided will be the item listed below")]
        [SerializeField]
        private bool fixedCurrency = false;

        [Tooltip("Only applies if fixedItem is true")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(Currency))]
        private string currencyName = string.Empty;

        private Currency currency = null;

        [Tooltip("If true, all parameters will be ignored, and the amount given will be the amount listed below")]
        [SerializeField]
        private bool fixedAmount = false;

        [Tooltip("Only applies if fixedAmount is true")]
        [SerializeField]
        private int currencyAmount = 0;


        // game manager references
        //SystemCurrencyManager systemItemManager = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            //systemItemManager = systemGameManager.SystemItemManager;
        }

        public override void ExecuteCommand(string commandParameters) {
            //Debug.Log("GainCurrencyCommand.ExecuteCommand() Executing command " + DisplayName + " with parameters (" + commandParameters + ")");

            // add a fixed item
            if (fixedCurrency == true && fixedAmount == true) {
                AddCurrency(currency, currencyAmount);
                return;
            }

            // the currency or amount comes from parameters, but none were given
            if (commandParameters == string.Empty) {
                return;
            }

            // currency is fixed, but amount is not
            // try to get the amount
            if (fixedCurrency == true && fixedAmount == false) {
                int i = 0;
                bool result = int.TryParse(commandParameters, out i);
                if (result == true) {
                    AddCurrency(currency, i);
                }
            }

            if (commandParameters.Contains(" ") == true) {
                string[] parameterList = commandParameters.Split(' ');

                bool amountFound = false;
                int amountIndex = -1;
                if (int.TryParse(parameterList[0], out currencyAmount) == true) {
                    amountFound = true;
                    amountIndex = 0;
                } else if (int.TryParse(parameterList[parameterList.Length - 1], out currencyAmount) == true) {
                    amountFound = true;
                    amountIndex = parameterList.Length - 1;
                }

                if (amountFound == true) {
                    currencyName = string.Empty;
                    // join the parameters into the currency name, ignoring the parameter that represented the value
                    for (int i = 0; i < parameterList.Length; i++) {
                        if (amountIndex != i) {
                            currencyName += parameterList[i];
                        }
                    }

                    // get the currency from the factory and add the amount found
                    currency = systemDataFactory.GetResource<Currency>(currencyName);
                    if (currency != null) {
                        AddCurrency(currency, currencyAmount);
                    }
                }

            }

        }

        private void AddCurrency(Currency currency, int amount) {
            //Debug.Log("GainCurrencyCommand.AddCurrency(" + currency.DisplayName + ", " + amount + ")");

            playerManager.UnitController.CharacterCurrencyManager.AddCurrency(currency, amount);
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            if (fixedCurrency == true && currencyName != null && currencyName != string.Empty) {
                Currency tmpCurrency = systemDataFactory.GetResource<Currency>(currencyName);
                if (tmpCurrency != null) {
                    currency = tmpCurrency;
                } else {
                    Debug.LogError("GainItemCommand.SetupScriptableObjects(): Could not find currency : " + currencyName + " while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
                }
            }
        }

    }

}