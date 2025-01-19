using System;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding.Clipper2Lib;
using UnityEngine.Profiling;

namespace Pathfinding.Graphs.Navmesh {
	using Pathfinding;
	using Pathfinding.Util;
	using Pathfinding.Collections;
	using Unity.Collections;
	using Unity.Collections.LowLevel.Unsafe;
	using Unity.Mathematics;
	using Pathfinding.Sync;
	using Pathfinding.Pooling;
	using Unity.Burst;
	using System.Runtime.InteropServices;
	using UnityEngine.Assertions;
	using Unity.Jobs;

#if UNITY_2022_3_OR_NEWER && MODULE_COLLECTIONS_2_2_0_OR_NEWER
	using andywiecko.BurstTriangulator.LowLevel.Unsafe;
#endif

#if MODULE_COLLECTIONS_2_1_0_OR_NEWER
	using NativeHashMapVector2IntInt = Unity.Collections.NativeHashMap<Vector2Int, int>;
	using Unity.Jobs.LowLevel.Unsafe;
#else
	using NativeHashMapVector2IntInt = Unity.Collections.NativeParallelHashMap<Vector2Int, int>;
#endif

	public struct TileCutter {
		NavmeshBase graph;
		GridLookup<NavmeshClipper> cuts;
		TileLayout tileLayout;

		public TileCutter (NavmeshBase graph, GridLookup<NavmeshClipper> cuts, TileLayout tileLayout) {
			this.graph = graph;
			this.cuts = cuts;
			this.tileLayout = tileLayout;
		}

		public struct TileCutterOutput : IProgress, System.IDisposable {
			public TileMeshesUnsafe tileMeshes;

			public float Progress => 0;

			public void Dispose () {
				tileMeshes.Dispose(Allocator.Persistent);
			}
		}

		static void DisposeTileData (UnsafeSpan<UnsafeList<UnsafeSpan<Int3> > > tileVertices, UnsafeSpan<UnsafeList<UnsafeSpan<int> > > tileTriangles, UnsafeSpan<UnsafeList<UnsafeSpan<int> > > tileTags, Allocator allocator, bool skipFirst) {
			for (int ti = 0; ti < tileVertices.Length; ti++) {
				for (int i = skipFirst ? 1 : 0; i < tileVertices[ti].Length; i++) {
					tileVertices[ti][i].Free(allocator);
					tileTriangles[ti][i].Free(allocator);
					tileTags[ti][i].Free(allocator);
				}
				tileVertices[ti].Dispose();
				tileTriangles[ti].Dispose();
				tileTags[ti].Dispose();
			}
			tileVertices.Free(allocator);
			tileTriangles.Free(allocator);
			tileTags.Free(allocator);
		}

		public static void EnsurePreCutDataExists (NavmeshBase graph, NavmeshTile tile) {
			if (!tile.isCut) {
				Assert.IsTrue(tile.preCutTris.Length == 0);
				Assert.IsTrue(tile.preCutVertsInTileSpace.Length == 0);
				Assert.IsTrue(tile.preCutTags.Length == 0);
				Assert.IsTrue(tile.nodes.Length * 3 == tile.tris.Length);

				// No mesh data from before cutting this tile
				// This means that the tile hasn't had any cuts applied to it before,
				// and the arrays were not saved to reduce memory usage.
				// We need to re-create the arrays from the post-cut data (which is identical, since it didn't have any cuts applied to it).
				tile.preCutTris = tile.tris.Clone(Allocator.Persistent);
				tile.preCutVertsInTileSpace = tile.vertsInGraphSpace.Clone(Allocator.Persistent);

				// Convert from graph space to tile space
				var tileMinInGraphSpace = (Int3)graph.GetTileBoundsInGraphSpace(tile.x, tile.z).min;
				for (int j = 0; j < tile.preCutVertsInTileSpace.Length; j++) {
					tile.preCutVertsInTileSpace[j] -= tileMinInGraphSpace;
				}

				// Read the tags from the nodes
				tile.preCutTags = new UnsafeSpan<uint>(Allocator.Persistent, tile.nodes.Length);
				for (int j = 0; j < tile.nodes.Length; j++) {
					tile.preCutTags[j] = tile.nodes[j].Tag;
				}
				tile.isCut = true;
			}
			Assert.IsTrue(tile.preCutTags.Length * 3 == tile.preCutTris.Length);
		}

		static bool CheckVersion () {
#if !UNITY_2022_3_OR_NEWER
			Debug.LogError("The NavmeshCut component requires Unity 2022.3 or newer to work, due to Unity bugs in earlier versions. Please update Unity to 2022.3.21 or later, if you want to use navmesh cutting.");
			return false;
#elif !MODULE_COLLECTIONS_2_2_0_OR_NEWER
			Debug.LogError("The NavmeshCut component requires the Collections package version 2.2.0 or newer to work. Please update the Collections package to 2.2.0 or later, if you want to use navmesh cutting.");
			return false;
#else
			return true;
#endif
		}

