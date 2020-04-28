using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
public interface IClickable {
    Image MyIcon {
        get;
        set;
    }

    int MyCount {
        get;
    }

    TextMeshProUGUI StackSizeText {
        get;
    }
}

}