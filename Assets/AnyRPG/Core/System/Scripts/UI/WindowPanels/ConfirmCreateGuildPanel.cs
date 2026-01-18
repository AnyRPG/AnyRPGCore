using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AnyRPG {
    public class ConfirmCreateGuildPanel : WindowPanel {

        [Header("Create Guild Panel")]

        [SerializeField]
        private TMP_Text confirmText = null;

        [SerializeField]
        private CurrencyBarController currencyBarController = null;

        // game manager references
        private PlayerManager playerManager = null;
        private GuildmasterManagerClient guildMasterManagerClient = null;
        private GuildServiceClient guildServiceClient = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            currencyBarController.Configure(systemGameManager);
            guildServiceClient.OnGuildNameAvailable += HandleGuildNameAvailable;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
            guildMasterManagerClient = systemGameManager.GuildmasterManagerClient;
            guildServiceClient = systemGameManager.GuildServiceClient;
        }

        private void HandleGuildNameAvailable() {
            Open();
        }

        public void CancelAction() {
            //Debug.Log("NameChangePanelController.CancelAction()");
            Close();
        }

        public void ConfirmAction() {
            //Debug.Log("NameChangePanelController.ConfirmAction()");
            guildMasterManagerClient.RequestCreateGuild(playerManager.UnitController);
            Close();
        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("NameChangePanelController.ProcessOpenWindowNotification()");
            base.ProcessOpenWindowNotification();
            confirmText.text = $"Create guild <{guildMasterManagerClient.SavedGuildName}> for a fee of";
            currencyBarController.UpdateCurrencyAmount(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency, systemConfigurationManager.CreateGuildCurrencyAmount);
        }

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("NameChangePanelController.ReceiveClosedWindowNotification()");
            base.ReceiveClosedWindowNotification();
        }


    }

}