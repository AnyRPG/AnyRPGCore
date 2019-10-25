using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
[System.Serializable]
public class CraftingMaterial
{
    [SerializeField]
    private Item item;

    [SerializeField]
    private int count;

    public Item MyItem { get => item; }
    public int MyCount { get => count; }
}

}