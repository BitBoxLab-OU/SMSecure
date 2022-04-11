using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Newtonsoft.Json;
using Xamarin.Essentials;
using Xamarin.Auth.Presenters;
using Xamarin.Forms;
using Telegraph.Droid.Backup;
using Telegraph.Models;
using Telegraph.Backup;
using Utils;

[assembly: Dependency(typeof(GoogleDriveHelper))]
namespace Telegraph.Droid.Backup
{
    public class GoogleDriveHelper : IDriveService
    {

        private static string scope = "https://www.googleapis.com/auth/drive.appdata";
        private static string clientId = "418506772713-9clalb263heqpd2ovs9f66kbrhspglep.apps.googleusercontent.com";
        private static string redirectUrl = "com.dtsocialize.uup:/oauth2redirect";

        private static DriveService driveService;

        public static OAuth2Authenticator Auth { get; private set; }

        public GoogleDriveHelper()
        {
        }
        public byte[] ReadFileContent(string fileId, string folderId)
        {
            using System.IO.MemoryStream ms = new System.IO.MemoryStream();
            driveService.Files.Get(fileId).Download(ms);
            return ReadFully(ms);
        }

        public string CreateFolder(string folderName, string parentId = null)
        {
            FileData folder = GetFolder(folderName);
            if (folder != null) return folder.Id;
            var FileMetaData = new File();
            FileMetaData.Name = folderName;

            FileMetaData.Parents = new List<string>() { parentId ?? "appDataFolder" };
            FileMetaData.MimeType = "application/vnd.google-apps.folder";

            FilesResource.CreateRequest request = driveService.Files.Create(FileMetaData);
            request.Fields = "id";
            var file = request.Execute();
            return file.Id;
        }

        public FileData GetFolder(string folderName)
        {
            FilesResource.ListRequest FileListRequest = driveService.Files.List();
            FileListRequest.Spaces = "appDataFolder";
            FileListRequest.Q = "mimeType = 'application/vnd.google-apps.folder' and trashed = false and name = '" + folderName + "'";
            IList<File> files = FileListRequest.Execute().Files;
            return GetFirstFileData(files);

        }

        public string CreateOrUpdateFile(string folderId, string fileName, byte[] content)
        {
            var file = GetFileByName(folderId, fileName);
            var fileId = file == null ? CreateFile(folderId, fileName).Result : file.Id;
            File metadata = new File()
            {
                Name = fileName,
            };
            driveService.Files.Update(metadata, fileId, new System.IO.MemoryStream(content), "text/plain").UploadAsync();
            return fileId;
        }

        public void DeleteFile(string fileId, string folderId = null)
        {
            driveService.Files.Delete(fileId).ExecuteAsync();
        }

        public FileData GetFileById(string folderId, string fileId)
        {
            FilesResource.ListRequest FileListRequest = driveService.Files.List();
            FileListRequest.Spaces = "appDataFolder";
            FileListRequest.Fields = "nextPageToken, files(*)";
            FileListRequest.Q = "'" + folderId + "' in parents and mimeType != 'application/vnd.google-apps.folder' and trashed = false and id = '" + fileId + "'";
            IList<File> files = FileListRequest.Execute().Files;
            return (GetFirstFileData(files));

        }
        public FileData GetFileByName(string folderId, string fileName)
        {
            FilesResource.ListRequest FileListRequest = driveService.Files.List();
            FileListRequest.Spaces = "appDataFolder";
            FileListRequest.Fields = "nextPageToken, files(*)";
            FileListRequest.Q = "'" + folderId + "' in parents and mimeType != 'application/vnd.google-apps.folder' and trashed = false and name = '" + fileName + "'";
            IList<File> files = FileListRequest.Execute().Files;
            return (GetFirstFileData(files));
        }

        public List<FileData> GetFilesByFolderId(string folderId)
        {
            FilesResource.ListRequest FileListRequest = driveService.Files.List();
            FileListRequest.Spaces = "appDataFolder";
            FileListRequest.Fields = "nextPageToken, files(*)";
            FileListRequest.Q = "'" + folderId + "' in parents and mimeType != 'application/vnd.google-apps.folder' and trashed = false";
            IList<File> files = FileListRequest.Execute().Files;

            return GetFileDatas(files);
        }

        public List<FileData> GetFolders(string parentFolderId)
        {
            FilesResource.ListRequest FileListRequest = driveService.Files.List();
            FileListRequest.Spaces = "appDataFolder";
            FileListRequest.Q = "'" + parentFolderId + "' in parents and mimeType != 'application/vnd.google-apps.folder' and trashed = false";

            IList<File> files = FileListRequest.Execute().Files;
            files = files.Where(x => x.MimeType == "application/vnd.google-apps.folder").ToList();
            return GetFileDatas(files);
        }

