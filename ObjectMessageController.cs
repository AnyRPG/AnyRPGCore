using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace AnyRPG {
    public class ObjectMessageController : MonoBehaviour {

        [SerializeField]
        private ObjectMessageTemplate messageTemplate = null;

        private Dictionary<string, ObjectMessageNode> messageDictionary = new Dictionary<string, ObjectMessageNode>();

        // Start is called before the first frame update
        void Awake() {
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

            // message responses
            foreach (MessageResponseNode messageResponseNode in objectMessageNode.MyMessageResponses) {
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

            // property responses
            foreach (PropertyResponseNode propertyResponseNode in objectMessageNode.MyPropertyResponses) {
                EventParam usedEventParam = eventParam;
                if (propertyResponseNode.MyUseCustomParam == true) {
                    usedEventParam = propertyResponseNode.MyCustomParameters;
                }

                //Debug.Log(gameObject.name + "ObjectMessageController.ProcessEvent(): MyScriptName: " + propertyResponseNode.MyScriptName);
                Type type = Type.GetType(propertyResponseNode.MyScriptName);
                //Debug.Log(gameObject.name + "ObjectMessageController.ProcessEvent(): MyScriptName: " + propertyResponseNode.MyScriptName + "; type: " + (type == null ? "null" : type.Name));

                Component component = GetComponent(Type.GetType(propertyResponseNode.MyScriptName));
                if (component != null) {
                    foreach (FieldInfo fieldInfo in (component as MonoBehaviour).GetType().GetFields()) {
                        //Debug.Log(gameObject.name + "ObjectMessageController.ProcessEvent(): found field: "+ fieldInfo.Name);
                    }
                    //(component as MonoBehaviour).GetType().GetFields();

                    FieldInfo primaryFieldType = (component as MonoBehaviour).GetType().GetField(propertyResponseNode.MyPropertyName);
                    object primaryFieldObject = component;
                    object primaryFieldValue = primaryFieldType.GetValue(primaryFieldObject);
                    //Debug.Log(gameObject.name + "ObjectMessageController.ProcessEvent(): primaryField: " + primaryFieldType.Name + "; GetType(): " + primaryFieldType.GetType().Name + "; reflected: " + primaryFieldType.ReflectedType);
                    //Debug.Log(gameObject.name + "ObjectMessageController.ProcessEvent(): object: " + primaryFieldObject);

                    FieldInfo usedFieldType = primaryFieldType;
                    object usedFieldValue = primaryFieldValue;
                    object usedFieldObject = primaryFieldObject;

                    if (propertyResponseNode.MySubPropertyName != string.Empty) {
                        //Debug.Log(gameObject.name + "ObjectMessageController.ProcessEvent(): sub property name was: " + propertyResponseNode.MySubPropertyName);
                        usedFieldType = primaryFieldValue.GetType().GetField(propertyResponseNode.MySubPropertyName);
                        //Debug.Log(gameObject.name + "ObjectMessageController.ProcessEvent(): usedFieldType: " + usedFieldType.Name);
                        usedFieldValue = usedFieldType.GetValue(primaryFieldValue);
                        usedFieldObject = primaryFieldValue;
                        //Debug.Log(gameObject.name + "ObjectMessageController.ProcessEvent(): usedFieldObject: " + usedFieldObject);
                        //Debug.Log(gameObject.name + "ObjectMessageController.ProcessEvent(): primaryField.GetType(): " + primaryField.ReflectedType.Name + "; usedfield.GetType(): " + usedField.GetType().Name + "; reflected: " + usedField.ReflectedType.Name);
                        foreach (FieldInfo fieldInfo in primaryFieldType.GetValue(component).GetType().GetFields()) {
                            //Debug.Log(gameObject.name + "ObjectMessageController.ProcessEvent(): found sub field: " + fieldInfo.Name);
                        }
                    } else {
                        //Debug.Log(gameObject.name + "ObjectMessageController.ProcessEvent(): sub property name was empty");
                    }

                    //Debug.Log(gameObject.name + "ObjectMessageController.ProcessEvent(): usedFieldType: " + usedFieldType.Name + "; GetType(): " + usedFieldType.GetType().Name + "; reflected: " + usedFieldType.ReflectedType);
                    //Debug.Log(gameObject.name + "ObjectMessageController.ProcessEvent(): object: " + usedFieldObject);


                    if (propertyResponseNode.MyParameter == EventParamType.noneType) {
                        usedFieldType.SetValue(usedFieldObject, null);
                    } else if (propertyResponseNode.MyParameter == EventParamType.floatType) {
                        usedFieldType.SetValue(usedFieldObject, usedEventParam.FloatParam);
                        //gameObject.SendMessage(propertyResponseNode.MyFunctionName, usedEventParam.FloatParam, SendMessageOptions.DontRequireReceiver);
                    } else if (propertyResponseNode.MyParameter == EventParamType.intType) {
                        usedFieldType.SetValue(usedFieldObject, usedEventParam.IntParam);
                        //gameObject.SendMessage(propertyResponseNode.MyFunctionName, usedEventParam.IntParam, SendMessageOptions.DontRequireReceiver);
                    } else if (propertyResponseNode.MyParameter == EventParamType.boolType) {
                        usedFieldType.SetValue(usedFieldObject, usedEventParam.BoolParam);
                        //gameObject.SendMessage(propertyResponseNode.MyFunctionName, usedEventParam.BoolParam, SendMessageOptions.DontRequireReceiver);
                    } else if (propertyResponseNode.MyParameter == EventParamType.stringType) {
                        usedFieldType.SetValue(usedFieldObject, usedEventParam.StringParam);
                        //gameObject.SendMessage(propertyResponseNode.MyFunctionName, usedEventParam.StringParam, SendMessageOptions.DontRequireReceiver);
                    }
                }
            }

            // component responses
            foreach (ComponentResponseNode componentResponseNode in objectMessageNode.MyComponentResponses) {

                //Debug.Log(gameObject.name + "ObjectMessageController.ProcessEvent(): MyScriptName: " + propertyResponseNode.MyScriptName);
                Type type = Type.GetType(componentResponseNode.MyScriptName);
                //Debug.Log(gameObject.name + "ObjectMessageController.ProcessEvent(): MyScriptName: " + propertyResponseNode.MyScriptName + "; type: " + (type == null ? "null" : type.Name));

                Component component = GetComponent(Type.GetType(componentResponseNode.MyScriptName));
                if (component != null) {
                    if (componentResponseNode.MyComponentAction == ComponentAction.Enable) {
                        //Debug.Log(gameObject.name + "ObjectMessageController.ProcessEvent(): eventName: " + objectMessageNode.MyEventName + "; enabling " + component.name);
                        (component as MonoBehaviour).enabled = true;
                    } else {
                        //Debug.Log(gameObject.name + "ObjectMessageController.ProcessEvent(): eventName: " + objectMessageNode.MyEventName + "; disabling " + component.name);
                        (component as MonoBehaviour).enabled = false;
                    }
                }
            }

        }

        public void OnDestroy() {
            //Debug.Log(gameObject.name + "ObjectMessageController.OnDestroy()");
            CleanupMessageResponses();
        }

        private void CleanupMessageResponses() {
            //Debug.Log(gameObject.name + "ObjectMessageController.CleanupMessageResponses()");
            foreach (ObjectMessageNode objectMessageNode in messageTemplate.MyEventList) {
                //Debug.Log(gameObject.name + "ObjectMessageController.CleanupMessageResponses(): processing objectMessageNode");
                messageDictionary.Remove(objectMessageNode.MyEventName);
                SystemEventManager.StopListening(objectMessageNode.MyEventName, CallbackFunction);
            }
        }

    }

}
