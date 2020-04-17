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


        [SerializeField]
        private List<ObjectMessageNode> eventList = new List<ObjectMessageNode>();

        public List<ObjectMessageNode> MyEventList { get => eventList; set => eventList = value; }
    }

    [System.Serializable]
    public class ObjectMessageNode {

        [SerializeField]
        private string eventName = string.Empty;

        [FormerlySerializedAs("responses")]
        [SerializeField]
        private List<MessageResponseNode> messageResponses = new List<MessageResponseNode>();

        [SerializeField]
        private List<PropertyResponseNode> propertyResponses = new List<PropertyResponseNode>();

        public string MyEventName { get => eventName; set => eventName = value; }
        public List<MessageResponseNode> MyMessageResponses { get => messageResponses; set => messageResponses = value; }
        public List<PropertyResponseNode> MyPropertyResponses { get => propertyResponses; set => propertyResponses = value; }
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
        private EventParam customParameters = new EventParam();

        public EventParamType MyParameter { get => parameter; set => parameter = value; }
        public bool MyUseCustomParam { get => useCustomParam; set => useCustomParam = value; }
        public string MyFunctionName { get => functionName; set => functionName = value; }
        public EventParam MyCustomParameters { get => customParameters; set => customParameters = value; }
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
        private EventParam customParameters = new EventParam();

        public EventParamType MyParameter { get => parameter; set => parameter = value; }
        public bool MyUseCustomParam { get => useCustomParam; set => useCustomParam = value; }
        public EventParam MyCustomParameters { get => customParameters; set => customParameters = value; }
        public string MyScriptName { get => scriptName; set => scriptName = value; }
        public string MyPropertyName { get => propertyName; set => propertyName = value; }
        public string MySubPropertyName { get => subPropertyName; set => subPropertyName = value; }
    }


    public enum EventParamType { noneType, stringType, intType, floatType, boolType }

}