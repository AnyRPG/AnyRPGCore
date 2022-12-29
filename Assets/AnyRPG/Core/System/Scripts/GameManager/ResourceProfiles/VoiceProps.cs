using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class VoiceProps {

        [Header("Character")]

        [Tooltip("List of audio clips to choose from when an NPC that is not in combat aggros other characters.")]
        [SerializeField]
        private List<AudioClip> aggro = new List<AudioClip>();

        [Tooltip("List of audio clips to choose from when character performs an attack on other characters.")]
        [SerializeField]
        private List<AudioClip> attack = new List<AudioClip>();

        [Tooltip("List of audio clips to choose from when character takes damage.")]
        [SerializeField]
        private List<AudioClip> damage = new List<AudioClip>();

        [Tooltip("List of audio clips to choose from when character jumps.")]
        [SerializeField]
        private List<AudioClip> jump = new List<AudioClip>();

        [Tooltip("List of audio clips to choose from when character takes fall damage.")]
        [SerializeField]
        private List<AudioClip> fallDamage = new List<AudioClip>();

        [Tooltip("List of audio clips to choose from when character dies.")]
        [SerializeField]
        private List<AudioClip> death = new List<AudioClip>();

        [Tooltip("List of audio clips to choose from when the character kills another character.")]
        [SerializeField]
        private List<AudioClip> victory = new List<AudioClip>();

        [Header("Interactions")]

        [Tooltip("List of audio clips to choose from when the character starts interacting.")]
        [SerializeField]
        private List<AudioClip> startInteract = new List<AudioClip>();

        [Tooltip("List of audio clips to choose from when the character starts interacting with a vendor.")]
        [SerializeField]
        private List<AudioClip> startVendorInteract = new List<AudioClip>();

        /*
        [Tooltip("List of audio clips to choose from when the character makes a purchase.")]
        [SerializeField]
        private List<AudioClip> purchase = new List<AudioClip>();
        */

        [Tooltip("List of audio clips to choose from when the character stops interacting (closes an interaction window).")]
        [SerializeField]
        private List<AudioClip> stopInteract = new List<AudioClip>();

        [Tooltip("List of audio clips to choose from when the character stops interacting with a vendor (closes the vendor window).")]
        [SerializeField]
        private List<AudioClip> stopVendorInteract = new List<AudioClip>();


        public AudioClip RandomAggro {
            get {
                if (aggro.Count > 0) {
                    return aggro[Random.Range(0, aggro.Count)];
                }
                return null;
            }
        }

        public AudioClip RandomAttack {
            get {
                if (attack.Count > 0) {
                    return attack[Random.Range(0, attack.Count)];
                }
                return null;
            }
        }

        public AudioClip RandomDamage {
            get {
                if (damage.Count > 0) {
                    return damage[Random.Range(0, damage.Count)];
                }
                return null;
            }
        }

        public AudioClip RandomJump {
            get {
                if (jump.Count > 0) {
                    return jump[Random.Range(0, jump.Count)];
                }
                return null;
            }
        }

        public AudioClip RandomFallDamage {
            get {
                if (fallDamage.Count > 0) {
                    return fallDamage[Random.Range(0, fallDamage.Count)];
                }
                return null;
            }
        }

        public AudioClip RandomDeath {
            get {
                if (death.Count > 0) {
                    return death[Random.Range(0, death.Count)];
                }
                return null;
            }
        }

        public AudioClip RandomVictory {
            get {
                if (victory.Count > 0) {
                    return victory[Random.Range(0, victory.Count)];
                }
                return null;
            }
        }

        public AudioClip RandomStartInteract {
            get {
                if (startInteract.Count > 0) {
                    return startInteract[Random.Range(0, startInteract.Count)];
                }
                return null;
            }
        }

        public AudioClip RandomStartVendorInteract {
            get {
                if (startVendorInteract.Count > 0) {
                    return startVendorInteract[Random.Range(0, startVendorInteract.Count)];
                }
                return null;
            }
        }

        /*
        public AudioClip RandomPurchase {
            get {
                if (purchase.Count > 0) {
                    return purchase[Random.Range(0, purchase.Count)];
                }
                return null;
            }
        }
        */

        public AudioClip RandomStopInteract {
            get {
                if (stopInteract.Count > 0) {
                    return stopInteract[Random.Range(0, stopInteract.Count)];
                }
                return null;
            }
        }

        public AudioClip RandomStopVendorInteract {
            get {
                if (stopVendorInteract.Count > 0) {
                    return stopVendorInteract[Random.Range(0, stopVendorInteract.Count)];
                }
                return null;
            }
        }


    }

}