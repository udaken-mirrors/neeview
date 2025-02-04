## Q&A

(2019-07-13)


- - - 
### :pushpin: Q. 起動が遅い

A. .NET Framework で作成しているため、ある程度のマシンスペックを必要とし、初回起動が遅い傾向があります。  
  
- - - 
### :pushpin: Q. 起動しなくなってしまった

A1. PCを再起動してからお試しください。  
以前のプロセスがなんらかの原因で残っていると多重起動制御のために起動できなくなってしまうことがあるためです。タスクマネージャーでそのプロセスを終了させるか、PCを再起動することで解消されます。

A2. ユーザー設定ファイルを削除してからの起動を試してください。  
ユーザー設定ファイル `UserSetting.json` を消してから起動すると初期設定で起動されます。
Zip版では、ユーザ設定ファイルは実行ファイルと同じ場所にあります。

- - -
### :pushpin: Q. メモリの使用量を少なくしたい

A. メモリを最も使用するのはメモリ上に展開される画像データなので、これを少なくする設定にします。設定は `設定ウィンドウ` の `メモリ＆パフォーマンス` にまとめられています。

- - -
### :pushpin: Q. 「次のブック」を選ぶ基準を変更したい

A. ブックの移動は本棚の並び順に依存しています。

本棚で並び順を変更してお試しください。

- - -
### :pushpin: Q. とても大きな画像を表示しようとすると「プログラムの実行を続行するための十分なメモリがありませんでした。」と表示され、画像表示できない

A. .NETであつかえる最大画像サイズを超えている可能性があります。

`設定ウィンドウ` の `ブック全般` > `最大画像サイズ` を 4096 x 4096 のように適当な値に設定し、さらに `読み込み画像サイズ制限` をONにしてメモリ上に展開する画像サイズを制限することで読み込めるようになる可能性があります。

- - -
### :pushpin: Q. サブフォルダーもまとめてブックとして開きたい

A. サブフォルダーを読み込む方法や設定は複数あります。

* 設定「ページ設定」の「サブフォルダーを読み込む」を設定する。
  本を開いたときの動作を設定します。

* メニュー「ページ > サブフォルダーを読み込む」を選択する。
  そのブックでのサブフォルダー読み込みをON/OFFします。

* 本棚での右クリックメニューで「サブフォルダーを読み込む」を選択する。
  そのブックでのサブフォルダー読み込みをON/OFFします。

* 本棚の「…」メニューの「この場所ではサブフォルダーを読み込む」  
  この設定をした場所にあるフォルダーは既定のページ設定にかかわらずサブフォルダーを読み込むようになります。

* サブフォルダーが１つだけの場合に自動でサブフォルダーも読み込むかの設定は、設定「ブック > サブフォルダー」で設定します。

- - -
### :pushpin: Q. タッチ位置で前ページに戻る、次ページに進むを設定できますが、クリック位置でも同様の設定ができるようになりませんか？

A. 「タッチエミュレート」というコマンドがあります。コマンドリストの最後のほうにあります。
このコマンドはタッチ操作をカーソル位置で実行するコマンドですので、このコマンドにクリック操作を設定することでご実現可能です。

- - -
### :pushpin: Q. フルスクリーンから「Esc」などのショートカットキーで一気に終了させたい

A. 「アプリを終了する」というコマンドがあります。このコマンドのショートカットにEscキーを割り当てることで実現可能です。

- - -
### :pushpin: Q. オリジナルサイズ表示ともとの表示設定への切り替えを１操作で行いたい

A. 「コマンド設定」で「大きい場合ウィンドウサイズに合わせる」コマンドのパラメータ設定で「オリジナルサイズとの切り替え」というスイッチがあるのでこれをONにしてお試しください。この設定は各サイズ切り替えコマンド共通の設定で、オリジナルサイズとの切り替え動作になります。  
別の方法として、「表示サイズを切り替える」コマンドのパラメータ設定で切り替えるモードを制限することで同じような動作になります。


- - - 
### :pushpin: Q. コマンドにマウスの左右同時押しを設定しているが、反応が悪い

A. 左右同時押しは以下の２つの入力のどちらかになります。

* LeftButton+RightClick「左ボタンを押しながら右クリック」
* RightButton+LeftClick「右ボタンを押しながら左クリック」

同時押し後にどちらのボタンが先に離されるかでどちらになるかが決まります。同時押しの感覚ですとこのあたりがばらつくため反応しないことがあるのだと思われます。  
対策としては、上記二つの入力を同じコマンドに割り当てることで反応が良くなると思いますのでお試しください。


- - -
### :pushpin: Q. マウスホイール操作をコマンドに割り当てたい

A. コマンド設定で「ショートカット設定」「マウス入力」の「ここでマウス入力」を選択肢、ホイール操作することで入力できます。

- - -
### :pushpin: Q. 特定のZIPファイルが開けない

A. 標準のZIP展開処理が対応していない可能性があります(Deflate64で圧縮されたZIP等)。`詳細設定`の `ZIP`>`標準機能によるZIP圧縮ファイル展開を使用する`をOFFにすることで7z.dllを使ってZIP展開を行うようになるため、これで開けるようになる可能性があります。

- - -
### :pushpin: Q. UNICODE文字を使用しているファイルをSusieプラグインで開けない

A. ファイルシステムに8.3形式のショートネームが保存されている必要があります。  
保存されているか確認するには、コマンドラインで `dir /x` を実行してください。
設定を変更するにはは管理者権限が必要です。 詳細は `fsutil` `8dot3name` をキーワードに検索してください。
レジストリの変更等、OSのシステムを変更してしまうので、よくわからない場合は設定しないほうが安全です。

- - -
### :pushpin: Q. 再生できない動画がある

A. .NETの機能で再生しているため、それに依存しています。ご了承ください。  
目安としては、 Windows Media Player で再生可能な動画がおおよそ対応動画となります。