		public Promise<TileCutterOutput> Schedule (List<Vector2Int> tileCoordinates) {
			if (cuts == null) {
				// No cuts have been added
				return new Promise<TileCutterOutput>(default, default);
			}

			var tileCount = tileCoordinates.Count;
			var tileVertices = new UnsafeSpan<UnsafeList<UnsafeSpan<Int3> > >(Allocator.Persistent, tileCount);
			var tileTriangles = new UnsafeSpan<UnsafeList<UnsafeSpan<int> > >(Allocator.Persistent, tileCount);
			var tileTags = new UnsafeSpan<UnsafeList<UnsafeSpan<int> > >(Allocator.Persistent, tileCount);
			for (int ti = 0; ti < tileVertices.Length; ti++) {
				tileVertices[ti] = new UnsafeList<UnsafeSpan<Int3> >(1, Allocator.Persistent);
				tileTriangles[ti] = new UnsafeList<UnsafeSpan<int> >(1, Allocator.Persistent);
				tileTags[ti] = new UnsafeList<UnsafeSpan<int> >(1, Allocator.Persistent);
			}

			var cutCollection = TileHandler.CollectCuts(cuts, tileCoordinates, graph.NavmeshCuttingCharacterRadius, tileLayout, ref tileVertices, ref tileTriangles, ref tileTags);
			if (!cutCollection.cuttingRequired || !CheckVersion()) {
				// Dispose all the arrays
				DisposeTileData(tileVertices, tileTriangles, tileTags, Allocator.Persistent, false);
				cutCollection.Dispose();

				// If there are no cuts or adds, then skip the cutting step
				return new Promise<TileCutterOutput>(default, default);
			}

#if UNITY_2022_3_OR_NEWER && MODULE_COLLECTIONS_2_2_0_OR_NEWER
			Profiler.BeginSample("Schedule cutting");
			var meshes = new NativeArray<TileMesh.TileMeshUnsafe>(tileCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			for (int i = 0; i < tileCoordinates.Count; i++) {
				var tile = graph.GetTile(tileCoordinates[i].x, tileCoordinates[i].y);
				EnsurePreCutDataExists(graph, tile);
				Assert.IsTrue(tile.isCut);
				meshes[i] = new TileMesh.TileMeshUnsafe {
					triangles = tile.preCutTris,
					verticesInTileSpace = tile.preCutVertsInTileSpace,
					tags = tile.preCutTags,
				};
			}
			var tileWorldSize = new Vector2(graph.TileWorldSizeX, graph.TileWorldSizeZ);
			var inputTileMeshes = new TileMeshesUnsafe(meshes, new IntRect(0, 0, -1, -1), tileWorldSize);

			// Clear memory for output, so that if the job crashes for some reason, it will not contain garbage data
			var outputTileMeshesArr = new NativeArray<TileMesh.TileMeshUnsafe>(tileCount, Allocator.Persistent, NativeArrayOptions.ClearMemory);
			var outputTileMeshes = new TileMeshesUnsafe(outputTileMeshesArr, new IntRect(0, 0, -1, -1), tileWorldSize);
			var output = new TileCutterOutput {
				tileMeshes = outputTileMeshes,
			};

			TileHandler.InitDelegates();
			var handle = new JobCutTiles {
				tileVertices = tileVertices,
				tileTriangles = tileTriangles,
				tileTags = tileTags,
				cutCollection = cutCollection,
				inputTileMeshes = inputTileMeshes,
				outputTileMeshes = outputTileMeshesArr,
			}.Schedule();
			meshes.Dispose(handle);

			Profiler.EndSample();
			return new Promise<TileCutterOutput>(handle, output);
#else
			throw new System.Exception("Unreachable");
#endif
		}

		public Promise<TileCutterOutput> Schedule (Promise<TileBuilder.TileBuilderOutput> builderOutput) {
			if (cuts == null) {
				// No cuts have been added
				return new Promise<TileCutterOutput>(builderOutput.handle, default);
			}

			Profiler.BeginSample("Schedule cutting");
			var input = builderOutput.GetValue();
			var tileRect = input.tileMeshes.tileRect;
			var tileCoordinates = tileRect.GetInnerCoordinates();

			var tileCount = tileCoordinates.Count;
			var tileVertices = new UnsafeSpan<UnsafeList<UnsafeSpan<Int3> > >(Allocator.Persistent, tileCount);
			var tileTriangles = new UnsafeSpan<UnsafeList<UnsafeSpan<int> > >(Allocator.Persistent, tileCount);
			var tileTags = new UnsafeSpan<UnsafeList<UnsafeSpan<int> > >(Allocator.Persistent, tileCount);
			for (int ti = 0; ti < tileVertices.Length; ti++) {
				tileVertices[ti] = new UnsafeList<UnsafeSpan<Int3> >(1, Allocator.Persistent);
				tileTriangles[ti] = new UnsafeList<UnsafeSpan<int> >(1, Allocator.Persistent);
				tileTags[ti] = new UnsafeList<UnsafeSpan<int> >(1, Allocator.Persistent);
			}

			var cutCollection = TileHandler.CollectCuts(cuts, tileCoordinates, graph.NavmeshCuttingCharacterRadius, tileLayout, ref tileVertices, ref tileTriangles, ref tileTags);
			if (!cutCollection.cuttingRequired || !CheckVersion()) {
				DisposeTileData(tileVertices, tileTriangles, tileTags, Allocator.Persistent, false);
				cutCollection.Dispose();

				Profiler.EndSample();
				// If there are no cuts or adds, then skip the cutting step
				return new Promise<TileCutterOutput>(builderOutput.handle, default);
			}

#if UNITY_2022_3_OR_NEWER && MODULE_COLLECTIONS_2_2_0_OR_NEWER
			// Clear memory for output, so that if the job crashes for some reason, it will not contain garbage data
			var outputTileMeshesArr = new NativeArray<TileMesh.TileMeshUnsafe>(input.tileMeshes.tileMeshes.Length, Allocator.Persistent, NativeArrayOptions.ClearMemory);
			var outputTileMeshes = new TileMeshesUnsafe(outputTileMeshesArr, input.tileMeshes.tileRect, input.tileMeshes.tileWorldSize);
			var output = new TileCutterOutput {
				tileMeshes = outputTileMeshes,
			};

			TileHandler.InitDelegates();
			var handle = new JobCutTiles {
				tileVertices = tileVertices,
				tileTriangles = tileTriangles,
				tileTags = tileTags,
				cutCollection = cutCollection,
				inputTileMeshes = input.tileMeshes,
				outputTileMeshes = outputTileMeshesArr,
			}.Schedule(builderOutput.handle);

			Profiler.EndSample();
			return new Promise<TileCutterOutput>(handle, output);
#else
			throw new System.Exception("Unreachable");
#endif
		}

#if UNITY_2022_3_OR_NEWER && MODULE_COLLECTIONS_2_2_0_OR_NEWER
		[BurstCompile]
		struct JobCutTiles : IJob {
			// Will be disposed when the job is done
			public UnsafeSpan<UnsafeList<UnsafeSpan<Int3> > > tileVertices;
			// Will be disposed when the job is done
			public UnsafeSpan<UnsafeList<UnsafeSpan<int> > > tileTriangles;
			// Will be disposed when the job is done
			public UnsafeSpan<UnsafeList<UnsafeSpan<int> > > tileTags;
			// Will be disposed when the job is done
			public TileHandler.CutCollection cutCollection;
			public TileMeshesUnsafe inputTileMeshes;
			public NativeArray<TileMesh.TileMeshUnsafe> outputTileMeshes;

			public void Execute () {
				var tileCount = inputTileMeshes.tileMeshes.Length;
				Assert.AreEqual(tileCount, tileVertices.Length);
				Assert.AreEqual(tileCount, tileTriangles.Length);
				Assert.AreEqual(tileCount, tileTags.Length);
				Assert.AreEqual(tileCount, outputTileMeshes.Length);
				Assert.AreEqual(tileCount, cutCollection.tileCuts.Length);

				for (int i = 0; i < tileCount; i++) {
					// TODO: Insert at end instead
					tileVertices[i].InsertRange(0, 1);
					tileTriangles[i].InsertRange(0, 1);
					tileTags[i].InsertRange(0, 1);
					tileVertices[i][0] = inputTileMeshes.tileMeshes[i].verticesInTileSpace;
					tileTriangles[i][0] = inputTileMeshes.tileMeshes[i].triangles;
					tileTags[i][0] = inputTileMeshes.tileMeshes[i].tags.Reinterpret<int>();
				}
				var outputSpan = outputTileMeshes.AsUnsafeSpan();
				var tileSize = new Vector2Int(Mathf.RoundToInt(inputTileMeshes.tileWorldSize.x * Int3.FloatPrecision), Mathf.RoundToInt(inputTileMeshes.tileWorldSize.y * Int3.FloatPrecision));
				TileHandler.CutTiles(ref tileVertices, ref tileTriangles, ref tileTags, ref tileSize, ref cutCollection, ref outputSpan, Allocator.Persistent);

				// Note: The first element is the input mesh, which should not be freed
				DisposeTileData(tileVertices, tileTriangles, tileTags, Allocator.Persistent, true);
				cutCollection.Dispose();
			}
		}
#endif
	}

#if UNITY_2022_3_OR_NEWER && MODULE_COLLECTIONS_2_2_0_OR_NEWER
	static class TileHandlerCache {
		internal static Clipper64[] cachedClippers = new Clipper64[JobsUtility.ThreadIndexCount];
	}
#endif

