# DotResolution
DotResolution は、C# / Visual Basic のソースコードを理解するための補助ツールです。３作目になります。


![DotResolution 使用例](https://github.com/sutefu7/DotResolution/blob/main/Docs/Images/image-01.png "プロジェクト間の参照関係を表示")

![DotResolution 使用例](https://github.com/sutefu7/DotResolution/blob/main/Docs/Images/image-02.png "ソースコードを表示")

![DotResolution 使用例](https://github.com/sutefu7/DotResolution/blob/main/Docs/Images/image-03.png "あるクラスの継承元関係を表示")

![DotResolution 使用例](https://github.com/sutefu7/DotResolution/blob/main/Docs/Images/image-04.png "あるクラスの継承先関係を表示")

![DotResolution 使用例](https://github.com/sutefu7/DotResolution/blob/main/Docs/Images/image-05.png "定義へ移動")

![DotResolution 使用例](https://github.com/sutefu7/DotResolution/blob/main/Docs/Images/image-06.png "定義元があれば切り替わります。")

# 機能

- ファイル → ソリューションファイルの選択メニューから指定するか、ソリューションエクスプローラーへソリューションファイルをドラッグアンドドロップすることで分析が始まります。
- ソリューションノードのクリックで、全てのプロジェクト間の参照関係を一覧表示
- プロジェクトノードのクリックで、プロジェクト間の参照関係を表示
- ソースファイルノードのクリックで、定義一覧ツリー、ソースコードビューア、継承元ツリー、継承先ツリー、を表示
  - 定義一覧ツリーの各ノードをクリックで、ソースコードビューアの各定義位置へ表示移動
    - この時、ノードの種類が Class / Struct / Interface の場合、継承元ツリー、継承先ツリー、を表示更新
    - 継承元ツリーはクラス図のような見た目で、ソースファイル間・クラス間の関係性を知ることができます。リンクのソースファイル名をクリックすることで、新しいタブでそのソース情報を確認することができます。見慣れない時期は、行ったり来たりして何度も見ることが大切です。継承先ツリーも同様です。
  - ソースコードビューアでは、以下の２つの機能があります。
    - 調べたい位置にキャレットを置いて、右クリックからのコンテキストメニュー「定義へ移動」をクリックすると、その定義している場所へダイレクトジャンプすることができます。
    - 「ソースコードを整形して表示」をクリックすると、最低限の C# 基準フォーマット形式で整形したソースコードビューアを別タブで表示します。


# 開発環境＆動作環境

本プログラムは、以下の環境で作成＆動作確認をおこなっています。

| 項目 | 値                                                               |
| ----- |:---------------------------------------------------- |
| OS   | Windows 11 Pro (64 bit)                              |
| IDE  | Visual Studio Community 2022                     |
| 言語 | C#                                                       |
| 種類 | WPF アプリケーション (.NET Framework 4.8) |

# 動作対象ソースコード

.NET Framework 1.0 ～ .NET Framework 4.8 までをターゲットに作られたソースコード一式です。.NET Core 以降は未対応です。


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

- C#/VBのソースコードを表示するためのエディター

   AvalonEdit

- ソースコードを読み込む際、エンコードを自動判定するためのライブラリ

   ReadJEnc
   主に、SyntaxWalker する際に使う

- C#/VB のソースコードを解析するためのライブラリ

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
















