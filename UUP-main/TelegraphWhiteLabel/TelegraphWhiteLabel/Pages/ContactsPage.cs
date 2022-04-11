using System;
using EncryptedMessaging;
using EncryptedMessaging.Resources;
using Xamarin.Forms;
namespace AnonymousWhiteLabel.Pages
{
	internal class ContactsPage : ContentPage
	{
		//========= Setup =======	
		private readonly bool _doubleClickToEntry = false;
		//======= End Setup =====	

		public ContactsPage()
		{
			var edit = new ToolbarItem
			{
				Text = Dictionary.Edit,
			};
			edit.Command = new Command(() =>
			{
				edit.IsEnabled = false;
				if (_list.SelectedItem != null)
					Navigation.PushAsync(new ContactPage((Contact)_list.SelectedItem));
			});
			edit.IsEnabled = false;
			ChatPage = new ChatPage();
			ChatPage.Appearing += (sender, e) => edit.IsEnabled = true; //Assigning the command automatically sets the IsEnabled value to true. So this statement has to stay here.
			var add = new ToolbarItem
			{
				Text = Dictionary.Add,
			};
			add.Command = new Command(() =>
			{
				add.IsEnabled = false;
				Navigation.PushAsync(new ContactPage());
			});

			var oneTime = false;
			Appearing += (sender, e) =>
					{
						if (!oneTime && Parent is NavigationPage navigationPage)
						{
							oneTime = true;
							navigationPage.ToolbarItems.Add(add);
							navigationPage.ToolbarItems.Add(edit);
						}
						add.IsEnabled = true;
						_list.SelectedItem = null;
						_lastTapped = null;
					};
			Disappearing += (sender, e) => { add.IsEnabled = false; };
			Content = new StackLayout
			{
				VerticalOptions = LayoutOptions.CenterAndExpand,
				Children = { _list }
			};

			_list.ItemSelected += (sender, e) =>
			{
				edit.IsEnabled = e.SelectedItem != null;
				if (e.SelectedItem == null)
					_lastTapped = null;
			};

			_list.ItemTapped += (sender, e) =>
			{
				var contact = (Contact)e.Item;
				if (contact == _lastTapped || _doubleClickToEntry == false)
				{
					if (contact == null)
						return;
					if (Navigation.NavigationStack[Navigation.NavigationStack.Count - 1] == this)
					{
						ChatPage.SetCurrentContact(contact);
						Navigation.PushAsync(ChatPage);
					}
				}
				_lastTapped = contact;
			};
		}

		private Contact _lastTapped; //Necessary to detect the double tapped

		private readonly ListView _list = new ListView
		{
			ItemsSource = App.Context.Contacts.GetContacts(),
			ItemTemplate = _contactDataTemplate,
			Margin = new Thickness(10, 0, 10, 0),
		};

		public class IntToFontAttributes : IValueConverter
		{
			public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => (FontAttributes)value;
			public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => (int)value;
		}

		public class IsNotZero : IValueConverter
		{
			public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => (int)value != 0;
			public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => (bool)value ? 1 : 0;
		}

		private static readonly DataTemplate _contactDataTemplate = new DataTemplate(() =>
		{
			var grid = new Grid();
			var nameLabel = new Label { FontAttributes = FontAttributes.Bold };
			nameLabel.SetBinding(Label.TextProperty, nameof(Contact.Name));

			var lastMessage = new Label();
			lastMessage.SetBinding(Label.TextProperty, nameof(Contact.LastMessagePreview));
			lastMessage.SetBinding(Label.FontAttributesProperty, nameof(Contact.LastMessageFontAttributes), BindingMode.Default, new IntToFontAttributes());

			var lastMessageTimeDistance = new Label { HorizontalTextAlignment = TextAlignment.End };
			lastMessageTimeDistance.SetBinding(Label.TextProperty, nameof(Contact.LastMessageTimeDistance));

			var ballon = new Frame { Margin = 0, Padding = 0, WidthRequest = 18, HeightRequest = 18, CornerRadius = 9, BackgroundColor = Color.Red, HorizontalOptions = LayoutOptions.Start };
			ballon.SetBinding(IsVisibleProperty, nameof(Contact.UnreadMessages), BindingMode.Default, new IsNotZero());
			var unreadMessages = new Label { FontSize = 9, BackgroundColor = Color.Transparent, TextColor = Color.White, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
			unreadMessages.SetBinding(Label.TextProperty, nameof(Contact.UnreadMessages));
			ballon.Content = unreadMessages;

			var box = new StackLayout { Orientation = StackOrientation.Horizontal, Children = { ballon, nameLabel } };
			grid.Children.Add(box);
			grid.Children.Add(lastMessageTimeDistance, 1, 0);
			grid.Children.Add(lastMessage, 0, 1);

			return new ViewCell
			{
				View = grid
			};
		});
		public static ChatPage ChatPage;
	}
}
