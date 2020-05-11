
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.Profiling;

using Den.Tools;
using Den.Tools.GUI;
using Den.Tools.SceneEdit;

using MapMagic.Terrains;

namespace MapMagic.Core.GUI
{
	public static class PinDraw
	{
		private static readonly Color pinButtonColor = new Color(0.5f, 0.58f, 0.7f);
		private static readonly Color previewButtonColor = new Color(0.22f, 0.42f, 0.69f);
		private static readonly Color unpinButtonColor = new Color(0.68f, 0.14f, 0.15f);

		private const float margins = 0;
		private const int numSteps = 100;
		private const float width = 4;

		private static PolyLine polyLine; //to draw frames
		private static Texture2D lineTex;

		public enum SelectionMode { none, pin, pinLowres, pinExisting, selectPreview, unpin }


		public static void DrawInspectorGUI (MapMagicObject mapMagic, ref SelectionMode selectionMode)
		{
			Cell.EmptyLinePx(2);

			using (Cell.LinePx(25)) DrawPinButton(ref selectionMode, SelectionMode.pin, "MapMagic/Icons/Pin", "MapMagic/PinButtons/PinTop", "Pin New Tile", pinButtonColor);
			using (Cell.LinePx(25)) DrawPinButton(ref selectionMode, SelectionMode.pinLowres, "MapMagic/Icons/PinDraft", "MapMagic/PinButtons/PinMid","Pin As Draft", pinButtonColor);
			using (Cell.LinePx(25)) DrawPinButton(ref selectionMode, SelectionMode.pinExisting, "MapMagic/Icons/PinExisting", "MapMagic/PinButtons/PinBottom", "Pin Existing Terrain", pinButtonColor);
			
			Cell.EmptyLinePx(5);
			using (Cell.LinePx(25)) DrawPinButton(ref selectionMode, SelectionMode.selectPreview, "MapMagic/Icons/Preview", "MapMagic/PinButtons/Preview", "Select Preview", previewButtonColor);
			
			Cell.EmptyLinePx(5);
			using (Cell.LinePx(25)) DrawPinButton(ref selectionMode, SelectionMode.unpin, "MapMagic/Icons/Unpin", "MapMagic/PinButtons/Remove", "Unpin", unpinButtonColor);

			Cell.EmptyLinePx(2);
		}

		private static void DrawPinButton (ref SelectionMode selectionMode, SelectionMode buttonMode, string iconName, string buttonName, string label, Color color)
		{
			using (Cell.Padded(0,0,-1,-1))
			{
				bool isPinning = selectionMode==buttonMode;

				Draw.CheckButton(ref isPinning, visible:false);

				GUIStyle style = UI.current.textures.GetElementStyle(isPinning ? buttonName+"_pressed" : buttonName);
				Draw.Element(style);

				Cell.EmptyRowPx(10);

				using (Cell.RowPx(30))
				{
					//using (Cell.Padded(1,1,1,1))
					//{
					//	if (isPinning) { color = color/2; color.a=1; }
					//	Draw.Rect(color);
					//}

					Texture2D icon = UI.current.textures.GetTexture(iconName);
					Draw.Icon(icon);
				}

				using (Cell.Row) Draw.Label(label, style:UI.current.styles.middleLabel);

				if (Cell.current.valChanged)
				{
					if (isPinning) selectionMode = buttonMode;
					else selectionMode = SelectionMode.none;
				}
			}
		}



