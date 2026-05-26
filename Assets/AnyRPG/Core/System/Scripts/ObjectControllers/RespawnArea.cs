using UnityEngine;

namespace AnyRPG {
    /// <summary>
    /// respawn a player when they touch the collider - used to prevent infinite fall
    /// </summary>
    public class RespawnArea : AutoConfiguredMonoBehaviour {

        /*
        [SerializeField]
        private Collider respawnCollider = null;
        */

        // game manager references
        private PlayerManagerClient playerManagerClient = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            playerManagerClient = systemGameManager.PlayerManagerClient;
        }

        public void OnTriggerEnter(Collider other) {
            //Debug.Log($"{gameObject.name}.MovementSoundArea.OnTriggerEnter()");

            // TO DO : FIX ME this will not work in multiplayer

            if (playerManagerClient.ActiveUnitController == null) {
                return;
            }
            if (other.gameObject == playerManagerClient.ActiveUnitController.gameObject) {
                playerManagerClient.RequestRespawnPlayer();
            }
        }

    }

}
