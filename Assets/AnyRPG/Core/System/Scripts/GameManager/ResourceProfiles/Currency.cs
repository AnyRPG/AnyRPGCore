using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "NewCurrency", menuName = "AnyRPG/Currencies/Currency")]
    public class Currency : DescribableResource {

        // game manager references
        protected PlayerManagerClient playerManagerClient = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManagerClient = systemGameManager.PlayerManagerClient;
        }

        public override string GetDescription() {
            return string.Format("Current Amount: {0}", playerManagerClient.UnitController.CharacterCurrencyManager.GetCurrencyAmount(this));
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