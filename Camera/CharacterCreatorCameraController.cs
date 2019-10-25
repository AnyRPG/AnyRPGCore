using AnyRPG;
﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
//Added to detect whether the pointer is over a UI element...
using UnityEngine.EventSystems;
using UMA;

namespace AnyRPG {
//DOS MODIFIED really we want this to 'look at' the target bone but move with the Global one
//not sure how to do that though...
//TODO if the user changes the size of the head to be really big we end up inside. We need this to calculate its minimum distance based on the size of thecapsule collider (assuming this is correct
public class MouseOrbitImproved2 : MonoBehaviour
    {

        //DOS Modified added an option to choose which mose button to use (for touch you dont have a right button option)
        public enum mouseBtnOpts { Left = 0, Right = 1, Middle = 2 }
        public mouseBtnOpts mouseButtonToUse = mouseBtnOpts.Right;

        public Transform target;
        public float distance = 5.0f;
        public float xSpeed = 120.0f;
        public float ySpeed = 120.0f;
        public float scrollrate = 3.0f;
        public float yMinLimit = -20f;
        public float yMaxLimit = 80f;

        public float distanceMin = .5f;
        public float distanceMax = 15f;
        public Vector3 Offset;
        public bool AlwaysOn = false;

        [Tooltip("use this to enable the user to orbit the camera around the character on touchscreen devices")]
        public bool singleTouchOrbiting = true;
        [Tooltip("use this to enable the user to pinch to zoom the camera on touchscreen devices")]
        public bool pinchToZoom = true;


        public bool Clip;
        public enum targetOpts {  Head, Chest, Spine, Hips, LeftFoot, LeftHand, LeftLowerArm, LeftLowerLeg, LeftShoulder, LeftUpperArm, LeftUpperLeg, RightFoot, RightHand, RightLowerArm, RightLowerLeg, RightShoulder, RightUpperArm, RightUpperLeg }
        public targetOpts TargetBone;

        private string[] targetStrings = { "Head", "Chest", "Spine", "Hips", "LeftFoot", "LeftHand", "LeftLowerArm", "LeftLowerLeg", "LeftShoulder", "LeftUpperArm", "LeftUpperLeg", "RightFoot", "RightHand", "RightLowerArm", "RightLowerLeg", "RightShoulder", "RightUpperArm", "RightUpperLeg" };
        private UMAData umaData;
        private Rigidbody _rigidbody;
        private GameObject TargetGO;

        bool switchingTarget = false;
        float smoothing = 7f;


        float defaultx, defaulty, defaultdistance;
        float x = 0.0f;
        float y = 0.0f;

        class TempTransform {
            public Vector3 position;
            public Quaternion rotation;
        }

        // Use this for initialization
        void Start()
        {
            Vector3 angles = transform.eulerAngles;
            x = angles.y;
            y = angles.x;
            defaultx = x;
            defaulty = y;
            defaultdistance = distance;

            _rigidbody = GetComponent<Rigidbody>();

            // Make the rigid body not change rotation
            if (_rigidbody != null)
            {
                _rigidbody.freezeRotation = true;
            }
        }

        public void Reset()
        {
            x = defaultx;
            y = defaulty;
            distance = defaultdistance;
        }

        public void SwitchTarget(Transform _dstTarget)
        {
            StopAllCoroutines();
            target = null;
            switchingTarget = true;
            StartCoroutine(SwitchTargetCoroutine(_dstTarget));
        }

        IEnumerator SwitchTargetCoroutine(Transform _dstTarget)
        {
            yield return null;
            while (Vector3.Distance(transform.position, UpdatePos(_dstTarget).position) > 0.01f)
            {
                transform.position = Vector3.Lerp(transform.position, UpdatePos(_dstTarget).position, smoothing * Time.deltaTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, UpdatePos(_dstTarget).rotation, smoothing * Time.deltaTime);
                
                yield return null;
            }
            switchingTarget = false;
            target = _dstTarget;
        }

