# uGUIFrameWork

### ライブラリについて
UI用のコードを自動生成をして楽をするためのライブラリです

### 何が出来る？
MV(R)PパターンでUIを作る際、手続き的なコードになりがちなModelの型定義、Model⇔Viewのデータバインド処理のコードを自動生成します。
これによって人が書く部分はMV(R)PのP(プレゼンター)部分のみで良くなります。
また、Uiパーツに同規格のInterfaceを用いることが出来ます。

### 仕組み
UIをUnity上で組み立てる際、スクリプトから変更可能にしたい個所に本フレームワークのComponentをアタッチし、メニューから生成コマンドを選択することで配下のUIパーツの参照を集め自動でコードが生成されます。

### 備考
急ぎのサンプルコードなのでOdinや自プロジェクトを参照してる部分があり、そのままプロジェクトに入れても動作はしません。
