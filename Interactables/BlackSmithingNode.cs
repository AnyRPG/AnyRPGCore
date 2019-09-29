using System.Collections;
using UnityEngine;

public class BlackSmithingNode : CraftingNode {

    protected override void Start() {
        base.Start();
        interactionPanelTitle = "BlackSmithing";
    }

}
