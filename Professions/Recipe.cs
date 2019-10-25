using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
[CreateAssetMenu(fileName = "New Recipe", menuName = "Recipes/Recipe")]
public class Recipe : DescribableResource {

    [SerializeField]
    private CraftingMaterial[] craftingMaterials;

    [SerializeField]
    private Item output;

    [SerializeField]
    private int outputCount;

    [SerializeField]
    private CraftAbility craftAbility;

    public Item MyOutput { get => output; set => output = value; }
    public CraftingMaterial[] MyCraftingMaterials { get => craftingMaterials; set => craftingMaterials = value; }
    public int MyOutputCount { get => outputCount; set => outputCount = value; }
    public CraftAbility MyCraftAbility { get => craftAbility; set => craftAbility = value; }
}

}