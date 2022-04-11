﻿using System;
using Android.App;
using Android.Content;
using Android.OS;
using Telegraph.Droid.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(TaskKilledService))]
namespace Telegraph.Droid.Services
{
    [Service(Enabled = true, Exported = false)]
    public class TaskKilledService : IntentService
    {
        public TaskKilledService()
        {
        }

        public override void OnTaskRemoved(Intent rootIntent)
        {
            base.OnTaskRemoved(rootIntent);
            AndroidNotificationManager.GetInstance().CancelOnGoingCallNotification();
        }

        protected override void OnHandleIntent(Intent intent)
        {
        }
    }
}
