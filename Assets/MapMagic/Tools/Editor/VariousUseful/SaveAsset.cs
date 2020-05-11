// Simple scripts to save all kind of assets
// by Denis Pahunov

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

public static class SaveAssetClass 
{
	public static void SaveAsset (UnityEngine.Object asset, string extension="asset", string savePath=null) 
	{
		if (savePath==null) savePath = UnityEditor.EditorUtility.SaveFilePanel(
					"Save Data as Unity Asset",
					"Assets",
					"Asset", 
					extension);
		if (savePath!=null && savePath.Length!=0)
		{
			savePath = savePath.Replace(Application.dataPath, "Assets");

			AssetDatabase.DeleteAsset(savePath);
			UnityEditor.AssetDatabase.CreateAsset(asset, savePath);
			UnityEditor.AssetDatabase.SaveAssets();
		}
	}

	public static string SaveAssetDialog (string startPath="Assets", string defaultName="Asset", string extension="asset")
	{
		string savePath = UnityEditor.EditorUtility.SaveFilePanel(
					"Save Asset",
					startPath,
					defaultName, 
					extension);
		if (savePath!=null && savePath.Length!=0)
		{
			savePath = savePath.Replace(Application.dataPath, "Assets");
			//AssetDatabase.DeleteAsset(savePath);
		}

		return savePath;
	}

	public static void SaveTerrainData (TerrainData terrainData, string savePath=null)
	{
		//backup splats
		float[,,] splats = terrainData.GetAlphamaps(0,0,terrainData.alphamapResolution, terrainData.alphamapResolution); 
		
		SaveAsset(terrainData, savePath);
		
		//re-assign splats
		terrainData.SetAlphamaps(0,0,splats);
		AssetDatabase.SaveAssets();
	}

	public static void SaveTerrainsRecursively (GameObject obj, string saveDir)
	{
		Terrain terrain = obj.GetComponent<Terrain>();
		if (terrain != null)
			SaveTerrainData(terrain.terrainData, savePath:saveDir + "\\" + obj.name + ".asset");

		for (int c=0; c<obj.transform.childCount; c++)
			SaveTerrainsRecursively(obj.transform.GetChild(c).gameObject, saveDir);
	}



	[MenuItem ("Edit/Save Asset/Terrain")]
	static void SaveTerrain ()
	{
		Terrain terrain = Selection.activeGameObject.GetComponent<Terrain>();
		SaveTerrainData(terrain.terrainData);
	}


	[MenuItem ("Edit/Save Asset/Multiple Terrains")]
	static void SaveTerrains ()
	{
		string saveDir = UnityEditor.EditorUtility.SaveFolderPanel(
					"Save Data as Unity Asset",
					"Assets",
					"Terrains");
		for (int i=0; i<Selection.gameObjects.Length; i++)
		{
			GameObject obj = Selection.gameObjects[i];
			SaveTerrainsRecursively(obj, saveDir);	
		}
	}


	[MenuItem ("Edit/Save Asset/Mesh")]
	public static void SaveMesh ()
	{
		MeshFilter filter = Selection.activeGameObject.GetComponent<MeshFilter>();
		SaveAsset(filter.sharedMesh, "mesh");
	}

	[MenuItem ("Edit/Save Asset/Texture")]
	public static void SaveTexture ()
	{
		Texture2D tex = Selection.activeObject as Texture2D;
		if (tex == null) { Debug.Log("No texture selected"); return; }

		string path = SaveAssetDialog(extension:"png");
		if (path == null) return;

		SaveTexture(tex, path);
	}


	public static void SaveTexture (Texture2D tex, string path)
	{
		//byte[] bytes = tex.EncodeToEXR(Texture2D.EXRFlags.OutputAsFloat);
		byte[] bytes = tex.EncodeToPNG();
		System.IO.File.WriteAllBytes(path, bytes);
		AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

		TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path); //should work if theres nothing at path yet
		importer.isReadable = true;
		importer.sRGBTexture = false;
		importer.mipmapEnabled = false;
		importer.wrapMode = TextureWrapMode.Clamp;
		importer.filterMode = FilterMode.Point;
		importer.SaveAndReimport();
	}



	[MenuItem ("Edit/Save Asset/Material")]
	static void SaveMat ()
	{
		Material material = Selection.activeObject as Material;
		if (material != null)
			{ SaveAsset(material); return; }

		MeshRenderer renderer = Selection.activeGameObject.GetComponent<MeshRenderer>();
		if (renderer != null)
			SaveAsset(renderer.sharedMaterial);

		Terrain terrain = Selection.activeGameObject.GetComponent<Terrain>();
		if (terrain != null)
			SaveAsset(terrain.materialTemplate);
	}

	[MenuItem ("Edit/Save Asset/Mesh Obj")]
	static void SaveObj ()
	{
		MeshFilter filter = Selection.activeGameObject.GetComponent<MeshFilter>();

		string savePath = UnityEditor.EditorUtility.SaveFilePanel("Save Mesh as Obj", "Assets", "Mesh", "obj");
		if (savePath!=null && savePath.Length!=0)
		{
			savePath = savePath.Replace(Application.dataPath, "Assets");

			using (StreamWriter sw = new StreamWriter(savePath)) 
			{
				sw.Write( MeshToString(filter.sharedMesh) );
			}
		}
	}

	[MenuItem ("Edit/Save Asset/Child Meshes")]
	static void SaveChildMeshes ()
	{
		MeshFilter[] filters = Selection.activeGameObject.GetComponentsInChildren<MeshFilter>();

		string savePath = UnityEditor.EditorUtility.SaveFolderPanel(
			"Save Child Meshes as Unity Asset",
			"Assets",
			"Asset");

		if (savePath!=null && savePath.Length!=0)
		{
			savePath = savePath.Replace(Application.dataPath, "Assets");

			for (int i=0; i<filters.Length; i++)
			{
				Mesh mesh = filters[i].sharedMesh;
				if (mesh == null) continue;

				UnityEditor.AssetDatabase.CreateAsset(mesh, savePath + "/" + mesh.name + ".asset");
				UnityEditor.AssetDatabase.SaveAssets();
			}
		}
	}


	public static string MeshToString(Mesh m) 
	{
        //Mesh m = mf.mesh;
        //Material[] mats = mf.renderer.sharedMaterials;
 
        StringBuilder sb = new StringBuilder();
 
        sb.Append("g ").Append(m.name + "Mesh").Append("\n");
        foreach(Vector3 v in m.vertices) {
            sb.Append(string.Format("v {0} {1} {2}\n",v.x,v.y,v.z));
        }
        sb.Append("\n");
        foreach(Vector3 v in m.normals) {
            sb.Append(string.Format("vn {0} {1} {2}\n",v.x,v.y,v.z));
        }
        sb.Append("\n");
        foreach(Vector3 v in m.uv) {
            sb.Append(string.Format("vt {0} {1}\n",v.x,v.y));
        }
		
		{
            sb.Append("\n");
            sb.Append("usemtl ").Append("Mat").Append("\n");
            sb.Append("usemap ").Append("Mat").Append("\n");
 
            int[] triangles = m.triangles;
            for (int i=0;i<triangles.Length;i+=3) {
                sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n", 
                    triangles[i]+1, triangles[i+1]+1, triangles[i+2]+1));
            }
        }
        return sb.ToString();
    }
 
}
