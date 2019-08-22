/***

   Copyright (C) 2019. rollrat. All Rights Reserved.
   
   Author: HyunJun Jeong

***/

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
    /// CorrectDialog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CorrectDialog : UserControl
    {
        public CorrectDialog(string title)
        {
            InitializeComponent();

            Message1.Text = title;
        }
    }
}
