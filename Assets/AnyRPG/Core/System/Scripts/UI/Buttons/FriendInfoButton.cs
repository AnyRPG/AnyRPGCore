using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace AnyRPG {
    public class FriendInfoButton : HighlightButton, IPointerClickHandler {

        [Header("Friend Info Button")]

        /*
        [SerializeField]
        protected Image attachmentBackgroundImage = null;
        */

        [SerializeField]
        protected Image classImage = null;

        [SerializeField]
        protected DescribableIcon describableIcon = null;

        [SerializeField]
        protected TextMeshProUGUI characterNameText = null;

        [SerializeField]
        protected TextMeshProUGUI characterLevelText = null;

        [SerializeField]
        protected TextMeshProUGUI zoneNameText = null;

        [SerializeField]
        protected TextMeshProUGUI statusText = null;

        [SerializeField]
        protected HighlightButton unfriendCharacterButton = null;

        [SerializeField]
        protected HighlightButton messageCharacterButton = null;

        private CharacterSummaryData characterSummaryData = null;

        // game manager references
        private PlayerManager playerManager = null;
        private FriendServiceClient friendServiceClient = null;

        public CharacterSummaryData CharacterSummaryData { get => characterSummaryData; set => characterSummaryData = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            unfriendCharacterButton.Configure(systemGameManager);
            messageCharacterButton.Configure(systemGameManager);
            describableIcon.Configure(systemGameManager);
		}

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
            friendServiceClient = systemGameManager.FriendServiceClient;
        }

        public void AddFriend(CharacterSummaryData characterSummaryData) {
            //Debug.Log($"{gameObject.name}.AuctionButton.AddMessage({mailMessage.MessageId})");

            this.characterSummaryData = characterSummaryData;
            characterNameText.text = $"{characterSummaryData.CharacterName}";
            characterLevelText.text = $"{characterSummaryData.Level}";
            statusText.text = characterSummaryData.IsOnline == true ? "Online" : "Offline";
            if (characterSummaryData.CharacterClass != null) {
                describableIcon.SetDescribable(characterSummaryData.CharacterClass);
                //classImage.sprite = characterSummaryData.CharacterClass.Icon;
            } else {
                describableIcon.SetDescribable(null);
                classImage.sprite = systemConfigurationManager.UIConfiguration.DefaultFactionIcon;
            }

            if (characterSummaryData.IsOnline == true) {
                characterLevelText.color = Color.white;
                zoneNameText.text = characterSummaryData.CurrentZoneName;
                zoneNameText.color = Color.white;
				messageCharacterButton.gameObject.SetActive(true);
                statusText.color = Color.green;
                characterNameText.color = Color.white;
            } else {
                characterLevelText.color = Color.gray;
				zoneNameText.text = "Offline";
                zoneNameText.color = Color.gray;
				messageCharacterButton.gameObject.SetActive(false);
                statusText.color = Color.gray;
                characterNameText.color = Color.gray;
            }
        }

        public void ClearButton() {
            characterSummaryData = null;
            characterNameText.text = string.Empty;
            statusText.text = string.Empty;
        }

        public void UnfriendCharacter() {
            //Debug.Log($"{gameObject.name}.FriendInfoButton.UnfriendCharacter()");

            friendServiceClient.RequestRemoveCharacterFromFriendList(characterSummaryData.CharacterId);
        }

        public void MessageCharacter() {
            systemGameManager.MessageLogClient.BeginPrivateMessage($"{systemConfigurationManager.PrivateMessageChatCommand} \"{characterSummaryData.CharacterName}\" ");
        }

    }

}