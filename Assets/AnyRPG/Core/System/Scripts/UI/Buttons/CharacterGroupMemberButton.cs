using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace AnyRPG {
    public class CharacterGroupMemberButton : HighlightButton, IPointerClickHandler {

        [Header("Character Group Member Button")]

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
        protected TextMeshProUGUI rankText = null;

        [SerializeField]
        protected TextMeshProUGUI statusText = null;

        [SerializeField]
        protected HighlightButton kickCharacterButton = null;

        [SerializeField]
        protected TextMeshProUGUI kickButtonText = null;

        [SerializeField]
        protected HighlightButton promoteCharacterButton = null;

        [SerializeField]
        protected HighlightButton messageButton = null;

        private CharacterGroupMemberData characterGroupMemberData = null;

        // game manager references
        private PlayerManager playerManager = null;
        private CharacterGroupServiceClient characterGroupServiceClient = null;

        public CharacterGroupMemberData CharacterGroupMemberData { get => characterGroupMemberData; set => characterGroupMemberData = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            promoteCharacterButton.Configure(systemGameManager);
            kickCharacterButton.Configure(systemGameManager);
            messageButton.Configure(systemGameManager);
            describableIcon.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
            characterGroupServiceClient = systemGameManager.CharacterGroupServiceClient;
        }

        public void AddGroupMember(CharacterGroupMemberData characterGroupMemberData) {
            //Debug.Log($"{gameObject.name}.AuctionButton.AddMessage({mailMessage.MessageId})");

            this.characterGroupMemberData = characterGroupMemberData;
            string extraInfoString = string.Empty;
            if (playerManager.UnitController.CharacterId == characterGroupMemberData.CharacterSummaryData.CharacterId) {
                extraInfoString = " (You)";
            }
            characterNameText.text = $"{characterGroupMemberData.CharacterSummaryData.CharacterName}{extraInfoString}";
            characterLevelText.text = $"{characterGroupMemberData.CharacterSummaryData.Level}";
            rankText.text = characterGroupMemberData.Rank.ToString();
            rankText.color = Color.white;
            statusText.text = characterGroupMemberData.CharacterSummaryData.IsOnline == true ? "Online" : "Offline";
            if (characterGroupMemberData.CharacterSummaryData.CharacterClass != null) {
                //classImage.sprite = characterSummaryData.CharacterClass.Icon;
                describableIcon.SetDescribable(characterGroupMemberData.CharacterSummaryData.CharacterClass);
            } else {
                describableIcon.SetDescribable(null);
                classImage.sprite = systemConfigurationManager.UIConfiguration.DefaultFactionIcon;
            }

            if (characterGroupMemberData.CharacterSummaryData.IsOnline == true) {
                characterLevelText.color = Color.white;
                zoneNameText.text = characterGroupMemberData.CharacterSummaryData.CurrentZoneName;
                zoneNameText.color = Color.white;
                if (characterGroupMemberData.CharacterSummaryData.CharacterId == playerManager.UnitController.CharacterId) {
                    messageButton.gameObject.SetActive(false);
                } else {
                    messageButton.gameObject.SetActive(true);
                }
                statusText.color = Color.green;
                characterNameText.color = Color.white;
            } else {
                characterLevelText.color = Color.gray;
                zoneNameText.text = "Offline";
                zoneNameText.color = Color.gray;
                rankText.color = Color.gray;
                messageButton.gameObject.SetActive(false);
                statusText.color = Color.gray;
                characterNameText.color = Color.gray;
            }

            if (characterGroupServiceClient.CurrentCharacterGroup.leaderPlayerCharacterId == playerManager.UnitController.CharacterId
                && characterGroupMemberData.CharacterSummaryData.CharacterId != playerManager.UnitController.CharacterId) {
                promoteCharacterButton.gameObject.SetActive(true);
                kickCharacterButton.gameObject.SetActive(true);
            } else {
                promoteCharacterButton.gameObject.SetActive(false);
                kickCharacterButton.gameObject.SetActive(false);
            }
            CharacterGroupRank playerRank = characterGroupServiceClient.CurrentCharacterGroup.MemberList[UnitControllerMode.Player][playerManager.UnitController.CharacterId].Rank;
            if (characterGroupMemberData.CharacterSummaryData.CharacterId == playerManager.UnitController.CharacterId) {
                promoteCharacterButton.gameObject.SetActive(false);
                kickCharacterButton.gameObject.SetActive(false);
            } else if (playerRank == CharacterGroupRank.Leader) {
                promoteCharacterButton.gameObject.SetActive(characterGroupMemberData.Rank != CharacterGroupRank.Leader);
                if (characterGroupMemberData.Rank == CharacterGroupRank.Assistant) {
                    kickButtonText.text = "Demote";
                } else {
                    kickButtonText.text = "Kick";
                }
                kickCharacterButton.gameObject.SetActive(true);
            } else if (playerRank == CharacterGroupRank.Assistant) {
                if (characterGroupMemberData.Rank == CharacterGroupRank.Member) {
                    promoteCharacterButton.gameObject.SetActive(true);
                    kickCharacterButton.gameObject.SetActive(true);
                    kickButtonText.text = "Kick";
                } else {
                    promoteCharacterButton.gameObject.SetActive(false);
                    kickCharacterButton.gameObject.SetActive(false);
                }
            } else {
                promoteCharacterButton.gameObject.SetActive(false);
                kickCharacterButton.gameObject.SetActive(false);
            }
        }

        public void ClearButton() {
            characterGroupMemberData = null;
            characterNameText.text = string.Empty;
            statusText.text = string.Empty;
        }

        public void KickCharacter() {
            if (kickButtonText.text == "Demote") {
                characterGroupServiceClient.RequestDemoteGroupCharacter(characterGroupMemberData.CharacterSummaryData.CharacterId);
                return;
            }
            characterGroupServiceClient.RequestRemoveCharacterFromGroup(characterGroupMemberData.CharacterSummaryData.CharacterId);

        }

        public void PromoteCharacter() {
            characterGroupServiceClient.RequestPromoteCharacterToLeader(characterGroupMemberData.CharacterSummaryData.CharacterId);
        }

        public void MessageCharacter() {
            systemGameManager.MessageLogClient.BeginPrivateMessage($"{systemConfigurationManager.PrivateMessageChatCommand} \"{characterGroupMemberData.CharacterSummaryData.CharacterName}\" ");
        }

    }

}