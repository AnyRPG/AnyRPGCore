using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class ClassSpecializationButton : TransparencyButton {

        [SerializeField]
        private ClassSpecialization classSpecialization;

        [SerializeField]
        private Image icon;

        [SerializeField]
        private Text classSpecializationName;

        [SerializeField]
        private Text description;

        public void AddClassSpecialization(ClassSpecialization newClassSpecialization) {
            classSpecialization = newClassSpecialization;
            icon.sprite = this.classSpecialization.MyIcon;
            icon.color = Color.white;
            classSpecializationName.text = classSpecialization.MyName;
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