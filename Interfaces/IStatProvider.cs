using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public interface IStatProvider {

        List<StatScalingNode> PrimaryStats { get; set; }
        List<PowerResource> PowerResourceList { get; set; }

    }

}