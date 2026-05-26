using TMPro;
using UnityEngine;

namespace AnyRPG {
    public class TextLogController : MonoBehaviour {

        [SerializeField]
        private TextMeshProUGUI text = null;

        public void InitializeTextLogController(string textToDisplay) {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();

            text.text = textToDisplay;
        }

    }

}