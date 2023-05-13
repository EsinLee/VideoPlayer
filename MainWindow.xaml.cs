using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.WebRequestMethods;

namespace VideoPlayer
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        CefSharp.Wpf.ChromiumWebBrowser _browser;
        // 按鈕(鎖定視窗)可拖動外框改變尺寸狀態
        bool _isLocked = true;
        // 狀態紀錄(視窗可拖動)
        bool _canDragMove = true;
        // 狀態紀錄(視窗可改變大小)
        bool _canResize = true;
        // 狀態紀錄(視窗全螢幕)
        bool _isFullScreen = false;
        // 狀態紀錄(控制區顯示)
        bool _isShowControlPanel = false;
        // 狀態紀錄(曾為置頂狀態)
        bool _wasTopMost = false;
        // 狀態紀錄
        bool _wasCanResize = false;
        // 視窗比例狀態
        double windowRatio;
        // 視窗位置紀錄
        double originPosition_x;
        double originPosition_y;
        // 視窗尺寸紀錄(寬)
        double originWindowWidth;
        // 視窗尺寸紀錄(高)
        double originWindowHeight;
        // 網址暫存
        string _str;
        public MainWindow()
        {
            InitializeComponent();
            Initialize_custom();
        }// 自訂初始化
        private void Initialize_custom()
        {
            // 設定初始視窗大小
            windowRatio = (double)Btn_SetRatio_Stat.SIXTEEN_NINE;
            double sc_w = System.Windows.SystemParameters.PrimaryScreenWidth;
            double sc_h = System.Windows.SystemParameters.PrimaryScreenHeight;
            this.Width = sc_w / 3;
            this.Height = (sc_w / 3) * (double)Btn_SetRatio_Stat.SIXTEEN_NINE;
            // 設定視窗最小尺寸(640*360)
            this.MinWidth = 480;
            this.MinHeight = 270;
            // 設定視窗最大尺寸
            this.MaxWidth = sc_w;
            this.MaxHeight = sc_w * (double)Btn_SetRatio_Stat.SIXTEEN_NINE;
            // 設定視窗邊框為'無邊框'
            this.WindowStyle = WindowStyle.None;
            // 設定視窗初始位置為'螢幕中央'
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            // 創建網頁瀏覽器
            _browser = new CefSharp.Wpf.ChromiumWebBrowser();
            _browser.AddressChanged += Browser_AddressChanged;
            GridViewer.Children.Add(_browser);

            // 初始化置頂紀錄
            this.Topmost = true;
            // 更改置頂紀錄
            _wasTopMost = this.Topmost;
            // 初始化置頂按鈕UI(旋轉按鈕圖片)
            RotateTransform btn_rotateTransform = new RotateTransform(Topmost ? (int)Btn_Topmost_Angle.TOPMOST : (int)Btn_Topmost_Angle.NOT_TOPMOST);
            btn_mosttop.RenderTransform = btn_rotateTransform;
            // 初始化鎖定紀錄
            this._isLocked = true;
            // 更改視窗鎖定狀態
            this.ResizeMode = (ResizeMode)(_isLocked ? Btn_Lock_Stat.NORESIZE : Btn_Lock_Stat.CANRESIZE);
            // 初始預設控制元件為隱藏狀態
            gd_maincontroller_1.Visibility = Visibility.Hidden;
            gd_maincontroller_2.Visibility = Visibility.Hidden;

            // 更改瀏覽器網址
            if (this._browser != null)
            {
                _str = $"https://www.youtube-nocookie.com/embed/bn8GsyhciYc";
                _browser.Address = _str;
            }
            else
            {
                System.Windows.MessageBox.Show("Floating Winsdow Error.");
            }
        }
        // 按鈕事件-收放控制區
        private void Btn_Click_ControlThumb(object sender, RoutedEventArgs e)
        {
            // 更改狀態紀錄
            _isShowControlPanel = !_isShowControlPanel;
            // 控制區顯示控制(隱藏/顯示)
            ControlPanel_Visible(_isShowControlPanel);
            // 按鈕圖片旋轉
            RotateTransform btn_rotateTransform = new RotateTransform(_isShowControlPanel ? (int)Btn_ControlThumb_Angle.OPENED : (int)Btn_ControlThumb_Angle.CLOSED);
            btn_controlthumb.RenderTransform = btn_rotateTransform;
        }
        // Btn_ControlThumb按鈕圖片旋轉角度列舉
        private enum Btn_ControlThumb_Angle
        {
            OPENED = 315,
            CLOSED = 0
        }
        // 控制區顯示控制(隱藏/顯示)
        private void ControlPanel_Visible(bool show) 
        {
            if (show)
            {
                gd_maincontroller_1.Visibility = Visibility.Visible;
                gd_maincontroller_2.Visibility = Visibility.Visible;
            }
            else 
            {
                gd_maincontroller_1.Visibility = Visibility.Hidden;
                gd_maincontroller_2.Visibility = Visibility.Hidden;
            }
        }
        // 按鈕事件-置頂-將視窗鎖定在畫面最上層
        private void Btn_Click_Topmost(object sender, RoutedEventArgs e)
        {
            // 更改狀態紀錄
            this.Topmost = !this.Topmost;
            // 更改置頂紀錄
            _wasTopMost = this.Topmost;
            // 按鈕圖片旋轉
            //buttonImageAngle = Topmost ? (int)Btn_Topmost_Angle.TOPMOST : (int)Btn_Topmost_Angle.NOT_TOPMOST;
            RotateTransform btn_rotateTransform = new RotateTransform(Topmost ? (int)Btn_Topmost_Angle.TOPMOST : (int)Btn_Topmost_Angle.NOT_TOPMOST);
            btn_mosttop.RenderTransform = btn_rotateTransform;
        }
        // Btn_Topmost按鈕圖片旋轉角度列舉
        private enum Btn_Topmost_Angle
        {
            TOPMOST = 315,
            NOT_TOPMOST = 0
        }
        // 按鈕事件-將視窗尺寸拖動調整功能鎖定
        private void Btn_Click_Lock(object sender, RoutedEventArgs e)
        {
            if (_canResize)
            {
                // 更改狀態
                _isLocked = !_isLocked;
                // 隱藏式窗外框 並 鎖定位置(因已設定拖動鈕，此功能棄用)
                //this.WindowStyle = (WindowStyle)(_isLocked ? Btn_Lock_Stat.LOCKED : Btn_Lock_Stat.OPENED);
                // 鎖定尺寸拖動
                this.ResizeMode = (ResizeMode)(_isLocked ? Btn_Lock_Stat.NORESIZE : Btn_Lock_Stat.CANRESIZE);
            }
        }
        // 視窗鎖定狀態列舉
        private enum Btn_Lock_Stat
        {
            LOCKED = WindowStyle.None,
            OPENED = WindowStyle.SingleBorderWindow,
            CANRESIZE = ResizeMode.CanResize,
            NORESIZE = ResizeMode.NoResize
        }
        // 隱藏自訂控制元件(置頂及鎖定...等物件)
        private void MainGrid_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // 更改狀態紀錄
            _isShowControlPanel = true;
            // 控制區顯示控制(隱藏/顯示)
            ControlPanel_Visible(_isShowControlPanel);
            // 按鈕圖片旋轉
            RotateTransform btn_rotateTransform = new RotateTransform(_isShowControlPanel ? (int)Btn_ControlThumb_Angle.OPENED : (int)Btn_ControlThumb_Angle.CLOSED);
            btn_controlthumb.RenderTransform = btn_rotateTransform;
        }
        // 顯示自訂控制元件(置頂及鎖定...等物件)
        private void MainGrid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // 更改狀態紀錄
            _isShowControlPanel = false;
            // 控制區顯示控制(隱藏/顯示)
            ControlPanel_Visible(_isShowControlPanel);
            // 按鈕圖片旋轉
            RotateTransform btn_rotateTransform = new RotateTransform(_isShowControlPanel ? (int)Btn_ControlThumb_Angle.OPENED : (int)Btn_ControlThumb_Angle.CLOSED);
            btn_controlthumb.RenderTransform = btn_rotateTransform;
        }
        // 按鈕事件_按比例調整視窗尺寸
        private void Btn_Click_SetRatio(object sender, RoutedEventArgs e)
        {
            if (_canResize)
            {
                double window_size_width = this.Width;
                FrameworkElement sourceFrameworkElement = e.Source as FrameworkElement;
                switch (sourceFrameworkElement.Name)
                {
                    case "btn_ratio_16_9": // 按鈕(16:9)
                        windowRatio = (double)Btn_SetRatio_Stat.SIXTEEN_NINE;
                        break;
                    case "btn_ratio_16_10": // 按鈕(16:10)
                        windowRatio = (double)Btn_SetRatio_Stat.SIXTEEN_TEN;
                        break;
                }
                // 依據寬度調整高度
                this.Height = window_size_width * windowRatio;
                e.Handled = true;
            }
        }
        // 視窗比例設定狀態列舉
        public struct Btn_SetRatio_Stat
        {
            public const double SIXTEEN_NINE = 0.5625; // 視窗尺寸9:16
            public const double SIXTEEN_TEN = 0.625;// 視窗尺寸10:16
        }
        // 宣告視窗移動相關變數
        Point _startPoint;
        bool _isDragging = false;
        // 按鈕事件-視窗拖動(3 funtions- 1.Btn_Mouse_PreviewMouseLeftButtonDown 2.OnPreviewMouseMove 3.OnPreviewMouseLeftButtonUp)
        private void Btn_Mouse_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.Capture(this) && _canDragMove)
            {
                _isDragging = true;
                _startPoint = PointToScreen(Mouse.GetPosition(this));
            }
        }
        protected override void OnPreviewMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            if (_isDragging && _canDragMove)
            {
                Point newPoint = PointToScreen(Mouse.GetPosition(this));
                int diffX = (int)(newPoint.X - _startPoint.X);
                int diffY = (int)(newPoint.Y - _startPoint.Y);
                if (Math.Abs(diffX) > 1 || Math.Abs(diffY) > 1)
                {
                    Left += diffX;
                    Top += diffY;
                    InvalidateVisual();
                    _startPoint = newPoint;
                }
            }
        }
        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (_isDragging && _canDragMove)
            {
                _isDragging = false;
                Mouse.Capture(null);
            }
        }
        // 按鈕事件-依視窗尺寸設定視窗大小
        private void Btn_Click_SetSize(object sender, RoutedEventArgs e)
        {
            if (_canResize)
            {
                // 螢幕尺寸
                double screen_size_width = System.Windows.SystemParameters.PrimaryScreenWidth;
                double divisor = 4.0;

                FrameworkElement sourceFrameworkElement = e.Source as FrameworkElement;
                switch (sourceFrameworkElement.Name)
                {
                    case "btn_size_1_2": // 設定尺寸1/2按鈕
                        divisor = 2.0;
                        break;
                    case "btn_size_1_3": // 設定尺寸1/3按鈕
                        divisor = 3.0;
                        break;
                    case "btn_size_1_4": // 設定尺寸1/4按鈕
                        divisor = 4.0;
                        break;
                }
                this.Width = screen_size_width / divisor;
                this.Height = screen_size_width / divisor * windowRatio;
                e.Handled = true;
            }
        }
        // 按鈕事件-視窗尺寸設定全螢幕
        private void Btn_Click_SetFullScreen(object sender, RoutedEventArgs e)
        {
            if (_isFullScreen) // 解除全螢幕狀態
            {
                //視窗尺寸復原
                this.Height = originWindowHeight;
                this.Width = originWindowWidth;
                // 視窗位置復原
                this.Left = originPosition_x;
                this.Top = originPosition_y;
                // 視窗設為'可拖動'
                _canDragMove = true;
                // 紀錄當前狀態為'非全螢幕'
                _isFullScreen = false;
                // 設定當前可用按鈕調整大小
                _canResize = true;
                // 停用拖動按鈕(不停用會有Bug)
                btn_dragmove.IsEnabled = true;
                // 更改置頂狀態(避免因Topmost未解除卡住)-以_wasTopMost判定 -> 若最大化前本就是非置頂狀態，則不更改
                if (_wasTopMost)
                {
                    // 更改置頂狀態(false -> true)
                    this.Topmost = !this.Topmost;
                    // 按鈕圖片旋轉
                    //buttonImageAngle = Topmost ? (int)Btn_Topmost_Angle.TOPMOST : (int)Btn_Topmost_Angle.NOT_TOPMOST;
                    RotateTransform btn_rotateTransform = new RotateTransform(Topmost ? (int)Btn_Topmost_Angle.TOPMOST : (int)Btn_Topmost_Angle.NOT_TOPMOST);
                    btn_mosttop.RenderTransform = btn_rotateTransform;
                }
                // 鎖定尺寸拖動(避免全螢幕時拖動到改變尺寸)
                if (_isLocked == false)
                {
                    // 鎖定尺寸拖動(此處必為解鎖)
                    this.ResizeMode = (ResizeMode)(_isLocked ? Btn_Lock_Stat.NORESIZE : Btn_Lock_Stat.CANRESIZE);
                }
            }
            else // 進入全螢幕狀態
            {
                // 紀錄視窗尺寸
                originWindowHeight = this.Height;
                originWindowWidth = this.Width;
                // 紀錄視窗位置
                originPosition_x = this.Left;
                originPosition_y = this.Top;

                // 取得螢幕尺寸
                double screen_size_width = System.Windows.SystemParameters.PrimaryScreenWidth;
                double screen_size_height = System.Windows.SystemParameters.PrimaryScreenHeight;
                // 數值設定
                this.Width = screen_size_width;
                this.Height = screen_size_height;
                // 初始位置設為0,0
                this.WindowStartupLocation = WindowStartupLocation.Manual;
                this.Left = 0;
                this.Top = 0;
                // 視窗設為'不可拖動'
                _canDragMove = false;
                // 紀錄當前狀態為'全螢幕'
                _isFullScreen = true;
                // 設定當前不可用按鈕調整大小
                _canResize = false;
                // 啟用拖動按鈕(不停用會有Bug)
                btn_dragmove.IsEnabled = false;
                // 更改置頂狀態(避免因Topmost未解除卡住) -> 若最大化前本就是非置頂狀態，則不更改
                if (this.Topmost)
                {
                    // 更改置頂狀態(此處必定為true -> false)
                    this.Topmost = !this.Topmost;
                    // 按鈕圖片旋轉
                    //buttonImageAngle = Topmost ? (int)Btn_Topmost_Angle.TOPMOST : (int)Btn_Topmost_Angle.NOT_TOPMOST;
                    RotateTransform btn_rotateTransform = new RotateTransform(Topmost ? (int)Btn_Topmost_Angle.TOPMOST : (int)Btn_Topmost_Angle.NOT_TOPMOST);
                    btn_mosttop.RenderTransform = btn_rotateTransform;
                }
                // 鎖定尺寸拖動(避免全螢幕時拖動到改變尺寸)-(註:此處不改變_isLocked狀態，以利退出全螢幕時可恢復先前設定)
                if (_isLocked == false)
                {
                    // 鎖定尺寸拖動
                    this.ResizeMode = ResizeMode.NoResize;
                }
            }
        }
        // 按鈕事件-關閉視窗
        private void Btn_Click_Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        } // 按鈕事件-Update網頁
        private void Btn_Click_Update(object sender, RoutedEventArgs e)
        {
            if (txb_videourl.Text.Contains(@".html"))
            {
                _browser.Load(txb_videourl.Text);
            }
            else
            {
                _browser.Address = txb_videourl.Text;
            }
        }
        // Bool-確認Youbtube網址正確並解析出影片id
        private bool Bool_CheckAndEditUrl()
        {
            try
            {
                _str = txb_videourl.Text.Split('=')[1].Split('&')[0];
                return true;
            }
            catch
            {
                //System.Windows.MessageBox.Show("Unknow video address.");
                return false;
            }
        }
        // 監聽網址變化
        private void Browser_AddressChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            string url = e.NewValue as string;
            //txtbox_url.Invoke(new Action(() => txtbox_url.Text = url));
            txb_videourl.Text = url.Trim();

            // 浮動式視窗
            if (Bool_CheckAndEditUrl() && this._browser != null && _str.Length > 9)
            {
                txb_videourl.Text = $"https://www.youtube-nocookie.com/embed/{_str}";
            }
        }
        
    }
}
