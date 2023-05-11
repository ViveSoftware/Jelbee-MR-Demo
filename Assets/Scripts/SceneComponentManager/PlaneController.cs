using com.HTC.WVRLoader;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneController : MonoBehaviour
{
    [SerializeField] private ShapeTypeEnum shapeType;
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshCollider meshCollider;

    private PlaneData data;
    public PlaneData Data { get { return data; } }

    public ShapeTypeEnum ShapeType { get { return shapeType; }}

    public void Initialize(PlaneData planeData)
    {
        data = planeData;
        meshFilter.mesh = GenerateQuadFromPoints(transform, planeData.Points);
        meshCollider.sharedMesh = meshFilter.mesh;
    }

    private static Mesh GenerateQuadFromPoints(Transform quadTrans, Vector3[] points)
    {
        Vector3 center = (points[2] + points[0]) / 2f;
        Vector3 normal = Vector3.Cross(points[1] - points[0], points[2] - points[0]).normalized;

        Quaternion worldToLocal = Quaternion.FromToRotation(normal, Vector3.forward);

        Vector3[] vertices = new Vector3[4];
        for (int i = 0; i < 4; ++i)
        {
            vertices[i] = worldToLocal * (points[i] - center);
        }

        Quaternion rotAroundNormal = Quaternion.FromToRotation((vertices[1] - vertices[2]).normalized, Vector3.up);
        for (int i = 0; i < 4; ++i)
        {
            vertices[i] = rotAroundNormal * vertices[i];
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        Vector2[] uv = new Vector2[4] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0) };
        mesh.uv = uv;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        quadTrans.position = center;
        quadTrans.rotation = Quaternion.Inverse(worldToLocal) * Quaternion.Inverse(rotAroundNormal);
        quadTrans.localScale = Vector3.one;
        return mesh;
    }
}
