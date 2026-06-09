using System.Collections.Generic;

namespace AnyRPG {
    public class GuildmasterManagerServer : InteractableOptionManager {

        public void CreateGuild(UnitController sourceUnitController, InteractableBase interactable, int componentIndex, string guildName) {
            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.GetCurrentInteractables(sourceUnitController);
            if (currentInteractables[componentIndex] is GuildmasterComponent) {
                (currentInteractables[componentIndex] as GuildmasterComponent).CreateGuild(sourceUnitController, guildName);
            }
        }

        public void CheckGuildName(UnitController sourceUnitController, InteractableBase interactable, int componentIndex, string guildName) {
            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.GetCurrentInteractables(sourceUnitController);
            if (currentInteractables[componentIndex] is GuildmasterComponent) {
                (currentInteractables[componentIndex] as GuildmasterComponent).CheckGuildName(sourceUnitController, guildName);
            }
        }

    }

}