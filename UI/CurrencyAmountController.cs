using AnyRPG;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

namespace AnyRPG {
    public class CurrencyAmountController : MonoBehaviour {

        [SerializeField]
        private DescribableIcon currencyIcon;

        [SerializeField]
        private TextMeshProUGUI amountText;

        public DescribableIcon MyCurrencyIcon { get => currencyIcon; set => currencyIcon = value; }
        public TextMeshProUGUI MyAmountText { get => amountText; set => amountText = value; }
    }


}