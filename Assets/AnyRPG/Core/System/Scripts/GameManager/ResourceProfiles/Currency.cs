using AnyRPG;
using UnityEngine;
using UnityEngine.Serialization;
using System;
using System.Collections.Generic;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "NewCurrency", menuName = "AnyRPG/Currencies/Currency")]
    public class Currency : DescribableResource {

        // game manager references
        protected PlayerManager playerManager = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
        }

        public override string GetDescription() {
            return string.Format("Current Amount: {0}", playerManager.UnitController.CharacterCurrencyManager.GetCurrencyAmount(this));
            //return string.Format("{0}\nCurrent Amount: {1}", description, playerManager.UnitController.CharacterCurrencyManager.GetCurrencyAmount(this));
            //return string.Format("{0}", description);
        }

    }

    [System.Serializable]
    public struct CurrencyNode {

        public Currency currency;
        public int Amount;

    }


}