using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class BagNode {

        public event System.Action<Bag> OnAddBagHandler = delegate { };
        public event System.Action OnRemoveBagHandler = delegate { };

        [SerializeField]
        private BagButton bagButton;

        [SerializeField]
        private bool isBankNode = false;

        //private CloseableWindow bagWindow;

        private Bag bag;

        private BagPanel bagPanel;

        public Bag Bag {
            get {
                return bag;
            }
            set {
                bag = value;
                if (value != null) {
                    OnAddBagHandler(bag);
                } else {
                    //Debug.Log("BagNode.MyBag = null");
                    OnRemoveBagHandler();
                    if (BagPanel != null) {
                        BagPanel.ClearSlots();
                    }
                }
            }
        }

        public BagPanel BagPanel { get => bagPanel; set => bagPanel = value; }
        public bool IsBankNode { get => isBankNode; set => isBankNode = value; }
        public BagButton BagButton { get => bagButton; set => bagButton = value; }
        //public CloseableWindow BagWindow { get => bagWindow; set => bagWindow = value; }
    }

}