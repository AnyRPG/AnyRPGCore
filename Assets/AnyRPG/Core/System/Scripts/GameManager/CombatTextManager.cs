using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AnyRPG {
    public class CombatTextManager : ConfiguredMonoBehaviour {

        [SerializeField]
        private GameObject combatTextPrefab;

        [SerializeField]
        private Canvas combatTextCanvas;

        //private List<CombatTextController> combatTextControllers = new List<CombatTextController>();

        private List<CombatTextController> inUseCombatTextControllers = new List<CombatTextController>();
        private List<CombatTextController> returnList = new List<CombatTextController>();

        // game manager references
        private CameraManager cameraManager = null;
        private PlayerManager playerManager = null;
        private ObjectPooler objectPooler = null;
        private CutsceneBarController cutSceneBarController = null;

        public Canvas CombatTextCanvas { get => combatTextCanvas; set => combatTextCanvas = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            //Debug.Log("NamePlateManager.Awake(): " + SystemGameManager.Instance.UIManager.NamePlateManager.gameObject.name);
            SystemEventManager.StartListening("AfterCameraUpdate", HandleAfterCameraUpdate);
            systemEventManager.OnLevelUnloadClient += HandleLevelUnload;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            cameraManager = systemGameManager.CameraManager;
            playerManager = systemGameManager.PlayerManager;
            objectPooler = systemGameManager.ObjectPooler;
            cutSceneBarController = systemGameManager.UIManager.CutSceneBarController;
        }

        public void HandleAfterCameraUpdate(string eventName, EventParamProperties eventParamProperties) {
            UpdateCombatText();
        }

        /*
        public void LateUpdate() {
            if (systemConfigurationManager.UseThirdPartyCameraControl == true
                && cameraManager.ThirdPartyCamera.activeInHierarchy == true
                && playerManager.PlayerUnitSpawned == true) {
                UpdateCombatText();
            }
        }
        */

        public void HandleLevelUnload(int sceneHandle, string sceneName) {
            List<CombatTextController> removeList = new List<CombatTextController>();
            removeList.AddRange(inUseCombatTextControllers);
            foreach (CombatTextController combatTextController in removeList) {
                ReturnControllerToPool(combatTextController);
            }
        }

        private void UpdateCombatText() {
            if (cameraManager.ActiveMainCamera == null) {
                return;
            }
            if (cutSceneBarController.CurrentCutscene != null) {
                return;
            }
            foreach (CombatTextController combatTextController in inUseCombatTextControllers) {
                combatTextController.RunCombatTextUpdate();
            }
            if (returnList.Count > 0) {
                foreach (CombatTextController combatTextController in returnList) {
                    ReturnControllerToPool(combatTextController);
                }
                returnList.Clear();
            }
        }

        public CombatTextController GetCombatTextController() {
            GameObject pooledObject = objectPooler.GetPooledObject(combatTextPrefab, combatTextCanvas.transform);
            if (pooledObject != null) {
                return pooledObject.GetComponent<CombatTextController>();
            }

            return null;
        }

        /// <summary>
        /// wait until the end of the frame and then return the object to the pool to avoid modifying the collection in the foreach loop
        /// </summary>
        /// <param name="combatTextController"></param>
        public void RequestReturnControllerToPool(CombatTextController combatTextController) {
            returnList.Add(combatTextController);
        }

        public void ReturnControllerToPool(CombatTextController combatTextController) {
            inUseCombatTextControllers.Remove(combatTextController);
            objectPooler.ReturnObjectToPool(combatTextController.gameObject);
        }

        public void SpawnCombatText(Interactable target, int damage, CombatTextType combatType, CombatMagnitude combatMagnitude, AbilityEffectContext abilityEffectContext) {
            //Debug.Log($"CombatTextManager.SpawnCombatText({target.gameObject.name}, {damage}, {combatType}, {combatMagnitude})");

            if (PlayerPrefs.GetInt("UseFloatingCombatText") == 0) {
                return;
            }
            CombatTextController combatTextController = GetCombatTextController();
            if (combatTextController != null) {
                combatTextController.Configure(systemGameManager);
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

        public void SpawnCombatText(Interactable target, StatusEffectProperties statusEffect, bool gainEffect) {
            //Debug.Log($"CombatTextManager.SpawnCombatText({target.gameObject.name}, {statusEffect.ResourceName}, {gainEffect})");

            if (PlayerPrefs.GetInt("UseFloatingCombatText") == 0) {
                return;
            }
            //Debug.Log("Combat Text manager Spawning Combat Text attached to: " + target.name);
            //Debug.Log("About to Set MainTarget on combat text");
            CombatTextController combatTextController = GetCombatTextController();
            if (combatTextController != null) {
                combatTextController.Configure(systemGameManager);
                inUseCombatTextControllers.Add(combatTextController);
                combatTextController.InitializeCombatTextController(target,
                    statusEffect.Icon,
                    statusEffect.DisplayName,
                    (gainEffect == true ? CombatTextType.gainBuff : CombatTextType.loseBuff)
                    );
            }
        }

    }

}