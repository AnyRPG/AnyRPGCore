using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace AnyRPG {
    public class InanimateUnit : MonoBehaviour, INamePlateUnit {

        [SerializeField]
        private NamePlateProps namePlateProps = new NamePlateProps();

        private BaseNamePlateController namePlateController = null;

        [Tooltip("Reference to local component controller prefab with nameplate target, speakers, etc")]
        [SerializeField]
        private UnitComponentController unitComponentController = null;

        private Interactable interactable;

        public INamePlateController NamePlateController { get => namePlateController; }
        public Interactable Interactable { get => interactable; set => interactable = value; }
        public UnitComponentController UnitComponentController { get => unitComponentController; set => unitComponentController = value; }
        public NamePlateProps NamePlateProps { get => namePlateProps; set => namePlateProps = value; }

        public void Awake() {
            interactable = GetComponent<Interactable>();
            namePlateController = new BaseNamePlateController(this);
            namePlateController.Init();

            if (interactable != null) {
                interactable.Initialize();
            }
        }

        public void OnDestroy() {
            Debug.Log(gameObject.name + ".InanimateUnit.OnDestroy()");
        }
    }

}