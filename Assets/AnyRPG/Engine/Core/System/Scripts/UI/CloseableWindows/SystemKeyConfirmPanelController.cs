using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemKeyConfirmPanelController : WindowContentController {

        [SerializeField]
        private HighlightButton cancelButton = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            cancelButton.Configure(systemGameManager);
        }

        public void CancelBind() {
            SystemGameManager.Instance.KeyBindManager.CancelKeyBind();
        }

    }

}