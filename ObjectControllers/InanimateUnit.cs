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

        [Header("NAMEPLATE SETTINGS")]

        [SerializeField]
        private BaseNamePlateController namePlateController = new BaseNamePlateController();

        [Tooltip("Reference to local component controller prefab with nameplate target, speakers, etc")]
        [SerializeField]
        private UnitComponentController unitComponentController = null;

        private Interactable interactable;

        public INamePlateController NamePlateController { get => namePlateController; }
        public Interactable Interactable { get => interactable; set => interactable = value; }
        public UnitComponentController UnitComponentController { get => unitComponentController; set => unitComponentController = value; }

        public void Start() {
            interactable = GetComponent<Interactable>();
            namePlateController.Setup(this);
        }
    }

}