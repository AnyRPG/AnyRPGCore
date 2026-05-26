using UnityEngine;

namespace AnyRPG {
    public class FloatingCastbarPanel : NavigableInterfaceElement {

        [Header("Floating Castbar Panel")]

        [SerializeField]
        protected CastBarController castBarController = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            castBarController.Configure(systemGameManager);
            castBarController.SetCloseableWindow(closeableWindow);
        }

    }

}