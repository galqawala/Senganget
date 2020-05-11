using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Den.Tools;
using Den.Tools.GUI;
using Den.Tools.Splines;
using Den.Tools.Matrices;

using MapMagic.Nodes;
using MapMagic.Terrains;
using MapMagic.Core;
using MapMagic.Products;

namespace MapMagic.Nodes
{
	public class GeneratorsTester : MonoBehaviour, ISerializationCallbackReceiver
	{
		public Generator gen;
		public ITestInput[] inputs = new ITestInput[0];
		public ITestOutput output;

		public CoordRect pixelRect = new CoordRect(-100,-100,512,512);
		public CoordRect worldRect = new CoordRect(0,0,1000,1000);
		public float worldHeight = 200;
		public int margins = 0;

		public Noise random = new Noise(12345, permutationCount:65536);

		public bool testOnSceneChange;
		public bool testOnGuiChange;
		public double benchmarkTime;

		public bool guiInputs;
		public bool guiGen;
		public bool guiOutput;

		public int benchmarkIterations = 5;
		public int benchmarkApproaches = 5;
		public double benchmarkAvgIterationTime;
		public double benchmarkMinIterationTime;
		public double benchmarkTotalTime;
		public double benchmarkPerMegapixelTime;
		
		#region Test

			public void Test ()
			{
				TileData data = new TileData();
				data.products = new ProductsDataMockup();
				data.area = new Area(worldRect, pixelRect, margins);
				data.random = random;

				data.globals = new Globals();
				data.globals.height = worldHeight;

				ReadInputs(gen, data);
				
				gen.Generate(data, null);

				object result = data.products[(IOutlet<object>)gen];

				if (output != null)
					output.Write(result);
			}


			public void Benchmark ()
			{
				double totalTime = 0;
				double minTime = double.MaxValue;
				for (int a=0; a<benchmarkApproaches; a++)
				{
					TileData[] datas = new TileData[benchmarkIterations];
					for (int i=0; i<benchmarkIterations; i++)
					{
						datas[i] = new TileData();
						datas[i].area = new Area(worldRect, pixelRect, 0);
						datas[i].random = random;

						datas[i].globals = new Globals();
						datas[i].globals.height = worldHeight;

						ReadInputs(gen, datas[i]);
					}

					System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
					timer.Start();

					for (int i=0; i<benchmarkIterations; i++)
						gen.Generate(datas[i], null);

					timer.Stop();
					totalTime += timer.ElapsedMilliseconds;
					if (timer.ElapsedMilliseconds<minTime)
						minTime = timer.ElapsedMilliseconds;

					object result = datas[datas.Length-1].products[(IOutlet<object>)gen];
					if (output != null)
						output.Write(result);
				}

				benchmarkTotalTime = totalTime;
				benchmarkAvgIterationTime = (totalTime / benchmarkApproaches) / benchmarkIterations;
				benchmarkMinIterationTime = minTime / benchmarkIterations;
				benchmarkPerMegapixelTime = benchmarkMinIterationTime / (1f*pixelRect.Count / 1000000);
			}


			public void ReadInputs (Generator gen, TileData data)
			/// reads this inputs and stores results to data
			{
				int i = 0;

				if (gen is IInlet<object> inletGen)
				{
					object product = inputs[i].Read(worldRect, worldHeight, pixelRect);
					(data.products as ProductsDataMockup)[inletGen] = product;
				}

				if (gen is IMultiInlet multiInletGen)
					foreach (IInlet<object> inlet in multiInletGen.Inlets())
					{
						if (i>=inputs.Length || inputs[i] == null) continue;
						object product = inputs[i].Read(worldRect, worldHeight, pixelRect);
						(data.products as ProductsDataMockup)[inlet] = product;

						i++;
						if (i>=inputs.Length) break;
					}
			}


			public static void DebugSplats (Matrix matrix) 
				{ DebugSplats(matrix, Color.gray, Color.red); }