		public static void DrawSceneGUI (MapMagicObject mapMagic, SelectionMode selectionMode)
		{
			Dictionary<Coord,TerrainTile> tilesLut = new Dictionary<Coord,TerrainTile>(mapMagic.tiles.grid);

			if (selectionMode!=SelectionMode.none  &&  !Event.current.alt)
			{
				//returning if no scene veiw (right after script compile)
				SceneView sceneview = UnityEditor.SceneView.lastActiveSceneView;
				if (sceneview==null || sceneview.camera==null) return;

				//disabling selection
				HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
				
				//preparing the sets of both custom and tile terrains
				HashSet<Terrain> pinnedCustomTerrains = new HashSet<Terrain>(); //custom terrains that were already pinned before
				foreach (TerrainTile tile in mapMagic.tiles.customTiles)
				{
					if (tile.main != null) pinnedCustomTerrains.Add(tile.main.terrain);
					if (tile.draft != null) pinnedCustomTerrains.Add(tile.draft.terrain);
				}

				HashSet<Terrain> pinnedTileTerrains = new HashSet<Terrain>();
				foreach (var kvp in tilesLut)
				{
					TerrainTile tile = kvp.Value;
					if (tile.main != null) pinnedTileTerrains.Add(tile.main.terrain);
					if (tile.draft != null) pinnedTileTerrains.Add(tile.draft.terrain);
				}

				if (selectionMode == SelectionMode.pin)
				{
					Handles.color = FrameDraw.pinColor;

					List<Coord> selectedCoords = SelectTiles(mapMagic.tileSize, false, tilesLut, mapMagic.transform);
					if (selectedCoords != null)
					{
						UnityEditor.Undo.RegisterFullObjectHierarchyUndo(mapMagic.gameObject, "MapMagic Pin Terrains"); 

						foreach (Coord coord in selectedCoords)
							mapMagic.tiles.Pin(coord, false, mapMagic);

						foreach (Coord coord in selectedCoords)
							UnityEditor.Undo.RegisterCreatedObjectUndo(mapMagic.tiles[coord].gameObject, "MapMagic Pin Terrains"); 
					}
				}

				if (selectionMode == SelectionMode.pinLowres)
				{
					Handles.color = FrameDraw.pinColor;

					List<Coord> selectedCoords = SelectTiles(mapMagic.tileSize, true, tilesLut, mapMagic.transform);
					if (selectedCoords != null)
					{
						UnityEditor.Undo.RegisterFullObjectHierarchyUndo(mapMagic.gameObject, "MapMagic Pin Draft Terrains");

						foreach (Coord coord in selectedCoords)
							mapMagic.tiles.Pin(coord, true, mapMagic);

						foreach (Coord coord in selectedCoords)
							UnityEditor.Undo.RegisterCreatedObjectUndo(mapMagic.tiles[coord].gameObject, "MapMagic Pin Terrains"); 
					}
				}

				if (selectionMode == SelectionMode.pinExisting)
				{
					//excluding tiles
					HashSet<Terrain> possibleTerrains = new HashSet<Terrain>();
					Terrain[] allTerrains = GameObject.FindObjectsOfType<Terrain>();
					foreach (Terrain terrain in allTerrains)
						if (!pinnedTileTerrains.Contains(terrain)) possibleTerrains.Add(terrain);

					HashSet<Terrain> selectedTerrains = SelectTerrains(possibleTerrains, FrameDraw.pinColor, false);
					if (selectedTerrains != null)
					{
						UnityEditor.Undo.RegisterFullObjectHierarchyUndo(mapMagic.gameObject, "MapMagic Pin Terrains");

						foreach (Terrain terrain in selectedTerrains)
						{
							if (pinnedCustomTerrains.Contains(terrain)) continue;
							terrain.transform.parent = mapMagic.transform;

							TerrainTile tile = terrain.gameObject.GetComponent<TerrainTile>();
							if (tile == null) tile = terrain.gameObject.AddComponent<TerrainTile>();
							tile.main.terrain = terrain;
							//tile.main.area = new Terrains.Area(terrain);
							//tile.main.use = true;
							//tile.lodData = null;

							mapMagic.tiles.PinCustom(tile);
							mapMagic.StartGenerate(tile);
						}
							
					}
				}


				if (selectionMode == SelectionMode.selectPreview)
				{
					//hash set of all pinned terrains contains main data
					HashSet<Terrain> pinnedMainTerrains = new HashSet<Terrain>();
					foreach (var kvp in tilesLut)
					{
						TerrainTile tile = kvp.Value;
						if (tile.main != null) pinnedMainTerrains.Add(tile.main.terrain);
					}
					pinnedMainTerrains.UnionWith(pinnedCustomTerrains);

					HashSet<Terrain> selectedTerrains = SelectTerrains(pinnedMainTerrains, FrameDraw.selectPreviewColor, false);
					if (selectedTerrains != null && selectedTerrains.Count == 1)
					{
						UnityEditor.Undo.RegisterCompleteObjectUndo(mapMagic, "MapMagic Select Preview");

						Terrain selectedTerrain = selectedTerrains.Any();

						//clearing preview
						if (selectedTerrain == mapMagic.AssignedPreviewTerrain) 
						{
							mapMagic.ClearPreviewTile();
							TerrainTile.OnPreviewAssigned(mapMagic.PreviewData);
						}

						//assigning new
						else
							foreach (var kvp in tilesLut)
							{
								TerrainTile tile = kvp.Value;
								if (tile.main?.terrain == selectedTerrain  &&  mapMagic.AssignedPreviewTerrain != selectedTerrain) 
								{
									mapMagic.AssignPreviewTile(tile);
									mapMagic.AssignedPreviewData.isPreview = true;
									TerrainTile.OnPreviewAssigned(mapMagic.AssignedPreviewData);

									UI.RepaintAllWindows();
								}
							}	
					}
				}


				if (selectionMode == SelectionMode.unpin)
				{
					HashSet<Terrain> possibleTerrains = new HashSet<Terrain>(pinnedTileTerrains);
					possibleTerrains.UnionWith(pinnedCustomTerrains);

					HashSet<Terrain> selectedTerrains = SelectTerrains(possibleTerrains, FrameDraw.unpinColor, false);
					if (selectedTerrains != null)
					{
						UnityEditor.Undo.RegisterFullObjectHierarchyUndo(mapMagic.gameObject, "MapMagic Unpin Terrains");

						foreach (Terrain terrain in selectedTerrains)
						{
							//terrain-to-coord lut (to pick tile terrain coord faster)
							Dictionary<Terrain, Coord> terrainToCoordLut = new Dictionary<Terrain, Coord>();
							foreach (var kvp in tilesLut)
							{
								Coord coord = kvp.Key;
								TerrainTile tile = kvp.Value;
								if (tile.main != null) terrainToCoordLut.Add(tile.main.terrain, coord);
								if (tile.draft != null) terrainToCoordLut.Add(tile.draft.terrain, coord);
							}

							//if it's tile
							if (terrainToCoordLut.ContainsKey(terrain))
							{
								Coord coord = terrainToCoordLut[terrain];
								mapMagic.tiles.Unpin(coord);
							}

							//if it's custom
							if (pinnedCustomTerrains.Contains(terrain))
							{
								TerrainTile tileComponent = terrain.gameObject.GetComponent<TerrainTile>();
								mapMagic.tiles.UnpinCustom(tileComponent);
								GameObject.DestroyImmediate(tileComponent);
							}
						}
							
					}
				}

				//redrawing scene
				SceneView.lastActiveSceneView.Repaint();
			}
		}

