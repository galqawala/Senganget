using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Den.Tools;
using Den.Tools.GUI;
using Den.Tools.Matrices;
using MapMagic.Core;
using MapMagic.Products;

namespace MapMagic.Nodes.MatrixGenerators
{
	[System.Serializable]
	[GeneratorMenu (menu="Map/Initial", name ="Constant", iconName="GeneratorIcons/Constant", disengageable = true, helpLink ="https://gitlab.com/denispahunov/mapmagic/wikis/map_generators/constant")]
	public class Constant200 : Generator, IOutlet<MatrixWorld>
	{
		[Val("Level")] public float level;

		public override void Generate (TileData data, StopToken stop) 
		{
			MatrixWorld matrix = new MatrixWorld(data.area.full.rect, data.area.full.worldPos, data.area.full.worldSize, data.globals.height);
			matrix.Fill(level);
			data.products[this] = matrix;
		}
	}


	[System.Serializable]
	//[GeneratorMenu (menu="Map/Test", name ="Weld Tester", icon="GeneratorIcons/Constant", disengageable = true, helpLink ="https://gitlab.com/denispahunov/mapmagic/wikis/map_generators/constant")]
	public class WeldTester200 : Generator, IOutlet<MatrixWorld>
	{
		[Val("Max level")] public float level;

		public override void Generate (TileData data, StopToken stop) 
		{
			MatrixWorld matrix = new MatrixWorld(data.area.full.rect, data.area.full.worldPos, data.area.full.worldSize, data.globals.height);
			matrix.Fill(level * data.area.Coord.x * data.area.Coord.z);
			data.products[this] = matrix;
		}
	}


	[System.Serializable]
	[GeneratorMenu (menu="Map/Initial", name ="Noise", iconName="GeneratorIcons/Noise", disengageable = true, helpLink ="https://gitlab.com/denispahunov/mapmagic/wikis/map_generators/noise")]
	public class Noise200 : Generator, IOutlet<MatrixWorld>
	{
		public enum Type { Unity=0, Linear=1, Perlin=2, Simplex=3 };
		[Val("Type")] public Type type = Type.Perlin;

		[Val("Seed")]		public int seed = 12345;
		[Val("Intensity")]	public float intensity = 1f;
		[Val("Size")]		public float size = 200f;
		[Val("Detail")]		public float detail = 0.5f;
		[Val("Turbulence")] public float turbulence = 0f;
		[Val("Offset")]		public Vector2D offset = new Vector2D(0,0);
		

		public override void Generate (TileData data, StopToken stop) 
		{
			MatrixWorld matrix = new MatrixWorld(data.area.full.rect, data.area.full.worldPos, data.area.full.worldSize, data.globals.height);
			Noise noise = new Noise(data.random, seed);

            #if MM_NATIVE
			if (type!=Type.Unity) //obviously, no native for unity noise
				GeneratorNoise200(matrix, noise, stop,
					(int)type, intensity, size, detail, turbulence, offset.x, offset.z, 
					matrix.worldPos.x, matrix.worldPos.z, matrix.worldSize.x, matrix.worldSize.z);
			else
				Noise(matrix, noise, stop, (int)type, intensity, size, detail, turbulence, offset);
			#else
			Noise(matrix, noise, stop, (int)type, intensity, size, detail, turbulence, offset);
	        #endif

			data.products[this] = matrix;
		}

		[DllImport("NativePlugins", CallingConvention = CallingConvention.Cdecl, EntryPoint = "GeneratorNoise200")]
		private static extern float GeneratorNoise200(Matrix matrix, Noise noise, StopToken stop,
	        int type, float intensity, float size, float detail, float turbulence, float offsetX, float offsetZ,
			float worldRectPosX, float worldRectPosZ, float worldRectSizeX, float worldRectSizeZ);

		private static void Noise (MatrixWorld matrix, Noise noise, StopToken stop,
			int type, float intensity, float size, float detail, float turbulence, Vector2D offset)
		{
			int iterations = (int)Mathf.Log(size,2) + 1; //+1 max size iteration

			Coord min = matrix.rect.Min; Coord max = matrix.rect.Max;
			for (int x=min.x; x<max.x; x++)
			{
				for (int z=min.z; z<max.z; z++)
				{ 
					Vector2D relativePos = new Vector2D (
						(float)(x - matrix.rect.offset.x) / (matrix.rect.size.x-1),
						(float)(z - matrix.rect.offset.z) / (matrix.rect.size.z-1) );

					Vector2D worldPos = new Vector2D (
						relativePos.x*matrix.worldSize.x + matrix.worldPos.x,
						relativePos.z*matrix.worldSize.z + matrix.worldPos.z );

				    float val = noise.Fractal(worldPos.x+offset.x, worldPos.z+offset.z, size, iterations, detail,turbulence,(int)type);
					val *= intensity;

					if (val < 0) val = 0; //before mask?
					if (val > 1) val = 1;

					matrix[x,z] += val;
				}

				if (stop!=null && stop.stop) return; //checking stop every x line
			}
		}
	}


