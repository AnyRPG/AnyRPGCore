using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class PlayerFactionManager : CharacterFactionManager {

        public override void AddReputation(Faction faction, int reputationAmount) {
            base.AddReputation(faction, reputationAmount);
            SystemEventManager.MyInstance.NotifyOnReputationChange();
        }

        public override void SetReputation(Faction newFaction) {
            base.SetReputation(newFaction);
            SystemEventManager.MyInstance.NotifyOnReputationChange();
        }

    }

}