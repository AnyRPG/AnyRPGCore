using UnityEngine;

namespace AnyRPG {
    public class SpawnPlayerRequest {
        public string locationTag = string.Empty;
        public bool overrideSpawnLocation = false;
        public Vector3 spawnLocation = Vector3.zero;
        public bool overrideSpawnDirection = false;
        public Vector3 spawnForwardDirection = Vector3.forward;
        public float xOffset = 0f;
        public float zOffset = 0f;

        public SpawnPlayerRequest () {
            CreateRandomOffset();
        }

        public SpawnPlayerRequest (Vector3 position, Vector3 forwardDirection) {
            spawnLocation = position;
            spawnForwardDirection = forwardDirection;
            CreateRandomOffset();
        }

        private void CreateRandomOffset () {
            //Debug.Log($"{gameObject.name}.SpawnPlayerRequest.CreateRandomOffset()");
            xOffset = UnityEngine.Random.Range(-2f, 2f);
            zOffset = UnityEngine.Random.Range(-2f, 2f);
        }
    }
}

