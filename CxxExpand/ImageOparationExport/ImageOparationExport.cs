#define INCLUDE_IMAGE_OPARATION_EXPORT

using System.Runtime.InteropServices;

namespace cxxExpand
{
	public static class ImageOparationExport
	{
		[StructLayout(LayoutKind.Sequential)]
		public struct ImgHandle
		{
			public int w;
			public int h;
			public int cmop;
			public System.IntPtr data;
		}

		[DllImport("ImageOparationExport.dll")]
		public static extern ImgHandle LoadImage(System.IntPtr data, int len);

		[DllImport("ImageOparationExport.dll")]
		public static extern void FreeImage(ImgHandle img);

		[DllImport("ImageOparationExport.dll")]
		public static extern ImgHandle ResizeImage(ImgHandle img, int w, int h);

		[DllImport("ImageOparationExport.dll")]
		public static extern bool SaveImageToPngFile(string path, ImgHandle img);

		[DllImport("ImageOparationExport.dll")]
		public static extern bool CreateThumbnail(string srcPath, string dstPath, int fixedWidth);
	}
}
