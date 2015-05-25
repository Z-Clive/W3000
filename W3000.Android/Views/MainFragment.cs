
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Support.V4.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using W3000.SharedLibrary;

namespace W3000.Android.Views
{
	public class MainFragment : Fragment
	{
		public Word Word { get; set; }

		public MainFragment(Word word)
		{
			Word = word;
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var fv = inflater.Inflate (Resource.Layout.MainFragment, container, false);
			var txtEng = fv.FindViewById<TextView> (Resource.Id.txtEng);
			var txtChn = fv.FindViewById<TextView> (Resource.Id.txtChn);

			txtEng.Text = Word.Eng;
			txtChn.Touch += (s, e) => txtChn.Text = Word.Chn;

			return fv;
		}
	}
}

