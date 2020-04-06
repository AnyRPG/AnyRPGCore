using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public interface IPrerequisiteOwner {

        void HandlePrerequisiteUpdates();
    }

}