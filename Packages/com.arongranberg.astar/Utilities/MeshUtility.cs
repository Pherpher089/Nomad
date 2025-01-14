using UnityEngine;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using Pathfinding.Collections;
using Unity.Mathematics;

namespace Pathfinding.Util {
#if MODULE_COLLECTIONS_2_1_0_OR_NEWER
	using NativeHashMapInt3Int = Unity.Collections.NativeHashMap<Int3, int>;
#else
	using NativeHashMapInt3Int = Unity.Collections.NativeParallelHashMap<Int3, int>;
#endif

	/// <summary>Helper class for working with meshes efficiently</summary>
	[BurstCompile]
	static class MeshUtility {
		public static void GetMeshData (Mesh.MeshDataArray meshData, int meshIndex, out NativeArray<Vector3> vertices, out NativeArray<int> indices) {
			var rawMeshData = meshData[meshIndex];
			vertices = new NativeArray<Vector3>(rawMeshData.vertexCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			rawMeshData.GetVertices(vertices);
			int totalIndices = 0;
			for (int subMeshIndex = 0; subMeshIndex < rawMeshData.subMeshCount; subMeshIndex++) {
				totalIndices += rawMeshData.GetSubMesh(subMeshIndex).indexCount;
			}
			indices = new NativeArray<int>(totalIndices, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			int offset = 0;
			for (int subMeshIndex = 0; subMeshIndex < rawMeshData.subMeshCount; subMeshIndex++) {
				var submesh = rawMeshData.GetSubMesh(subMeshIndex);
				rawMeshData.GetIndices(indices.GetSubArray(offset, submesh.indexCount), subMeshIndex);
				offset += submesh.indexCount;
			}
		}

		/// <summary>
		/// Flips triangles such that they are all clockwise in graph space.
		///
		/// The triangles may not be clockwise in world space since the graphs can be rotated.
		///
		/// The triangles array will be modified in-place.
		/// </summary>
		[BurstCompile]
		public static void MakeTrianglesClockwise (ref UnsafeSpan<Int3> vertices, ref UnsafeSpan<int> triangles) {
			for (int i = 0; i < triangles.Length; i += 3) {
				// Make sure the triangle is clockwise in graph space (it may not be in world space since the graphs can be rotated)
				// Note that we also modify the original triangle array because if the graph is cached then we will re-initialize the nodes from that array and assume all triangles are clockwise.
				if (!VectorMath.IsClockwiseXZ(vertices[triangles[i+0]], vertices[triangles[i+1]], vertices[triangles[i+2]])) {
					var tmp = triangles[i+0];
					triangles[i+0] = triangles[i+2];
					triangles[i+2] = tmp;
				}
			}
		}

		/// <summary>Removes duplicate vertices from the array and updates the triangle array.</summary>
		[BurstCompile]
		public struct JobRemoveDuplicateVertices : IJob {
			public NativeList<Int3> vertices;
			public NativeList<int> triangles;
			public NativeList<int> tags;

			public static int3 cross(int3 x, int3 y) => (x * y.yzx - x.yzx * y).yzx;

			public void Execute () {
				int numDegenerate = 0;
				unsafe {
					var firstVerts = new NativeHashMapInt3Int(vertices.Length, Allocator.Temp);

					// Remove duplicate vertices
					var compressedPointers = new NativeArray<int>(vertices.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

					var trianglesSpan = triangles.AsUnsafeSpan().Reinterpret<int3>(4);
					var verticesSpan = vertices.AsUnsafeSpan();
					var tagsSpan = tags.AsUnsafeSpan();

					int vertexCount = 0;
					for (int i = 0; i < verticesSpan.length; i++) {
						if (firstVerts.TryAdd(verticesSpan[i], vertexCount)) {
							compressedPointers[i] = vertexCount;
							verticesSpan[vertexCount++] = verticesSpan[i];
						} else {
							// There are some cases, rare but still there, that vertices are identical
							compressedPointers[i] = firstVerts[vertices[i]];
						}
					}
					vertices.Length = vertexCount;

					var verticesSpanI3 = vertices.AsUnsafeSpan().Reinterpret<int3>();

					uint triCount = 0;
					for (uint ti = 0; ti < trianglesSpan.length; ti++) {
						var tri = trianglesSpan[ti];
						tri = new int3(compressedPointers[tri.x], compressedPointers[tri.y], compressedPointers[tri.z]);

						// In some cases, users feed a navmesh graph a mesh with degenerate triangles.
						// These are triangles with a zero area.
						// We must remove these as they can otherwise cause issues for the JobCalculateTriangleConnections job, and they are generally just bad to include a navmesh.
						// Note: This cross product calculation can result in overflows if the triangle is large, but since we check for equality with zero it should not be a problem in practice.
						if (math.all(cross(verticesSpanI3[tri.y] - verticesSpanI3[tri.x], verticesSpanI3[tri.z] - verticesSpanI3[tri.x]) == 0)) {
							// Degenerate triangle
							numDegenerate++;
							continue;
						}
						trianglesSpan[triCount] = tri;
						tagsSpan[triCount] = tagsSpan[ti];
						triCount++;
					}

					triangles.Length = (int)triCount * 3;
					tags.Length = (int)triCount;
				}
				if (numDegenerate > 0) {
					Debug.LogWarning($"Input mesh contained {numDegenerate} degenerate triangles. These have been removed.\nA degenerate triangle is a triangle with zero area. It resembles a line or a point.");
				}
			}
		}
	}
}