        private Task InitDriveServiceHelper(Account account)
        {
            var initializer = new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets()
                {
                    ClientId = clientId,
                }
            };
            initializer.Scopes = new[] { scope };
            initializer.DataStore = new FileDataStore("Google.Apis.Auth");
            var flow = new GoogleAuthorizationCodeFlow(initializer);
            var user = "UUP";
            var token = new TokenResponse()
            {
                AccessToken = account.Properties["access_token"],
                ExpiresInSeconds = Convert.ToInt64(account.Properties["expires_in"]),
                RefreshToken = account.Properties["refresh_token"],
                Scope = account.Properties["scope"],
                TokenType = account.Properties["token_type"]
            };

            UserCredential userCredential = new UserCredential(flow, user, token);
            driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = userCredential,
                ApplicationName = "UUP",
            });
            if (App.Passphrase != null)
                App.RestoreBackup();
            else
                App.UploadBackup();
            //Dictionary<string, string> dictionary = new Dictionary<string, string> {
            //            { "refresh_token", account.Properties["refresh_token"] },
            //            { "client_id", clientId },
            //            { "grant_type", "refresh_token" } };
            //var success = await AuthenticatorHelper.OAuth2Authenticator.RequestAccessTokenAsync(dictionary);
            //var request = new OAuth2Request("POST", new Uri("https://www.googleapis.com/oauth2/v4/token"), dictionary, account);
            //var response = await request.GetResponseAsync();
            return Task.CompletedTask;
        }

        private async void LoadUser()
        {

            var store = await SecureStorage.GetAsync("GoogleKey");
            if (store != null)
            {
                Account account = JsonConvert.DeserializeObject<Account>(store);
                {
                    XamarinShared.Setup.SetSecureValue("Backup", "true");
                    await InitDriveServiceHelper(account);
                    GetFiles();
                }
            }
            else
                Login();
        }
        private async void GetFiles()
        {
            //    // ToDo create parent folder based on user id and create a folder for each chat inside this folder. UUP=>UserId=>[ChatId1=>[Message1, Message2], ChatId2 => [Message1, Message2]]
            //    GetDriveFolders();

            //    var parentFolderId = driveServiceHelper.CreateFolder("UserId");

            //    var folderId = driveServiceHelper.CreateFolder("Musabir1", parentFolderId);
            //    var fileId = await driveServiceHelper.SaveFile(folderId, "test", "test save content");
            //    driveServiceHelper.GetDriveFile(folderId, "test");
            //    //var content = await driveServiceHelper.ReadFile(fileId);
            //    //  Console.WriteLine(content);
            //    //  driveServiceHelper.GetDriveFiles(folderId);
            //    driveServiceHelper.GetDriveFolders();
        }

        public void Init()
        {
            Auth = new OAuth2Authenticator(
             clientId,
             string.Empty,
             scope,
             new Uri("https://accounts.google.com/o/oauth2/v2/auth"),
             new Uri(redirectUrl),
             new Uri("https://www.googleapis.com/oauth2/v4/token"),
             isUsingNativeUI: true);

            //Login();
            LoadUser();

            Auth.Completed += async (sender, e) =>
            {
                if (e.IsAuthenticated)
                {
                    XamarinShared.Setup.SetSecureValue("Backup", "true");
                    await InitDriveServiceHelper(e.Account);
                    await SecureStorage.SetAsync("GoogleKey", JsonConvert.SerializeObject(e.Account));

                    GetFiles();

                }
                else
                {
                    XamarinShared.Setup.RemoveSecureValue("Backup");
                    App.DriveLoginFailed();
                }

            };
            Auth.Error += (sender, e) =>
            {
                XamarinShared.Setup.RemoveSecureValue("Backup");
                Console.WriteLine("here");
                App.DriveLoginFailed();
            };
        }

        public void Login()
        {
            var presenter = new OAuthLoginPresenter();
            presenter.Login(Auth);
        }

        private async Task<string> CreateFile(string folderId, string name)
        {
            File metadata = new File()
            {
                Parents = new List<string>() { folderId },
                MimeType = "text/plain",
                Name = name
            };
            var googleFile = await driveService.Files.Create(metadata).ExecuteAsync();
            if (googleFile == null)
            {
                throw new System.IO.IOException("Null result when requesting file creation.");
            }
            return googleFile.Id;
        }

        private FileData GetFirstFileData(IList<File> files)
        {
            File file = files.FirstOrDefault();
            if (file == null) return null;
            FileData fileData = new FileData()
            {
                Name = file.Name,
                Id = file.Id,
            };
            return fileData;
        }

        private List<FileData> GetFileDatas(IList<File> files)
        {
            List<FileData> fileDatas = new List<FileData>();
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    FileData f = new FileData
                    {
                        Name = file.Name,
                        Id = file.Id,
                    };
                    fileDatas.Add(f);
                }
            }
            return fileDatas;
        }

        private byte[] ReadFully(System.IO.Stream stream)
        {
            var bytes = new byte[stream.Length];
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            stream.ReadAsync(bytes, 0, bytes.Length);
            stream.Dispose();
            return bytes;
        }

        public bool SupportBackup()
        {
            return AsyncUtil.RunSync(() => SecureStorage.GetAsync("GoogleKey")) != null;
        }
    }
}