using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Object Message Template", menuName = "AnyRPG/ObjectMessageTemplate")]
    [System.Serializable]
    public class ObjectMessageTemplate : DescribableResource {

        [FormerlySerializedAs("eventList")]
        [SerializeField]
        private List<ObjectMessageNode> systemEventList = new List<ObjectMessageNode>();

        [SerializeField]
        private List<LocalEventNode> localEventList = new List<LocalEventNode>();

        public List<ObjectMessageNode> MySystemEventList { get => systemEventList; set => systemEventList = value; }
        public List<LocalEventNode> MyLocalEventList { get => localEventList; set => localEventList = value; }
    }

    [System.Serializable]
    public class LocalEventNode {

        [SerializeField]
        private string eventName = string.Empty;

        // the monobehavior script to access
        [SerializeField]
        private string scriptName = string.Empty;

        [Tooltip("Set this to any positive number above zero to limit the number of times this event response will be processed.")]
        [SerializeField]
        private int responseLimit = 0;

        [FormerlySerializedAs("responses")]
        [SerializeField]
        private List<MessageResponseNode> messageResponses = new List<MessageResponseNode>();

        [SerializeField]
        private List<PropertyResponseNode> propertyResponses = new List<PropertyResponseNode>();

        [SerializeField]
        private List<ComponentResponseNode> componentResponses = new List<ComponentResponseNode>();

        public string MyEventName { get => eventName; set => eventName = value; }
        public List<MessageResponseNode> MyMessageResponses { get => messageResponses; set => messageResponses = value; }
        public List<PropertyResponseNode> MyPropertyResponses { get => propertyResponses; set => propertyResponses = value; }
        public List<ComponentResponseNode> MyComponentResponses { get => componentResponses; set => componentResponses = value; }
        public string MyScriptName { get => scriptName; set => scriptName = value; }
    }


    [System.Serializable]
    public class ObjectMessageNode {

        [SerializeField]
        private string eventName = string.Empty;

        [SerializeField]
        private List<MessageResponseNode> messageResponses = new List<MessageResponseNode>();

        [SerializeField]
        private List<PropertyResponseNode> propertyResponses = new List<PropertyResponseNode>();

        [SerializeField]
        private List<ComponentResponseNode> componentResponses = new List<ComponentResponseNode>();

        public string MyEventName { get => eventName; set => eventName = value; }
        public List<MessageResponseNode> MyMessageResponses { get => messageResponses; set => messageResponses = value; }
        public List<PropertyResponseNode> MyPropertyResponses { get => propertyResponses; set => propertyResponses = value; }
        public List<ComponentResponseNode> MyComponentResponses { get => componentResponses; set => componentResponses = value; }
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

        public EventParamType MyParameter { get => parameter; set => parameter = value; }
        public bool MyUseCustomParam { get => useCustomParam; set => useCustomParam = value; }
        public EventParamProperties MyCustomParameters { get => customParameters; set => customParameters = value; }
        public string MyScriptName { get => scriptName; set => scriptName = value; }
        public string MyPropertyName { get => propertyName; set => propertyName = value; }
        public string MySubPropertyName { get => subPropertyName; set => subPropertyName = value; }
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

}