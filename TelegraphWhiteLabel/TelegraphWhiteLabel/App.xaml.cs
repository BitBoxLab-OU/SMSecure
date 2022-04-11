
using AnonymousWhiteLabel.DesignHandler;
using AnonymousWhiteLabel.Pages;
using MessageCompose;
using MessageCompose.Model;
using System;
using Utils;
using Xamarin.Forms;
using XamarinShared.ViewCreator;
using static XamarinShared.ViewCreator.MessageViewCreator;
#if DEBUG_RAM
using Banking.Ehtereum.Services;
using Banking.Services;
#endif

namespace AnonymousWhiteLabel
{
    public partial class App : Application
    {
        public static EncryptedMessaging.Context Context;
        public App()
        {
            SetStyle();
            InitializeComponent();
            SetStyle();

            if (Xamarin.Essentials.Preferences.Get("firsrStartup", true) == true)
            {
                Xamarin.Essentials.Preferences.Set("firsrStartup", false);
                var restore = new RestoreAccount(Start);
                MainPage = restore;
            }
            else
            {
                Start();
            }

        }

        private void SetStyle()
        {
            Current.Resources = DesignResourceManager.ChangeTheme();
            Icons.IconProvider += ProvideImageSource;

            //must be replaced with new Sources .

            //CustomViewElements.Palette.CommonBackgroundColor = DesignResourceManager.GetColorFromStyle("Color1");
            //CustomViewElements.Palette.BackIcon = DesignResourceManager.GetImageSource("ic_new_back_icon.png");
            //CustomViewElements.Palette.SearchIcon = DesignResourceManager.GetImageSource("ic_toolbar_search.png");
            //CustomViewElements.Palette.CreateConversationIcon = DesignResourceManager.GetImageSource("ic_add_new_chat.png");
            //CustomViewElements.Palette.DefaultGroupIcon = DesignResourceManager.GetImageSource("ic_new_addGroup_contact.png");
            //CustomViewElements.Palette.NoItemIcon = DesignResourceManager.GetImageSource("ic_noResult.png");
            //CustomViewElements.Palette.NoResultIcon = DesignResourceManager.GetImageSource("ic_noItem.png");
            //CustomViewElements.Palette.SearchClearIcon = DesignResourceManager.GetImageSource("ic_toolbar_search_clear.png");

        }
        public ImageSource ProvideImageSource(string key)
        {
            return DesignHandler.DesignResourceManager.GetImageSource(key);
        }


        public void Start()
        {
            MessageViewCreator.Instance.Setup(fileDetailsProvider: ProvideFileDetails,composerStateProvider:ProvideComposerState);

            Context = XamarinShared.Setup.Initialize(OnMessageArrived, OnNewMessageAddedToView, Config.Connection.EntryPoint, Config.Connection.NetworkName, Config.ChatUI.MultipleChatModes, Config.ChatUI.NewMessageOnTop, ChatPage.MessageContainer, null);
            var main = new MainTabsPage(Context);
            MainPage = main;
#if DEBUG_RAM
           
            Banking.CryptoWallet.Initialize(main, Context);

            //main.Children.Insert(1, new NavigationPage(new Banking.Bitcoin.Views.ShowBalancePage()) { Title = "BTC Wallet" });
            //main.Children.Insert(2, new NavigationPage(new Banking.Ehtereum.Views.BalancePage()) { Title = "ETH Wallet" });
#endif
#if DEBUG_AND
			// add Community functions to main
			CommunityClient.Social.Initialize(main, Context);
#if false
			// ====================== START TEST BANKING CLOUD FUNCTIONS =================================
			// add Banking functions to main
			Banking.BankCloud.Initialize(Context);
			void RunOnConfirmation(List<byte[]> values) // We create a function that is executed when the cloud responds to our command (this code is not mandatory, it is used to manage the vloud responses)
			{
				if (values == null)
				{
					Debug.WriteLine("Timeout is occurred: The device o the cloud it is not online");
					return;
				}
				var status = (Banking.Communication.Feedback)values[0][0]; // The cloud as the first parameter gives us a byte that indicates the outcome of the operation
				Debug.WriteLine("Operation " + status.ToString());
				if (status == Banking.Communication.Feedback.Successful)
				{
					// If you want to have something run when the cloud has finished processing the request, write the code here
				}
				else
				{
					// There was an error, write the code to warn the user about the type of error that occurred.
				}
			};
			//			Communication.CreateErc20Token(RunOnConfirmation, "name", "symbol", 100000, 100000);
			// ====================== END TEST BANKING CLOUD FUNCTIONS  =================================			
#endif
#endif
        }

        private bool ProvideComposerState(MessageViewCreator.RequiredComposerState requiredState, int newValue)
        {

            if (requiredState == RequiredComposerState.AudioSend)
            {
                if (newValue == 0)
                {
                    Composer.IsAudioSendCancelled = false;
                }
                else if (newValue == 1)
                {
                    Composer.IsAudioSendCancelled = true;
                }
                return Composer.IsAudioSendCancelled;
            }
            else
            {
                if (newValue == 0)
                {
                    Composer.IsAudioRecordCancelled = false;
                }
                else if (newValue == 1)
                {
                    Composer.IsAudioRecordCancelled = true;
                }
                return Composer.IsAudioRecordCancelled;

            }
        }

        private Tuple<byte[], string> ProvideFileDetails(byte[] data)  {
        	SerializableFileData details = Utils.Utils.ByteArrayToObject(data) as SerializableFileData;
			return Tuple.Create(details.Data, details.FileName);
        }

        protected override void OnStart()
        {
            // The code here on Android is started even when the screen is rotated, or with the application already started

        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        //The code in here is executed when a message is received
        private void OnMessageArrived(EncryptedMessaging.Message message) => ((MainTabsPage)MainPage).NotificationManager.ScheduleNotification(Localization.Resources.Dictionary.StrictlyConfidentialMessage, Localization.Resources.Dictionary.NewMessage); //To prevent the operating system from capturing sensitive information, there is no other information in the notification
        public static System.Action IgnoreBatteryOptimizations { get; set; }

        private static void OnNewMessageAddedToView()
        {
            // Scroll to End;
        }
    }
}
