using System;
using System.Threading;
using Telegraph.iOS.Services;
using Telegraph.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(CloseApplicationManager))]
namespace Telegraph.iOS.Services
{
    public class CloseApplicationManager : ICloseApplication
    {
        public CloseApplicationManager()
        {
        }

        public void CloseApplication() => Thread.CurrentThread.Abort();

    }
}
