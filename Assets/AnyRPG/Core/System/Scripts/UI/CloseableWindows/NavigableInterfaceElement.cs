using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class NavigableInterfaceElement : CloseableWindowContents {

        [Header("Navigable Interface Element")]

        [SerializeField]
        protected Image outline = null;

        protected Color hiddenColor = new Color32(0, 0, 0, 0);

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            UnFocus();
        }

        public virtual void Focus() {
            outline.color = Color.white;
        }

        public virtual void UnFocus() {
            outline.color = hiddenColor;
            HideControllerHints();
        }

    }

}