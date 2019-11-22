using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
[System.Serializable]
public class VendorItem
{
    [SerializeField]
    private Item item;

    [SerializeField]
    private int quantity;

    [SerializeField]
    private bool unlimited;

    public Item MyItem
    {
        get
        {
            return item;
        }
            set {
                item = value;
            }
    }

    public int MyQuantity
    {
        get
        {
            return quantity;
        }

        set
        {
            quantity = value;
        }
    }

    public bool Unlimited
    {
        get
        {
            return unlimited;
        }
    }
}

}