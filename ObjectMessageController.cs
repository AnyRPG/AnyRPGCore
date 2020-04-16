using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AnyRPG {
    public class ObjectMessageController : MonoBehaviour {

        [SerializeField]
        private ObjectMessageTemplate messageTemplate = null;

        private Dictionary<string, ObjectMessageNode> messageDictionary = new Dictionary<string, ObjectMessageNode>();

        // Start is called before the first frame update
        void Start() {
            //Debug.Log(gameObject.name + ".ObjectMessageController.Start()");
            InitializeMessageResponses();
        }

        private void InitializeMessageResponses() {
            //Debug.Log(gameObject.name + ".ObjectMessageController.InitializeMessageResponses()");
            if (messageTemplate == null) {
                return;
            }

            foreach (ObjectMessageNode objectMessageNode in messageTemplate.MyEventList) {
                messageDictionary[objectMessageNode.MyEventName] = objectMessageNode;
                //Debug.Log(gameObject.name + ".ObjectMessageController.InitializeMessageResponses(): listening to: " + objectMessageNode.MyEventName);
                SystemEventManager.StartListening(objectMessageNode.MyEventName, CallbackFunction);
            }
        }

        public void CallbackFunction(string eventName, EventParam eventParam) {
            //Debug.Log(gameObject.name + ".ObjectMessageController.CallbackFunction(" + eventName + ")");
            if (messageDictionary.ContainsKey(eventName)) {
                ProcessEvent(messageDictionary[eventName], eventParam);
            }
        }

        private void ProcessEvent(ObjectMessageNode objectMessageNode, EventParam eventParam) {
            //Debug.Log(gameObject.name + ".ObjectMessageController.ProcessEvent()");
            foreach (MessageResponseNode messageResponseNode in objectMessageNode.MyResponses) {
                EventParam usedEventParam = eventParam;
                if (messageResponseNode.MyUseCustomParam == true) {
                    usedEventParam = messageResponseNode.MyCustomParameters;
                }

                if (messageResponseNode.MyParameter == EventParamType.noneType) {
                    gameObject.SendMessage(messageResponseNode.MyFunctionName, SendMessageOptions.DontRequireReceiver);
                } else if (messageResponseNode.MyParameter == EventParamType.floatType) {
                    gameObject.SendMessage(messageResponseNode.MyFunctionName, usedEventParam.FloatParam, SendMessageOptions.DontRequireReceiver);
                } else if (messageResponseNode.MyParameter == EventParamType.intType) {
                    gameObject.SendMessage(messageResponseNode.MyFunctionName, usedEventParam.IntParam, SendMessageOptions.DontRequireReceiver);
                } else if (messageResponseNode.MyParameter == EventParamType.boolType) {
                    gameObject.SendMessage(messageResponseNode.MyFunctionName, usedEventParam.BoolParam, SendMessageOptions.DontRequireReceiver);
                } else if (messageResponseNode.MyParameter == EventParamType.stringType) {
                    gameObject.SendMessage(messageResponseNode.MyFunctionName, usedEventParam.StringParam, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        public void OnDestroy() {
            CleanupMessageResponses();
        }

        private void CleanupMessageResponses() {
            foreach (ObjectMessageNode objectMessageNode in messageTemplate.MyEventList) {
                messageDictionary[objectMessageNode.MyEventName] = objectMessageNode;
                SystemEventManager.StopListening(objectMessageNode.MyEventName, CallbackFunction);
            }
        }

    }

}
