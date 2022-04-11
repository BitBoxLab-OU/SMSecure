using System.IO;
using System;
using Xamarin.Forms;
using System.Linq;
using EncryptedMessaging;
using XamarinShared;
using XamarinShared.ViewCreator;
using MessageCompose.Services;

namespace AnonymousWhiteLabel.Pages
{
    internal class ChatPage : ContentPage
    {
        private IAudioPlayer Player;

        public ChatPage()
        {
            Disappearing += (m, n) =>
            {
                if (!Navigation.NavigationStack.Take(Navigation.NavigationStack.Count - 1).Contains(this)) // Change the current contact only if you go back on the navigation stack
                    App.Context.Messaging.CurrentChatRoom = null;
            };
            Appearing += (m, n) =>
            {
                if (App.Context.Messaging.CurrentChatRoom == null)
                    Navigation.PopAsync(); // Automatic go back when delete the contact in ContactPage
            };
         
            Content = new Views.EditMessage(MessageContainer);
            Player = DependencyService.Get<IAudioPlayer>();
            InitalizeComposer();
            
        }

        private void InitalizeComposer()
        {
            var composer = (Content as Views.EditMessage).Composer;
            composer.Init(onSend: onSendClick, onMediaFileSelected: OnMediaFileSelected);
            composer.AudioPlayerEvent += AudioPlayer_Clicked;
            composer.OnSendClick += RemoveUnreadedMessagesView;

        }

        private void AudioPlayer_Clicked() {
            if (Player == null) return;
            if (Player.IsPlaying)
            {
                StopAudio();
                return;
            }
            PlayAudio(DependencyService.Get<IAudioRecorder>().GetOutput());
            Player.PlaybackEnded += (s, args) =>
            {
                StopAudio();
            };
        }

        private void PlayAudio(byte[] data)
        {
            var ms = new MemoryStream(data);
            Player?.Load(ms);
            Player?.Play();
            var composer = (Content as Views.EditMessage).Composer;
            composer?.PlayAudio();

        }

        private void StopAudio()
        {
            var composer = (Content as Views.EditMessage).Composer;
            composer?.StopAudio();
            Player?.Stop();
        }


        private void RemoveUnreadedMessagesView()
        {
            if (!isUnreadedMessagesViewRemoved)
                ChatPageSupport.RemoveUnreadedMessageView();
            isUnreadedMessagesViewRemoved = true;
        }

        private void onSendClick(ulong? replyToPostId,string text = null, byte[] image = null, byte[] audio = null, Tuple<double, double> coordinate = null, byte[] pdf = null, byte[] phoneContact = null)
        {
            var currentContact = App.Context.Messaging.CurrentChatRoom;
            if (!string.IsNullOrEmpty(text))
                App.Context.Messaging.SendText(text, currentContact);
            if (image != null)
                App.Context.Messaging.SendPicture(image, currentContact);
            if (audio != null)
                App.Context.Messaging.SendAudio(audio, currentContact);
            if (coordinate != null)
                App.Context.Messaging.SendLocation(coordinate.Item1, coordinate.Item2, currentContact);
            if (pdf != null)
                App.Context.Messaging.SendPdfDocument(pdf, currentContact);
            if (phoneContact != null)
                App.Context.Messaging.SendPhoneContact(phoneContact, currentContact);
        }

        private void OnMediaFileSelected(byte[] image, ulong? replyToPostId)
        {
            onSendClick(replyToPostId,image: image);
            //var editImagePage = new EditImagePage(image);
            //editImagePage.AttachPicture += (data) => OnSend(image: data);
            //Application.Current.MainPage.Navigation.PushAsync(editImagePage, false);
        }

        //void onSendClick(MessageFormat.MessageType type, object message)
        //{
        //    if (type == MessageFormat.MessageType.Text)
        //        App.Context.Messaging.SendText(message as string, App.Context.Messaging.CurrentChatRoom);
        //    else if (type == MessageFormat.MessageType.Image)
        //        App.Context.Messaging.SendPicture(message as byte[], App.Context.Messaging.CurrentChatRoom);
        //}

