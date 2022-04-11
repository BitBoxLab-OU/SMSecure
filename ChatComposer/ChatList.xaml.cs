using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using CustomViewElements;
using EncryptedMessaging;
using Rg.Plugins.Popup.Services;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Forms;
using XamarinShared.ViewCreator;
using static EncryptedMessaging.Contacts;

namespace ChatComposer
{
    public partial class ChatList : BaseContentView
    {
        public delegate void ItemClickEvent(Contact contact, ChatItemClickType chatItemClick);
        public delegate void PlaceHolderVisibility(bool isVisible);
        public delegate void ToolbarTitleChangeEvent(bool isContactSelected);
        public delegate void CreateGroupEvent(List<Contact> contacts);
        public delegate void ShowPopUpEvent();
        private ItemClickEvent _onChatItemClicked;
        private PlaceHolderVisibility _placeHolderVisibility;
        private ToolbarTitleChangeEvent _toolbarTitleChangeEvent;
        private CreateGroupEvent _createGroupEvent;
        private Action _showPopUpEvent;
        private Action _addButtonClickEvent;
        public Observable<Contact> FilteredContacts { set; get; }
        public Observable<Contact> OriginContacts { set; get; }
        public Observable<Contact> SelectedContacts { set; get; }




        private SwipeView _lastSwipeView;
        private bool isPlaceholderVisible
        {
            set
            {
                if (_placeHolderVisibility != null)
                    _placeHolderVisibility(value);

            }
        }

        private string _searchQuery = "";
        public ChatList()
        {
            InitializeComponent();
            NextButton.Click(Next_Clicked);


        }



        public void Init(ItemClickEvent chatItemClicked, Observable<Contact> contacts, ToolbarTitleChangeEvent titleChangeEvent, CreateGroupEvent createGroupEvent, Action addButtonClickEvent, Action showPopUpEvent)
        {
            OriginContacts = contacts;
            SelectedContacts = new Observable<Contact>();
            FilteredContacts = new Observable<Contact>();
            SelectedContacts.CollectionChanged += SelectedContacts_CollectionChanged;
            _onChatItemClicked = chatItemClicked;
            _toolbarTitleChangeEvent = titleChangeEvent;
            _createGroupEvent = createGroupEvent;
            _showPopUpEvent = showPopUpEvent;
            _addButtonClickEvent = addButtonClickEvent;
            FilterContacts(_searchQuery);
            BindingContext = this;
            isPlaceholderVisible = OriginContacts.Count == 0;
            contacts.CollectionChanged += Contacts_CollectionChanged;
            NextButton.IsVisible = true;
            Next.Source = Utils.Icons.IconProvider.Invoke("ic_add.png");

        }

        private void SelectedContacts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

            //NextButton.IsVisible = SelectedContacts.Count != 0;
            if (SelectedContacts.Count == 0)
                Next.Source = Utils.Icons.IconProvider.Invoke("ic_add.png");
            else if (SelectedContacts.Count == 1)
                Next.Source = Utils.Icons.IconProvider.Invoke("ic_next_disabled.png");
            else if (SelectedContacts.Count == 2)
                Next.Source = Utils.Icons.IconProvider.Invoke("ic_next_new.png");
            _toolbarTitleChangeEvent?.Invoke(SelectedContacts.Count != 0);
        }

