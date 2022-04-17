using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class NewGameClassSpecializationButton : HighlightButton {

        [SerializeField]
        protected ClassSpecialization classSpecialization = null;

        [SerializeField]
        protected Image icon = null;

        [SerializeField]
        protected TextMeshProUGUI characterClassName = null;

        [SerializeField]
        protected TextMeshProUGUI description = null;

        // game manager references
        protected NewGameManager newGameManager = null;

        public ClassSpecialization ClassSpecialization { get => classSpecialization; set => classSpecialization = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            newGameManager = systemGameManager.NewGameManager;
        }

        public void AddClassSpecialization(ClassSpecialization newClassSpecialization) {
            //Debug.Log("NewGameClassSpecializationbutton.AddClassSpecialization(" + (newClassSpecialization == null ? "null" : newClassSpecialization.DisplayName) + ")");
            classSpecialization = newClassSpecialization;
            if (classSpecialization != null) {
                icon.sprite = classSpecialization.Icon;
                icon.color = Color.white;
                characterClassName.text = classSpecialization.DisplayName;
                //description.text = this.faction.GetSummary();
                description.text = classSpecialization.GetDescription();
            } else {
                icon.sprite = systemConfigurationManager.DefaultFactionIcon;
                icon.color = Color.white;
                characterClassName.text = "None";
                //description.text = this.faction.GetSummary();
                description.text = "No specialization available";
            }

        }

        public void ClearClassSpecialization() {
            icon.sprite = null;
            icon.color = new Color32(0, 0, 0, 0);
            characterClassName.text = string.Empty;
            description.text = string.Empty;
        }

        public void CommonSelect() {
            newGameManager.SetClassSpecialization(classSpecialization);
        }

        public void RawSelect() {
            CommonSelect();
        }

        public override void Select() {
            CommonSelect();
            base.Select();
        }



    }

}