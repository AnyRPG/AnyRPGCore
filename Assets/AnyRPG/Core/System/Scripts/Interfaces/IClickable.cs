using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
public interface IClickable {
    Image Icon {
        get;
        set;
    }

    int Count {
        get;
    }

    TextMeshProUGUI StackSizeText {
        get;
    }
}

}