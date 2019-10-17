using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMA;

[CreateAssetMenu(fileName = "New Holdable Object", menuName = "Holdable Object")]
public class HoldableObject : DescribableResource {

    /// <summary>
    /// The prefab object to attach to the character when equipping this item
    /// </summary>
    [SerializeField]
    private GameObject physicalPrefab;

    /// <summary>
    /// The transform position of the physical prefab in relation to the target bone
    /// </summary>
    [SerializeField]
    private Vector3 physicalPosition = Vector3.zero;

    [SerializeField]
    private Vector3 sheathedPhysicalPosition = Vector3.zero;

    /// <summary>
    /// The transform rotation of the physical prefab
    /// </summary>
    [SerializeField]
    private Vector3 physicalRotation = Vector3.zero;

    [SerializeField]
    private Vector3 sheathedPhysicalRotation = Vector3.zero;

    /// <summary>
    /// The transform scale of the physical prefab
    /// </summary>
    [SerializeField]
    private Vector3 physicalScale = Vector3.one;

    [SerializeField]
    private Vector3 sheathedPhysicalScale = Vector3.one;

    /// <summary>
    /// The bone on the character model to attach the physical prefab to
    /// </summary>
    [SerializeField]
    private string targetBone;

    [SerializeField]
    private string sheathedTargetBone;

    public GameObject MyPhysicalPrefab { get => physicalPrefab; }
    public Vector3 MyPhysicalPosition { get => physicalPosition; }
    public Vector3 MyPhysicalRotation { get => physicalRotation; }
    public Vector3 MyPhysicalScale { get => physicalScale; }
    public string MyTargetBone { get => targetBone; }

    public Vector3 MySheathedPhysicalPosition { get => sheathedPhysicalPosition; }
    public Vector3 MySheathedPhysicalRotation { get => sheathedPhysicalRotation; }
    public Vector3 MySheathedPhysicalScale { get => sheathedPhysicalScale; }
    public string MySheathedTargetBone { get => sheathedTargetBone; }

}