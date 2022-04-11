using System;
using Xamarin.Forms;

namespace Telegraph.ViewModels
{
	public class FlyoutPageItem
	{
		public string Title { get; set; }

		public ImageSource IconSource { get; set; }

		public Type TargetType { get; set; }
	}
}
