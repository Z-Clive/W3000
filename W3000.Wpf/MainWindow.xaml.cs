// 2015/4/14 - 21:59
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using W3000.SharedLibrary;
using System.Speech.Synthesis;
using System.IO;
using System.Threading.Tasks;

namespace W3000.Wpf
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		double _windowWidth;
		double _windowHeight;
		double _left;
		double _top;

		readonly SpeechSynthesizer _speaker = new SpeechSynthesizer ();
        CourseManager cou;
        
		bool _isReviewing;

		public MainWindow ()
		{
			InitializeComponent ();            
            
			foreach (var voice in _speaker.GetInstalledVoices()) {
				cbSpeakers.Items.Add (voice.VoiceInfo.Name);
			}
            
			cbSpeakers.SelectedItem = cbSpeakers.Items [0];
            
			KeyDown += onKeyDown;
            
			_setButtonsEnabledStatus (false);
		}

		void onKeyDown (object sender, KeyEventArgs e)
		{
			switch (e.Key) {
			case Key.S:
				btnBad_Click (null, null);
				break;
			case Key.D:
				btnSoso_Click (null, null);
				break;
			case Key.F:
				btnGood_Click (null, null);
				break;
			case Key.J:
				btnEnter_Click (null, null);
				break;
			case Key.K:
				btnPrevious_Click (null, null);
				break;
			case Key.L:
				btnNext_Click (null, null);
				break;
			case Key.H:
				_speaker.SpeakAsync (cou.CurrentWord.Eng);
				break;
			}
		}

		void btnBad_Click (object sender, RoutedEventArgs e)
		{
			cou.SetRating (WordRating.Poor);
			cou.MoveNext ();
            
			_displayWord ();
		}

		void _displayWord ()
		{
			lblEng.Content = string.Empty;
			lblChn.Content = string.Empty;
			pbRating.Value = 0;
            
			if (cou.Position < cou.Count) {
				lblEng.Content = cou.CurrentWord.Eng;
				pbRating.Value = cou.CurrentWord.Rating;
                
				_speaker.SpeakAsync (cou.CurrentWord.Eng);
			} else {
				MessageBox.Show ("恭喜你完成了所有任务！");
				lblEng.Content = "完成";
			}
		}

		void btnSoso_Click (object sender, RoutedEventArgs e)
		{
			cou.SetRating (WordRating.Soso);
			cou.MoveNext ();
            
			_displayWord ();
		}

		void btnGood_Click (object sender, RoutedEventArgs e)
		{
			cou.SetRating (WordRating.Good);
			cou.MoveNext ();
            
			_displayWord ();
		}

		void btnEnter_Click (object sender, RoutedEventArgs e)
		{
			lblChn.Content = cou.CurrentWord.Chn;
		}

		void btnToggleFullScreen_Click (object sender, RoutedEventArgs e)
		{
			if (WindowState == WindowState.Maximized) {
				WindowStyle = WindowStyle.SingleBorderWindow;
				WindowState = WindowState.Normal;
				Width = _windowWidth;
				Height = _windowHeight;
				Left = _left;
				Top = _top;
			} else {
				_windowHeight = Height;
				_windowWidth = Width;
				_top = Top;
				_left = Left;
                
				WindowStyle = WindowStyle.None;
				WindowState = WindowState.Maximized;
				Width = SystemParameters.VirtualScreenWidth;
				Height = SystemParameters.VirtualScreenHeight;
				Left = 0;
				Top = 0;
			}
		}

		void btnReview_Click (object sender, RoutedEventArgs e)
		{
			if (_isReviewing) {
				cou.LeaveReview ();
				_displayWord ();
				_isReviewing = false;
                
				btnReview.Content = "开始回顾";
			} else {
				if (!cou.EnterReview ()) {
					var result = MessageBox.Show ("恭喜你完成所有任务！是否继续学习？",
						             "提示", MessageBoxButton.YesNo);
					if (result == MessageBoxResult.Yes) {
						cou.LeaveReview ();
					} else {
						cou = null;
						_setButtonsEnabledStatus (false);
					}
                    
					_isReviewing = false;
				} else {
					_displayWord ();
					_isReviewing = true;
                    
					btnReview.Content = "停止回顾";
				}
			}
		}

		void btnExport_Click (object sender, RoutedEventArgs e)
		{
			var sfd = new SaveFileDialog (){ Filter = "单词文档 (*.txt) | *.txt" };
			if (sfd.ShowDialog () != System.Windows.Forms.DialogResult.OK) {
				return;
			}
            
			try {
				using (var fs = new FileStream (sfd.FileName, FileMode.Create))
				using (var sw = new StreamWriter (fs)) {
					cou.SaveAndOrderBy (sw, w => w.Rating);
				}
                
				MessageBox.Show ("导出成功！");
			} catch {
				MessageBox.Show ("导出失败！");
				if (File.Exists (sfd.FileName)) {
					File.Delete (sfd.FileName);
				}
			}
		}

		void btnNext_Click (object sender, RoutedEventArgs e)
		{
			cou.MoveNext ();
			_displayWord ();
		}

		void btnPrevious_Click (object sender, RoutedEventArgs e)
		{
			cou.MovePrev ();
			_displayWord ();
		}

		void btnOpen_Click (object sender, RoutedEventArgs e)
		{
			var ofd = new OpenFileDialog (){ Filter = "单词文档 (*.txt) | *.txt" };
			if (ofd.ShowDialog () != System.Windows.Forms.DialogResult.OK) {
				return;
			}
            
			try {
				using (var sr = new StreamReader (ofd.FileName)) {
					cou = new CourseManager (sr, chkIsRandom.IsChecked ?? false);
				}
			} catch (Exception ex) {
				MessageBox.Show (ex.Message);
				return;
			}
            
			_setButtonsEnabledStatus (true);
			_displayWord ();
		}

		void cbSpeakers_SelectionChanged (object sender, SelectionChangedEventArgs e)
		{
			try {
				_speaker.SelectVoice (e.AddedItems [0].ToString ());
			} catch {
				MessageBox.Show ("切换出错！");
				_speaker.SelectVoice (_speaker.Voice.Name);
			}
		}

		void _setButtonsEnabledStatus (bool isEnabled)
		{
			foreach (var v in gridRoot.Children) {
				if (v is Button && (v as Button).Name != "btnOpen") {
					(v as Button).IsEnabled = isEnabled;
				}
			}
		}
	}
}