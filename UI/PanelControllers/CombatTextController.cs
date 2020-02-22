using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CombatTextController : MonoBehaviour {
        [SerializeField]
        private Text text;

        [SerializeField]
        private Image image;

        [SerializeField]
        private float xUIOffset = 50f;

        [SerializeField]
        private float yUnitOffset = 2.2f;

        // pixels per second up or down
        //[SerializeField]
        private float movementSpeed = 1.0f;

        [SerializeField]
        private float fadeTime;

        [SerializeField]
        private int defaultFontSize = 30;

        private string displayText;
        private GameObject mainTarget;
        private float alpha;
        private Vector2 targetPos;
        private float fadeOutTimer;
        private float fadeRate;
        private Color textColor;
        private CombatMagnitude combatMagnitude;
        private CombatTextType textType;

        private float randomXLimit = 100f;
        private float randomYLimit = 100f;
        private float randomX;
        private float randomY;

        // change direction to downward text for hits against player
        private int directionMultiplier = 1;


        public string MyDisplayText { get => displayText; set => displayText = value; }
        public GameObject MyMainTarget { get => mainTarget; set => mainTarget = value; }
        public CombatMagnitude MyCombatMagnitude { get => combatMagnitude; set => combatMagnitude = value; }
        public CombatTextType MyCombatType { get => textType; set => textType = value; }
        public Image MyImage { get => image; set => image = value; }

        /*
        void Start() {
            
        }
        */

        public void InitializeCombatTextController() {
            gameObject.SetActive(true);
            //Debug.Log("Combat Text spawning: " + textType);
            randomX = Random.Range(0, randomXLimit);
            randomY = Random.Range(0, randomYLimit);
            //Debug.Log("Combat Text spawning: " + textType + "; randomX: " + randomX + "; randomY: " + randomY);
            targetPos = Camera.main.WorldToScreenPoint(mainTarget.transform.position);
            //alpha = text.color.a;
            alpha = 1f;
            text.fontSize = defaultFontSize;
            fadeOutTimer = fadeTime;
            fadeRate = 1.0f / fadeTime;

            string preText = string.Empty;
            string postText = string.Empty;

            if (image.sprite == null) {
                image.color = new Color32(0, 0, 0, 0);
            }
            if (mainTarget == PlayerManager.MyInstance.MyPlayerUnitObject) {
                directionMultiplier = -1;
                switch (textType) {
                    case CombatTextType.normal:
                        textColor = Color.red;
                        int parseResult;
                        if (int.TryParse(displayText, out parseResult)) {
                            preText += parseResult > 0 ? "-" : "";
                        }
                        break;
                    case CombatTextType.gainXP:
                        textColor = Color.yellow;
                        preText += "+";
                        postText += " XP";
                        text.fontSize = text.fontSize * 2;
                        break;
                    case CombatTextType.gainBuff:
                        textColor = Color.cyan;
                        preText += "+";
                        //text.fontSize = text.fontSize * 2;
                        break;
                    case CombatTextType.loseBuff:
                        textColor = Color.cyan;
                        preText += "+";
                        //text.fontSize = text.fontSize * 2;
                        break;
                    case CombatTextType.ability:
                        textColor = Color.magenta;
                        preText += "-";
                        text.fontSize = text.fontSize * 2;
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
                        text.fontSize = text.fontSize * 2;
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
                    text.fontSize = text.fontSize * 2;
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
                case CombatTextType.gainMana:
                    textColor = Color.blue;
                    preText += "+";
                    text.fontSize = text.fontSize * 2;
                    break;
                default:
                    break;
            }

            text.color = textColor;
            string finalString = preText + displayText + postText;
            text.text = finalString;
            if (MyCombatMagnitude == CombatMagnitude.critical) {
                text.fontSize = text.fontSize * 2;
            }
            RunCombatTextUpdate();
        }

        public void RunCombatTextUpdate() {
            //Debug.Log("CombatTextController.FixedUpdate()");
            if (mainTarget != null) {
                //Debug.Log("CombatTextController.FixedUpdate(): maintarget is not null");
                targetPos = Camera.main.WorldToScreenPoint(mainTarget.transform.position + new Vector3(0, yUnitOffset, 0));
                //Debug.Log("CombatTextController.FixedUpdate(): targetpos:" + targetPos);
                transform.position = targetPos + new Vector2(randomX + xUIOffset, randomY);
            } else {
                //Debug.Log("CombatTextController.FixedUpdate(): maintarget is null");
            }
            if (fadeOutTimer > 0) {
                fadeOutTimer -= Time.fixedDeltaTime;

                alpha -= fadeRate * Time.fixedDeltaTime;

                // fade text
                Color tmp = text.color;
                tmp.a = alpha;
                text.color = tmp;

                // fade image
                if (image.sprite != null) {
                    Color imageColor = image.color;
                    imageColor.a = alpha;
                    image.color = imageColor;
                }

                randomY += (movementSpeed * directionMultiplier);
            } else {
                CombatTextManager.MyInstance.returnControllerToPool(this);
                //Destroy(this.gameObject);
            }

        }

        void FixedUpdate() {
            RunCombatTextUpdate();
        }
    }

}