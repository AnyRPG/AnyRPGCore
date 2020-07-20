using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace AnyRPG {
    public class NamePlateUnit : MonoBehaviour, INamePlateTarget  {


        [Header("NAMEPLATE SETTINGS")]

        [Tooltip("Set a transform to override the default nameplate placement above the interactable.  Useful for interactables that are not 2m tall.")]
        [SerializeField]
        private Transform namePlateTransform = null;

        public Transform NamePlateTransform { get => namePlateTransform; }
    }

}