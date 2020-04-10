using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
public class DescribableCraftingInputIcon : DescribableIcon
{
    [SerializeField]
    private TextMeshProUGUI description = null;

    [SerializeField]
    private GameObject materialSlot = null;

    public GameObject MyMaterialSlot { get => materialSlot; }

    public override void UpdateVisual() {
        //Debug.Log("DescribableCraftingInputIcon.UpdateVisual()");
        base.UpdateVisual();
        description.text = MyDescribable.MyName;
        
        //if (count > 1) {
            stackSize.text = InventoryManager.MyInstance.GetItemCount(MyDescribable.MyName) + " / " + count.ToString();
        //} else {
            //stackSize.text = "";
        //}
        CraftingUI.MyInstance.UpdateCraftAmountArea();
    }

    protected override void SetDescribableCommon(IDescribable describable) {
        base.SetDescribableCommon(describable);
        SystemEventManager.MyInstance.OnItemCountChanged += UpdateVisual;
    }


    public override void OnDisable() {
        base.OnDisable();
        if (InventoryManager.MyInstance != null) {
            SystemEventManager.MyInstance.OnItemCountChanged -= UpdateVisual;
        }

    }

}

}