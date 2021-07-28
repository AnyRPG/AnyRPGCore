using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemKeyConfirmPanelController : WindowContentController {

        public void CancelBind() {
            SystemGameManager.Instance.KeyBindManager.CancelKeyBind();
        }

    }

}