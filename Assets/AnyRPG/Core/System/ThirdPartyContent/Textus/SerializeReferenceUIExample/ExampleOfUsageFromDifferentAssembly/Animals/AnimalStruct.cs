using System;
using UnityEngine;

[Serializable]
public struct AnimalStruct : IAnimal
{
    public string name;
    public void Feed() => Debug.Log("thanks");
}
