/***

   Copyright (C) 2019. rollrat. All Rights Reserved.
   
   Author: HyunJun Jeong

***/

using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace kakaotalk_analyzer
{
    /// <summary>
    /// MonthlyKeywords.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MonthlyKeywords : Window
    {
        public MonthlyKeywords()
        {
            InitializeComponent();

            Loaded += MonthlyKeywords_Loaded;
        }
        
        bool loaded = false;
        private void MonthlyKeywords_Loaded(object sender, RoutedEventArgs e)
        {
            if (loaded) return;
            loaded = true;
            
            var dayConfig = Mappers.Xy<ScatterPoint>()
                .X(dayModel => dayModel.X)
                .Y(dayModel => dayModel.Y);

            Series = new SeriesCollection(dayConfig);
            Series.Clear();
            
            var ll = TalkInstance.Instance.Manager.DateWords.ToList().Select(x => 
                new Tuple<int, List<Tuple<string, int>>>(x.Key, x.Value.Select(y => new Tuple<string, int>(y.Key, y.Value)).ToList())).ToList();
            foreach (var lls in ll)
            {
                lls.Item2.Sort((x, y) => y.Item2.CompareTo(x.Item2));
                if (lls.Item2.Count > 30)
                    lls.Item2.RemoveRange(30, lls.Item2.Count - 30);
            }
            ll.Sort((x, y) => x.Item1.CompareTo(y.Item1));

            var coord = new Dictionary<string, List<Tuple<int, int>>>();
            foreach (var lls in ll)
            {
                foreach (var llss in lls.Item2)
                {
                    if (!coord.ContainsKey(llss.Item1))
                        coord.Add(llss.Item1, new List<Tuple<int, int>>());
                    coord[llss.Item1].Add(new Tuple<int, int>(lls.Item1, llss.Item2));
                }
            }

            Series.AddRange(coord.Select(pp =>
            {
                Random rm = new Random(pp.Key.GetHashCode());
                return new LineSeries
                {
                    Title = pp.Key,
                    Values = new ChartValues<ScatterPoint>(
                        pp.Value.Select(y => new ScatterPoint((y.Item1 / 100 * 12) + (y.Item1 % 100), y.Item2))),
                    Fill = Brushes.Transparent,
                    Stroke = new SolidColorBrush(Color.FromRgb((byte)rm.Next(256), (byte)rm.Next(256), (byte)rm.Next(256))),
                    LineSmoothness = 0.7,
                    PointGeometrySize = 10
                };
            }));

            var mindate = 999999;

            foreach (var lls in ll)
                if (mindate > lls.Item1)
                    mindate = lls.Item1;

            AxisX.MinValue = (mindate / 100 * 12) + (mindate % 100);
            Formatter = value => $"{(int)value / 12}.{((int)value % 12).ToString("00")}";
            DataContext = this;
        }

        public Func<double, string> Formatter { get; set; }
        public SeriesCollection Series { get; set; }
    }
}
