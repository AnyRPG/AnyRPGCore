using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class ClassSpecializationButton : TransparencyButton {

        [SerializeField]
        protected ClassSpecialization classSpecialization = null;

        [SerializeField]
        protected Image icon = null;

        [SerializeField]
        protected TextMeshProUGUI classSpecializationName = null;

        [SerializeField]
        protected TextMeshProUGUI description = null;

        public void AddClassSpecialization(ClassSpecialization newClassSpecialization) {
            classSpecialization = newClassSpecialization;
            icon.sprite = this.classSpecialization.Icon;
            icon.color = Color.white;
            classSpecializationName.text = classSpecialization.DisplayName;
            //description.text = this.faction.GetSummary();
            description.text = classSpecialization.GetDescription();

        }

        public void ClearClassSpecialization() {
            icon.sprite = null;
            icon.color = new Color32(0, 0, 0, 0);
            classSpecializationName.text = string.Empty;
            description.text = string.Empty;
        }


    }

}