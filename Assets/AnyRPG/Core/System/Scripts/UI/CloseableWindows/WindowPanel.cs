using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class WindowPanel : CloseableWindowContents {

        [Header("Window Panel")]

        [SerializeField]
        protected CanvasGroup canvasGroup = null;

        public virtual void HidePanel() {
            DisablePanelDisplay();
        }

        public void DisablePanelDisplay() {
            if (canvasGroup == null) {
                return;
            }
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        public virtual void ShowPanel() {
            EnablePanelDisplay();
        }

        protected void EnablePanelDisplay() {
            if (canvasGroup == null) {
                return;
            }
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

    }

}