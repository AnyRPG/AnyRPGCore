using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class FactionNode : ConfiguredClass, IDescribable {

        public Faction faction;

        [SerializeField]
        private string factionName = string.Empty;

        // game manager references
        private SystemDataFactory systemDataFactory = null;

        public int reputationAmount;

        public Sprite Icon { get => faction.Icon; }

        public string DisplayName { get => faction.DisplayName; }

        public string GetDescription() {
            return faction.GetDescription(); ;
        }

        public string GetSummary() {
            return faction.GetSummary();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            systemDataFactory = systemGameManager.SystemDataFactory;
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