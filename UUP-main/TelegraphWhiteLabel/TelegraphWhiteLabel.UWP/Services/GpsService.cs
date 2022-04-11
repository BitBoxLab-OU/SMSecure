using AnonymousWhiteLabel.UWP.Services;
using MessageCompose.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.System;
using Xamarin.Forms;

[assembly: Dependency(typeof(GpsService))]
namespace AnonymousWhiteLabel.UWP.Services
{
    public class GpsService : IGpsService
    {
        public bool IsGpsEnable() {
            var accessStatus =  Geolocator.RequestAccessAsync();

            //switch (accessStatus)
            //{
            //    case GeolocationAccessStatus.:

            //}
            
                
            return false;
        }
        public void OpenSettings()
        {
            _ =  Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-location"));
        }
    }
}
