using ChatComposer;
using CustomViewElements;
using System;
using EncryptedMessaging;
using XamarinShared;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Collections.Generic;
using ChatComposer.PopupViews;
using Rg.Plugins.Popup.Services;
using Xamarin.CommunityToolkit.Extensions;

namespace Telegraph.Views

{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : BasePage
    {
        private string query;

        public MainPage()
        {
            InitializeComponent();
            ChatList.SetPlaceHolderVisibility(PlaceHolderVisibility);
            Toolbar.AddSearchButton(null);
            Toolbar.AddLeftButton(Utils.Icons.IconProvider?.Invoke("ic_hamburger_menu.png"), OnMenuItemClicked);
            Toolbar.SearchEntry.TextChanged += Search_TextChanged;

            ChatList.Init(ChatItemClicked, NavigationTappedPage.Context.Contacts.GetContacts(), HandleToolbarTitleChangeEvent, HandleCreateGroupEvent, AddButtonClicked, AddContactPopupClicked);
        }

        private void OnMenuItemClicked(object sender, EventArgs e) => ((App)Application.Current).GetRootPage().IsPresented = true;

        protected override void OnAppearing()
        {
        }

        protected override void OnDisappearing()
        {
            Toolbar.ClearViewState();

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

        private void PlaceHolderVisibility(bool isVisible)
        {
            NoItemPage.IsVisible = isVisible && string.IsNullOrWhiteSpace(query);
            NoResultPage.IsVisible = isVisible && !string.IsNullOrWhiteSpace(query);
            ChatList.IsVisible = !isVisible;
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
            Application.Current.MainPage.Navigation.PushAsync(new GroupCreatePage(contacts), false);
        }

        private async void OnChatItemTapped(Contact contact)
        {
            await Application.Current.MainPage.Navigation.PushAsync(new ChatRoom(contact), false).ConfigureAwait(true);
        }

        private async void OnChatItemEditClicked(Contact contact)
        {
            await Application.Current.MainPage.Navigation.PushAsync(new ChatUserProfilePage(contact), false).ConfigureAwait(true);
            //ChatList.ResetSwipe();
        }

        private async void OnChatItemClearClicked(Contact contact)
        {
            var clearAnswer = await Application.Current.MainPage.DisplayAlert(Localization.Resources.Dictionary.Alert, Localization.Resources.Dictionary.ClearAlertQuestion, Localization.Resources.Dictionary.Yes, Localization.Resources.Dictionary.No);
            if (clearAnswer)
            {
                Calls.GetInstance().ClearCleanedContactCalls(contact.PublicKeys);
                NavigationTappedPage.Context.Contacts.ClearContact(contact.PublicKeys);
                ChatList.ClearState();
                // ChatList.ResetSwipe();
            }
        }

        private async void OnChatItemDeleteClicked(Contact contact)
        {
            var deleteAnswer = await Application.Current.MainPage.DisplayAlert(Localization.Resources.Dictionary.Alert, Localization.Resources.Dictionary.DeleteAlertQuestion, Localization.Resources.Dictionary.Yes, Localization.Resources.Dictionary.No);
            if (deleteAnswer)
            {
                Calls.GetInstance().ClearCleanedContactCalls(contact.PublicKeys);
                NavigationTappedPage.Context.Contacts.RemoveContact(contact);
                ChatList.ClearState();
                // ChatList.ResetSwipe();
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
    }
}