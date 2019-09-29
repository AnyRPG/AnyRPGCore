using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;

public interface IUseable
{
    Sprite MyIcon { get; }
    string MyName { get; }
    void Use();
}