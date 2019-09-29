using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
