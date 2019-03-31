//using System;
//using HordeEngine;
//using UnityEngine;

//// NOT USED ATM

//[ExecuteInEditMode]
//public class DynamicQuadRenderer : MonoBehaviour
//{
//    public Material Material;
//    [SerializeField, Layer] public LayerMask Layer;
//    public Vector3 Offset = Vector3.zero;
//    public int InitialCapacity = 100;

//    [Header("Debug")]
//    public int QuadCount;

//    [NonSerialized] public DynamicQuadMesh QuadMesh;

//    void EnsureCreated()
//    {
//        if (QuadMesh == null)
//            QuadMesh = new DynamicQuadMesh(InitialCapacity);
//    }

//    public void LateUpdate()
//    {
//        EnsureCreated();

//        QuadMesh.ApplyChanges();
//        QuadCount = QuadMesh.QuadCount();

//        Matrix4x4 matrix = Matrix4x4.identity;
//        matrix.SetTRS(Offset, Quaternion.identity, Vector3.one);
//        Graphics.DrawMesh(QuadMesh.GetMesh(), matrix, Material, Layer.value);

//        QuadMesh.Clear();
//    }
//}