	/// <summary>
	/// Utility class for updating tiles of navmesh/recast graphs.
	///
	/// Most operations that this class does are asynchronous.
	/// They will be added as work items to the AstarPath class
	/// and executed when the pathfinding threads have finished
	/// calculating their current paths.
	///
	/// See: navmeshcutting (view in online documentation for working links)
	/// See: <see cref="NavmeshUpdates"/>
	/// </summary>
	[BurstCompile]
	public static class TileHandler {
		static readonly Unity.Profiling.ProfilerMarker MarkerTriangulate = new Unity.Profiling.ProfilerMarker("Triangulate");
		static readonly Unity.Profiling.ProfilerMarker MarkerClipping = new Unity.Profiling.ProfilerMarker("Clipping");
		static readonly Unity.Profiling.ProfilerMarker MarkerPrepare = new Unity.Profiling.ProfilerMarker("Prepare");
		static readonly Unity.Profiling.ProfilerMarker MarkerAllocate = new Unity.Profiling.ProfilerMarker("Allocate");
		static readonly Unity.Profiling.ProfilerMarker MarkerCore = new Unity.Profiling.ProfilerMarker("Core");
		static readonly Unity.Profiling.ProfilerMarker MarkerCompress = new Unity.Profiling.ProfilerMarker("Compress");
		static readonly Unity.Profiling.ProfilerMarker MarkerRefine = new Unity.Profiling.ProfilerMarker("Refine");
		static readonly Unity.Profiling.ProfilerMarker MarkerEdgeSnapping = new Unity.Profiling.ProfilerMarker("EdgeSnapping");
		static readonly Unity.Profiling.ProfilerMarker MarkerCopyClippingResult = new Unity.Profiling.ProfilerMarker("CopyClippingResult");
		static readonly Unity.Profiling.ProfilerMarker CopyTriangulationToOutputMarker = new Unity.Profiling.ProfilerMarker("Copy to output");

#if UNITY_2022_3_OR_NEWER && MODULE_COLLECTIONS_2_2_0_OR_NEWER
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate bool CutFunction(ref UnsafeSpan<Point64Wrapper> subject, ref UnsafeSpan<Point64Wrapper> contourVertices, ref UnsafeSpan<NavmeshCut.ContourBurst> contours, ref UnsafeSpan<int> contourIndices, ref UnsafeSpan<int> contourIndicesDual, ref UnsafeList<Vector2Int> outputVertices, ref UnsafeList<int> outputVertexCountPerPolygon, int dual);

		struct CutFunctionKey {}
		private static readonly SharedStatic<IntPtr> CutFunctionPtr = SharedStatic<IntPtr>.GetOrCreate<CutFunctionKey>();
		private static CutFunction DelegateGCRoot;
#endif

		/// <summary>See <see cref="SnapEdges"/></summary>
		const int EdgeSnappingMaxDistance = 1;

		internal struct TileCuts {
			public int contourStartIndex;
			public int contourEndIndex;
		}

		internal struct ContourMeta {
			public bool isDual;
			public bool cutsAddedGeom;
		}

		internal struct CutCollection : System.IDisposable {
			/// <summary>
			/// Vertices of all cut contours in all tiles
			/// Stored in tile space for the tile they belong to.
			/// </summary>
			public UnsafeList<float2> contourVertices;
			public UnsafeList<NavmeshCut.ContourBurst> contours;
			public UnsafeList<ContourMeta> contoursExtra;
			public UnsafeList<TileCuts> tileCuts;
			[MarshalAs(UnmanagedType.U1)]
			public bool cuttingRequired;

			public void Dispose () {
				contourVertices.Dispose();
				contours.Dispose();
				contoursExtra.Dispose();
				tileCuts.Dispose();
			}
		}

