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
	public static class GraphPopup
	{

			public static PopupMenu.Item GraphItems (Graph graph, int priority=1)
			{
				PopupMenu.Item graphItems = new PopupMenu.Item("Graph");
				graphItems.onDraw = RightClick.DrawItem;
				graphItems.icon = RightClick.texturesCache.GetTexture("MapMagic/Popup/Graph");
				graphItems.color = Color.gray;
				graphItems.subItems = new List<PopupMenu.Item>();
				graphItems.priority = priority;

				PopupMenu.Item importItem = new PopupMenu.Item("Import", onDraw:RightClick.DrawItem, priority:9) { icon = RightClick.texturesCache.GetTexture("MapMagic/Popup/Import"), color = Color.gray };
				importItem.onClick = ()=>
				{
					Graph imported = ScriptableAssetExtensions.LoadAsset<Graph>("Load Graph", filters: new string[]{"Asset","asset"} );
					if (imported != null)
						graph.Import(imported);
				};
				graphItems.subItems.Add(importItem);
				
				PopupMenu.Item exportItem = new PopupMenu.Item("Export", onDraw:RightClick.DrawItem, priority:9) { icon = RightClick.texturesCache.GetTexture("MapMagic/Popup/Export"), color = Color.gray };
				exportItem.disabled = GraphWindow.current.selected==null || GraphWindow.current.selected.Count==0;
				exportItem.onClick = ()=>
				{
					Graph exported = graph.Export(GraphWindow.current.selected);
					ScriptableAssetExtensions.SaveAsset(exported, caption:"Save Graph", type:"asset");
				};
				graphItems.subItems.Add(exportItem);
				
				graphItems.subItems.Add( new PopupMenu.Item("Update All", onDraw:RightClick.DrawItem, priority:7) { icon = RightClick.texturesCache.GetTexture("MapMagic/Popup/Update"), color = Color.gray } );

				//graphItems.subItems.Add( new PopupMenu.Item("Background", onClick:()=>GraphWindow.current.noBackground=!GraphWindow.current.noBackground, onDraw:RightClick.DrawItem, priority:8));
				//graphItems.subItems.Add( new PopupMenu.Item("Debug", onClick:()=>GraphWindow.current.drawGenDebug=!GraphWindow.current.drawGenDebug, onDraw:RightClick.DrawItem, priority:9));

				return graphItems;
			} 
	}
}