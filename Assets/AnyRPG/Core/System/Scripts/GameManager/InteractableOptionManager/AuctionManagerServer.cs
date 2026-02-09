using System;
using System.Collections.Generic;

namespace AnyRPG {
    public class AuctionManagerServer : InteractableOptionManager {

        // game manager references
        private AuctionService auctionService = null;
        private PlayerManagerServer playerManagerServer = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            auctionService = systemGameManager.AuctionService;
            playerManagerServer = systemGameManager.PlayerManagerServer;
        }

        public void RequestListAuctionItems(UnitController sourceUnitController, Interactable interactable, int componentIndex, ListAuctionItemRequest listAuctionItemRequest) {
            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.GetCurrentInteractables(sourceUnitController);
            if (currentInteractables[componentIndex] is AuctionComponent) {
                (currentInteractables[componentIndex] as AuctionComponent).ListItems(sourceUnitController, listAuctionItemRequest);
            }
        }

        public void RequestBuyAuctionItem(int accountId, int auctionItemId) {
            int playerCharacterId = playerManagerServer.GetPlayerCharacterId(accountId);
            if (playerCharacterId == -1) {
                return;
            }
            if (auctionService.BuyAuctionItem(playerCharacterId, auctionItemId)) {
                networkManagerServer.AdvertiseBuyAuctionItem(accountId, auctionItemId);
            }
        }

        public void RequestCancelAuction(int accountId, int auctionItemId) {
            int playerCharacterId = playerManagerServer.GetPlayerCharacterId(accountId);
            if (playerCharacterId == -1) {
                return;
            }
            if (auctionService.CancelAuction(playerCharacterId, auctionItemId)) {
                networkManagerServer.AdvertiseCancelAuction(accountId, auctionItemId);
            }
        }

        public void RequestSearchAuctions(UnitController sourceUnitController, Interactable interactable, int componentIndex, string searchText, bool onlyShowOwnAuctions) {
            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.GetCurrentInteractables(sourceUnitController);
            if (currentInteractables[componentIndex] is AuctionComponent) {
                (currentInteractables[componentIndex] as AuctionComponent).SearchAuctions(sourceUnitController, searchText, onlyShowOwnAuctions);
            }
        }
    }

}