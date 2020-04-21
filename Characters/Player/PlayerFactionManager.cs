using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class PlayerFactionManager : CharacterFactionManager {

        public override void AddReputation(Faction faction, int reputationAmount, bool notify = true) {
            base.AddReputation(faction, reputationAmount);
            if (notify) {
                SystemEventManager.TriggerEvent("OnReputationChange", new EventParamProperties());
            }
        }

        public override void SetReputation(Faction newFaction) {
            base.SetReputation(newFaction);
            SystemEventManager.TriggerEvent("OnReputationChange", new EventParamProperties());
        }

    }

}