		internal static CutCollection CollectCuts (GridLookup<NavmeshClipper> cuts, List<Vector2Int> tileCoordinates, float characterRadius, TileLayout tileLayout, ref UnsafeSpan<UnsafeList<UnsafeSpan<Int3> > > tileVertices, ref UnsafeSpan<UnsafeList<UnsafeSpan<int> > > tileTriangles, ref UnsafeSpan<UnsafeList<UnsafeSpan<int> > > tileTags) {
			Profiler.BeginSample("Collect navmesh cuts");
			var contourVertices = new UnsafeList<float2>(0, Allocator.Persistent);
			var contours = new UnsafeList<NavmeshCut.ContourBurst>(0, Allocator.Persistent);
			var contoursExtra = new UnsafeList<ContourMeta>(0, Allocator.Persistent);
			bool cuttingRequired = false;

			var tileCuts = new UnsafeList<TileCuts>(0, Allocator.Persistent);
			Int3[] vbuffer = null;

			for (int tileIndex = 0; tileIndex < tileCoordinates.Count; tileIndex++) {
				// Calculate tile bounds so that the correct cutting offset can be used
				// The tile will be cut in local space (i.e it is at the world origin) so cuts need to be translated
				// to that point from their world space coordinates
				var tileCoord = tileCoordinates[tileIndex];
				var graphSpaceBounds = tileLayout.GetTileBoundsInGraphSpace(tileCoord.x, tileCoord.y);
				var cutOffset = graphSpaceBounds.min;
				var transform = tileLayout.transform * Matrix4x4.Translate(cutOffset);

				var c = tileCoordinates[tileIndex];
				List<NavmeshCut> navmeshCuts = cuts.QueryRect<NavmeshCut>(new IntRect(c.x, c.y, c.x, c.y));

				var tileStartIndex = contours.Length;
				cuttingRequired |= navmeshCuts.Count > 0;

				for (int cutIndex = 0; cutIndex < navmeshCuts.Count; cutIndex++) {
					int startIndex = contours.Length;
					unsafe {
						navmeshCuts[cutIndex].GetContourBurst(&contourVertices, &contours, transform.inverseMatrix, characterRadius);
					}
					var m = new ContourMeta {
						isDual = navmeshCuts[cutIndex].isDual,
						cutsAddedGeom = navmeshCuts[cutIndex].cutsAddedGeom
					};
					for (int k = startIndex; k < contours.Length; k++) {
						contoursExtra.Add(m);
					}
				}

				ListPool<NavmeshCut>.Release(ref navmeshCuts);

				List<NavmeshAdd> navmeshAdds = cuts.QueryRect<NavmeshAdd>(new IntRect(c.x, c.y, c.x, c.y));
				cuttingRequired |= navmeshAdds.Count > 0;

				for (int cutIndex = 0; cutIndex < navmeshAdds.Count; cutIndex++) {
					navmeshAdds[cutIndex].GetMesh(ref vbuffer, out var tbuffer, out int vertexCount, transform);

					var vspan = new UnsafeSpan<Int3>(Allocator.Persistent, vertexCount);
					var tspan = new UnsafeSpan<int>(Allocator.Persistent, tbuffer.Length);

					for (int i = 0; i < vertexCount; i++) {
						vspan[i] = vbuffer[i];
					}
					for (int i = 0; i < tbuffer.Length; i++) {
						tspan[i] = tbuffer[i];
					}
					var tagsSpan = new UnsafeSpan<int>(Allocator.Persistent, tbuffer.Length / 3);
					for (int i = 0; i < tagsSpan.Length; i++) {
						tagsSpan[i] = 0;
					}

					tileVertices[tileIndex].Add(vspan);
					tileTriangles[tileIndex].Add(tspan);
					tileTags[tileIndex].Add(tagsSpan);
				}

				ListPool<NavmeshAdd>.Release(ref navmeshAdds);

				tileCuts.Add(new TileCuts {
					contourStartIndex = tileStartIndex,
					contourEndIndex = contours.Length
				});
			}

			Pathfinding.Pooling.ArrayPool<Int3>.Release(ref vbuffer);
			Profiler.EndSample();

			return new CutCollection {
					   contourVertices = contourVertices,
					   contours = contours,
					   contoursExtra = contoursExtra,
					   tileCuts = tileCuts,
					   cuttingRequired = cuttingRequired,
			};
		}

#if UNITY_2022_3_OR_NEWER && MODULE_COLLECTIONS_2_2_0_OR_NEWER
		[BurstCompile]
		internal static void CutTiles (ref UnsafeSpan<UnsafeList<UnsafeSpan<Int3> > > tileVertices, ref UnsafeSpan<UnsafeList<UnsafeSpan<int> > > tileTriangles, ref UnsafeSpan<UnsafeList<UnsafeSpan<int> > > tileTags, ref Vector2Int tileSize, ref CutCollection cutCollection, ref UnsafeSpan<TileMesh.TileMeshUnsafe> output, Allocator allocator) {
			Assert.AreEqual(tileVertices.Length, tileTriangles.Length);
			Assert.AreEqual(tileVertices.Length, tileTags.Length);
			Assert.AreEqual(tileVertices.Length, cutCollection.tileCuts.Length);

			MarkerPrepare.Begin();
			var contourVerticesP64 = ConvertTo64BitCoordinates(cutCollection.contourVertices);
			var cutBounds = CalculateCutBounds(ref cutCollection, ref contourVerticesP64);
			MarkerPrepare.End();

			MarkerAllocate.Begin();
			var tileCount = tileVertices.Length;
			var interestingCuts = new NativeList<int>(4, Allocator.Temp);
			var interestingDualCuts = new NativeList<int>(0, Allocator.Temp);
			var triBufferClip = new NativeArray<Int3>(7, Allocator.Temp);
			var triBufferClipTemp = new NativeArray<Int3>(7, Allocator.Temp);
			var triBuffer = new NativeArray<Point64Wrapper>(16, Allocator.Temp);

			var triangulationPositions = new NativeList<int2>(Allocator.Temp);
			var constraintEdges = new NativeList<int>(Allocator.Temp);
			var seenVertices = new NativeHashMap<int2, int>(64, Allocator.Temp);

			var triangulatorOutput = new andywiecko.BurstTriangulator.LowLevel.Unsafe.OutputData<int2>() {
				Positions = new NativeList<int2>(Allocator.Temp),
				Triangles = new NativeList<int>(Allocator.Temp),
				Status = new NativeReference<andywiecko.BurstTriangulator.Status>(Allocator.Temp),
				Halfedges = new NativeList<int>(Allocator.Temp),
				ConstrainedHalfedges = new NativeList<andywiecko.BurstTriangulator.HalfedgeState>(Allocator.Temp),
			};
			var tileOutputVertices = new NativeList<Int3>(Allocator.Temp);
			var tileOutputTriangles = new NativeList<int>(Allocator.Temp);
			var tileOutputTags = new NativeList<int>(Allocator.Temp);

			var triangulationOutputVertices = new NativeList<Vector2Int>(0, Allocator.Temp);
			var triangulationVertexCountPerPolygon = new NativeList<int>(0, Allocator.Temp);

			MarkerAllocate.End();

			for (int tileIndex = 0; tileIndex < tileCount; tileIndex++) {
				var allVertices = tileVertices[tileIndex];
				var allTriangles = tileTriangles[tileIndex];
				var cuts = cutCollection.tileCuts[tileIndex];

				if (cuts.contourStartIndex == cuts.contourEndIndex && allVertices.Length == 1) {
					// No cuts or adds: copy whole tile to output
					output[tileIndex] = new TileMesh.TileMeshUnsafe {
						verticesInTileSpace = allVertices[0].Clone(allocator),
						triangles = allTriangles[0].Clone(allocator),
						tags = tileTags[tileIndex][0].Clone(allocator).Reinterpret<uint>(),
					};
					continue;
				}

				tileOutputVertices.Clear();
				tileOutputTriangles.Clear();
				tileOutputTags.Clear();

				// Iterate over all meshes in the tile
				// This will typically only be 1, but there can be more if NavmeshAdd components are present
				for (int mi = 0; mi < allVertices.Length; mi++) {
					var vertices = allVertices[mi];
					var triangles = allTriangles[mi].Reinterpret<int3>(4);
					var tags = tileTags[tileIndex][mi];
					Assert.AreEqual(tags.Length, triangles.Length);

					// Iterate over all triangles in the tile
					for (int tileTriangleIndex = 0; tileTriangleIndex < triangles.Length; tileTriangleIndex++) {
						var tri = triangles[tileTriangleIndex];
						var a = vertices[tri.x];
						var b = vertices[tri.y];
						var c = vertices[tri.z];

						var triBounds = TriangleBounds(a, b, c);
						triBounds.max += EdgeSnappingMaxDistance;
						triBounds.min -= EdgeSnappingMaxDistance;

						FindIntersectingCuts(
							cutCollection.contoursExtra.AsUnsafeSpan().Slice(cuts.contourStartIndex, cuts.contourEndIndex - cuts.contourStartIndex),
							cutBounds.AsUnsafeSpan().Slice(cuts.contourStartIndex, cuts.contourEndIndex - cuts.contourStartIndex),
							interestingCuts,
							interestingDualCuts,
							triBounds,
							mi > 0
							);

						var tag = tags[tileTriangleIndex];
						if (interestingCuts.Length == 0 && interestingDualCuts.Length == 0 && mi == 0) {
							// Copy triangle to output as-is
							var vertexOffset = tileOutputVertices.Length;
							tileOutputVertices.Capacity = math.max(tileOutputVertices.Capacity, tileOutputVertices.Length + 3);
							tileOutputTriangles.Capacity = math.max(tileOutputTriangles.Capacity, tileOutputTriangles.Length + 3);

							tileOutputVertices.AddNoResize(a);
							tileOutputVertices.AddNoResize(b);
							tileOutputVertices.AddNoResize(c);
							for (int i = 0; i < 3; i++) {
								tileOutputTriangles.AddNoResize(vertexOffset + i);
							}
							tileOutputTags.Add(tag);
							continue;
						} else {
							// Copy triangle to buffer, and clip it against the tile if necessary
							int vertexCount;
							if (mi > 0) {
								triBufferClip[0] = a;
								triBufferClip[1] = b;
								triBufferClip[2] = c;
								// This is a navmesh add. We need to clip it against the tile bounds
								vertexCount = ClipAgainstRectangle(triBufferClip.AsUnsafeSpan(), triBufferClipTemp.AsUnsafeSpan(), tileSize);
								for (int i = 0; i < vertexCount; i++) {
									triBuffer[i] = new Point64Wrapper(triBufferClip[i].x, triBufferClip[i].z);
								}
							} else {
								vertexCount = 3;
								triBuffer[0] = new Point64Wrapper(a.x, a.z);
								triBuffer[1] = new Point64Wrapper(b.x, b.z);
								triBuffer[2] = new Point64Wrapper(c.x, c.z);
							}

							// Insert extra vertices on the edges of the triangle, if necessary
							var contoursSpan = cutCollection.contours.AsUnsafeSpan().Slice(cuts.contourStartIndex, cuts.contourEndIndex - cuts.contourStartIndex);
							MarkerEdgeSnapping.Begin();
							SnapEdges(ref triBuffer, ref vertexCount, contoursSpan, ref interestingCuts, ref contourVerticesP64);
							MarkerEdgeSnapping.End();

							// First iteration: Cut the triangle using normal navmesh cuts
							// Second iteration: Handle dual navmesh cuts (we keep the interior of these cuts)
							for (int mode = 0; mode < 2; mode++) {
								if (mode == 1 && interestingDualCuts.Length == 0) {
									// No dual cuts. Skip the second iteration
									break;
								}

								var triSpan = triBuffer.AsUnsafeReadOnlySpan().Slice(0, vertexCount);
								var interestingCutsSpan = interestingCuts.AsUnsafeSpan();
								var interestingCutsDualSpan = interestingDualCuts.AsUnsafeSpan();
								var verticesSpan = contourVerticesP64.AsUnsafeReadOnlySpan();
								triangulationOutputVertices.Clear();
								triangulationVertexCountPerPolygon.Clear();

								unsafe {
									MarkerClipping.Begin();
									// Clip the triangle against the cuts

									// We call a managed function from a burstified function to do this.
									// It requires a bit of setup, but is doable, and has pretty good performance too.
									var cutFunction = new FunctionPointer<CutFunction>(CutFunctionPtr.Data);
									var ok = cutFunction.Invoke(
										ref triSpan,
										ref verticesSpan,
										ref contoursSpan,
										ref interestingCutsSpan,
										ref interestingCutsDualSpan,
										ref UnsafeUtility.AsRef<UnsafeList<Vector2Int> >(triangulationOutputVertices.GetUnsafeList()),
										ref UnsafeUtility.AsRef<UnsafeList<int> >(triangulationVertexCountPerPolygon.GetUnsafeList()),
										mode
										);
									MarkerClipping.End();
									if (!ok) {
										Debug.LogError("Error during cutting");
										continue;
									}
								}

								if (triangulationVertexCountPerPolygon.Length == 0) {
									// No output. Everything was cut away.
									continue;
								} else if (triangulationOutputVertices.Length == 3 && triangulationVertexCountPerPolygon.Length == 1) {
									// Output is a simple triangle. Just copy to output
									CopyTriangulationToOutputMarker.Begin();
									var vertexOffset = tileOutputVertices.Length;
									tileOutputVertices.Capacity = math.max(tileOutputVertices.Capacity, tileOutputVertices.Length + 3);
									tileOutputTriangles.Capacity = math.max(tileOutputTriangles.Capacity, tileOutputTriangles.Length + 3);

									var interpolator = new Polygon.BarycentricTriangleInterpolator(a, b, c);
									for (int i = 0; i < 3; i++) {
										var p = new Int3(triangulationOutputVertices[i].x, 0, triangulationOutputVertices[i].y);
										p.y = interpolator.SampleY(new int2(p.x, p.z));
										tileOutputVertices.Add(p);
									}

									tileOutputTriangles.Add(vertexOffset + 0);
									tileOutputTriangles.Add(vertexOffset + 1);
									tileOutputTriangles.Add(vertexOffset + 2);
									tileOutputTags.Add(tag);
									CopyTriangulationToOutputMarker.End();
								} else {
									MarkerTriangulate.Begin();

									triangulationPositions.Clear();
									constraintEdges.Clear();
									seenVertices.Clear();

									var vertexIndexOffset = 0;
									for (int ki = 0; ki < triangulationVertexCountPerPolygon.Length; ki++) {
										var startIndex = vertexIndexOffset;
										vertexIndexOffset += triangulationVertexCountPerPolygon[ki];
										var endIndex = vertexIndexOffset;

										var prevVertexId = -1;
										var startConstraintId = constraintEdges.Length;
										for (int k = startIndex; k < endIndex; k++) {
											var p = new int2((int)triangulationOutputVertices[k].x, (int)triangulationOutputVertices[k].y);
											int vertexId;
											if (seenVertices.TryGetValue(p, out vertexId)) {
											} else {
												vertexId = triangulationPositions.Length;
												triangulationPositions.Add(p);
												seenVertices.Add(p, vertexId);
											}
											if (prevVertexId != -1) {
												constraintEdges.Add(prevVertexId);
												constraintEdges.Add(vertexId);
											}
											prevVertexId = vertexId;
										}
										// Close the path
										constraintEdges.Add(prevVertexId);
										constraintEdges.Add(constraintEdges[startConstraintId]);
									}
									Assert.IsTrue(triangulationPositions.Length >= 3);

									var input = new andywiecko.BurstTriangulator.LowLevel.Unsafe.InputData<int2>() {
										Positions = triangulationPositions.AsArray(),
										ConstraintEdges = constraintEdges.AsArray(),
									};

									{
										MarkerCore.Begin();
										var args = new andywiecko.BurstTriangulator.LowLevel.Unsafe.Args(
											preprocessor: andywiecko.BurstTriangulator.Preprocessor.None,
											sloanMaxIters: 1_000_000,
											autoHolesAndBoundary: true,
											refineMesh: false,
											restoreBoundary: false,
											validateInput: false,
											verbose: false,
											refinementThresholdAngle: 0,
											refinementThresholdArea: 0
											);
										new UnsafeTriangulator<int2>().Triangulate(input, triangulatorOutput, args, Allocator.Temp);
										MarkerCore.End();
									}

									if (triangulatorOutput.Status.Value.IsError) {
										Debug.LogError("Error during triangulation");
									} else {
										CopyTriangulationToOutputMarker.Begin();
										CopyTriangulationToOutput(ref triangulatorOutput, tileOutputVertices, tileOutputTriangles, tileOutputTags, tag, a, b, c);
										CopyTriangulationToOutputMarker.End();
									}

									MarkerTriangulate.End();
								}
							}
						}
					}
				}

				output[tileIndex] = CompressAndRefineTile(tileOutputVertices, tileOutputTriangles, tileOutputTags, allocator);
			}

			tileOutputVertices.Dispose();
			tileOutputTriangles.Dispose();
			tileOutputTags.Dispose();
		}

