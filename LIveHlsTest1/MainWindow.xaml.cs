using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace LIveHlsTest1
{
	/// <summary>
	/// HLSの1動画ファイル情報
	/// </summary>
	public class HlsTsMedia
	{
		/// <summary>
		/// 動画の長さ（EXTINF）
		/// </summary>
		public double Duration { get; set; }

		/// <summary>
		/// ダウンロードURI
		/// </summary>
		public string uri { get; set; }
	}

	/// <summary>
	/// アクセストークン、リフレッシュトークンの戻り値
	/// </summary>
	public class TestTokenResponse
	{
		public string access_token { get; set; }
		public string token_type { get; set; }
		public long expires_in { get; set; }
		public string refresh_token { get; set; }
	}

	/// <summary>
	/// HLSでライブ配信を行う（テスト１）
	/// </summary>
	public partial class MainWindow : Window
	{
		List<HlsTsMedia> m_listHlsTsMedia = new List<HlsTsMedia>();

		/// <summary>
		/// ライブ用m3u8ファイルのパス（これを書き換える）
		/// </summary>
		string m_m3u8Path = ".\\m3u8\\ffmpeg_out.m3u8";

		string m_tsSrcFoldr = ".\\m3u8\\ffmpeg_out";
		string m_tsDstFolder = ".\\m3u8";

		/// <summary>
		/// キャンセルトークン
		/// </summary>
		CancellationTokenSource m_cancelTokenSrc = null;


		/// <summary>
		/// コンストラクタ
		/// </summary>
		public MainWindow()
		{
			InitializeComponent();

			btnStop.IsEnabled = false;
			btnReset.IsEnabled = false;

			// HLSデータの設定
			m_listHlsTsMedia.Add(new HlsTsMedia() { uri = "video000.ts", Duration = 8.333322, });
			m_listHlsTsMedia.Add(new HlsTsMedia() { uri = "video001.ts", Duration = 8.333333, });
			m_listHlsTsMedia.Add(new HlsTsMedia() { uri = "video002.ts", Duration = 7.966656, });
			m_listHlsTsMedia.Add(new HlsTsMedia() { uri = "video003.ts", Duration = 6.999989, });
			m_listHlsTsMedia.Add(new HlsTsMedia() { uri = "video005.ts", Duration = 8.333322, });
		}

		/// <summary>
		/// 開始ボタン押下
		/// </summary>
		private void btnStart_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				// ライブ配信タスクの開始
				if (m_cancelTokenSrc == null) m_cancelTokenSrc = new CancellationTokenSource();
				var token = m_cancelTokenSrc.Token;
				var task = Task.Factory.StartNew(() => DoLiveStreaming(token), token)
					.ContinueWith(t =>
					{
						m_cancelTokenSrc.Dispose();
						m_cancelTokenSrc = null;
					});

				btnStart.IsEnabled = false;
				btnStop.IsEnabled = true;
				btnReset.IsEnabled = false;
			}
			catch (Exception ex)
			{
				MessageBox.Show($"例外発生. {ex.Message}");
			}
		}

		/// <summary>
		/// 終了ボタン押下
		/// </summary>
		private void btnStop_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				// ライブ配信タスクの終了
				if (m_cancelTokenSrc != null)
				{
					m_cancelTokenSrc.Cancel(true);
				}

				btnStart.IsEnabled = true;
				btnStop.IsEnabled = false;
				btnReset.IsEnabled = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show($"例外発生. {ex.Message}");
			}
		}

		/// <summary>
		/// リセットボタン押下
		/// </summary>
		private void btnReset_Click(object sender, RoutedEventArgs e)
		{
			try
			{

			}
			catch (Exception ex)
			{
				MessageBox.Show($"例外発生. {ex.Message}");
			}
		}

		/// <summary>
		/// ライブ配信タスク用
		/// </summary>
		/// <param name="cancelToken">キャンセルトークン</param>
		private void DoLiveStreaming(CancellationToken cancelToken)
		{
			int maxTsIndex = m_listHlsTsMedia.Count - 1;
			int tsIndex = 0;
			ulong sequenceVal = 0;

			DateTime dtEndTs = DateTime.Now;
			System.Diagnostics.Debug.WriteLine($"[{dtEndTs:HH:mm:ss.ffffff}] Live Start.");

			// 書き換えループ
			while (!cancelToken.IsCancellationRequested)
			{
				var tsMedia = m_listHlsTsMedia[tsIndex];
				dtEndTs = dtEndTs.AddSeconds(tsMedia.Duration);

				// tsファイルを置く ⇒ 今回は配置済み

				// m3u8ファイルを書き換える
				WriteM3U8(sequenceVal, tsMedia);
				System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.ffffff}] change m3u8. ({sequenceVal})");

				Dispatcher.Invoke((Action)(() => { txtCount.Text = sequenceVal.ToString(); }));

				// デュレーションの6割を超えるまで待つ。
				DateTime dtWait = dtEndTs.AddSeconds(tsMedia.Duration * -0.4);
				while (dtWait > DateTime.Now)
				{
					if (cancelToken.IsCancellationRequested)
						break;
					Thread.Sleep(10);
				}

				if (++tsIndex > maxTsIndex) tsIndex = 0;
				sequenceVal++;
			}
		}

		/// <summary>
		/// u3m8ファイルを書き換える
		/// </summary>
		/// <param name="sequenceVal">シーケンス値</param>
		/// <param name="tsMedia">動画ファイル情報</param>
		private void WriteM3U8(ulong sequenceVal, HlsTsMedia tsMedia)
		{
			// tsファイルを名前を換えてコピー
			string srcTs = System.IO.Path.Combine(m_tsSrcFoldr, tsMedia.uri);
			string dstTs = System.IO.Path.Combine(m_tsDstFolder, $"output{sequenceVal}.ts");
			File.Copy(srcTs, dstTs, true);

			// m3u8ファイルの書き換え
			string tsUri = $"/output{sequenceVal}.ts";
			using (var writer = new StreamWriter(m_m3u8Path))
			{
				string m3u8 =
					"#EXTM3U\n" +
					"#EXT-X-VERSION:3\n" +
					"#EXT-X-TARGETDURATION:9\n" +
					$"#EXT-X-MEDIA-SEQUENCE:{sequenceVal}\n" +
					"#EXT-X-DISCONTINUITY\n" +
					"#EXT-X-PLAYLIST-TYPE:EVENT\n" +
					$"#EXTINF:{tsMedia.Duration:F6},\n" +
					tsUri;

				writer.WriteLine(m3u8);
			}
		}

		/// <summary>
		/// Authorizeボタン押下
		/// </summary>
		private async void btnAuthorize_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				// サーバから送られた認可コード
				string authCode = "";
				// アクセストークン
				string accessToken = "";
				// リフレッシュトークン
				string refreshToken = "";

				// 認可用ブラウザ画面
				var webWin = new AuthorizeWindow();
				webWin.Owner = this;

				// 簡易WEBサーバを起動
				var listener = new HttpListener();
				listener.Prefixes.Add("http://localhost:10003/");
				listener.Start();

				var task = Task.Factory.StartNew(() =>
				{
					// 認可コードが届くのを待つ
					var context = listener.GetContext();
					var req = context.Request;
					var res = context.Response;

					authCode = req.QueryString.Get("code");

					{
						byte[] text = Encoding.UTF8.GetBytes(
							"<html><head><meta charset='utf-8'/></head><body><center>" +
							"認証が完了しました。</br>画面を閉じてください。" +
							"</center></body></html>");
						res.OutputStream.Write(text, 0, text.Length);
					}
					res.StatusCode = 200;
					res.Close();

					listener.Stop();
					listener.Close();

					// 認可ウィンドウを閉じる
					Dispatcher.Invoke((Action)(() => { webWin.Close(); }));
				});

				// 認可ウィンドウを開き、そこから認可URLにアクセス
				webWin.ShowDialog();

				// 取得した認可コードの表示
				if (!string.IsNullOrEmpty(authCode))
				{
					txtAuthCode.Text = authCode;
				}

				// アクセストークンの取得
				try
				{
					var param = new Dictionary<string, string>()
					{
						{ "client_id", "AAA" },							// クライアントID
						{ "client_secret", "BBB" },						// クライアントシークレット
						{ "grant_type", "authorization_code " },		// 認証タイプ
						{ "redirect_uri", "http://localhost:10003/" },	// ユーザー認証で指定したURL
						{ "code", authCode },							// 認可コード
					};

					var content = new FormUrlEncodedContent(param);

					using (var client = new HttpClient())
					{
						var response = await client.PostAsync("https://localhost:44377/api/auth/token", content);
						var resStatusCoode = response.StatusCode;

						var resBodyJsonStr = response.Content.ReadAsStringAsync().Result;
						var testToken = JsonSerializer.Deserialize<TestTokenResponse>(resBodyJsonStr);
						accessToken = testToken.access_token;
						refreshToken = testToken.refresh_token;
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show($"アクセストークン取得時に例外が発生. {ex.Message}");
				}

				if (!string.IsNullOrEmpty(accessToken))
				{
					txtAccessToken.Text = accessToken;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"例外発生. {ex.Message}");
			}
		}
	}
}
