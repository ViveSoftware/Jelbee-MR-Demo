using com.HTC.WVRLoader;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneMeshGenerator : MonoBehaviour
{
    [SerializeField]
    private float triangleWidth = 0.05f;

    [SerializeField]
    private float offsetRange = 0.03f;

    public Mesh Generate(List<PlaneData> planeDataList)
    {
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        int indexCount = 0;
        foreach (PlaneData planeData in planeDataList)
        {
            if (planeData.Type == ShapeTypeEnum.window.ToString() || planeData.Type == ShapeTypeEnum.ceiling.ToString() || planeData.Type == ShapeTypeEnum.chair.ToString()) continue;

            List<Vector3> planeVertices = new List<Vector3>();
            List<int> planeTriagles = new List<int>();

            Vector3 vx = planeData.Points[1] - planeData.Points[0];
            Vector3 vxDir = vx.normalized;
            int countX = Mathf.CeilToInt(vx.magnitude / triangleWidth) + 1;

            Vector3 vy = planeData.Points[2] - planeData.Points[1];
            Vector3 vyDir = vy.normalized;
            int countY = Mathf.CeilToInt(vy.magnitude / triangleWidth) + 1;

            Debug.Log($"vx.magnitude = {vx.magnitude}, triangleWidth = {triangleWidth}, countX = {countX}");

            for (int y = 0; y < countY; y++)
            {
                for (int x = 0; x < countX; ++x)
                {
                    Vector3 pos = planeData.Points[0] +
                        ((x < countX - 1) ? vxDir * triangleWidth * x : vx) +
                        ((y < countY - 1) ? vyDir * triangleWidth * y : vy);

                    pos += Vector3.ProjectOnPlane(Random.insideUnitSphere * offsetRange, planeData.Normal); 

                    planeVertices.Add(pos);
                }
            }

            for (int y = 0; y < countY - 1; y++)
            {
                for (int x = 0; x < countX - 1; ++x)
                {
                    planeTriagles.Add(countX * y + x + indexCount);
                    planeTriagles.Add(countX * y + (x + 1) + indexCount);
                    planeTriagles.Add(countX * (y + 1) + (x + 1) + indexCount);
                    
                    planeTriagles.Add(countX * y + x + indexCount);
                    planeTriagles.Add(countX * (y + 1) + (x + 1) + indexCount);
                    planeTriagles.Add(countX * (y + 1) + x + indexCount);                    
                }
            }

            vertices.AddRange(planeVertices);
            indexCount = vertices.Count;

            triangles.AddRange(planeTriagles);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateBounds();

        return mesh;
    }
}