		static void FindIntersectingCuts (UnsafeSpan<ContourMeta> contoursMeta, UnsafeSpan<IntBounds> cutBounds, NativeList<int> interestingCuts, NativeList<int> interestingDualCuts, IntBounds triBounds, bool addedGeometry) {
			interestingCuts.Clear();
			interestingDualCuts.Clear();
			triBounds.max += EdgeSnappingMaxDistance;
			triBounds.min -= EdgeSnappingMaxDistance;

			for (int k = 0; k < cutBounds.Length; k++) {
				// Check if the cut potentially intersects the triangle
				if (IntBounds.Intersects(triBounds, cutBounds[k])) {
					if (addedGeometry && !contoursMeta[k].cutsAddedGeom) continue;

					if (contoursMeta[k].isDual) {
						interestingDualCuts.Add(k);
					} else {
						interestingCuts.Add(k);
					}
				}
			}
		}

		static IntBounds TriangleBounds (Int3 a, Int3 b, Int3 c) {
			var mn = (int3)a;
			var mx = (int3)a;
			mn = math.min(mn, (int3)b);
			mn = math.min(mn, (int3)c);
			mx = math.max(mx, (int3)b);
			mx = math.max(mx, (int3)c);
			return new IntBounds(mn, mx + 1);
		}

