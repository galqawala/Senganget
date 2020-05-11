using UnityEngine;
using System;
using System.Collections.Generic;

namespace Den.Tools
{
	public static class DebugGizmos
	{
		public class GizmoGroup
		{
			public bool enabled = true;
			public List<IGizmo> gizmos = new List<IGizmo>();

			public bool colorOverride;
			public Color color;
		}

		static public Dictionary<string, GizmoGroup> allGizmos = new Dictionary<string, GizmoGroup>();

		#region Gizmos
			public interface IGizmo
			{
				void Draw ();
				Color Color { get; set; }
			}

			struct LineGizmo : IGizmo
			{
				public Vector3 from;
				public Vector3 to;
				public Color Color { get; set; }

				public void Draw ()
				{ 
					#if UNITY_EDITOR
					UnityEditor.Handles.DrawLine(from, to);
					#endif
				}
			}

			struct PolyLineGizmo : IGizmo
			{
				public Vector3[] points;
				public Color Color { get; set; }

				public void Draw ()
				{ 
					#if UNITY_EDITOR
					UnityEditor.Handles.DrawPolyLine(points);
					#endif
				}
			}

			struct SphereGizmo : IGizmo
			{
				public Vector3 pos;
				public float radius;
				public Color Color { get; set; }

				public void Draw ()
				{ 
					#if UNITY_EDITOR
					UnityEditor.Handles.SphereHandleCap(0, pos, Quaternion.identity, radius, EventType.Repaint);
					#endif
				}
			}

			struct RectGizmo : IGizmo
			{
				public Vector2D pos;
				public Vector2D size;
				public Color Color { get; set; }

				public void Draw ()
				{
					#if UNITY_EDITOR
					UnityEditor.Handles.DrawLine(pos, pos + new Vector3(size.x,0,0));
					UnityEditor.Handles.DrawLine(pos + new Vector3(size.x,0,0), pos+size);
					UnityEditor.Handles.DrawLine(pos+size, pos + new Vector3(0,0,size.z));
					UnityEditor.Handles.DrawLine(pos+new Vector3(0,0,size.z), pos);
					#endif
				}
			}

			struct GridGizmo : IGizmo
			{
				public Vector3 pos;
				public Vector3 size;
				public int cellsX;
				public int cellsZ;
				public Color Color { get; set; }

				public void Draw ()
				{ 
					#if UNITY_EDITOR
					float cellSizeX = size.x / cellsX;
					float cellSizeZ = size.z / cellsZ;

					for (int x=0; x<=cellsX; x++)
						UnityEditor.Handles.DrawLine(
							pos + new Vector3(x*cellSizeX, 0, 0),
							pos + new Vector3(x*cellSizeX, 0, size.z));

					for (int z=0; z<=cellsZ; z++)
						UnityEditor.Handles.DrawLine(
							pos + new Vector3(0, 0, z*cellSizeX),
							pos + new Vector3(size.x, 0, z*cellSizeX));
					#endif
				}
			}

			struct DotGizmo : IGizmo
			{
				public Vector3 pos;
				public float size;
				public Color Color { get; set; }

				public void Draw ()
				{ 
					#if UNITY_EDITOR
					float size3d = UnityEditor.HandleUtility.GetHandleSize(pos) * size / 100;
					UnityEditor.Handles.DotHandleCap(0, pos, Quaternion.identity, size3d, EventType.Repaint);
					#endif
				}
			}

		#endregion


		#region Add
			
			public static void DrawLine (string name, Vector3 from, Vector3 to, Color color=new Color(), bool additive=false)
			{
				LineGizmo gizmo = new LineGizmo() { from=from, to=to };
				AddGizmo(name, gizmo, color, clearOthers:!additive); 
			}

			public static void DrawRay (string name, Vector3 pos, Vector3 dir, Color color=new Color(), bool additive=false)
			{
				LineGizmo gizmo = new LineGizmo() { from=pos, to=pos+dir };
				AddGizmo(name, gizmo, color, clearOthers:!additive); 
			}

			public static void DrawPolyLine (string name, Vector3[] points, Color color=new Color(), bool additive=false)
			{
				PolyLineGizmo gizmo = new PolyLineGizmo() { points=points };
				AddGizmo(name, gizmo, color, clearOthers:!additive);
			}

			public static void DrawSphere (string name, Vector3 pos, float radius, Color color=new Color(), bool additive=false)
			{
				SphereGizmo gizmo = new SphereGizmo() { pos=pos, radius=radius };
				AddGizmo(name, gizmo, color, clearOthers:!additive);
			}

			public static void DrawDot (string name, Vector3 pos, float size, Color color=new Color(), bool additive=false)
			{
				DotGizmo gizmo = new DotGizmo() { pos=pos, size=size };
				AddGizmo(name, gizmo, color, clearOthers:!additive);
			}

