using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AnyRPG {
    public class CreateGuildPanel : WindowPanel {

        [Header("Create Guild Panel")]

        [SerializeField]
        private TMP_InputField textInput = null;

        [SerializeField]
        private CurrencyBarController currencyBarController = null;

        [SerializeField]
        private HighlightButton createButton = null;

        // game manager references
        private GuildmasterManagerClient guildMasterManagerClient = null;
        private GuildServiceClient guildServiceClient = null;
        private PlayerManager playerManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            currencyBarController.Configure(systemGameManager);
            guildServiceClient.OnJoinGuild += HandleJoinGuild;
        }

        private void HandleJoinGuild() {
            //Debug.Log("CreateGuildPanel.HandleJoinGuild()");

            Close();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            guildMasterManagerClient = systemGameManager.GuildmasterManagerClient;
            guildServiceClient = systemGameManager.GuildServiceClient;
            playerManager = systemGameManager.PlayerManager;
        }

        /// <summary>
        /// disable hotkeys and movement while text input is active
        /// </summary>
        public void ActivateTextInput() {
            controlsManager.ActivateTextInput();
        }

        public void DeativateTextInput() {
            controlsManager.DeactivateTextInput();
        }


        public void CancelAction() {
            //Debug.Log("NameChangePanelController.CancelAction()");
            Close();
        }

        public void ConfirmAction() {
            //Debug.Log("NameChangePanelController.ConfirmAction()");
            if (textInput.text != null && textInput.text != string.Empty) {
                guildMasterManagerClient.RequestCreateGuild(textInput.text);
                //uIManager.confirmCreateGuildWindow.OpenWindow();
            }
        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("NameChangePanelController.ProcessOpenWindowNotification()");
            base.ProcessOpenWindowNotification();
            textInput.text = string.Empty;
            currencyBarController.UpdateCurrencyAmount(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency, systemConfigurationManager.CreateGuildCurrencyAmount, "Fee:");

            // check that player has enough money and disable create button if not
            if (playerManager.UnitController.CharacterCurrencyManager.GetBaseCurrencyValue(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency) < systemConfigurationManager.CreateGuildCurrencyAmount) {
                createButton.Button.interactable = false;
            } else {
                createButton.Button.interactable = true;
            }
        }

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("NameChangePanelController.ReceiveClosedWindowNotification()");
            base.ReceiveClosedWindowNotification();
            guildMasterManagerClient.EndInteraction();
        }


    }

}