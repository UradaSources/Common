using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace cxxExpand
{ 
	public class ImageOparationExportTest : MonoBehaviour
	{
		public static bool CreateTImage(string srcPath, string dstPath, int fixedWidth)
		{
			try
			{
				if (!System.IO.File.Exists(srcPath))
				{
					Debug.LogWarning($"file {srcPath} does not exist");
					return false;
				}

				var data = System.IO.File.ReadAllBytes(srcPath);

				unsafe
				{
					fixed (byte* p = data)
					{
						var img = ImageOparationExport.LoadImage((System.IntPtr)p, data.Length);
						if (img.data == System.IntPtr.Zero)
						{
							Debug.LogWarning($"fild {srcPath} load faild");
							return false;
						}

						Debug.Log("file load done");

						const int FixedWidth = 300;

						float r = (float)img.w / img.h;
						int h = (int)(FixedWidth / r);
						var mipImg = ImageOparationExport.ResizeImage(img, FixedWidth, h);
						if (mipImg.data == System.IntPtr.Zero)
						{
							ImageOparationExport.FreeImage(img);

							Debug.LogWarning("resize load faild");
							return false;
						}

						Debug.Log("resize done");

						bool saved = ImageOparationExport.SaveImageToPngFile(dstPath, mipImg);
						if (!saved)
							Debug.LogWarning("save file faild");

						ImageOparationExport.FreeImage(img);
						return saved;
					}
				}
			}
			catch (System.Exception exc)
			{
				Debug.LogWarning(exc);
				return false;
			}
		}

		public void Test1()
		{
			const string SrcPath = "C:/Users/Admin/Desktop/test1.png";
			const string DstPath = "C:/Users/Admin/Desktop/test2.png";

			var data = System.IO.File.ReadAllBytes(SrcPath);

			try
			{
				unsafe
				{
					fixed (byte* p = data)
					{
						var img = ImageOparationExport.LoadImage((System.IntPtr)p, data.Length);
						if (img.data == System.IntPtr.Zero)
						{
							Debug.LogWarning("fild load faild");
							return;
						}

						Debug.Log("load done");

						const int FixedWidth = 300;

						float r = (float)img.w / img.h;
						int h = (int)(FixedWidth / r);
						var mipImg = ImageOparationExport.ResizeImage(img, FixedWidth, h);
						if (mipImg.data == System.IntPtr.Zero)
						{
							ImageOparationExport.FreeImage(img);

							Debug.LogWarning("resize load faild");
							return;
						}

						Debug.Log("resize done");

						bool saved = ImageOparationExport.SaveImageToPngFile(DstPath, mipImg);
						if (!saved)
							Debug.LogWarning("save file faild");

						ImageOparationExport.FreeImage(img);
					}
				}
			}
			catch (System.Exception exc)
			{
				Debug.LogException(exc);
			}
		}

		public void Test2()
		{ 
			const string SrcPath = "C:/Users/Admin/Desktop/test1.png";
			const string DstPath = "C:/Users/Admin/Desktop/test2.png";

			bool success = ImageOparationExport.CreateThumbnail(SrcPath, DstPath, 240);
			if (!success) Debug.Log("create thumbnail faild");
		}

		public void Test3()
		{
			const string SrcPath = "C:/Users/Admin/Desktop/test1.png";
			const string DstPath = "C:/Users/Admin/Desktop/test2.png";

			CreateTImage(SrcPath, DstPath, 240);
		}

		public void Start()
		{
			this.Test3();
		}
	}
}

