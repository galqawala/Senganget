using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using UnityEngine.Profiling;

using Den.Tools;
using Den.Tools.GUI;
using Den.Tools.Splines;
using Den.Tools.Matrices;
using MapMagic.Nodes;
using MapMagic.Terrains;
using MapMagic.Core;
using MapMagic.Products;
//using Plugins.Interface;


namespace MapMagic.Tests
{
	[CustomEditor(typeof(GeneratorsTester))]
	public class GeneratorsTesterInspector : Editor
	{
		UI ui = new UI();
		Type[] allGenTypes;
		string[] allGenNames;
		Type[] allInTypes;
		string[] allInNames;
		Type[] allOutTypes;
		string[] allOutNames;
		

		#region Test On Scene Change Event

			private static HashSet<GeneratorsTester> updatedTesters = null;

			public static void TestOnSceneChange (SceneView sceneView) 
			{ 
				if (updatedTesters == null)
				{
					updatedTesters = new HashSet<GeneratorsTester>();
					
					GeneratorsTester[] allEnabledTesters = FindObjectsOfType<GeneratorsTester>();
					for (int t=0; t<allEnabledTesters.Length; t++)
						if (allEnabledTesters[t].testOnSceneChange) 
							updatedTesters.Add(allEnabledTesters[t]);
				}

				foreach (GeneratorsTester tester in updatedTesters)
					tester.Test();

				//removing disabled
				List<GeneratorsTester> disabled = null;
				foreach (GeneratorsTester tester in updatedTesters)
					if ((UnityEngine.Object)tester != (UnityEngine.Object)null  ||  !tester.isActiveAndEnabled)
					{
						if (disabled == null) disabled = new List<GeneratorsTester>();
						disabled.Add(tester);
					}
				if (disabled != null)
					foreach (GeneratorsTester disTester in disabled)
						updatedTesters.Remove(disTester);
			}

			[RuntimeInitializeOnLoadMethod, UnityEditor.InitializeOnLoadMethod] 
			public static void Subscribe () 
			{ 
				#if UNITY_2019_1_OR_NEWER
				SceneView.duringSceneGui += TestOnSceneChange;
				#else
				SceneView.onSceneGUIDelegate += TestOnSceneChange;
				#endif
			}

		#endregion


		#region Main GUI

			public override void  OnInspectorGUI ()
			{
				ui.Draw(DrawUI);
			}

