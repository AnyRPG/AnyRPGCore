using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class AchievementButton : TransparencyButton {

        [SerializeField]
        protected Achievement achievement = null;

        [SerializeField]
        protected Image icon = null;

        [SerializeField]
        protected TextMeshProUGUI resourceNameField = null;

        [SerializeField]
        protected TextMeshProUGUI descriptionField = null;

        public void AddResource(Achievement achievement) {
            this.achievement = achievement;
            icon.sprite = this.achievement.Icon;
            icon.color = Color.white;
            resourceNameField.text = this.achievement.DisplayName;
            descriptionField.text = this.achievement.GetDescription();
        }

        public void ClearResource() {
            icon.sprite = null;
            icon.color = new Color32(0, 0, 0, 0);
            resourceNameField.text = string.Empty;
            descriptionField.text = string.Empty;
        }

    }

}