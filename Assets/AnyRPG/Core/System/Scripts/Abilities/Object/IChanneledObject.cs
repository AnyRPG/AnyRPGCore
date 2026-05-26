using UnityEngine;

namespace AnyRPG {
    public interface IChanneledObject {
        GameObject EndObject { get; set; }
        Vector3 EndPosition { get; set; }
        GameObject StartObject { get; set; }
        Vector3 StartPosition { get; set; }
        void Setup(GameObject startObject, Vector3 startPosition, GameObject endObject, Vector3 endPosition, SystemGameManager systemGameManager);
    }
}