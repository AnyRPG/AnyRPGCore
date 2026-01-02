using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CurrencyEntryBarController : ConfiguredMonoBehaviour {

        public event System.Action OnRecalculateBaseCurrency = delegate { };

        protected bool eventSubscriptionsInitialized = false;

        private bool hideNonZeroAmounts = false;

        private CurrencyNode currencyNode = new CurrencyNode();

        // game manager references
        private CurrencyConverter currencyConverter = null;
        private PlayerManager playerManager = null;
        private ControlsManager controlsManager = null;

        [SerializeField]
        protected List<CurrencyEntryAmountController> currencyEntryAmountControllers = new List<CurrencyEntryAmountController>();

        public bool HideNonZeroAmounts { get => hideNonZeroAmounts; set => hideNonZeroAmounts = value; }
        public CurrencyNode CurrencyNode { get => currencyNode; }

        public override void Configure(SystemGameManager systemGameManager) {
            //Debug.Log($"CurrencyEntryBarController.Configure()");

            base.Configure(systemGameManager);

            currencyNode.currency = systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency;

            // intialize all controllers
            foreach (CurrencyEntryAmountController currencyEntryAmountController in currencyEntryAmountControllers) {
                currencyEntryAmountController.Configure(systemGameManager);
            }

            // configure only the necessary ones
            int i = 0;
            foreach (Currency currency in systemConfigurationManager.DefaultCurrencyGroup.GetCurrencyList()) {
                if (i >= currencyEntryAmountControllers.Count) {
                    break;
                }
                currencyEntryAmountControllers[i].SetCurrency(currency);
                i++;
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            currencyConverter = systemGameManager.CurrencyConverter;
            playerManager = systemGameManager.PlayerManager;
            controlsManager = systemGameManager.ControlsManager;
        }


        public void RecalculateBaseCurrency() {
            //Debug.Log($"CurrencyEntryBarController.RecalculateBaseCurrency()");

            currencyNode.Amount = 0;
            foreach (CurrencyEntryAmountController currencyEntryAmountController in currencyEntryAmountControllers) {
                int currencyValue = int.Parse(currencyEntryAmountController.TextInput.text);
                if (currencyValue > 0) {
                    currencyNode.Amount += currencyConverter.GetBaseCurrencyAmount(currencyEntryAmountController.Currency, currencyValue);
                }
            }

            if (currencyNode.Amount > playerManager.UnitController.CharacterCurrencyManager.GetBaseCurrencyValue(currencyNode.currency)) {
                foreach (CurrencyEntryAmountController currencyEntryAmountController in currencyEntryAmountControllers) {
                    currencyEntryAmountController.SetAmountTooHigh();
                }
                return;
            }
            foreach (CurrencyEntryAmountController currencyEntryAmountController in currencyEntryAmountControllers) {
                currencyEntryAmountController.SetAmountValid();
            }

            OnRecalculateBaseCurrency();
        }

        public void DisableTooltip() {
            foreach (CurrencyEntryAmountController currencyAmountController in currencyEntryAmountControllers) {
                currencyAmountController.DisableTooltip();
            }
        }

        public void SetToolTipTransform(RectTransform toolTipTransform) {
            foreach (CurrencyEntryAmountController currencyAmountController in currencyEntryAmountControllers) {
                currencyAmountController.SetToolTipTransform(toolTipTransform);
            }
        }

        /*
        public void HandlePointerClick() {
            //Debug.Log($"CurrencyEntryBarController.HandlePointerClick()");

            controlsManager.ActivateTextInput();
        }
        */

        public void HandleEndEdit() {
            //Debug.Log($"CurrencyEntryBarController.HandleEndEdit()");

            RecalculateBaseCurrency();
            controlsManager.DeactivateTextInput();
        }

        public void ResetCurrencyAmounts() {
            foreach (CurrencyEntryAmountController currencyEntryAmountController in currencyEntryAmountControllers) {
                currencyEntryAmountController.SetAmount(0);
            }
            currencyNode.Amount = 0;
        }

        public void UpdateCurrencyAmount(Currency currency, int currencyAmount) {
            //Debug.Log($"{gameObject.name}.CurrencyBarController.UpdateCurrencyAmount(" + currency.DisplayName + ", " + currencyAmount + ", " + priceString + ")");

            Dictionary<Currency, int> currencyList = currencyConverter.RedistributeCurrency(currency, currencyAmount);

            int counter = 0;
            foreach (KeyValuePair<Currency, int> currencyPair in currencyList) {
                if (currencyEntryAmountControllers.Count > counter) {
                    currencyEntryAmountControllers[counter].SetAmount(currencyPair.Value);
                }
                counter += 1;
            }

            RecalculateBaseCurrency();
        }

    }

}