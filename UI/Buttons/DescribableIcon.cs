using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
public class DescribableIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// A reference to the useable on the actionbutton
    /// </summary>
    public IDescribable MyDescribable { get; set; }

    [SerializeField]
    protected Text stackSize;

    protected int count;

    public Image MyIcon {
        get {
            return icon;
        }

        set {
            icon = value;
        }
    }

    public int MyCount {
        get {
            return count;
        }
    }

    public Text MyStackSizeText {
        get {
            return stackSize;
        }
    }

    [SerializeField]
    private Image icon;

    private void Awake() {
        //Debug.Log("ActionButton.Awake()");
        MyDescribable = null;
    }

    void Start() {
        //Debug.Log("ActionButton.Start()");
    }


    /// <summary>
    /// Sets the describable on the describablebutton
    /// </summary>
    /// <param name="describable"></param>
    public void SetDescribable(IDescribable describable) {
        //Debug.Log("DescribableIcon.SetDescribable(" + (describable == null ? "null" : describable.MyName) + ")");
        SetDescribableCommon(describable);
    }

    public void SetDescribable(IDescribable describable, int count) {
        //Debug.Log("DescribableIcon.SetDescribable(" + describable.MyName + ")");
        this.count = count;
        SetDescribableCommon(describable);
    }

    protected virtual void SetDescribableCommon(IDescribable describable) {
        //Debug.Log("DescribableIcon.SetDescribableCommon(" + describable.MyName + ")");
        this.MyDescribable = describable;
        UpdateVisual();
        UIManager.MyInstance.RefreshTooltip(describable as IDescribable);
    }

    public virtual void UpdateVisual(Item item) {
        //Debug.Log("DescribableIcon.UpdateVisual()");
        /*
        if ((item as IDescribable) == MyDescribable) {
            count = InventoryManager.MyInstance.GetItemCount(item.MyName);
        }
        */
        UpdateVisual();
    }

    /// <summary>
    /// UPdates the visual representation of the describablebutton
    /// </summary>
    public virtual void UpdateVisual() {
        //Debug.Log("DescribableIcon.UpdateVisual()");
        if (MyDescribable == null) {
            //Debug.Log("DescribableIcon.UpdateVisual(): MyDescribable is null!");
        }
        if (MyIcon == null) {
            //Debug.Log("DescribableIcon.UpdateVisual(): MyIcon is null!");
        }
        if (MyDescribable != null && MyIcon != null) {
            if (MyIcon.sprite != MyDescribable.MyIcon) {
                MyIcon.sprite = null;
                MyIcon.sprite = MyDescribable.MyIcon;
                MyIcon.color = Color.white;
            }
        } else if (MyDescribable == null && MyIcon != null) {
            MyIcon.sprite = null;
            MyIcon.color = new Color32(0, 0, 0, 0);
        }

        /*
        if (count > 1) {
            UIManager.MyInstance.UpdateStackSize(this, count);
        } else if (MyDescribable is BaseAbility) {
            UIManager.MyInstance.ClearStackCount(this);
        }
        */
    }

    public void OnPointerEnter(PointerEventData eventData) {
        IDescribable tmp = null;

        if (MyDescribable != null && MyDescribable is IDescribable) {
            tmp = (IDescribable)MyDescribable;
            //UIManager.MyInstance.ShowToolTip(transform.position);
        }// else if (MyUseables.Count > 0) {
            //UIManager.MyInstance.ShowToolTip(transform.position);
        //}
        if (tmp != null) {
            UIManager.MyInstance.ShowToolTip(transform.position, tmp);
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        UIManager.MyInstance.HideToolTip();
    }

    public virtual void OnDisable() {
    }
}

}