using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CombatLogWindow : CloseableWindow {

        [Header("Combat Log Window")]

        [SerializeField]
        protected DraggableWindow buttonsHeading = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            buttonsHeading.Configure(systemGameManager);
        }

    }

}