		#region Selecting

			public static List<Coord> SelectTiles (Vector3 tileSize, bool dotted, Dictionary<Coord,TerrainTile> terrainsLut=null, Transform parent=null)
			/// Selects tile coordinates via click or selection frame
			/// Returns null if selection was not finally made
			/// Could use only the terrainsLut, but that's not so intuitive
			{
				Select.UpdateFrame();

				List<Coord> framedCoords = GetTilesInFrame(Select.frameRect, tileSize, parent);
				
				//displaying frame
				foreach (Coord coord in framedCoords)
					FrameDraw.DrawFrame(coord, tileSize, FrameDraw.pinColor, dotted, terrainsLut, FrameDraw.defaultZOffset*2, parent);

				//returning selected
				if (Select.justReleased || (!Select.isFrame  &&  Event.current.type==EventType.MouseUp  &&  Event.current.button==0  &&  !Event.current.alt))
					return framedCoords;

				return null;
			}


			public static HashSet<Terrain> SelectTerrains (HashSet<Terrain> possibleTerrains, Color color, bool dotted)
			/// Selects terrains via click or selection frame
			{
				Select.UpdateFrame();

				HashSet<Terrain> framedTerrains = GetTerrainsInFrame(Select.frameRect, possibleTerrains);
				
				//displaying frame
				foreach (Terrain terrain in framedTerrains)
					FrameDraw.DrawTerrainFrame(terrain, color, dotted, FrameDraw.defaultZOffset*2);

				//returning selected
				if (Select.justReleased || (!Select.isFrame  &&  Event.current.type==EventType.MouseUp  &&  Event.current.button==0  &&  !Event.current.alt))
					return framedTerrains;

				return null;
			}


			private static HashSet<Terrain> GetTerrainsInFrame (Rect screenFrame, HashSet<Terrain> possibleTerrains)
			{
				HashSet<Terrain> terrains = new HashSet<Terrain>();

				//if frame is small (or single click) selecting terrains by raycast
				Vector2[] screenFrameCorners = new Vector2[] {
					new Vector2(screenFrame.x, screenFrame.y),
					new Vector2(screenFrame.x, screenFrame.y+screenFrame.size.y),
					new Vector2(screenFrame.x+screenFrame.size.x, screenFrame.y),
					new Vector2(screenFrame.x+screenFrame.size.x, screenFrame.y+screenFrame.size.y) };

				for (int c=0; c<screenFrameCorners.Length; c++)
				{
					Ray worldRay = HandleUtility.GUIPointToWorldRay(screenFrameCorners[c]);
					Terrain terrain = GetAimedTerrain(worldRay, possibleTerrains);
					if (terrain == null) continue;
					if (!terrains.Contains(terrain)) terrains.Add(terrain);
				}

				//selecting terrains by their bounding boxes if the frame is larger than one click
				if (screenFrame.size.x > 10 && screenFrame.size.y > 10)
				{
					foreach (Terrain terrain in possibleTerrains)
					{
						Bounds bounds = new Bounds(terrain.transform.position + terrain.terrainData.size/2, terrain.terrainData.size);
						
						if (IsBoundingBoxInFrame(bounds, screenFrame)  &&  !terrains.Contains(terrain))
							terrains.Add(terrain);
					}
				}

				//debug
				//List<Terrain> terrainsList = new List<Terrain>(terrains);
				//for (int i=0; i<terrainsList.Count; i++)
				//{
				//	Vector3 size = terrainsList[i].terrainData.size;
				//	Vector3 pos = terrainsList[i].transform.position;
				//	Handles.DrawWireCube(pos+size/2,size);
				//}

				return terrains;
			}