		static TileMesh.TileMeshUnsafe CompressAndRefineTile (NativeList<Int3> tileOutputVertices, NativeList<int> tileOutputTriangles, NativeList<int> tileOutputTags, Allocator allocator) {
			unsafe {
				MarkerCompress.Begin();
				// This next step will remove all duplicate vertices in the data (of which there are quite a few)
				new MeshUtility.JobRemoveDuplicateVertices {
					vertices = tileOutputVertices,
					triangles = tileOutputTriangles,
					tags = tileOutputTags,
				}.Execute();
				MarkerCompress.End();
			}

			MarkerRefine.Begin();
			var newIndexCount = DelaunayRefinement(tileOutputVertices.AsUnsafeSpan(), tileOutputTriangles.AsUnsafeSpan(), tileOutputTags.AsUnsafeSpan(), true, true);
			tileOutputTriangles.Length = newIndexCount;
			tileOutputTags.Length = newIndexCount / 3;
			MarkerRefine.End();

			return new TileMesh.TileMeshUnsafe {
					   // We return the data as spans that own their data
					   verticesInTileSpace = tileOutputVertices.AsUnsafeSpan().Clone(allocator),
					   triangles = tileOutputTriangles.AsUnsafeSpan().Clone(allocator),
					   tags = tileOutputTags.AsUnsafeSpan().Reinterpret<uint>().Clone(allocator)
			};
		}

		static void CopyTriangulationToOutput (ref OutputData<int2> triangulatorOutput, NativeList<Int3> tileOutputVertices, NativeList<int> tileOutputTriangles, NativeList<int> tileOutputTags, int tag, Int3 a, Int3 b, Int3 c) {
			var vertexOffset = tileOutputVertices.Length;
			var interpolator = new Polygon.BarycentricTriangleInterpolator(a, b, c);
			for (int pi = 0; pi < triangulatorOutput.Positions.Length; pi++) {
				var p = new Int3(triangulatorOutput.Positions[pi].x, 0, triangulatorOutput.Positions[pi].y);
				p.y = interpolator.SampleY(new int2(p.x, p.z));
				tileOutputVertices.Add(p);
			}

			tileOutputTags.AddReplicate(in tag, triangulatorOutput.Triangles.Length / 3);
			for (int ti = 0; ti < triangulatorOutput.Triangles.Length; ti += 3) {
				var t0 = triangulatorOutput.Triangles[ti+0] + vertexOffset;
				var t1 = triangulatorOutput.Triangles[ti+1] + vertexOffset;
				var t2 = triangulatorOutput.Triangles[ti+2] + vertexOffset;

				// Add the triangle with the correct indices
				tileOutputTriangles.Add(t0);
				tileOutputTriangles.Add(t1);
				tileOutputTriangles.Add(t2);
			}
		}

		/// <summary>
		/// Find cut vertices that lie exactly on the polygon edges, and insert them into the polygon.
		///
		/// These vertices will need to be added to the polygon outline before cutting,
		/// to ensure they end up in the final triangulation.
		/// This is because adjacent triangles may have a cut there, and
		/// if this triangle doesn't also get a cut there, connections between
		/// nodes may not be created properly.
		///
		///    /|  /\
		///   / | /  \
		///  /  |/   /
		/// /___| \ /
		///
		/// The EdgeSnappingMaxDistance must be at least 1 (possibly only 0.5) for this to handle all edge cases.
		/// If it is larger, it will nicely prevent too small triangles, but it can also cause issues when snapping very
		/// thin triangles so much that they become invalid. So a value of 1 seems best.
		///
		/// Using this method adds some overhead for cutting, but it is necessary to handle edge cases where
		/// navmesh cuts exactly touch the edges of the triangles.
		/// The overhead seems to be roughly 1% of the total cutting time.
		/// </summary>
		static void SnapEdges (ref NativeArray<Point64Wrapper> triBuffer, ref int vertexCount, UnsafeSpan<NavmeshCut.ContourBurst> contours, ref NativeList<int> interestingCuts, ref NativeArray<Point64Wrapper> contourVerticesP64) {
			for (int next = 0, prev = vertexCount - 1; next < vertexCount; prev = next, next++) {
				var c1 = new int2((int)triBuffer[prev].x, (int)triBuffer[prev].y);
				var c2 = new int2((int)triBuffer[next].x, (int)triBuffer[next].y);
				var dir = c2 - c1;
				var lengthSq = (long)dir.x * (long)dir.x + (long)dir.y * (long)dir.y;
				var baseLength = (long)math.sqrt((double)lengthSq);
				var threshold = baseLength * EdgeSnappingMaxDistance * 2;

				for (int i = 0; i < interestingCuts.Length; i++) {
					var cut = contours[interestingCuts[i]];
					for (int pi = cut.startIndex; pi < cut.endIndex; pi++) {
						var p = new int2((int)contourVerticesP64[pi].x, (int)contourVerticesP64[pi].y);
						// Check if the point is close enough to the (infinite) line
						// Use the fact that the a triangle's area is base * height / 2
						if (math.abs(VectorMath.SignedTriangleAreaTimes2(c1, c2, p)) <= threshold) {
							var pdir = p - c1;
							var dot = (long)pdir.x * (long)dir.x + (long)pdir.y * (long)dir.y;
							// Check if the point is strictly between the start and end points of the segment
							if (dot > 0 && dot < lengthSq) {
								// Insert
								if (triBuffer.Length < vertexCount + 1) {
									// In very rare cases, we may have to resize the buffer to fit all additional vertices
									var newBuffer = new NativeArray<Point64Wrapper>(vertexCount * 2, Allocator.Temp);
									triBuffer.AsUnsafeSpan().CopyTo(newBuffer.AsUnsafeSpan());
									triBuffer.Dispose();
									triBuffer = newBuffer;
								}
								triBuffer.AsUnsafeSpan().Move(next, next + 1, vertexCount - next);
								triBuffer[next] = new Point64Wrapper(p.x, p.y);
								vertexCount++;

								c2 = p;

								// Recalculate the threshold
								dir = c2 - c1;
								lengthSq = (long)dir.x * (long)dir.x + (long)dir.y * (long)dir.y;
								baseLength = (long)math.sqrt((double)lengthSq);
								threshold = baseLength * EdgeSnappingMaxDistance * 2;
							}
						}
					}
				}
			}
		}