        private Vector3 GetTarget(Transform dstTarget = null)
        {
            Transform t = target;
            if (dstTarget != null)
                t = dstTarget;

            //if (!string.IsNullOrEmpty(TargetBone))
            if(TargetBone >= 0)
            {
                if (dstTarget != null)
                {
                    umaData = dstTarget.GetComponent<UMAData>();
                }
                else
                {
                    umaData = target.GetComponent<UMAData>();
                }

                if (umaData != null && umaData.umaRecipe != null && umaData.umaRecipe.raceData != null && umaData.umaRecipe.raceData.umaTarget == RaceData.UMATarget.Humanoid && umaData.skeleton != null)
                {
                    string boneName = umaData.umaRecipe.raceData.TPose.BoneNameFromHumanName(targetStrings[(int)TargetBone]);
					if (!string.IsNullOrEmpty(boneName))
					{
						var bone = umaData.skeleton.GetBoneGameObject(Animator.StringToHash(boneName));
						if(bone != null)
							t = bone.transform;
					}
                }

                if (t == null)
                {
                    if (dstTarget != null)
                    {
                        Transform rendTrans = dstTarget.Find("UMARenderer");
                        if(rendTrans == null)
                            return dstTarget.position;
                        Renderer rend = rendTrans.GetComponent<Renderer>();
                        float height = rend.bounds.size.y;
                        distance = (height / 2) * 1.75f;
                        return dstTarget.Find("Root").position + new Vector3(0, height / 2, 0);
                    }
                    else
                    {
                        Transform rendTrans = target.Find("UMARenderer");
                        if(rendTrans == null)
                            return target.position;
                        Renderer rend = rendTrans.GetComponent<Renderer>();
                        float height = rend.bounds.size.y;
                        distance = (height / 2) * 1.75f;
                        return target.Find("Root").position + new Vector3(0, height / 2, 0);
                    }
                }
            }

            Vector3 dest = t.position + Offset;
            return dest;
        }

        void LateUpdate()
        {
            if (switchingTarget || target == null)
                return;
            UpdatePos();
        }

        TempTransform UpdatePos(Transform dstTarget = null)
        {
            TempTransform newTransform = new TempTransform();
            Vector3 tgt = GetTarget(dstTarget);

			//if the target height is less than or equal to 0, it is building or downloading- in this case dont move the camera
			if(tgt.y <= 0f)
			{
				return newTransform;
			}

            //DOS Modified tweaked this to be selectable
            if (Input.GetMouseButton((int)mouseButtonToUse) && Input.touchCount == 0)
            {
                x += Input.GetAxis("Mouse X") * xSpeed * 0.04f;
                y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
            }
            else if (Input.touchCount == 1 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId) == false)
            {
                Touch touchZero = Input.GetTouch(0);
                x += touchZero.deltaPosition.x * (xSpeed / 5)/* * distance */ * 0.04f;
                y -= touchZero.deltaPosition.y * (ySpeed / 5) * 0.02f;
            }

            if (EventSystem.current.currentSelectedGameObject == null || AlwaysOn == true)
            {
                if (Input.touchCount == 2)
                {
                    // Store both touches.
                    Touch touchZero = Input.GetTouch(0);
                    Touch touchOne = Input.GetTouch(1);

                    // Find the position in the previous frame of each touch.
                    Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                    Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                    // Find the magnitude of the vector (the distance) between the touches in each frame.
                    float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                    float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

                    // Find the difference in the distances between each frame. Flip it so it goes the right way
                    float deltaMagnitudeDiff = (prevTouchDeltaMag - touchDeltaMag) * -1;
                    distance = Mathf.Clamp(distance - (deltaMagnitudeDiff / 10) * (scrollrate / 10), distanceMin, distanceMax);
                }
                else
                {
                    distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * scrollrate, distanceMin, distanceMax);
                }
            }


            y = ClampAngle(y, yMinLimit, yMaxLimit);


            Quaternion rotation = Quaternion.Euler(y, x, 0);

            if (Clip)
            {
                RaycastHit hit;
                if (Physics.Linecast(tgt, transform.position, out hit))
                {
                    distance -= hit.distance;
                }
            }
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * negDistance + tgt;

            newTransform.rotation = rotation;
            newTransform.position = position;
            if (dstTarget == null)
            {
                transform.rotation = rotation;
                transform.position = position;
            }
            return newTransform;
        }

        public static float ClampAngle(float angle, float min, float max)
        {
            // These must be "while" loops. It's possible at low framerate that we have moved more than 360 degrees.
            while (angle < -360F)
                angle += 360F;
            while (angle > 360F)
                angle -= 360F;
            return Mathf.Clamp(angle, min, max);
        }
    }

}