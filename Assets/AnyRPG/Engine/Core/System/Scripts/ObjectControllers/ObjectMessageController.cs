using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;


namespace AnyRPG {
    public class ObjectMessageController : MonoBehaviour {

        [SerializeField]
        private ObjectMessageTemplate messageTemplate = null;

        private ObjectMessageTemplate eventTemplate = null;

        // 2 dictionaries to prevent naming conflicts between system and local events
        private Dictionary<string, SystemEventResponseNode> systemEventDictionary = new Dictionary<string, SystemEventResponseNode>();

        private Dictionary<string, LocalEventResponseNode> localEventDictionary = new Dictionary<string, LocalEventResponseNode>();

        public Dictionary<string, SystemEventResponseNode> SystemEventDictionary { get => systemEventDictionary; set => systemEventDictionary = value; }
        public Dictionary<string, LocalEventResponseNode> LocalEventDictionary { get => localEventDictionary; set => localEventDictionary = value; }

        // Start is called before the first frame update
        void Awake() {
            //Debug.Log(gameObject.name + ".ObjectMessageController.Start()");
            SetupScriptableObjects();
            InitializeEventResponses(SubscribeStage.Awake);
        }

        private void Start() {
            InitializeEventResponses(SubscribeStage.Start);
        }

        private void InitializeEventResponses(SubscribeStage subscribeStage) {
            //Debug.Log(gameObject.name + ".ObjectMessageController.InitializeMessageResponses()");
            if (eventTemplate == null) {
                return;
            }

            InitializeSystemEventResponses(subscribeStage);

            InitializeLocalEventResponses(subscribeStage);

        }

        public void InitializeSystemEventResponses(SubscribeStage subscribeStage) {
            // system event responses
            foreach (SystemEventResponseNode eventResponseNode in eventTemplate.MySystemEventList) {
                if (eventResponseNode.SubscribeStage == subscribeStage) {
                    systemEventDictionary[eventResponseNode.MyEventName] = eventResponseNode;
                    //Debug.Log(gameObject.name + ".ObjectMessageController.InitializeMessageResponses(): listening to: " + objectMessageNode.MyEventName);
                    SystemEventManager.StartListening(eventResponseNode.MyEventName, CallbackFunction);
                    eventResponseNode.Listener = CallbackFunction;
                }
            }
        }

