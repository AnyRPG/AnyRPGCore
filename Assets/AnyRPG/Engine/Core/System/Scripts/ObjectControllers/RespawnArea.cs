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
        private PlayerManager playerManager = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            playerManager = systemGameManager.PlayerManager;
        }

        public void OnTriggerEnter(Collider other) {
            //Debug.Log(gameObject.name + ".MovementSoundArea.OnTriggerEnter()");
            if (other.gameObject == playerManager.ActiveUnitController.gameObject) {
                playerManager.RespawnPlayer();
            }
        }

    }

}
