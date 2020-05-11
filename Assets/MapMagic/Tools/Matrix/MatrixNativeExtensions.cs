# if MM_NATIVE

using UnityEngine;
using System.Runtime.InteropServices;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Tests")]

namespace Den.Tools.Matrices
{
	public static class MatrixNativeExtensions
	{
		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="Matrix_ImportRawBytes")] 
		public static extern void ImportRawBytes (Matrix thism, byte[] bytes, int bytesLength, Coord bytesOffset, Coord bytesSize, int start, int step);

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="Matrix_ImportRaw16")] 
		public static extern void ImportRaw16 (Matrix thism, byte[] bytes, int bytesLength, Coord texOffset, Coord texSize);
				
		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="Matrix_ImportRawFloat")] 
		public static extern void ImportRawFloat (Matrix thism, byte[] bytes, int bytesLength, Coord texOffset, Coord texSize, float mult = 1);

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="Matrix_ExportRawBytes")] 
		public static extern void ExportRawBytes (Matrix thism, byte[] bytes, int bytesLength, Coord bytesOffset, Coord bytesSize, int start, int step);

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="Matrix_ExportRaw16")] 
		public static extern void ExportRaw16 (Matrix thism, byte[] bytes, int bytesLength, Coord texOffset, Coord texSize);
				
		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="Matrix_ExportRawFloat")] 
		public static extern int ExportRawFloat (Matrix thism, byte[] bytes, int bytesLength, Coord texOffset, Coord texSize, float mult = 1);



		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixFillVal")] 
		public static extern void Fill (this Matrix thisMatrix, float val);

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixFill")] 
		public static extern void Fill (this Matrix thisMatrix, Matrix m);

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixFillOpacity")] 
		public static extern void Fill (this Matrix thisMatrix, float val, float opacity);
			
		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixMix")] 
		public static extern void Mix (this Matrix thisMatrix, Matrix m, float opacity=1);

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixMixMask")] 
		public static extern void Mix (this Matrix thisMatrix, Matrix m, Matrix mask);

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixMixMaskOpacity")] 
		public static extern void Mix (this Matrix thisMatrix, Matrix m, Matrix mask, float opacity);

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixMixComplex")] 
		public static extern void Mix (this Matrix thisMatrix, Matrix m, Matrix mask, float maskMin, float maskMax, bool maskInvert, bool fallof, float opacity);

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixInvMix")] 
		public static extern void InvMix (this Matrix thisMatrix, Matrix m, Matrix invMask, float opacity=1);
		//using inverted mask: mask1 will leave original value, mask0 will use m

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixAdd")] 
		public static extern void Add (this Matrix thisMatrix, Matrix add, float opacity=1);

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixAddMask")] 
		public static extern void Add (this Matrix thisMatrix, Matrix add, Matrix mask, float opcaity=1);

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixAddVal")] 
		public static extern void Add (this Matrix thisMatrix, float add);

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixBlend")] 
		public static extern void Blend (this Matrix thisMatrix, Matrix matrix, Matrix mask, Matrix add, Matrix addMask);

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixMaxComplex")] 
		public static extern void Max (this Matrix thisMatrix, Matrix matrix, Matrix mask, Matrix add, Matrix addMask);

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixStep")] 
		public static extern void Step (this Matrix thisMatrix, float mid=0.5f);

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixSubtract")] 
		public static extern void Subtract (this Matrix thisMatrix, Matrix m, float opacity=1);

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixInvSubtract")] 
		public static extern void InvSubtract (this Matrix thisMatrix, Matrix m, float opacity=1);
		/// subtracting this matrix from m

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixMultiply")] 
		public static extern void Multiply (this Matrix thisMatrix, Matrix m, float opacity=1);

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixMultiplyVal")] 
		public static extern void Multiply (this Matrix thisMatrix, float m);

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixContrast")] 
		public static extern void Contrast (this Matrix thisMatrix, float m);
		/// Leaving 0.5 values untouched, and increasing/shrinking 1-0 range

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixDivide")] 
		public static extern void Divide (this Matrix thisMatrix, Matrix m, float opacity=1);

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixDifference")] 
		public static extern void Difference (this Matrix thisMatrix, Matrix m, float opacity=1);

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixOverlay")] 
		public static extern void Overlay (this Matrix thisMatrix, Matrix m, float opacity=1);

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixHardLight")] 
		public static extern void HardLight (this Matrix thisMatrix, Matrix m, float opacity=1);
		/// Same as overlay but estimating b>0.5

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixSoftLight")] 
		public static extern void SoftLight (this Matrix thisMatrix, Matrix m, float opacity=1);

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixMax")] 
		public static extern void Max (this Matrix thisMatrix, Matrix m, float opacity=1);

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixMin")] 
		public static extern void Min (this Matrix thisMatrix, Matrix m, float opacity=1);

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixInvert")] 
		public static extern void Invert(this Matrix thisMatrix);

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixInvertOne")] 
		public static extern void InvertOne(this Matrix thisMatrix);

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixSelectRange")] 
		public static extern void SelectRange (this Matrix thisMatrix, float minFrom, float minTo, float maxFrom, float maxTo);
		/// Fill all values within min1-max0 with 1, while min0-1 and max0-1 are filled with blended

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixChangeRange")] 
		public static extern void ChangeRange (this Matrix thisMatrix, float fromMin, float fromMax, float toMin, float toMax);
		/// Used to convert matrix from -1 1 range to 0 1 or vice versa

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixClamp01")] 
		public static extern void Clamp01 (this Matrix thisMatrix);

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixMaxValue")] 
		public static extern float MaxValue (this Matrix thisMatrix);

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixMinValue")] 
		public static extern float MinValue (this Matrix thisMatrix);

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="Average")] 
		public static extern float Average (this Matrix thisMatrix);

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixIsEmpty")] 
		public static extern bool IsEmpty (this Matrix thisMatrix);
		/// Better than MinValue since it can quit if matrix is not empty

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixIsEmptyDelta")] 
		public static extern bool IsEmpty (this Matrix thisMatrix, float delta);

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixBlackWhite")] 
		public static extern void BlackWhite (this Matrix thisMatrix, float mid);
		/// Sets all values bigger than mid to white (1), and those lower to black (0)

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixBrightnessContrast")] 
		public static extern void BrighnesContrast (this Matrix thisMatrix, float brightness, float contrast);

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixTerrace")] 
		private static extern void Terrace (this Matrix thisMatrix, float[] terraces, int terraceCount, float steepness);
		public static void Terrace (this Matrix thisMatrix, float[] terraces, float steepness) => Terrace(thisMatrix, terraces, terraces.Length, steepness);

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixLevels")] 
		public static extern void Levels (this Matrix thisMatrix, float inMin, float inMax, float gamma, float outMin, float outMax);

		[DllImport ("NativePlugins", CallingConvention=CallingConvention.Cdecl, EntryPoint="MatrixUniformCurve")] 
		private static extern void UniformCurve (this Matrix thisMatrix, float[] lut, int lutCount);
		public static void UniformCurve (this Matrix thisMatrix, float[] lut) => UniformCurve(thisMatrix, lut, lut.Length);
		/// Applies curve that got curve lut with uniformly placed times
		/// A copy of curve's EvaluateLuted
	}
}

#endif