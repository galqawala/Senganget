﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Den.Tools;
using MapMagic.Nodes;
		
namespace MapMagic.Terrains
{
			[Serializable]
			public class Area
			{
				[Serializable]
				public class Dimensions
				{
					public CoordRect rect;
					public Vector2D worldPos;
					public Vector2D worldSize; 

					public Dimensions (CoordRect rect, Vector2D worldPos, Vector2D worldSize)
						{ this.rect = rect; this.worldPos = worldPos; this.worldSize = worldSize; }

					public Dimensions (Vector2D worldPos, Vector2D worldSize, int resolution)
					{ 
						this.worldPos = worldPos; this.worldSize = worldSize; 
						
						Vector3 pixelSize = 1f * worldSize / resolution;
						Coord offset = new Coord( Mathf.FloorToInt(worldPos.x / pixelSize.x), Mathf.FloorToInt(worldPos.z / pixelSize.x));
						this.rect = new CoordRect(offset, new Coord(resolution,resolution));
					}

					public Vector3 PixelSize {get{ return new Vector3(worldSize.x/rect.size.x, 1, worldSize.z/rect.size.z); }}

					public override string ToString () { return "Rect:" + rect.ToString() + " Pos:" + worldPos + " Size:" + worldSize; }

					public Vector3 CoordToWorld (int x, int z)
					{
						//finding relative percent
						float percentX = 1f * (x - rect.offset.x) / rect.size.x;
						float percentZ = 1f * (z - rect.offset.z) / rect.size.z;

						//get map coordinates
						float worldX = percentX*worldSize.x + worldPos.x;
						float worldZ = percentZ*worldSize.z + worldPos.z;

						return new Vector3(worldX, 0, worldZ);
					}

					public bool Contains (Vector3 pos)
					{
						if (pos.x < worldPos.x || pos.x > worldPos.x+worldSize.x ||
							pos.z < worldPos.z || pos.z > worldPos.z+worldSize.z) return false;
						return true;
					}
				}

				public Dimensions active;
				public Dimensions full;  

				public int Margins {get{ return (full.rect.size.x-active.rect.size.x)/2; }}
				public Vector3 PixelSize {get{ return active.PixelSize; }}
				public override string ToString() { return "Active:(" + active.ToString() + "), Full:(" + full.ToString() + ")"; }

				public Coord Coord { get{ return new Coord(active.rect.offset.x/active.rect.size.x, active.rect.offset.z/active.rect.size.z); }}


				private static CoordRect WorldToPixels (Vector3 worldPos, Vector3 worldSize, int resolution)
				{
					Vector3 pixelSize = 1f * worldSize / resolution;
					Coord activeOffset = new Coord( Mathf.FloorToInt(worldPos.x / pixelSize.x), Mathf.FloorToInt(worldPos.z / pixelSize.x) );
					return new CoordRect(activeOffset, new Coord(resolution,resolution));
				}


				private static Dimensions GetFullDimensions (Dimensions active, int margins)
				{
					Vector3 pixelSize = active.PixelSize;
					return new Dimensions(
						rect: new CoordRect( new Coord(active.rect.offset.x-margins, active.rect.offset.z-margins), new Coord(active.rect.size.x+margins*2, active.rect.size.z+margins*2) ),
						worldPos: new Vector2D(active.worldPos.x - margins*pixelSize.x, active.worldPos.z - margins*pixelSize.z), 
						worldSize: new Vector2D(active.worldSize.x + margins*pixelSize.x*2, active.worldSize.z + margins*pixelSize.z*2) );
				}


				public Area (CoordRect activeWorldRect, CoordRect activePixelRect, int margins)
				{
					active = new Dimensions(
						rect: activePixelRect,
						worldPos: activeWorldRect.offset.vector2d,
						worldSize: activeWorldRect.size.vector2d);

					full = GetFullDimensions(active, margins);
				}


				public Area (Vector2D activeWorldPos, Vector2D activeWorldSize, int activeResolution, int margins)
				{
					active = new Dimensions(
						rect: WorldToPixels(activeWorldPos, activeWorldSize, activeResolution),
						worldPos: activeWorldPos,
						worldSize: activeWorldSize);

					full = GetFullDimensions(active, margins);
				}

				public Area (Terrain terrain, int margins=2)
				{
					Vector2D terrainPos = (Vector2D)terrain.transform.position;
					Vector2D terrainSize = (Vector2D)terrain.terrainData.size;

					active = new Dimensions(
						rect: WorldToPixels(terrainPos, terrainSize, terrain.terrainData.heightmapResolution-1),
						worldPos: terrainPos,
						worldSize: terrainSize);

					full = GetFullDimensions(active, margins);
				}

				public Area (Coord coord, int activeResolution, int margins, Vector2D activeWorldSize)
				{
					active = new Dimensions(
						rect: new CoordRect( coord.x*activeResolution, coord.z*activeResolution, activeResolution, activeResolution ),
						worldPos: new Vector2D(coord.x*activeWorldSize.x, coord.z*activeWorldSize.z),
						worldSize: new Vector2D(activeWorldSize.x, activeWorldSize.z));

					full = GetFullDimensions(active, margins);
				}

				public void MoveTo (Vector2D activeWorldPos)
				{
					active = new Dimensions(
						rect: WorldToPixels(activeWorldPos, active.worldSize, active.rect.size.x),
						worldPos: activeWorldPos,
						worldSize: active.worldSize);

					full = GetFullDimensions(active, Margins);
				}

				public void MoveTo (Coord coord, int activeResolution, Vector2D activeWorldPos)
				{
					active.rect = new CoordRect( coord.x*activeResolution, coord.z*activeResolution, activeResolution, activeResolution );
					active.worldPos = new Vector2D(coord.x*activeWorldPos.x, coord.z*activeWorldPos.z);
					active.worldSize = activeWorldPos;

					full = GetFullDimensions(active, Margins);
				}
			}
}