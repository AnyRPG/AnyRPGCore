using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace AnyRPG {
    public class GuildMemberButton : HighlightButton, IPointerClickHandler {

        [Header("Guild Member Button")]

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
        protected TextMeshProUGUI kickCharacterText = null;

        [SerializeField]
        protected HighlightButton promoteCharacterButton = null;

        [SerializeField]
        protected HighlightButton messageButton = null;

        private GuildMemberData guildMemberData = null;

        // game manager references
        private PlayerManager playerManager = null;
        private GuildServiceClient guildServiceClient = null;

        public GuildMemberData GuildMemberData { get => guildMemberData; set => guildMemberData = value; }

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
            guildServiceClient = systemGameManager.GuildServiceClient;
        }

        public void AddGuildMember(GuildMemberData guildMemberData) {
            //Debug.Log($"{gameObject.name}.AuctionButton.AddMessage({mailMessage.MessageId})");

            this.guildMemberData = guildMemberData;

            string extraInfoString = string.Empty;
            if (playerManager.UnitController.CharacterId == guildMemberData.CharacterSummaryData.CharacterId) {
                extraInfoString = " (You)";
            }
            characterNameText.text = $"{guildMemberData.CharacterSummaryData.CharacterName}{extraInfoString}";
            characterLevelText.text = $"{this.guildMemberData.CharacterSummaryData.Level}";
            rankText.text = guildMemberData.Rank.ToString();
            statusText.text = guildMemberData.CharacterSummaryData.IsOnline == true ? "Online" : "Offline";
            if (guildMemberData.CharacterSummaryData.CharacterClass != null) {
                describableIcon.SetDescribable(guildMemberData.CharacterSummaryData.CharacterClass);
                //classImage.sprite = characterSummaryData.CharacterClass.Icon;
            } else {
                describableIcon.SetDescribable(null);
                classImage.sprite = systemConfigurationManager.UIConfiguration.DefaultFactionIcon;
            }


            if (guildMemberData.CharacterSummaryData.IsOnline == true) {
                characterLevelText.color = Color.white;
                zoneNameText.color = Color.white;
				zoneNameText.text = this.guildMemberData.CharacterSummaryData.CurrentZoneName;
                rankText.color = Color.white;
				if (guildMemberData.CharacterSummaryData.CharacterId != playerManager.UnitController.CharacterId) {
                    messageButton.gameObject.SetActive(true);
                } else {
                    messageButton.gameObject.SetActive(false);
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
            GuildRank playerRank = guildServiceClient.CurrentGuild.MemberList[playerManager.UnitController.CharacterId].Rank;
            if (guildMemberData.CharacterSummaryData.CharacterId == playerManager.UnitController.CharacterId) {
                promoteCharacterButton.gameObject.SetActive(false);
                kickCharacterButton.gameObject.SetActive(false);
            } else if (playerRank == GuildRank.Leader) {
                promoteCharacterButton.gameObject.SetActive(guildMemberData.Rank != GuildRank.Leader);
                if (guildMemberData.Rank == GuildRank.Officer) {
                    kickCharacterText.text = "Demote";
                } else {
                    kickCharacterText.text = "Kick";
                }
                kickCharacterButton.gameObject.SetActive(true);
            } else if (playerRank == GuildRank.Officer) {
                if (guildMemberData.Rank == GuildRank.Member) {
                    promoteCharacterButton.gameObject.SetActive(true);
                    kickCharacterButton.gameObject.SetActive(true);
                    kickCharacterText.text = "Kick";
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
            guildMemberData = null;
            characterNameText.text = string.Empty;
            statusText.text = string.Empty;
        }

        public void KickCharacter() {
            if (kickCharacterText.text == "Demote") {
                guildServiceClient.RequestDemoteCharacter(guildMemberData.CharacterSummaryData.CharacterId);
            } else {
                guildServiceClient.RequestRemoveCharacterFromGuild(guildMemberData.CharacterSummaryData.CharacterId);
            }
        }

        public void PromoteCharacter() {
            //Debug.Log("GuildMemberButton.PromoteCharacter()");

            guildServiceClient.RequestPromoteCharacter(guildMemberData.CharacterSummaryData.CharacterId);
        }

        public void MessageCharacter() {
            systemGameManager.MessageLogClient.BeginPrivateMessage($"{systemConfigurationManager.PrivateMessageChatCommand} \"{guildMemberData.CharacterSummaryData.CharacterName}\" ");
        }

    }

}