/***

   Copyright (C) 2019. rollrat. All Rights Reserved.
   
   Author: HyunJun Jeong

***/

using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Defaults;
using LiveCharts.Definitions.Series;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

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
            
            Chart.Series = Series = refresh();
            Formatter = value => $"{(int)value / 12}.{((int)value % 12).ToString("00")}";
            DataContext = this;
        }

        public Func<double, string> Formatter { get; set; }
        public SeriesCollection Series { get; set; }

        private SeriesCollection refresh()
        {
            var Series = new SeriesCollection();
            Series.Clear();

            var ll = TalkInstance.Instance.Manager.DateWords.ToList().Select(x =>
                new Tuple<int, List<Tuple<string, int>>>(x.Key, x.Value.Select(y => new Tuple<string, int>(y.Key, y.Value)).ToList())).ToList();
            var sval = (int)Slider.Value;
            foreach (var lls in ll)
            {
                lls.Item2.Sort((x, y) => y.Item2.CompareTo(x.Item2));
                if (lls.Item2.Count > sval)
                    lls.Item2.RemoveRange(sval, lls.Item2.Count - sval);
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
                    PointGeometrySize = 10,
                    //DataLabels = true,
                    //LabelPoint = x => pp.Key,
                };
            }));
            
            var mindate = 999999;

            foreach (var lls in ll)
                if (mindate > lls.Item1)
                    mindate = lls.Item1;
            AxisX.MinValue = (mindate / 100 * 12) + (mindate % 100);
            //Chart.Series = Series;
            return Series;
        }

        DispatcherTimer delete_timer;
        object update_lock = new object();
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!loaded) return;

            if (delete_timer != null)
                delete_timer.Stop();

            //using (var d = Dispatcher.DisableProcessing())
            lock (update_lock)
            {
                var new_series = refresh().ToList();

                // Remove unavailable series.
                var ava_series = new HashSet<string>();
                new_series.ForEach(x => ava_series.Add(x.Title));
                var remove_series = new List<ISeriesView>();
                Series.Where(x => !ava_series.Contains(x.Title)).ToList().ForEach(x => remove_series.Add(x));
                remove_series.ForEach(x => x.Values.Clear());
                
                // Insert available series but not contains previous SeriesCollection
                var all_series = new HashSet<string>();
                Series.ToList().ForEach(x => all_series.Add(x.Title));
                new_series.Where(x => !all_series.Contains(x.Title)).ToList().ForEach(x => Series.Add(x));

                // Adjust series values
                var item_dict = new Dictionary<string, ISeriesView>();
                new_series.ForEach(x => item_dict.Add(x.Title, x));
                Series.Where(x => ava_series.Contains(x.Title) && all_series.Contains(x.Title)).ToList().ForEach(x =>
                {
                    var item = item_dict[x.Title];
                    var item_cnt = item.Values.Count;

                    for (int i = 0, j = 0; i < item_cnt && j < x.Values.Count; )
                    {
                        var i1 = ((ScatterPoint)item.Values[i]).X;
                        var i2 = ((ScatterPoint)x.Values[j]).X;

                        if (i1 < i2)
                        {
                            x.Values.RemoveAt(j);
                        }
                        else if (i1 > i2)
                        {
                            x.Values.Insert(j, item.Values[i]);
                            j++;
                            i++;
                        }
                        else if (i1 == i2)
                        {
                            i++;
                            j++;
                        }
                    }

                    if (item_cnt < x.Values.Count)
                    {
                        for (int i = item_cnt; i < x.Values.Count;)
                        {
                            x.Values.RemoveAt(i);
                        }
                    }
                    else if (item_cnt > x.Values.Count)
                    {
                        for (int i = x.Values.Count; i < item_cnt; i++)
                            x.Values.Add(item.Values[i]);
                    }
                });

                Chart.Series = Series;
                
                delete_timer = new DispatcherTimer();
                delete_timer.Tick += new EventHandler((obj, args) => {
                    var remove_series1 = new List<ISeriesView>();
                    Series.Where(x => x.Values.Count == 0).ToList().ForEach(x => remove_series1.Add(x));
                    remove_series1.ForEach(x => Series.Remove(x));
                    delete_timer.Stop();
                });
                delete_timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
                delete_timer.Start();
            }
        }
    }
}
