/***

   Copyright (C) 2019. rollrat. All Rights Reserved.
   
   Author: HyunJun Jeong

***/

using kakaotalk_analyzer.Core;
using MaterialDesignThemes.Wpf;
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

namespace kakaotalk_analyzer.Dialog
{
    /// <summary>
    /// LoadingDialog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoadingDialog : UserControl
    {
        string filename;
        public LoadingDialog(string filename)
        {
            InitializeComponent();

            this.filename = filename;
        }

        bool loaded = false;
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (loaded) return;
            loaded = true;

            Task.Run(() =>
            {
                bool err = false;
                TalkInstance.Instance.Open(filename);
                Extends.Post(() => {
                    Message.Text = TalkInstance.Instance.Title;
                    Message2.Text = "대화 분석 중 입니다...";
                    Message2.Visibility = Visibility.Visible;
                });
                try
                {
                    TalkInstance.Instance.Parse();
                    TalkInstance.Instance.Manager.ExtractMember();
                }
                catch (Exception ex)
                {
                    Monitor.Instance.Save();
                    err = true;
                }
                Extends.Post(() => MainWindow.Instance.CloseDialog(!err));
            });
        }
    }
}
