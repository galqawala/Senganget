using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Den.Tools;
using MapMagic.Terrains;

#if VEGETATION_STUDIO_PRO
using AwesomeTechnologies.VegetationStudio;
using AwesomeTechnologies.VegetationSystem;
using AwesomeTechnologies.Vegetation.Masks;
using AwesomeTechnologies.Vegetation.PersistentStorage;
using AwesomeTechnologies.Billboards;
#endif

namespace MapMagic.VegetationStudio
{
	[ExecuteInEditMode]
	public class VegetationStudioTile : MonoBehaviour, ISerializationCallbackReceiver
	/// Helper script to automatically add/remove and serialize textures and persistent storage data
	{
		#if VEGETATION_STUDIO_PRO
		const byte VS_MM_id = 15; //15 for MapMagic, 18 for Voxeland

		public static VegetationSystemPro system;

		public UnityTerrain unityTerrain;
		public Terrain terrain;

		public VegetationPackagePro lastUsedPackage;
		public Texture2D[] lastUsedTextures;
		public int[] lastUsedMaskGroupNums;
		[System.NonSerialized] public bool masksApplied;

		public List<Transition>[] lastUsedTransitions;
		public string[] lastUsedObjectIds;
		[System.NonSerialized] public bool objectApplied;
		



		#region MonoBeh

			public void OnRenderObject () //Unity terrain component should be added after VS update done, otherwise it will cause flickering
			{
				if (system == null) system = GameObject.FindObjectOfType<VegetationSystemPro>();
				if (terrain == null) terrain = GetComponent<Terrain>();
				if (unityTerrain == null) unityTerrain = GetComponent<UnityTerrain>();

				Rect terrainRect = new Rect(transform.position.x, transform.position.z, terrain.terrainData.size.x, terrain.terrainData.size.z);

				//unity terrain
				if (unityTerrain == null || !system.VegetationStudioTerrainList.Contains(unityTerrain))
					AddUnityTerrain();

				//masks
				if (!masksApplied  &&  lastUsedTextures != null  &&  lastUsedMaskGroupNums != null  &&  lastUsedPackage != null)
				{
					SetTextures(terrainRect, lastUsedTextures, lastUsedMaskGroupNums, lastUsedPackage);
					masksApplied = true;
				}

				//objects
				if (!objectApplied  &&  lastUsedTransitions != null  &&  lastUsedObjectIds != null)
				{
					SetObjects(lastUsedTransitions, lastUsedObjectIds);
					objectApplied = true;
				}
			}


			public void OnDisable () 
			{
				RemoveUnityTerrain();

				Terrain terrain = GetComponent<Terrain>();
				Rect terrainRect = new Rect(transform.position.x, transform.position.z, terrain.terrainData.size.x, terrain.terrainData.size.z);

				FlushTextures(terrainRect, lastUsedPackage);
				masksApplied = false;
			
				FlushObjects(terrainRect);
				objectApplied = false;
			}

		#endregion

		
		#region Set/Flush

			public void AddUnityTerrain ()
			{
				Terrain terrain = GetComponent<Terrain>();

				//setting up UnityTerrain
				UnityTerrain unityTerrain = terrain.gameObject.GetComponent<UnityTerrain>();
				if (unityTerrain == null) unityTerrain = terrain.gameObject.AddComponent<UnityTerrain>();
				unityTerrain.Terrain = terrain;
				unityTerrain.TerrainPosition = terrain.transform.position;

				//adding terrain to VS
				if (system == null) system = GameObject.FindObjectOfType<VegetationSystemPro>();
				if (system != null  &&  !system.VegetationStudioTerrainList.Contains(unityTerrain))  //check if already added since AddTerrain is long operation
					system.AddTerrain(terrain.gameObject);
			}


			public void RemoveUnityTerrain ()
			{
				if (system == null) system = GameObject.FindObjectOfType<VegetationSystemPro>();
				if (system != null)
					system.RemoveTerrain(gameObject);
			}


			public static void SetTextures (Rect terrainRect, Texture2D[] textures, int[] maskGroupNums, VegetationPackagePro package)
			{
				for (int i=0; i<textures.Length; i++)
				{
					Texture2D tex = textures[i];
					if (tex == null) continue;

					TextureMaskGroup maskGroup = package.TextureMaskGroupList[maskGroupNums[i]];

					//creating new mask only if the mask with the same rect doesn't exist
					TextureMask mask = maskGroup.TextureMaskList.Find(m => m.TextureRect == terrainRect);
					if (mask == null)
					{
						mask = new TextureMask { TextureRect = terrainRect };
						maskGroup.TextureMaskList.Add(mask);
					}

					mask.MaskTexture = tex;
				}

				//VegetationSystemPro system = GameObject.FindObjectOfType<VegetationSystemPro>();
				//if (system != null) 
				//	system.ClearCache(); //clearing cache causes flickering
				VegetationStudioManager.RefreshTerrainHeightMap();
			}


			public static void FlushTextures (Rect terrainRect, VegetationPackagePro package)
			{
				if (package == null) return;

				foreach (TextureMaskGroup maskGroup in package.TextureMaskGroupList)
				{
					for (int i=maskGroup.TextureMaskList.Count-1; i>=0; i--)
					{
						TextureMask mask = maskGroup.TextureMaskList[i];
						if (mask.MaskTexture == null  ||  mask.TextureRect == terrainRect)
						{
							mask.Dispose();
							maskGroup.TextureMaskList.RemoveAt(i);
							//if (system != null) system.SelectedTextureMaskGroupTextureIndex = 0;
						}
					}
				}

				//if (system != null) 
				//	system.ClearCache();
			}


			public static void SetObjects (List<Transition>[] allTransitions, string[] allIds)
			{
				VegetationSystemPro system = GameObject.FindObjectOfType<VegetationSystemPro>();
				PersistentVegetationStorage storage = system.PersistentVegetationStorage;

				for (int i=0; i<allTransitions.Length; i++)
				{
					if (allTransitions[i] == null) continue;
					foreach (Transition obj in allTransitions[i])
					{
						storage.AddVegetationItemInstance(
								allIds[i], 
								obj.pos,
								obj.scale, 
								obj.rotation,
								applyMeshRotation: true, 
								vegetationSourceID: VS_MM_id, 
								distanceFalloff: 1, 
								clearCellCache:true);
					}
				}

				//for (int i=0; i<system.VegetationCellList.Count; i++)
				//	system.VegetationCellList[i].ClearCache();

				system.RefreshBillboards();

				#if UNITY_EDITOR
				UnityEditor.EditorUtility.SetDirty(storage.PersistentVegetationStoragePackage);
				UnityEditor.EditorUtility.SetDirty(system);
				#endif
			}


			public static void FlushObjects (Rect terrainRect, bool clearCache=true)
			{
				VegetationSystemPro system = GameObject.FindObjectOfType<VegetationSystemPro>();
				if (system == null) return;

				PersistentVegetationStorage storage = system.PersistentVegetationStorage;

				PersistentVegetationStoragePackage storagePackage = system.PersistentVegetationStorage.PersistentVegetationStoragePackage;
				if (storagePackage == null) return;

				List<VegetationCell> overlapCellList = new List<VegetationCell>(); 
				system.VegetationCellQuadTree.Query(terrainRect, overlapCellList);

				for (int i=0; i < overlapCellList.Count; i++)
				{
					int cellIndex = overlapCellList[i].Index;

					//storagePackage.PersistentVegetationCellList[cellIndex].ClearCell();

					var infoList = storagePackage.PersistentVegetationCellList[cellIndex].PersistentVegetationInfoList;
					for (int j=0; j<infoList.Count; j++)
					{
						var itemList = infoList[j].VegetationItemList;

						for (int k=itemList.Count-1; k>=0; k--)
						{
							Vector3 pos = itemList[k].Position + system.VegetationSystemPosition;
							Vector2 pos2 = pos.V2();

							if (terrainRect.Contains(pos2))
							{
								itemList.RemoveAt(k);
								//storage.RemoveVegetationItemInstance(infoList[j].VegetationItemID, pos, 1, clearCellCache:false);
							}
						}
					}

					//VegetationItemIndexes indexes = VegetationSystemPro.GetVegetationItemIndexes(vegetationItemID);                    
					//system.ClearCache(overlapCellList[i],indexes.VegetationPackageIndex,indexes.VegetationItemIndex);

				
				}

				//if (clearCache)
				//{
				//	for (int i=0; i<system.VegetationCellList.Count; i++)
				//		system.VegetationCellList[i].ClearCache();
				//}

				system.RefreshBillboards();
			}

		#endregion


		#region Serialization

			[System.Serializable] public class TransitionaListHolder { public List<Transition> transitions; }
			public TransitionaListHolder[] serializedUsedTransitions = new TransitionaListHolder[0];

			public void OnBeforeSerialize ()
			{
				if (lastUsedTransitions == null) return;

				serializedUsedTransitions = new TransitionaListHolder[lastUsedTransitions.Length];
				for (int i=0; i<serializedUsedTransitions.Length; i++)
				{
					if (serializedUsedTransitions[i] == null) serializedUsedTransitions[i] = new TransitionaListHolder();
					serializedUsedTransitions[i].transitions = lastUsedTransitions[i];
				}
			}

			public void OnAfterDeserialize ()
			{
				if (serializedUsedTransitions.Length == 0) { lastUsedTransitions = null; return; }

				lastUsedTransitions = new List<Transition>[serializedUsedTransitions.Length];
				for (int i=0; i<serializedUsedTransitions.Length; i++)
					lastUsedTransitions[i] = serializedUsedTransitions[i].transitions;
			}

		#endregion
	#else
	public void OnBeforeSerialize () { }
	public void OnAfterDeserialize () { }
	#endif
	}
}
