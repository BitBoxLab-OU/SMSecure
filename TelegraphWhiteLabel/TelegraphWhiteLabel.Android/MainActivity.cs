
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Com.Xamarin.Formsviewgroup;
using System.IO;
using System.Threading.Tasks;



namespace AnonymousWhiteLabel.Droid
{
	[Activity(Label = "TelegraphWhiteLabel", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity
	{
		internal static MainActivity Instance { get; private set; }
		protected override void OnCreate(Bundle savedInstanceState)
		{
			TabLayoutResource = Xamarin.Forms.Platform.Android.Resource.Layout.Tabbar;
			ToolbarResource = Xamarin.Forms.Platform.Android.Resource.Layout.Toolbar;
			base.OnCreate(savedInstanceState);
			Xamarin.Essentials.Platform.Init(this, savedInstanceState);
			Xamarin.Forms.Forms.Init(this, savedInstanceState);
			LoadApplication(new App());
			Instance = this;

			Rg.Plugins.Popup.Popup.Init(this);

			App.IgnoreBatteryOptimizations = () =>
			{
				//============= Ignore Battery Optimizations =============
				//https://stackoverflow.com/questions/32627342/how-to-whitelist-app-in-doze-mode-android-6-0
				if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
				{
					using (var intent = new Intent())
					{
						var packageName = BuildConfig.ApplicationId;
						var pm = (PowerManager)GetSystemService(PowerService);
						if (!pm.IsIgnoringBatteryOptimizations(packageName))
						{
							intent.SetAction(Settings.ActionRequestIgnoreBatteryOptimizations);
							intent.SetData(Android.Net.Uri.Parse("package:" + packageName));
							StartActivity(intent);
						}
					}
				}
				//========================================================
			};

		}
		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
		{
			Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

			base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		}

		// Field, property, and method for Picture Picker
		public static readonly int PickImageId = 1000;

		public TaskCompletionSource<Stream> PickImageTaskCompletionSource { set; get; }

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent)
		{
			base.OnActivityResult(requestCode, resultCode, intent);

			if (requestCode == PickImageId)
			{
				if ((resultCode == Result.Ok) && (intent != null))
				{
					Android.Net.Uri uri = intent.Data;
					Stream stream = ContentResolver.OpenInputStream(uri);

					// Set the Stream as the completion of the Task
					PickImageTaskCompletionSource.SetResult(stream);
				}
				else
				{
					PickImageTaskCompletionSource.SetResult(null);
				}
			}
		}

		protected override void OnNewIntent(Intent intent)
		{
			CreateNotificationFromIntent(intent);
		}

		void CreateNotificationFromIntent(Intent intent)
		{
			if (intent?.Extras != null)
			{
				var title = intent.Extras.GetString(AndroidNotificationManager.TitleKey);
				var message = intent.Extras.GetString(AndroidNotificationManager.MessageKey);
				Xamarin.Forms.DependencyService.Get<INotificationManager>().ReceiveNotification(title, message);
			}
		}

	}
}