using System;
using UnityEngine;
using Wave.Native;

public static class MeshGenerationHelper
{
    public static Vector3[] GenerateQuadVertex(WVR_Extent2Df extend2D)
    {
        Vector3[] vertices = new Vector3[4]; //Four corners

        vertices[0] = new Vector3(-extend2D.width / 2, -extend2D.height / 2, 0); //Bottom Left
        vertices[1] = new Vector3(extend2D.width / 2, -extend2D.height / 2, 0); //Bottom Right
        vertices[2] = new Vector3(-extend2D.width / 2, extend2D.height / 2, 0); //Top Left
        vertices[3] = new Vector3(extend2D.width / 2, extend2D.height / 2, 0); //Top Right

        return vertices;
    }

    public static Mesh GenerateQuadMesh(Vector3[] vertices)
    {
        Mesh quadMesh = new Mesh();
        quadMesh.vertices = vertices;

        //Create array that represents vertices of the triangles
        int[] triangles = new int[6];
        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        triangles[3] = 1;
        triangles[4] = 3;
        triangles[5] = 2;

        quadMesh.triangles = triangles;
        Vector2[] uv = new Vector2[vertices.Length];
        Vector4[] tangents = new Vector4[vertices.Length];
        Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);
        for (int i = 0, y = 0; y < 2; y++)
        {
            for (int x = 0; x < 2; x++, i++)
            {
                uv[i] = new Vector2((float)x, (float)y);
                tangents[i] = tangent;
            }
        }
        quadMesh.uv = uv;
        quadMesh.tangents = tangents;
        quadMesh.RecalculateNormals();

        return quadMesh;
    }
    public static Mesh GenerateMesh(WVR_Vector3f_t[] vertexBuffer, UInt32[] indexBuffer)
    {
        Mesh generatedMesh = new Mesh();

        if (vertexBuffer.Length >= 65535)
        {
            generatedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        }

        Vector3[] vertexBufferUnity = new Vector3[vertexBuffer.Length];
        for (int i = 0; i < vertexBuffer.Length; i++)
        {
            Coordinate.GetVectorFromGL(vertexBuffer[i], out vertexBufferUnity[i]);
        }
        generatedMesh.vertices = vertexBufferUnity;

        int[] indexBufferUnity = new int[indexBuffer.Length];
        for (int i = 0; i < indexBuffer.Length; i++)
        {
            int indexMod3 = i % 3;
            if (indexMod3 == 0)
            {
                indexBufferUnity[i] = (int)indexBuffer[i];
            }
            else if (indexMod3 == 1)
            {
                indexBufferUnity[i] = (int)indexBuffer[i + 1];
            }
            else if (indexMod3 == 2)
            {
                indexBufferUnity[i] = (int)indexBuffer[i - 1];
            }
        }

        generatedMesh.triangles = indexBufferUnity;
        Vector2[] uv = new Vector2[vertexBuffer.Length];
        Vector4[] tangents = new Vector4[vertexBuffer.Length];
        Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);
        for (int i = 0, y = 0; y < 2; y++)
        {
            for (int x = 0; x < 2; x++, i++)
            {
                uv[i] = new Vector2((float)x, (float)y);
                tangents[i] = tangent;
            }
        }
        generatedMesh.uv = uv;
        generatedMesh.tangents = tangents;
        generatedMesh.RecalculateNormals();


        return generatedMesh;
    }
}