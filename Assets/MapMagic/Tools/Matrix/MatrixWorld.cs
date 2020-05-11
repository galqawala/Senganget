using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Den.Tools.Matrices
{
	public interface ITest { }

	[System.Serializable]
	public class MatrixWorld : Matrix, ITest, ICloneable
	{
		//public CoordRect rect; //in base
		public Vector3 worldPos;
		public Vector3 worldSize;
		public Vector3 WorldMax => worldPos+worldSize;

		public Vector2D PixelSize {get{ return new Vector2(worldSize.x/rect.size.x, worldSize.z/rect.size.z); }}


		#region Constructors

			public MatrixWorld () { arr = new float[0]; rect = new CoordRect(0,0,0,0); } //for serializer

			public MatrixWorld (CoordRect rect, Vector2D worldPos, Vector2D worldSize, float worldHeight, float[] array=null)
			{
				this.rect = rect;
				this.count = rect.Count;
				DefineArray(array);

				this.worldPos = worldPos;
				this.worldSize = worldSize;
				this.worldSize.y = worldHeight;
			}

			public MatrixWorld (CoordRect rect, Vector3 worldPos, Vector3 worldSize)
			{
				this.rect = rect;
				this.count = rect.Count;
				arr = new float[rect.size.x*rect.size.z];
				this.worldPos = worldPos;
				this.worldSize = worldSize;
			}

			public MatrixWorld (Coord offset, Coord size, Vector3 worldPos, Vector3 worldSize)
			{
				this.rect = new CoordRect(offset,size);
				this.count = rect.Count;
				arr = new float[rect.size.x*rect.size.z];
				this.worldPos = worldPos;
				this.worldSize = worldSize;
			}

			public MatrixWorld (Matrix matrix, Vector3 worldPos, Vector3 worldSize)
			{
				this.rect = matrix.rect;
				this.count = rect.Count;
				arr = matrix.arr;
				this.worldPos = worldPos;
				this.worldSize = worldSize;
			}

			public MatrixWorld (MatrixWorld mw)
			{
				this.rect = mw.rect;
				this.count = rect.Count;
				arr = new float[mw.arr.Length];
				Array.Copy(mw.arr, arr, mw.arr.Length);
				worldPos = mw.worldPos;
				worldSize = mw.worldSize;
			}

			public object Clone () { return new MatrixWorld(this); }

			//to create from simple Matrix use new MatrixWorld(matrix.rect, wp, ws, matrix.arr);

		#endregion


		#region World to Pixel / Pixel to World

			public Coord WorldToPixel (float x, float z)
			{
				//finding relative percent
				float percentX = (x - worldPos.x) / worldSize.x;
				float percentZ = (z - worldPos.z) / worldSize.z;

				//get map coordinates
				float mapX = percentX*rect.size.x + rect.offset.x;
				float mapZ = percentZ*rect.size.z + rect.offset.z;

				//flooring map values (values should be floored, not rounded since height pixel on terrain has it's own dimensions)
				int ix = (int)mapX; if (mapX<0) ix--; if (ix==rect.offset.x+rect.size.x) ix--;
				int iz = (int)mapZ; if (mapZ<0) iz--; if (iz==rect.offset.z+rect.size.z) iz--;

				return new Coord(ix, iz);
			}


			public Vector3 PixelToWorld (int x, int z)
			{
				//finding relative percent
				float percentX = 1f * (x - rect.offset.x) / rect.size.x;
				float percentZ = 1f * (z - rect.offset.z) / rect.size.z;

				//get map coordinates
				float worldX = percentX*worldSize.x + worldPos.x;
				float worldZ = percentZ*worldSize.z + worldPos.z;

				return new Vector3(worldX, 0, worldZ);
			}


			public Vector3 WorldToPixelInterpolated (float x, float z)
			{
				//finding relative percent
				float percentX = (x - worldPos.x) / worldSize.x;
				float percentZ = (z - worldPos.z) / worldSize.z;

				//get map coordinates
				float mapX = percentX*rect.size.x + rect.offset.x;
				float mapZ = percentZ*rect.size.z + rect.offset.z;

				return new Vector3(mapX, 0, mapZ);
			}


			public int WorldDistToPixel (float worldX)
			{
				float percentX = worldX / worldSize.x;
				float mapX = percentX*rect.size.x;// + rect.offset.x;
				int ix = (int)(mapX); if (mapX<0) ix--; if (ix==rect.offset.x+rect.size.x) ix--;
				return ix;
			}

			public float WorldDistToPixelInterpolated (float worldX)
			{
				float percentX = worldX / worldSize.x;
				return percentX*rect.size.x;// + rect.offset.x;
			}

			public float PixelDistToWorld (int mapX)
			{
				float percentX = (mapX+0.5f - rect.offset.x) / rect.size.x;  //taking the center of the pixel
				float worldX = percentX*worldSize.x;// + worldPos.x;
				return worldX;
			}

		#endregion


		#region Get/Set

			public bool ContainsWorldValue (float x, float z)
			{
				return  x > worldPos.x  &&   x < worldPos.x+worldSize.x  &&
						z > worldPos.z  &&   z < worldPos.z+worldSize.z;
			}

			public virtual float GetWorldValue (float x, float z)
			{
				Coord coord = WorldToPixel(x,z);
				return this[coord];

				/*//finding relative percent
				float percentX = (x - worldPos.x) / worldSize.x;
				float percentZ = (z - worldPos.z) / worldSize.z;

				//get map coordinates
				float mapX = percentX*rect.size.x + rect.offset.x;
				float mapZ = percentZ*rect.size.z + rect.offset.z;

				//flooring map values (values should be floored, not rounded since height pixel on terrain has it's own dimensions)
				int ix = (int)mapX; if (mapX<0) ix--; if (ix==rect.offset.x+rect.size.x) ix--;
				int iz = (int)mapZ; if (mapZ<0) iz--; if (iz==rect.offset.z+rect.size.z) iz--;

				UnityEngine.Assertions.Assert.IsTrue(ix>=rect.offset.x && iz>=rect.offset.z && ix<rect.offset.x+rect.size.x && iz<rect.offset.z+rect.size.z);
			
				return arr[(iz-rect.offset.z)*rect.size.x + ix - rect.offset.x];*/ 
			}

			public void SetWorldValue (float x, float z, float val)
			{
				//finding relative percent
				float percentX = (x - worldPos.x) / worldSize.x;
				float percentZ = (z - worldPos.z) / worldSize.z;

				//get map coordinates
				float mapX = percentX*rect.size.x + rect.offset.x;
				float mapZ = percentZ*rect.size.z + rect.offset.z;

				//flooring map values (values should be floored, not rounded since height pixel on terrain has it's own dimensions)
				int ix = (int)mapX; if (mapX<0) ix--; if (ix==rect.offset.x+rect.size.x) ix--;
				int iz = (int)mapZ; if (mapZ<0) iz--; if (iz==rect.offset.z+rect.size.z) iz--;

				UnityEngine.Assertions.Assert.IsTrue(ix>=rect.offset.x && iz>=rect.offset.z && ix<rect.offset.x+rect.size.x && iz<rect.offset.z+rect.size.z);
			
				arr[(iz-rect.offset.z)*rect.size.x + ix - rect.offset.x] = val; 
			}

			public virtual float GetWorldInterpolatedValue (float x, float z)
			{
				//finding relative percent
				float percentX = (x - worldPos.x) / worldSize.x;
				float percentZ = (z - worldPos.z) / worldSize.z;

				//get map coordinates
				float mapX = percentX*rect.size.x + rect.offset.x;
				float mapZ = percentZ*rect.size.z + rect.offset.z;

				return GetInterpolated(mapX, mapZ); //copy
			}

		#endregion
	}
}
