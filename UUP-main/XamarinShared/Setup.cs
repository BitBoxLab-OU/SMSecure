using System;
using System.Diagnostics;
using EncryptedMessaging;
using Xamarin.Forms;
using XamarinShared.ViewCreator;

namespace XamarinShared
{
    public static class Setup
    {
        internal static Context Context;
        // Remove onViewMessage parameter
        public static Context Initialize(Context.OnMessageArrived onNotification, ChatPageSupport.OnNewMessageAddedToView onNewMessageAddedToView, string entryPoint, string networkName, bool multipleChatModes, bool newMessageOnTop, ScrollView messageScrollView, string privatKeyOrPassphrase = null, PaletteSetting paletteSetting = null)
        {
            // privatKeyOrPassphrase = "title because exile rhythm expire note rare smile alpha merit loan curve";
            if (paletteSetting == null)
                Palette.Colors = new PaletteSetting() { ForegroundColor = Color.FromHex("#E2E8F3"), BackgroundColor = Color.FromHex("#B7CBF2") };
            else Palette.Colors = paletteSetting;
            if (Context != null)
            {
                Debugger.Break(); // You don't have to go in here! There is something conceptually wrong with creating the interface! Correct the UI logic! It must not attempt to instantiate the context twice
            }
            else
            {
                MessageReadStatus messageReadStatus = new MessageReadStatus();
                //messageReadStatus for status label
                MessageViewCreator.Instance.SetReadStatus(messageReadStatus);

                ChatPageSupport.Initialize(multipleChatModes, newMessageOnTop, messageScrollView, onNewMessageAddedToView);

                Contact.RuntimePlatform os;
                if (Device.RuntimePlatform == Device.Android)
                    os = Contact.RuntimePlatform.Android;
                else if (Device.RuntimePlatform == Device.iOS)
                    os = Contact.RuntimePlatform.iOS;
                else if (Device.RuntimePlatform == Device.UWP)
                    os = Contact.RuntimePlatform.UWP;
                else
                    os = Contact.RuntimePlatform.Undefined;


//                ChatPageSupport.ViewMessage, //, Messaging.ViewMessageUi viewMessage
// ChatPageSupport.OnContactEvent,  //Action<Message> onContactEvent
// onNotification, //Messaging.OnMessageArrived onNotification

                Context = new Context(entryPoint, networkName, multipleChatModes, privatKeyOrPassphrase, false, null, Device.BeginInvokeOnMainThread, GetSecureValue, SetSecureValue, runtimePlatform: os);
                //privatKeyOrPassphrase = "base cup tape theory segment document spare dove slush absurd enough February";

                Context.ViewMessage += ChatPageSupport.ViewMessage;
                Context.OnContactEvent +=ChatPageSupport.OnContactEvent;
                Context.OnLastReadedTimeChange += messageReadStatus.OnLastReadedTimeChange;
                Context.OnMessageDelivered += messageReadStatus.OnMessageDelivered;
                Context.OnNotification += onNotification;
                // Bind the event to change the connection when the connectivity changes
                Xamarin.Essentials.Connectivity.ConnectivityChanged += (o, c) => Context.OnConnectivityChange(c.NetworkAccess == Xamarin.Essentials.NetworkAccess.Internet || Xamarin.Essentials.Connectivity.NetworkAccess == Xamarin.Essentials.NetworkAccess.ConstrainedInternet);
                // Set the current connection status
                Context.OnConnectivityChange(Xamarin.Essentials.Connectivity.NetworkAccess == Xamarin.Essentials.NetworkAccess.Internet || Xamarin.Essentials.Connectivity.NetworkAccess == Xamarin.Essentials.NetworkAccess.ConstrainedInternet);
            }
            Settings.Load();
            Calls.GetInstance().SetMyId(Context.My.GetId());

            //Context.Contacts.ForEachContact(x => x.Save(true));
            //EncryptedMessaging.Cloud.SendCloudCommands.GetAllObject(Context, "Contact");
            //EncryptedMessaging.Cloud.SendCloudCommands.PostPushNotification(Context, "740f4707 bebcf74f 9b7c25d4 8e335894 5f6aa01d a5ddb387 462c7eaf 61bb78ad", 12345678910, true, "Andrea");

            //Context.My.SetAvatar(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 });

            //Context.Contacts.RestoreContactFromCloud();


            return Context;
        }

        public static void SetSecureValue(string key, string value) => _ = Xamarin.Essentials.SecureStorage.SetAsync(key, value);

        public static void RemoveSecureValue(string key) => _ = Xamarin.Essentials.SecureStorage.Remove(key);

        public static string GetSecureValue(string key)
        {
            System.Threading.Tasks.Task<string> task = System.Threading.Tasks.Task.Run(async () => await Xamarin.Essentials.SecureStorage.GetAsync(key));
            return task.Result;
        }

        public static Settings Setting;
        public class Settings
        {
            public string Pseudonym = null;
            public bool NameVis = true;
            public bool PicVis = true;
            public bool ThemeVis = false;
            public bool MessagePreloading = true;
            public bool SendContact = true;
            public bool AllowOtherApp = true;
            public bool KeyBoardVis = false;
            public bool NotificationsVis = true;
            public bool NotificationsToneVis = true;
            public bool VibrateVis = true;
            public bool PaymentTick = false;
            public bool FirebaseNotificationVis = true;
            public bool CheckBoxTick_Visa = true;
            public bool CheckBoxTick_Master = true;
            public float FontRatio = 1f;

            public static void Load()
            {
                Setting = (Settings)Context.SecureStorage.ObjectStorage.LoadObject(typeof(Settings), "Setting");
                if (Setting == null)
                    Setting = new Settings();

            }
            public static void Save() => Context.SecureStorage.ObjectStorage.SaveObject(Setting, "Setting");
        }
    }
}