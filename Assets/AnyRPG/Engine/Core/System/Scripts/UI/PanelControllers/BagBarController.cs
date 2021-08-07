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

        private bool localComponentsGotten = false;

        protected bool eventSubscriptionsInitialized = false;

        // game manager references
        private ObjectPooler objectPooler = null;


        public List<BagButton> MyBagButtons { get => bagButtons; set => bagButtons = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            objectPooler = systemGameManager.ObjectPooler;

            GetComponentReferences();
            SetBackGroundColor();
            CreateEventSubscriptions();
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

        public void HandleInventoryTransparencyUpdate(string eventName, EventParamProperties eventParamProperties) {
            SetBackGroundColor();
        }

        public void GetComponentReferences() {
            if (localComponentsGotten == true) {
                return;
            }
            if (backGroundImage == null) {
                //Debug.Log(gameObject.name + "SlotScript.Awake(): background image is null, trying to get component");
                backGroundImage = GetComponent<Image>();
            }
            localComponentsGotten = true;
        }

        public BagButton AddBagButton() {
            //Debug.Log(gameObject.name + "BagBarController.AddBagButton()");
            foreach (BagButton _bagButton in bagButtons) {
                if (_bagButton.MyBagNode == null) {
                    //Debug.Log("BagBarController.AddBagButton(): found an empty bag button");
                    return _bagButton;
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
            foreach (BagButton _bagButton in bagButtons) {
                if (_bagButton.MyBagNode != null) {
                    if (_bagButton.MyBagNode.MyBag != null) {
                        Destroy(_bagButton.MyBagNode.MyBag);
                        _bagButton.MyBagNode.MyBag = null;
                    }
                }
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