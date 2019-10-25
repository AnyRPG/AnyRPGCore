using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
[System.Serializable]
public class Loot
{
    [SerializeField]
    private Item item;

    [SerializeField]
    private float dropChance;

    [SerializeField]
    private int minDrops = 1;

    [SerializeField]
    private int maxDrops = 1;

    [SerializeField]
    protected List<PrerequisiteConditions> prerequisiteConditions = new List<PrerequisiteConditions>();

    public Item MyItem { get => item; }
    public float MyDropChance { get => dropChance; }
    public int MyMinDrops { get => minDrops; set => minDrops = value; }
    public int MyMaxDrops { get => maxDrops; set => maxDrops = value; }

    public bool MyPrerequisitesMet {
        get {
            foreach (PrerequisiteConditions prerequisiteCondition in prerequisiteConditions) {
                if (!prerequisiteCondition.IsMet()) {
                    return false;
                }
            }
            // there are no prerequisites, or all prerequisites are complete
            return true;
        }
    }

}

}