        public static readonly ScrollView MessageContainer = new ScrollView();
        private bool isUnreadedMessagesViewRemoved;

        public void SetCurrentContact(Contact contact)
        {
            Title = contact?.Name;
            //App.Context.Messaging.CurrentChatRoom = contact;
            XamarinShared.ChatPageSupport.SetCurrentContact(contact);
        }
//        public static void OnViewMessage(Message message, bool isMyMessage, out View content)
//        {
//            Contact contact = message.Contact;
//            var paddingLeft = 5; var paddingRight = 5;
//            Color background;
//            if (isMyMessage)
//            {
//                paddingLeft = 20;
//                background = Template.BackgroundMyMessage;
//            }
//            else
//            {
//                background = Template.BackgroundMessage;
//                paddingRight = 20;
//            }
//            var frame = new Frame { CornerRadius = 10, BackgroundColor = background, Padding = new Thickness(paddingLeft, 5, paddingRight, 5), Margin = 5 };
//            frame.HasShadow = true;
//            var box = new StackLayout();
//            frame.Content = box;
//            TextAlignment align = isMyMessage ? TextAlignment.End : TextAlignment.Start;
//            if (!isMyMessage && contact?.Participants.Count > 2)
//            {
//                var authorLabel = new Label() { Text = message.AuthorName() + ":" };
//                authorLabel.HorizontalTextAlignment = align;
//                box.Children.Add(authorLabel);
//            }
//            switch (message.Type)
//            {
//                case MessageFormat.MessageType.Text:
//                    var textMessage = new Label { Text = System.Text.Encoding.Unicode.GetString(message.GetData()) };
//                    textMessage.HorizontalTextAlignment = align;
//                    box.Children.Add(textMessage);
//                    break;
//                case MessageFormat.MessageType.Image:
//                    var image = new Image { Source = ImageSource.FromStream(() => new MemoryStream(message.GetData())), Aspect = Aspect.AspectFill };
//                    box.Children.Add(image);
//                    break;
//                case MessageFormat.MessageType.Audio:
//                    break;
//                default:
//                    break;
//            }
//            var timeLabel = new Label();
//            DateTime messageLocalTime = message.Creation.ToLocalTime();
//            TimeSpan difference = DateTime.Now - messageLocalTime;
//            timeLabel.Text = difference.TotalDays < 1 ? messageLocalTime.ToLongTimeString() : messageLocalTime.ToLongDateString() + " - " + messageLocalTime.ToLongTimeString();
//            timeLabel.HorizontalTextAlignment = align;
//#if !WINDOWS_UWP
//            //In windows this throws an error because it is outside the GUI thread
//            try
//            { //For an alleged bug of the current version of VS the compiler does not exclude the project UWP
//                if (Device.RuntimePlatform != Device.UWP) //Here the WINDOWS_UWP symbol cannot work because we are in the common library
//                {
//                    timeLabel.FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label));
//                }
//            }
//            catch (Exception) { }
//#endif

//            var flagAndTime = new StackLayout() { Orientation = StackOrientation.Horizontal };
//            var statusLabel = new Label() { Text = Status.pending.ToString() };
//            flagAndTime.Children.Add(timeLabel);

//            lock (_statusContacts)
//            {
//                if (!_statusContacts.TryGetValue(contact, out List<Tuple<DateTime, Label>> flags))
//                {
//                    flags = new List<Tuple<DateTime, Label>>();
//                    _statusContacts.Add(contact, flags);
//                }
//                flags.Add(Tuple.Create(message.Creation, statusLabel));
//            }
//            box.Children.Add(flagAndTime);
//            content = frame;
//        }
//        private enum Status
//        {
//            pending = '☐',
//            delivered = '☑',
//            readed = '✅',
//        }
//        private static Dictionary<Contact, List<Tuple<DateTime, Label>>> _statusContacts = new Dictionary<Contact, List<Tuple<DateTime, Label>>>();

        internal struct Template
        {
            public static Color BackgroundMessage = Color.FromRgb(0xb7, 0xcb, 0xf2);
            public static Color BackgroundMyMessage = Color.FromRgb(0xe2, 0xe8, 0xf3);
        }

    }
}