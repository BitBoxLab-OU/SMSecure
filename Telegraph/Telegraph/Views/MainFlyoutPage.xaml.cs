using System;
using System.Collections.Generic;
using Telegraph.ViewModels;
using Xamarin.Forms;

namespace Telegraph.Views
{
    public partial class MainFlyoutPage : FlyoutPage
    {
        public static FlyoutPage Instance;
        public MainFlyoutPage()
        {
            InitializeComponent();
            flyoutPage.listView.ItemSelected += OnItemSelected;

            if (Device.RuntimePlatform == Device.UWP)
            {
                FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;
            }
            Instance = this;
        }

        void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as FlyoutPageItem;
            if (item != null)
            {
                Application.Current.MainPage.Navigation.PushAsync((Page)Activator.CreateInstance(item.TargetType), false);
                flyoutPage.listView.SelectedItem = null;
                IsPresented = false;
            }
        }
    }
}