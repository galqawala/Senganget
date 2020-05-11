using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Profiling;
using UnityEditor;

using Den.Tools;
using Den.Tools.GUI;
using MapMagic.Core;
using MapMagic.Nodes;
using MapMagic.Nodes.GUI;

namespace MapMagic.Nodes.GUI
{
	public static class PortalSelectorPopup
	{
		private static GUIStyle itemTextStyle;

		public static void DrawPortalSelector (Graph graph, IPortalExit<object> portalExit)
		{
			//if (MapMagic.instance.guiGens == null) MapMagic.instance.guiGens = MapMagic.instance.gens;
			//GeneratorsAsset gens = MapMagic.instance.guiGens;
			//if (MapMagic.instance.guiGens != null) gens = MapMagic.instance.guiGens;

			Type exitType = typeof(Den.Tools.Matrices.MatrixWorld); //portalExit.GetType().GetGenericArguments()[0];
			//TODO: generic portals

			if (itemTextStyle == null)
			{
				itemTextStyle = new GUIStyle(UnityEditor.EditorStyles.label); 
				itemTextStyle.normal.textColor = itemTextStyle.focused.textColor = itemTextStyle.active.textColor = Color.black;
			}
			
			List<PopupMenu.Item> enterPortalsItems = new List<PopupMenu.Item>();

			for (int g=0; g<graph.generators.Length; g++)
			{
				IPortalEnter<object> portalEnter = graph.generators[g] as IPortalEnter<object>;
				if (portalEnter == null) continue;
				//if (portalEnter.GetType().GetGenericArguments()[0] != exitType) continue;
				//TODO: generic portals

				PopupMenu.Item item = new PopupMenu.Item( portalEnter.Name, 
					onDraw: (i, r) => EditorGUI.LabelField(r, i.name, itemTextStyle),
					onClick: ()=>
					{ 
						if (graph.AreDependent((Generator)portalExit, (Generator)portalEnter))   
							{ EditorUtility.DisplayDialog("MapMagic", "Linking portals this way will create a dependency loop.", "Cancel"); return; }
					
						portalExit.AssignEnter(portalEnter, graph);

						GraphWindow.RefreshMapMagic();
					} );

				enterPortalsItems.Add(item);
			}

			PopupMenu menu = new PopupMenu() { items=enterPortalsItems };
			menu.Show(Event.current.mousePosition);

		}

	}
}