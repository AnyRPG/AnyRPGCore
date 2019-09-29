using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BagNode {

    public event System.Action<Bag> OnAddBagHandler = delegate { };
    public event System.Action OnRemoveBagHandler = delegate { };

    [SerializeField]
    private BagButton bagButton;

    [SerializeField]
    private bool isBankNode = false;

    private CloseableWindow bagWindow;

    private Bag bag;

    private BagPanel bagPanel;

    public Bag MyBag {
        get {
            return bag;
        }
        set {
            bag = value;
            if (value != null) {
                OnAddBagHandler(bag);
            } else {
                Debug.Log("BagNode.MyBag = null");
                OnRemoveBagHandler();
                if (MyBagPanel != null) {
                    MyBagPanel.ClearSlots();
                }
            }
        }
    }

    public BagPanel MyBagPanel { get => bagPanel; set => bagPanel = value; }
    public bool MyIsBankNode { get => isBankNode; set => isBankNode = value; }
    public BagButton MyBagButton { get => bagButton; set => bagButton = value; }
    public CloseableWindow MyBagWindow { get => bagWindow; set => bagWindow = value; }
}
