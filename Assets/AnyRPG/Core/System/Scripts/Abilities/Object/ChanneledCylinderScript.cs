﻿using AnyRPG;
using UnityEngine;
using System.Collections.Generic;

namespace AnyRPG {

    public class ChanneledCylinderScript : ConfiguredMonoBehaviour, IChanneledObject {

        private float xRadius = 0.1f;
        private float zRadius = 0.1f;

        // keep track if this has been set at least once
        //private bool objectInitialized = false;

        [Tooltip("The game object where the object will emit from. If null, StartPosition is used.")]
        [SerializeField]
        private GameObject startObject;

        [Tooltip("The start position where the object will emit from. This is in world space if StartObject is null, otherwise this is offset from StartObject position.")]
        [SerializeField]
        private Vector3 startPosition;

        [Tooltip("The game object where the object will end at. If null, EndPosition is used.")]
        [SerializeField]
        private GameObject endObject;

        [Tooltip("The end position where the object will end at. This is in world space if EndObject is null, otherwise this is offset from EndObject position.")]
        [SerializeField]
        private Vector3 endPosition;

        // keep track of whether the end object was originally null
        private bool nullEndObject = false;

        //private Vector3 lastStartPosition = Vector3.zero;
        //private Vector3 lastEndPosition = Vector3.zero;

        // game manager references
        private ObjectPooler objectPooler = null;

        public GameObject StartObject { get => startObject;
            set {
                startObject = value;
                //Debug.Log("start Object : " + startObject.name);
            }
        }
        public Vector3 StartPosition { get => startPosition; set => startPosition = value; }
        public GameObject EndObject { get => endObject;
            set {
                endObject = value;
                //Debug.Log("end Object : " + endObject.name);
            }
        }
        public Vector3 EndPosition { get => endPosition; set => endPosition = value; }

        public void Setup(GameObject startObject, Vector3 startPosition, GameObject endObject, Vector3 endPosition, SystemGameManager systemGameManager) {
            //Debug.Log(gameObject.name + ".ChanneledCylinderScript.Setup(" + (startObject == null ? "null" : startObject.name) + ", " + startPosition + ", " + (endObject == null ? "null" : endObject.name) + ", " + endPosition + ")");
            Configure(systemGameManager);
            StartObject = startObject;
            StartPosition = startPosition;
            EndObject = endObject;
            if (endObject == null) {
                nullEndObject = true;
            }
            EndPosition = endPosition;
            UpdateTransform();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            objectPooler = systemGameManager.ObjectPooler;
        }

        private void Update() {

            if (StartObject == null || (nullEndObject == false && (endObject == null || endObject.activeInHierarchy == false))) {
                // need to be able to shoot at ground, but should still exit if we had an actual original target
                objectPooler.ReturnObjectToPool(gameObject);
                return;
            }
            UpdateTransform();
            /*
            if (objectInitialized == false || lastStartPosition != MyStartObject.transform.position || lastEndPosition != MyEndObject.transform.position) {
                //UpdateTransform();
            }
            */
            //lastStartPosition = MyStartObject.transform.position;
            //lastEndPosition = MyEndObject.transform.position;
        }

        private void UpdateTransform() {
            if (StartObject == null || (nullEndObject == false && (endObject == null || endObject.activeInHierarchy == false))) {
                // need to be able to shoot at ground, but should still exit if we had an actual original target
                objectPooler.ReturnObjectToPool(gameObject);
                return;
            }
            Vector3 absoluteStartPosition = StartObject.transform.TransformPoint(StartPosition);
            Vector3 absoluteEndPosition = Vector3.zero;
            if (EndObject == null) {
                absoluteEndPosition = EndPosition;
            } else {
                absoluteEndPosition = EndObject.transform.TransformPoint(EndPosition);
            }

            Vector3 directionVector = (absoluteStartPosition - absoluteEndPosition).normalized;
            //Vector3 midPoint = new Vector3((absoluteStartPosition.x - absoluteEndPosition.x) / 2f, (absoluteStartPosition.y - absoluteEndPosition.y) / 2f, (absoluteStartPosition.z - absoluteEndPosition.z) / 2f);
            Vector3 midPoint = new Vector3((absoluteStartPosition.x + absoluteEndPosition.x) / 2f, (absoluteStartPosition.y + absoluteEndPosition.y) / 2f, (absoluteStartPosition.z + absoluteEndPosition.z) / 2f);

            float distanceMagnitude = Vector3.Distance(absoluteStartPosition, absoluteEndPosition);

            Vector3 updatedLocalScale = new Vector3(xRadius, distanceMagnitude / 2f, zRadius);

            transform.position = midPoint;
            transform.localScale = updatedLocalScale;
            //transform.rotation = Quaternion.LookRotation(directionVector);
            transform.forward = directionVector;
            transform.Rotate(90, 0, 0);

            //objectInitialized = true;
        }
    }
}