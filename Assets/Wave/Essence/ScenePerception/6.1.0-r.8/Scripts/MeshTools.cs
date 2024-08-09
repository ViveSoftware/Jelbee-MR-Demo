using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wave.Essence
{
	public static class MeshTools
	{
		public static Vector3[] GenerateQuadVertices()
		{
			// 2---3     Y
			// | \ |     |
			// 0---1  X--*
			//          /
			//         Z
			float s = 0.5f;
			return new Vector3[4]
			{
				new Vector3( s, -s, 0),
				new Vector3(-s, -s, 0),
				new Vector3( s,  s, 0),
				new Vector3(-s,  s, 0),
			};
		}

		// Unity is using clockwise winding order for front face
		public static int[] GenerateQuadIndices()
		{
			// 2---3
			// | \ |
			// 0---1
			return new int[] { 0, 2, 1, 1, 2, 3 };
		}

		public static Vector2[] GenerateQuadUVs()
		{
			// 2---3
			// | \ |
			// 0---1
			return new Vector2[]
			{
				new Vector2(0, 0),
				new Vector2(1, 0),
				new Vector2(0, 1),
				new Vector2(1, 1),
			};
		}

		public static Mesh GenerateQuadMesh()
		{
			Mesh quadMesh = new Mesh();
			quadMesh.vertices = GenerateQuadVertices();
			quadMesh.triangles = GenerateQuadIndices();
			quadMesh.uv = GenerateQuadUVs();
			quadMesh.RecalculateNormals();
			quadMesh.RecalculateTangents();
			return quadMesh;
		}

		public class Triangle
		{
			public readonly Vector3[] p = new Vector3[3];
			public readonly int[] i = new int[3];
			public readonly Vector3[] v = new Vector3[3];  // Vector of vertex
			// Not used +
			public readonly Vector2[] u = new Vector2[3];  // UV
			public readonly Vector2[] vu = new Vector2[3];  // Vector of uv
			// Not used -

			public Vector3 Normal { get; set; }
			public Vector3 DirToCenter { get; set; }
			public bool IsCenterNormalSameSide { get; set; }

			public void Calculate()
			{
				// V0 started from p0
				v[0] = p[1] - p[0];
				v[1] = p[2] - p[1];
				v[2] = p[0] - p[2];
				//area = Vector3.Cross(v0, v1).magnitude * 0.5f;
				Normal = Vector3.Cross(v[0], v[1]).normalized;
			}

			public Triangle(Vector3 p0, Vector3 p1, Vector3 p2, int i0, int i1, int i2)
			{
				p[0] = p0;
				p[1] = p1;
				p[2] = p2;
				i[0] = i0;
				i[1] = i1;
				i[2] = i2;
				Calculate();
			}

			// vertices and indices must be a triangle
			public Triangle(Vector3[] vertices, int[] indices)
			{
				if (vertices == null || indices == null)
					throw new ArgumentNullException();
				if (vertices.Length > 3 || indices.Length > 3)
					throw new ArgumentException();
				p[0] = vertices[0];
				p[1] = vertices[1];
				p[2] = vertices[2];
				i[0] = indices[0];
				i[1] = indices[1];
				i[2] = indices[2];
				Calculate();
			}

			// idxVertex, idxIndex are the index of the first vertex and index of the first index in the array
			// After idxVertex and idxIndex, the next 3 vertices and 3 indices will be used to construct the triangle
			public Triangle(Vector3[] vertices, int idxVertex, int[] indices, int idxIndex)
			{
				if (vertices == null || indices == null)
					throw new ArgumentNullException();
				if (vertices.Length - idxVertex >= 3 || indices.Length - idxIndex >= 3)
					throw new ArgumentException();
				p[0] = vertices[indices[idxVertex + 0]];
				p[1] = vertices[indices[idxVertex + 1]];
				p[2] = vertices[indices[idxVertex + 2]];
				i[0] = indices[idxIndex + 0];
				i[1] = indices[idxIndex + 1];
				i[2] = indices[idxIndex + 2];

				Calculate();
			}

			public int GetOrderIdInTriangle(int vId)
			{
				if (vId == i[0])
					return 0;
				if (vId == i[1])
					return 1;
				if (vId == i[2])
					return 2;
				return -1;
			}

			public int GetNextOrderId(int oId)
			{
				return (oId + 1) % 3;
			}

			internal void UpdateUV(MeshData md)
			{
				u[0] = md.uvs[i[0]];
				u[1] = md.uvs[i[1]];
				u[2] = md.uvs[i[2]];
				vu[0] = u[1] - u[0];
				vu[1] = u[2] - u[1];
				vu[2] = u[0] - u[2];
			}
		}

		public class MeshData
		{
			public class UVData
			{
				public UVData(Vector2 uv) { this.uv = uv;}
				public Vector2 uv;
				public List<int> tIds = new List<int>();
			}

			public class VertexData
			{
				public int vId;
				public readonly List<int> tIds = new List<int>(); // triangle id, not index of vertex
				public readonly List<UVData> newUVs = new List<UVData>();
				public VertexData(int id) { vId = id; }
			}

			public Vector3[] vertices;
			public int[] indices;
			public Triangle[] triangles;
			public VertexData[] vertexData;
			public Vector2[] uvs;

			public MeshData(Vector3[] verticesIn, int[] indicesIn)
			{
				vertices = new Vector3[verticesIn.Length];
				Array.Copy(verticesIn, vertices, verticesIn.Length);

				indices = new int[indicesIn.Length];
				Array.Copy(indicesIn, indices, indicesIn.Length);

				// Make Triangles by indices
				int ct = indices.Length / 3;
				triangles = new Triangle[ct];
				for (int i = 0; i < ct; i++)
				{
					var i0 = indices[i * 3];
					var i1 = indices[i * 3 + 1];
					var i2 = indices[i * 3 + 2];
					var p0 = vertices[i0];
					var p1 = vertices[i1];
					var p2 = vertices[i2];
					triangles[i] = new Triangle(p0, p1, p2, i0, i1, i2);
				}
				// Make vertices map to triangles
				int cv = vertices.Length;
				vertexData = new VertexData[cv];
				for (int i = 0; i < cv; i++)
					vertexData[i] = new VertexData(i);
				for (int i = 0; i < ct; i++)
				{
					var t = triangles[i];
					vertexData[t.i[0]].tIds.Add(i);
					vertexData[t.i[1]].tIds.Add(i);
					vertexData[t.i[2]].tIds.Add(i);
				}

				MakeTrianglesInVerticesMapUnique();
			}

			void MakeTrianglesInVerticesMapUnique()
			{
				// Make triangles in verticesMap unique
				int cv = vertices.Length;
				for (int i = 0; i < cv; i++)
				{
					var vd = vertexData[i];
					for (int j = 0; j < vd.tIds.Count; j++)
					{
						var tId = vd.tIds[j];
						for (int k = j + 1; k < vd.tIds.Count; k++)
						{
							if (tId == vd.tIds[k])
							{
								vd.tIds.RemoveAt(k);
								k--;
							}
						}
					}
				}
			}

			public void GenerateSmoothNormalsFromTriangles(out Vector3[] normals, out Vector4[] tangents)
			{
				int cv = vertices.Length;
				normals = new Vector3[cv];
				tangents = new Vector4[cv];
				for (int i = 0; i < cv; i++)
				{
					var vd = vertexData[i];
					var normal = Vector3.zero;
					var tangent = Vector3.zero;
					foreach (var tId in vd.tIds)
					{
						normal += triangles[tId].Normal;
						tangent += triangles[tId].v[0];
					}
					normal = normal.normalized;
					tangent = Vector3.Cross(normal, tangent.normalized).normalized;

					normals[i] = normal;
					tangents[i] = new Vector4(tangent.x, tangent.y, tangent.z, 1);
				}
			}

			public Vector3[] GenerateSmoothTangentsFromTriangles()
			{
				int cv = vertices.Length;
				Vector3[] normals = new Vector3[cv];
				for (int i = 0; i < cv; i++)
				{
					var vd = vertexData[i];
					var normal = Vector3.zero;
					foreach (var tId in vd.tIds)
					{
						normal += triangles[tId].Normal;
					}
					normals[i] = normal.normalized;
				}

				return normals;
			}
		}

		static Vector3 CalculateUVNormal(Vector2 u0, Vector2 u1, Vector2 u2)
		{
			// Note UV is top-left origin
			u0.y = 1 - u0.y;
			u1.y = 1 - u1.y;
			u2.y = 1 - u2.y;
			Vector3 uv0 = u1 - u0;
			Vector3 uv1 = u2 - u1;
			uv0.z = 0;
			uv1.z = 0;
			return Vector3.Cross(uv0, uv1).normalized;
		}

		// This unwrap method is not good.  It will make the UV mapping not continuous.
		// After u+=1 to fix the negative u, the UV normal will flip.
		//
		//     Correct           Wrong
		//    u0          u1    u0          u1
		//     |          |      |          |
		//   1-|-----2    |      |     2--1 |
		//    \|    /     |      |    /  -  |
		//     |   /      |      |   / -    |
		//     |\ /       |      |  /-      |
		//     | 0        |      | 0        |
		public static void SphereUnwrapSimple(MeshData md)
		{
			Bounds bounds = GenerateBoundsFromTriangles(md.vertices, md.indices);
			var center = bounds.center;
			var scale = bounds.size;
			// Avoid divid by zero:
			var scaleInv = new Vector3(1 / scale.x, 1 / scale.y, 1 / scale.z);

			int cv = md.vertices.Length;
			if (md.uvs == null)
				md.uvs = new Vector2[cv];
			if (md.uvs.Length != cv)
				Array.Resize(ref md.uvs, cv);

			// Calculate each vertex's UV mapping to a sphere based on the center of mesh.
			for (int i = 0; i < cv; i++)
			{
				var vc = md.vertices[i] - center;
				// Size normalized vertex
				var vn = Vector3.Scale(vc, scaleInv).normalized;
				// Calculate UV mapping.  When UV is (0.5f, 0.5f), the vertex is at the center of the sphere.
				// Atan2(1, 0) is PI/2, Atan2(-1, 0) is -PI/2, Atan2(0, 1) is 0, Atan2(0, -1) is PI
				// Add PI/2 to make the UV start from texture center.
				float u = (float)((Math.Atan2(vn.z, vn.x) + Math.PI / 2) / (Math.PI * 2));
				// Acos(0) is PI/2, Acos(1) is 0, Acos(-1) is PI
				// Acos(0) will be PI/2, so the UV will start from texture center.
				float v = 1 - Mathf.Acos(vn.y) / Mathf.PI;
				if (u < 0)
					u += 1;

				md.uvs[i] = new Vector2(u, v);
				md.vertexData[i].newUVs.Clear();
				//Debug.Log($"v[{i}]=({vc.x}, {vc.y}, {vc.z}) UV[{i}]=({u}, {v})");
			}
		}

		// This unwrap method fix flip problem in SphereUnwrapSimple.
		//
		//     Correct           Wrong         
		//    u0         u1     u0         u1  
		//     |          |      |          |  
		//   1-|-----2    |      |     2--1 |  
		//    \|    /     |      |    /  -  |  
		//     |   /      |      |   / -    |  
		//     |\ /       |      |  /-      |  
		//     | 0        |      | 0        |  
		//
		// In Sphere Projection, a triangle should not have one side's u size is greater
		// than 0.5.  If it is, the triangle will be flipped.
		//
		// try add one to the corner with minimal u.  And check the triangle's side length
		// again.  If it is still greater than 0.5, add one to the next corner.
		// After 2 times trial, the flip problem should be fixed
		//
		// Record the correct related u and v for each vertex as two vector
		// Keep one corner's u as previous u, and move the other two corners's u with
		// the related u shift.
		//
		// After fixed some point at pole position will still incorrect.  Especially the
		// Cube's case. Upper and lower pole will have incorrect UV.
		// For the sphere mesh case, the pole still not ok.
		// For complex mesh, most uv looks great.
		//
		// Before unwrap, the normals and tangents should be calculated first.
		// Otherwise, the normal on duplicated vertices will be incorrect.
		public static void SphereUnwrap(MeshData md, out MeshData mdOut, List<Vector3> normals, List<Vector4> tangents)
		{
			Bounds bounds = GenerateBoundsFromTriangles(md.vertices, md.indices);
			var center = bounds.center;
			var scale = bounds.size;
			// Avoid divid by zero:
			var scaleInv = new Vector3(1 / scale.x, 1 / scale.y, 1 / scale.z);

			int cv = md.vertices.Length;
			if (md.uvs == null)
				md.uvs = new Vector2[cv];

			Vector3[] centerNormals = new Vector3[cv];
			// Calculate each vertex's UV mapping to a sphere based on the center of mesh.
			for (int i = 0; i < cv; i++)
			{
				var vc = md.vertices[i] - center;
				// Size normalized vertex
				var vn = Vector3.Scale(vc, scaleInv).normalized;
				centerNormals[i] = vn;
				// Calculate UV mapping.  When UV is (0.5f, 0.5f), the vertex is at the center of the sphere.
				// Atan2(1, 0) is PI/2, Atan2(-1, 0) is -PI/2, Atan2(0, 1) is 0, Atan2(0, -1) is PI
				// Add PI/2 to make the UV start from texture center.
				float u = (float)((Math.Atan2(vn.z, vn.x) + Math.PI / 2) / (Math.PI * 2));
				// Acos(0) is PI/2, Acos(1) is 0, Acos(-1) is PI
				// Acos(0) will be PI/2, so the UV will start from texture center.
				float v = 1 - Mathf.Acos(vn.y) / Mathf.PI;

				// In this projection, some vertex's u will be negative.
				if (u < 0)
					u += 1;

				md.uvs[i] = new Vector2(u, v);
				md.vertexData[i].newUVs.Clear();
				//Debug.Log($"v[{i}]=({vc.x}, {vc.y}, {vc.z}) UV[{i}]=({u}, {v})");
			}

			List<int> tidsEdgeCrossPole = new List<int>();
			List<int> tidsHasPointAtPole = new List<int>();
			List<int> tidsNeedFixEdgeTooLong = new List<int>();
			int ct = md.triangles.Length;
			// Check if the triangle's uv's normal is not same direction to the normal from vertex to center.
			// If not, The triangle's uv should be flipped.
			for (int tId = 0; tId < ct; tId++)
			{
				var t = md.triangles[tId];

				// Check if the triangle's normal is toward to center of mesh
				t.DirToCenter = (centerNormals[t.i[0]] + centerNormals[t.i[1]] + centerNormals[t.i[2]]).normalized;
				t.IsCenterNormalSameSide = Vector3.Dot(t.DirToCenter, t.Normal) >= 0;
				t.UpdateUV(md);

				SphereUnwrapCheckTriangleUVProblem(md, tId, out var isPointAtThePole, out var isEdgeTooLong, out var hasEdgeCrossPole);

				if (hasEdgeCrossPole)
				{
					Debug.Log($"SphereUnwrap: tId={tId} has edge cross pole.");
				}

				if (isPointAtThePole)
				{
					// Not to check if there are two points at the pole.
					// It is possible if a edge is a stralight line vertically at center.
					// Still possible the point at pole need fix edge too long.
					Debug.Log($"SphereUnwrap: tId={tId} has point at the pole.");
					tidsHasPointAtPole.Add(tId);
				}

				if (isEdgeTooLong)
				{
					Debug.Log($"SphereUnwrap: tId={tId} need fix edge too long.");
					tidsNeedFixEdgeTooLong.Add(tId);
				}
				else
				{
					// These are correct triangles.  Add unique uvs to each vertex
					AddUniqueUV(md.vertexData[t.i[0]].newUVs, md.uvs[t.i[0]], tId);
					AddUniqueUV(md.vertexData[t.i[1]].newUVs, md.uvs[t.i[1]], tId);
					AddUniqueUV(md.vertexData[t.i[2]].newUVs, md.uvs[t.i[2]], tId);
					continue;
				}
			}

			Debug.Log($"SphereUnwrap: need fixed tid count is {tidsNeedFixEdgeTooLong.Count}.");

			int fixedCount = 0;
			// Correct it by keep the triangle's uv shape
			for (int i = 0; i < tidsNeedFixEdgeTooLong.Count; i++)
			{
				var tId = tidsNeedFixEdgeTooLong[i];
				SphereUnwrapFixEdgeTooLong(md, tId);
				fixedCount++;
			}

			for (int i = 0; i < tidsHasPointAtPole.Count; i++)
			{
				var tId = tidsHasPointAtPole[i];
				var t = md.triangles[tId];
				var p0 = t.i[0];
				var p1 = t.i[1];
				var p2 = t.i[2];
				Vector2[] u = new Vector2[3];
				// After fixed edge too long. the u may be changed at x.  We deal with y here.  Not effected by x.
				u[0] = md.uvs[p0];
				u[1] = md.uvs[p1];
				u[2] = md.uvs[p2];

				// Check which u has y=0 or y=1
				// Sort by y.
				int[] order = new int[3] { 0, 1, 2 };
				if (u[order[0]].y > u[order[1]].y)
					(order[1], order[0]) = (order[0], order[1]);
				if (u[order[1]].y > u[order[2]].y)
					(order[2], order[1]) = (order[1], order[2]);
				if (u[order[0]].y > u[order[1]].y)
					(order[1], order[0]) = (order[0], order[1]);

				bool o0 = Mathf.Approximately(u[order[0]].y, 0);
				bool o1 = Mathf.Approximately(u[order[1]].y, 0) || Mathf.Approximately(u[order[1]].y, 1);
				bool o2 = Mathf.Approximately(u[order[2]].y, 1);

				// TODO Cannot fix this case now
				if (o0 && o2 || o1 && o2 || o0 && o1)
				{
					continue;
				}

				var pOrderAtPole = order[0];
				if (o1)
					pOrderAtPole = order[1];
				else if (o2)
					pOrderAtPole = order[2];

				// First point use exist vertex
				// UV map
				// +-*--*--*--+
				// | |\ |\ |\ |
				// | | \| \| \|
				// | *--*--*--*
				// |          |
				// +----------+
				bool found = false;
				var newX = Mathf.Min(u[(pOrderAtPole + 1) % 3].x, u[(pOrderAtPole + 2) % 3].x);

				// Modify the uv of the point at the pole
				foreach (var uvd in md.vertexData[t.i[pOrderAtPole]].newUVs)
				{
					// Must have a matched.
					if (uvd.uv == u[pOrderAtPole])
					{
						uvd.uv.x = newX;
						found = true;
						break;
					}
				}
				if (!found) {
					Debug.LogWarning("SphereUnwrap: FixPoleProblem: Not found a matching UVData");
					continue;
				}
			}

			Debug.Log($"SphereUnwrap: fixedCount={fixedCount}.");

			// Make a copy of original vertices, indices, and uvs
			List<Vector3> vertices = new List<Vector3>(md.vertices);
			List<int> indices = new List<int>(md.indices);
			List<Vector2> uvs = new List<Vector2>(md.uvs);
			//List<int> indicesUdpateTimes = new List<int>(md.indices.Length);

			Debug.Log($"SphereUnwrap: vertices count is {vertices.Count}.");

			//for (int i = 0; i < indices.Count; i++)
			//	indicesUdpateTimes.Add(0);


			// Check all vertices's newUVs, and add new vertices, indices, and uvs
			for (int vId = 0; vId < cv; vId++)
			{
				var vd = md.vertexData[vId];
				var uvc = vd.newUVs.Count;
				if (uvc == 0)
				{
					Debug.Log("SphereUnwrap: Vertex is not used in any triangle.");
					continue;
				}

				uvs[vId] = vd.newUVs[0].uv;
				if (uvc == 1)
					continue;

				// Duplicate vertex and uv
				for (int j = 1; j < uvc; j++)
				{
					int newVId = vertices.Count;
					vertices.Add(md.vertices[vId]);
					uvs.Add(vd.newUVs[j].uv);
					normals.Add(normals[vId]);
					tangents.Add(tangents[vId]);

					// Update related triangle's indices
					foreach (var tId in vd.newUVs[j].tIds)
					{
						var t = md.triangles[tId];
						int indexID = tId * 3 + t.GetOrderIdInTriangle(vId);
						indices[indexID] = newVId;
						//indicesUdpateTimes[indexID]++;
					}
				}
			}

			//for (int i = 0; i < indicesUdpateTimes.Count; i++)
			//{
			//	if (indicesUdpateTimes[i] > 1)
			//		Debug.LogError($"SphereUnwrap: indicesUdpateTimes[{i}]={indicesUdpateTimes[i]}");
			//}

			mdOut = new MeshData(vertices.ToArray(), indices.ToArray());
			mdOut.uvs = uvs.ToArray();

			Debug.Log($"SphereUnwrap: new vertices count is {vertices.Count}.");
		}

		static void SphereUnwrapCheckTriangleUVProblem(MeshData md, int tId, out bool isPointAtThePole, out bool isEdgeTooLong, out bool hasEdgeCrossPole)
		{
			var t = md.triangles[tId];
			// Edge of the triangle's uv should never greater than 0.5f in sphere projection.  This need fix.
			isEdgeTooLong = t.vu[0].x > 0.5f || t.vu[1].x > 0.5f || t.vu[2].x > 0.5f;
			// If a point is at the pole, we should create more vertices in its triangle.
			isPointAtThePole =
				Mathf.Approximately(t.u[0].y, 0) || Mathf.Approximately(t.u[0].y, 1) ||
				Mathf.Approximately(t.u[1].y, 0) || Mathf.Approximately(t.u[1].y, 1) ||
				Mathf.Approximately(t.u[2].y, 0) || Mathf.Approximately(t.u[2].y, 1);

			// Check if edge cross the pole
			hasEdgeCrossPole =
				Mathf.Approximately(Mathf.Abs(t.vu[0].x), 0.5f) ||
				Mathf.Approximately(Mathf.Abs(t.vu[1].x), 0.5f) ||
				Mathf.Approximately(Mathf.Abs(t.vu[2].x), 0.5f);
		}

		// Fix Edge too long problem by try add 1 to the min u and check the triangle's side length again.
		static void SphereUnwrapFixEdgeTooLong(MeshData md, int tId)
		{
			var t = md.triangles[tId];
			var p0 = t.i[0];
			var p1 = t.i[1];
			var p2 = t.i[2];
			Vector2[] u = new Vector2[3];
			u[0] = md.uvs[p0];
			u[1] = md.uvs[p1];
			u[2] = md.uvs[p2];

			// Harmful effect to cube
			//// If there are two point have same u, the problem is eaiser to fix.
			//// If not, we need to try two times to fix it.
			//// Check if there are two points have same u.  If true, add 1 to the u which is not equals.
			//bool twoPointHaveSameU = u[0].x == u[1].x || u[1].x == u[2].x || u[0].x == u[2].x;
			//if (twoPointHaveSameU)
			//{
			//	if (u[0].x == u[1].x)
			//		u[2].x += 1;
			//	else if (u[1].x == u[2].x)
			//		u[0].x += 1;
			//	else if (u[0].x == u[2].x)
			//		u[1].x += 1;

			//	var uvNormal = CalculateUVNormal(u[0], u[1], u[2]);
			//	bool toOutside = uvNormal.z >= 0;
			//	if (toOutside == t.IsCenterNormalSameSide)
			//	{
			//		AddUniqueUV(md.vertexData[p0].newUVs, u[0], tId);
			//		AddUniqueUV(md.vertexData[p1].newUVs, u[1], tId);
			//		AddUniqueUV(md.vertexData[p2].newUVs, u[2], tId);
			//		return;
			//	}
			//}

			// To avoid two points have same u, sort the u by x. Smaller x will be u[0]
			int[] order = new int[3] { 0, 1, 2 };
			if (u[order[0]].x > u[order[1]].x)
				(order[1], order[0]) = (order[0], order[1]);
			if (u[order[1]].x > u[order[2]].x)
				(order[2], order[1]) = (order[1], order[2]);
			if (u[order[0]].x > u[order[1]].x)
				(order[1], order[0]) = (order[0], order[1]);

			bool hasFix = false;
			// Two times trial to fix the triangle's uv
			for (int z = 0; z < 2; z++)
			{
				// Add 1 to the min u
				u[order[z]].x += 1;

				var uDiff0 = u[1].x - u[0].x;
				var uDiff1 = u[2].x - u[1].x;
				var uDiff2 = u[0].x - u[2].x;
				if (uDiff0 > 0.5f || uDiff1 > 0.5f || uDiff2 > 0.5f)
				{
					continue;
				}
				else
				{
					hasFix = true;
					break;
				}
			}

			if (!hasFix)
			{
				// TODO The triangle maybe contain or have edge cross the pole, we should fix it by add new vertex at pole.
				Debug.Log($"SphereUnwrap: tId={tId} has no fix.  Keep origin.");

				// Keep orignal result.
				AddUniqueUV(md.vertexData[p0].newUVs, md.uvs[p0], tId);
				AddUniqueUV(md.vertexData[p1].newUVs, md.uvs[p1], tId);
				AddUniqueUV(md.vertexData[p2].newUVs, md.uvs[p2], tId);
				return;
			}

			// calculate related position of new u
			float[] vector = new float[3];
			vector[0] = u[1].x - u[0].x;
			vector[1] = u[2].x - u[1].x;
			vector[2] = u[0].x - u[2].x;

			u[0] = md.uvs[p0];
			u[1] = md.uvs[p1];
			u[2] = md.uvs[p2];

			// Keep u[0] not move, and move u[1] and u[2] to the right position
			u[1].x = u[0].x + vector[0];
			u[2].x = u[1].x + vector[1];
			//u[0].x = u[2].x + vector[2];
			//u[1].x = u[0].x + vector[0];

			// Add unique uvs to each vertex
			AddUniqueUV(md.vertexData[p0].newUVs, u[0], tId);
			AddUniqueUV(md.vertexData[p1].newUVs, u[1], tId);
			AddUniqueUV(md.vertexData[p2].newUVs, u[2], tId);
			Debug.Log($"SphereUnwrap: tId={tId} fixed.");
		}



		public static void AddUniqueUV(List<MeshData.UVData> list, Vector2 newUv, int tId)
		{
			foreach (var uvd in list)
			{
				if (newUv == uvd.uv)
				{
					uvd.tIds.Add(tId);
					return;
				}
			}
			var newUvd = new MeshData.UVData(newUv);
			newUvd.tIds.Add(tId);
			list.Add(newUvd);
		}

		public static Vector3[] GenerateSmoothNormalsFromTriangles(Vector3[] vertices, int[] indices)
		{
			int cv = vertices.Length;
			int ct = indices.Length / 3;

			Vector3[] normals = new Vector3[cv];
			for (int i = 0; i < cv; i++)
				normals[i] = Vector3.zero;

			for (int i = 0; i < ct; i++)
			{
				var i0 = i * 3;
				var i1 = i * 3 + 1;
				var i2 = i * 3 + 2;
				var p0 = vertices[i0];
				var p1 = vertices[i1];
				var p2 = vertices[i2];

				var v0 = p1 - p0;
				var v1 = p2 - p1;
				var normal = Vector3.Cross(v0, v1).normalized;

				normals[i] += normal;
			}

			for (int i = 0; i < cv; i++)
				normals[i] = normals[i].normalized;

			return normals;
		}

		public static Bounds GenerateBoundsFromTriangles(Vector3[] vertices, int[] indices)
		{
			// Only adapt vertices that are used in indices
			Bounds bounds = new Bounds();
			for (int i = 0; i < indices.Length; i++)
			{
				bounds.Encapsulate(vertices[indices[i]]);
			}
			return bounds;
		}

		// Unity is using clockwise winding order for front face.  Input should be in Unity's left-handed coordinate system.
		public static Mesh GenerateMeshDataFromTriangle(Vector3[] vertices, int[] indices)
		{
			Mesh mesh = new Mesh();

			if (vertices.Length >= 65535)
			{
				mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
			}

			var md = new MeshData(vertices, indices);
			mesh.vertices = md.vertices;
			mesh.triangles = md.indices;
			mesh.RecalculateNormals();
			mesh.RecalculateTangents();

			List<Vector3> normals = new List<Vector3>(mesh.normals);
			List<Vector4> tangents = new List<Vector4>(mesh.tangents);

			// Make normals before unwrap can help duplicated vertex's normal to be smooth.

			SphereUnwrap(md, out var mdOut, normals, tangents);
			//SphereUnwrapSimple(md);
			mesh.vertices = mdOut.vertices;
			mesh.triangles = mdOut.indices;
			mesh.uv = mdOut.uvs;
			mesh.normals = normals.ToArray();
			mesh.tangents = tangents.ToArray();

			return mesh;
		}

		public static Vector3[] GenerateCubeVertex()
		{

			//     2------3
			//    /|     /|
			//   / |    / |
			//  0------1  |
			//  |  6---|--7
			//  | /    | /    Y Z
			//  |/     |/     |/
			//  4------5      *--X

			var s = 0.5f;
			return new Vector3[8] {
					new Vector3(-s,  s, -s), // Top Left Back
					new Vector3( s,  s, -s), // Top Right Back
					new Vector3(-s,  s,  s), // Top Left Front
					new Vector3( s,  s,  s), // Top Right Front
					new Vector3(-s, -s, -s), // Bottom Left Back
					new Vector3( s, -s, -s), // Bottom Right Back
					new Vector3(-s, -s,  s), // Bottom Left Front
					new Vector3( s, -s,  s), // Bottom Right Front
				};
		}

		public static int[] GenerateCubeIndices()
		{
			//     2------3
			//    /|     /|
			//   / |    / |
			//  0------1  |
			//  |  6---|--7
			//  | /    | /    Y Z
			//  |/     |/     |/
			//  4------5      *--X

			return new int[36] { // 6 faces * 2 triangles * 3 vertices
					0, 1, 2, 1, 3, 2, // Top
					4, 6, 5, 5, 6, 7, // Bottom
					0, 2, 4, 2, 6, 4, // Left
					1, 5, 3, 3, 5, 7, // Right
					0, 4, 1, 1, 4, 5, // Back
					2, 3, 6, 3, 7, 6, // Front
				};
		}
	}
}
