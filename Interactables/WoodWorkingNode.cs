using System.Collections;
using UnityEngine;

public class WoodWorkingNode : CraftingNode
{
    protected override void Start() {
        base.Start();
        interactionPanelTitle = "WoodWorking";
    }

}
