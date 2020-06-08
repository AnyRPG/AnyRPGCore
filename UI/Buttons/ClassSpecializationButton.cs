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
        private ClassSpecialization classSpecialization = null;

        [SerializeField]
        private Image icon = null;

        [SerializeField]
        private TextMeshProUGUI classSpecializationName = null;

        [SerializeField]
        private TextMeshProUGUI description = null;

        public void AddClassSpecialization(ClassSpecialization newClassSpecialization) {
            classSpecialization = newClassSpecialization;
            icon.sprite = this.classSpecialization.MyIcon;
            icon.color = Color.white;
            classSpecializationName.text = classSpecialization.MyDisplayName;
            //description.text = this.faction.GetSummary();
            description.text = classSpecialization.GetSummary();

        }

        public void ClearClassSpecialization() {
            icon.sprite = null;
            icon.color = new Color32(0, 0, 0, 0);
            classSpecializationName.text = string.Empty;
            description.text = string.Empty;
        }


    }

}