using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class MailNotificationIcon : DescribableIcon, IDescribable {

        [Header("Mail Notification Icon")]
        [SerializeField]
        private Image mailImage = null;

        public string ResourceName => "You have unread mail";

        public string DisplayName => "You have unread mail";

        public string Description => "You have unread mail";

        Sprite IDescribable.Icon => mailImage.sprite;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            SetDescribable(this);
        }

        public string GetDescription() {
            return "You have unread mail";
        }

        public string GetSummary() {
            return "You have unread mail";
        }

        public override void UpdateVisual() {
            // do not call base method.
        }

    }

}