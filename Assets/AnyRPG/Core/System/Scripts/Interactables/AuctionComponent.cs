using System;
using UnityEngine;

namespace AnyRPG {
    public class AuctionComponent : InteractableOptionComponent {

        // game manager references
        AuctionManagerClient auctionManagerClient = null;
        AuctionService auctionService = null;

        public AuctionProps Props { get => interactableOptionProps as AuctionProps; }

        public AuctionComponent(Interactable interactable, AuctionProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            auctionManagerClient = systemGameManager.AuctionManagerClient;
            auctionService = systemGameManager.AuctionService;
        }

        public override bool ProcessInteract(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            //Debug.Log($"{interactable.gameObject.name}.AuctionInteractable.ProcessInteract({sourceUnitController?.gameObject.name}, {componentIndex}, {choiceIndex})");
            
            base.ProcessInteract(sourceUnitController, componentIndex, choiceIndex);
            return true;
        }

        public override void ClientInteraction(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            base.ClientInteraction(sourceUnitController, componentIndex, choiceIndex);
            auctionManagerClient.SetProps(this, componentIndex, choiceIndex);
            uIManager.auctionWindow.OpenWindow();
            //auctionManagerClient.RequestSearchAuctions(string.Empty, false);
        }

        public override void StopInteract() {
            base.StopInteract();
            uIManager.auctionWindow.CloseWindow();
        }

        public override int GetCurrentOptionCount(UnitController sourceUnitController) {
            //Debug.Log(interactable.gameObject.name + ".AuctionInteractable.GetCurrentOptionCount(): returning " + GetValidOptionCount());
            return GetValidOptionCount(sourceUnitController);
        }

        public void ListItems(UnitController sourceUnitController, ListAuctionItemRequest listAuctionItemRequest) {
            //Debug.Log($"AuctionComponent.SendMail({sourceUnitController.gameObject.name})");

            auctionService.ListNewItems(sourceUnitController, listAuctionItemRequest);

            NotifyOnConfirmAction(sourceUnitController);
        }

        public void SearchAuctions(UnitController sourceUnitController, string searchText, bool onlyShowOwnAuctions) {
            auctionService.SearchAuctionItems(sourceUnitController, searchText, onlyShowOwnAuctions);
        }

        //public override bool PlayInteractionSound() {
        //    return true;
        //}

    }

}