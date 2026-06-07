using UnityEngine;

namespace AnyRPG {
    /// <summary>
    /// respawn a player when they touch the collider - used to prevent infinite fall
    /// </summary>
    public class RespawnArea : AutoConfiguredMonoBehaviour {

        // game manager references
        private PlayerManagerServer playerManagerServer = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            playerManagerServer = systemGameManager.PlayerManagerServer;
        }

        public void OnTriggerEnter(Collider other) {

            if (playerManagerServer.ActivePlayerGameObjects.ContainsKey(other.gameObject)) {
                playerManagerServer.RespawnPlayerUnit(playerManagerServer.ActivePlayerGameObjects[other.gameObject]);
            }
        }

    }

}
