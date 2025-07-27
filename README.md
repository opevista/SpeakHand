# SpeakHand

Windows用のシンプル＆モダンなオーディオデバイス切り替えツール（WPF/.NET 9.0）

## 特徴

- ワンクリックで既定のスピーカー・ヘッドホンを切り替え
- お気に入りデバイス登録＆パネル表示
- アニメーションGIFによる楽しいUI
- 設定はJSONで自動保存
- アイコン・GIFも同梱、自己完結型exeで配布可能
- NAudio, WpfAnimatedGif使用

## スクリーンショット

![screenshot](screenshot.png)  

## 使い方

1. exeを起動
2. デバイス一覧から切り替えたいスピーカー/ヘッドホンを選択
3. 「お気に入り」に追加するとパネルに表示
4. GIFアニメで状態が分かりやすい

## ビルド方法

1. .NET 9.0 SDKをインストール
2. リポジトリをクローン
3. `dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true`
4. `bin/Release/net9.0-windows/win-x64/publish/` にexeが生成されます

## 依存ライブラリ

- [NAudio](https://github.com/naudio/NAudio)
- [WpfAnimatedGif](https://github.com/thomaslevesque/WpfAnimatedGif)
- System.Text.Json (.NET標準)

## ライセンス

MIT License
