using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// this is almost identical to questscript

public class RecipeScript : HighlightButton {

    private string recipeName;

    public string MyRecipeName { get => recipeName; set => recipeName = value; }

    public void SetRecipeName(string recipeName) {
        if (recipeName != null && recipeName != string.Empty) {
            this.recipeName = recipeName;
        }
    }

    public override void Select() {
        Debug.Log(gameObject.name + ".RecipeScript.Select(): " + recipeName);

        base.Select();
        CraftingUI.MyInstance.MySelectedRecipeScript = this;

        //GetComponent<Text>().color = Color.red;
        CraftingUI.MyInstance.ShowDescription(MyRecipeName);

    }

    public override void DeSelect() {
        //Debug.Log("RecipeScript.DeSelect()");
        base.DeSelect();
    }

}
