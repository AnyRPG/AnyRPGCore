using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestScript : BagPanel
{
    // Start is called before the first frame update
    public override void Awake() {
        base.Awake();
        AddSlots(48);
    }

}
