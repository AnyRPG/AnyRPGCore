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

        [SerializeField]
        private List<MessageResponseNode> responses = new List<MessageResponseNode>();

        public string MyEventName { get => eventName; set => eventName = value; }
        public List<MessageResponseNode> MyResponses { get => responses; set => responses = value; }
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

    public enum EventParamType { noneType, stringType, intType, floatType, boolType }

}