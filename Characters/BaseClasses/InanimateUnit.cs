using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace AnyRPG {
    public class InanimateUnit : InteractableOption, INamePlateUnit {

        public event System.Action OnInitializeNamePlate = delegate { };
        public event Action<INamePlateUnit> NamePlateNeedsRemoval = delegate { };
        public event Action<int, int> HealthBarNeedsUpdate = delegate { };

        public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        [SerializeField]
        private string displayName = string.Empty;

        private NamePlateController namePlate;

        [SerializeField]
        private string unitFrameTarget = string.Empty;

        [SerializeField]
        private Vector3 unitFrameCameraLookOffset = Vector3.zero;

        [SerializeField]
        private Vector3 unitFrameCameraPositionOffset = Vector3.zero;

        [SerializeField]
        private Transform namePlateTransform = null;

        public NamePlateController MyNamePlate { get => namePlate; set => namePlate = value; }
        public string MyDisplayName { get => displayName; }
        public Faction MyFaction { get => null; }
        public string MyUnitFrameTarget { get => unitFrameTarget; }
        public Vector3 MyUnitFrameCameraLookOffset { get => unitFrameCameraLookOffset; set => unitFrameCameraLookOffset = value; }
        public Vector3 MyUnitFrameCameraPositionOffset { get => unitFrameCameraPositionOffset; set => unitFrameCameraPositionOffset = value; }
        public Transform MyNamePlateTransform {
            get {
                if (namePlateTransform != null) {
                    return namePlateTransform;
                }
                return transform;
            }
        }

        public bool HasHealth() {
            return false;
        }

        public int CurrentHealth() {
            return 1;
        }

        public int MaxHealth() {
            return 1;
        }

        private void OnEnable() {
            //Debug.Log(gameObject.name + ": running OnEnable()");
            //InitializeNamePlate();
        }

        public override void OnDisable() {
            if (NamePlateManager.MyInstance != null) {
                NamePlateManager.MyInstance.RemoveNamePlate(this as INamePlateUnit);
            }
        }

        protected override void Awake() {
            //Debug.Log(gameObject.name + ": Awake() about to get references to all local components");
            base.Awake();
        }

        protected override void Start() {
            //Debug.Log(gameObject.name + ".InanimateUnit.Start()");
            base.Start();
            InitializeNamePlate();
        }

        /*
        public override void CleanupEventSubscriptions() {
            //Debug.Log(gameObject.name + ".InanimateUnit.CleanupEventSubscriptions()");
            base.CleanupEventSubscriptions();
        }
        */

        public void InitializeNamePlate() {
            //Debug.Log(gameObject.name + ".InanimateUnit.InitializeNamePlate()");

            if (interactable.CanInteract()) {
                //Debug.Log(gameObject.name + ".InanimateUnit.InitializeNamePlate(): isStarted && interactable.CanInteract() == true");
                NamePlateController _namePlate = NamePlateManager.MyInstance.AddNamePlate(this, (namePlateTransform == null ? true : false));
                if (_namePlate != null) {
                    namePlate = _namePlate;
                }
                OnInitializeNamePlate();
            } else {
                //Debug.Log(gameObject.name + ".InanimateUnit.InitializeNamePlate(): isStarted && interactable.CanInteract() == false");
                return;
            }
        }

        public override bool HasMiniMapText() {
            return true;
        }

        public override bool SetMiniMapText(TextMeshProUGUI text) {
            //Debug.Log(gameObject.name + ".InanimateUnit.SetMiniMapText()");
            text.text = "";
            text.color = new Color32(0, 0, 0, 0);
            //text.fontSize = 50;
            //text.color = Faction.GetFactionColor(baseCharacter.MyFaction);
            return true;
        }

        public override bool CanInteract() {
            return false;
        }

        public override bool Interact(CharacterUnit source) {
            return false;
        }

        public override void StopInteract() {
            base.StopInteract();
        }

        public void OnDestroy() {
            CleanupEventSubscriptions();
        }

        public override void HandlePrerequisiteUpdates() {
            //Debug.Log(gameObject.name + ".InanimateUnit.HandlePrerequisiteUpdates()");
            base.HandlePrerequisiteUpdates();
            MiniMapStatusUpdateHandler(this);
            InitializeNamePlate();
        }

        public override void HandlePlayerUnitSpawn() {
            //Debug.Log(gameObject.name + ".InanimateUnit.HandlePlayerUnitSpawn()");
            base.HandlePlayerUnitSpawn();
            MiniMapStatusUpdateHandler(this);
            InitializeNamePlate();
        }
    }

}