using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CreditCategoryController : MonoBehaviour {

        [SerializeField]
        private Text titleText = null;

        public Text MyTitleText { get => titleText; set => titleText = value; }

    }
}