			public static void DebugSplats (Matrix matrix, Color backColor, Color foreColor)
			/// Draws gray/red mask on the first terrain found
			{
				Terrain terrain = FindObjectOfType<Terrain>();
				if (terrain == null) return;

				TerrainData data = terrain.terrainData;

				data.terrainLayers = new TerrainLayer[2];
				data.terrainLayers[0] = new TerrainLayer() { diffuseTexture=TextureExtensions.ColorTexture(2,2,backColor) };
				data.terrainLayers[1] = new TerrainLayer() { diffuseTexture=TextureExtensions.ColorTexture(2,2,foreColor) };

				Matrix invertedMatrix = new Matrix(matrix);
				invertedMatrix.InvertOne();

				data.alphamapResolution = matrix.rect.size.x;
				matrix.ExportTerrainData(data,1);
				invertedMatrix.ExportTerrainData(data,0);
			}

		#endregion


		#region Inputs/Outputs

			public interface ITestInput
			{  
				object Read(CoordRect worldRect, float worldHeight, CoordRect pixelRect); 
			}

			public interface ITestOutput
			{  
				void Write(object result);
			}

			[System.Serializable]
			public class TextureInout : ITestInput, ITestOutput
			{
				[Val("Texture")] public Texture2D texture;

				public bool preview = false;
				//public ScrollZoom previewScrollZoom = new ScrollZoom() { maxZoom=100, minZoom = 0.01f };
				public float previewZoom = 1;
				public Vector2 previewScroll = new Vector2();

				public bool previewInScene = false;
				public GameObject previewObj;
				public Material previewMat;

				public object Read (CoordRect worldRect, float worldHeight, CoordRect pixelRect)
				{
					CoordRect texRect = new CoordRect( 0,0,texture.width, texture.height);
					MatrixWorld texMatrix = new MatrixWorld(texRect, worldRect.offset.vector2d, worldRect.size.vector2d, worldHeight);
					texMatrix.ImportTexture(texture, 0);

					//resizing if pixel rect differs
					MatrixWorld testMatrix;
					if (texMatrix.rect.size != pixelRect.size)
					{
						testMatrix = new MatrixWorld(pixelRect, worldRect.offset.vector2d, worldRect.size.vector2d, worldHeight);
						MatrixOps.Resize(texMatrix, testMatrix);
					}
					else
						testMatrix = texMatrix;
					
					return testMatrix;
				}

				public void Write (object result)
				{
					MatrixWorld matrix = (MatrixWorld)result;
					matrix.ExportTextureRaw(texture);

					if (previewInScene)
					{
						if (previewObj==null) CreateSceneObject();
						previewObj.transform.position = matrix.worldPos + matrix.worldSize/2;
						previewObj.transform.localScale = new Vector3(matrix.worldSize.x, matrix.worldSize.z, 1);
						previewMat.SetTexture("_MainTex", texture);
					}
				}



				public void CreateSceneObject ()
				{
					previewObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
					previewObj.transform.rotation = Quaternion.Euler(90,0,0);
					Shader shader = Shader.Find("Unlit/Texture");
					previewMat = new Material(shader);
					previewObj.GetComponent<MeshRenderer>().sharedMaterial = previewMat;
				}
			}
 

			[System.Serializable]
			public class ObjectsIn : ITestInput
			{
				[Val("Parent", allowSceneObject = true)] public Transform parent;
				[Val("Resolution")] public int resolution = 64;
				//[Val("Starting Id")] public int startId = 0;

				public object Read (CoordRect worldRect, float worldHeight, CoordRect pixelRect)
				{
					int childCount = parent.childCount;
					TransitionsList trns = new TransitionsList();

					for (int c=0; c<childCount; c++)
					{
						Transform child = parent.GetChild(c);
						Transition trs = new Transition(child);
						trns.Add(trs);
					}

					return trns;
				}
			}


			[System.Serializable]
			public class ObjectsOut : ITestOutput
			{
				[Val("Parent", allowSceneObject = true)] public Transform parent;
				[Val("Object Prefab")] public Transform objPrefab;

