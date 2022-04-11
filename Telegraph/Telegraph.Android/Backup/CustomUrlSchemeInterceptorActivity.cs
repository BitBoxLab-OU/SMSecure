using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Xamarin.Auth;
namespace Telegraph.Droid.Backup
{
    [Activity(Label = "CustomUrlSchemeInterceptorActivity", NoHistory = true, LaunchMode = LaunchMode.SingleTop)]
    [IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataSchemes = new[] { "com.dtsocialize.uup" }, DataPath = "/oauth2redirect")]
    public class CustomUrlSchemeInterceptorActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            global::Android.Net.Uri uri_android = Intent.Data;
            CustomTabsConfiguration.CustomTabsClosingMessage = null;
            var uri = new Uri(Intent.Data.ToString());
            GoogleDriveHelper.Auth.OnPageLoading(uri);
            var intent = new Intent(this, typeof(MainActivity));
            intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
            StartActivity(intent);
            this.Finish();
            return;
        }
    }

}