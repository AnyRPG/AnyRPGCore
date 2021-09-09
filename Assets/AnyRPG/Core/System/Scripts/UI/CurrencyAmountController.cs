using AnyRPG;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

namespace AnyRPG {
    public class CurrencyAmountController : ConfiguredMonoBehaviour {

        [SerializeField]
        private DescribableIcon currencyIcon;

        [SerializeField]
        private TextMeshProUGUI amountText;

        public DescribableIcon CurrencyIcon { get => currencyIcon; set => currencyIcon = value; }
        public TextMeshProUGUI AmountText { get => amountText; set => amountText = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            currencyIcon.Configure(systemGameManager);
        }
    }


}