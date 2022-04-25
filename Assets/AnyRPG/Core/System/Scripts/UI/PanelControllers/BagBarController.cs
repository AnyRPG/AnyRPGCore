using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class BagBarController : ConfiguredMonoBehaviour {

        [SerializeField]
        private GameObject BagButtonPrefab = null;

        [SerializeField]
        private List<BagButton> bagButtons = new List<BagButton>();

        [SerializeField]
        protected Image backGroundImage = null;

        protected int bagButtonCount = 0;

        protected bool eventSubscriptionsInitialized = false;

        // game manager references
        private ObjectPooler objectPooler = null;

        public int FreeBagSlots {
            get {
                int freeBagSlots = 0;
                for (int i = 0; i < bagButtonCount; i++) {
                    if (bagButtons[i].BagNode.Bag == null) {
                        freeBagSlots++;
                    }
                }
                return freeBagSlots;
            }
        }
        public List<BagButton> MyBagButtons { get => bagButtons; set => bagButtons = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            SetBackGroundColor();
            CreateEventSubscriptions();

            foreach (BagButton bagButton in bagButtons) {
                bagButton.Configure(systemGameManager);
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            objectPooler = systemGameManager.ObjectPooler;
        }

        public void SetBagPanel(BagPanel bagPanel) {
            foreach (BagButton bagButton in bagButtons) {
                bagButton.SetBagpanel(bagPanel);
            }
        }


        private void CreateEventSubscriptions() {
            //Debug.Log("BagBarController.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StartListening("OnInventoryTransparencyUpdate", HandleInventoryTransparencyUpdate);
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StopListening("OnInventoryTransparencyUpdate", HandleInventoryTransparencyUpdate);
            eventSubscriptionsInitialized = false;
        }

        public void SetBagButtonCount(int count) {
            bagButtonCount = count;
            for (int i = 0; i < bagButtons.Count; i++) {
                if (i >= bagButtonCount) {
                    bagButtons[i].gameObject.SetActive(false);
                }
            }
        }

        public void HandleInventoryTransparencyUpdate(string eventName, EventParamProperties eventParamProperties) {
            SetBackGroundColor();
        }

        public BagButton AddBagButton(BagNode bagNode) {
            //Debug.Log(gameObject.name + "BagBarController.AddBagButton()");
            for (int i = 0; i < bagButtonCount; i++) {
                if (bagButtons[i].BagNode == null) {
                    //Debug.Log("BagBarController.AddBagButton(): found an empty bag button");
                    bagButtons[i].BagNode = bagNode;
                    return bagButtons[i];
                }
            }
            //Debug.Log("BagBarController.AddBagButton(): Could not find an unused bag button!");
            return null;
        }

        public BagButton InstantiateBagButton() {
            //Debug.Log("BagBarController.InstantiateBagButton()");
            BagButton bagButton = objectPooler.GetPooledObject(BagButtonPrefab, this.gameObject.transform).GetComponent<BagButton>();
            bagButton.Configure(systemGameManager);
            bagButtons.Add(bagButton);
            return bagButton;
        }

        public void ClearBagButtons() {
            //Debug.Log(gameObject.name + "BagBarController.ClearBagButtons()");
            foreach (BagButton _bagButton in bagButtons) {
                if (_bagButton.BagNode != null) {
                    if (_bagButton.BagNode.Bag != null) {
                        Destroy(_bagButton.BagNode.Bag);
                        _bagButton.BagNode.RemoveBag();
                    }
                }
                // fix me
                // should the bagnode be set to null here ?
                // then should the bagbutton be sent back to the object pooler?

                // setting to null to deal with above issue
                _bagButton.BagNode = null;
            }
        }

        public void SetBackGroundColor() {
            //Debug.Log("BagBarController.SetBackGroundColor()");
            int opacityLevel = (int)(PlayerPrefs.GetFloat("InventoryOpacity") * 255);
            if (backGroundImage != null) {
                backGroundImage.color = new Color32(0, 0, 0, (byte)opacityLevel);
                RebuildLayout();
            }
        }

        public void RebuildLayout() {
            //Debug.Log("ActionBarController.RebuildLayout()");
            //LayoutRebuilder.ForceRebuildLayoutImmediate(gameObject.transform.parent.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(gameObject.transform.GetComponent<RectTransform>());
        }

        public void OnEnable() {
            // do this here since it probably won't get done if the window is closed - thanks Unity!!! :(
            SetBackGroundColor();
        }

        public void OnDestroy() {
            CleanupEventSubscriptions();
        }

    }

}