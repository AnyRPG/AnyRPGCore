using AnyRPG;
using UnityEngine;
using System.Collections.Generic;
using UMA.CharacterSystem;
using UMA;
using UMA.CharacterSystem.Examples;
using UnityEngine.UI;

namespace AnyRPG {
    public class CurrencyAmountController : MonoBehaviour {

        [SerializeField]
        private DescribableIcon currencyIcon;

        [SerializeField]
        private Text amountText;

        public DescribableIcon MyCurrencyIcon { get => currencyIcon; set => currencyIcon = value; }
        public Text MyAmountText { get => amountText; set => amountText = value; }
    }


}