			private void DrawUI ()
			{
				GeneratorsTester tester = (GeneratorsTester)target;

				using (Cell.LinePx(20))
					if (Draw.Button("Test"))
					{
						tester.Test();
						if (SceneView.lastActiveSceneView != null) SceneView.lastActiveSceneView.Repaint();
					}

				using (Cell.LineStd)
				{
					Draw.ToggleLeft(ref tester.testOnSceneChange, "Test on Scene Change");
					if (tester.testOnSceneChange && !updatedTesters.Contains(tester))
						updatedTesters.Add(tester);
					if (!tester.testOnSceneChange && updatedTesters.Contains(tester))
						updatedTesters.Remove(tester); 

					Draw.ToggleLeft(ref tester.testOnGuiChange, "Test on GUI Change");
				}

				Cell.EmptyLinePx(5);
				using (Cell.LineStd) 
					if (Draw.Button("Benchmark"))
						tester.Benchmark();

				using (Cell.LineStd) Draw.Field(ref tester.benchmarkIterations, "Iterations");
				using (Cell.LineStd) Draw.Field(ref tester.benchmarkApproaches, "Approaches");

				using (Cell.LineStd) Draw.DualLabel("Avg Iteration Time", tester.benchmarkAvgIterationTime.ToString("0.") + " ms");
				using (Cell.LineStd) Draw.DualLabel("Min Iteration Time", tester.benchmarkMinIterationTime.ToString("0.") + " ms");
				using (Cell.LineStd) Draw.DualLabel("MegaPixel Time", tester.benchmarkPerMegapixelTime.ToString("0.") + " ms");
				using (Cell.LineStd) Draw.DualLabel("Total Time", tester.benchmarkTotalTime.ToString("0.") + " ms");


				Cell.EmptyLinePx(5);
				using (Cell.LineStd) 
				{
					int newSeed = Draw.Field(tester.random.Seed, "Seed");
					if (Cell.current.valChanged)
						tester.random = new Noise(newSeed, permutationCount:65536);
				}

				Cell.EmptyLinePx(5);
				using (Cell.LineStd) Draw.Field(ref tester.pixelRect, "Pixel Rect"); 

				using (Cell.LineStd)
				{
					Cell.EmptyRowRel(1-Cell.current.fieldWidth);
					using (Cell.RowRel(Cell.current.fieldWidth))
					{
						using (Cell.Row) if (Draw.Button("32")) tester.pixelRect.size = new Coord(32,32);
						//using (Cell.Row) if (Draw.Button("64")) tester.pixelRect.size = new Coord(64,64);
						using (Cell.Row) if (Draw.Button("128")) tester.pixelRect.size = new Coord(128,128);
						//using (Cell.Row) if (Draw.Button("256")) tester.pixelRect.size = new Coord(256,256);
						using (Cell.Row) if (Draw.Button("512")) tester.pixelRect.size = new Coord(512,512);
						//using (Cell.Row) if (Draw.Button("1024")) tester.pixelRect.size = new Coord(1024,1024);
						using (Cell.Row) if (Draw.Button("2K")) tester.pixelRect.size = new Coord(2048,2048);
					}
				}

				using (Cell.LineStd) Draw.Field(ref tester.margins, "Margins");
				using (Cell.LineStd)
				{
					Cell.EmptyRowRel(1-Cell.current.fieldWidth);
					using (Cell.RowRel(Cell.current.fieldWidth))
					{
						using (Cell.Row) if (Draw.Button("0")) tester.margins = 0;
						using (Cell.Row) if (Draw.Button("2")) tester.margins = 2;
						using (Cell.Row) if (Draw.Button("8")) tester.margins = 8;
						using (Cell.Row) if (Draw.Button("32")) tester.margins = 32;
						using (Cell.Row) if (Draw.Button("128")) tester.margins = 128;
					}
				}

				using (Cell.LineStd) Draw.Field(ref tester.worldRect, "World Rect");
				using (Cell.LineStd) Draw.Field(ref tester.worldHeight, "World Height");



				if (allInTypes == null)
				{
					allInTypes = typeof(GeneratorsTester.ITestInput).Subtypes();
					allInNames = GetTypesNames(allInTypes);
				}

				if (allOutTypes == null)
				{
					allOutTypes = typeof(GeneratorsTester.ITestOutput).Subtypes();
					allOutNames = GetTypesNames(allOutTypes);
				}

				Cell.EmptyLinePx(5);
				using (Cell.LineStd) 
					using (new Draw.FoldoutGroup(ref tester.guiInputs, "Inputs"))
						if (tester.guiInputs)
				{
					if (tester.inputs == null)
						tester.inputs = new GeneratorsTester.ITestInput[0];

					Cell.EmptyLinePx(3);
					using (Cell.LineStd) 
					{
						int inputsCount = Draw.Field(tester.inputs.Length, "Inputs Count");
						if (Cell.current.valChanged)
							ArrayTools.Resize(ref tester.inputs, inputsCount);
					}
					Cell.EmptyLinePx(3);

					for (int i=0; i<tester.inputs.Length; i++)
					{
						GeneratorsTester.ITestInput input = tester.inputs[i];

						Cell.EmptyLinePx(5);
						using (Cell.LineStd)
						{
							int selectedNum = input != null ? allInTypes.Find(input.GetType()) : -1;
							Draw.PopupSelector(ref selectedNum, allInNames, "Input " + i);

							if (Cell.current.valChanged)
							{
								input = (GeneratorsTester.ITestInput)Activator.CreateInstance(allInTypes[selectedNum]);
								tester.inputs[i] = input;
							}
						}

						if (input != null)
							using (Cell.LinePx(0))
						{
							Draw.Class(input);
							Draw.Editor(input);
						}
					}
				}


				Cell.EmptyLinePx(5);
				using (Cell.LineStd) 
					using (new Draw.FoldoutGroup(ref tester.guiGen, "Generator"))
						if (tester.guiGen)
				{
					Cell.EmptyLinePx(3);

					using (Cell.LineStd)
					{
						if (allGenTypes == null)
						{
							allGenTypes = typeof(Generator).Subtypes();
							allGenNames = GetTypesNames(allGenTypes);
						}

						int selectedNum = tester.gen != null ? allGenTypes.Find(tester.gen.GetType()) : -1;
						Draw.PopupSelector(ref selectedNum, allGenNames, "Generator");

						if (Cell.current.valChanged)
							tester.gen = Generator.Create(allGenTypes[selectedNum], null);
					}

					Cell.EmptyLinePx(5);
					if (tester.gen != null)
						using (Cell.LinePx(0))
					{
						Draw.Class(tester.gen);
						Draw.Editor(tester.gen);
					}

					Cell.EmptyLinePx(3);
				}


				Cell.EmptyLinePx(5);
				using (Cell.LineStd) 
					using (new Draw.FoldoutGroup(ref tester.guiOutput, "Output"))
						if (tester.guiOutput)
				{
					Cell.EmptyLinePx(3);

					using (Cell.LineStd)
					{
						int selectedNum = tester.output != null ? allOutTypes.Find(tester.output.GetType()) : -1;
						Draw.PopupSelector(ref selectedNum, allOutNames, "Output");

						if (Cell.current.valChanged)
							tester.output = (GeneratorsTester.ITestOutput)Activator.CreateInstance(allOutTypes[selectedNum]); 
					}

					Cell.EmptyLinePx(5);
					if (tester.output != null)
						using (Cell.LinePx(0))
					{
						Draw.Class(tester.output);
						Draw.Editor(tester.output);
					}

					Cell.EmptyLinePx(3);
				}

				if (Cell.current.valChanged)
					EditorUtility.SetDirty(target);

				if (tester.testOnGuiChange && Cell.current.valChanged)
					tester.Test();
			}


