using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CreditCategoryController : MonoBehaviour {

        [SerializeField]
        private TextMeshProUGUI titleText = null;

        public TextMeshProUGUI MyTitleText { get => titleText; set => titleText = value; }

    }
}

