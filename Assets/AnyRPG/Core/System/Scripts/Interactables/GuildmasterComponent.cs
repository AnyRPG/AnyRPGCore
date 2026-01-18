using System.Collections;
using UnityEngine;

namespace AnyRPG {
    public class GuildmasterComponent : InteractableOptionComponent {

        // game manager references
        private GuildmasterManagerClient guildmasterManager = null;
        private GuildServiceServer guildServiceServer = null;

        public GuildmasterProps Props { get => interactableOptionProps as GuildmasterProps; }

        public GuildmasterComponent(Interactable interactable, GuildmasterProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            if (interactionPanelTitle == string.Empty) {
                interactionPanelTitle = "Guildmaster";
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            guildmasterManager = systemGameManager.GuildmasterManagerClient;
            guildServiceServer = systemGameManager.GuildServiceServer;
        }

        public override void ProcessCreateEventSubscriptions() {
            //Debug.Log("GatheringNode.CreateEventSubscriptions()");
            base.ProcessCreateEventSubscriptions();

            systemEventManager.OnSetGuildId += HandleSetGuildId;
        }

        public override void ProcessCleanupEventSubscriptions() {
            //Debug.Log($"{gameObject.name}.GuildmasterInteractable.CleanupEventSubscriptions()");
            base.ProcessCleanupEventSubscriptions();

            systemEventManager.OnSetGuildId -= HandleSetGuildId;
        }

        public void HandleSetGuildId(int guildId) {
            HandleOptionStateChange();
        }

        public override bool ProcessInteract(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            //Debug.Log(interactable.gameObject.name + ".GuildmasterInteractable.Interact()");

            base.ProcessInteract(sourceUnitController, componentIndex, choiceIndex);

            return true;
        }

        public override void ClientInteraction(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            base.ClientInteraction(sourceUnitController, componentIndex, choiceIndex);

            guildmasterManager.SetProps(Props, this, componentIndex, choiceIndex);
            uIManager.createGuildWindow.OpenWindow();
        }

        public override void StopInteract() {
            base.StopInteract();
            uIManager.createGuildWindow.CloseWindow();
        }

        public override int GetCurrentOptionCount(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.CharacterCreatorInteractable.GetCurrentOptionCount()");
            return GetValidOptionCount(sourceUnitController);
        }

        // faction is a special type of prerequisite
        public override bool PrerequisitesMet(UnitController sourceUnitController) {
                if (sourceUnitController.CharacterGuildManager.IsInGuild() == true) {
                    return false;
                }
                return base.PrerequisitesMet(sourceUnitController);
        }

        public void CreateGuild(UnitController sourceUnitController, string guildName) {
            //Debug.Log($"GuildmasterComponent.CreateGuild({sourceUnitController.gameObject.name}, {guildName})");

            guildServiceServer.CreateGuild(sourceUnitController.CharacterId, guildName);
            NotifyOnConfirmAction(sourceUnitController);
        }

        public void CheckGuildName(UnitController sourceUnitController, string guildName) {
            //Debug.Log($"GuildmasterComponent.CheckGuildName({sourceUnitController.gameObject.name}, {guildName})");

            guildServiceServer.CheckGuildName(sourceUnitController, guildName);
        }

        //public override bool PlayInteractionSound() {
        //    return true;
        //}


    }

}