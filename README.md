# unity-reference-viewer

![unity-reference-viewer](https://amagamina.jp/blog/wp-content/uploads/2019/07/how-to-1.gif)

[**English**](README_EN.md)

Unity上でアセットの参照を検索し、ウィンドウ上に表示するツールです。  
(例：テクスチャを使用しているマテリアルを調べる)    
  
本パッケージをAssets以下の任意の場所に配置するだけで動作します。
  
詳しい解説は[**こちら**](https://amagamina.jp/blog/reference-viewer/)

## UPMインストール

upm経由でインストールする場合は `https://github.com/ina-amagami/unity-reference-viewer.git` を指定して下さい。  
  
`Packages/manifest.json` のdependencies内に追加してもインストールできます。
```json
{
  "dependencies": {
    "jp.amagamina.reference-viewer": "https://github.com/ina-amagami/unity-reference-viewer.git"
  }
}
```

## ライセンス条項

MITライセンス
https://opensource.org/licenses/mit-license.php  

コード内のライセンス表記を残して頂ければ自由に使用可能です。

Copyright (c) 2019-2021 ina-amagami (ina@amagamina.jp)
