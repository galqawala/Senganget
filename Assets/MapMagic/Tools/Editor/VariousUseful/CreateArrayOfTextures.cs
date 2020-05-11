using UnityEngine;
using UnityEditor;
using Den.Tools;
using System.Collections.Generic;

public class CreateArrayOfTextures
{
	[MenuItem ("Edit/CreateArrayOfTextures")]
	static void CreateArrayOfTexturesFn ()
	{
		//finding textures from selection
		List<Texture2D> textures = new List<Texture2D>();
		for (int i=0; i<Selection.objects.Length; i++)
		{
			if (Selection.objects[i] is Texture2D)
			{
				Texture2D tex = Selection.objects[i] as Texture2D;
				
				if (textures.Count == 0) textures.Add(tex);

				else 
				{
					if (tex.width==textures[0].width && tex.height==textures[0].height) textures.Add(tex);
					else { Debug.Log(tex + " has a different size"); }
				}
			}
		}

		if (textures.Count == 0) { Debug.Log("No Texture Selected"); return; }

		Texture2DArray arr = new Texture2DArray(textures[0].width, textures[0].height, textures.Count, TextureFormat.RGBA32, textures[0].mipmapCount!=0, linear:true);
		
		for (int i=0; i<textures.Count; i++)
		{
			Texture2D tex = textures[i];

			Color[] pixels = tex.GetPixels();
			arr.SetPixels(pixels, i);
		}

		arr.Apply();

		//finding path
		string path= UnityEditor.EditorUtility.SaveFilePanel("Save Texture Array", "Assets", "TextureArray.asset", "asset");
		if (path==null || path.Length==0) return;

		//saving
		path = path.Replace(Application.dataPath, "Assets");
		UnityEditor.AssetDatabase.CreateAsset(arr, path);
	}
}

