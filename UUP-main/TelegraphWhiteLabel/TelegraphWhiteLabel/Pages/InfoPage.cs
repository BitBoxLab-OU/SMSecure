using EncryptedMessaging.Resources;
using Xamarin.Forms;

namespace AnonymousWhiteLabel.Pages
{
    public class InfoPage : ContentPage
    {
        public InfoPage()
        {
            _grid = new Grid();
            _grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            _grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            _grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            _grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            _grid.ColumnDefinitions.Add(new ColumnDefinition());
            _grid.Children.Add(new Label { Text = Dictionary.Info }, 0, 0);
            var labelPublicKey = new Label { Text = Dictionary.PublicKey + ":" };
            var publicKey = new Editor { Text = App.Context.My.Contact.GetQrCode(), IsReadOnly = true };
            var sharePublicKeyButton = new Button { Text = Dictionary.Share };
            sharePublicKeyButton.Command = new Command(() => Xamarin.Essentials.Share.RequestAsync(publicKey.Text));
            var showPassphrase = new Button { Text = Dictionary.ShowPassphrase };
            showPassphrase.Command = new Command(async () => await DisplayAlert("Passphrase", App.Context.My.GetPassphrase(), Dictionary.Ok));
            var pubKeyLayout = new StackLayout { Orientation = StackOrientation.Vertical, Children = { labelPublicKey, publicKey, sharePublicKeyButton, showPassphrase } };
            _grid.Children.Add(pubKeyLayout, 0, 1);
            Content = _grid;

        }
        private readonly Grid _grid;
    }

}
