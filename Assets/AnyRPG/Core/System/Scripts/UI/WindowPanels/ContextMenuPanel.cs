using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class ContextMenuPanel : WindowPanel {

        [Header("Context Menu Panel")]

        [SerializeField]
        private HighlightButton inspectButton = null;

        [SerializeField]
        private HighlightButton messageButton = null;

        [SerializeField]
        private HighlightButton tradeButton = null;

        [SerializeField]
        private HighlightButton inviteButton = null;

        [SerializeField]
        private HighlightButton kickButton = null;

        [SerializeField]
        private HighlightButton disbandButton = null;

        [SerializeField]
        private HighlightButton leaveButton = null;

        [SerializeField]
        private HighlightButton promoteButton = null;

        [SerializeField]
        private HighlightButton splitButton = null;

        [SerializeField]
        private HighlightButton destroyButton = null;

        [SerializeField]
        private HighlightButton dropButton = null;

        [SerializeField]
        private UINavigationController uINavigationController = null;

        // game manager references
        private ContextMenuService contextMenuService = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            contextMenuService = systemGameManager.ContextMenuService;
        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("ContextMenuPanel.ProcessOpenWindowNotification()");

            base.ProcessOpenWindowNotification();
            if (contextMenuService.ContextMenuTarget != null) {
                SetupButtons();
            } else {
                contextMenuService.CloseContextMenu();
            }
        }

        private void SetupButtons() {
            //Debug.Log("ContextMenuPanel.SetupButtons()");

            HideAllButtons();
            if (contextMenuService.ContextMenuTarget != null) {
                contextMenuService.ContextMenuTarget.SetupContextMenu(this);
            }

            uINavigationController.UpdateNavigationList();

            if (uINavigationController.ActiveNavigableButtonCount == 0) {
                contextMenuService.CloseContextMenu();
            }
        }

        private void HideAllButtons() {
            //Debug.Log("ContextMenuPanel.HideAllButtons()");

            inspectButton.gameObject.SetActive(false);
            messageButton.gameObject.SetActive(false);
            tradeButton.gameObject.SetActive(false);
            inviteButton.gameObject.SetActive(false);
            kickButton.gameObject.SetActive(false);
            disbandButton.gameObject.SetActive(false);
            leaveButton.gameObject.SetActive(false);
            promoteButton.gameObject.SetActive(false);
            splitButton.gameObject.SetActive(false);
            destroyButton.gameObject.SetActive(false);
            dropButton.gameObject.SetActive(false);
        }

        public void EnableInspectButton(bool active) {
            inspectButton.gameObject.SetActive(active);
        }

        public void EnableTradeButton(bool active) {
            tradeButton.gameObject.SetActive(active);
        }

        public void EnableMessageButton(bool active) {
            messageButton.gameObject.SetActive(active);
        }

        public void EnableInviteButton(bool active) {
            inviteButton.gameObject.SetActive(active);
        }

        public void EnableKickButton(bool active) {
            kickButton.gameObject.SetActive(active);
        }

        public void EnablePromoteButton(bool active) {
            promoteButton.gameObject.SetActive(active);
        }

        public void EnableDisbandButton(bool active) {
            disbandButton.gameObject.SetActive(active);
        }

        public void EnableLeaveButton(bool active) {
            leaveButton.gameObject.SetActive(active);
        }

        public void EnableSplitButton(bool active) {
            splitButton.gameObject.SetActive(active);
        }

        public void EnableDestroyButton(bool active) {
            //Debug.Log($"ContextMenuPanel.EnableDestroyButton({active})");

            destroyButton.gameObject.SetActive(active);
        }

        public void EnableDropButton(bool active) {
            dropButton.gameObject.SetActive(active);
        }

        public void Inspect() {
            //Debug.Log("ContextMenuPanel.Inspect()");
            if (contextMenuService.ContextMenuTarget == null) {
                return;
            }
            contextMenuService.ContextMenuTarget.PerformContextMenuAction("Inspect");
            contextMenuService.CloseContextMenu();
        }

        public void Trade() {
            //Debug.Log("ContextMenuPanel.Trade()");

            contextMenuService.ContextMenuTarget.PerformContextMenuAction("Trade");
            contextMenuService.CloseContextMenu();
        }

        public void Message() {
            //Debug.Log("ContextMenuPanel.Message()");

            contextMenuService.ContextMenuTarget.PerformContextMenuAction("Message");
            contextMenuService.CloseContextMenu();
        }

        public void Invite() {
            //Debug.Log("ContextMenuPanel.Invite()");
            contextMenuService.ContextMenuTarget.PerformContextMenuAction("Invite");
            contextMenuService.CloseContextMenu();
        }

        public void Kick() {
            //Debug.Log("ContextMenuPanel.Kick()");
            contextMenuService.ContextMenuTarget.PerformContextMenuAction("Kick");
            contextMenuService.CloseContextMenu();
        }

        public void Promote() {
            //Debug.Log("ContextMenuPanel.Promote()");

            contextMenuService.ContextMenuTarget.PerformContextMenuAction("Promote");
            contextMenuService.CloseContextMenu();
        }

        public void Disband() {
            //Debug.Log("ContextMenuPanel.Disband()");

            contextMenuService.ContextMenuTarget.PerformContextMenuAction("Disband");
            contextMenuService.CloseContextMenu();
        }

        public void Leave() {
            //Debug.Log("ContextMenuPanel.Leave()");

            contextMenuService.ContextMenuTarget.PerformContextMenuAction("Leave");
            contextMenuService.CloseContextMenu();
        }

        public void Split() {
            //Debug.Log("ContextMenuPanel.Split()");

            contextMenuService.ContextMenuTarget.PerformContextMenuAction("Split");
            contextMenuService.CloseContextMenu();
        }

        public void Destroy() {
            //Debug.Log("ContextMenuPanel.Destroy()");

            contextMenuService.ContextMenuTarget.PerformContextMenuAction("Destroy");
            contextMenuService.CloseContextMenu();
        }

        public void Drop() {
            //Debug.Log("ContextMenuPanel.Drop()");

            contextMenuService.ContextMenuTarget.PerformContextMenuAction("Drop");
            contextMenuService.CloseContextMenu();
        }

    }

}