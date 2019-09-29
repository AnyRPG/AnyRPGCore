using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CurrencyButton : TransparencyButton {

    [SerializeField]
    private Currency currency;

    [SerializeField]
    private Image icon;

    [SerializeField]
    private Text currencyName;

    [SerializeField]
    private Text description;

    public void AddCurrency(string currency) {
        Currency addCurrency = SystemCurrencyManager.MyInstance.GetResource(currency);
        AddCurrency(addCurrency);
    }

    public void AddCurrency(Currency currency) {
        this.currency = currency as Currency;
        icon.sprite = this.currency.MyIcon;
        icon.color = Color.white;
        currencyName.text = this.currency.MyName;
        description.text = this.currency.GetSummary();
    }

    public void ClearCurrency() {
        icon.sprite = null;
        icon.color = new Color32(0, 0, 0, 0);
        currencyName.text = string.Empty;
        description.text = string.Empty;
    }

    /*
    public void OnPointerClick(PointerEventData eventData) {
        Debug.Log("FactionButton.OnPointerClick()");
        if (eventData.button == PointerEventData.InputButton.Left) {
            Debug.Log("FactionButton.OnPointerClick(): left click");
            HandScript.MyInstance.TakeMoveable(faction);
        }
        if (eventData.button == PointerEventData.InputButton.Right) {
            Debug.Log("AbilityButton.OnPointerClick(): right click");
            PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.BeginAbility(faction);
        }
    }
    */
}