				public void Write (object result)
				{
					TransitionsList trns = (TransitionsList)result;
					int childCount = parent.childCount;

					int c=0;
					for (int t=0; t<trns.count; t++)
					{
						Transform tfm;
						if (c >= childCount)
						{
							if (objPrefab == null) tfm = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
							else tfm = GameObject.Instantiate(objPrefab);
						}
						else
							tfm = parent.GetChild(c);

						tfm.position = trns.arr[t].pos;
						tfm.localScale = trns.arr[t].scale;
						tfm.rotation = trns.arr[t].rotation;

						tfm.parent = parent;

						c++;
					}
				}
			}



			[System.Serializable]
			public class GizmosOut : ITestOutput
			{
				[Val("Color")] public Color color = Color.red;
				[Val("Size")] public float size = 1;

				[NonSerialized] private TransitionsList trns;

				public void Write (object result)
				{
					trns = (TransitionsList)result;
					
					#if UNITY_EDITOR
					#if UNITY_2019_1_OR_NEWER
					UnityEditor.SceneView.duringSceneGui -= DrawGizmos;
					UnityEditor.SceneView.duringSceneGui += DrawGizmos;
					#else
					UnityEditor.SceneView.onSceneGUIDelegate -= DrawGizmos;
					UnityEditor.SceneView.onSceneGUIDelegate += DrawGizmos;
					#endif
					#endif
				}

				#if UNITY_EDITOR
				public void DrawGizmos (UnityEditor.SceneView sceneView)
				{
					UnityEditor.Handles.color = color;

					for (int t=0; t<trns.count; t++)
					{
						Vector3 pos = trns.arr[t].pos;
						Vector3 scale = trns.arr[t].scale;  

						UnityEditor.Handles.CubeHandleCap(0, pos, Quaternion.identity, size, EventType.Repaint);
					}

					/*UnityEditor.Handles.color = new Color(color.r, color.g, color.b, color.a*0.1f);
					for (int c=0; c<posTab.cells.arr.Length; c++)
					{
						CoordRect rect = posTab.cells.arr[c].rect;
						UnityEditor.Handles.DrawLine(rect.offset.vector3, (rect.offset+new Coord(0, rect.size.z)).vector3);
						UnityEditor.Handles.DrawLine((rect.offset+new Coord(0, rect.size.z)).vector3, (rect.offset+new Coord(rect.size.x, rect.size.z)).vector3);
						UnityEditor.Handles.DrawLine((rect.offset+new Coord(rect.size.x, rect.size.z)).vector3, (rect.offset+new Coord(rect.size.x, 0)).vector3);
						UnityEditor.Handles.DrawLine((rect.offset+new Coord(rect.size.x, 0)).vector3, (rect.offset).vector3);
					}*/
				}
				#endif
			}


			[System.Serializable]
			public class SplineInout : ITestInput, ITestOutput
			{
				[Val("Spline", allowSceneObject=true)] public SplineObject splineObj; 

				public object Read (CoordRect worldRect, float worldHeight, CoordRect pixelRect)
				{
					return splineObj.splineSys;
				}

				public void Write (object result)
				{
					splineObj.splineSys = (SplineSys)result;
				}
			}


			[System.Serializable]
			public class TerrainInout : ITestInput, ITestOutput
			{
				[Val("Terrain", allowSceneObject=true)] public Terrain terrain;

				public object Read (CoordRect worldRect, float worldHeight, CoordRect pixelRect)
				{
					MatrixWorld matrix = new MatrixWorld(pixelRect, worldRect.offset.vector2d, worldRect.size.vector2d, worldHeight);

					float[,] arr2D = terrain.terrainData.GetHeights(0,0,terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution);
					matrix.ImportHeights(arr2D);

					return matrix;
				}