	[System.Serializable]
	[GeneratorMenu (menu="Map/Initial", name ="Voronoi", iconName="GeneratorIcons/Voronoi", disengageable = true, helpLink ="https://gitlab.com/denispahunov/mapmagic/wikis/map_generators/voronoi")]
	public class Voronoi200 : Generator, IOutlet<MatrixWorld>
	{
		[Val("Intensity")]	public float intensity = 1f;
		[Val("Cell Size")]	public int cellSize = 50;
		[Val("Uniformity")]	public float uniformity = 0;
		[Val("Seed")]	public int seed = 12345;
		public enum BlendType { flat, closest, secondClosest, cellular, organic }
		[Val("Blend Type")]	public BlendType blendType = BlendType.cellular;


		public override void Generate (TileData data, StopToken stop) 
		{
			MatrixWorld matrix = new MatrixWorld(data.area.full.rect, data.area.full.worldPos, data.area.full.worldSize, data.globals.height);
			Voronoi(matrix, new Noise(data.random,seed), stop);
			data.products[this] = matrix;
		}


		public void Voronoi (MatrixWorld matrix, Noise random, StopToken stop=null)
		{
			Vector3 matrixPos = matrix.worldPos;
			Vector3 matrixSize = matrix.worldSize;
			CoordRect rect = CoordRect.WorldToGridRect(ref matrixPos, ref matrixSize, cellSize);

			rect.offset -= 1; rect.size += 2; //leaving 1-cell margins
			matrixPos.x -= cellSize; matrixPos.z -= cellSize;
			matrixSize.x += cellSize*2; matrixSize.z += cellSize*2;
				
			PositionMatrix posMatrix = new PositionMatrix(rect, matrixPos, matrixSize);

			posMatrix.Scatter(uniformity, random);
			posMatrix = posMatrix.Relaxed();

			float relativeIntensity = intensity * (matrix.worldSize.x / cellSize) * 0.05f;

			Coord min = matrix.rect.Min; Coord max = matrix.rect.Max; 
			for (int x=min.x; x<max.x; x++)
			{
				if (stop!=null && stop.stop) return; //checking stop every x line

				for (int z=min.z; z<max.z; z++)
				{
					//Vector3 worldPos = matrix.PixelToWorld(x,z);

					Vector2D relativePos = new Vector2D (
						(float)(x - matrix.rect.offset.x) / (matrix.rect.size.x-1),
						(float)(z - matrix.rect.offset.z) / (matrix.rect.size.z-1) );

					Vector2D worldPos = new Vector2D (
						relativePos.x*matrix.worldSize.x + matrix.worldPos.x,
						relativePos.z*matrix.worldSize.z + matrix.worldPos.z );

					Vector3 closest; Vector3 secondClosest;
					float minDist; float secondMinDist;
					posMatrix.GetTwoClosest(worldPos, out closest, out secondClosest, out minDist, out secondMinDist); 

					float val = 0;
					switch (blendType)
					{
						case BlendType.flat: val = closest.y; break;
						case BlendType.closest: val = minDist / (matrix.worldSize.x*16); break;  //(MapMagic.instance.resolution*16); //TODO: why 16?
						case BlendType.secondClosest: val = secondMinDist / (matrix.worldSize.x*16); break;
						case BlendType.cellular: val = (secondMinDist-minDist) / (matrix.worldSize.x*16); break;
						case BlendType.organic: val = (secondMinDist+minDist)/2 / (matrix.worldSize.x*16); break;
					}

					matrix[x,z] += val*relativeIntensity;
				}
			}
		}
	}

	[System.Serializable]
	[GeneratorMenu (menu="Map/Initial", name ="Simple Form", iconName="GeneratorIcons/SimpleForm", disengageable = true, helpLink ="https://gitlab.com/denispahunov/mapmagic/wikis/map_generators/simple_form")]
	public class SimpleForm200 : Generator, IOutlet<MatrixWorld>, ISceneGizmo
	{
		public enum FormType { GradientX, GradientZ, Pyramid, Cone }
		[Val("Type")]		public FormType type = FormType.Cone;
		[Val("Intensity")]	public float intensity = 1;
		[Val("Scale")]		public float scale = 1;
		[Val("Offset")]	public UnityEngine.Vector2 offset;
		[Val("Wrap")]		public CoordRect.TileMode wrap = CoordRect.TileMode.Tile;


