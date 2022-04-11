using System;
using System.Threading;

using Foundation;
using UIKit;
using System.Text;
using Telegraph.Models;
using System.Collections.Generic;
using Telegraph.Backup;
using Xamarin.Forms;
using Telegraph.iOS.Backup;
using Utils;

[assembly: Dependency(typeof(ICloudHelper))]
namespace Telegraph.iOS.Backup
{
    public class ICloudHelper : IDriveService
    {
        bool HasiCloud { get; set; }
        bool CheckingForiCloud { get; set; }

        NSUrl iCloudUrl { get; set; }

        public ICloudHelper()
        {
        }

        public void Init()
        {
            ThreadPool.QueueUserWorkItem(_ => {
                CheckingForiCloud = true;
                Console.WriteLine("Checking for iCloud");
                var uburl = NSFileManager.DefaultManager.GetUrlForUbiquityContainer(null);

                if (uburl == null)
                {
                    XamarinShared.Setup.RemoveSecureValue("Backup");
                    HasiCloud = false;
                    Console.WriteLine("Can't find iCloud container, check your provisioning profile and entitlements");
                    App.DriveLoginFailed();
                }
                else
                {
                    XamarinShared.Setup.SetSecureValue("Backup", "true");
                    HasiCloud = true;
                    iCloudUrl = uburl;
                    Console.WriteLine("yyy Yes iCloud! {0}", uburl.AbsoluteUrl);
                    if (App.Passphrase != null)
                        App.RestoreBackup();
                    else
                        App.UploadBackup();
                }
                CheckingForiCloud = false;
            });
        }

        public string CreateOrUpdateFile(string folderId, string fileName, byte[] content)
        {
            DocumentRenderer doc = new DocumentRenderer(MakeUrl(fileName, folderId));
            doc.FileData = new FileData(fileName, content, fileName);
            doc.Save(doc.FileUrl, UIDocumentSaveOperation.ForOverwriting, (success) => Console.WriteLine("Success " + success));
            doc.CloseAsync();
            return fileName;
        }

        public void DeleteFile(string fileId, string folderId = null)
        {
            NSFileManager.DefaultManager.Remove(MakeUrl(fileId, folderId).Path, out NSError error);
        }

        public string CreateFolder(string folderName, string parentFolderId)
        {
            var url = iCloudUrl.Append("Documents", true);
            if (parentFolderId != null)
                url = url.Append(parentFolderId, true);
            url = url.Append(folderName, true);
            NSFileManager.DefaultManager.CreateDirectory(url.Path, true, null);
            return folderName;
        }

        public FileData GetFileById(string folderId, string fileId)
        {
            var doc = new DocumentRenderer(MakeUrl(fileId, folderId));
            bool isSuccess = AsyncUtil.RunSync(() => doc.OpenAsync());

            if (isSuccess)
            {
                Console.WriteLine("iCloud document opened");
                Console.WriteLine(" -- {0}", doc.FileData.Name);
            }
            else
            {
                Console.WriteLine("failed to open iCloud document");
            }
            FileData fileData = doc.FileData;
            doc.Dispose();
            return fileData;
        }

        public FileData GetFileByName(string folderId, string fileName)
        {
            return GetFileById(folderId, fileName);
        }

        public List<FileData> GetFilesByFolderId(string folderId)
        {
            List<FileData> fileDatas = new List<FileData>();
            var folders = NSFileManager.DefaultManager.GetDirectoryContent(MakeUrl(null, folderId).Path, out NSError error);
            foreach (string folder in folders)
                fileDatas.Add(new FileData() { Name = folder, Id = folder });
            return fileDatas;
        }

        public List<FileData> GetFolders(string parentFolderId)
        {
            return GetFilesByFolderId(parentFolderId);
        }

        public byte[] ReadFileContent(string fileId, string folderId = null)
        {
            if (GetFileById(folderId, fileId) != null)
                return GetFileById(folderId, fileId).Content;
            return null;
        }

        public bool SupportBackup()
        {
            return HasiCloud;
        }

        private NSUrl MakeUrl(string fname = null, string chatId = null)
        {
            var url = iCloudUrl.Append("Documents", true);
            if (chatId != null)
            {
                url = url.Append(chatId, true);
                bool isDir = true;
                bool isDirExists = NSFileManager.DefaultManager.FileExists(url.Path, ref isDir);
                if (!isDirExists)
                    NSFileManager.DefaultManager.CreateDirectory(url.Path, true, null);

            }
            if (fname != null) url = url.Append(fname, false);
            return url;
        }

        public FileData GetFolder(string folderName) => new FileData(folderName, null, folderName);
    }
}
