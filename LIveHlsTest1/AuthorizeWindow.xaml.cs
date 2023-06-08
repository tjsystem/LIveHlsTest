using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LIveHlsTest1
{
	/// <summary>
	/// AuthorizeWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class AuthorizeWindow : Window
	{
		public AuthorizeWindow()
		{
			InitializeComponent();

			// WEBブラウザに認可画面を表示させる。 パラメータはとりあえず決め打ち
			var uri = new Uri("https://localhost:44377/api/auth/authorize?client_id=AAA&response_type=code&scope=hoge-api&redirect_uri=http://localhost:10003/");
			webAuthorize.Source = uri;
			txtAddrBar.Text = uri.AbsoluteUri;
		}

		/// <summary>
		/// ナビゲート終了時
		/// </summary>
		private void webAuthorize_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
		{
			try
			{
				Mouse.OverrideCursor = null;
				txtAddrBar.Text = webAuthorize.Source.AbsoluteUri;
			}
			catch (Exception ex)
			{
				MessageBox.Show($"例外発生. {ex.Message}");
			}
		}
	}
}
