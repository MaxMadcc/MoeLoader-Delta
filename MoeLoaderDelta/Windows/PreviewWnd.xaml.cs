﻿using MoeLoaderDelta.Control;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace MoeLoaderDelta
{
    /// <summary>
    /// Interaction logic for PreviewWnd.xaml
    /// 预览窗口
    /// Fixed 201800419
    /// </summary>
    public partial class PreviewWnd : Window
    {
        private MainWindow mainW;

        //主窗口缩略图索引
        internal Dictionary<int, int> oriIndex = new Dictionary<int, int>();
        internal int selectedId;
        private int index;
        //上次鼠标的位置
        private int preMX, preMY;
        #region === GetSet封装 ===
        public int SelectedId
        {
            get => selectedId;
            set => selectedId = value;
        }

        public Dictionary<int, int> Imgs { get; set; } = new Dictionary<int, int>();

        public Dictionary<int, Img> Descs { get; } = new Dictionary<int, Img>();
        #endregion

        public PreviewWnd(MainWindow mainW)
        {
            this.mainW = mainW;
            InitializeComponent();
            Title = MainWindow.ProgramName + " Preview";

            if (!File.Exists(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\nofont.txt"))
                FontFamily = new FontFamily("Microsoft YaHei");

            Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xF5, 0xF5, 0xF5));
            MouseLeftButtonDown += new MouseButtonEventHandler(MainWindow_MouseLeftButtonDown);
            KeyDown += new KeyEventHandler(Window_KeyDown);
        }

        public PreviewWnd() { }

        private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch { }
        }

        /// <summary>
        /// 添加预览
        /// </summary>
        /// <param name="img"></param>
        /// <param name="parentIndex"></param>
        /// <param name="needReferer"></param>
        public void AddPreview(Img img, int parentIndex, string needReferer)
        {
            if (!Imgs.ContainsKey(img.Id))
            {
                Imgs.Add(img.Id, index++);
                oriIndex.Add(img.Id, parentIndex);
                Descs.Add(img.Id, img);
                //添加预览图分页按钮
                ToggleButton btn = new ToggleButton
                {
                    Content = img.Id,
                    Margin = new Thickness(3, 1, 3, 1)
                };
                btn.Checked += new RoutedEventHandler(btn_Click);
                btns.Children.Add(btn);

                //初始化预览图
                PreviewImg prei = new PreviewImg(this, img);
                prei.MouseLeftButtonUp += new MouseButtonEventHandler(delegate (object s1, MouseButtonEventArgs ea)
                {
                    preMX = 0; preMY = 0;
                });
                prei.MouseLeftButtonDown += new MouseButtonEventHandler(delegate (object s1, MouseButtonEventArgs ea)
                {
                    preMX = 0; preMY = 0;
                });
                prei.MouseDown += new MouseButtonEventHandler(delegate (object s1, MouseButtonEventArgs ea)
                {
                    //中键缩放
                    if (ea.MiddleButton == MouseButtonState.Pressed)
                        Button_Click_2(null, null);
                });
                prei.MouseMove += new MouseEventHandler(delegate (object s1, MouseEventArgs ea)
                {
                    //拖动
                    if (ea.LeftButton == MouseButtonState.Pressed)
                    {
                        if (preMY != 0 && preMX != 0)
                        {
                            int offX = (int)(ea.GetPosition(LayoutRoot).X) - preMX;
                            int offY = (int)(ea.GetPosition(LayoutRoot).Y) - preMY;
                            ScrollViewer sc = (imgGrid.Children[Imgs[selectedId]] as ScrollViewer);
                            sc.ScrollToHorizontalOffset(sc.HorizontalOffset - offX);
                            sc.ScrollToVerticalOffset(sc.VerticalOffset - offY);
                        }
                        preMX = (int)(ea.GetPosition(LayoutRoot).X);
                        preMY = (int)(ea.GetPosition(LayoutRoot).Y);
                    }
                });

                //加入预览图控件
                imgGrid.Children.Add(new ScrollViewer()
                {
                    Content = prei,
                    Visibility = Visibility.Hidden,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Disabled
                });
                //开始下载图片
                prei.DownloadImg(needReferer);

                if (selectedId == 0)
                {
                    (btns.Children[btns.Children.Count - 1] as ToggleButton).IsChecked = true;
                }
                ChangePreBtnText();
            }
        }


        /// <summary>
        /// 切换预览图
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btn_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)(sender as ToggleButton).Content;
            SwitchPreview(id);
        }

        /// <summary>
        /// 切换预览图操作
        /// </summary>
        /// <param name="id">预览ID</param>
        public void SwitchPreview(int id)
        {
            if (selectedId != id)
            {
                if (selectedId != 0 && Imgs.ContainsKey(selectedId))
                {
                    (btns.Children[Imgs[selectedId]] as ToggleButton).IsChecked = false;

                    //(imgGrid.Children[imgs[selectedId]] as Image).Opacity = 0;
                    //(imgGrid.Children[imgs[selectedId]] as Image).BeginStoryboard(FindResource("imgClose") as Storyboard);
                    ScrollViewer tempPreview = (imgGrid.Children[Imgs[selectedId]] as ScrollViewer);
                    Storyboard sb = new Storyboard();
                    DoubleAnimationUsingKeyFrames frames = new DoubleAnimationUsingKeyFrames();
                    Storyboard.SetTargetProperty(frames, new PropertyPath(UIElement.OpacityProperty));
                    frames.KeyFrames.Add(new SplineDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(80))));
                    sb.Children.Add(frames);
                    sb.Completed += new EventHandler(delegate (object s, EventArgs ea) { tempPreview.Visibility = Visibility.Hidden; });
                    sb.Begin(tempPreview);
                }
                selectedId = id;
                (btns.Children[Imgs[selectedId]] as ToggleButton).IsChecked = true;
                ScrollViewer tempPreview1 = (imgGrid.Children[Imgs[selectedId]] as ScrollViewer);
                tempPreview1.Visibility = Visibility.Visible;
                tempPreview1.BeginStoryboard(FindResource("imgShow") as Storyboard);
                ChangePreBtnText();

                ///////////////////////////////////////////////
                ////////////////////////////////////////////

                desc.Text = "";
                if (Descs[selectedId].OriginalUrl == Descs[selectedId].SampleUrl)
                {
                    desc.Inlines.Add("原图与预览图相同");
                    desc.Inlines.Add(new LineBreak());
                }
                desc.Inlines.Add("描述: " + Descs[selectedId].Id + " " + Descs[selectedId].Desc);
                desc.Inlines.Add(new LineBreak());
                desc.Inlines.Add("作者: " + Descs[selectedId].Author);
                desc.Inlines.Add(new LineBreak());
                try
                {
                    string fileType = Descs[selectedId].OriginalUrl.Substring(Descs[selectedId].OriginalUrl.LastIndexOf('.') + 1);
                    desc.Inlines.Add("类型: " + BooruProcessor.FormattedImgUrl("", fileType.ToUpper()));
                }
                catch { }
                desc.Inlines.Add(" 大小: " + Descs[selectedId].FileSize);
                desc.Inlines.Add(" 尺寸: " + Descs[selectedId].Dimension);
                //desc.Inlines.Add(new LineBreak());
                desc.Inlines.Add(" 评分: " + Descs[selectedId].Score);
                desc.Inlines.Add(new LineBreak());
                desc.Inlines.Add("时间: " + Descs[selectedId].Date);
                if (Descs[selectedId].Source.Length > 0)
                {
                    desc.Inlines.Add(new LineBreak());
                    desc.Inlines.Add("来源: " + Descs[selectedId].Source);
                }
            }
        }

        /// <summary>
        /// 选中并关闭 
        /// 取消选中并关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (selectedId != 0)
            {
                mainW.SelectByIndex(oriIndex[selectedId]);
                CloseImg();
            }
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            if (selectedId != 0)
            {
                CloseImg();
            }
        }

        private void CloseImg()
        {
            //close
            int oriId = selectedId;
            int imgc = Imgs.Count;

            PreviewImg pi = (imgGrid.Children[Imgs[oriId]] as ScrollViewer).Content as PreviewImg;
            pi.StopLoadImg(oriId, "");

            //移除按钮和预览图
            btns.Children.Remove(btns.Children[Imgs[oriId]]);
            imgGrid.Children.Remove(imgGrid.Children[Imgs[oriId]]);

            if (imgc > 0) index = Imgs[oriId];

            //删除关闭数据
            Imgs.Remove(oriId);
            Descs.Remove(oriId);
            oriIndex.Remove(oriId);

            imgc = Imgs.Count;
            if (imgc > 0)
            {
                //更新数组索引值
                for (int i = index; i < imgc; i++)
                {
                    int newindex = Imgs[(int)((ToggleButton)btns.Children[i]).Content];
                    Imgs[(int)((ToggleButton)btns.Children[i]).Content] = --newindex;
                }

                //切换预览图
                //选择关闭的图前一张
                index--;
                //没有前一张就选第一张
                index = index < 0 ? 0 : index;

                //选中按钮
                ToggleButton checkedTB = (ToggleButton)btns.Children[index];
                checkedTB.IsChecked = true;

                //index为0时添加图片不能自增、因此需要在此设1
                index = index < 1 ? 1 : index;
                GC.Collect(2, GCCollectionMode.Optimized);
            }
            else
            {
                selectedId = index = 0;
                GC.Collect(2, GCCollectionMode.Optimized);
                Close();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //清理预览图组数据
            foreach (int iid in Imgs.Values)
            {
                PreviewImg pi = (imgGrid.Children[iid] as ScrollViewer).Content as PreviewImg;
                var dicSort = from pireq in pi.Reqs orderby pireq.Value descending select pireq;
                foreach (KeyValuePair<int, System.Net.HttpWebRequest> req in dicSort)
                    req.Value.Abort();
                pi.Reqs.Clear();
            }
            Imgs.Clear();
            Descs.Clear();
            oriIndex.Clear();
            imgGrid.Children.Clear();
            mainW.previewFrm = null;

            GC.Collect(2, GCCollectionMode.Optimized);
        }

        /// <summary>
        /// 保存预览图
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            SavePreview(false, DownloadControl.SaveLocation + "\\" + GetSelectPreviewImgFileName());
        }

        /// <summary>
        /// 保存预览图到文件
        /// </summary>
        /// <param name="silent">静默模式</param>
        /// <param name="path">保存路径</param>
        /// <returns>预览文件类型</returns>
        private string SavePreview(bool silent, string path)
        {
            try
            {
                if (!((imgGrid.Children[Imgs[selectedId]] as ScrollViewer).Content as PreviewImg).ImgLoaded)
                {
                    ShowMessage("图片尚未加载完毕", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return "";
                }

                PreviewImg pimg = (PreviewImg)((ScrollViewer)imgGrid.Children[Imgs[selectedId]]).Content;
                switch (pimg.ImgType)
                {
                    case "bmp":
                    case "jpg":
                    case "png":
                        Image im = (Image)pimg.prewimg.Children[0];
                        DataHelpers.ImageToFile((BitmapSource)im.Source, pimg.ImgType, path);
                        break;
                    case "gif":
                        AnimatedGIF gi = (AnimatedGIF)pimg.prewimg.Children[0];
                        DataHelpers.MemoryStreamToFile((MemoryStream)gi.GIFSource, path);
                        break;
                    default:
                        throw new Exception(pimg.ImgType + "类型不支持保存");
                }

                if (!silent)
                    ShowMessage("已成功保存至下载文件夹", MessageBoxButton.OK, MessageBoxImage.Information);

                return pimg.ImgType;
            }
            catch (Exception ex)
            {
                if (!silent)
                    ShowMessage("保存失败\r\n" + ex.Message, MessageBoxButton.OK, MessageBoxImage.Warning);
                return "";
            }
        }

        /// <summary>
        /// 复制预览图
        /// 以QQ剪切板格式创建
        /// http://blog.csdn.net/crystal_lz/article/details/51737713
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //取当前预览内容、类型、保存路径
                PreviewImg pimg = (PreviewImg)((ScrollViewer)imgGrid.Children[Imgs[selectedId]]).Content;

                bool novprew = false;
                string ttype = pimg.ImgType;
                string tempfile = GetTempFilePath();

                if (!File.Exists(tempfile))
                    if (SavePreview(true, tempfile) == "") return;

                //构建剪贴对象
                IDataObject clipobj = new DataObject();

                //根据类型分类处理
                switch (ttype)
                {
                    case "bmp":
                    case "jpg":
                    case "png":
                    case "gif":

                        //--- 复制编辑框格式 ---
                        StringBuilder sb = new StringBuilder();
                        sb.Append("<QQRichEditFormat><Info version=\"1001\"></Info>");
                        sb.AppendFormat("<EditElement type=\"1\" filepath=\"{0}\" shortcut=\"\"></EditElement>", tempfile);
                        sb.Append("<EditElement type=\"0\"><![CDATA[]]></EditElement></QQRichEditFormat>");

                        byte[] bydate = Encoding.UTF8.GetBytes(sb.ToSafeString());
                        clipobj.SetData("QQ_Unicode_RichEdit_Format", new MemoryStream(bydate));
                        clipobj.SetData("QQ_RichEdit_Format", new MemoryStream(bydate));

                        //--- 复制图像数据 ---
                        System.Drawing.Image di = System.Drawing.Image.FromStream(pimg.Strs);
                        clipobj.SetData(DataFormats.Dib, di);
                        break;

                    default:
                        novprew = true;
                        break;
                }

                if (novprew)
                    ShowMessage(ttype + "类型不支持复制预览，仅复制了文件", MessageBoxButton.OK, MessageBoxImage.Warning);

                //--- 复制预览文件 ---
                string[] tempfs = new string[] { tempfile };
                clipobj.SetData("FileDrop", tempfs);
                clipobj.SetData("FileNameW", tempfs);
                clipobj.SetData("FileName", tempfs);

                //置入剪贴对象到剪贴板
                Clipboard.SetDataObject(clipobj, true);

            }
            catch (Exception ex)
            {
                ShowMessage("复制预览 " + selectedId + " 失败\r\n" + ex.Message, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// 缩放
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (!((imgGrid.Children[Imgs[selectedId]] as ScrollViewer).Content as PreviewImg).ImgLoaded)
            {
                MainWindow.MainW.Toast.Show("图片尚未加载完毕");
            }
            else
            {
                PreviewImg img = (imgGrid.Children[Imgs[selectedId]] as ScrollViewer).Content as PreviewImg;
                if (img.isZoom)
                {
                    (imgGrid.Children[Imgs[selectedId]] as ScrollViewer).HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                    (imgGrid.Children[Imgs[selectedId]] as ScrollViewer).VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                    img.ImgZoom(false, false);
                }
                else
                {
                    (imgGrid.Children[Imgs[selectedId]] as ScrollViewer).HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    (imgGrid.Children[Imgs[selectedId]] as ScrollViewer).VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    img.ImgZoom(false);
                }
            }
        }

        /// <summary>
        /// 复制预览链接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(Descs[selectedId].SampleUrl);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// 复制原始链接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(Descs[selectedId].OriginalUrl);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// 复制来源链接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(Descs[selectedId].Source.Replace("\n", ""));
            }
            catch (Exception) { }
        }

        /// <summary>
        /// 复制描述
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(Descs[selectedId].Desc.Replace("\n", ""));
            }
            catch (Exception) { }
        }

        /// <summary>
        /// 复制详情页链接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click_5(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Descs[selectedId].DetailUrl.Length > 0)
                    System.Diagnostics.Process.Start(Descs[selectedId].DetailUrl);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// 复制JPG链接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click_6(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(Descs[selectedId].JpegUrl);
            }
            catch (Exception) { }
        }

        #region 信息框鼠标悬浮样式处理
        private void Border_MouseEnter_1(object sender, MouseEventArgs e)
        {
            brdDesc.Opacity = 0.05;
        }

        private void Border_MouseLeave_1(object sender, MouseEventArgs e)
        {
            brdDesc.Opacity = 1;
        }
        #endregion

        /// <summary>
        /// 取预览图远程文件名
        /// </summary>
        /// <returns></returns>
        private string GetSelectPreviewImgFileName()
        {
            string dest = Uri.UnescapeDataString(Descs[selectedId].SampleUrl.Substring(Descs[selectedId].SampleUrl.LastIndexOf('/') + 1));
            dest = DownloadControl.ReplaceInvalidPathChars(dest);
            return dest;
        }

        /// <summary>
        /// 取临时文件路径
        /// </summary>
        /// <returns>临时路径</returns>
        private string GetTempFilePath()
        {
            string tp = Path.GetTempPath();
            string dr = tp + @"Moeloadelta\";

            if (!Directory.Exists(dr))
                Directory.CreateDirectory(dr);
            return dr + mainW.siteMenu.Header + "_" + GetSelectPreviewImgFileName();
        }

        /// <summary>
        /// 显示提示消息框
        /// </summary>
        /// <param name="Msg">消息内容</param>
        /// <param name="MsgButton">消息按钮</param>
        /// <param name="MsgImg">消息图标</param>
        private void ShowMessage(string Msg, MessageBoxButton MsgButton, MessageBoxImage MsgImg)
        {
            MessageBox.Show(this, Msg, MainWindow.ProgramName, MsgButton, MsgImg);
        }



        /// <summary>
        /// 预览窗口按键处理
        /// </summary>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl + C 复制预览
            if (MainWindow.IsCtrlDown() && e.Key == Key.C)
            {
                if (IsLoaded)
                {
                    MenuItem_Click(null, null);
                }
            }
            else if (e.Key == Key.NumPad4 || e.Key == Key.OemComma || unSafeHelper.GetPrivateField<int>(e, "_scanCode").Equals(51))
            {
                //小键盘4 或 ,键 切换上个预览
                SwitchNearPreview();
            }
            else if (e.Key == Key.NumPad6 || e.Key == Key.OemPeriod || unSafeHelper.GetPrivateField<int>(e, "_scanCode").Equals(52))
            {
                //数字键6 或 .键  切换下个预览
                SwitchNearPreview(1);
            }
            else if (MainWindow.IsCtrlDown() && e.Key == Key.W) //Ctrl + W 关闭当前预览
            {
                Button1_Click(null, null);
            }
        }

        /// <summary>
        /// 切换临近预览
        /// </summary>
        /// <param name="next">0上一个 非0下一个</param>
        private void SwitchNearPreview(int next)
        {
            if (Imgs.Count > 0)
            {
                bool isact = false;
                int nowprew = Imgs[selectedId];
                int lestprew = Imgs.Count - 1;

                if (next > 0 && nowprew < lestprew) //有下一个
                {
                    nowprew++;
                    isact = true;
                }
                else if (next < 1 && nowprew > 0)//有上一个
                {
                    nowprew--;
                    isact = true;
                }
                else if (next > 0 && nowprew == lestprew)//没有下一个时切到第1个
                {
                    nowprew = 0;
                    isact = true;
                }
                else if (next < 1 && nowprew == 0)//没有上一个时切到最后1个
                {
                    nowprew = lestprew;
                    isact = true;
                }

                if (isact)
                {
                    foreach (int id in Imgs.Keys)
                    {
                        if (Imgs[id] == nowprew)
                        {
                            nowprew = id;
                            break;
                        }
                    }
                    SwitchPreview(nowprew);
                }
            }
        }
        private void SwitchNearPreview()
        {
            SwitchNearPreview(0);
        }
        /// <summary>
        /// 用于修改PreviewWnd中的按钮文本
        /// </summary>
        public void ChangePreBtnText()
        {
            //判断当前浏览的预览图是否选中
            if (mainW.selected.Contains(oriIndex[selectedId]))
            {
                btnClick.ToolTip = "关闭该预览图并取消选中该图";
                btnClickText.Text = "取消选中并关闭(_A)";
            }
            else
            {
                btnClick.ToolTip = "关闭该预览图并选中该图";
                btnClickText.Text = "选中并关闭(_A)";
            }
        }

    }
}