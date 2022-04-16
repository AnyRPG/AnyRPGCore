using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class FactionNode : ConfiguredClass, IRewardable, IDescribable {

        [SerializeField]
        [ResourceSelector(resourceType = typeof(Faction))]
        private string factionName = string.Empty;

        private Faction faction;

        public int reputationAmount;

        public Sprite Icon { get => faction.Icon; }
        public string DisplayName { get => faction.DisplayName; }
        public string Description { get => faction.Description; }
        public Faction Faction { get => faction; }

        // game manager references
        protected PlayerManager playerManager = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            playerManager = systemGameManager.PlayerManager;
        }

        public string GetSummary() {
            return faction.GetSummary(); ;
        }

        public string GetDescription() {
            return faction.GetDescription();
        }

        public void GiveReward() {
            playerManager.MyCharacter.CharacterFactionManager.AddReputation(Faction, reputationAmount);
        }

        public bool HasReward() {
            return false;
        }

        public void SetupScriptableObjects(SystemGameManager systemGamenManager) {
            Configure(systemGamenManager);

            if (factionName != null && factionName != string.Empty) {
                faction = null;
                Faction tmpFaction = systemDataFactory.GetResource<Faction>(factionName);
                if (tmpFaction != null) {
                    faction = tmpFaction;
                } else {
                    Debug.LogError("FactionNode.SetupScriptableObjects(): Could not find faction : " + factionName + " while inititalizing quest " + DisplayName + ".  CHECK INSPECTOR");
                }
            }
        }
    }

}