				public void Write (object result)
				{
					MatrixWorld matrix = (MatrixWorld)result;
					if (terrain == null) return;

					int terrainRes = Mathf.NextPowerOfTwo(matrix.rect.size.x-1) + 1;
					float resFactor = 1f * terrainRes / matrix.rect.size.x;

					if (terrain.terrainData.heightmapResolution != terrainRes)
						terrain.terrainData.heightmapResolution = terrainRes;

					terrain.transform.position = matrix.worldPos;
					terrain.terrainData.size = new Vector3(matrix.worldSize.x*resFactor, matrix.worldSize.y, matrix.worldSize.z*resFactor); //since terrainRes is usually about 2 times bigger

					float[,] arr2D = new float[terrainRes, terrainRes];
					matrix.ExportHeights(arr2D);
					terrain.terrainData.SetHeights(0,0,arr2D);
				}
			}


			[System.Serializable]
			public class SplatsInout : ITestInput, ITestOutput
			{
				//[Val("Terrain", allowSceneObject = true)] 
				[Val("Terrain", allowSceneObject=true)] public Terrain terrain;
				[Val("Channel")] public int channel;

				public object Read (CoordRect worldRect, float worldHeight, CoordRect pixelRect)
				{
					MatrixWorld matrix = new MatrixWorld(pixelRect, worldRect.offset.vector2d, worldRect.size.vector2d, worldHeight);
					matrix.ImportData(terrain.terrainData, channel);
					return matrix;
				}

				public void Write (object result)
				{
					MatrixWorld matrix = (MatrixWorld)result;
					matrix.ExportTerrainData(terrain.terrainData, channel);
				}
			}


			[System.Serializable]
			public class ScatterIn : ITestInput
			{
				[Val("Count")] public int count;
				[Val("Uniformity")] public float uniformity;
				[Val("Seed")] public int seed = 12345;
				[Val("Relax")] public float relax = 0.1f;

				public object Read (CoordRect worldRect, float worldHeight, CoordRect pixelRect)
				{
					Vector3 worldPos = worldRect.offset.vector3;
					Vector3 worldSize = worldRect.size.vector3;
					CoordRect rect = CoordRect.WorldToGridRect(ref worldPos, ref worldSize, (int)Mathf.Sqrt(count));

					Noise random = new Noise(seed, permutationCount:65536);

					PositionMatrix posMatrix = new PositionMatrix(rect, worldPos, worldSize);
					posMatrix.Scatter(uniformity, random);
					posMatrix = posMatrix.Relaxed(relax);

					return posMatrix.ToTransitionsList();
				}
			}

		#endregion


		#region Create Test Case

			public static void CreateTestCase (Generator gen, TileData data)
			{
				if (data == null)
					throw new Exception("Could not find products: generate the graph before creating a test");

				GameObject go = new GameObject();
				go.name = gen.GetType().Name + " Test";
				GeneratorsTester tester = go.AddComponent<GeneratorsTester>();

				tester.gen = (Generator)Serializer.DeepCopy(gen); //to avoid changing graph when fiddling with tester

				tester.pixelRect = data.area.full.rect;
				tester.worldRect = new CoordRect((int)data.area.full.worldPos.x, (int)data.area.full.worldPos.z, (int)data.area.full.worldSize.x, (int)data.area.full.worldSize.z);
				tester.worldHeight = data.globals.height;

				//creating inputs
				List<ITestInput> testInputs = new List<ITestInput>();

				if (gen is IInlet<object> inletGen)
				{
					GameObject ingo = new GameObject();
					ingo.transform.parent = go.transform;
					ingo.name = inletGen.GetType().Name + " Input";

					object product = data.products.ReadInlet(inletGen);
					//testInputs.Add( CreateTestInput(ingo, (dynamic)inletGen, (dynamic)product) );
					testInputs.Add( CreateTestInput(ingo, inletGen, product) );
				}

				if (gen is IMultiInlet multiInletGen)
					foreach (IInlet<object> inlet in multiInletGen.Inlets())
					{
						GameObject ingo = new GameObject();
						ingo.transform.parent = go.transform;
						ingo.name = inlet.GetType().Name + " Input";

						object product = data.products.ReadInlet(inlet);
						//testInputs.Add( CreateTestInput(ingo, (dynamic)inlet, (dynamic)product) );
						testInputs.Add( CreateTestInput(ingo, inlet, product) );
					}

				tester.inputs = testInputs.ToArray();

				//creating output
				GameObject outgo = new GameObject(); 
				outgo.transform.parent = go.transform;
				outgo.name = "Output";

				tester.output = CreateTestOutput(outgo, (IOutlet<object>)gen); //CreateTestOutput(outgo, (dynamic)gen);
			}


