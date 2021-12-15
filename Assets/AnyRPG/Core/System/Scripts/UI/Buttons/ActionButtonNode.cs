using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class ActionButtonNode : ConfiguredClass {

        // A reference to the useable on the actionbutton
        private IUseable useable = null;

        // keep track of the last usable that was on this button in case an ability is re-learned
        private IUseable savedUseable = null;

        public IUseable Useable { get => useable; set => useable = value; }
        public IUseable SavedUseable { get => savedUseable; set => savedUseable = value; }
    }
}

