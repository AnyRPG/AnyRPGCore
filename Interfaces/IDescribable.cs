using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDescribable
{
    Sprite MyIcon { get; }
    string MyName { get; }
    string GetDescription();
    string GetSummary();
}
