using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class ChestScript : BagPanel {
        // Start is called before the first frame update
        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            AddSlots(48);
        }

    }

}