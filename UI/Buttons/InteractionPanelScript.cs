using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
// this is almost identical to questscript

public class InteractionPanelScript : MonoBehaviour {


    [SerializeField]
    private TextMeshProUGUI text = null;

    [SerializeField]
    private Image icon = null;

    public TextMeshProUGUI MyText {
        get {
            return text;
        }
    }

    private IInteractable interactableOption;

    public Image MyIcon { get => icon; set => icon = value; }
    public IInteractable MyInteractableOption { get => interactableOption;
        set {
            if (value.MyIcon != null) {
                icon.sprite = value.MyIcon;
                icon.color = Color.white;
            } else {
                icon.sprite = null;
                icon.color = new Color32(0, 0, 0, 0);
            }
            interactableOption = value;
        }
    }

    public void Interact() {
        MyInteractableOption.Interact(PlayerManager.MyInstance.MyCharacter.CharacterUnit);
        MyInteractableOption.MyInteractable.CloseInteractionWindow();
    }

}

}