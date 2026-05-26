using TMPro;
using UnityEngine;

namespace AnyRPG {
    public class CreditCategoryController : MonoBehaviour {

        [SerializeField]
        private TextMeshProUGUI titleText = null;

        public TextMeshProUGUI MyTitleText { get => titleText; set => titleText = value; }

    }
}

