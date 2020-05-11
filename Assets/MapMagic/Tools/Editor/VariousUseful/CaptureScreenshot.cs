using UnityEngine;
using UnityEditor;

public class CaptureScreenshot
{
	[MenuItem ("Edit/CaptureScreenshot")]
	static void CaptureScreenshotFn ()
	{
		//Application.CaptureScreenshot("Screenshot.png");
		string date = System.DateTime.Now.Year + "_" + System.DateTime.Now.Month + "_" + System.DateTime.Now.Day + " " + System.DateTime.Now.Hour + "_" + System.DateTime.Now.Minute + "_" + System.DateTime.Now.Second;
		ScreenCapture.CaptureScreenshot("C:\\Users\\Wraith\\Desktop\\Screens\\Screenshot " + date + ".png", 5);
		Debug.Log("Screen captured to C:\\Users\\Wraith\\Desktop\\Screens\\Screenshot " + date + ".png");
	}
}