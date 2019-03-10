# unity-reference-viewer

[**English**](README_EN.md)

Unity上でアセットの参照を検索し、ウィンドウ上に表示するツールです。  
(例：テクスチャを使用しているマテリアルを調べる)  
  
Unity標準の「ref:」はプロジェクト規模が大きいとUnityがクラッシュすることがあり、そのような場合に有効です。  
対象アセットのGUIDがファイル内容に含まれているかどうかをUnity上ではなくOSの機能で検索しているのが特徴です。  
  
環境にもよりますが比較的高速に動作するため、結果のキャッシュなどは行っていません。

## 使い方

対象のアセットもしくはフォルダを右クリックして「Find References In Project」を選択

## 除外設定について

「ExcludeSettings.asset」に検索結果から除外したいファイルを指定できます。  
バイナリファイルなどを指定しておくことで検索の正確性向上、速度向上が見込めます。

## Mac版のSpotlightとGrepの違いについて

SpotlightはMacで使われている標準の検索機能です。  
非常に高速に動作しますが、ファイルのインデックスが正しく作成されていないと検索に失敗します。  
正確に検索する必要がある場合はGrep版を使用して下さい。  
参考：[**Mac で Spotlight のインデックスを再作成する方法**](https://support.apple.com/ja-jp/HT201716)

## Windows版について

当初はMacしか使用していなかったため、Windows版が必要になった時に急ぎ作成しました。  
検索方法について精査していないので、もっと高速な検索が可能かもしれません。

## ライセンス条項

使用に際して制限はありません。  
再配布等については、MITライセンスに準拠して下さい。  
https://opensource.org/licenses/mit-license.php  

Copyright (c) 2019 ina-amagami (ina.amagami@gmail.com)