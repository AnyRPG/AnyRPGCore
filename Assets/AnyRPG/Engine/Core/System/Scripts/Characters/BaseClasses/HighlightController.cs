using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class HighlightController : MonoBehaviour {

        [Tooltip("A reference to the renderer that contains the material with the highlight circle")]
        [SerializeField]
        private MeshRenderer meshRenderer = null;

        CharacterUnit characterUnit = null;

        private Dictionary<string, ProjectorColorMapNode> colorDictionary = new Dictionary<string, ProjectorColorMapNode>();

        private Dictionary<string, ProjectorColorMapNode> colorOverrideDictionary = new Dictionary<string, ProjectorColorMapNode>();

        private float circleRadius = 0f;

        void Start() {
            SetupController();
            meshRenderer.enabled = false;
        }

        public void SetupController() {

            // make a copy of the material so we can change properties without unity trying to save the original material properties to disk
            /*
            materialCopy = new Material(meshRenderer.material);
            meshRenderer.material = materialCopy;
            */

            foreach (ProjectorColorMapNode colorMapNode in SystemConfigurationManager.MyInstance.FocusProjectorColorMap) {
                colorDictionary[ColorUtility.ToHtmlStringRGBA(colorMapNode.SourceColor)] = colorMapNode;
                //Debug.Log("FocusTargettingController.SetupController(): added " + ColorUtility.ToHtmlStringRGBA(colorMapNode.MySourceColor));
            }
        }

        public void ConfigureOwner(CharacterUnit characterUnit) {
            this.characterUnit = characterUnit;
        }

        public void HandleSetTarget() {
            //Debug.Log("FocusTargettingController.HandleSetTarget()");
            if (characterUnit == null) {
                // don't show this under inanimate units
                HandleClearTarget();
                return;
            }
            meshRenderer.enabled = true;
            colorOverrideDictionary.Clear();
            if (characterUnit.BaseCharacter.CharacterStats.Toughness != null && characterUnit.BaseCharacter.CharacterStats.Toughness.FocusProjectorOverrideMap != null) {
                foreach (ProjectorColorMapNode colorMapNode in characterUnit.BaseCharacter.CharacterStats.Toughness.FocusProjectorOverrideMap) {
                    colorOverrideDictionary[ColorUtility.ToHtmlStringRGBA(colorMapNode.SourceColor)] = colorMapNode;
                    //Debug.Log("FocusTargettingController.SetupController(): added override " + ColorUtility.ToHtmlStringRGBA(colorMapNode.MySourceColor));
                }
            }

            UpdateColors();

            // multiply by 2 to account for circles only being half the width of the plane, and then 2 again
            SetCircleRadius(characterUnit.HitBoxSize * 2f);
        }

        public void HandleClearTarget() {
            //Debug.Log("FocusTargettingController.HandleClearTarget()");
            if (meshRenderer != null) {
                meshRenderer.enabled = false;
            }
        }

        public void SetMaterial(Color materialColor) {
            //Debug.Log("FocusTargettingController.SetMaterial(" + (materialColor == null ? "null" : materialColor.ToString()) + ")");
            if (SystemConfigurationManager.MyInstance == null) {
                return;
            }

            /*
            foreach (string tmpColor in colorDictionary.Keys) {
                Debug.Log("Dictionary contains key: " + tmpColor.ToString());
            }
            */
            if (colorOverrideDictionary.ContainsKey(ColorUtility.ToHtmlStringRGBA(materialColor)) && characterUnit?.BaseCharacter?.CharacterStats?.IsAlive == true) {
                meshRenderer.material = new Material(colorOverrideDictionary[ColorUtility.ToHtmlStringRGBA(materialColor)].ProjectorMaterial);
                ProcessTint(colorOverrideDictionary[ColorUtility.ToHtmlStringRGBA(materialColor)]);
                //Debug.Log("FocusTargettingController.SetMaterial(): override dictionary contained color  " + ColorUtility.ToHtmlStringRGBA(materialColor));
            } else {
                if (colorDictionary.ContainsKey(ColorUtility.ToHtmlStringRGBA(materialColor))) {
                    meshRenderer.material = new Material(colorDictionary[ColorUtility.ToHtmlStringRGBA(materialColor)].ProjectorMaterial);
                    ProcessTint(colorDictionary[ColorUtility.ToHtmlStringRGBA(materialColor)]);
                    //Debug.Log("FocusTargettingController.SetMaterial(): dictionary contained color  " + ColorUtility.ToHtmlStringRGBA(materialColor));
                } else {
                    meshRenderer.material = new Material(SystemConfigurationManager.MyInstance.DefaultCastingLightProjector);
                    //Debug.Log("FocusTargettingController.SetMaterial(): dictionary did not contain color " + ColorUtility.ToHtmlStringRGBA(materialColor) + ", setting to default highlight");
                }
            }

        }

        private void ProcessTint(ProjectorColorMapNode projectorColorMapNode) {
            if (projectorColorMapNode.TintMaterial) {
                meshRenderer.material.color = projectorColorMapNode.SourceColor;
            }
        }

        public void UpdateColors() {
            //Debug.Log("CastTargettingController.FollowMouse()");

            if (meshRenderer.enabled == false) {
                return;
            }

            if (characterUnit?.BaseCharacter?.CharacterStats?.IsAlive == false) {
                SetMaterial(Color.gray);
            } else {
                Color newColor = Faction.GetFactionColor(characterUnit.BaseCharacter.UnitController);
                SetMaterial(newColor);
            }
        }

        public void SetCircleRadius(float newRadius) {
            //Debug.Log("CastTargettingController.SetCircleRadius()");
            circleRadius = newRadius;
            // multiply by 2 because the dimension is the diameter of the plane, not radius
            transform.localScale = new Vector3(circleRadius * 2f, circleRadius * 2f, 1f);
        }

    }

}