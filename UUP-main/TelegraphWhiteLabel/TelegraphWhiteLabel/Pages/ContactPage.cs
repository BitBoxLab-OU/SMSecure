using EncryptedMessaging;
using EncryptedMessaging.Resources;
using Xamarin.Forms;

namespace AnonymousWhiteLabel.Pages
{
	internal class ContactPage : ContentPage
	{
		public ContactPage(Contact contact = null)
		{
			Title = Dictionary.Contact;
			var labelPubKey = new Label { Text = Dictionary.PublicKey + ":" };
			var qrCode = new Entry { Text = contact?.GetQrCode(), IsSpellCheckEnabled = false };
			var labelName = new Label { Text = Dictionary.Name + ":" };
			var name = new Entry { Text = contact?.Name, IsSpellCheckEnabled = false };
			name.IsEnabled = !string.IsNullOrEmpty(name.Text);
			var labelChatId = new Label { Text = "ChatId = " + contact?.ChatId.ToString() };
			var labelUserId = new Label { Text = "UserId = " + contact?.UserId.ToString() };
			qrCode.TextChanged += (o, e) =>
			{
				try
				{
					var contactMessage = new ContactMessage(qrCode.Text);
					var participants = contactMessage.GetParticipantsKeys(App.Context);
					name.Text = string.IsNullOrEmpty(qrCode.Text) ? "" : App.Context.Contacts.Pseudonym(participants);
					name.IsEnabled = !string.IsNullOrEmpty(name.Text);
				}
				catch (System.Exception) { }
			};

			var ok = new Button { Text = Dictionary.Ok };
			ok.Clicked += (o, e) =>
			{
				if (contact != null)
					contact.Name = name.Text;
				else
				{
					try
					{
						var contactMessage = new ContactMessage(qrCode.Text);
						var newContact = App.Context.Contacts.AddContact(contactMessage);
						if (!string.IsNullOrEmpty(name.Text))
						{
							newContact.Name = name.Text;
						}
					}
					catch (System.Exception)
					{
						DisplayAlert(Dictionary.Alert, Dictionary.InvalidKey, Dictionary.Ok);
						return;
					}
				}
				Navigation.PopAsync();
			};
			var delete = new Button { Text = Dictionary.Delete, IsVisible = contact != null };
			delete.Clicked += (o, e) =>
			{
				App.Context.Contacts.RemoveContact(contact);
				ContactsPage.ChatPage.SetCurrentContact(null);
				Navigation.PopAsync();
			};
			var cancel = new Button { Text = Dictionary.Cancel };
			cancel.Clicked += (o, e) => Navigation.PopAsync();

			StackLayout stack;
			if (contact == null)
				stack = new StackLayout { Orientation = StackOrientation.Vertical, Children = { labelPubKey, qrCode, labelName, name, ok, delete, cancel } };
			else
				stack = new StackLayout { Orientation = StackOrientation.Vertical, Children = { labelPubKey, qrCode, labelName, name, labelChatId, labelUserId, ok, delete, cancel } };
			Content = stack;
		}
	}
}
