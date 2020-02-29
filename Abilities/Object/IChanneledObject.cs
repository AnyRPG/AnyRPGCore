using UnityEngine;

namespace AnyRPG {
    public interface IChanneledObject {
        GameObject MyEndObject { get; set; }
        Vector3 MyEndPosition { get; set; }
        GameObject MyStartObject { get; set; }
        Vector3 MyStartPosition { get; set; }
        void Setup(GameObject startObject, Vector3 startPosition, GameObject endObject, Vector3 endPosition);
    }
}