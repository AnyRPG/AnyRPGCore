using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AnyRPG {
    public class CombatTextManager : MonoBehaviour {

        [SerializeField]
        private GameObject combatTextPrefab;

        [SerializeField]
        private Canvas combatTextCanvas;

        //private List<CombatTextController> combatTextControllers = new List<CombatTextController>();

        private List<CombatTextController> inUseCombatTextControllers = new List<CombatTextController>();

        public Canvas MyCombatTextCanvas { get => combatTextCanvas; set => combatTextCanvas = value; }

        public void Init() {
            //Debug.Log("NamePlateManager.Awake(): " + SystemGameManager.Instance.UIManager.NamePlateManager.gameObject.name);
            SystemEventManager.StartListening("AfterCameraUpdate", HandleAfterCameraUpdate);
            SystemEventManager.StartListening("OnLevelUnload", HandleLevelUnload);
        }

        public void HandleAfterCameraUpdate(string eventName, EventParamProperties eventParamProperties) {
            UpdateCombatText();
        }

        public void LateUpdate() {
            if (SystemConfigurationManager.Instance.UseThirdPartyCameraControl == true
                && SystemGameManager.Instance.CameraManager.ThirdPartyCamera.activeInHierarchy == true
                && PlayerManager.Instance.PlayerUnitSpawned == true) {
                UpdateCombatText();
            }
        }

        public void HandleLevelUnload(string eventName, EventParamProperties eventParamProperties) {
            List<CombatTextController> removeList = new List<CombatTextController>();
            removeList.AddRange(inUseCombatTextControllers);
            foreach (CombatTextController combatTextController in removeList) {
                returnControllerToPool(combatTextController);
            }
        }

        private void UpdateCombatText() {
            if (SystemGameManager.Instance.CameraManager?.ActiveMainCamera == null) {
                return;
            }
            if (SystemGameManager.Instance.UIManager.CutSceneBarController.CurrentCutscene != null) {
                return;
            }
            foreach (CombatTextController combatTextController in inUseCombatTextControllers) {
                combatTextController.RunCombatTextUpdate();
            }
        }

        public CombatTextController GetCombatTextController() {
            GameObject pooledObject = ObjectPooler.Instance.GetPooledObject(combatTextPrefab, combatTextCanvas.transform);
            if (pooledObject != null) {
                return pooledObject.GetComponent<CombatTextController>();
            }

            return null;
        }

        /// <summary>
        /// wait until the end of the frame and then return the object to the pool to avoid modifying the collection in the foreach loop
        /// </summary>
        /// <param name="combatTextController"></param>
        public void returnControllerToPool(CombatTextController combatTextController) {
            StartCoroutine(ReturnAtEndOfFrame(combatTextController));
        }

        public IEnumerator ReturnAtEndOfFrame(CombatTextController combatTextController) {
            yield return new WaitForEndOfFrame();
            inUseCombatTextControllers.Remove(combatTextController);
            ObjectPooler.Instance.ReturnObjectToPool(combatTextController.gameObject);

        }

        public void SpawnCombatText(Interactable target, int damage, CombatTextType combatType, CombatMagnitude combatMagnitude, AbilityEffectContext abilityEffectContext) {
            //Debug.Log("CombatTextManager.SpawnCombatText(" + target.name + ", " + damage + ", " + combatType + ", " + combatMagnitude + ")");
            if (PlayerPrefs.GetInt("UseFloatingCombatText") == 0) {
                return;
            }
            CombatTextController combatTextController = GetCombatTextController();
            if (combatTextController != null) {
                //Debug.Log("About to Set MainTarget on combat text");
                inUseCombatTextControllers.Add(combatTextController);
                /*
                Rect rectTransform = combatTextController.RectTransform.rect;
                rectTransform.width = 0;
                rectTransform.height = 0;
                */
                combatTextController.RectTransform.rect.Set(combatTextController.RectTransform.rect.x, combatTextController.RectTransform.rect.y, 0, 0);
                combatTextController.InitializeCombatTextController(target,
                    null,
                    GetDisplayText(combatType, damage),
                    combatType,
                    combatMagnitude,
                    abilityEffectContext
                    );
            }
        }

        private string GetDisplayText(CombatTextType combatType, int damage) {
            if (combatType == CombatTextType.miss) {
                return "(Miss)";
            } else if (combatType == CombatTextType.immune) {
                return "(Immune)";
            } else {
                return damage.ToString();
            }
        }

        public void SpawnCombatText(Interactable target, StatusEffect statusEffect, bool gainEffect) {
            if (PlayerPrefs.GetInt("UseFloatingCombatText") == 0) {
                return;
            }
            //Debug.Log("Combat Text manager Spawning Combat Text attached to: " + target.name);
            //Debug.Log("About to Set MainTarget on combat text");
            CombatTextController combatTextController = GetCombatTextController();
            if (combatTextController != null) {
                inUseCombatTextControllers.Add(combatTextController);
                combatTextController.InitializeCombatTextController(target,
                    statusEffect.Icon,
                    statusEffect.DisplayName,
                    (gainEffect == true ? CombatTextType.gainBuff : CombatTextType.loseBuff)
                    );
            }
        }

        public void CleanupEventSubscriptions() {
            SystemEventManager.StopListening("AfterCameraUpdate", HandleAfterCameraUpdate);
            SystemEventManager.StopListening("OnLevelUnload", HandleLevelUnload);

        }

        public void OnDestroy() {
            CleanupEventSubscriptions();
        }


    }

}