			private static List<Coord> GetTilesInFrame (Rect screenFrame, Vector3 tileSize, Transform parent=null)
			{
				//transforming frame into 4 world points
				Vector2[] screenFrameCorners = new Vector2[] {
					new Vector2(screenFrame.x, screenFrame.y),
					new Vector2(screenFrame.x, screenFrame.y+screenFrame.size.y),
					new Vector2(screenFrame.x+screenFrame.size.x, screenFrame.y),
					new Vector2(screenFrame.x+screenFrame.size.x, screenFrame.y+screenFrame.size.y) };
					
				Vector3[] worldFrameCorners = new Vector3[4];
				for (int c=0; c<4; c++)
				{
					Ray worldRay = HandleUtility.GUIPointToWorldRay(screenFrameCorners[c]);
					worldFrameCorners[c] = GetAimPosAtZeroLevel(worldRay);

					if (parent != null) 
						worldFrameCorners[c] = parent.InverseTransformPoint(worldFrameCorners[c]);
				}

				//all possible coords rect
				CoordRect allTilesRect = new CoordRect( Coord.Floor(worldFrameCorners[0].Div(tileSize)), new Coord(0,0) );
				allTilesRect.Encapsulate( Coord.Floor(worldFrameCorners[1].Div(tileSize)) );
				allTilesRect.Encapsulate( Coord.Floor(worldFrameCorners[2].Div(tileSize)) );
				allTilesRect.Encapsulate( Coord.Floor(worldFrameCorners[3].Div(tileSize)) );
				allTilesRect.size.x ++;
				allTilesRect.size.z ++;

				//intersecting each tile with frame
				List<Coord> tilesInFrame = new List<Coord>();
				Coord min = allTilesRect.Min;
				Coord max = allTilesRect.Max;
				for (int x=min.x; x<max.x; x++)
					for (int z=min.z; z<max.z; z++)
					{
						Coord tile = new Coord(x,z);
						if (IsInFrame(tile, tileSize, screenFrame, worldFrameCorners))
							tilesInFrame.Add(tile);
					}

				//debug
				//for (int i=0; i<tilesInFrame.Count; i++)
				//{
				//	Coord tile = tilesInFrame[i];
				//	Vector3 pos = tile.vector3*tileSize + new Vector3(tileSize/2, 0, tileSize/2);
				//	Vector3 size = new Vector3(tileSize, 0, tileSize);
				//	Handles.DrawWireCube(pos,size);
				//}

				return tilesInFrame;
			}



			private static bool IsInFrame (Coord tileCoord, Vector3 tileSize, Rect screenFrame, Vector3[] worldFrame)
			{
				//looking if any of the tile corners lies within screen frame in screen space
				Vector3 cor =  tileCoord.vector3.Mul(tileSize);
				Vector2 p1 = HandleUtility.WorldToGUIPoint( cor );
				Vector2 p2 = HandleUtility.WorldToGUIPoint( cor + new Vector3(0,0,tileSize.z) );
				Vector2 p3 = HandleUtility.WorldToGUIPoint( cor + new Vector3(tileSize.x,0,0) );
				Vector2 p4 = HandleUtility.WorldToGUIPoint( cor + new Vector3(tileSize.x,0,tileSize.z) );
				
				if (LineRectIntersection(p1, p2, screenFrame)) return true;
				if (LineRectIntersection(p2, p3, screenFrame)) return true;
				if (LineRectIntersection(p3, p4, screenFrame)) return true;
				if (LineRectIntersection(p4, p1, screenFrame)) return true;

				//looking if any if the screen frame corner rays pass through tile in world space
				Rect worldRect = new Rect(new Vector2(tileCoord.x*tileSize.x, tileCoord.z*tileSize.z), new Vector2(tileSize.x,tileSize.z));
				for (int c=0; c<worldFrame.Length; c++)
				{
					if (worldRect.Contains( new Vector2(worldFrame[c].x, worldFrame[c].z) ))
						return true;
				}

				return false;
			}


