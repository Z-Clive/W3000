using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Uri = Android.Net;
using W3000.Android.Helpers;
using Java.Net;
using W3000.SharedLibrary;
using System.IO;

namespace W3000.Android.Activities
{
	[Activity (Label = "W3000", MainLauncher = true, Icon = "@drawable/icon")]
	public class OpenFileActivity : Activity
	{
		public void ShowFileChooser(string fileType)
		{
			var intent = new Intent (Intent.ActionGetContent);
			intent.SetType(FileUtil.GetMIME(fileType));
			intent.AddCategory (Intent.CategoryOpenable);

			try
			{
				this.StartActivityForResult(Intent.CreateChooser (intent,"请选择文件："), 0);
			}
			catch
			{
				Toast.MakeText (this, "请安装文件管理器。", ToastLength.Short).Show ();
			}
		}

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.OpenFileLayout);

			var btnOpen = FindViewById<Button> (Resource.Id.btnOpen);
			btnOpen.Click += btnOpen_Click;
		}

		void btnOpen_Click (object sender, EventArgs e)
		{
			ShowFileChooser (".txt");
		}

		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			if (resultCode == Result.Ok)
			{
				var path = FileUtil.GetPath (this, data.Data);
				var cbIsRandom = FindViewById<CheckBox> (Resource.Id.cbIsRandom);
				try
				{
					var intent = new Intent (this, typeof(MainActivity));
					intent.PutExtra ("Path", path);
					intent.PutExtra ("IsRandom", cbIsRandom.Checked);
					StartActivity (intent);
				}
				catch (URISyntaxException ex)
				{
					// TODO Auto-generated catch block
					ex.PrintStackTrace();
				}
			}

			base.OnActivityResult(requestCode, resultCode, data);
		}
	}
}


