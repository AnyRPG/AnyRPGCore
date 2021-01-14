using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Object Message Template", menuName = "AnyRPG/ObjectMessageTemplate")]
    [System.Serializable]
    public class ObjectMessageTemplate : DescribableResource {

        [Header("System Events")]

        [Tooltip("Subscribe to events emmitted by the SystemEventManager")]
        [FormerlySerializedAs("eventList")]
        [SerializeField]
        private List<SystemEventResponseNode> systemEventList = new List<SystemEventResponseNode>();

        [Header("Local Events")]

        [Tooltip("Subscribe to events on a monobehavior on the gameObject this script is attached to")]
        [SerializeField]
        private List<LocalEventResponseNode> localEventList = new List<LocalEventResponseNode>();

        public List<SystemEventResponseNode> MySystemEventList { get => systemEventList; set => systemEventList = value; }
        public List<LocalEventResponseNode> MyLocalEventList { get => localEventList; set => localEventList = value; }
    }

    [System.Serializable]
    public class EventResponseNode {

        [SerializeField]
        protected string eventName = string.Empty;

        [Tooltip("Set this to any positive number above zero to limit the number of times this event response will be processed.")]
        [SerializeField]
        private int responseLimit = 0;

        [Tooltip("Should this event be subscribed to in the Awake or Start stage.")]
        [SerializeField]
        private SubscribeStage subscribeStage = SubscribeStage.Awake;

        private int responseCounter = 0;


        [Tooltip("These responses use the Unity SendMessage functionality to send a message to the current GameObject")]
        [FormerlySerializedAs("responses")]
        [SerializeField]
        private List<MessageResponseNode> messageResponses = new List<MessageResponseNode>();

        [Tooltip("These responses set a property on a script or a component of a script")]
        [SerializeField]
        private List<PropertyResponseNode> propertyResponses = new List<PropertyResponseNode>();

        [Tooltip("These responses enable or disable monobehaviors")]
        [SerializeField]
        private List<ComponentResponseNode> componentResponses = new List<ComponentResponseNode>();

        [Tooltip("These responses invoke methods on a script or property of a script")]
        [SerializeField]
        private List<InvokeResponseNode> invokeResponses = new List<InvokeResponseNode>();

        public string MyEventName { get => eventName; set => eventName = value; }
        public List<MessageResponseNode> MessageResponses { get => messageResponses; set => messageResponses = value; }
        public List<PropertyResponseNode> PropertyResponses { get => propertyResponses; set => propertyResponses = value; }
        public List<ComponentResponseNode> ComponentResponses { get => componentResponses; set => componentResponses = value; }
        public int ResponseLimit { get => responseLimit; set => responseLimit = value; }
        public int ResponseCounter { get => responseCounter; set => responseCounter = value; }
        public List<InvokeResponseNode> InvokeResponses { get => invokeResponses; set => invokeResponses = value; }
        public SubscribeStage SubscribeStage { get => subscribeStage; set => subscribeStage = value; }

        public virtual void StopListening(ObjectMessageController objectMessageController) {
            // do nothing.  meant to be overwritten.
        }
    }

    [System.Serializable]
    public class LocalEventResponseNode : EventResponseNode {

        // the monobehavior script to access
        [SerializeField]
        private string scriptName = string.Empty;

        // we need this reference to find the stop listening method when we unsubscribe
        private EventInfo localEventInfo = null;

        // we need to create a dynamic method so we can add some hard coded parameters because the event we are listenting to will not send us any
        private DynamicMethod dynamicHandler = null;

        // we will want to remove the delegate after we stop listening
        private Delegate dynamicDelegate = null;

        private Component scriptComponent = null;

        public string MyScriptName { get => scriptName; set => scriptName = value; }
        public EventInfo LocalEventInfo { get => localEventInfo; set => localEventInfo = value; }
        public DynamicMethod DynamicHandler { get => dynamicHandler; set => dynamicHandler = value; }
        public Delegate DynamicDelegate { get => dynamicDelegate; set => dynamicDelegate = value; }
        public Component ScriptComponent { get => scriptComponent; set => scriptComponent = value; }

        public override void StopListening(ObjectMessageController objectMessageController) {
            base.StopListening(objectMessageController);
            if (objectMessageController.LocalEventDictionary.ContainsKey(MyEventName)) {

                // get a areference to the delegate add method
                MethodInfo removeHandler = localEventInfo.GetRemoveMethod();

                // subscribe our dynamic method to the event
                removeHandler.Invoke(scriptComponent, new System.Object[] { dynamicDelegate });

                // clear the variables
                dynamicDelegate = null;
                localEventInfo = null;
                scriptComponent = null;
                dynamicHandler = null;

                objectMessageController.LocalEventDictionary.Remove(MyEventName);
            }
        }
    }


    [System.Serializable]
    public class SystemEventResponseNode : EventResponseNode {

        private Action<string, EventParamProperties> listener = null;

        public Action<string, EventParamProperties> Listener { get => listener; set => listener = value; }

        public override void StopListening(ObjectMessageController objectMessageController) {
            base.StopListening(objectMessageController);
            if (objectMessageController.SystemEventDictionary.ContainsKey(MyEventName)) {
                SystemEventManager.StopListening(MyEventName, listener);
                objectMessageController.SystemEventDictionary.Remove(MyEventName);
            }
        }
    }

    [System.Serializable]
    public class MessageResponseNode {

        // the function to call with sendmessage call
        [SerializeField]
        private string functionName = string.Empty;

        [SerializeField]
        private EventParamType parameter = EventParamType.noneType;

        [SerializeField]
        private bool useCustomParam = false;

        [SerializeField]
        private EventParamProperties customParameters = new EventParamProperties();

        public EventParamType MyParameter { get => parameter; set => parameter = value; }
        public bool MyUseCustomParam { get => useCustomParam; set => useCustomParam = value; }
        public string MyFunctionName { get => functionName; set => functionName = value; }
        public EventParamProperties MyCustomParameters { get => customParameters; set => customParameters = value; }
    }

    [System.Serializable]
    public class PropertyResponseNode {

        // the monobehavior script to access
        [SerializeField]
        private string scriptName = string.Empty;

        // the public property on the script to change
        [SerializeField]
        private string propertyName = string.Empty;

        [SerializeField]
        private string subPropertyName = string.Empty;

        [SerializeField]
        private EventParamType parameter = EventParamType.noneType;

        [SerializeField]
        private bool useCustomParam = false;

        
        [SerializeField]
        private EventParamProperties customParameters = new EventParamProperties();

        //[SerializeField]
        //private List<EventParam> customParameterList = new List<EventParam>();

        public EventParamType Parameter { get => parameter; set => parameter = value; }
        public bool UseCustomParam { get => useCustomParam; set => useCustomParam = value; }
        public EventParamProperties CustomParameters { get => customParameters; set => customParameters = value; }
        public string ScriptName { get => scriptName; set => scriptName = value; }
        public string PropertyName { get => propertyName; set => propertyName = value; }
        public string SubPropertyName { get => subPropertyName; set => subPropertyName = value; }
    }

    [System.Serializable]
    public class InvokeResponseNode {

        [Tooltip("The monobehavior script which has the method we want to invoke")]
        [SerializeField]
        private string scriptName = string.Empty;

        [Tooltip("If no subMethodName exists, this is the method.  If a subMethodName exists, this is the parent property of that method")]
        [SerializeField]
        private string propertyName = string.Empty;

        [SerializeField]
        private string subMethodName = string.Empty;

        [SerializeField]
        private EventParamType parameter = EventParamType.noneType;

        [SerializeField]
        private bool useCustomParam = false;

        [SerializeField]
        private EventParamProperties customParameters = new EventParamProperties();

        public EventParamType Parameter { get => parameter; set => parameter = value; }
        public bool UseCustomParam { get => useCustomParam; set => useCustomParam = value; }
        public EventParamProperties CustomParameters { get => customParameters; set => customParameters = value; }
        public string ScriptName { get => scriptName; set => scriptName = value; }
        public string PropertyName { get => propertyName; set => propertyName = value; }
        public string SubMethodName { get => subMethodName; set => subMethodName = value; }
    }



    [System.Serializable]
    public class ComponentResponseNode {

        // the monobehavior script to access
        [SerializeField]
        private string scriptName = string.Empty;

        // the public property on the script to change
        [SerializeField]
        private ComponentAction componentAction = ComponentAction.Disable;


        public string MyScriptName { get => scriptName; set => scriptName = value; }
        public ComponentAction MyComponentAction { get => componentAction; set => componentAction = value; }
    }

    [System.Serializable]
    public class ObjectConfigurationNode {

        [SerializeField]
        private string objectName = string.Empty;

        [SerializeField]
        private List<SimpleParamNode> simpleParams = new List<SimpleParamNode>();

        public string MyObjectName { get => objectName; set => objectName = value; }
        public List<SimpleParamNode> MySimpleParams { get => simpleParams; set => simpleParams = value; }
    }

    [System.Serializable]
    public class SimpleParamNode {

        [SerializeField]
        private SimpleParamType paramType = SimpleParamType.intType;

        [SerializeField]
        private bool useCustomParam = false;

        [SerializeField]
        private EventParam simpleParams = new EventParam();

        public SimpleParamType MyParamType { get => paramType; set => paramType = value; }
        public bool MyUseCustomParam { get => useCustomParam; set => useCustomParam = value; }
        public EventParam MySimpleParams { get => simpleParams; set => simpleParams = value; }
    }



    public enum EventParamType { noneType, stringType, intType, floatType, boolType, objectType }

    public enum SimpleParamType { stringType, intType, floatType, boolType }

    public enum ComponentAction { Enable, Disable }

    public enum SubscribeStage { Awake, Start }

}