
using System;
using System.IO;
using Telegraph.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Rg.Plugins.Popup.Services;
using System.Threading.Tasks;
using CustomViewElements;
using Telegraph.DesignHandler;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Essentials;

namespace Telegraph.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProfilePage : BasePage
    {
        private string _usrnm;
        private FileResult _profilePicFileResult;
        private byte[] image;
        private string _publickey;
        public ProfilePage()
        {
            InitializeComponent();
            _usrnm = NavigationTappedPage.Context.My.Name;
            Name.Text = _usrnm;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _publickey = NavigationTappedPage.Context.My.GetPublicKey();
            PublicKey.Text = _publickey;

            image = NavigationTappedPage.Context.My.GetAvatar();
            _ = ImageDisplayAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Back();
        }

        public async Task ImageDisplayAsync()
        {
            try
            {
                if (_profilePicFileResult != null) return;
                if (image != null)
                {
                    Task<ImageSource> result = Task<ImageSource>.Factory.StartNew(() => ImageSource.FromStream(() => new MemoryStream(image)));
                    Profile_Photo.Source = await result;
                }
                else
                {
                    Profile_Photo.Source = DesignResourceManager.GetImageSource("ic_add_contact_profile.png");
                }
            }
            catch (Exception)
            {
            }
        }

        public async void Photo_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (((FileImageSource)Edit.Source).File == "ic_save_profile")
                    await PopupNavigation.Instance.PushAsync(new ImageViewPopupPage(Profile_Photo.Source), false).ConfigureAwait(true);
                else
                {
                    await PopupNavigation.Instance.PushAsync(new ImageViewPopupPage(image), false).ConfigureAwait(true);
                }
            }
            catch (Exception)
            {
            }
        }

        private async void ExportPrivateKey_ClickedAsync(object sender, EventArgs e)
        {
            var status = await PermissionManager.CheckStoragePermission().ConfigureAwait(true);
            if (!status)
            {
                await this.DisplayToastAsync(Localization.Resources.Dictionary.TheFileCannotBeExportedWithoutGrantingPermission);

                return;
            }
            var fn = "private_key.txt";
            var privateKey = NavigationTappedPage.Context.My.GetPrivateKey();

            if (Device.RuntimePlatform == Device.iOS)
            {
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                documentsPath = Path.Combine(documentsPath, Localization.Resources.Dictionary.Downloads);
                Directory.CreateDirectory(documentsPath);
                var filePath = Path.Combine(documentsPath, fn);
                File.WriteAllText(filePath, privateKey);
                await this.DisplayToastAsync(Localization.Resources.Dictionary.SuccessfullyExportedToDownloadsFile);
            }

            else if (Device.RuntimePlatform == Device.Android)
            {
                var directory = DependencyService.Get<IPathService>().PublicExternalFolder;
                var file = Path.Combine(directory, fn);
                File.WriteAllText(file, privateKey);
                await this.DisplayToastAsync(Localization.Resources.Dictionary.SuccessfullyExportedToDownloadsFile);
            }
        }

        private void Edit_Clicked(object sender, EventArgs e) => ChangeViewState(true);

        private async void Image_Clicked(object sender, EventArgs e)
        {
            if (!MediaPicker.IsCaptureSupported)
            {
                await Application.Current.MainPage.DisplayAlert(Localization.Resources.Dictionary.NoUpload, Localization.Resources.Dictionary.PickingaphotoIsNotSupported, Localization.Resources.Dictionary.Ok).ConfigureAwait(true);
                return;
            }
            var status = await PermissionManager.CheckStoragePermission().ConfigureAwait(true);
            if (!status)
            {
                await this.DisplayToastAsync(Localization.Resources.Dictionary.StoragePermissionIsNeeded);
                return;
            }
            _profilePicFileResult = await MediaPicker.PickPhotoAsync();
            if (_profilePicFileResult == null)
                return;
            var stream = await _profilePicFileResult.OpenReadAsync();
            Profile_Photo.Source = ImageSource.FromStream(() => stream);
            ChangeViewState(true);


        }

        private async void Save_Clicked(object sender, EventArgs e)
        {
            Name.Unfocus();
            if (!string.IsNullOrWhiteSpace(Name.Text))
            {
                NavigationTappedPage.Context.My.Name = Name.Text.Trim();
                CancelSaveLayout.IsVisible = false;
                Edit.IsVisible = true;
                Name.IsReadOnly = true;

            }
            else
            {
                await this.DisplayToastAsync(Localization.Resources.Dictionary.PleaseAddValidName);
                return;
            }
            if (_profilePicFileResult != null)
            {
                var stream = await _profilePicFileResult.OpenReadAsync();
                image = Utils.Utils.StreamToByteArray(stream);

                NavigationTappedPage.Context.My.SetAvatar(image);
                _profilePicFileResult = null;

            }
            XamarinShared.Setup.Settings.Save();
            ChangeViewState(false);
        }

        private async void DeletePicture_Clicked(object sender, EventArgs e)
        {
            var DeletePictures = await Application.Current.MainPage.DisplayAlert(Localization.Resources.Dictionary.Alert, Localization.Resources.Dictionary.DeletePictureQuestion, Localization.Resources.Dictionary.Yes, Localization.Resources.Dictionary.No);
            if (DeletePictures)
            {
                if (image != null)
                {
                    image = null;
                    NavigationTappedPage.Context.My.SetAvatar(image);
                }
            }
        }

        private void CustomEntry_Focused(object sender, FocusEventArgs e)
        {
            (sender as CustomEntry).PlaceholderColor = DesignResourceManager.GetColorFromStyle("BackgroundSecondary");
        }

        private void CustomEntry_Unfocused(object sender, FocusEventArgs e)
        {
            (sender as CustomEntry).PlaceholderColor = DesignResourceManager.GetColorFromStyle("WhiteColor");
        }

        private void Back()
        {
            Edit.IsVisible = true;
            Name.Unfocus();
            ChangeViewState(false);

            _profilePicFileResult = null;
            _ = ImageDisplayAsync();
            CancelSaveLayout.IsVisible = false;
            Name.IsReadOnly = true;
            Name.Text = _usrnm;

        }

        private void ChangeViewState(bool isEditEnabled)
        {

            //Edit.IsVisible = !isEditEnabled;
            CancelSaveLayout.IsVisible = isEditEnabled;
        }


        private void Cancel_Clicked(object sender, EventArgs args) => Back();
        private void Back_Clicked(object sender, EventArgs e) => OnBackButtonPressed();

        void EditUsername(System.Object sender, System.EventArgs e)
        {

            Name.IsReadOnly = !Name.IsReadOnly;
            Edit.IsVisible = false;
            Name.Focus();
            CancelSaveLayout.IsVisible = true;
        }

    }
}