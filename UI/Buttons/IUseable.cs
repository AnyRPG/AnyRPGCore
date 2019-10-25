using AnyRPG;
﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;

namespace AnyRPG {
public interface IUseable
{
    Sprite MyIcon { get; }
    string MyName { get; }
    void Use();
}
}