		public bool hideDefaultToolGizmo { get; set; }
		public bool drawOffsetScaleGizmo = false;

		public override void Generate (TileData data, StopToken stop) 
		{
			MatrixWorld matrix = new MatrixWorld(data.area.full.rect, data.area.full.worldPos, data.area.full.worldSize, data.globals.height);
			SimpleForm(matrix, offset, scale * data.area.active.worldSize, stop); //size is chunk-size relative
			matrix.Clamp01();
			data.products[this] = matrix;
		}

		public void SimpleForm (MatrixWorld matrix, Vector2 formOffset, Vector2 formSize, StopToken stop=null)
		{
			Vector2 center = formSize/2 + formOffset;
			float radius = Mathf.Min(formSize.x,formSize.y) / 2f;

			Coord min = matrix.rect.Min; Coord max = matrix.rect.Max;

			for (int x=min.x; x<max.x; x++)
			{
				if (stop!=null && stop.stop) return;

				for (int z=min.z; z<max.z; z++)
				{
					//Vector3 worldPos = matrix.PixelToWorld(x,z);

					Vector2D relativePos = new Vector2D (
						(float)(x - matrix.rect.offset.x) / (matrix.rect.size.x-1),
						(float)(z - matrix.rect.offset.z) / (matrix.rect.size.z-1) );

					Vector2D worldPos = new Vector2D (
						relativePos.x*matrix.worldSize.x + matrix.worldPos.x,
						relativePos.z*matrix.worldSize.z + matrix.worldPos.z );

					Vector2 formPos = Tile(worldPos, formOffset, formSize, wrap);

					float val = 0;
					switch (type)
					{
						case FormType.GradientX:
							val = (formPos.x-formOffset.x) / formSize.x;
							break;
						case FormType.GradientZ:
							val = (formPos.y-formOffset.y) / formSize.y;
							break;
						case FormType.Pyramid:
							float valX = (formPos.x-formOffset.x) / formSize.x; if (valX > 1-valX) valX = 1-valX;
							float valZ = (formPos.y-formOffset.y) / formSize.y; if (valZ > 1-valZ) valZ = 1-valZ;
							val = valX<valZ? valX*2 : valZ*2;
							break;
						case FormType.Cone:
							val = 1 - ((center-formPos).magnitude)/radius;
							if (val<0) val = 0;
							break;
					}

					matrix[x,z] = val*intensity;
				}
			}
		}

		public Vector2 Tile (Vector2D pos, Vector2D rectOffset, Vector2D rectSize, CoordRect.TileMode tileMode)
		/// Tiling pos in a 0-size rect
		/// Similar to CoordRect.Tile
		{
			pos.x -= rectOffset.x; //tile requires 0-based coordinates
			pos.z -= rectOffset.z;

			switch (tileMode)
			{
				case CoordRect.TileMode.Clamp:
					if (pos.x < 0) pos.x = 0; 
					if (pos.x >= rectSize.x) pos.x = rectSize.x - 1;
					if (pos.z < 0) pos.z = 0; 
					if (pos.z >= rectSize.z) pos.z = rectSize.z - 1;
					break;

				case CoordRect.TileMode.Tile:
					pos.x = pos.x % rectSize.x; 
					if (pos.x < 0) pos.x= rectSize.x + pos.x;
					pos.z = pos.z % rectSize.z; 
					if (pos.z < 0) pos.z= rectSize.z + pos.z;
					break;

				case CoordRect.TileMode.PingPong:
					pos.x = pos.x % (rectSize.x*2); 
					if (pos.x < 0) pos.x = rectSize.x*2 + pos.x; 
					if (pos.x >= rectSize.x) pos.x = rectSize.x*2 - pos.x - 1;

					pos.z = pos.z % (rectSize.z*2); 
					if (pos.z<0) pos.z=rectSize.z*2 + pos.z; 
					if (pos.z>=rectSize.z) pos.z = rectSize.z*2 - pos.z - 1;
					break;
			}

			pos.x += rectOffset.x;
			pos.z += rectOffset.z;

			return pos;
		}

		/*[Val]
		public void OnGUI_SetInScene (object graphBox)
		{
			UI.Empty(Size.LinePixels(5));
			UI.CheckButton(ref drawOffsetScaleGizmo, "Set In Scene");
		}*/

		public void DrawGizmo ()
		{
			//hideDefaultToolGizmo = drawOffsetScaleGizmo;
			//if (drawOffsetScaleGizmo)
			//	GeneratorUI.DrawNodeOffsetSize(ref offset, ref scale, nodeToChange:this);
		}
	}