		static NativeArray<Point64Wrapper> ConvertTo64BitCoordinates (UnsafeList<float2> vertices) {
			var verticesP64 = new NativeArray<Point64Wrapper>(vertices.Length, Allocator.Temp);
			for (int k = 0; k < vertices.Length; k++) {
				var p = math.round(vertices[k] * Int3.FloatPrecision);
				verticesP64[k] = new Point64Wrapper((long)p.x, (long)p.y);
			}
			return verticesP64;
		}

		static NativeArray<IntBounds> CalculateCutBounds (ref CutCollection cutCollection, ref NativeArray<Point64Wrapper> contourVerticesP64) {
			var cutBounds = new NativeArray<IntBounds>(cutCollection.contours.Length, Allocator.Temp);
			for (int i = 0; i < cutCollection.contours.Length; i++) {
				var contour = cutCollection.contours[i];
				var mn = new int2(int.MaxValue, int.MaxValue);
				var mx = new int2(int.MinValue, int.MinValue);
				for (int j = contour.startIndex; j < contour.endIndex; j++) {
					var p = new int2((int)contourVerticesP64[j].x, (int)contourVerticesP64[j].y);
					mn = math.min(mn, p);
					mx = math.max(mx, p);
				}

				cutBounds[i] = new IntBounds(
					new int3(
						mn.x,
						(int)(contour.ymin * Int3.FloatPrecision),
						mn.y
						),
					new int3(
						mx.x + 1,
						(int)math.ceil(contour.ymax * Int3.FloatPrecision),
						mx.y + 1
						)
					);
			}
			return cutBounds;
		}

		static void AddContours (Clipper64 clipper, ref UnsafeSpan<NavmeshCut.ContourBurst> contours, ref UnsafeSpan<Point64Wrapper> contourVertices, ref UnsafeSpan<int> contourIndices) {
			for (int i = 0; i < contourIndices.Length; i++) {
				var contour = contours[contourIndices[i]];
				var vertices = contourVertices.Slice(contour.startIndex, contour.endIndex - contour.startIndex);
				unsafe {
					clipper.AddPath((Point64*)vertices.ptr, vertices.Length, PathType.Clip);
				}
			}
		}

		static void CopyClipperOutput (List<List<Point64> > closedSolutions, ref UnsafeList<Vector2Int> outputVertices, ref UnsafeList<int> outputVertexCountPerPolygon) {
			outputVertexCountPerPolygon.Length = closedSolutions.Count;
			for (int i = 0; i < closedSolutions.Count; i++) {
				var solution = closedSolutions[i];
				for (int j = 0; j < solution.Count; j++) {
					outputVertices.Add(new Vector2Int((int)solution[j].X, (int)solution[j].Y));
				}
				outputVertexCountPerPolygon[i] = solution.Count;
			}
		}

		[AOT.MonoPInvokeCallback(typeof(CutFunction))]
		static bool CutPolygon (ref UnsafeSpan<Point64Wrapper> subject, ref UnsafeSpan<Point64Wrapper> contourVertices, ref UnsafeSpan<NavmeshCut.ContourBurst> contours, ref UnsafeSpan<int> contourIndices, ref UnsafeSpan<int> contourIndicesDual, ref UnsafeList<Vector2Int> outputVertices, ref UnsafeList<int> outputVertexCountPerPolygon, int mode) {
			// Cache the clipper object to avoid unnecessary allocations.
			// This method may be executed from multiple threads at the same time,
			// so we must ensure that we use different cached clipper objects for different threads.
			var idx = JobsUtility.ThreadIndex;
			var clipper = TileHandlerCache.cachedClippers[idx] = TileHandlerCache.cachedClippers[idx] ?? new Clipper64();
			clipper.PreserveCollinear = true;
			var closedSolutions = ListPool<List<Point64> >.Claim();
			var openSolutions = ListPool<List<Point64> >.Claim();

			// Ideally we'd just send a boolean to this function. But Burst doesn't like doing that. It compiles, but somehow it seems to mess up the value sometimes.
			bool dual = mode == 1;

			if (!dual) {
				// Calculate the difference between the subject polygon and the cut polygons
				clipper.Clear();
				unsafe {
					clipper.AddPath((Point64*)subject.ptr, subject.Length, PathType.Subject);
				}
				AddContours(clipper, ref contours, ref contourVertices, ref contourIndices);
				AddContours(clipper, ref contours, ref contourVertices, ref contourIndicesDual);
			} else {
				// Handle dual cuts
				// 1. Replace the subject with the intersection of subject and dual cuts
				// 2. Calculate the difference between the subject polygon and the cut polygons

				clipper.Clear();
				unsafe {
					clipper.AddPath((Point64*)subject.ptr, subject.Length, PathType.Subject);
				}
				AddContours(clipper, ref contours, ref contourVertices, ref contourIndicesDual);

				if (!clipper.Execute(ClipType.Intersection, FillRule.NonZero, closedSolutions, openSolutions)) return false;

				clipper.Clear();
				for (int i = 0; i < closedSolutions.Count; i++) {
					var solution = closedSolutions[i];
					unsafe {
						if (Clipper.IsPositive(solution)) {
							clipper.AddSubject(solution);
						} else {
							clipper.AddClip(solution);
						}
					}
				}
				AddContours(clipper, ref contours, ref contourVertices, ref contourIndices);

				closedSolutions.Clear();
				openSolutions.Clear();
			}

			bool success = clipper.Execute(ClipType.Difference, FillRule.NonZero, closedSolutions, openSolutions);
			if (!success) return false;

			MarkerCopyClippingResult.Begin();
			CopyClipperOutput(closedSolutions, ref outputVertices, ref outputVertexCountPerPolygon);
			MarkerCopyClippingResult.End();

			ListPool<List<Point64> >.Release(ref closedSolutions);
			ListPool<List<Point64> >.Release(ref openSolutions);
			return true;
		}

		internal static void InitDelegates () {
			if (DelegateGCRoot == null) {
				CutFunction del = CutPolygon;
				// If the delegate is garbage, the pointer becomes invalid.
				// So we keep a reference to the delegate to ensure it is not garbage collected.
				// It doesn't seem to be possible to pin this object (using IL2CPP in a standalone build it will throw "ArgumentException: Object contains references"),
				// but Marshal.GetFunctionPointerForDelegate doesn't mention anything about pinning, so maybe its safe even without it.
				DelegateGCRoot = del;
				CutFunctionPtr.Data = Marshal.GetFunctionPointerForDelegate(del);
			}
		}

		// Burst doesn't seem to like referencing types from the Clipper2 dll, so we create
		// a type here that is identical to the Point64 type in Clipper2
		public struct Point64Wrapper {
			public long x;
			public long y;

			public Point64Wrapper (long x, long y) {
				this.x = x;
				this.y = y;
			}
		}

