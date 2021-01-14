using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {
    [RequireComponent(typeof(LineRenderer))]
    public class NavigationDebugger : MonoBehaviour {

        //[SerializeField]
        private NavMeshAgent agentToDebug;

        private LineRenderer linerenderer;

        void Start() {
            linerenderer = GetComponent<LineRenderer>();
            agentToDebug = GetComponentInParent<NavMeshAgent>();
        }

        void Update() {
            if (agentToDebug.hasPath) {
                linerenderer.positionCount = agentToDebug.path.corners.Length;
                linerenderer.SetPositions(agentToDebug.path.corners);
                linerenderer.enabled = true;
            } else {
                linerenderer.enabled = false;
            }
        }
    }

}