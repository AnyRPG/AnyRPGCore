using AnyRPG;
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
        private PlayerManagerClient playerManager = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            playerManager = systemGameManager.PlayerManager;
        }

        public void OnTriggerEnter(Collider other) {
            //Debug.Log($"{gameObject.name}.MovementSoundArea.OnTriggerEnter()");

            // TO DO : FIX ME this will not work in multiplayer

            if (playerManager.ActiveUnitController == null) {
                return;
            }
            if (other.gameObject == playerManager.ActiveUnitController.gameObject) {
                playerManager.RequestRespawnPlayer();
            }
        }

    }

}
