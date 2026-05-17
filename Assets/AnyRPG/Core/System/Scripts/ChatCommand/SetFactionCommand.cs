using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Set Faction Command", menuName = "AnyRPG/Chat Commands/Set Faction Command")]
    public class SetFactionCommand : ChatCommand {

        [Header("Set Faction Command")]

        [Tooltip("If true, all parameters will be ignored, and the faction will be the one listed below")]
        [SerializeField]
        private bool fixedFaction = false;

        [Tooltip("The name of the faction to join")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(Faction))]
        private string factionName = string.Empty;

        private Faction faction = null;

        public override void ExecuteCommand(string commandParameters, int accountId) {
            //Debug.Log($"{ResourceName}.SetFactionCommand.ExecuteCommand({commandParameters}, {accountId})");

            // set the fixed faction
            if (fixedFaction == true && faction != null) {
                SetFaction(faction, accountId);
                return;
            }

            // the faction comes from parameters, but none were given
            if (commandParameters == string.Empty) {
                return;
            }

            faction = systemDataFactory.GetResource<Faction>(commandParameters);
            if (faction == null) {
                return;
            }
            SetFaction(faction, accountId);
        }

        private void SetFaction(Faction faction, int accountId) {
            //Debug.Log($"SetFactionCommand.SetFaction({faction.ResourceName}, {accountId})");

            playerManagerServer.SetPlayerFaction(faction, accountId);
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            if (factionName != string.Empty) {
                faction = systemDataFactory.GetResource<Faction>(factionName);
                if (faction == null) {
                    Debug.LogError($"SetFactionCommand.SetupScriptableObjects(): Could not find faction {factionName} for command {ResourceName}");
                }
            }
        }
    }

}