			private static string[] GetTypesNames (Type[] types)
			{
				string[] names = new string[types.Length];
				for (int i=0; i<types.Length; i++)
					names[i] = types[i].Name;
				return names;
			}


		#endregion


		#region Testers GUI

			[Draw.Editor(typeof(GeneratorsTester.TextureInout))]
			public static void DrawTextureInout (GeneratorsTester.TextureInout inout)
			{
				using (Cell.LineStd)
				{
					using (Cell.RowRel(1-Cell.current.fieldWidth))
						Draw.Label("Create New");

					using (Cell.RowRel(Cell.current.fieldWidth))
						if (Draw.Button("Create"))
						{
							//GeneratorTesterWindow window = (GeneratorTesterWindow)GetWindow(typeof (GeneratorTesterWindow));
							inout.texture = TextureExtensions.ColorTexture(512, 512, new Color(0.2f, 0.2f, 0.2f, 1));
							inout.texture.filterMode = FilterMode.Point;
							inout.texture.Apply();
						}
				}

				using (Cell.LineStd) Draw.Toggle(ref inout.previewInScene, "Preview In Scene");
				using (Cell.LineStd) Draw.Toggle(ref inout.preview, "Preview");

				if (inout.preview && inout.texture != null)
					using (Cell.LinePx(256))
				{
					ScrollZoom scrollZoom = new ScrollZoom();
					scrollZoom.scroll = inout.previewScroll;
					scrollZoom.zoom = inout.previewZoom;

					Draw.ScrollableTexture(inout.texture, scrollZoom);

					using (Cell.Custom (5,5,20,20))
						if (Draw.Button("Z")) { scrollZoom.scroll = Vector2.zero; scrollZoom.zoom = 1; }

					inout.previewScroll = scrollZoom.scroll;
					inout.previewZoom = scrollZoom.zoom;
				}
			}

		#endregion

	}
}