			public static void DrawGrid (string name, Vector3 worldPos, Vector3 worldSize, int cellsX, int cellsZ, Color color=new Color(), bool additive=false)
			{
				GridGizmo gizmo = new GridGizmo() { pos=worldPos, size=worldSize, cellsX=cellsX, cellsZ=cellsZ };
				AddGizmo(name, gizmo, color, clearOthers:!additive);
			}

			public static void DrawGrid (string name, CoordRect rect, Vector3 worldPos, Vector3 worldSize, Color color=new Color(), bool additive=false)
			{
				GridGizmo gizmo = new GridGizmo() { pos=worldPos, size=worldSize, cellsX=rect.size.x, cellsZ=rect.size.z };
				AddGizmo(name, gizmo, color, clearOthers:!additive);
			}

			public static void DrawRect (string name, Vector2D worldPos, Vector2D worldSize, Color color=new Color(), bool additive=false)
			{
				RectGizmo gizmo = new RectGizmo() { pos=worldPos, size=worldSize };
				AddGizmo(name, gizmo, color, clearOthers:!additive);
			}

			public static void DrawRect (string name, Vector3 worldPos, Vector3 worldSize, Color color=new Color(), bool additive=false)
			{
				RectGizmo gizmo = new RectGizmo() { pos=new Vector2D(worldPos.x, worldPos.z), size=new Vector2D(worldSize.x, worldSize.z) };
				AddGizmo(name, gizmo, color, clearOthers:!additive);
			}


			private static void AddGizmo (string name, IGizmo gizmo, Color color, bool clearOthers=false)
			{
				if (color.r==0 && color.g==0 && color.b==0 && color.a==0)
					color = Color.white;
				gizmo.Color = color;
				
				if (allGizmos.TryGetValue(name, out GizmoGroup group))
				{
					if (clearOthers) group.gizmos.Clear();
					group.gizmos.Add(gizmo);

					allGizmos[name] = group;
				}

				else
				{
					group = new GizmoGroup();
					group.gizmos.Add(gizmo);
					allGizmos.Add(name, group);
				}
			}

			public static void Clear (string name)
			{
				allGizmos.Remove(name);
			}


		#endregion


		#region Draw

			#if UNITY_EDITOR
			[RuntimeInitializeOnLoadMethod, UnityEditor.InitializeOnLoadMethod] 
			static void Subscribe ()
			{
				#if UNITY_2019_1_OR_NEWER
				UnityEditor.SceneView.duringSceneGui += Draw;
				#else
				UnityEditor.SceneView.onSceneGUIDelegate += Draw;
				#endif
			}

			//[UnityEditor.DrawGizmo(UnityEditor.GizmoType.NonSelected | UnityEditor.GizmoType.Active)]
			static public void Draw (UnityEditor.SceneView sceneView) //(Transform objectTransform, UnityEditor.GizmoType gizmoType)
			{
				foreach (var kvp in allGizmos)
				{
					GizmoGroup group = kvp.Value;

					if (!group.enabled)
						continue;

					if (group.colorOverride)
						UnityEditor.Handles.color = group.color;

					for (int i=0; i<group.gizmos.Count; i++)
					{
						if (!group.colorOverride)
							UnityEditor.Handles.color = group.gizmos[i].Color;

						group.gizmos[i].Draw();
					}
				}

				//allGizmos.Clear();
				//will show gizmos only the frame they were assigned
			}
		#endif

		#endregion


		#region Preview Matrix

			public static Action<Matrices.Matrix, string> onPreviewMatrix;
			public static Action<Matrices.Matrix, string> onPreviewMatrixAdd;

			public static void ToMatrixPreview (this Matrices.Matrix matrix, string name=null) { onPreviewMatrix?.Invoke(matrix,name); }
			public static void ToMatrixPreviewCopy (this Matrices.Matrix matrix, string name=null) { onPreviewMatrix?.Invoke(new Matrices.Matrix(matrix), name); }
			public static void ToMatrixPreviewAdd (this Matrices.Matrix matrix, string name=null) { onPreviewMatrixAdd?.Invoke(new Matrices.Matrix(matrix), name); }

			public static void ToMatrixPreviewMainthread (this Matrices.Matrix matrix, string name=null) { Den.Tools.Tasks.CoroutineManager.Enqueue(() => onPreviewMatrix?.Invoke(matrix, name) ); }
			public static void ToMatrixPreviewCopyMainthread (this Matrices.Matrix matrix, string name=null) 
			{ 
				Matrices.Matrix m2 = new Matrices.Matrix(matrix);
				Den.Tools.Tasks.CoroutineManager.Enqueue(() => onPreviewMatrix?.Invoke(m2, name) ); 
			}
			public static void ToMatrixPreviewAddMainthread (this Matrices.Matrix matrix, string name=null) { Den.Tools.Tasks.CoroutineManager.Enqueue(() => onPreviewMatrixAdd?.Invoke(new Matrices.Matrix(matrix), name) ); }

		#endregion
	}
}