	[System.Serializable]
	[GeneratorMenu (menu="Map/Initial", name ="Import", iconName="GeneratorIcons/Import", disengageable = true, disabled = false, helpLink ="https://gitlab.com/denispahunov/mapmagic/wikis/map_generators/raw_input")]
	public class Import200 : Generator, IOutlet<MatrixWorld>, ISceneGizmo
	{
		[Val("Map", priority = 10, type = typeof(MatrixAsset))]	public MatrixAsset matrixAsset;

		[Val("Wrap Mode", priority = 4)]	public CoordRect.TileMode wrapMode = CoordRect.TileMode.Clamp;
		[Val("Scale", priority = 3)]		public float scale = 1;
		[Val("Offset", priority = 2)]		public Vector2 offset;
		

		public bool hideDefaultToolGizmo { get; set; }
		public bool drawOffsetScaleGizmo = false;

		static Import200()
		{
			MatrixAsset.OnReloaded += OnMatrixAssetReloaded_ReGenerate;
		}

		public static void OnMatrixAssetReloaded_ReGenerate (MatrixAsset ma)
		/// If this matrixAsset is used then clearing this node in all related mapmagics and starting generate
		{
			MapMagicObject[] mapMagics = GameObject.FindObjectsOfType<MapMagicObject>();
			foreach (MapMagicObject mapMagic in mapMagics)
			{
				bool containsMa = false;
				if (mapMagic.graph != null)
					foreach (Import200 gen in mapMagic.graph.GeneratorsOfType<Import200>())
					{
						if (gen.matrixAsset == ma) 
						{
							mapMagic.Clear(gen);

							containsMa = true;
							break;
						}
					}

				if (containsMa)
					mapMagic.StartGenerate();
			}
		}


		public override void Generate (TileData data, StopToken stop) 
		{
			if (stop!=null && stop.stop) return;
			if (matrixAsset == null || matrixAsset.matrix==null || !enabled) { data.products.Remove(this); return; }

			MatrixWorld dstMatrix = new MatrixWorld(data.area.full.rect, data.area.full.worldPos, data.area.full.worldSize, data.globals.height);

			if (matrixAsset.matrix.rect.size.x * scale > data.area.active.rect.size.x)
				ImportWithEnlarge(matrixAsset.matrix, dstMatrix, data.area);
			else
				ImportWithDownscale(matrixAsset.matrix, dstMatrix, data.area);

			data.products[this] = dstMatrix;
		}

		public void ImportWithEnlarge (Matrix src, MatrixWorld dst, Terrains.Area area)
		{
			//finding chunk rect on raw matrix
			Vector3 srcPixelSize = new Vector3 (
				scale * dst.worldSize.x / src.rect.size.x,
				0,
				scale * dst.worldSize.z / src.rect.size.z);

			CoordRect relRect = new CoordRect(
				Mathf.FloorToInt((area.full.worldPos.x-offset.x) / srcPixelSize.x),  //taking full rect here
				Mathf.FloorToInt((area.full.worldPos.z-offset.y) / srcPixelSize.z),
				Mathf.CeilToInt(area.full.worldSize.x / srcPixelSize.x),
				Mathf.CeilToInt(area.full.worldSize.z / srcPixelSize.z) ); 

			//creating a small matrix
			Matrix smallMatrix = new Matrix(relRect);
			Matrix.ReadMatrix(src, smallMatrix, wrapMode);

			//upscaling small matrix to dst
			MatrixOps.Resize(smallMatrix, dst);
		}

		public void ImportWithDownscale (Matrix src, MatrixWorld dst, Terrains.Area area)
		{
			CoordRect relRect = new CoordRect(
				Mathf.FloorToInt(offset.x / dst.PixelSize.x),
				Mathf.FloorToInt(offset.y / dst.PixelSize.z),
				Mathf.CeilToInt(scale * dst.rect.size.x),
				Mathf.CeilToInt(scale * dst.rect.size.z) );
				
			//creating a small matrix
			Matrix smallMatrix = new Matrix(relRect);
			MatrixOps.Resize(matrixAsset.matrix, smallMatrix);

			//writing small matrix to dst
			Matrix.ReadMatrix(smallMatrix, dst, wrapMode);
		}

		/*[Val]
		public void OnGUI_SetInScene (object graphBox)
		{
			UI.Empty(Size.LinePixels(5));
			UI.CheckButton(ref drawOffsetScaleGizmo, "Set In Scene");
		}*/

		public void DrawGizmo ()
		{
			//hideDefaultToolGizmo = drawOffsetScaleGizmo;
			//if (drawOffsetScaleGizmo)
			//	GeneratorUI.DrawNodeOffsetSize(ref offset, ref scale, nodeToChange:this);
		}
	}
}
