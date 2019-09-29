using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SystemPanelButton : MonoBehaviour, IDescribable, IPointerEnterHandler, IPointerExitHandler {

    [SerializeField]
    private Sprite icon;

    [SerializeField]
    private string optionName;

    [SerializeField]
    private string description;

    public Sprite MyIcon { get => icon;  }
    public string MyName { get => optionName; }

    private void Awake() {
    }

    public string GetDescription() {
        return string.Format("<color=cyan>{0}</color>\n{1}", optionName, GetSummary());
    }

    public string GetSummary() {
        return description;
    }

    public void OnPointerEnter(PointerEventData eventData) {

        UIManager.MyInstance.ShowToolTip(transform.position, this);
    }

    public void OnPointerExit(PointerEventData eventData) {
        UIManager.MyInstance.HideToolTip();
    }

}
