using TMPro;

namespace AnyRPG {
    public class OnScreenKeyboardManager : ConfiguredMonoBehaviour {

        // events
        public event System.Action<TMP_InputField> OnActivateKeyboard = delegate { };

        // game manager references
        private UIManager uIManager = null;


        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
        }

        public void ActivateKeyboard(TMP_InputField inputField) {
            //Debug.Log("OnScreenKeyboardManager.ActivateKeyboard()");

            OnActivateKeyboard(inputField);
            uIManager.onScreenKeyboardWindow.OpenWindow();
        }


    }

}