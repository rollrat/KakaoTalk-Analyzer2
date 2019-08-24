/***

   Copyright (C) 2019. rollrat. All Rights Reserved.
   
   Author: HyunJun Jeong

***/

using kakaotalk_analyzer.Core;
using kakaotalk_analyzer.Model;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
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
using System.Windows.Shapes;

namespace kakaotalk_analyzer
{
    /// <summary>
    /// Member.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Member : Window
    {
        int id;
        public Member(int id)
        {
            InitializeComponent();

            this.id = id;

            Title += TalkInstance.Instance.Manager.Members[id].Name;

            TalkList.DataContext = new TalkListViewModel();
            TalkList.Sorting += new DataGridSortingEventHandler(new DataGridSorter<TalkListItemViewModel>(TalkList).SortHandler);
            TalkList.VerticalGridLinesBrush = TalkList.HorizontalGridLinesBrush;

            Loaded += Member_Loaded;
        }
        
        public string[] Labels { get; set; }
        public string[] Labels2 { get; set; }
        public Func<double, string> Formatter { get; set; }

        bool loaded = false;
        private void Member_Loaded(object sender, RoutedEventArgs e)
        {
            if (loaded) return;
            loaded = true;

            var type = new string[] { "메시지", "", "들어옴", "나감", "오류" };
            var talklistdc = TalkList.DataContext as TalkListViewModel;
            foreach (var talk in TalkInstance.Instance.Manager.Members[id].Talks)
            {
                talklistdc.Items.Add(new TalkListItemViewModel
                {
                    인덱스 = talk.Index.ToString(),
                    유형 = type[(int)talk.State],
                    내용 = talk.Content ?? "",
                    작성자 = talk.State == TalkState.Append ? "" : talk.Name ?? "",
                    날짜 = talk.State != TalkState.Append && talk.Time != new DateTime() ? talk.Time.ToString() : "",
                    Talk = talk
                });
            }

            var Series = new SeriesCollection();
            var week = new int[7];

            foreach (var talk in TalkInstance.Instance.Manager.Members[id].Talks)
            {
                if (talk.State == TalkState.Message)
                    week[(int)talk.Time.DayOfWeek]++;
            }

            Series.Add(new LineSeries
            {
                Title = "",
                Values = new ChartValues<int>(week),
                Fill = new SolidColorBrush
                {
                    Color = Colors.Pink,
                    Opacity = .4
                },
                Stroke = Brushes.Pink,
                LineSmoothness = 0.7,
                PointGeometrySize = 10,
            });
            
            Labels = "일월화수목금토".Select(x => x. ToString()).ToArray();
            WeeklyChart.Series = Series;

            var Series2 = new SeriesCollection();
            var day = new int[24];

            foreach (var talk in TalkInstance.Instance.Manager.Members[id].Talks)
            {
                if (talk.State == TalkState.Message)
                    day[talk.Time.Hour]++;
            }

            Series2.Add(new LineSeries
            {
                Title = "",
                Values = new ChartValues<int>(day),
                Fill = new SolidColorBrush
                {
                    Color = Colors.Pink,
                    Opacity = .4
                },
                Stroke = Brushes.Pink,
                LineSmoothness = 0.7,
                PointGeometrySize = 10,
            });

            Labels2 = Enumerable.Range(0, 24).Select(x => x.ToString()).ToArray();
            DailyChart.Series = Series2;

            DataContext = this;

            Task.Run(() =>
            {
                var name = TalkInstance.Instance.Manager.Members[id].Name;
                TalkInstance.Instance.Manager.StartSpecificMessageAnalyze(x => x.Name == name, x => { }, true, false, false, false);
                var ll = TalkInstance.Instance.Manager.Words.ToList();
                ll.Sort((x, y) => y.Value.CompareTo(x.Value));

                if (ll.Count > 5)
                    ll.RemoveRange(5, ll.Count - 5);

                Extends.Post(() =>
                {
                    KeywordCharts.Series = new SeriesCollection();
                    KeywordCharts.Series.AddRange(ll.Select(x =>
                    {
                        return new PieSeries
                        {
                            Title = "", //x.Key,
                            Values = new ChartValues<int>(new int[] { x.Value }),
                            DataLabels = true,
                            LabelPoint = y => x.Key,
                        };
                    }));
                    Progress.Visibility = Visibility.Collapsed;
                    KeywordCharts.Visibility = Visibility.Visible;
                });
            });
        }
    }
}
