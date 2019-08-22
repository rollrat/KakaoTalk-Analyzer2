/***

   Copyright (C) 2019. rollrat. All Rights Reserved.
   
   Author: HyunJun Jeong

***/

using kakaotalk_analyzer.Core;
using kakaotalk_analyzer.Dialog;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
using System.Windows.Threading;

namespace kakaotalk_analyzer
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance;

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;

            Monitor.Instance.controlEnable = false;
#if false
            Monitor.Instance.controlEnable = true;
            Monitor.Instance.Push("Hello!");
            Monitor.Instance.Start();
#endif
        }

        private async void Window_Drop(object sender, DragEventArgs e)
        {
            if (drag_enter)
            {
                RootDialogHost.IsOpen = false;

                if (correct_drop)
                {
                    load_enter = true;
                    await DialogHost.Show(new LoadingDialog(((string[])e.Data.GetData(DataFormats.FileDrop))[0]));
                    if (last_status == true)
                    {
                        (new AnalyzerHome()).Show();
                    }
                    else
                    {
                        await DialogHost.Show(new ErrorDialog());
                    }
                    load_enter = false;
                }

                correct_drop = false;
            }
        }

        bool last_status;
        public void CloseDialog(bool status)
        {
            RootDialogHost.IsOpen = false;
            last_status = status;
        }

        bool load_enter = false;
        bool drag_enter = false;
        bool correct_drop = false;
        DispatcherTimer drag_timer;
        private async void Window_DragEnter(object sender, DragEventArgs e)
        {
            if (drag_enter || load_enter) return;
            drag_enter = true;
            load_enter = false;
            correct_drop = false;

            drag_timer = new DispatcherTimer();
            drag_timer.Tick += new EventHandler(dispatcherTimer_Tick);
            drag_timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            drag_timer.Start();

            object msg = null;

            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                msg = new ForbiddenDialog("대화목록 파일을 끌어오세요!");
            }
            else
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files.Length != 1)
                {
                    msg = new ForbiddenDialog("하나의 파일만 끌어오세요!");
                }
                else if (Path.GetExtension(files[0]) != ".txt")
                {
                    msg = new ForbiddenDialog("옳바른 파일형식이 아닙니다!");
                }
                else
                {
                    var stream = new StreamReader(files[0]);
                    var firstline = stream.ReadLine();
                    if (!firstline.Contains("님과 카카오톡 대화"))
                    {
                        msg = new ForbiddenDialog("카카오톡 대화형식 파일이 아닙니다!");
                    }
                    else
                    {
                        msg = new CorrectDialog(firstline.Replace(" 님과 카카오톡 대화", ""));
                        correct_drop = true;
                    }
                    stream.Close();
                }
            }

            await DialogHost.Show(msg, "Dialog", (object s, DialogClosingEventArgs x) =>
            {
                x.Session.Close(false);
            });
            drag_enter = false;
            drag_timer.Stop();
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };
        public static Point GetMousePosition()
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            var pp = PointFromScreen(GetMousePosition());

            if ((pp.X < 0 || pp.Y < 0 || pp.X > Width || pp.Y > Height) && drag_enter)
            {
                RootDialogHost.IsOpen = false;
                drag_timer.Stop();
            }
        }
    }
}
