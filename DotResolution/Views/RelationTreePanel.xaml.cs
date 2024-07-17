using DotResolution.Data;
using DotResolution.Libraries;
using DotResolution.Views.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace DotResolution.Views
{
    /// <summary>
    /// ２つ以上のデータの相関図をツリー表示する画面です。
    /// </summary>
    public partial class RelationTreePanel : UserControl
    {
        private Point mouseDownPoint = new Point(0, 0);

        /// <summary>
        /// コメント付きツリーを表示するかどうかを取得、または設定します。
        /// </summary>
        public bool IsDisplayTreeListView { get; set; } = false;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        public RelationTreePanel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// マウスの左ボタンを押した時のイベントです。
        /// </summary>
        /// <remarks>
        /// TreeListView へのデータ表示更新をおこないます。<br></br>
        /// MouseLeftButtonDown の場合、Canvas はイベント補足できましたが、Thumb ができませんでした。<br></br>
        /// よって Thumb もイベント補足できるプレビューイベントの方で対応します。
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas1_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsDisplayTreeListView)
            {
                if (e.Source is Canvas)
                    treeListView1.Items.Clear();

                if (e.Source is Thumb thumb)
                    ShowTreeListViewData(thumb);
            }
        }

        /// <summary>
        /// マウスの左ボタンを押した時のイベントです。
        /// </summary>
        /// <remarks>
        /// MouseLeftButtonDown, MouseMove, MouseLeftButtonUp の組み合わせにより、Canvas をドラッグ移動で、表示範囲を移動させます。
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!(e.Source is Canvas))
                return;

            mouseDownPoint = e.GetPosition(this);
            canvas1.CaptureMouse();
        }

        /// <summary>
        /// マウスを移動した時のイベントです。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!(e.Source is Canvas))
                return;

            if (mouseDownPoint == new Point(0, 0))
                return;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                canvas1.Cursor = Cursors.ScrollAll;

                var mouseCurrentPoint = e.GetPosition(this);
                var dx = mouseDownPoint.X - mouseCurrentPoint.X;
                var dy = mouseDownPoint.Y - mouseCurrentPoint.Y;

                dx += scrollViewer1.HorizontalOffset;
                dy += scrollViewer1.VerticalOffset;

                scrollViewer1.ScrollToHorizontalOffset(dx);
                scrollViewer1.ScrollToVerticalOffset(dy);

                mouseDownPoint = mouseCurrentPoint;
            }
        }

        /// <summary>
        /// マウスの左ボタンを離した時のイベントです。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!(e.Source is Canvas))
                return;

            canvas1.Cursor = Cursors.Arrow;
            canvas1.ReleaseMouseCapture();
            mouseDownPoint = new Point(0, 0);
        }

        /// <summary>
        /// マウスホイールを回した時のイベントです。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas1_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var form = canvas1.RenderTransform as ScaleTransform;
            var scaleFactor = 1.1;

            if (e.Delta > 0)
            {
                form.ScaleX *= scaleFactor;
                form.ScaleY *= scaleFactor;
            }
            else
            {
                form.ScaleX /= scaleFactor;
                form.ScaleY /= scaleFactor;
            }
        }

        private void ShowTreeListViewData(Thumb target)
        {
            treeListView1.Items.Clear();

            // コンテナ系
            var model = target.DataContext as DefinitionHeaderModel;
            var root = model.ReferenceModel;
            var rootItem = new TreeListViewItem
            {
                Header = new TreeListViewItemHeaderModel { DefinitionType = root.DefinitionType, MemberName = root.Text, Comment = root.Comment },
                IsExpanded = true,
                Margin = new Thickness(0, 10, 0, 0),
            };
            treeListView1.Items.Add(rootItem);

            // Enum
            var definitionType = DefinitionTypes.Enum;
            var headerText = "列挙体";
            AddChildrenMembers(rootItem, root, definitionType, headerText);

            // Delegate
            definitionType = DefinitionTypes.Delegate;
            headerText = "デリゲート";
            AddChildrenMembers(rootItem, root, definitionType, headerText);

            // Event
            definitionType = DefinitionTypes.Event;
            headerText = "イベント";
            AddChildrenMembers(rootItem, root, definitionType, headerText);

            // Field
            definitionType = DefinitionTypes.Field;
            headerText = "フィールド";
            AddChildrenMembers(rootItem, root, definitionType, headerText);

            // Indexer
            definitionType = DefinitionTypes.Indexer;
            headerText = "インデクサー";
            AddChildrenMembers(rootItem, root, definitionType, headerText);

            // Property
            definitionType = DefinitionTypes.Property;
            headerText = "プロパティ";
            AddChildrenMembers(rootItem, root, definitionType, headerText);

            // Constructor
            definitionType = DefinitionTypes.Constructor;
            headerText = "コンストラクター";
            AddChildrenMembers(rootItem, root, definitionType, headerText);

            // Operator
            definitionType = DefinitionTypes.Operator;
            headerText = "オペレーター";
            AddChildrenMembers(rootItem, root, definitionType, headerText);

            // WindowsAPI
            definitionType = DefinitionTypes.WindowsAPI;
            headerText = "Windows API";
            AddChildrenMembers(rootItem, root, definitionType, headerText);

            // EventHandler
            definitionType = DefinitionTypes.EventHandler;
            headerText = "イベントハンドラー";
            AddChildrenMembers(rootItem, root, definitionType, headerText);

            // Method
            definitionType = DefinitionTypes.Method;
            headerText = "メソッド";
            AddChildrenMembers(rootItem, root, definitionType, headerText);
        }

        private void AddChildrenMembers(TreeListViewItem item, TreeViewItemModel referenceModel, DefinitionTypes definitionType, string headerText)
        {
            if (referenceModel.Children.Any(x => x.DefinitionType == definitionType))
            {
                var children = referenceModel.Children
                    .Where(x => x.DefinitionType == definitionType)
                    .OrderBy(x => x.Text);          // 名前順
                                                    //.OrderBy(x => x.StartOffset); // 定義順

                var headerItem = new TreeListViewItem
                {
                    Header = new TreeListViewItemHeaderModel { DefinitionType = definitionType, MemberName = headerText },
                    IsExpanded = true,
                    Margin = new Thickness(0, 10, 0, 0),
                };
                item.Items.Add(headerItem);

                foreach (var child in children)
                {
                    var memberName = child.Text;
                    var typeName = string.Empty;

                    if (memberName.Contains(" : "))
                    {
                        var splits = memberName.Split(new string[] { " : " }, StringSplitOptions.RemoveEmptyEntries);
                        memberName = splits[0].Trim();
                        typeName = splits[1].Trim();
                    }

                    if (memberName.Contains(" As "))
                    {
                        var splits = memberName.Split(new string[] { " As " }, StringSplitOptions.RemoveEmptyEntries);
                        memberName = splits[0].Trim();
                        typeName = splits[1].Trim();
                    }

                    var childItem = new TreeListViewItem
                    {
                        Header = new TreeListViewItemHeaderModel { DefinitionType = child.DefinitionType, MemberName = memberName, TypeName = typeName, Comment = child.Comment },
                    };
                    headerItem.Items.Add(childItem);

                    if (child.Children.Any())
                    {
                        childItem.IsExpanded = true;
                        childItem.Margin = new Thickness(0, 10, 0, 0);

                        foreach (var grandChild in child.Children)
                        {
                            var grandChildItem = new TreeListViewItem
                            {
                                Header = new TreeListViewItemHeaderModel { DefinitionType = grandChild.DefinitionType, MemberName = grandChild.Text, Comment = grandChild.Comment },
                            };
                            childItem.Items.Add(grandChildItem);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 指定のコレクションを表示します。
        /// </summary>
        /// <param name="models"></param>
        public void ShowData(List<DefinitionHeaderModel> models)
        {
            //
            if (IsDisplayTreeListView)
            {
                gridSplitter1.Visibility = Visibility.Visible;
                treeListView1.Visibility = Visibility.Visible;
            }
            else
            {
                gridSplitter1.Visibility = Visibility.Collapsed;
                treeListView1.Visibility = Visibility.Collapsed;
            }


            //
            canvas1.Children.Clear();

            foreach (var model in models)
                ShowData(model);
        }

        private void ShowData(DefinitionHeaderModel model)
        {
            // コントロールを作成
            var newThumb = CreateControl(model);

            // 関連付けたい相手のコントロールがあれば、取得しておく
            var relationThumb = canvas1.Children.OfType<Thumb>().FirstOrDefault(x =>
            {
                var candidateModel = x.DataContext as DefinitionHeaderModel;
                return (candidateModel != null && candidateModel.ID == model.RelationID);
            });

            // コントロールを登録
            canvas1.Children.Add(newThumb);

            // コントロールの表示位置をセット
            SetNewThumbLocation(relationThumb, newThumb);

            // 関連付けたい相手と線でつなげる
            if (relationThumb == null)
                return;

            ConnectArrowControl(relationThumb, newThumb);
        }

        private Thumb CreateControl(DefinitionHeaderModel model)
        {
            var thumb1 = new Thumb();
            thumb1.DragStarted += Thumb1_DragStarted;
            thumb1.DragDelta += Thumb1_DragDelta;
            thumb1.DragCompleted += Thumb1_DragCompleted;

            var template1 = FindResource("DefinitionTreeTemplate") as ControlTemplate;
            thumb1.Template = template1;
            thumb1.DataContext = model;

            Measure(new Size(canvas1.Width, canvas1.Height));
            thumb1.ApplyTemplate();
            thumb1.UpdateLayout();

            if (!model.IsExpanded)
            {
                var border1 = thumb1.Template.FindName("PART_RootBorder", thumb1) as Border;
                var newPadding = border1.Padding;
                newPadding.Right = 10;
                border1.Padding = newPadding;
                thumb1.UpdateLayout();
            }

            if (model.IsDifferenceFile)
            {
                //// TextBlock.MouseLeftButtonUp でやりたかったが、Expander が Z インデックス的にクリック動作を先に取られてしまう、より早く反応して開閉してしまうため、
                //// Preview 系で Down じゃないと発生順序で勝てない？
                var textBlock1 = thumb1.Template.FindName("PART_DifferenceName", thumb1) as TextBlock;
                textBlock1.ToolTip = model.DifferenceFile;
                textBlock1.Tag = model;
                textBlock1.PreviewMouseLeftButtonDown += TextBlock1_PreviewMouseLeftButtonDown;
                thumb1.UpdateLayout();
            }

            return thumb1;
        }

        /// <summary>
        /// Thumb をドラッグ移動開始する時のイベントです。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Thumb1_DragStarted(object sender, DragStartedEventArgs e)
        {
            //var thumb1 = sender as Thumb;
            //if (thumb1 == null)
            //    return;

            //var rect1 = thumb1.Template.FindName("PART_Rectangle", thumb1) as Rectangle;
            //if (rect1 == null)
            //    return;

            //rect1.StrokeThickness = 5.0;
        }

        /// <summary>
        /// Thumb をドラッグ移動している時のイベントです。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Thumb1_DragDelta(object sender, DragDeltaEventArgs e)
        {
            // 対象 Thumb の移動
            var thumb1 = sender as Thumb;
            if (thumb1 == null)
                return;
            
            var x = Canvas.GetLeft(thumb1) + e.HorizontalChange;
            var y = Canvas.GetTop(thumb1) + e.VerticalChange;

            var parentCanvas = thumb1.Parent as Canvas;
            if (parentCanvas == null)
                return;

            x = Math.Max(x, 0);
            y = Math.Max(y, 0);

            x = Math.Min(x, parentCanvas.ActualWidth - thumb1.ActualWidth);
            y = Math.Min(y, parentCanvas.ActualHeight - thumb1.ActualHeight);

            Canvas.SetLeft(thumb1, x);
            Canvas.SetTop(thumb1, y);

            // 対象 Thumb につなげている矢印の移動
            var arrows = canvas1.Children.OfType<Arrow>();
            foreach (var arrow in arrows)
            {
                if (arrow.StartThumb == thumb1 || arrow.EndThumb == thumb1)
                    arrow.UpdateLocation();
            }
        }

        /// <summary>
        /// Thumb のドラッグ移動を止めた時のイベントです。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Thumb1_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            //var thumb1 = sender as Thumb;
            //if (thumb1 == null)
            //    return;

            //var rect1 = thumb1.Template.FindName("PART_Rectangle", thumb1) as Rectangle;
            //if (rect1 == null)
            //    return;

            //rect1.StrokeThickness = 1.0;
        }

        /// <summary>
        /// マウスの左ボタンを押した時のイベントです。
        /// </summary>
        /// <remarks>
        /// Winforms でいう LinkLabel、WPF でいう HyperLinks を表現するための、疑似クリックイベントです。
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBlock1_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var target = sender as TextBlock;
            if (target == null)
                return;

            var model = target.Tag as DefinitionHeaderModel;
            if (model == null)
                return;

            AppEnv.MainView.ShowDocumentPane(model.DifferenceFile, model.StartOffset);

            e.Handled = true;
        }

        private void SetNewThumbLocation(Thumb relationThumb, Thumb newThumb)
        {
            // コレクションの１つ目の Thumb の場合
            if (relationThumb == null && canvas1.Children.Count == 1)
            {
                var pos = new Point(10, 10);
                Canvas.SetLeft(newThumb, pos.X);
                Canvas.SetTop(newThumb, pos.Y);

                return;
            }

            // ２つ目以降の Thumb の場合
            var relationModel = relationThumb?.DataContext as DefinitionHeaderModel;
            var newModel = newThumb.DataContext as DefinitionHeaderModel;

            if (string.IsNullOrWhiteSpace(newModel.RelationID))
            {
                // プロジェクト間の参照ツリー（全体）ペインの場合で、各プロジェクト毎のコレクションのうち１つ目の場合
                // コレクションの１つ目の Thumb と同じパターン

                // 新しいプロジェクトの始まりの場合、1つ目の列に追加する
                relationThumb = canvas1.Children.OfType<Thumb>().FirstOrDefault();

                // 真下
                var pos = GetMostBottomSideLocation(relationThumb);
                Canvas.SetLeft(newThumb, pos.X);
                Canvas.SetTop(newThumb, pos.Y);
            }
            else if (relationModel.ID == newModel.RelationID)
            {
                // 右隣
                var pos = GetRightSideLocation2(relationThumb);
                Canvas.SetLeft(newThumb, pos.X);
                Canvas.SetTop(newThumb, pos.Y);
            }
            else
            {
                /*
                 * 以下の場合
                 * relationModel.ID != string.Empty
                 * newModel.RelationID != string.Empty
                 * relationModel.ID != newModel.RelationID
                 * 
                 */

                throw new InvalidOperationException("RelationTreePanel.xaml.cs / SetNewThumbLocation()");
            }
        }

        // 基準コントロールの真下位置
        private Point GetBottomSideLocation(Thumb target)
        {
            // これは、コントロールの左上の位置となる
            // var x = Canvas.GetLeft(target)
            // var y = Canvas.GetTop(target)

            var newHeight = target.DesiredSize.Height;

            var spaceMarginHeight = 40;
            var newX = Canvas.GetLeft(target);
            var newY = Canvas.GetTop(target) + newHeight + spaceMarginHeight;
            var pos = new Point(newX, newY);

            return pos;
        }

        // 基準コントロールの右隣位置
        private Point GetRightSideLocation(Thumb target)
        {
            var newWidth = target.DesiredSize.Width;

            var spaceMarginWidth = 40;
            var newX = Canvas.GetLeft(target) + newWidth + spaceMarginWidth;
            var newY = Canvas.GetTop(target);
            var pos = new Point(newX, newY);

            return pos;
        }

        // 基準コントロールの斜め右下位置
        private Point GetBottomRightSideLocation(Thumb target)
        {
            var newWidth = target.DesiredSize.Width;
            var newHeight = target.DesiredSize.Height;

            var spaceMarginWidth = 40;
            var spaceMarginHeight = 40;

            var newX = Canvas.GetLeft(target) + newWidth + spaceMarginWidth;
            var newY = Canvas.GetTop(target) + newHeight + spaceMarginHeight;
            var pos = new Point(newX, newY);

            return pos;
        }

        // 基準コントロールの真上位置
        private Point GetTopSideLocation(Thumb targetThumb, Thumb newThumb)
        {
            var x = Canvas.GetLeft(targetThumb);
            var y = Canvas.GetTop(targetThumb);

            var spaceMarginHeight = 40;

            var newWidth = newThumb.DesiredSize.Width;
            var newHeight = newThumb.DesiredSize.Height;

            var newX = x;
            var newY = y - spaceMarginHeight - newHeight;
            var pos = new Point(newX, newY);

            return pos;
        }

        // 基準コントロールの左隣位置
        private Point GetLeftSideLocation(Thumb targetThumb, Thumb newThumb)
        {
            var x = Canvas.GetLeft(targetThumb);
            var y = Canvas.GetTop(targetThumb);

            var spaceMarginWidth = 40;

            var newWidth = newThumb.DesiredSize.Width;
            var newHeight = newThumb.DesiredSize.Height;

            var newX = x - spaceMarginWidth - newWidth;
            var newY = y;
            var pos = new Point(newX, newY);

            return pos;
        }

        // 基準コントロールの斜め左上位置
        private Point GetTopLeftSideLocation(Thumb targetThumb, Thumb newThumb)
        {
            var x = Canvas.GetLeft(targetThumb);
            var y = Canvas.GetTop(targetThumb);

            var spaceMarginWidth = 40;
            var spaceMarginHeight = 40;

            var newWidth = newThumb.DesiredSize.Width;
            var newHeight = newThumb.DesiredSize.Height;

            var newX = x - spaceMarginWidth - newWidth;
            var newY = y - spaceMarginHeight - newHeight;
            var pos = new Point(newX, newY);

            return pos;
        }

        // 基準コントロールの真下位置、かつ全ての表示中のコントロールよりも真下位置
        // つまり、基準コントロールの真下の場合もあるし、コントロール何個分のスペースを開けて、その下の位置の場合もある
        private Point GetMostBottomSideLocation(Thumb target)
        {
            var newHeight = target.DesiredSize.Height;

            var spaceMarginHeight = 40;
            var newX = Canvas.GetLeft(target);
            var newY = Canvas.GetTop(target) + newHeight + spaceMarginHeight;

            // 予測計算した位置に、すでに他のコントロールが配置されていないかチェック
            // 一部が重なり合う場合、重ならないように予測位置を修正して、再度全チェック
            // つまり、候補の左上位置の Y 座標が、全てのコントロールの表示位置左下位置の Y 座標よりも、下であること
            var items = canvas1.Children.OfType<Thumb>();
            var last = items.Last();
            var found = false;

            while (true)
            {
                // 初期化してチャレンジ、または再チャレンジ
                found = false;

                foreach (var checkThumb in items)
                {
                    if (checkThumb == last)
                        continue;

                    // チェックするのは Y 座標関連のみでOK
                    var checkY = Canvas.GetTop(checkThumb);
                    var checkHeight = checkThumb.DesiredSize.Height;
                    var candidateY = checkY + checkHeight + spaceMarginHeight;

                    if (newY < candidateY)
                    {
                        found = true;
                        newY = candidateY;
                    }
                }

                // 既存配置しているコントロール全てに重ならなかったので、この位置で決定
                if (!found)
                    break;

                // 見つかった場合は、１つ以上重なっていたことにより、予測位置を修正したので、
                // もう一度コントロール全部と再チェック（２週目は修正無しなはず）
            }

            var pos = new Point(newX, newY);
            return pos;
        }

        // 基準コントロールの右隣位置、かつすでにそこにコントロールがいた場合は、コントロールがいない位置まで真下に移動した後の位置
        private Point GetRightSideLocation2(Thumb target)
        {
            var newWidth = target.DesiredSize.Width;

            var spaceMarginWidth = 40;
            var spaceMarginHeight = 40;

            var newX = Canvas.GetLeft(target) + newWidth + spaceMarginWidth;
            var newY = Canvas.GetTop(target);

            // 予測計算した位置に、すでに他のコントロールが配置されていないかチェック
            // 一部が重なり合う場合、重ならないように予測位置を修正して、再度全チェック
            // つまり、候補の左上位置の Y 座標が、全てのコントロールの表示位置左下位置の Y 座標よりも、下であること
            var items = canvas1.Children.OfType<Thumb>();
            var last = items.Last();
            var found = false;

            while (true)
            {
                // 初期化してチャレンジ、または再チャレンジ
                found = false;

                foreach (var checkThumb in items)
                {
                    if (checkThumb == last)
                        continue;

                    // 
                    var checkX = Canvas.GetLeft(checkThumb);
                    var checkY = Canvas.GetTop(checkThumb);

                    if (newX == checkX && newY == checkY)
                    {
                        found = true;
                        var checkHeight = checkThumb.DesiredSize.Height;
                        var candidateY = checkHeight + spaceMarginHeight;
                        newY += candidateY;
                    }
                }

                // 既存配置しているコントロール全てに重ならなかったので、この位置で決定
                if (!found)
                    break;

                // 見つかった場合は、１つ以上重なっていたことにより、予測位置を修正したので、
                // もう一度コントロール全部と再チェック（２週目は修正無しなはず）
            }

            var pos = new Point(newX, newY);
            return pos;
        }

        // 矢印コントロールを Canvas に追加して、指定の Thumb コントロール２つにつなげる
        private void ConnectArrowControl(Thumb relationThumb, Thumb newThumb)
        {
            var model = newThumb.DataContext as DefinitionHeaderModel;
            var arrow1 = new Arrow
            {
                Stroke = Brushes.LightPink,
                Fill = Brushes.LightPink,

                IsArrowDirectionEnd = model.IsArrowDirectionEnd,
                StartThumb = relationThumb,
                EndThumb = newThumb,
            };

            canvas1.Children.Add(arrow1);

            // newThumb 矢印の位置が左上になってしまうバグの対応。幅と高さを更新させる
            newThumb.UpdateLayout();
            arrow1.UpdateLocation();
        }

        /// <summary>
        /// 指定のコレクションを、左側に向かって表示します。
        /// </summary>
        /// <param name="baseTypeModels"></param>
        public void ShowDataForLeftSideBaseTypes(List<DefinitionHeaderModel> baseTypeModels)
        {
            //
            if (IsDisplayTreeListView)
            {
                gridSplitter1.Visibility = Visibility.Visible;
                treeListView1.Visibility = Visibility.Visible;
            }
            else
            {
                gridSplitter1.Visibility = Visibility.Collapsed;
                treeListView1.Visibility = Visibility.Collapsed;
            }

            // 表示範囲を中央に移動する（Canvas の中央位置までスクロールする）
            var viewX = canvas1.Width / 2;
            var viewY = canvas1.Height / 2;

            scrollViewer1.ScrollToHorizontalOffset(viewX);
            scrollViewer1.ScrollToVerticalOffset(viewY);

            // 厳密に言うと表示範囲の中央位置ではなく左上位置が(中央位置）となるので、多少ずれているが分かりやすく考えられるため、このままとする

            //
            canvas1.Children.Clear();

            foreach (var baseTypeModel in baseTypeModels)
                ShowDataForLeftSideBaseTypes(baseTypeModel);
        }

        private void ShowDataForLeftSideBaseTypes(DefinitionHeaderModel model)
        {
            // コントロールを作成
            var newThumb = CreateControl(model);

            // 関連付けたい相手のコントロールがあれば、取得しておく
            var relationThumb = canvas1.Children.OfType<Thumb>().FirstOrDefault(x =>
            {
                var candidateModel = x.DataContext as DefinitionHeaderModel;
                return (candidateModel != null && candidateModel.ID == model.RelationID);
            });

            // コントロールを登録
            canvas1.Children.Add(newThumb);

            // コントロールの表示位置をセット
            SetNewThumbLocationForLeftSideBaseTypes(relationThumb, newThumb);

            // 関連付けたい相手と線でつなげる
            if (relationThumb == null)
                return;

            ConnectArrowControlForLeftSideBaseTypes(relationThumb, newThumb);
        }

        private void SetNewThumbLocationForLeftSideBaseTypes(Thumb relationThumb, Thumb newThumb)
        {
            // コレクションの１つ目の Thumb の場合
            if (relationThumb == null && canvas1.Children.Count == 1)
            {
                // ShowDataForLeftSideBaseTypes(List<DefinitionHeaderModel> baseTypeModels) の処理内で、
                // canvas1 の中央位置を表示している（厳密に言うと表示範囲の中央位置ではなく左上位置が(中央位置））

                // newThumb を表示範囲の中央上位置に表示する

                // 表示範囲の左上位置を取得
                var x = scrollViewer1.HorizontalOffset;
                var y = scrollViewer1.VerticalOffset;

                // 表示範囲の中央上になるように加算
                x += scrollViewer1.ActualWidth / 2;
                //y += scrollViewer1.ActualHeight / 2;

                // コントロールの半分のサイズ分、引き算
                newThumb.UpdateLayout();
                x -= newThumb.DesiredSize.Width / 2;
                //y -= newThumb.DesiredSize.Height / 2;
                y += 10;

                var pos = new Point(x, y);
                Canvas.SetLeft(newThumb, pos.X);
                Canvas.SetTop(newThumb, pos.Y);

                return;
            }

            // ２つ目以降の Thumb の場合
            var relationModel = relationThumb?.DataContext as DefinitionHeaderModel;
            var newModel = newThumb.DataContext as DefinitionHeaderModel;

            if (string.IsNullOrWhiteSpace(newModel.RelationID))
            {
                // 通常ここには来ることは無いはず。ただ来たら以下を設定する

                // 新しいプロジェクトの始まりの場合、1つ目の列に追加する
                relationThumb = canvas1.Children.OfType<Thumb>().FirstOrDefault();

                // 真下
                var pos = GetMostBottomSideLocation(relationThumb);
                Canvas.SetLeft(newThumb, pos.X);
                Canvas.SetTop(newThumb, pos.Y);
            }
            else if (relationModel.ID == newModel.RelationID)
            {
                // 左隣
                var pos = GetLeftSideLocation2(relationThumb, newThumb);
                Canvas.SetLeft(newThumb, pos.X);
                Canvas.SetTop(newThumb, pos.Y);
            }
            else
            {
                /*
                 * 以下の場合
                 * relationModel.ID != string.Empty
                 * newModel.RelationID != string.Empty
                 * relationModel.ID != newModel.RelationID
                 * 
                 */

                throw new InvalidOperationException("RelationTreePanel.xaml.cs / SetNewThumbLocationForLeftSideBaseTypes()");
            }
        }

        // 基準コントロールの左隣位置、かつすでにそこにコントロールがいた場合は、コントロールがいない位置まで真下に移動した後の位置
        private Point GetLeftSideLocation2(Thumb targetThumb, Thumb newThumb)
        {
            var x = Canvas.GetLeft(targetThumb);
            var y = Canvas.GetTop(targetThumb);

            var spaceMarginWidth = 40;
            var spaceMarginHeight = 40;

            newThumb.UpdateLayout();
            var newWidth = newThumb.DesiredSize.Width;
            var newHeight = newThumb.DesiredSize.Height;

            // 候補位置は右上位置
            var newX = x - spaceMarginWidth;
            var newY = y;

            // 予測計算した位置に、すでに他のコントロールが配置されていないかチェック
            // 一部が重なり合う場合、重ならないように予測位置を修正して、再度全チェック
            // つまり、候補の左上位置の Y 座標が、全てのコントロールの表示位置左下位置の Y 座標よりも、下であること
            var items = canvas1.Children.OfType<Thumb>();
            var last = items.Last();
            var found = false;

            while (true)
            {
                // 初期化してチャレンジ、または再チャレンジ
                found = false;

                foreach (var checkThumb in items)
                {
                    if (checkThumb == last)
                        continue;

                    // チェック対称の図形も、右上位置を取得
                    var checkX = Canvas.GetLeft(checkThumb);
                    var checkY = Canvas.GetTop(checkThumb);

                    checkX += checkThumb.DesiredSize.Width;

                    if (newX == checkX && newY == checkY)
                    {
                        found = true;
                        var checkHeight = checkThumb.DesiredSize.Height;
                        var candidateY = checkHeight + spaceMarginHeight;
                        newY += candidateY;
                    }
                }

                // 既存配置しているコントロール全てに重ならなかったので、この位置で決定
                if (!found)
                    break;

                // 見つかった場合は、１つ以上重なっていたことにより、予測位置を修正したので、
                // もう一度コントロール全部と再チェック（２週目は修正無しなはず）
            }

            // 右上位置に戻す
            newX -= newWidth;
            var pos = new Point(newX, newY);
            return pos;
        }

        // 矢印コントロールを Canvas に追加して、指定の Thumb コントロール２つにつなげる
        private void ConnectArrowControlForLeftSideBaseTypes(Thumb relationThumb, Thumb newThumb)
        {
            var model = newThumb.DataContext as DefinitionHeaderModel;
            var arrow1 = new Arrow
            {
                Stroke = Brushes.LightPink,
                Fill = Brushes.LightPink,

                IsArrowDirectionEnd = !model.IsArrowDirectionEnd,
                StartThumb = newThumb,
                EndThumb = relationThumb,
            };

            canvas1.Children.Add(arrow1);

            // newThumb 矢印の位置が左上になってしまうバグの対応。幅と高さを更新させる
            newThumb.UpdateLayout();
            arrow1.UpdateLocation();
        }

        /// <summary>
        /// 指定のコレクションを、右側に向かって表示します。
        /// </summary>
        /// <param name="inheritanceTypeModels"></param>
        public void ShowDataForRightSideInheritanceTypes(List<DefinitionHeaderModel> inheritanceTypeModels)
        {
            if (!inheritanceTypeModels.Any())
                return;

            var targetThumb = canvas1.Children.OfType<Thumb>().FirstOrDefault();
            var leftSideRoot = targetThumb.DataContext as DefinitionHeaderModel;

            // 先に継承元ツリーを表示したので、Canvas.Children の１つ目は、今から追加する継承先ツリーの１つ目と同じになる。
            // ID, BaseTypes を入れ替えて、（重複しないように）継承先ツリーの１つ目を削除する（継承元ツリーの１つ目を関係先に変える）
            var rightSideRoot = inheritanceTypeModels[0];

            foreach (var baseTypeModel in inheritanceTypeModels)
            {
                if (baseTypeModel.RelationID == rightSideRoot.ID)
                    baseTypeModel.RelationID = leftSideRoot.ID;
            }

            inheritanceTypeModels.RemoveAt(0);

            //
            foreach (var model in inheritanceTypeModels)
                ShowDataForRightSideInheritanceTypes(model);
        }

        private void ShowDataForRightSideInheritanceTypes(DefinitionHeaderModel model)
        {
            // コントロールを作成
            var newThumb = CreateControl(model);

            // 関連付けたい相手のコントロールがあれば、取得しておく
            var relationThumb = canvas1.Children.OfType<Thumb>().FirstOrDefault(x =>
            {
                var candidateModel = x.DataContext as DefinitionHeaderModel;
                return (candidateModel != null && candidateModel.ID == model.RelationID);
            });

            // コントロールを登録
            canvas1.Children.Add(newThumb);

            // コントロールの表示位置をセット
            SetNewThumbLocationForRightSideInheritanceTypes(relationThumb, newThumb);

            // 関連付けたい相手と線でつなげる
            if (relationThumb == null)
                return;

            ConnectArrowControlForRightSideInheritanceTypes(relationThumb, newThumb);
        }

        private void SetNewThumbLocationForRightSideInheritanceTypes(Thumb relationThumb, Thumb newThumb)
        {
            //
            var relationModel = relationThumb?.DataContext as DefinitionHeaderModel;
            var newModel = newThumb.DataContext as DefinitionHeaderModel;

            if (string.IsNullOrWhiteSpace(newModel.RelationID))
            {
                throw new InvalidOperationException("RelationTreePanel.xaml.cs / SetNewThumbLocationForRightSideInheritanceTypes()");
            }
            else if (relationModel.ID == newModel.RelationID)
            {
                // 右隣
                var pos = GetRightSideLocation2(relationThumb);
                Canvas.SetLeft(newThumb, pos.X);
                Canvas.SetTop(newThumb, pos.Y);
            }
            else
            {
                /*
                 * 以下の場合
                 * relationModel.ID != string.Empty
                 * newModel.RelationID != string.Empty
                 * relationModel.ID != newModel.RelationID
                 * 
                 */

                throw new InvalidOperationException("RelationTreePanel.xaml.cs / SetNewThumbLocationForRightSideInheritanceTypes()");
            }
        }

        // 矢印コントロールを Canvas に追加して、指定の Thumb コントロール２つにつなげる
        private void ConnectArrowControlForRightSideInheritanceTypes(Thumb relationThumb, Thumb newThumb)
        {
            var model = newThumb.DataContext as DefinitionHeaderModel;
            var arrow1 = new Arrow
            {
                Stroke = Brushes.LightPink,
                Fill = Brushes.LightPink,

                IsArrowDirectionEnd = model.IsArrowDirectionEnd,
                StartThumb = relationThumb,
                EndThumb = newThumb,
            };

            canvas1.Children.Add(arrow1);

            // newThumb 矢印の位置が左上になってしまうバグの対応。幅と高さを更新させる
            newThumb.UpdateLayout();
            arrow1.UpdateLocation();
        }
    }
}
