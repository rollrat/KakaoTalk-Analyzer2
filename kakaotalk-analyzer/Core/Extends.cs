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
using System.Windows.Threading;

namespace kakaotalk_analyzer.Core
{
    public static class Extends
    {
        public static int ToInt(this string str) => Convert.ToInt32(str);
        
        public static void Post(Action acc) =>
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
               new Action(() => acc()));
    }
}
