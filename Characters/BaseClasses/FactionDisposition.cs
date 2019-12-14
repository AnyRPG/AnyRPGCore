using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class FactionDisposition {


        //public Faction faction;
        [SerializeField]
        private string factionName;

        private Faction faction;

        public float disposition;

        public Faction MyFaction { get => faction; set => faction = value; }
    }

}