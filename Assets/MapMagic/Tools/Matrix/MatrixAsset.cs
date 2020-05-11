using UnityEngine;
using System.Collections;
using System;
using System.IO;

using Den.Tools.Matrices;

namespace Den.Tools
{
	[HelpURL("https://gitlab.com/denispahunov/mapmagic/wikis/home")]
	[CreateAssetMenu(menuName = "MapMagic/Imported Map", fileName = "Imported Map.asset", order = 113)]
	[Serializable, PreferBinarySerialization]
	public class MatrixAsset : ScriptableObject
	{ 
		public Matrix matrix = new Matrix( new CoordRect(0,0,1,1) );
		public Texture2D preview; 

		public enum Source { Raw, Texture }
		public Source source;

		//values to reload
		public string rawPath;
		public Texture2D textureSource;
		public enum Channel { Average, Grayscale, Red, Green, Blue, Alpha };
		public Channel channelSource;

		//public static WeakEvent<MatrixAsset,Matrix> OnReloaded = new WeakEvent<MatrixAsset,Matrix>();
		//public static WeakEvent<Texture2D> OnTextureImported = new WeakEvent<Texture2D>(); 

		public static Action<MatrixAsset> OnReloaded;
		public static Action<Texture2D> OnTextureImported; //called in AssetPostprocessor

		public MatrixAsset() : base()
		{
			OnTextureImported += 
				tex => { if (source == Source.Texture && textureSource == tex) Reload(); };
		}


		public void ImportRaw (string path=null)
		{
			#if UNITY_EDITOR
			//importing
			if (path==null) path = UnityEditor.EditorUtility.OpenFilePanel("Import Texture File", "", "raw,r16");
			if (path==null || path.Length==0) return;

			UnityEditor.Undo.RecordObject(this, "Import RAW");
			#endif

			//reading file
			FileInfo fileInfo = new FileInfo(path);
			FileStream stream = fileInfo.Open(FileMode.Open, FileAccess.Read);

			int size = (int)Mathf.Sqrt(stream.Length/2);
			byte[] vals = new byte[size*size*2];

			stream.Read(vals,0,vals.Length);
			stream.Close();

			//setting matrix
			if (matrix == null  ||  matrix.rect.size.x != size  ||  matrix.rect.size.z != size)
				matrix = new Matrix( new CoordRect(0,0,size,size) );
			matrix.ImportRaw16(vals, size, size);

			//flipping vertically
			Matrix flipped = new Matrix(matrix.rect);
			MatrixOps.FlipVertical(matrix, flipped);
			matrix = flipped;

			rawPath = path;

			//generating preview
			RefreshPreview();

			//callback
			if (OnReloaded != null)
				OnReloaded(this);
		}

		public void ImportTexture (Texture2D texture, Channel channel = Channel.Average)
		{
			if (!texture.IsReadable())
				texture = texture.ReadableClone();

			Color[] colors = texture.GetPixels();

			//setting matrix
			if (matrix == null  ||  matrix.rect.size.x != texture.width  ||  matrix.rect.size.z != texture.height)
				matrix = new Matrix( new CoordRect(0,0,texture.width,texture.height) );
			int i = 0;
			Coord min = matrix.rect.Min; Coord max = matrix.rect.Max;
			//for (int z=max.z-1; z>=min.z; z--)
			for (int z=min.z; z<max.z; z++)
				for (int x=min.x; x<max.x; x++)
			{
				float val;

				switch (channel)
				{
					case Channel.Average: default: val = (colors[i].r + colors[i].g + colors[i].b) / 3f; break;
					case Channel.Grayscale: val = 0.21f*colors[i].r + 0.72f*colors[i].g + 0.07f*colors[i].b; break;
					case Channel.Red: val = colors[i].r; break;
					case Channel.Green: val = colors[i].g; break;
					case Channel.Blue: val = colors[i].b; break;
					case Channel.Alpha: val = colors[i].a; break;
				}

				matrix[x,z] = val;
				i++;
			}

			//textureSource = texture; //will assign non-readable clone
			//channelSource = channel;

			//generating preview
			RefreshPreview(256);

			//callback
			if (OnReloaded != null)
				OnReloaded(this);
		}

		public void ImportArray (float[,] heights)
		{
			//creating ref matrix
			if (matrix == null  ||  matrix.rect.size.x != heights.GetLength(1)  ||  matrix.rect.size.z != heights.GetLength(0))
				matrix = new Matrix( new CoordRect(0, 0, heights.GetLength(1), heights.GetLength(0)) );

			//importing
			matrix.ImportHeights(heights);

			//generating preview
			RefreshPreview(256);

			//callback
			if (OnReloaded != null)
				OnReloaded(this);
		}

		public void RefreshPreview (int size=128)
		{
			if (matrix != null)
			{
				Matrix previewMatrix = matrix;// new Matrix( new CoordRect(0,0,size,size) );
				//MatrixOps.Resize(matrix, previewMatrix);
				preview = new Texture2D(previewMatrix.rect.size.x, previewMatrix.rect.size.z);
				previewMatrix.ExportTexture(preview, -1);
			}
			else preview = TextureExtensions.ColorTexture(2,2,Color.black);
		}

		public void Reload ()
		{
			if (source == Source.Raw)
			{
				if (rawPath != null) 
					ImportRaw(rawPath);
			}
			else
			{
				if (textureSource != null)
					ImportTexture(textureSource, channelSource);
			}
		}
	}
}
