using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.HTC.WVRLoader
{
    public class QuadGenerator : MonoBehaviour
    {
        [SerializeField] private Material deskMat;
        [SerializeField] private Material wallMat;
        [SerializeField] private Material doorMat;
        [SerializeField] private Material windowMat;
        [SerializeField] private Material ceilingMat;
        [SerializeField] private Material floorMat;
        [SerializeField] private Material defaultMat;

        public GameObject CreateQuad(PlaneData data)
        {
            GameObject quadObj = new GameObject(data.Type);

            MeshFilter meshFilter = quadObj.AddComponent<MeshFilter>();
            meshFilter.mesh = generateQuadFromPoints(quadObj.transform, data.Points);

            MeshRenderer meshRenderer = quadObj.AddComponent<MeshRenderer>();

            meshRenderer.materials = new Material[] { getMaterial((ShapeTypeEnum)Enum.Parse(typeof(ShapeTypeEnum), data.Type)) };

            Vector3 quadSize = meshFilter.mesh.bounds.size;

            meshRenderer.materials[0].SetVector("_Scale", new Vector4(
                Mathf.Max(quadSize.x, 0.00001f),
                Mathf.Max(quadSize.y, 0.00001f),
                Mathf.Max(quadSize.z, 0.00001f), 1));

            return quadObj;
        }

        private Material getMaterial(ShapeTypeEnum type)
        {
            switch(type)
            {
                case ShapeTypeEnum.ceiling:
                    return ceilingMat;
                case ShapeTypeEnum.door:
                    return doorMat;
                case ShapeTypeEnum.floor:
                    return floorMat;
                case ShapeTypeEnum.table:
                    return deskMat;
                case ShapeTypeEnum.wall:
                    return wallMat;
                case ShapeTypeEnum.window:
                    return windowMat;
                default:
                    return defaultMat;
            }
        }

        //four points in clockwise order, create a quad mesh with center-pivot and set it's transform
        private static Mesh generateQuadFromPoints(Transform quadTrans, Vector3[] points)
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

            quadTrans.position = center;
            quadTrans.rotation = Quaternion.Inverse(worldToLocal) * Quaternion.Inverse(rotAroundNormal);
            quadTrans.localScale = Vector3.one;
            return mesh;
        }
    }

}
