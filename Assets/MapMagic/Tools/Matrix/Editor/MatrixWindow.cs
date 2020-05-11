﻿using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using UnityEngine.Profiling;

using Den.Tools;
using Den.Tools.Matrices;
using Den.Tools.GUI;


namespace Den.Tools.Matrices.Window
{
	[System.Serializable]
	public class MatrixWindow : BaseMatrixWindow
	{
		[SerializeField] Matrix matrix;
		public override Matrix Matrix => matrix;

		Texture2D previewTexture;
		public override Texture2D PreviewTexture => previewTexture;

		[RuntimeInitializeOnLoadMethod, UnityEditor.InitializeOnLoadMethod]
		public static void Subscribe ()
		{
			DebugGizmos.onPreviewMatrix += (m,n) => OpenUpdate(m,n);
			Matrix.onPreview += (m,n) => OpenUpdate(m,n);
		}

		public static MatrixWindow GetWindow (Matrix matrix, string name=null)
		/// Tries to find opened window by matrix (and name if provided)
		{
			MatrixWindow[] windows = Resources.FindObjectsOfTypeAll<MatrixWindow>();

			for (int i=0; i<windows.Length; i++)
				if (windows[i].matrix == matrix || (name!=null && windows[i].name==name))
					return windows[i];

			return null;
		}


		public static MatrixWindow GetCreateWindow (Matrix matrix, string name=null)
		/// Tries to find opened window by matrix/name and updates it, and opens new window if non is found
		{
			MatrixWindow window = GetWindow(matrix, name);

			if (window == null)
			{
				window = CreateInstance<MatrixWindow>();
				window.ShowTab();
				window.SetMatrix(matrix, name);
			}

			window.Show();

			return window;
		}


		public void SetMatrix (Matrix matrix, string name=null)
		{
			//generating preview texture 
			if (previewTexture == null || previewTexture.width != matrix.rect.size.x || previewTexture.height != matrix.rect.size.z)
			{
				previewTexture = new Texture2D(matrix.rect.size.x, matrix.rect.size.z, TextureFormat.RFloat, false, true);
				previewTexture.filterMode = FilterMode.Point;
			}

			matrix.ExportTextureRaw(previewTexture);

			SetMatrix(matrix, previewTexture);

			if (name != null) this.name = name;
		}


		public void SetMatrix (Matrix matrix, Texture2D previewTexture)
		/// In case we already have preview texture (generated by MapMagic for instance)
		{
			this.matrix = matrix;
			this.previewTexture = previewTexture;

			foreach (IPlugin plugin in plugins)
				plugin.Setup(matrix);

			Repaint();
		}


		public static void OpenUpdate (Matrix matrix, string name=null)
		{
			MatrixWindow window = GetCreateWindow(matrix, name);
			if (name != null) window.name = name;
			else window.name = "Matrix Preview";
			window.SetMatrix(matrix);
		}

		public static void OpenUpdate (Matrix matrix, Texture2D tex, string name=null)
		{
			MatrixWindow window = GetCreateWindow(matrix, name);
			if (name != null) window.name = name;
			else window.name = "Matrix Preview";
			window.SetMatrix(matrix, tex);
		}


		[MenuItem ("Window/Test/Matrix Preview")]
		public static void ShowWindow ()
		{
			MatrixWindow window = (MatrixWindow)GetWindow(typeof (MatrixWindow));
			window.position = new Rect(100,100,300,800);
		}

		[MenuItem ("Assets/To Matrix Preview")]
		private static void LoadToMatrixPreviewWindow()
		{
			Texture2D tex = Selection.activeObject as Texture2D;
			if (tex == null) return;

			Matrix matrix = new Matrix( new CoordRect(0,0,tex.width, tex.height) );
			matrix.ImportTexture(tex, channel:0);

			GetCreateWindow(matrix, tex.name);
		}

		[MenuItem ("Assets/To Matrix Preview", true)]
		private static bool LoadToMatrixPreviewWindowValidation()
		{
			return Selection.activeObject is Texture2D;
		}
	}

	[System.Serializable]
	public abstract class BaseMatrixWindow : EditorWindow
	{
		public abstract Matrix Matrix { get; } 
		public abstract Texture2D PreviewTexture { get; }
		private Material textureRawMat;

		public bool colorize;
		public bool relief;
		public float min = 0;
		public float max = 1;

