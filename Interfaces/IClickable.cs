using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
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

    Text MyStackSizeText {
        get;
    }
}

}