        private void Contacts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            isPlaceholderVisible = OriginContacts.Count == 0;
            FilterContacts(_searchQuery);
        }

        private void Clear_Clicked(object sender, EventArgs e)
        {

            if (sender == null) return;
            SwipeItemView item = sender as SwipeItemView;
            if (item == null || item.CommandParameter == null) return;
            var contact = item.CommandParameter as Contact;
            if (contact != null)
                _onChatItemClicked(contact, ChatItemClickType.CLEAR);
        }

        public void SetPlaceHolderVisibility(PlaceHolderVisibility placeHolderVisibility)
        {
            _placeHolderVisibility = placeHolderVisibility;
        }

        private void Delete_Clicked(object sender, EventArgs e)
        {
            if (sender == null) return;
            SwipeItemView item = sender as SwipeItemView;
            if (item == null || item.CommandParameter == null) return;
            var contact = item.CommandParameter as Contact;
            if (contact != null)
                _onChatItemClicked(contact, ChatItemClickType.DELETE);
        }

        private void Edit_Clicked(object sender, EventArgs e)
        {
            if (sender == null) return;
            SwipeItemView item = sender as SwipeItemView;
            if (item == null || item.CommandParameter == null) return;
            var contact = item.CommandParameter as Contact;
            if (contact != null)
                _onChatItemClicked(contact, ChatItemClickType.EDIT);
        }

        public void FilterContacts(string query)
        {
            _searchQuery = query;
            RemoveDeletedContacts();

            if (string.IsNullOrWhiteSpace(_searchQuery))
            {
                _searchQuery = string.Empty;
                if (ItemsListView.ItemsSource != OriginContacts)
                    ItemsListView.ItemsSource = OriginContacts;
                return;

            }
            else
            {
                ItemsListView.ItemsSource = FilteredContacts;
            }

            _searchQuery = _searchQuery.ToLowerInvariant();

            var filteredItems = OriginContacts.Where(value => value.Name.ToLowerInvariant().Contains(_searchQuery)).ToList();

            foreach (var value in OriginContacts)
            {
                if (!filteredItems.Contains(value))
                {
                    FilteredContacts.Remove(value);
                }
                else if (!FilteredContacts.Contains(value))
                {
                    if (value == null)
                    {
                    }
                    InsertCall(value);
                }
            }
        }
        //this method is used for insert to list;
        private void InsertCall(Contact value)
        {
            Contact contact = FilteredContacts.FirstOrDefault(c => c.LastMessageTime.CompareTo(value.LastMessageTime) < 0);
            if (contact != null)
                FilteredContacts.Insert(FilteredContacts.IndexOf(contact), value);
            else
                FilteredContacts.Add(value);

        }

        private void RemoveDeletedContacts()
        {
            foreach (Contact contact in new List<Contact>(OriginContacts))
                if (!OriginContacts.Contains(contact))
                    FilteredContacts.Remove(contact);
        }


        public void ClearState()
        {
            ItemsListView.SelectedItem = null;
        }

        public void ResetSwipe()
        {
        }

        public override void OnAppearing()
        {

        }

        public override void OnDisappearing()
        {
        }

        private void ItemsListView_ItemTapped(object sender, MR.Gestures.TapEventArgs e)
        {
            try
            {
                Contact contact = e.Sender.TappedCommandParameter as Contact;
                if (contact != null)
                    if (SelectedContacts.Count > 0)
                    {
                        var lyt = sender as MR.Gestures.StackLayout;
                        lyt.FindByName<Image>("Added").IsVisible = !lyt.FindByName<Image>("Added").IsVisible;
                        if (!SelectedContacts.Contains(contact)) SelectedContacts.Add(contact);
                        else SelectedContacts.Remove(contact);
                        return;
                    }
                _onChatItemClicked(contact, ChatItemClickType.TAP);
            }
            catch (FormatException) { }

        }


        void SwipeView_SwipeStarted(System.Object sender, Xamarin.Forms.SwipeStartedEventArgs e)
        {
            if (_lastSwipeView != sender as SwipeView)
                _lastSwipeView?.Close();
            _lastSwipeView = sender as SwipeView;
        }

        private void ListViewItem_LongPressed(object sender, MR.Gestures.LongPressEventArgs e)
        {
            Contact contact = e.Sender.LongPressingCommandParameter as Contact;
            var lyt = sender as MR.Gestures.StackLayout;
            lyt.FindByName<Image>("Added").IsVisible = !lyt.FindByName<Image>("Added").IsVisible;
            if (contact == null) return;
            if (!SelectedContacts.Contains(contact)) SelectedContacts.Add(contact);
            else SelectedContacts.Remove(contact);
            return;
        }

        async private void Next_Clicked(object sender, EventArgs args)
        {
            XamarinShared.ViewCreator.Utils.HandleButtonSingleClick(sender);
            if (SelectedContacts.Count() == 0)
            {
                // NextButton.IsVisible = false;
                _showPopUpEvent.Invoke();



            }
            else if (SelectedContacts.Count() > 1)
                _createGroupEvent?.Invoke(SelectedContacts.ToList());
            //else
            //    this.DisplayToastAsync(Localization.Resources.Dictionary.YouNeedToSelectAtLeastTwoUser);
        }

    }

}