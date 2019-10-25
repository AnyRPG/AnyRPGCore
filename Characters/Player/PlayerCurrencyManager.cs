using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
public class PlayerCurrencyManager : MonoBehaviour {

    public event System.Action OnCurrencyChange = delegate { };

    private BaseCharacter baseCharacter;

    private Dictionary<string, CurrencySaveData> currencyList = new Dictionary<string, CurrencySaveData>();
    
    public Dictionary<string, CurrencySaveData> MyCurrencyList { get => currencyList; }

    protected void Awake() {
        //Debug.Log(gameObject.name + ".PlayerFactionManager.Awake()");
        baseCharacter = GetComponent<BaseCharacter>();
    }

    protected void Start() {
    }

    public void AddCurrency(string currency, int currencyAmount) {
        AddCurrency(SystemCurrencyManager.MyInstance.GetResource(currency), currencyAmount);
    }

    public void AddCurrency(Currency currency, int currencyAmount) {
        //Debug.Log(gameObject.name + ".PlayerCurrencyManager.AddCurrency(" + currency.MyName + ", " + currencyAmount + ")");
        //bool foundReputation = false;
        CurrencySaveData newSaveData = new CurrencySaveData();
        string keyName = SystemResourceManager.prepareStringForMatch(currency.MyName);
        if (MyCurrencyList.ContainsKey(keyName)) {
            newSaveData = new CurrencySaveData();
            newSaveData.MyName = MyCurrencyList[keyName].MyName;
            newSaveData.MyAmount = currencyAmount + MyCurrencyList[keyName].MyAmount;
            MyCurrencyList[keyName] = newSaveData;
            return;
        }

        newSaveData = new CurrencySaveData();
        newSaveData.MyName = currency.MyName;
        newSaveData.MyAmount = currencyAmount;
        MyCurrencyList[keyName] = newSaveData;
        OnCurrencyChange();
    }

    public bool SpendCurrency(Currency currency, int currencyAmount) {
        //Debug.Log(gameObject.name + ".PlayerCurrencyManager.SpendCurrency(" + currency.MyName + ", " + currencyAmount + ")");
        //bool foundReputation = false;
        CurrencySaveData newSaveData = new CurrencySaveData();
        string keyName = SystemResourceManager.prepareStringForMatch(currency.MyName);
        if (MyCurrencyList.ContainsKey(keyName)) {
                if (currencyAmount < MyCurrencyList[keyName].MyAmount) {
                    newSaveData = new CurrencySaveData();
                    newSaveData.MyName = MyCurrencyList[keyName].MyName;
                    newSaveData.MyAmount = MyCurrencyList[keyName].MyAmount - currencyAmount;
                    MyCurrencyList[keyName] = newSaveData;
                    return true;
                } else {
                    return false;
                }
        }
        return false;
    }

    public int GetCurrencyAmount(Currency currency) {
        //Debug.Log(gameObject.name + ".PlayerCurrencyManager.GetCurrency(" + currency.MyName + ")");
        //bool foundReputation = false;
        string keyName = SystemResourceManager.prepareStringForMatch(currency.MyName);
        if (MyCurrencyList.ContainsKey(keyName)) {
            return MyCurrencyList[keyName].MyAmount;
        }

        // default return
        return 0;
    }



}

}