		UI toolbarUI = new UI();
		UI previewUI = UI.ScrolledUI(maxZoom:16);
		Vector2 serializedScroll;
		bool serializedScrollStored = false;
		float serializedZoom = 1;

		const int toolbarWidth = 260; //128+4

		public IPlugin[] plugins = new IPlugin[] { 
			new StatsPlugin(), 
			new ViewPlugin(),
			new PixelPlugin(),
			new SlicePlugin(),
			new ProcessPlugin(),
			new ImportPlugin(),
			new ExportPlugin() };



		#region Scroll/Zoom

			public Vector2 Scroll 
			{ 
				get{ return previewUI.scrollZoom.scroll;} 
				set{ previewUI.scrollZoom.scroll=value; serializedScroll=value; } 
			}

			public float Zoom 
			{ 
				get{ return previewUI.scrollZoom.zoom;} 
				set{previewUI.scrollZoom.zoom=value; serializedZoom=value;} 
			}

			public Vector2 ScrollZero => new Vector2(0,0);

			public Vector2 ScrollCenter 
			{get{
				if (Matrix==null) return Vector2.zero;
				else return new Vector2(
				(-Matrix.rect.offset.x-Matrix.rect.size.x/2) * previewUI.scrollZoom.zoom - toolbarWidth/2 + Screen.width/2, //or + previewUI.rootVisualRect.width/2, 
				(Matrix.rect.offset.z+Matrix.rect.size.z/2) * previewUI.scrollZoom.zoom + Screen.height/2 );
			}}

			public Rect MatrixRect
			{get{
				if (Matrix == null) return new Rect(0,0,0,0);
				else return new Rect(Matrix.rect.offset.x, -Matrix.rect.offset.z-Matrix.rect.size.z, Matrix.rect.size.x, Matrix.rect.size.z);
			}}

			public Rect ToMatrixRect (CoordRect srcRect) =>
				 new Rect(srcRect.offset.x, -srcRect.offset.z-srcRect.size.z, srcRect.size.x, srcRect.size.z);
				

		#endregion


		#region GUI

			private void OnGUI () 
			{
				titleContent = new GUIContent(name);

				if (serializedScrollStored  ||  Matrix==null) previewUI.scrollZoom.scroll = serializedScroll;
				else previewUI.scrollZoom.scroll = ScrollCenter;
				previewUI.scrollZoom.zoom = serializedZoom;

				previewUI.Draw(DrawPreview, offsetAfterDraw:false);
				toolbarUI.Draw(DrawToolbar);

				serializedScroll = previewUI.scrollZoom.scroll;
				serializedScrollStored = true;
				serializedZoom = previewUI.scrollZoom.zoom; 
			}


			protected virtual void DrawPreview ()
			{
				//background
				Rect displayRect = new Rect(0, 0, Screen.width, Screen.height);
				float gridBackgroundColor = !StylesCache.isPro ? 0.45f : 0.2f;
				float gridColor = !StylesCache.isPro ? 0.5f : 0.23f;
				Draw.StaticGrid(
					displayRect: displayRect,
					cellSize:128,
					color:new Color(gridColor,gridColor,gridColor), 
					background:new Color(gridBackgroundColor,gridBackgroundColor,gridBackgroundColor),
					fadeWithZoom:true);

				using (Cell.Custom(MatrixRect))
					Draw.MatrixPreviewTexture(PreviewTexture, colorize:colorize, relief:relief, min:min, max:max);

				Draw.StaticAxis(displayRect, 0, false, Color.red);
				Draw.StaticAxis(displayRect, 0, true, Color.blue);

				foreach (IPlugin plugin in plugins)
					plugin.DrawWindow(Matrix, this);
			}


			protected virtual void DrawToolbar ()
			{
				Cell.EmptyRow();
				using (Cell.RowPx(toolbarWidth)) 
				{
					foreach (IPlugin plugin in plugins)
					{
						Cell.EmptyLinePx(6);
						using (Cell.LineStd)
						{
							bool enabled = plugin.Enabled;

							using (new Draw.FoldoutGroup(ref enabled, plugin.Name, isLeft:false, style:UI.current.styles.foldoutOpaque, backgroundWhileClosed:true))
								if (enabled)
									plugin.DrawInspector(Matrix, this);
								
							plugin.Enabled = enabled;
						}
					}
				}

				Cell.EmptyRowPx(8);
			}

		#endregion

	}
}