        public void InitializeLocalEventResponses(SubscribeStage subscribeStage) {
            // local event responses

            foreach (LocalEventResponseNode eventResponseNode in eventTemplate.MyLocalEventList) {

                if (eventResponseNode.SubscribeStage == subscribeStage) {
                    // add this event response to the dictionary for lookups when the event is received
                    localEventDictionary[eventResponseNode.MyEventName] = eventResponseNode;

                    ObjectMessageController objectMessageController = this;
                    // get the type of the script that has the event that we want to subscribe to
                    Type scriptType = Type.GetType(eventResponseNode.MyScriptName);

                    if (scriptType == null) {
                        Debug.LogError("Could not get Got scriptType " + eventResponseNode.MyScriptName);
                    }

                    // get a reference to the script
                    eventResponseNode.ScriptComponent = GetComponent(scriptType);

                    if (eventResponseNode.ScriptComponent == null) {
                        Debug.LogError("Could not get component");
                    }

                    // get a reference to the event
                    //Debug.Log("Attempting to get event " + eventResponseNode.MyEventName);
                    eventResponseNode.LocalEventInfo = scriptType.GetEvent(eventResponseNode.MyEventName);
                    
                    if (eventResponseNode.LocalEventInfo == null) {
                        Debug.LogError("Could not get event " + eventResponseNode.MyEventName);
                    }

                    // get a reference to the event type
                    Type tDelegate = eventResponseNode.LocalEventInfo.EventHandlerType;

                    // ensure the return type of the event is void
                    Type returnType = tDelegate.GetMethod("Invoke").ReturnType;
                    if (returnType != typeof(void))
                        throw new ApplicationException("Delegate has a return type.");

                    // make a list of parameters with the first parameter being this object type
                    // the rest are the actual parameters the event will be sending back to us
                    // we are going to ignore them for now, but still need to receive them
                    Type[] parameterTypeArray = GetDelegateParameterTypes(tDelegate);
                    Type[] typeArray = new Type[parameterTypeArray.Length + 1];
                    typeArray[0] = typeof(ObjectMessageController);
                    Array.Copy(parameterTypeArray, 0, typeArray, 1, parameterTypeArray.Length);

                    // construct a dynamic function with no name and attack it to the current class
                    eventResponseNode.DynamicHandler = new DynamicMethod("", null, typeArray, typeof(ObjectMessageController));

                    // create a generator to generate the function body
                    ILGenerator ilgen = eventResponseNode.DynamicHandler.GetILGenerator();

                    // get a reference to the method we will be calling from our dynamic function to process this event
                    Type[] processLocalEventParameters = { typeof(string) };
                    MethodInfo processLocalEvent =
                        typeof(ObjectMessageController).GetMethod("ProcessLocalEvent", processLocalEventParameters);

                    // because we bind with 'this' in CreateDelegate we have a 'magical' first parameter that is actually this instance
                    // i say 'magical' because the event we are subscribing to does not emit that paramter, but we still receive it
                    // lets put it on the stack, so when we call ProcessLocalEvent, we will also magically call this instances copy of it
                    // once again i say 'magically' because that method does not actually accept an instance parameter, only a string
                    ilgen.Emit(OpCodes.Ldarg_0);

                    // now we can put the string parameter on the stack
                    ilgen.Emit(OpCodes.Ldstr, eventResponseNode.MyEventName);

                    // call the ProcessLocalEvent function with the 2 parameters we just placed on the stack
                    ilgen.Emit(OpCodes.Call, processLocalEvent);

                    // there are no parameters left because we just sent them to the event.  we can safely return now
                    ilgen.Emit(OpCodes.Ret);

                    // create a delegate that matches the event signature of the event we need to subscribe to and bind to this instance
                    eventResponseNode.DynamicDelegate = eventResponseNode.DynamicHandler.CreateDelegate(tDelegate, this);

                    // get a areference to the delegate add method
                    MethodInfo addHandler = eventResponseNode.LocalEventInfo.GetAddMethod();

                    // subscribe our dynamic method to the event
                    addHandler.Invoke(eventResponseNode.ScriptComponent, new System.Object[] { eventResponseNode.DynamicDelegate });
                }
            }
        }

        public void ProcessLocalEvent(string eventName) {
            //Debug.Log("eventName was: " + eventName);
            if (enabled == false) {
                // we should not process events if we are disabled
                return;
            }

            if (localEventDictionary.ContainsKey(eventName)) {
                //Debug.Log("ProcessLocalEvent(): Responding to event: " + eventName);
                LocalEventResponseNode localEventResponseNode = localEventDictionary[eventName];
                EventParamProperties eventParamProperties = new EventParamProperties();

                ProcessEvent(localEventResponseNode, eventParamProperties);
            }
        }

        private Type[] GetDelegateParameterTypes(Type d) {
            if (d.BaseType != typeof(MulticastDelegate))
                throw new ApplicationException("Not a delegate.");

            MethodInfo invoke = d.GetMethod("Invoke");
            if (invoke == null)
                throw new ApplicationException("Not a delegate.");

            ParameterInfo[] parameters = invoke.GetParameters();
            Type[] typeParameters = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++) {
                typeParameters[i] = parameters[i].ParameterType;
            }
            return typeParameters;
        }

        public void CallbackFunction(string eventName, EventParamProperties eventParam) {
            //Debug.Log(gameObject.name + ".ObjectMessageController.CallbackFunction(" + eventName + ")");
            if (enabled == false) {
                // we should not process events if we are disabled
                return;
            }
            if (systemEventDictionary.ContainsKey(eventName)) {
                ProcessEvent(systemEventDictionary[eventName], eventParam);
            }
        }