		/// <summary>
		/// Clips the input polygon against a rectangle with one corner at the origin and one at size in XZ space.
		///
		/// Returns: Number of output vertices
		/// </summary>
		/// <param name="clipIn">Input vertices. Output will also be placed here.</param>
		/// <param name="clipTmp">Temporary vertices. This buffer must be large enough to contain all output vertices.</param>
		/// <param name="size">The clipping rectangle has one corner at the origin and one at this position in XZ space.</param>
		static int ClipAgainstRectangle (UnsafeSpan<Int3> clipIn, UnsafeSpan<Int3> clipTmp, Vector2Int size) {
			int ct;

			var simpleClipper = new Pathfinding.Graphs.Navmesh.Voxelization.Int3PolygonClipper();
			ct = simpleClipper.ClipPolygon(clipIn, 3, clipTmp, 1, 0, 0);
			if (ct == 0)
				return ct;

			ct = simpleClipper.ClipPolygon(clipTmp, ct, clipIn, -1, size.x, 0);
			if (ct == 0)
				return ct;

			ct = simpleClipper.ClipPolygon(clipIn, ct, clipTmp, 1, 0, 2);
			if (ct == 0)
				return ct;

			ct = simpleClipper.ClipPolygon(clipTmp, ct, clipIn, -1, size.y, 2);
			return ct;
		}

		/// <summary>
		/// Refine a mesh using delaunay refinement.
		/// Loops through all pairs of neighbouring triangles and check if it would be better to flip the diagonal joining them
		/// using the delaunay criteria.
		///
		/// Does not require triangles to be clockwise, triangles will be checked for if they are clockwise and made clockwise if not.
		/// The resulting mesh will have all triangles clockwise.
		///
		/// See: https://en.wikipedia.org/wiki/Delaunay_triangulation
		/// </summary>
		static int DelaunayRefinement (UnsafeSpan<Int3> verts, UnsafeSpan<int> tris, UnsafeSpan<int> tags, bool delaunay, bool colinear) {
			if (tris.Length % 3 != 0) throw new System.ArgumentException("Triangle array length must be a multiple of 3");
			if (tags.Length != tris.Length / 3) throw new System.ArgumentException("There must be exactly 1 tag per 3 triangle indices");

			var lookup = new NativeHashMapVector2IntInt(tris.Length, Allocator.Temp);

			static void AddTriangleToLookup (NativeHashMapVector2IntInt lookup, UnsafeSpan<int> tris, int i) {
				lookup[new Vector2Int(tris[i+0], tris[i+1])] = i+2;
				lookup[new Vector2Int(tris[i+1], tris[i+2])] = i+0;
				lookup[new Vector2Int(tris[i+2], tris[i+0])] = i+1;
			}

			for (int i = 0; i < tris.Length; i += 3) {
				if (!VectorMath.IsClockwiseXZ(verts[tris[i]], verts[tris[i+1]], verts[tris[i+2]])) {
					int tmp = tris[i];
					tris[i] = tris[i+2];
					tris[i+2] = tmp;
				}

				AddTriangleToLookup(lookup, tris, i);
			}

			var tCount = tris.Length;
			for (int i = 0, k = 0; i < tCount; i += 3, k++) {
				var tag = tags[k];
				for (int j = 0; j < 3; j++) {
					var il = tris[i+((j+0)%3)];
					var ir = tris[i+((j+1)%3)];
					if (lookup.TryGetValue(new Vector2Int(ir, il), out var opp)) {
						// The vertex which we are using as the viewpoint
						var io = tris[i+((j+2)%3)];
						var po = verts[io];

						// Right vertex of the edge
						var pr = verts[ir];

						// Left vertex of the edge
						var pl = verts[il];

						// Opposite vertex (in the other triangle)
						var popp = verts[tris[opp]];

						var oppTag = tags[opp/3];

						// Only allow flipping if the two adjacent triangles share the same tag
						if (tag != oppTag) continue;

						po.y = 0;
						pr.y = 0;
						pl.y = 0;
						popp.y = 0;

						bool noDelaunay = false;

						if (!VectorMath.RightOrColinearXZ(po, pl, popp) || VectorMath.RightXZ(po, pr, popp)) {
							if (colinear) {
								noDelaunay = true;
							} else {
								continue;
							}
						}

						if (colinear) {
							static void RemoveTriangleWithVertex (int vertexIndex, ref int tCount, UnsafeSpan<int> tris, UnsafeSpan<int> tags, NativeHashMapVector2IntInt lookup) {
								tCount -= 3;
								int root = (vertexIndex/3)*3;
								// Remove the opposite triangle by swapping it with the last triangle
								if (root != tCount) {
									tris[root+0] = tris[tCount+0];
									tris[root+1] = tris[tCount+1];
									tris[root+2] = tris[tCount+2];
									tags[root/3] = tags[tCount/3];
									lookup[new Vector2Int(tris[root+0], tris[root+1])] = root+2;
									lookup[new Vector2Int(tris[root+1], tris[root+2])] = root+0;
									lookup[new Vector2Int(tris[root+2], tris[root+0])] = root+1;

									tris[tCount+0] = 0;
									tris[tCount+1] = 0;
									tris[tCount+2] = 0;
								}
							}
							const int MaxErrorSq = 3 * 3;
							// Check if [o] - [right] - [opposite in other] - is (almost) colinear
							// and if the edge [right]-[o] is not shared, and if the edge [opposite in other]-[right] is not shared
							//
							//         l
							//        /|\
							//       / | \
							//      /  |  \
							//     /   |   \
							//  o /____|r___\ opp
							//
							// If so, we collapse the two triangles like this:
							//
							//         l
							//        / \
							//       /   \
							//      /     \
							//     /       \
							//  o /_________\ r
							// TODO: Can use a faster check for colinearity. We only need to check if its close to the infinite line.
							// Note: We don't have to do the same check for the left vertex, because we will at some point visit the opposite
							// triangle and check the edge from the other side, and then left and right will be swapped.
							if (VectorMath.SqrDistancePointSegmentApproximate(po, popp, pr) <= MaxErrorSq &&
								!lookup.ContainsKey(new Vector2Int(io, ir)) &&
								!lookup.ContainsKey(new Vector2Int(ir, tris[opp]))) {
								// Move right vertex to the other triangle's opposite
								tris[i+((j+1)%3)] = ir = tris[opp];

								RemoveTriangleWithVertex(opp, ref tCount, tris, tags, lookup);

								// Since the above mentioned edges are not shared, we don't need to bother updating them
								// However some need to be updated
								AddTriangleToLookup(lookup, tris, i);
								j--;
								continue;
							}
						}

						if (delaunay && !noDelaunay) {
							// TODO: Can use dot products to improve performance
							float beta = Int3.Angle(pr-po, pl-po);
							float alpha = Int3.Angle(pr-popp, pl-popp);

							if (alpha > (2*Mathf.PI - 2*beta)) {
								// Denaunay condition not holding, refine please
								tris[i+((j+1)%3)] = tris[opp];

								int root = (opp/3)*3;
								int off = opp-root;
								tris[root+((off-1+3) % 3)] = tris[i+((j+2)%3)];

								AddTriangleToLookup(lookup, tris, i);

								lookup[new Vector2Int(tris[root+0], tris[root+1])] = root+2;
								lookup[new Vector2Int(tris[root+1], tris[root+2])] = root+0;
								lookup[new Vector2Int(tris[root+2], tris[root+0])] = root+1;
							}
						}
					}
				}
			}

			return tCount;
		}
#endif
	}
}
