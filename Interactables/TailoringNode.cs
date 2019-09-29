using System.Collections;
using UnityEngine;

public class TailoringNode : CraftingNode
{
    protected override void Start() {
        base.Start();
        interactionPanelTitle = "Tailoring";
    }

}
