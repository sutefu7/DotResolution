# DotResolution
DotResolution は、C# / Visual Basic のソースコードを理解するための補助ツールです。３作目になります。

ファイルメニュー → ソリューションファイルの選択メニューからダイアログで指定するか、調べたいソリューションファイルをドラッグアンドドロップして、本プログラムに読み込ませます。

![DotResolution 使用例](https://github.com/sutefu7/DotResolution/blob/main/Docs/Images/01-solution-load.gif "ソリューションファイルを読み込む")

最初に、このプロジェクト一式の関係性を知ることから始めます。具体的には、各プロジェクトのプロジェクト参照を再帰的に見ることで、メインとサブの関係が分かります。

![DotResolution 使用例](https://github.com/sutefu7/DotResolution/blob/main/Docs/Images/02-solution-tree-watch.gif "プロジェクト間の参照関係図を見る")

次に見るのはソースコードです。ソースコードを読んでもよくわからんの場合は、Visual Studio などでデバッグしてステップ実行して、ソースコード内を歩き回ること、生きたデータのライフサイクルを見ること、だと思います。ソースコードを文字列のかたまりではなく、ドラマの登場人物や役割のようなイメージができれば、分かってくると思います。

Visual Studio ではない本プログラムで何ができるか、何を見たいか考えた結果、条件分岐やループレベルのフローチャートを表示する、継承関係図を表示することでした。
ただし、命令をフローチャートで表示しても、プログラミング言語の扱い方を知ることであり、処理内容（＝仕様）を知ることにはつながらないと思いましたので却下として、継承関係図のみを実装しました。継承関係図は Visual Studio のクラス図に似せています。

定義一覧ツリーのうち、クラス・構造体・インターフェースのノードをクリックすることで、継承関係図が表示されます。フィールドやプロパティなどのメンバーノードをクリックすると消えます。継承元ツリーは継承元を再帰的にたどり、継承先ツリーは継承先を再帰的にたどります。継承ツリーは両方です。継承関係を見ることで、クラスの関係性が分かります。

各図形をクリックすると、画面下部にある詳細情報（コメント付きバージョン）が表示されます。背景をクリックすると消えます。

![DotResolution 使用例](https://github.com/sutefu7/DotResolution/blob/main/Docs/Images/03-source-tree-watch.gif "ソースコードを見る")

表示された図形のうち、リンクが表示された図形は別のソースファイルで定義されたものです。同じプロジェクト内のソースファイルか、別のプロジェクトのソースファイルです。リンクをクリックすることでその定義位置を表示します。ソースコード上で右クリックからのコンテキストメニュー「定義へ移動」をクリックすることでも定義位置を表示します。「ソースコードを整形して表示」をクリックすると、最低限の C# / Visual Basic 基準フォーマット形式で整形したソースコードビューアを別タブで表示します。

![DotResolution 使用例](https://github.com/sutefu7/DotResolution/blob/main/Docs/Images/04-source-tree-watch.gif "リンククリック、または定義へ移動でダイレクトジャンプ")

継承関係が大きい場合はマウスホイールを回すことで表示倍率を拡大・縮小することができます。倍率を変えると図形を見失いますが、画面右下にあるサムネイル ナビゲーター内をドラッグ移動して、図形がある位置に移動してください。

![DotResolution 使用例](https://github.com/sutefu7/DotResolution/blob/main/Docs/Images/05-source-tree-expand.gif "表示倍率の拡大・縮小")

4K モニターをお使いの場合などで、本プログラムの文字が小さい場合は、アプリケーション自体の表示倍率の拡大・縮小をおこないます。実行ファイルと同じ場所に自動生成される「DotResolution.cfg.txt」を開いて、「Magnification=1.0」の値を 1.5 などに書き換えてください。double 型で 0.0 ～ X.X 倍の拡大率（縮小率）という意味になります。

![DotResolution 使用例](https://github.com/sutefu7/DotResolution/blob/main/Docs/Images/06-application-expand.gif "アプリケーションの表示倍率の拡大・縮小")

# 想定する使い方

コンソールアプリケーションやデスクトップアプリケーションを想定しています。ほぼ Visual Studio と同じ機能です。ただし、本プログラムではソースコードを理解する際、文字列を読み進めて関係性を把握するよりも、グラフィカルな図形を見て把握する方が、理解するスピードが早いのではないかと考えています。よって、メインは Visual Studio で開いて読み進める一方で、本プログラムも横に並べて補助的に見るのがベストだと考えます。

# 開発環境＆動作環境

本プログラムは、以下の環境で作成＆動作確認をおこなっています。

| 項目 | 値                                                               |
| ----- |:---------------------------------------------------- |
| OS   | Windows 11 Pro (64 bit)                              |
| IDE  | Visual Studio Community 2022                     |
| 言語 | C#                                                       |
| 種類 | WPF アプリケーション (.NET Framework 4.8)、コードビハインド方式 |

# 動作対象ソースコード

.NET Framework 1.0 ～ .NET Framework 4.8 までをターゲットに作られたソースコード一式です。ビルドしていなくてもソースコードがあれば表示することができます。

# 未対応部分（この先も対応しないと思っているもの）

- C# プロジェクトと Visual Basic プロジェクトが混在したソリューション構成の場合は未対応です。
- .NET Core 以降（のプロジェクト記載方式や、言語仕様の新機能、追加、修正分）は未対応です。

# 利用ライブラリ

各ライブラリは、ライブラリ作成者様のライセンスに帰属します。
詳しくは、ライブラリ作成者様のホームページ、または GitHub をご参照ください。

- ドッキングウィンドウコントロール

   Dirkster.AvalonDock
   
   Dirkster.AvalonDock.Theme.Aero　※各テーマは NuGet 参照しているけど未使用
   
   Dirkster.AvalonDock.Theme.Expression
   
   Dirkster.AvalonDock.Theme.Metro
   
   Dirkster.AvalonDock.Theme.VS2010
   
   Dirkster.AvalonDock.Theme.VS2013

- C# / Visual Basic のソースコードを表示するためのエディター

   AvalonEdit

- ソースコードを読み込む際、エンコードを自動判定するためのライブラリ

   ReadJEnc
   
   主に、SyntaxWalker する際に利用

- C# / Visual Basic のソースコードを解析するためのライブラリ

   Microsoft.CodeAnalysis.CSharp
   
   Microsoft.CodeAnalysis.CSharp.Workspaces
   
   Microsoft.CodeAnalysis.VisualBasic
   
   Microsoft.CodeAnalysis.VisualBasic.Workspaces
   
   Microsoft.CodeAnalysis.Workspaces.MSBuild

- Roslyn API を使うために追加参照

   Microsoft.CodeAnalysis.MSBuild.MSBuildWorkspace.Create() を呼び出すためには、ローカルPC参照の拡張参照で、以下２つの参照追加が必要
   
   Microsoft.Extensions.Logging
   
   Microsoft.Extensions.Options

- アイコン

   Visual Studio 2013 Image Library

# 参考サイト

- サムネイル的なナビゲーター

   Grabacr07 様
   WPF の Thumb コントロールで Photoshop のナビゲーターを再現するやつ
   https://gist.github.com/Grabacr07/988bc04fb7f16aaa4fdc












