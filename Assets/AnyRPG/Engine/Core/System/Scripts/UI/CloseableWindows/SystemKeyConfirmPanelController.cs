using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemKeyConfirmPanelController : WindowContentController {

        [SerializeField]
        private HighlightButton cancelButton = null;

        // game manager references
        private KeyBindManager keyBindManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            cancelButton.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            keyBindManager = systemGameManager.KeyBindManager;
        }

        public void CancelBind() {
            keyBindManager.CancelKeyBind();
        }

    }

}