        private void ProcessMessageResponses(EventResponseNode objectMessageNode, EventParamProperties eventParam) {
            // message responses
            foreach (MessageResponseNode messageResponseNode in objectMessageNode.MessageResponses) {
                EventParamProperties usedEventParam = eventParam;
                if (messageResponseNode.MyUseCustomParam == true) {
                    usedEventParam = messageResponseNode.MyCustomParameters;
                }

                if (messageResponseNode.MyParameter == EventParamType.noneType) {
                    gameObject.SendMessage(messageResponseNode.MyFunctionName, SendMessageOptions.DontRequireReceiver);
                } else if (messageResponseNode.MyParameter == EventParamType.floatType) {
                    gameObject.SendMessage(messageResponseNode.MyFunctionName, usedEventParam.simpleParams.FloatParam, SendMessageOptions.DontRequireReceiver);
                } else if (messageResponseNode.MyParameter == EventParamType.intType) {
                    gameObject.SendMessage(messageResponseNode.MyFunctionName, usedEventParam.simpleParams.IntParam, SendMessageOptions.DontRequireReceiver);
                } else if (messageResponseNode.MyParameter == EventParamType.boolType) {
                    gameObject.SendMessage(messageResponseNode.MyFunctionName, usedEventParam.simpleParams.BoolParam, SendMessageOptions.DontRequireReceiver);
                } else if (messageResponseNode.MyParameter == EventParamType.stringType) {
                    gameObject.SendMessage(messageResponseNode.MyFunctionName, usedEventParam.simpleParams.StringParam, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        private void ProcessPropertyResponses(EventResponseNode objectMessageNode, EventParamProperties eventParam) {
            // property responses
            foreach (PropertyResponseNode propertyResponseNode in objectMessageNode.PropertyResponses) {
                EventParamProperties usedEventParam = eventParam;
                if (propertyResponseNode.UseCustomParam == true) {
                    usedEventParam = propertyResponseNode.CustomParameters;
                }

                //Debug.Log(gameObject.name + "ObjectMessageController.ProcessEvent(): MyScriptName: " + propertyResponseNode.MyScriptName);
                Type type = Type.GetType(propertyResponseNode.ScriptName);
                //Debug.Log(gameObject.name + "ObjectMessageController.ProcessEvent(): MyScriptName: " + propertyResponseNode.MyScriptName + "; type: " + (type == null ? "null" : type.Name));

                Component component = GetComponent(Type.GetType(propertyResponseNode.ScriptName));
                if (component != null) {
                    foreach (FieldInfo fieldInfo in (component as MonoBehaviour).GetType().GetFields()) {
                        //Debug.Log(gameObject.name + "ObjectMessageController.ProcessEvent(): found field: "+ fieldInfo.Name);
                    }
                    //(component as MonoBehaviour).GetType().GetFields();

                    FieldInfo primaryFieldType = (component as MonoBehaviour).GetType().GetField(propertyResponseNode.PropertyName);
                    object primaryFieldObject = component;
                    object primaryFieldValue = primaryFieldType.GetValue(primaryFieldObject);
                    //Debug.Log(gameObject.name + "ObjectMessageController.ProcessEvent(): primaryField: " + primaryFieldType.Name + "; GetType(): " + primaryFieldType.GetType().Name + "; reflected: " + primaryFieldType.ReflectedType);
                    //Debug.Log(gameObject.name + "ObjectMessageController.ProcessEvent(): object: " + primaryFieldObject);

                    FieldInfo usedFieldType = primaryFieldType;
                    object usedFieldValue = primaryFieldValue;
                    object usedFieldObject = primaryFieldObject;

                    if (propertyResponseNode.SubPropertyName != string.Empty) {
                        //Debug.Log(gameObject.name + "ObjectMessageController.ProcessEvent(): sub property name was: " + propertyResponseNode.MySubPropertyName);
                        usedFieldType = primaryFieldValue.GetType().GetField(propertyResponseNode.SubPropertyName);
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


                    if (propertyResponseNode.Parameter == EventParamType.noneType) {
                        usedFieldType.SetValue(usedFieldObject, null);
                    } else if (propertyResponseNode.Parameter == EventParamType.floatType) {
                        usedFieldType.SetValue(usedFieldObject, usedEventParam.simpleParams.FloatParam);
                        //gameObject.SendMessage(propertyResponseNode.MyFunctionName, usedEventParam.FloatParam, SendMessageOptions.DontRequireReceiver);
                    } else if (propertyResponseNode.Parameter == EventParamType.intType) {
                        usedFieldType.SetValue(usedFieldObject, usedEventParam.simpleParams.IntParam);
                        //gameObject.SendMessage(propertyResponseNode.MyFunctionName, usedEventParam.IntParam, SendMessageOptions.DontRequireReceiver);
                    } else if (propertyResponseNode.Parameter == EventParamType.boolType) {
                        usedFieldType.SetValue(usedFieldObject, usedEventParam.simpleParams.BoolParam);
                        //gameObject.SendMessage(propertyResponseNode.MyFunctionName, usedEventParam.BoolParam, SendMessageOptions.DontRequireReceiver);
                    } else if (propertyResponseNode.Parameter == EventParamType.stringType) {
                        usedFieldType.SetValue(usedFieldObject, usedEventParam.simpleParams.StringParam);
                        //gameObject.SendMessage(propertyResponseNode.MyFunctionName, usedEventParam.StringParam, SendMessageOptions.DontRequireReceiver);
                    } else if (propertyResponseNode.Parameter == EventParamType.objectType) {

                        string usedObjectTypeName = string.Empty;
                        if (propertyResponseNode.CustomParameters.objectParam.MyObjectName != null && propertyResponseNode.CustomParameters.objectParam.MyObjectName != string.Empty) {
                            usedObjectTypeName = propertyResponseNode.CustomParameters.objectParam.MyObjectName;
                        } else {
                            usedObjectTypeName = eventParam.objectParam.MyObjectName;
                        }
                        //Debug.Log(gameObject.name + "ObjectMessageController.ProcessPropertyResponses(): usedObjectTypeName: " + usedObjectTypeName);


                        // set object type
                        Type objectType = Type.GetType(usedObjectTypeName);
                        int numParameters = 0;

                        // get parameter counts and lists
                        List<SimpleParamNode> paramNodes;
                        //Debug.Log(gameObject.name + "ObjectMessageController.ProcessPropertyResponses(): event parameter count : " + eventParam.objectParam.MySimpleParams.Count);
                        //Debug.Log(gameObject.name + "ObjectMessageController.ProcessPropertyResponses(): response parameter count : " + propertyResponseNode.MyCustomParameters.objectParam.MySimpleParams.Count);
                        if (propertyResponseNode.UseCustomParam == false) {
                            // get parameters from input
                            numParameters = eventParam.objectParam.MySimpleParams.Count;
                            paramNodes = eventParam.objectParam.MySimpleParams;
                        } else {
                            // get custom parameters
                            numParameters = propertyResponseNode.CustomParameters.objectParam.MySimpleParams.Count;
                            paramNodes = propertyResponseNode.CustomParameters.objectParam.MySimpleParams;
                        }
                        Type[] parameterTypes = new Type[numParameters];
                        object[] parameterValues = new object[numParameters];

                        //Debug.Log(gameObject.name + "ObjectMessageController.ProcessPropertyResponses(): usedObjectTypeName: " + usedObjectTypeName + "; parameters: " + numParameters);

                        // set parameter types and values
                        int index = 0;
                        foreach (SimpleParamNode simpleParamNode in paramNodes) {
                            if (simpleParamNode.MyParamType == SimpleParamType.intType) {
                                parameterTypes[index] = typeof(int);
                                if (simpleParamNode.MyUseCustomParam == true) {
                                    parameterValues[index] = simpleParamNode.MySimpleParams.IntParam;
                                } else {
                                    parameterValues[index] = eventParam.objectParam.MySimpleParams[index].MySimpleParams.IntParam;
                                }
                            } else if (simpleParamNode.MyParamType == SimpleParamType.floatType) {
                                parameterTypes[index] = typeof(float);
                                if (simpleParamNode.MyUseCustomParam == true) {
                                    parameterValues[index] = simpleParamNode.MySimpleParams.FloatParam;
                                } else {
                                    parameterValues[index] = eventParam.objectParam.MySimpleParams[index].MySimpleParams.FloatParam;
                                }
                            } else if (simpleParamNode.MyParamType == SimpleParamType.stringType) {
                                parameterTypes[index] = typeof(string);
                                if (simpleParamNode.MyUseCustomParam == true) {
                                    parameterValues[index] = simpleParamNode.MySimpleParams.StringParam;
                                } else {
                                    parameterValues[index] = eventParam.objectParam.MySimpleParams[index].MySimpleParams.StringParam;
                                }
                            } else if (simpleParamNode.MyParamType == SimpleParamType.boolType) {
                                parameterTypes[index] = typeof(bool);
                                if (simpleParamNode.MyUseCustomParam == true) {
                                    parameterValues[index] = simpleParamNode.MySimpleParams.BoolParam;
                                } else {
                                    parameterValues[index] = eventParam.objectParam.MySimpleParams[index].MySimpleParams.BoolParam;
                                }
                            }
                            index++;
                        }

                        // get constructor for object
                        ConstructorInfo constructorInfoObj = objectType.GetConstructor(
                                        BindingFlags.Instance | BindingFlags.Public, null,
                                        CallingConventions.HasThis, parameterTypes, null);

                        // call constructor and get instance object
                        object instanceObject = constructorInfoObj.Invoke(parameterValues);

                        // set the field value to the new object
                        usedFieldType.SetValue(usedFieldObject, instanceObject);
                    }
                }
            }
        }

        private bool ProcessInvokeResponses(EventResponseNode eventResponseNode, EventParamProperties eventParam) {
            //Debug.Log(gameObject.name + ".ObjectMessageController.ProcessInvokeResponses():");

            foreach (InvokeResponseNode invokeResponseNode in eventResponseNode.InvokeResponses) {
                EventParamProperties usedEventParam = eventParam;
                if (invokeResponseNode.UseCustomParam == true) {
                    usedEventParam = invokeResponseNode.CustomParameters;
                }

                Type scriptType = Type.GetType(invokeResponseNode.ScriptName);
                //Debug.Log(gameObject.name + "ObjectMessageController.ProcessInvokeResponses(): Got scriptType: " + scriptType);

                Component component = GetComponent(scriptType);
                if (component != null) {
                    foreach (FieldInfo fieldInfo in (component as MonoBehaviour).GetType().GetFields()) {
                        //Debug.Log(gameObject.name + "ObjectMessageController.ProcessEvent(): found field: "+ fieldInfo.Name);
                    }

                    string usedMethodName = invokeResponseNode.PropertyName;
                    object primaryFieldObject = component;
                    object usedFieldObject = primaryFieldObject;

                    //Debug.Log(gameObject.name + "ObjectMessageController.ProcessInvokeResponses(): primaryObject: " + (primaryFieldObject == null ? "null" : primaryFieldObject.ToString()) + "; usedObject: " + (usedFieldObject == null ? "null" : usedFieldObject.ToString()) + "; component: " + (component as MonoBehaviour).ToString());

                    if (invokeResponseNode.SubMethodName != null && invokeResponseNode.SubMethodName != string.Empty) {
                        usedMethodName = invokeResponseNode.SubMethodName;

                        FieldInfo primaryFieldType = (component as MonoBehaviour).GetType().GetField(invokeResponseNode.PropertyName);
                        //Debug.Log(gameObject.name + "ObjectMessageController.ProcessInvokeResponses(): primary field type: " + primaryFieldType.Name);
                        object usedFieldValue = primaryFieldType.GetValue(primaryFieldObject);
                        if (usedFieldValue == null) {
                            // the field we are trying to access may not be initialized yet
                            return false;
                        }
                        usedFieldObject = usedFieldValue;
                        //FieldInfo usedFieldType = usedFieldValue.GetType().GetField(invokeResponseNode.PropertyName);

                        //usedFieldObject = primaryFieldType.GetValue(usedFieldValue);
                    }
                    //Debug.Log(gameObject.name + "ObjectMessageController.ProcessInvokeResponses(): object: " + (usedFieldObject == null ? "null" : usedFieldObject.ToString()) + "; component: " + (component as MonoBehaviour).name);

                    object[] parameterValues = new object[1];

                    // in the case of any single value, we will send that as the only parameter
                    if (invokeResponseNode.Parameter == EventParamType.noneType) {
                        parameterValues = new object[0];
                    } else if (invokeResponseNode.Parameter == EventParamType.floatType) {
                        parameterValues[0] = usedEventParam.simpleParams.FloatParam;
                    } else if (invokeResponseNode.Parameter == EventParamType.intType) {
                        parameterValues[0] = usedEventParam.simpleParams.IntParam;
                    } else if (invokeResponseNode.Parameter == EventParamType.boolType) {
                        parameterValues[0] = usedEventParam.simpleParams.BoolParam;
                    } else if (invokeResponseNode.Parameter == EventParamType.stringType) {
                        parameterValues[0] = usedEventParam.simpleParams.StringParam;
                    } else if (invokeResponseNode.Parameter == EventParamType.objectType) {
                        // in the case of an object, we will send the parameters that it contained
                        string usedObjectTypeName = string.Empty;
                        if (invokeResponseNode.CustomParameters.objectParam.MyObjectName != null && invokeResponseNode.CustomParameters.objectParam.MyObjectName != string.Empty) {
                            usedObjectTypeName = invokeResponseNode.CustomParameters.objectParam.MyObjectName;
                        } else {
                            usedObjectTypeName = eventParam.objectParam.MyObjectName;
                        }
                        //Debug.Log(gameObject.name + "ObjectMessageController.ProcessPropertyResponses(): usedObjectTypeName: " + usedObjectTypeName);


                        // set object type
                        Type objectType = Type.GetType(usedObjectTypeName);
                        int numParameters = 0;

                        // get parameter counts and lists
                        List<SimpleParamNode> paramNodes;
                        //Debug.Log(gameObject.name + "ObjectMessageController.ProcessPropertyResponses(): event parameter count : " + eventParam.objectParam.MySimpleParams.Count);
                        //Debug.Log(gameObject.name + "ObjectMessageController.ProcessPropertyResponses(): response parameter count : " + propertyResponseNode.MyCustomParameters.objectParam.MySimpleParams.Count);
                        if (invokeResponseNode.UseCustomParam == false) {
                            // get parameters from input
                            numParameters = eventParam.objectParam.MySimpleParams.Count;
                            paramNodes = eventParam.objectParam.MySimpleParams;
                        } else {
                            // get custom parameters
                            numParameters = invokeResponseNode.CustomParameters.objectParam.MySimpleParams.Count;
                            paramNodes = invokeResponseNode.CustomParameters.objectParam.MySimpleParams;
                        }
                        Type[] parameterTypes = new Type[numParameters];
                        parameterValues = new object[numParameters];

                        //Debug.Log(gameObject.name + "ObjectMessageController.ProcessPropertyResponses(): usedObjectTypeName: " + usedObjectTypeName + "; parameters: " + numParameters);

                        // set parameter types and values
                        int index = 0;
                        foreach (SimpleParamNode simpleParamNode in paramNodes) {
                            if (simpleParamNode.MyParamType == SimpleParamType.intType) {
                                parameterTypes[index] = typeof(int);
                                if (simpleParamNode.MyUseCustomParam == true) {
                                    parameterValues[index] = simpleParamNode.MySimpleParams.IntParam;
                                } else {
                                    parameterValues[index] = eventParam.objectParam.MySimpleParams[index].MySimpleParams.IntParam;
                                }
                            } else if (simpleParamNode.MyParamType == SimpleParamType.floatType) {
                                parameterTypes[index] = typeof(float);
                                if (simpleParamNode.MyUseCustomParam == true) {
                                    parameterValues[index] = simpleParamNode.MySimpleParams.FloatParam;
                                } else {
                                    parameterValues[index] = eventParam.objectParam.MySimpleParams[index].MySimpleParams.FloatParam;
                                }
                            } else if (simpleParamNode.MyParamType == SimpleParamType.stringType) {
                                parameterTypes[index] = typeof(string);
                                if (simpleParamNode.MyUseCustomParam == true) {
                                    parameterValues[index] = simpleParamNode.MySimpleParams.StringParam;
                                } else {
                                    parameterValues[index] = eventParam.objectParam.MySimpleParams[index].MySimpleParams.StringParam;
                                }
                            } else if (simpleParamNode.MyParamType == SimpleParamType.boolType) {
                                parameterTypes[index] = typeof(bool);
                                if (simpleParamNode.MyUseCustomParam == true) {
                                    parameterValues[index] = simpleParamNode.MySimpleParams.BoolParam;
                                } else {
                                    parameterValues[index] = eventParam.objectParam.MySimpleParams[index].MySimpleParams.BoolParam;
                                }
                            }
                            index++;
                        }
                    }
                    //Debug.Log(gameObject.name + "ObjectMessageController.ProcessPropertyResponses(): parameter count: " + parameterValues.Length);

                    // call the method with the correct list of parameters
                    MethodInfo invokedMethod = usedFieldObject.GetType().GetMethod(usedMethodName);
                    if (parameterValues.Length == 0) {
                        invokedMethod.Invoke(usedFieldObject, new object[] { });
                    } else {
                        invokedMethod.Invoke(usedFieldObject, new object[] { parameterValues });
                    }
                }
            }
            return true;
        }

        private void ProcessComponentResponses(EventResponseNode objectMessageNode, EventParamProperties eventParam) {
            // component responses
            foreach (ComponentResponseNode componentResponseNode in objectMessageNode.ComponentResponses) {

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

        private void ProcessEvent(EventResponseNode eventResponseNode, EventParamProperties eventParam) {
            //Debug.Log(gameObject.name + ".ObjectMessageController.ProcessEvent()");
            if (enabled == false) {
                // we should not process events if we are disabled
                return;
            }

            if (eventResponseNode.ResponseLimit > 0) {
                //Debug.Log(gameObject.name + ".ObjectMessageController.ProcessEvent(): responseLimit: " + eventResponseNode.ResponseLimit + "; counter: " + eventResponseNode.ResponseCounter);
                if (eventResponseNode.ResponseCounter >= eventResponseNode.ResponseLimit) {
                    
                    // unsubscribe first to avoid unnecessary event sending if we have passed the processing limit count
                    eventResponseNode.StopListening(this);

                    return;
                }
            }

            ProcessMessageResponses(eventResponseNode, eventParam);

            ProcessPropertyResponses(eventResponseNode, eventParam);

            ProcessComponentResponses(eventResponseNode, eventParam);

            if (ProcessInvokeResponses(eventResponseNode, eventParam) == true) {
                // only increment count on successful invocation
                eventResponseNode.ResponseCounter += 1;
            }
        }

        public void OnDisable() {
            //Debug.Log(gameObject.name + "ObjectMessageController.OnDisable()");
            CleanupEventResponses();
        }

        private void CleanupEventResponses() {
            //Debug.Log(gameObject.name + "ObjectMessageController.CleanupMessageResponses()");
            foreach (SystemEventResponseNode systemEventResponseNode in eventTemplate.MySystemEventList) {
                //Debug.Log(gameObject.name + "ObjectMessageController.CleanupMessageResponses(): processing objectMessageNode");
                //if (systemEventDictionary.ContainsKey(systemEventResponseNode.MyEventName)) {
                    //systemEventDictionary.Remove(systemEventResponseNode.MyEventName);
                    systemEventResponseNode.StopListening(this);
                //}
                //SystemEventManager.StopListening(systemEventResponseNode.MyEventName, CallbackFunction);
            }

            foreach (LocalEventResponseNode localEventResponseNode in eventTemplate.MyLocalEventList) {
                //Debug.Log(gameObject.name + "ObjectMessageController.CleanupMessageResponses(): processing objectMessageNode");
                //if (localEventDictionary.ContainsKey(localEventResponseNode.MyEventName)) {
                    //localEventDictionary.Remove(localEventResponseNode.MyEventName);
                    localEventResponseNode.StopListening(this);
                //}
            }

        }

        public void SetupScriptableObjects() {
            if (messageTemplate != null) {
                eventTemplate = ScriptableObject.Instantiate(messageTemplate);
            }
        }

    }

}
