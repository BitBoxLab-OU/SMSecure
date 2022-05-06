using CustomViewElements;
using Rg.Plugins.Popup.Services;
using System.Collections.Generic;
using EncryptedMessaging;
using XamarinShared;
using XamarinShared.ViewCreator;
using Xamarin.Forms;
using Xamarin.CommunityToolkit.Extensions;

namespace Telegraph.PopupViews
{
    public partial class DeleteMessagePopupPage : BasePopupPage
    {
        private readonly List<Message> _message;

        public DeleteMessagePopupPage(List<Message> message, bool isMyMessage)
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            _message = message;
            if (!isMyMessage)
            {
                DeleteForEveryone.IsVisible = false;
            }
        }

        private async void DeleteForMe_Clicked(object sender, System.EventArgs e)
        {
            CheckMessageDeleted();

            if (_message != null && MessageViewCreator.SelectedMessageHashCode != _message.GetHashCode())
                ChatPageSupport.RemoveMessages(_message, false);
            else
                await this.DisplayToastAsync(Localization.Resources.Dictionary.SelectedMessageCannotBeDeleted);
            await PopupNavigation.Instance.PopAsync(true);

        }

        private async void DeleteForEveryone_Clicked(object sender, System.EventArgs e)
        {
            CheckMessageDeleted();

            if (_message != null && MessageViewCreator.SelectedMessageHashCode != _message.GetHashCode())
                ChatPageSupport.RemoveMessages(_message, true);
            else
                await this.DisplayToastAsync(Localization.Resources.Dictionary.SelectedMessageCannotBeDeleted);
            await PopupNavigation.Instance.PopAsync(true);
        }

        private void CheckMessageDeleted()
        {
            if (_message == null)
            {
                this.DisplayToastAsync(Localization.Resources.Dictionary.MessageAlreadDeleted);
                PopupNavigation.Instance.PopAsync(true);
                return;
            }
        }

        private void Cancel_Clicked(object sender, System.EventArgs e) => PopupNavigation.Instance.PopAsync(true);
    }
}