			public static ITestInput CreateTestInput (GameObject go, IInlet<object> inlet, object product)
			{
				if (inlet is IInlet<MatrixWorld> mInlet) //checking inlet class, not product: product could be null
					return CreateTestInput(go, mInlet, (MatrixWorld)product);

				if (inlet is IInlet<TransitionsList> tInlet) 
					return CreateTestInput(go, tInlet,(TransitionsList)product);

				if (inlet is IInlet<SplineSys> sInlet) 
					return CreateTestInput(go, sInlet, (SplineSys)product);

				Debug.Log("Create Test: Skipping inlet of unknown type"); 
				return null;
			}

			public static ITestInput CreateTestInput (GameObject go, IInlet<MatrixWorld> inlet, MatrixWorld matrix)
			{
				Terrain terrain = CreateTerrain(go); 

				int terrainRes = Mathf.NextPowerOfTwo(matrix.rect.size.x-1) + 1;
				float resFactor = 1f * terrainRes / matrix.rect.size.x;

				terrain.terrainData.heightmapResolution = terrainRes;
				float[,] arr2D = new float[terrainRes, terrainRes];
				matrix.ExportHeights(arr2D);
				terrain.terrainData.SetHeights(0,0,arr2D);

				terrain.transform.position = matrix.worldPos;
				terrain.terrainData.size = new Vector3(matrix.worldSize.x*resFactor, matrix.worldSize.y, matrix.worldSize.z*resFactor); //since terrainRes is usually about 2 times bigger

				TerrainInout terrIn = new TerrainInout() { terrain = terrain };
				return terrIn;
			}

			public static ITestInput CreateTestInput (GameObject go, IInlet<TransitionsList> inlet, TransitionsList trsList)
			{
				Mesh mesh = new Mesh();
				mesh.vertices = new Vector3[] {new Vector3 (-.5f, -.5f, -.5f), new Vector3 (.5f,-.5f,-.5f), new Vector3 (.5f,.5f,-.5f), new Vector3 (-.5f,.5f,-.5f), new Vector3 (-.5f,.5f,.5f), new Vector3 (.5f,.5f,.5f), new Vector3 (.5f,-.5f,.5f), new Vector3 (-.5f,-.5f,.5f) };
				mesh.triangles = new int[] {0, 2, 1, 0, 3, 2, 2, 3, 4, 2, 4, 5, 1, 2, 5, 1, 5, 6, 0, 7, 4, 0, 4, 3, 5, 4, 7, 5, 7, 6, 0, 6, 7, 0, 1, 6 };

				for (int t=0; t<trsList.count; t++)
				{
					GameObject trsGo = new GameObject();
					MeshRenderer renderer = trsGo.AddComponent<MeshRenderer>();
					MeshFilter filter = trsGo.AddComponent<MeshFilter>();
					filter.mesh = mesh;
					trsGo.transform.localScale = new Vector3(5,5,5);
					trsGo.name = "Object" + t;
					trsGo.transform.position = trsList.arr[t].pos;
					trsGo.transform.parent = go.transform;
				}

				ObjectsIn objsIn = new ObjectsIn() { parent = go.transform };
				return objsIn;
			}

			public static ITestInput CreateTestInput (GameObject go, IInlet<SplineSys> inlet, SplineSys splineSys)
			{
				SplineObject splineObj = go.AddComponent<SplineObject>();
				splineObj.splineSys = splineSys;
				
				SplineInout splineIn = new SplineInout() { splineObj = splineObj };
				return splineIn;
			}



			public static ITestOutput CreateTestOutput (GameObject go, IOutlet<object> outlet)
			{
				if (outlet is IOutlet<MatrixWorld> mOutlet) 
					return CreateTestOutput(go, mOutlet);

				if (outlet is IOutlet<TransitionsList> tOutlet) 
					return CreateTestOutput(go, tOutlet);

				if (outlet is IOutlet<SplineSys> sOutlet) 
					return CreateTestOutput(go, sOutlet);

				Debug.Log("Create Test: Skipping outlet of unknown type"); 
				return null;
			}

