using UnityEngine;
using TMPro;
using System;

namespace AnyRPG {
    public class CurrencyEntryAmountController : ConfiguredMonoBehaviour {

        [SerializeField]
        private DescribableIcon currencyIcon;

        [SerializeField]
        private TMP_InputField textInput;

        private Currency currency = null;

        public DescribableIcon CurrencyIcon { get => currencyIcon; set => currencyIcon = value; }
        public Currency Currency { get => currency; }
        public TMP_InputField TextInput { get => textInput; set => textInput = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            //Debug.Log($"CurrencyEntryAmountController.Configure()");

            base.Configure(systemGameManager);

            currencyIcon.Configure(systemGameManager);
        }

        public void SetCurrency(Currency currency) {
            //Debug.Log($"CurrencyEntryAmountController.SetCurrency({currency.ResourceName})");

            this.currency = currency;
            currencyIcon.Icon.sprite = currency.icon;
            currencyIcon.SetDescribable(currency);
        }

        public void DisableTooltip() {
            currencyIcon.DisableTooltip();
        }

        public void SetToolTipTransform(RectTransform toolTipTransform) {
            currencyIcon.SetToolTipTransform(toolTipTransform);
        }

        public void SetAmountTooHigh() {
            currencyIcon.Icon.color = Color.red;
        }

        public void SetAmountValid() {
            currencyIcon.Icon.color = Color.white;
        }

        public void SetAmount(int currencyValue) {
            textInput.text = currencyValue.ToString();
        }
    }


}