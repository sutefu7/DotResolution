using DotResolution.Libraries;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace DotResolution.Views.Controls
{
    /// <summary>
    /// 色分けに対応した TextBlock です。
    /// </summary>
    public class ColorTextBlock : TextBlock
    {
        #region コンストラクタ

        // TreeView の ItemTemplate 内で TextBlock を使っているところまでは問題ないのだが、Inlines に Run クラスのオブジェクトをセットしている場合、
        // TreeView のノードをクリックした際、Run 型から Visual 型にキャストできない旨の、例外エラーが発生してしまうバグの対応
        // 
        // https://stackoverflow.com/questions/38325162/why-click-tree-throws-system-windows-documents-run-is-not-a-visual-or-visual3d
        // 
        // 上記より、
        // IsHitTestVisible に false をセットして、クリック反応の対象外にしてしまう

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        public ColorTextBlock()
        {
            IsHitTestVisible = false;
        }

        #endregion

        #region ColorTextProperty 依存関係プロパティ

        /// <summary>
        /// セットした文字列に対して、キーワード定義別の色に変えて表示します。
        /// </summary>
        public static readonly DependencyProperty ColorTextProperty =
            DependencyProperty.Register(
                "ColorText",
                typeof(string),
                typeof(ColorTextBlock),
                new PropertyMetadata(string.Empty, OnColorTextPropertyChanged));

        public string ColorText
        {
            get => (string)GetValue(ColorTextProperty);
            set => SetValue(ColorTextProperty, value);
        }

        private static void OnColorTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = d as ColorTextBlock;
            if (target == null)
                return;

            var s = e.NewValue as string;
            if (string.IsNullOrWhiteSpace(s))
                return;

            if (s.Contains("(") || s.Contains(":") || s.Contains(" As "))
                ConvertStringToInlines(target, s);
            else
                target.Text = s;
        }

        private static void ConvertStringToInlines(ColorTextBlock target, string s)
        {
            if (target.Inlines.Any())
                target.Inlines.Clear();

            var items = ConvertStringToInlines(s);
            foreach (var item in items)
                target.Inlines.Add(item);
        }

        private static IEnumerable<Run> ConvertStringToInlines(string value)
        {
            var tokens = ConvertToken(value);
            var firstIdentifier = true;
            var firstParenOffset = -1;
            var lastParenOffset = -1;

            // カッコがある場合、引数を見やすくするため、スペースを追加する
            // ただし、引数がある場合だけに制限する
            if (value.Contains("("))
            {
                // 戻り値がある場合かつ、戻り値が配列の場合、誤判定防止のため削る
                var tmp = value;

                // C#
                if (tmp.Contains(" : "))
                    tmp = tmp.Substring(0, tmp.LastIndexOf(" : "));

                // VBNet
                // シグネチャとして表示させるため、引数名は消去している。つまり、
                // Func(Integer, String) As Integer と、As が付く場合は戻り値だけ
                if (tmp.Contains(" As "))
                    tmp = tmp.Substring(0, tmp.LastIndexOf(" As "));

                // 引数無しを置換してもカッコがある場合、引数があるということ
                if (tmp.Replace("()", string.Empty).Contains("("))
                {
                    // VBNet
                    // Func1(Integer)
                    // Func1(Dictionary(Of Integer, String()))
                    // Names(,,,)
                    firstParenOffset = tmp.IndexOf("(");
                    lastParenOffset = tmp.LastIndexOf(")");
                }
            }

            var previousToken = default(Token);

            foreach (var token in tokens)
            {
                switch (token.TokenType)
                {
                    case TokenTypes.Comma:

                        // 後置スペース
                        // 引数カンマは後置スペースを入れたいが、
                        // 2次元配列などの場合は入れたくない
                        // Func(int i, string s)
                        // string[,] items
                        // → TokenKinds.Identifier 側で対応する
                        yield return new Run { Foreground = Brushes.Black, Text = $"{token.Value}" };
                        break;

                    case TokenTypes.Coron:

                        // 前後置スペース
                        yield return new Run { Foreground = Brushes.Black, Text = $" {token.Value} " };
                        break;

                    case TokenTypes.Parentheses:

                        if (token.StartOffset == firstParenOffset)
                        {
                            yield return new Run { Foreground = Brushes.Black, Text = $"{token.Value} " };
                        }
                        else if (token.StartOffset == lastParenOffset)
                        {
                            yield return new Run { Foreground = Brushes.Black, Text = $" {token.Value}" };
                        }
                        else
                        {
                            yield return new Run { Foreground = Brushes.Black, Text = $"{token.Value}" };
                        }

                        break;

                    case TokenTypes.Identifier:

                        // 1つ前のトークンがカンマの場合、前置スペースを追加する
                        // 引数に属性がついている場合は、後置スペースを追加する
                        var frontSpace = string.Empty;
                        var rearSpace = string.Empty;

                        if (previousToken?.TokenType == TokenTypes.Comma)
                            frontSpace = " ";

                        if (firstIdentifier)
                        {
                            if (IsSubKeyword(token.Value))
                            {
                                // C#
                                // operator +(Class1 x, Class1 y)
                                yield return new Run { Foreground = Brushes.Blue, Text = $"{token.Value} " };
                            }
                            else
                            {
                                // 定義名
                                yield return new Run { Foreground = Brushes.Black, Text = $"{token.Value}" };
                                firstIdentifier = false;
                            }
                        }
                        else
                        {
                            // キーワードと定義で色分けする
                            var b1 = Brushes.LightSeaGreen;

                            if (IsKeyword(token.Value))
                                b1 = Brushes.Blue;

                            if (IsSubKeyword(token.Value))
                            {
                                b1 = Brushes.Blue;
                                rearSpace = " ";

                                // As キーワードの場合、前後スペース
                                if (token.Value == "As")
                                    frontSpace = " ";
                            }

                            yield return new Run { Foreground = b1, Text = $"{frontSpace}{token.Value}{rearSpace}" };
                        }

                        break;
                }

                previousToken = token;
            }
        }



        //

        private enum TokenTypes
        {
            // 文字列（キーワード、定義名は共通）
            Identifier,

            // カッコ（()<>{}[]）
            Parentheses,

            // ,
            Comma,

            // :
            Coron,
        }

        private class Token
        {
            public int StartOffset { get; set; }

            public string Value { get; set; }

            public TokenTypes TokenType { get; set; }
        }

        private static IEnumerable<Token> ConvertToken(string value)
        {
            // C#, Visual Basic 共通

            var position = -1;
            var buffer = new StringBuilder();
            var keywords = new List<char> { '(', ')', '<', '>', '[', ']', ',', ':', ' ' };

            for (var i = 0; i < value.Length; i++)
            {
                //
                var currentChar = value[i];
                var nextChar = '\0';
                var thirdChar = '\0';

                if (i + 1 < value.Length) nextChar = value[i + 1];
                if (i + 2 < value.Length) thirdChar = value[i + 2];

                position++;


                //
                switch (currentChar)
                {
                    case '(':
                    case ')':
                    case '<':
                    case '>':
                    case '[':
                    case ']':

                        yield return new Token { StartOffset = position, TokenType = TokenTypes.Parentheses, Value = $"{currentChar}" };
                        break;

                    case ',':

                        yield return new Token { StartOffset = position, TokenType = TokenTypes.Comma, Value = $"{currentChar}" };
                        break;

                    case ':':

                        yield return new Token { StartOffset = position, TokenType = TokenTypes.Coron, Value = $"{currentChar}" };
                        break;

                    case ' ':
                        break;

                    default:

                        buffer.Append(currentChar);

                        if (keywords.Contains(nextChar))
                        {
                            yield return new Token { StartOffset = position, TokenType = TokenTypes.Identifier, Value = $"{buffer}" };
                            buffer.Clear();
                        }

                        break;
                }
            }

            if (buffer.Length != 0)
            {
                yield return new Token { StartOffset = position, TokenType = TokenTypes.Identifier, Value = $"{buffer}" };
                buffer.Clear();
            }
        }

        private static bool IsKeyword(string value)
        {
            var keywords = new List<string>();

            switch (AppEnv.LanguageType)
            {
                case LanguageTypes.CSharp:

                    keywords.AddRange(new string[] { "bool", "byte", "sbyte", "char", "decimal", "double", "float", "int", "uint", "long", "ulong", "short", "ushort", "string", "object", "DateTime", "void" });
                    if (keywords.Contains(value))
                        return true;

                    break;

                case LanguageTypes.VisualBasic:

                    keywords.AddRange(new string[] { "Boolean", "Byte", "SByte", "Char", "Decimal", "Double", "Single", "Integer", "UInteger", "Long", "ULong", "Short", "UShort", "String", "Object", "Date", "Void" });
                    if (keywords.Contains(value))
                        return true;

                    break;
            }

            return false;
        }

        private static bool IsSubKeyword(string value)
        {
            var keywords = new List<string>();

            switch (AppEnv.LanguageType)
            {
                case LanguageTypes.CSharp:

                    keywords.AddRange(new string[] { "ref", "in", "out", "params", "operator" });
                    if (keywords.Contains(value))
                        return true;

                    break;

                case LanguageTypes.VisualBasic:

                    keywords.AddRange(new string[] { "ByRef", "ParamArray", "Of", "As", "Default" });
                    if (keywords.Contains(value))
                        return true;

                    break;
            }

            return false;
        }

        #endregion

        #region ChangeSelectedForegroundColor 依存関係プロパティ

        /// <summary>
        /// 選択中の場合の文字列色を白色に変えます。
        /// </summary>
        public static readonly DependencyProperty ChangeSelectedForegroundColorProperty =
            DependencyProperty.Register(
                "ChangeSelectedForegroundColor",
                typeof(bool),
                typeof(ColorTextBlock),
                new PropertyMetadata(false, OnChangeSelectedForegroundColorPropertyChanged));

        public bool ChangeSelectedForegroundColor
        {
            get => (bool)GetValue(ChangeSelectedForegroundColorProperty);
            set => SetValue(ChangeSelectedForegroundColorProperty, value);
        }

        private static void OnChangeSelectedForegroundColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = d as ColorTextBlock;
            if (target == null)
                return;

            if (e.NewValue == null)
                return;

            var isTrue = (bool)e.NewValue;
            if (isTrue)
            {
                // 全部白色に変更（選択状態のため、背景色がブルーになっている）
                foreach (var inline in target.Inlines)
                    inline.Foreground = Brushes.White;
            }
            else
            {
                // 
                var s = string.Empty;
                foreach (var inline in target.Inlines)
                    s += (inline as Run).Text;

                var args = new DependencyPropertyChangedEventArgs(ColorTextProperty, null, s);
                OnColorTextPropertyChanged(d, args);
            }
        }


        #endregion

    }
}
