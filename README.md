### USBカメラ映像をHLSで配信するテスト

1. ffmpegで、USBカメラからHLS用ファイルを作らせる。　出力先はIISの公開フォルダ。

    `ffmpeg -f dshow -i video="HD Pro Webcam C920" -f hls -hls_time 9 -hls_playlist_type vod -hls_segment_filename ".\m3u8\ffmpeg_out\video%3d.ts" .\m3u8\ffmpeg_out\video.m3u8`  
    ⇒ これだと、VODになってしまった。 IIS経由でちゃんと再生はされた。 カメラ映像は左右反転  
    
    `ffmpeg -f dshow -i video="HD Pro Webcam C920" -f hls -hls_time 9 -hls_playlist_type event -hls_segment_filename ".\m3u8\ffmpeg_out\video%3d.ts" .\m3u8\ffmpeg_out\video.m3u8`  
   ⇒ EVENT（ライブ）になってくれて、IIS経由でも再生された。　もりもりtsファイルが作られる。m3u8ファイルが作られるまでにそれなりに時間がかかり、その分遅延する。10秒以上。  

    `ffmpeg -f dshow -i video="HD Pro Webcam C920" -f hls -hls_time 5 -hls_list_size 5 -hls_wrap 5 -hls_playlist_type event -hls_segment_filename ".\m3u8\ffmpeg_out\video%3d.ts" .\m3u8\ffmpeg_out\video.m3u8`  
	⇒ hls_wrap を指定すると、出力されるtsファイルの数が抑えられる。　m3u8ファイルの方は積み上げられるだけで、EXT-X-MEDIA-SEQUENCE も0のままになってしまった。
	
2. 出力されたm3u8を見て、tsファイルを別の場所へコピー、用ファイルを内部で保持し、数分遅らせてIISの公開フォルダへ送る。	

### OAUTH2認可サーバへの問い合わせテスト
  - c#でtcpポートを開けて、疑似WEBサーバを立てる
  - c#からブラウザを起動し、認可サーバへアクセスさせる[GET]。クライアントIDとか必要。
  - 認可サーバでOKになれば、ブラウザにリダイレクト（302）が返ってくる[GET]。 クエリパラメータには認可コードが追加されている。	
  - C#からWEBアクセスでアクセストークンを取得に行く[POST]。 リフレッシュトークンも一緒に返ってくるはず。
  - 参考ページ
    - [C#でトークンを取得してセットしたい。](https://teratail.com/questions/57509)
    - [FacebookのOAuth認証で取得したアクセストークンをC#で検証する方法](https://www.aruse.net/entry/2018/10/20/121115)
    - [Windows (WPF) アプリに認証を追加する](https://learn.microsoft.com/ja-jp/azure/developer/mobile-apps/azure-mobile-apps/quickstarts/wpf/authentication)
    - [C# 今更ですが、HttpClientを使う](https://qiita.com/rawr/items/f78a3830d894042f891b)
    - [C# による OAuth 2.0 と OpenID Connect の実装 (Authlete)](https://qiita.com/TakahikoKawasaki/items/657ef040802f8524403a)


### 01authorize.html
```
<!DOCTYPE html>
<html lang="ja" >
  <head>
    <meta charset="UTF-8">
    <title>Test for Authorize GET</title>
  </head>
  <body>
    <a href="https://localhost:44377/api/auth/authorize?client_id=AAA&response_type=code&scope=asfie-api&redirect_uri=http://localhost:10003/">
    認可ページへGET
    </a>
  </body>
</html>
```

### Sample: html & m3u8
#### inde.html
```
<!DOCTYPE html>
<html lang="ja" >
  <head>
    <meta charset="UTF-8">
    <title>Test for HTTP Live Streaming</title>

    <link href="//vjs.zencdn.net/7.10.2/video-js.min.css" rel="stylesheet">
    <script src="//vjs.zencdn.net/7.10.2/video.min.js"></script>
  </head>
  <body>
    <br/>
    <video-js id="video3" width=480 height=270 class="video-js vjs-default-skin vjs-big-play-centered"
        autoplay muted controls preload="auto" data-setup=''>
      <source src="ffmpeg_out.m3u8">
    </video-js>
  </body>
</html>
```

#### index.m3u8
```
#EXTM3U
#EXT-X-VERSION:3
#EXT-X-TARGETDURATION:5
#EXT-X-MEDIA-SEQUENCE:1
#EXT-X-PLAYLIST-TYPE:EVENT
#EXTINF:5.0,
http://localhost:8008/seg/seg1.ts
#EXTINF:5.0,
http://localhost:8008/seg/seg2.ts
#EXTINF:5.0,
http://localhost:8008/seg/seg3.ts
```

#### web.config
```
<?xml version="1.0" encoding="UTF-8"?>
<configuration>
    <system.webServer>
        <staticContent>
            <remove fileExtension=".ts" />
            <mimeMap fileExtension=".m3u8" mimeType="application/vnd.apple.mpegurl" />
            <mimeMap fileExtension=".ts" mimeType="video/mp2t" />
        </staticContent>
    </system.webServer>
</configuration>
```
