using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class PlayerFactionManager : CharacterFactionManager {

        public override void AddReputation(string factionName, int reputationAmount) {
            base.AddReputation(factionName, reputationAmount);
            SystemEventManager.MyInstance.NotifyOnReputationChange();
        }

        public override void SetReputation(string newFactionName) {
            base.SetReputation(newFactionName);
            SystemEventManager.MyInstance.NotifyOnReputationChange();
        }

    }

}