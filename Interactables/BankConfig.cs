using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Bank Config", menuName = "AnyRPG/Interactable/BankConfig")]
    public class BankConfig : InteractableOptionConfig {

        [SerializeField]
        private BankProps interactableOptionProps = new BankProps();

        public override Sprite Icon {
            get {
                if (SystemConfigurationManager.MyInstance.MyBankInteractionPanelImage != null) {
                    return SystemConfigurationManager.MyInstance.MyBankInteractionPanelImage;
                }
                return base.Icon;
            }
        }
        public override Sprite NamePlateImage {
            get {
                if (SystemConfigurationManager.MyInstance.MyBankNamePlateImage != null) {
                    return SystemConfigurationManager.MyInstance.MyBankNamePlateImage;
                }
                return base.NamePlateImage;
            }
        }

    }

}