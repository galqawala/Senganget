using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using Den.Tools;
using Den.Tools.GUI;
using Den.Tools.Matrices;
using MapMagic.Products;
using MapMagic.Terrains;
using MapMagic.Nodes;

#if VEGETATION_STUDIO_PRO
using AwesomeTechnologies.VegetationSystem;
using AwesomeTechnologies.Vegetation.Masks;
#endif

namespace MapMagic.VegetationStudio
{
	[System.Serializable]
	[GeneratorMenu(
		menu = "Map/Output", 
		name = "VS Pro Maps", 
		section =2,
		drawButtons = false,
		colorType = typeof(MatrixWorld), 
		helpLink = "https://gitlab.com/denispahunov/mapmagic/wikis/output_generators/Grass")]
	public class VSProMapsOut : Generator, IInlet<MatrixWorld>, IOutputGenerator, IOutput
	{
		public OutputLevel outputLevel = OutputLevel.Draft | OutputLevel.Main;
		public OutputLevel OutputLevel { get{ return outputLevel; } }

		//[Val("Package", type = typeof(VegetationPackagePro))] public VegetationPackagePro package; //in globals
		
		public float density = 0.5f;
		
		public int maskGroup = 0;
		public int textureChannel = 0;


		public override void Generate (TileData data, StopToken stop) 
		{
			if (stop!=null && stop.stop) return;
			MatrixWorld src = data.products.ReadInlet(this);
			if (src == null) return; 

			if (!enabled) 
				{ data.finalize.Remove(finalizeAction, this); return; }

			data.finalize.Add(finalizeAction, this, src, data.currentBiomeMask); 
		}


		public static FinalizeAction finalizeAction = Finalize; //class identified for FinalizeData
		public static void Finalize (TileData data, StopToken stop)
		{
			#if VEGETATION_STUDIO_PRO

			//creating splats and prototypes arrays
			int layersCount = data.finalize.GetTypeCount(finalizeAction, data.subDatas);
			int splatsSize = data.area.active.rect.size.x;

			//preparing texture colors
			VegetationPackagePro package = data.globals.vegetationPackage as VegetationPackagePro;
			Color[][] colors = new Color[package.TextureMaskGroupList.Count][];
			for (int c=0; c<colors.Length; c++)
				colors[c] = new Color[splatsSize*splatsSize];
			int[] maskGroupNums = new int[colors.Length];

			//filling colors
			int i=0;
			foreach ((VSProMapsOut output, MatrixWorld matrix, MatrixWorld biomeMask) 
				in data.finalize.ProductSets<VSProMapsOut,MatrixWorld,MatrixWorld>(finalizeAction, data.subDatas))
			{
				if (matrix == null) continue;
				BlendLayer(colors, data.area, matrix, biomeMask, output.density, output.maskGroup, output.textureChannel, stop);
				maskGroupNums[i] = output.maskGroup;
				i++;
			}

			//pushing to apply
			if (stop!=null && stop.stop) return;
			ApplyData applyData = new ApplyData() {package=package, colors=colors, maskGroupNums=maskGroupNums};
			Graph.OnBeforeOutputFinalize?.Invoke(typeof(VSProMapsOut), data, applyData, stop);
			data.apply.Add(applyData);

			#endif
		}


		public static void BlendLayer (Color[][] colors, Area area, MatrixWorld matrix, MatrixWorld biomeMask, float opacity, int maskGroup, int textureChannel, StopToken stop=null)
		{
			Color[] cols = colors[maskGroup];
			int splatsSize = area.active.rect.size.x;
			int fullSize = area.full.rect.size.x;
			int margins = area.Margins;

			for (int x=0; x<splatsSize; x++)
				for (int z=0; z<splatsSize; z++)
				{
					if (stop!=null && stop.stop) return;

					int matrixPos = (z+margins)*fullSize + (x+margins);

					float val = matrix.arr[matrixPos];

					if (biomeMask != null) //no empty biomes in list (so no mask == root biome)
						val *= biomeMask.arr[matrixPos]; //if mask is not assigned biome was ignored, so only main outs with mask==null left here

					val *= opacity;

					if (val < 0) val = 0; if (val > 1) val = 1;

					int colsPos = z*splatsSize + x;
					switch (textureChannel)
					{
						case 0: cols[colsPos].r += val; break;
						case 1: cols[colsPos].g += val; break;
						case 2: cols[colsPos].b += val; break;
						case 3: cols[colsPos].a += val; break;
					}
				}
		}


		public void Purge (TileData data, Terrain terrain)
		{

		}

		#if VEGETATION_STUDIO_PRO
		public class ApplyData : IApplyData
		{
			public VegetationPackagePro package;
			public Color[][] colors;
			public int[] maskGroupNums;

			public void Read (Terrain terrain)  { throw new System.NotImplementedException(); }

			public void Apply (Terrain terrain)
			{
				VegetationStudioTile vsTile = terrain.GetComponent<VegetationStudioTile>();
				if (vsTile == null) vsTile = terrain.gameObject.AddComponent<VegetationStudioTile>();

				Texture2D[] textures = WriteTextures(vsTile.lastUsedTextures, colors);
			
				//Rect terrainRect = new Rect(terrain.transform.position.x, terrain.transform.position.z, terrain.terrainData.size.x, terrain.terrainData.size.z);
				//SetTextures(terrainRect, textures, maskGroupNums, package);

				vsTile.lastUsedTextures = textures;
				vsTile.lastUsedMaskGroupNums = maskGroupNums;
				vsTile.lastUsedPackage = package;
				vsTile.masksApplied = false;
			}

			public static ApplyData Empty
			{get{
				return new ApplyData() { 
					colors = new Color[0][],
					maskGroupNums = new int[0]  };
			}}

			public int Resolution
			{get{
				if (colors.Length==0) return 0;
				else return (int)Mathf.Sqrt(colors[0].Length);
			}}
		}


		public static Texture2D[] WriteTextures (Texture2D[] oldTextures, Color[][] colors)
		{
			int numTextures = colors.Length;
			if (numTextures==0) return new Texture2D[0];
			int resolution = (int)Mathf.Sqrt(colors[0].Length);

			Texture2D[] textures = new Texture2D[numTextures];

			//making textures of colors in coroutine
			for (int i=0; i<numTextures; i++)
			{
				//trying to reuse last used texture
				Texture2D tex;
				if (oldTextures != null  &&
					i < oldTextures.Length  &&
					oldTextures[i] != null && 
					oldTextures[i].width == resolution && 
					oldTextures[i].height == resolution)
						tex = oldTextures[i];
					
				else
				{
					tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, true, true);
					tex.wrapMode = TextureWrapMode.Mirror; //to avoid border seams
				}
					
				tex.SetPixels(0,0,tex.width,tex.height,colors[i]);
				tex.Apply();

				textures[i] = tex;
			}

			return textures;
		}
		#endif
	}
}
