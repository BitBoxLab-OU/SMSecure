

using CustomViewElements;

using Syncfusion.XForms.Buttons;
using System;
using Telegraph.DesignHandler;
using Telegraph.PopupViews;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace Telegraph.Views
{
    public partial class OtherSettingPage : BasePage
    {
        public OtherSettingPage()
        {
            InitializeComponent();
        }
        protected override void OnAppearing()

        {
            CheckSwitchCase();
            base.OnAppearing();
            
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }
        private void SfSwitch_SendContactStateChanged(object sender, ToggledEventArgs e)
        {
            App.Setting.SendContact = e.Value;
            XamarinShared.Setup.Settings.Save();
        }

        private void MessageLimits_Clicked(object sender, ValueChangedEventArgs e)
        {
            double data = Math.Round(e.NewValue);
            messageLimitSlider.Value = data;
            NavigationTappedPage.Context.Setting.KeepPost = Convert.ToInt32(data * 10);
        }

        private void MessageDuration_Clicked(object sender, Xamarin.Forms.ValueChangedEventArgs e)
        {
            double data = Math.Round(e.NewValue);
            slider.Value = data;
            NavigationTappedPage.Context.Setting.PostPersistenceDays = Convert.ToInt32(data * 10);
        }
        private void Back_Clicked(object sender, EventArgs e) => OnBackButtonPressed();
        private void CheckSwitchCase()
        {
            AppLock1.IsToggled = XamarinShared.Setup.GetSecureValue("LockPin") != null;
            //SendContact1.IsToggled = App.Setting.SendContact != false;
        }

        private async void SfSwitch_AppLockStateChanged(object sender, ToggledEventArgs e)
        {
            if (AppLock1.IsToggled == true && XamarinShared.Setup.GetSecureValue("LockPin") == null)
            {
                await Application.Current.MainPage.Navigation.PushAsync(new CreatePinPage(), false);
            }
            else if (AppLock1.IsToggled == false && XamarinShared.Setup.GetSecureValue("LockPin") != null)
            {
                XamarinShared.Setup.RemoveSecureValue("LockPin");
                XamarinShared.Setup.RemoveSecureValue("LastAttemptTime");
                XamarinShared.Setup.RemoveSecureValue("NumberOfAttempts");
            }
        }


    }
}
