using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class GuildmasterManagerClient : InteractableOptionManager {

        private GuildmasterProps guildmasterProps = null;
        private GuildmasterComponent guildmasterComponent = null;
        private string savedGuildName = string.Empty;

        public GuildmasterProps GuildmasterProps { get => guildmasterProps; set => guildmasterProps = value; }
        public GuildmasterComponent GuildmasterComponent { get => guildmasterComponent; set => guildmasterComponent = value; }
        public string SavedGuildName { get => savedGuildName; set => savedGuildName = value; }

        public void SetProps(GuildmasterProps guildmasterProps, GuildmasterComponent guildmasterComponent, int componentIndex, int choiceIndex) {
            //Debug.Log("VendorManager.SetProps()");
            this.guildmasterProps = guildmasterProps;
            this.guildmasterComponent = guildmasterComponent;
            BeginInteraction(guildmasterComponent, componentIndex, choiceIndex);
        }

        public void RequestCreateGuild(UnitController sourceUnitController) {
            //Debug.Log("GuildmasterManagerClient.RequestCreateGuild()");

            if (systemGameManager.GameMode == GameMode.Local) {
                guildmasterComponent.CreateGuild(sourceUnitController, savedGuildName);
            } else {
                networkManagerClient.RequestCreateGuild(guildmasterComponent.Interactable, componentIndex, savedGuildName);
            }
        }

        public void RequestCreateGuild(string guildName) {

            this.savedGuildName = guildName;
            networkManagerClient.CheckGuildName(guildmasterComponent.Interactable, componentIndex, savedGuildName);
        }

        public override void EndInteraction() {
            base.EndInteraction();

            guildmasterProps = null;
            guildmasterComponent = null;
        }


    }

}