using System;
using UnityEngine.SceneManagement;

namespace AnyRPG {

    public class SceneData {
        public SceneInstanceType SceneInstanceType = SceneInstanceType.World;
        public Scene Scene;
        public SceneNode SceneNode = null;
        public bool HasNavMesh = false;

        // the time that this scene became empty of players
        // used for tracking instance unloading timeouts
        public DateTime EmptyTime = DateTime.MinValue;

        public int ClientCount = 0;
        
        public SceneData(SceneInstanceType sceneInstanceType, Scene scene, SceneNode sceneNode, bool hasNavMesh) {
            SceneInstanceType = sceneInstanceType;
            Scene = scene;
            EmptyTime = DateTime.Now;
            SceneNode = sceneNode;
            HasNavMesh = hasNavMesh;
        }
    }

}
