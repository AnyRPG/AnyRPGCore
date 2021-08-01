using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class FactionNode : IDescribable {

        public Faction faction;

        [SerializeField]
        private string factionName = string.Empty;

        public int reputationAmount;

        public Sprite Icon { get => faction.Icon; }

        public string DisplayName { get => faction.DisplayName; }

        public string GetDescription() {
            return faction.GetDescription(); ;
        }

        public string GetSummary() {
            return faction.GetSummary();
        }

        public void SetupScriptableObjects() {
            if (factionName != null && factionName != string.Empty) {
                faction = null;
                Faction tmpFaction = SystemDataFactory.Instance.GetResource<Faction>(factionName);
                if (tmpFaction != null) {
                    faction = tmpFaction;
                } else {
                    Debug.LogError("FactionNode.SetupScriptableObjects(): Could not find faction : " + factionName + " while inititalizing quest " + DisplayName + ".  CHECK INSPECTOR");
                }
            }
        }
    }

}