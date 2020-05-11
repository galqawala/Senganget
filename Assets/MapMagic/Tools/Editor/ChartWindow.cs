using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Profiling;


namespace Den.Tools
{
	public class ChartWindow : EditorWindow
	{
		public Vector2 range = new Vector2(10,10);
		public Vector2 zoom = new Vector2(10,10);
		public Vector2 scroll = new Vector2(5,5); 
		public Vector2 divisions = new Vector2(1,1);

		public class Chart
		{
			public Vector2[] points;
			public Color color;
		}

		public static List<Chart> charts = new List<Chart>();


		public static void Evaluate (Func<float,float> fn, float start, float end, float step, Color color=new Color())
		{
			Chart chart = new Chart();

			int numPoints = (int)((end-start) / step) + 1;
			chart.points = new Vector2[numPoints];

			for (int i=0; i<numPoints; i++)
			{
				float x = start + step*i;
				float y = fn(x);
				chart.points[i] = new Vector2(x,y);
			}

			if (color.a == 0) color = Color.red;
			chart.color = color;

			charts.Add(chart);
		}

		public static void Clear () { charts.Clear(); }


		private void OnGUI() 
		{
			//charts = new Chart[] { new Chart() { points=new Vector2[] {new Vector2(0,0), new Vector2(1,1), new Vector2(1.5f, 1.5f) }, color=Color.red } };
			//charts.Clear();
			//Evaluate(x => x*x+1.25f, -1, 5, 0.1f);

			EditorGUI.LabelField(new Rect (10,10, 50, 18), "Range");
			EditorGUI.LabelField(new Rect (10,30, 50, 18), "Zoom");

			range = EditorGUI.Vector2Field( new Rect (50,10, 200, 18), "", range);
			zoom = EditorGUI.Vector2Field( new Rect (50,30, 200, 18), "", zoom);



			//scrollZoom.Scroll();
			//scroll = scrollZoom.scroll;
			Scroll(); 

			scroll = UnityEngine.GUI.BeginScrollView(
				position:new Rect(0, 50, position.width, position.height-50), 
				scrollPosition:scroll, 
				viewRect:new Rect(-range.x*zoom.x, -range.y*zoom.y, range.x*2*zoom.x, range.y*2*zoom.y),
				alwaysShowHorizontal:true,
				alwaysShowVertical:true);

				for (float x=-range.x; x<range.x; x+=divisions.x)
				{
					if (x*zoom.x - (scroll.x-range.x*zoom.x) < 0) continue;
					if (x*zoom.x - (scroll.x-range.x*zoom.x) > position.width) continue;

					EditorGUI.DrawRect( new Rect(x*zoom.x, -range.y*zoom.y, 1, range.y*2*zoom.y), Color.gray);
					EditorGUI.LabelField( new Rect(x*zoom.x, -range.y*zoom.y + scroll.y, 40, range.y*2*zoom.y), x.ToString());
				}

				for (float y=-range.y; y<range.y; y+=divisions.y)
				{
					if (y*zoom.y - (scroll.y-range.y*zoom.y) < 0) continue;
					if (y*zoom.y - (scroll.y-range.y*zoom.y) > position.height) continue;

					EditorGUI.DrawRect( new Rect(-range.x*zoom.x, y*zoom.y, range.x*2*zoom.x, 1), Color.gray);
					EditorGUI.LabelField( new Rect(-range.x*zoom.x + scroll.x, y*zoom.y, range.x*2*zoom.x, 18), (-y).ToString());
				}

				EditorGUI.DrawRect( new Rect(0, -range.y*zoom.y, 1, range.y*2*zoom.y), Color.black);
				EditorGUI.DrawRect( new Rect(-range.x*zoom.x, 0, range.x*2*zoom.x, 1), Color.black);

				int chartsCount = charts.Count;
				for (int c=0; c<chartsCount; c++)
				{
					Chart chart = charts[c];
					Handles.color = chart.color;

					Vector2 prevPoint = new Vector2();
					for (int i=0; i<chart.points.Length; i++)
					{
						Vector2 point = chart.points[i];
						point = new Vector2(point.x*zoom.x, -point.y*zoom.y);
						EditorGUI.DrawRect( new Rect(point.x-1, point.y-1, 3,3), chart.color); 

						if (i!=0)
							Handles.DrawLine(prevPoint, point);

						prevPoint = point;
					}
				}


			UnityEngine.GUI.EndScrollView();
		}


		private bool isScrolling = false;
		private Vector2 clickPos;
		private Vector2 clickScroll = new Vector2(0,0);

		public void Scroll()
		{
			if (Event.current.type == EventType.MouseDown)
			{
				clickPos = Event.current.mousePosition;
				clickScroll = scroll;
				isScrolling = true;
			}

			if (Event.current.type == EventType.MouseDown  &&  Event.current.alt)  //alternative way to scroll
			{
				clickPos = Event.current.mousePosition;
				clickScroll = scroll;
				isScrolling = true;
			}

			if (Event.current.rawType == EventType.MouseUp) 
			{
				isScrolling = false;
			}
			
			if (isScrolling)
			{
				scroll = clickScroll - Event.current.mousePosition + clickPos;
				Repaint();
			}
		}


		[MenuItem ("Window/Test/Chart")]
		public static void ShowWindow ()
		{
			ChartWindow window = (ChartWindow)GetWindow(typeof (ChartWindow));
			window.position = new Rect(100,100,300,300);
		}
	}
}