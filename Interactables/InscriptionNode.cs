using System.Collections;
using UnityEngine;

public class InscriptionNode : CraftingNode
{
    protected override void Start() {
        base.Start();
        interactionPanelTitle = "Inscription";
    }

}
