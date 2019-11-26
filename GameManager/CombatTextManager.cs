using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CombatTextManager : MonoBehaviour {

        #region Singleton
        private static CombatTextManager instance;

        public static CombatTextManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<CombatTextManager>();
                }

                return instance;
            }
        }

        #endregion

        [SerializeField]
        private GameObject combatTextPrefab;

        [SerializeField]
        private Canvas combatTextCanvas;

        public Canvas MyCombatTextCanvas { get => combatTextCanvas; set => combatTextCanvas = value; }

        public void SpawnCombatText(GameObject target, int damage, CombatTextType combatType, CombatMagnitude combatMagnitude) {
            //Debug.Log("Combat Text manager Spawning Combat Text attached to: " + target.name + "; damage: " + damage + "; type: " + combatType);
            if (PlayerPrefs.GetInt("UseFloatingCombatText") == 0) {
                return;
            }
            GameObject _gameObject = Instantiate(combatTextPrefab, target.transform.position, Quaternion.identity, combatTextCanvas.transform);
            //Debug.Log("About to Set MainTarget on combat text");
            _gameObject.transform.GetComponent<CombatTextController>().MyMainTarget = target;
            if (combatType == CombatTextType.miss) {
                _gameObject.transform.GetComponent<CombatTextController>().MyDisplayText = "(Miss)";
            } else {
                _gameObject.transform.GetComponent<CombatTextController>().MyDisplayText = damage.ToString();
            }
            _gameObject.transform.GetComponent<CombatTextController>().MyCombatMagnitude = combatMagnitude;
            _gameObject.transform.GetComponent<CombatTextController>().MyCombatType = combatType;
        }

        public void SpawnCombatText(GameObject target, StatusEffect statusEffect, bool gainEffect) {
            if (PlayerPrefs.GetInt("UseFloatingCombatText") == 0) {
                return;
            }
            //Debug.Log("Combat Text manager Spawning Combat Text attached to: " + target.name);
            GameObject _gameObject = Instantiate(combatTextPrefab, target.transform.position, Quaternion.identity, combatTextCanvas.transform);
            //Debug.Log("About to Set MainTarget on combat text");
            _gameObject.transform.GetComponent<CombatTextController>().MyMainTarget = target;
            _gameObject.transform.GetComponent<CombatTextController>().MyImage.sprite = statusEffect.MyIcon;
            _gameObject.transform.GetComponent<CombatTextController>().MyDisplayText = statusEffect.MyName;
            if (gainEffect) {
                _gameObject.transform.GetComponent<CombatTextController>().MyCombatType = CombatTextType.gainBuff;
            } else {
                _gameObject.transform.GetComponent<CombatTextController>().MyCombatType = CombatTextType.loseBuff;
            }
        }

    }

}