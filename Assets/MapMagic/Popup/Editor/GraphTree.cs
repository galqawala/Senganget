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
	public static class GraphTreePopup
	{
		public static void DrawGraphTree (Graph rootGraph)
		{
			List<PopupMenu.Item> items = new List<PopupMenu.Item>();
			FillSubGraphItems(rootGraph, rootGraph, "", items);

			PopupMenu menu = new PopupMenu() { items=items, sortItems=false };  //items=new List<PopupMenu.Item>() {item}
			menu.Show(Event.current.mousePosition);
		}


		private static void FillSubGraphItems (Graph graph, Graph root, string prefix, List<PopupMenu.Item> items)
		{
			PopupMenu.Item item = new PopupMenu.Item(prefix + graph.name);
			items.Add(item);
			item.onClick = () => GraphWindow.current.OpenBiome(graph, root);

			foreach (Graph subGraph in graph.SubGraphs())
				FillSubGraphItems(subGraph, root, $"{prefix}   ", items);
		}
	}
}