			public static ITestOutput CreateTestOutput (GameObject go, IOutlet<MatrixWorld> outlet)
			{
				Terrain terrain = CreateTerrain(go);
				TerrainInout terrOut = new TerrainInout() { terrain = terrain };
				return terrOut;
			}

			public static ITestOutput CreateTestOutput (GameObject go, IOutlet<TransitionsList> outlet)
			{
				return new GizmosOut();
			}

			public static ITestOutput CreateTestOutput (GameObject go, IOutlet<SplineSys> outlet)
			{
				SplineObject splineObj = go.AddComponent<SplineObject>();
				splineObj.splineSys = new SplineSys();
				
				SplineInout splineIn = new SplineInout() { splineObj = splineObj };
				return splineIn;
			}



			private static Terrain CreateTerrain (GameObject go)
			{
				Terrain terrain = go.AddComponent<Terrain>();
				terrain.terrainData = new TerrainData();
				terrain.heightmapPixelError = 1;

				Shader shader = Shader.Find("Nature/Terrain/Standard");
				terrain.materialTemplate = new Material(shader);

				TerrainLayer terrainLayer = new TerrainLayer();
				terrainLayer.diffuseTexture = new Texture2D(1,1);
				terrainLayer.diffuseTexture.SetPixel(0,0,Color.gray);
				terrainLayer.diffuseTexture.Apply();
				terrain.terrainData.terrainLayers = new TerrainLayer[] {terrainLayer};
				terrain.terrainData.alphamapResolution = 1;
				terrain.terrainData.SetAlphamaps(0,0, new float[,,] {{{1}}});

				return terrain;
			}

		#endregion


		#region Products Data Mockup

			private class ProductsDataMockup : ProductsData
			/// The products data that allows storing products per-inlet
			{
				private Dictionary<IOutlet<object>,object> outletProducts = new Dictionary<IOutlet<object>,object>();
				private Dictionary<IInlet<object>,object> inletProducts = new Dictionary<IInlet<object>,object>();

				public new object this[IOutlet<object> outlet]
				{
					get 
					{
						if (outlet == null) return null;
						if (outletProducts.TryGetValue(outlet, out object obj)) return obj;
						return null;
					}

					set 
					{
						if (outletProducts.ContainsKey(outlet)) outletProducts[outlet] = value;
						else outletProducts.Add(outlet, value);
					}
				}


				//renamed to ReadInlet
				public object this[IInlet<object> inlet]
				{
					get 
					{
						if (inlet == null) return null;
						if (inletProducts.TryGetValue(inlet, out object obj)) return obj;
						return null;
					}

					set 
					{
						if (inletProducts.ContainsKey(inlet)) inletProducts[inlet] = value;
						else inletProducts.Add(inlet, value);
					}
				}

				public override T ReadInlet<T> (IInlet<T> inlet) 
				{
					if (inlet == null) return null;
					if (inletProducts.TryGetValue(inlet, out object obj)) return (T)obj;
					return null;
				}
			}

		#endregion

		#region Serialization

		// To prevent re-setuping generator after each script compile

		public Serializer.Object[] serializedGen = new Serializer.Object[0];
			public Serializer.Object[] serializedInputs = new Serializer.Object[0];
			public Serializer.Object[] serializedOutput = new Serializer.Object[0];

			public void OnBeforeSerialize ()
			{ 
				serializedGen = Serializer.Serialize(gen);
				serializedInputs = Serializer.Serialize(inputs);
				serializedOutput = Serializer.Serialize(output);
			}

			public void OnAfterDeserialize () 
			{
				gen = (Generator)Serializer.Deserialize(serializedGen);
				inputs = (ITestInput[])Serializer.Deserialize(serializedInputs);
				output = (ITestOutput)Serializer.Deserialize(serializedOutput);
			}

		#endregion
	}
}
