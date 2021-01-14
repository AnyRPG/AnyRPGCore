using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class FactionDisposition {

        //public Faction faction;
        [SerializeField]
        private string factionName = string.Empty;

        private Faction faction = null;

        public float disposition = 0f;

        public Faction Faction { get => faction; set => faction = value; }

        public void SetupScriptableObjects() {
            faction = null;
            if (factionName != null && factionName != string.Empty) {
                Faction tmpFaction = SystemFactionManager.MyInstance.GetResource(factionName);
                if (tmpFaction != null) {
                    faction = tmpFaction;
                } else {
                    Debug.LogError("Quest.SetupScriptableObjects(): Could not find factionName : " + factionName + " while inititalizing a faction disposition.  CHECK INSPECTOR");
                }
            }
        }

    }

}