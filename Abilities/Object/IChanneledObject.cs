using UnityEngine;

namespace AnyRPG {
    public interface IChanneledObject {
        GameObject MyEndObject { get; set; }
        Vector3 MyEndPosition { get; set; }
        GameObject MyStartObject { get; set; }
        Vector3 MyStartPosition { get; set; }
    }
}