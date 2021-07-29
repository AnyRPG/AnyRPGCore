using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CombatTextController : MonoBehaviour {
        //[SerializeField]
        //private TextMeshProUGUI tmpProtext = null;

        [SerializeField]
        private TextMeshProUGUI tmpProtext = null;

        [SerializeField]
        private Image image = null;

        [SerializeField]
        private RectTransform rectTransform = null;

        [SerializeField]
        private RectTransform textRectTransform = null;

        [Tooltip("Text will always be placed this many pixels to the left or right of the target center")]
        [SerializeField]
        private float xUIOffset = 25f;

        // keep track of current y scrolling position
        private float yUIOffset = 0f;

        [Tooltip("Distance (in game units) above the target pivot to place the text")]
        [SerializeField]
        private float yUnitOffset = 2.2f;

        [Tooltip("pixels to move over the total fade time")]
        [SerializeField]
        private float distanceToMove = 200f;

        [Tooltip("Text will fade and disappear over this much time")]
        [SerializeField]
        private float fadeTime = 3f;

        [SerializeField]
        private int defaultFontSize = 30;

        [SerializeField]
        private float randomXLimit = 100f;

        [SerializeField]
        private float randomYLimit = 25f;

        private string displayText = string.Empty;
        private Interactable mainTarget = null;
        private float alpha;
        private Vector2 targetPos;
        private float fadeOutTimer;
        private float fadeRate;
        private Color textColor;
        private CombatMagnitude combatMagnitude;
        private CombatTextType textType;
        private AbilityEffectContext abilityEffectContext = null;

        private float randomX;
        private float randomY;

        // change direction to downward text for hits against player
        private int directionMultiplier = 1;

        // make text bigger for crits and non white damage
        private int fontSizeMultiplier = 1;

        // for players, make negative amounts go on the opposite side
        private int xDirectionMultiplier = 1;

        // reduce use of local variables for garbage collection
        string preText = string.Empty;
        string postText = string.Empty;

        public RectTransform RectTransform { get => rectTransform; set => rectTransform = value; }

        public void InitializeCombatTextController(Interactable mainTarget, Sprite sprite, string displayText, CombatTextType combatTextType, CombatMagnitude combatMagnitude = CombatMagnitude.normal, AbilityEffectContext abilityEffectContext = null) {
            this.mainTarget = mainTarget;
            image.sprite = sprite;
            this.displayText = displayText;
            this.textType = combatTextType;
            this.combatMagnitude = combatMagnitude;
            this.abilityEffectContext = abilityEffectContext;

            gameObject.SetActive(true);

            // if the combat text ui is not active, then we should just immediately disable this
            if (gameObject.activeInHierarchy == false) {
                SystemGameManager.Instance.UIManager.CombatTextManager.returnControllerToPool(this);
                return;
            }

            //Debug.Log("Combat Text spawning: " + textType);
            randomX = Random.Range(0, randomXLimit);
            randomY = Random.Range(0, randomYLimit);
            //Debug.Log("Combat Text spawning: " + textType + "; randomX: " + randomX + "; randomY: " + randomY);
            targetPos = SystemGameManager.Instance.CameraManager.ActiveMainCamera.WorldToScreenPoint(mainTarget.InteractableGameObject.transform.position);
            //alpha = text.color.a;
            alpha = 1f;
            fadeOutTimer = fadeTime;
            fadeRate = 1.0f / fadeTime;
            directionMultiplier = 1;
            fontSizeMultiplier = 1;
            yUIOffset = 0f;

            preText = string.Empty;
            postText = string.Empty;

            if (image.sprite == null) {
                image.color = new Color32(0, 0, 0, 0);
            } else {
                image.color = Color.white;
            }
            if (mainTarget.InteractableGameObject == SystemGameManager.Instance.PlayerManager.ActiveUnitController.gameObject) {
                directionMultiplier = -1;
                switch (textType) {
                    case CombatTextType.normal:
                        textColor = Color.red;
                        int parseResult;
                        if (int.TryParse(displayText, out parseResult)) {
                            preText += parseResult > 0 ? "-" : "";
                            xDirectionMultiplier = parseResult > 0 ? 1 : -1;
                        }
                        break;
                    case CombatTextType.gainXP:
                        textColor = Color.yellow;
                        preText += "+";
                        postText += " XP";
                        fontSizeMultiplier *= 2;
                        xDirectionMultiplier = -1;
                        break;
                    case CombatTextType.gainBuff:
                        textColor = Color.cyan;
                        preText += "+";
                        //text.fontSize = text.fontSize * 2;
                        xDirectionMultiplier = -1;
                        break;
                    case CombatTextType.loseBuff:
                        textColor = Color.cyan;
                        preText += "+";
                        //text.fontSize = text.fontSize * 2;
                        xDirectionMultiplier = -1;
                        break;
                    case CombatTextType.ability:
                        textColor = Color.magenta;
                        preText += "-";
                        fontSizeMultiplier *= 2;
                        break;
                    case CombatTextType.gainHealth:
                        xDirectionMultiplier = -1;
                        break;
                    case CombatTextType.gainResource:
                        xDirectionMultiplier = -1;
                        break;
                    default:
                        break;
                }
            } else {
                switch (textType) {
                    case CombatTextType.normal:
                        textColor = Color.white;
                        break;
                    case CombatTextType.ability:
                        textColor = Color.yellow;
                        fontSizeMultiplier *= 2;
                        break;
                    default:
                        break;
                }
            }
            // defaults
            switch (textType) {
                case CombatTextType.gainHealth:
                    textColor = Color.green;
                    preText += "+";
                    fontSizeMultiplier *= 2;
                    break;
                case CombatTextType.miss:
                    textColor = Color.white;
                    //preText += "";
                    //text.fontSize = text.fontSize * 2;
                    break;
                case CombatTextType.immune:
                    textColor = Color.white;
                    //preText += "";
                    //text.fontSize = text.fontSize * 2;
                    break;
                case CombatTextType.gainResource:
                    if (abilityEffectContext?.powerResource != null) {
                        textColor = abilityEffectContext.powerResource.DisplayColor;
                        postText += " " + abilityEffectContext.powerResource.DisplayName;
                    } else {
                        textColor = Color.blue;
                    }
                    preText += "+";
                    // rage looks funny bigger
                    //tmpProtext.fontSize = tmpProtext.fontSize * 2;
                    break;
                default:
                    break;
            }

            tmpProtext.color = textColor;
            tmpProtext.text = preText + displayText + postText;
            if (combatMagnitude == CombatMagnitude.critical) {
                fontSizeMultiplier *= 2;
            }
            tmpProtext.fontSize = defaultFontSize * fontSizeMultiplier;

            // make criticals and other large text go farther up and to the right to avoid covering smaller text
            randomY += (fontSizeMultiplier / 2f) * randomYLimit;
            randomX += (fontSizeMultiplier / 2f) * randomXLimit;

            // before running the first combat text update, the layout should be updated, or the rect for the text will still have the old width before the end of the current frame
            LayoutRebuilder.ForceRebuildLayoutImmediate(textRectTransform);
            RunCombatTextUpdate();
        }

        public void RunCombatTextUpdate() {
            //Debug.Log("CombatTextController.RunCombatTextUpdate() fadeOutTimer: " + fadeOutTimer + " " + tmpProtext.text);
            if (mainTarget != null) {
                //Debug.Log("CombatTextController.FixedUpdate(): maintarget is not null");
                targetPos = SystemGameManager.Instance.CameraManager.ActiveMainCamera.WorldToScreenPoint(mainTarget.InteractableGameObject.transform.position + new Vector3(0, yUnitOffset, 0));
                //Debug.Log("CombatTextController.FixedUpdate(): targetpos:" + targetPos);
                transform.position = targetPos + new Vector2((randomX + xUIOffset + (xDirectionMultiplier == 1 ? 0 : textRectTransform.rect.width)) * xDirectionMultiplier, yUIOffset + randomY);
            }
            if (fadeOutTimer > 0f) {
                fadeOutTimer -= Time.deltaTime;

                alpha -= fadeRate * Time.deltaTime;

                // fade text
                Color tmp = tmpProtext.color;
                tmp.a = alpha;
                tmpProtext.color = tmp;

                // fade image
                if (image.sprite != null) {
                    Color imageColor = image.color;
                    imageColor.a = alpha;
                    image.color = imageColor;
                }

                //randomY += (movementSpeed * directionMultiplier);
                yUIOffset = distanceToMove * (((fadeOutTimer - fadeTime) * -1) / fadeTime) * directionMultiplier;
            } else {
                SystemGameManager.Instance.UIManager.CombatTextManager.returnControllerToPool(this);
            }

        }

    }

}