
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using W3000.SharedLibrary;
using W3000.Android.Views;

namespace W3000.Android.Adapters
{
	public class MainFragmentAdapter : FragmentStatePagerAdapter
	{
		CourseManager _courseManager;

		public MainFragmentAdapter(FragmentManager fm, CourseManager cm)
			:base(fm)
		{
			_courseManager = cm;
		}

		public override int Count { get { return _courseManager.Count; } }

		public override Fragment GetItem (int position)
		{
			var word = _courseManager.GetWordAt (position);
			var mf = new MainFragment (word);

			return mf;
		}

		public override int GetItemPosition (Java.Lang.Object @object)
		{
			return PositionNone;
		}
	}
}

