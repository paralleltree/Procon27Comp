# Procon27Comp

第28回高専プロコンで作成したソルバです。
名前が27になってるのは察してください。

## ビルド
ビルドはx64構成で行ってください。
本番用はReleaseビルドです。

## 使い方
改行区切りでQRコードによる形状情報を記録したファイルをコマンドライン引数に指定して実行します。

## 試合結果
|試合|問題|使用データ|順位|
|:--:|:--:|:--------:|:--:|
|1回戦第4試合|角島灯台|配置情報(LEVEL4)|第5位|
|準決勝第1試合|とらふく|形状情報|第6位|
|決勝|鯛|形状情報|第8位|

## 使用ライブラリ
  * [General Polygon Clipper library](http://www.cs.man.ac.uk/~toby/gpc/)
  * [PriorityQueue](https://www.nuget.org/packages/PriorityQueue/)
  * [System.Numerics.Vectors](https://www.nuget.org/packages/System.Numerics.Vectors/)
  * [Vertesaur.Core](https://www.nuget.org/packages/Vertesaur.Core)
