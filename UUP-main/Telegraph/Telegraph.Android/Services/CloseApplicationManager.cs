using System;
using Android.App;
using Telegraph.Droid.Services;
using Telegraph.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(CloseApplicationManager))]
namespace Telegraph.Droid.Services
{
    public class CloseApplicationManager: ICloseApplication
    {
        public void CloseApplication()
        {
            var activity = (Activity)Forms.Context;
            if(activity!=null)
                activity.FinishAffinity();
        }
    }
}
