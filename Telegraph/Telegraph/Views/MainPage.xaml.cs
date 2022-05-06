using ChatComposer;
using CustomViewElements;
using System;
using EncryptedMessaging;
using XamarinShared;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.CommunityToolkit.Extensions;
using System.Collections.Generic;
using ChatComposer.PopupViews;
using Rg.Plugins.Popup.Services;

namespace Telegraph.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : BasePage
    {
        private string query;
        public MainPage()
        {
            try
            {
                InitializeComponent();
            }
            catch(Exception e) { }
            Toolbar.AddSearchButton(null);
            Toolbar.AddLeftButton(Utils.Icons.IconProvider?.Invoke("ic_hamburger_menu.png"), OnMenuItemClicked, iconHeight: 64, iconWidth: 64);
            Toolbar.SearchEntry.TextChanged += Search_TextChanged;
            ChatList.Init(ChatItemClicked, NavigationTappedPage.Context.Contacts.GetContacts(), HandleToolbarTitleChangeEvent, HandleCreateGroupEvent, AddButtonClicked, AddContactPopupClicked);

        }

     

        protected override void OnDisappearing()
        {
            Toolbar.ClearViewState();

        }

        protected override void OnAppearing()
        {
        }

        private void ChatItemClicked(Contact contact, ChatItemClickType chatItemClick)
        {
            switch (chatItemClick)
            {
                case ChatItemClickType.CLEAR:
                    OnChatItemClearClicked(contact);
                    break;
                case ChatItemClickType.EDIT:
                    OnChatItemEditClicked(contact);
                    break;
                case ChatItemClickType.DELETE:
                    OnChatItemDeleteClicked(contact);
                    break;
                default:
                    OnChatItemTapped(contact);
                    break;
            }
        }

        private void HandleToolbarTitleChangeEvent(bool isContactSelected)
        {
            Toolbar.Title = !isContactSelected ? Localization.Resources.Dictionary.Chat : Localization.Resources.Dictionary.NewGroup;
        }

        private void HandleCreateGroupEvent(List<Contact> contacts)
        {
            foreach (Contact contact in contacts)
            {
                if (contact == null || contact.IsGroup)
                {
                    this.DisplayToastAsync("You cannot select a group.");
                    return;
                }
            }
            Application.Current.MainPage.Navigation.PushAsync(new GroupCreatePage(contacts, ClearUserSelection), false);
        }

        private void ClearUserSelection()
        {
            ChatList.ClearUserSelection();
        }
        private async void OnChatItemTapped(Contact contact)
        {
            if (App.IsVideoUploading && App.ContactVideoUploading == contact)
                await Application.Current.MainPage.Navigation.PushAsync(App.ChatRoomVideoUploading, false).ConfigureAwait(true);
            else
                await Application.Current.MainPage.Navigation.PushAsync(new ChatRoom(contact), false).ConfigureAwait(true);
        }

        private async void OnChatItemEditClicked(Contact contact)
        {
            await Application.Current.MainPage.Navigation.PushAsync(new ChatUserProfilePage(contact), false).ConfigureAwait(true);
            ChatList.ResetSwipe();
        }

        private async void OnChatItemClearClicked(Contact contact)
        {
            var clearAnswer = await Application.Current.MainPage.DisplayAlert(Localization.Resources.Dictionary.Alert, Localization.Resources.Dictionary.ClearAlertQuestion, Localization.Resources.Dictionary.Yes, Localization.Resources.Dictionary.No);
            if (clearAnswer)
            {
                Calls.GetInstance().ClearCleanedContactCalls(contact.PublicKeys);
                NavigationTappedPage.Context.Contacts.ClearContact(contact.PublicKeys);
                ChatList.ClearState();
                ChatList.ResetSwipe();
            }
        }

        private async void OnChatItemDeleteClicked(Contact contact)
        {
            var deleteAnswer = await Application.Current.MainPage.DisplayAlert(Localization.Resources.Dictionary.Alert, Localization.Resources.Dictionary.DeleteAlertQuestion, Localization.Resources.Dictionary.Yes, Localization.Resources.Dictionary.No);
            if (deleteAnswer)
            {
                contact.IsBlocked = true;
                Calls.GetInstance().ClearCleanedContactCalls(contact.PublicKeys);
                NavigationTappedPage.Context.Contacts.RemoveContact(contact);
                ChatList.ClearState();
                ChatList.ResetSwipe();
            }
        }

        private void Search_TextChanged(object sender, EventArgs e)
        {
            query = ((CustomEntry)sender).Text;
            ChatList.FilterContacts(query);
        }

        private async void AddButtonClicked()
        {
            if (IsBusy)
                return;
            await Application.Current.MainPage.Navigation.PushAsync(new EditItemPage());

        }

        private void AddContactPopupClicked()
        {
            var addContactPopup = new AddContactPopupPage();
            PopupNavigation.Instance.PushAsync(addContactPopup, true);

        }


        private void OnMenuItemClicked(object sender, EventArgs e) => ((App)Application.Current).GetRootPage().IsPresented = true;


        //private void AddNewChat_Click(object sender, EventArgs e)
        //{
        //    PopupView.IsVisible = !PopupView.IsVisible;
        //}

        //private void AddNewGroup_Click(object sender, EventArgs e)
        //{
        //    Application.Current.MainPage.Navigation.PushAsync(new GroupUserSelectPage(), false).ConfigureAwait(true);
        //}

        //private void AddNewContact_Click(object sender, EventArgs e)
        //{
        //    Application.Current.MainPage.Navigation.PushAsync(new EditItemPage(), false).ConfigureAwait(true);
        //}










    }
}