			private static bool LineRectIntersection (Vector2 a, Vector2 b, Rect r) 
			/// Finds an intersection between the line and rect in 2D
			{
				Vector2 min = Vector2.Min(a,b);
				Vector2 max = Vector2.Max(a,b);

				if (r.xMin > max.x || r.xMax < min.x ||
					r.yMin > max.y || r.yMax < min.y) 
						return false;

				if (r.xMin < min.x && max.x < r.xMax) return true;
				if (r.yMin < min.y && max.y < r.yMax) return true;

				//line function
				float yAtRectLeft = a.y - (r.xMin - a.x) * ((a.y - b.y) / (b.x - a.x));
				float yAtRectRight = a.y - (r.xMax - a.x) * ((a.y - b.y) / (b.x - a.x));

				if (r.yMax < yAtRectLeft && r.yMax < yAtRectRight) return false;
				if (r.yMin > yAtRectLeft && r.yMin > yAtRectRight) return false;

				return true;
			}


			private static Vector3 GetAimPosAtZeroLevel (Ray aimRay)
			/// Finds ray intersection with zero level and returns a coord 
			{
				//aiming coord tile (aim position at zero level)
				aimRay.direction = aimRay.direction.normalized;
				float aimDist = aimRay.origin.y / (-aimRay.direction.y);
				return aimRay.origin + aimRay.direction*aimDist;
			
			}

			private static Terrain GetAimedTerrain (Ray aimRay, HashSet<Terrain> possibleTerrains=null, HashSet<Terrain> ignoredTerrains=null)
			/// Finds an aimed terrain from the list of all possible terrains (will not aim the terrain that is not in list)
			{
				RaycastHit[] hits = Physics.RaycastAll(aimRay, Mathf.Infinity);
				for (int h=0; h<hits.Length; h++)
				{
					Terrain hitTerrain = hits[h].collider.gameObject.GetComponent<Terrain>();
					if (hitTerrain == null) continue;
					if (possibleTerrains!=null && !possibleTerrains.Contains(hitTerrain)) continue;
					if (ignoredTerrains!=null && ignoredTerrains.Contains(hitTerrain)) continue;

					return hitTerrain;
				}

				return null;
			}

			private static bool IsBoundingBoxInFrame (Bounds bounds, Rect screenFrame)
			{
				//if bounding box edges intersect frame
				Vector3 min = bounds.min;
				Vector3 max = bounds.max;

				Vector2 a = HandleUtility.WorldToGUIPoint(min);
				Vector2 b = HandleUtility.WorldToGUIPoint( new Vector3(max.x, min.y, min.z) );
				Vector2 c = HandleUtility.WorldToGUIPoint( new Vector3(max.x, min.y, max.z) );
				Vector2 d = HandleUtility.WorldToGUIPoint( new Vector3(min.x, min.y, max.z) );

				Vector2 A = HandleUtility.WorldToGUIPoint( new Vector3(min.x, max.y, min.z) );
				Vector2 B = HandleUtility.WorldToGUIPoint( new Vector3(max.x, max.y, min.z) );
				Vector2 C = HandleUtility.WorldToGUIPoint( max );
				Vector2 D = HandleUtility.WorldToGUIPoint( new Vector3(min.x, max.y, max.z) );

				if (LineRectIntersection(a, b, screenFrame)) return true;
				if (LineRectIntersection(b, c, screenFrame)) return true;
				if (LineRectIntersection(c, d, screenFrame)) return true;
				if (LineRectIntersection(d, a, screenFrame)) return true;

				if (LineRectIntersection(A, B, screenFrame)) return true;
				if (LineRectIntersection(B, C, screenFrame)) return true;
				if (LineRectIntersection(C, D, screenFrame)) return true;
				if (LineRectIntersection(D, A, screenFrame)) return true;

				if (LineRectIntersection(a, A, screenFrame)) return true;
				if (LineRectIntersection(b, B, screenFrame)) return true;
				if (LineRectIntersection(c, C, screenFrame)) return true;
				if (LineRectIntersection(d, D, screenFrame)) return true;

				//looking if any if the screen frame corner rays pass through bb in world space
				//TODO, but not actually required - we are checking ray intersection anyways

				return false;
			}

		#endregion



	}
}
