
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using W3000.Android.Adapters;
using W3000.SharedLibrary;
using Android.Support.V4.View;
using W3000.Android.Helpers;
using System.Runtime.CompilerServices;
using Android.Util;
using Java.Net;
using Android.Speech.Tts;
using System.IO;
using System.Threading;
using System.Reflection;

namespace W3000.Android.Activities
{
	[Activity (Label = "学习", Icon = "@drawable/icon")]			
	public class MainActivity : FragmentActivity, TextToSpeech.IOnInitListener
	{
		MainFragmentAdapter _fragmentAdapter;
		CourseManager _courseManager;
		ViewPager _viewPager;
		TextToSpeech _tts;
		ProgressBar _progressBar;

		string _path;
		bool _isReviewing;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Create your application here
			SetContentView (Resource.Layout.MainViewLayout);

			var progress = ProgressDialog.Show (this, "准备中", "正在导入课程...");

			ThreadPool.QueueUserWorkItem ((sen) =>
			{
				_path = Intent.GetStringExtra ("Path");
				var isRandom = Intent.GetBooleanExtra ("IsRandom", false);
				using (var sr = new StreamReader (_path)) {
					_courseManager = new CourseManager (sr, isRandom);
				}

				_fragmentAdapter = new MainFragmentAdapter (SupportFragmentManager, _courseManager);
				_viewPager = FindViewById<ViewPager> (Resource.Id.mainViewPager);
				
				_progressBar = FindViewById<ProgressBar> (Resource.Id.pbRating);
				_progressBar.Progress = _courseManager.CurrentWord.Rating;

				var btnGood = FindViewById<Button> (Resource.Id.btnGood);
				btnGood.Click += (s, e) => {
					_courseManager.SetRating (WordRating.Good);

					_viewPager.SetCurrentItem (++_viewPager.CurrentItem, true);
				};

				var btnSoso = FindViewById<Button> (Resource.Id.btnSoso);
				btnSoso.Click += (s, e) => {
					_courseManager.SetRating (WordRating.Soso);

					_viewPager.SetCurrentItem (++_viewPager.CurrentItem, true);
				};

				var btnBad = FindViewById<Button> (Resource.Id.btnBad);
				btnBad.Click += (s, e) => {
				_courseManager.SetRating (WordRating.Poor);

				_viewPager.SetCurrentItem (++_viewPager.CurrentItem, true);
				};

				_viewPager.PageSelected += (s, e) =>
				{
					_courseManager.MoveTo (e.Position);
					_progressBar.Progress = _courseManager.CurrentWord.Rating;

					_fragmentAdapter.NotifyDataSetChanged ();
				};

				RunOnUiThread (()=>
						{
							_tts = new TextToSpeech(this,this);
							_viewPager.Adapter = _fragmentAdapter;
							_viewPager.OffscreenPageLimit = 0;
							progress.Dismiss ();
						});
			});
		}

		public override bool OnCreateOptionsMenu (IMenu menu)
		{
			MenuInflater.Inflate (Resource.Menu.MainActivityMenu, menu);

			return true;
		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			switch (item.ItemId)
			{
				case Resource.Id.mainMenuReview:
					if(_isReviewing)
					{
						_courseManager.LeaveReview ();
						_fragmentAdapter = new MainFragmentAdapter (SupportFragmentManager, _courseManager);
						_viewPager = FindViewById<ViewPager> (Resource.Id.mainViewPager);
						_viewPager.Adapter = _fragmentAdapter;

						item.SetTitle ("开始回顾");
						_isReviewing = false;
					}
					else
					{
						if (_courseManager.EnterReview ()) {
							_fragmentAdapter = new MainFragmentAdapter (SupportFragmentManager, _courseManager);
							_viewPager = FindViewById<ViewPager> (Resource.Id.mainViewPager);
							_viewPager.Adapter = _fragmentAdapter;

							item.SetTitle ("停止回顾");
							_isReviewing = true;
						}
						else
							Toast.MakeText (this, "恭喜你，没有需要复习的单词！", ToastLength.Short).Show ();
					}
					return true;
				case Resource.Id.mainMenuSave:
					var savePath = _path.Substring (0, _path.Length - 3) + "saved.txt";
					try
					{
						using(var sw = new StreamWriter (savePath))
						{
							_courseManager.SaveAndOrderBy (sw, w => w.Rating);
						}
						Toast.MakeText (this, "保存成功！保存至：" + savePath, ToastLength.Short).Show ();
					}
					catch (Exception ex)
					{
						Toast.MakeText (this, "保存失败！" + ex.Message, ToastLength.Short).Show ();
					}
					return true;
				default:
					return base.OnOptionsItemSelected (item);
			}
		}

		public void OnInit (OperationResult status)
		{
			if(status == OperationResult.Success)
			{
				var result = _tts.SetLanguage (Java.Util.Locale.Us);
				if(result != LanguageAvailableResult.MissingData
					&& result != LanguageAvailableResult.NotSupported)
				{
					_tts.Speak ("Welcome to your course manager.", QueueMode.Add, null);

					_viewPager.PageSelected += (s, e) =>
					{
						_tts.Speak (_courseManager.CurrentWord.Eng, QueueMode.Flush, null);
					};
				}
			}
		}
	}
}

