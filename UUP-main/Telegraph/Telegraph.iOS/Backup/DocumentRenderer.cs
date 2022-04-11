using System;
using System.Text;
using System.Threading.Tasks;
using Foundation;
using Telegraph.Models;
using UIKit;

namespace Telegraph.iOS.Backup
{
	public class DocumentRenderer : UIDocument
	{
		public FileData FileData;

		public DocumentRenderer(NSUrl url) : base(url)
		{
		}

		public override bool LoadFromContents(NSObject contents, string typeName, out NSError outError)
		{
			outError = null;

			Console.WriteLine("LoadFromContents({0})", typeName);
			if (contents != null)
			{
				FileData = new FileData(typeName, ToByteArray((NSData)contents), typeName);
			}
			return true;
		}

		public override NSObject ContentsForType(string typeName, out NSError outError)
		{
			outError = null;
			return NSData.FromArray(FileData.Content);
		}

		private byte[] ToByteArray(NSData data)
		{
			var dataBytes = new byte[data.Length];
			System.Runtime.InteropServices.Marshal.Copy(data.Bytes, dataBytes, 0, Convert.ToInt32(data.Length));
			return dataBytes;
		}

	}
}