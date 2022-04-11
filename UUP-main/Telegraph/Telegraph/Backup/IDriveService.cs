using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegraph.Models;

namespace Telegraph.Backup
{
    public interface IDriveService
    {
        void Init();

        string CreateOrUpdateFile(string folderId, string fileName, byte[] content);

        void DeleteFile(string fileId, string folderId = null);

        string CreateFolder(string folderName, string parentFolderId);

        FileData GetFolder(string folderName);

        FileData GetFileById(string folderId, string fileId);

        FileData GetFileByName(string folderId, string fileName);

        List<FileData> GetFilesByFolderId(string folderId);

        List<FileData> GetFolders(string parentFolderId);

        byte[] ReadFileContent(string fileId, string folderId = null);

        bool SupportBackup();

    }
}
