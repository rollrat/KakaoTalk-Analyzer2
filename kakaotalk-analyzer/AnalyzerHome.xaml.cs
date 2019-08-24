/***

   Copyright (C) 2019. rollrat. All Rights Reserved.
   
   Author: HyunJun Jeong

***/

using kakaotalk_analyzer.Core;
using kakaotalk_analyzer.Model;
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
    /// AnalyzerHome.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AnalyzerHome : Window
    {
        public AnalyzerHome()
        {
            InitializeComponent();

            Title += TalkInstance.Instance.Title;

            TalkList.DataContext = new TalkListViewModel();
            TalkList.Sorting += new DataGridSortingEventHandler(new DataGridSorter<TalkListItemViewModel>(TalkList).SortHandler);
            TalkList.VerticalGridLinesBrush = TalkList.HorizontalGridLinesBrush;

            MemberList.DataContext = new MemberListViewModel();
            MemberList.Sorting += new DataGridSortingEventHandler(new DataGridSorter<MemberListItemViewModel>(MemberList).SortHandler);
            MemberList.VerticalGridLinesBrush = MemberList.HorizontalGridLinesBrush;

            KeywordList.DataContext = new KeywordListViewModel();
            KeywordList.Sorting += new DataGridSortingEventHandler(new DataGridSorter<KeywordListItemViewModel>(KeywordList).SortHandler);
            KeywordList.VerticalGridLinesBrush = KeywordList.HorizontalGridLinesBrush;
        }

        bool loaded = false;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (loaded) return;
            loaded = true;

            L1.Text = TalkInstance.Instance.Manager.GetExactTalksCount().ToString("#,0");
            L2.Text = TalkInstance.Instance.Manager.Members.Count.ToString("#,0");

            var week = new int[7];
            var day = new int[5];

            var day_day = new int[24] {
                0, 0, 0, 0, 0,          // 0 ~ 5
                1, 1, 1, 1,             // 5 ~ 9
                2, 2, 2, 2, 2, 2, 2, 2, // 9 ~ 17
                3, 3, 3, 3,             // 17 ~ 21
                4, 4, 4,                // 21 ~ 24 
            };
            TalkInstance.Instance.Manager.Talks.ForEach(x =>
            {
                if (x.State == TalkState.Message || x.State == TalkState.Append)
                {
                    week[(int)x.Time.DayOfWeek]++;
                    day[day_day[x.Time.Hour]]++;
                }
            });

            var range = TalkInstance.Instance.Manager.LastAvailableTalksTime() - TalkInstance.Instance.Manager.FirstAvailableTalksTime();

            L3.Text = (week.Sum() / (range.TotalDays)).ToString("#,0.0");
            L4.Text = ((week[1] + week[2] + week[3] + week[4] + week[5]) / (range.TotalDays / 7 * 5)).ToString("#,0.0");
            L5.Text = ((week[0] + week[6]) / (range.TotalDays / 7 * 2)).ToString("#,0.0");

            var format = new string[] { "새벽", "아침", "낮", "저녁", "밤" };
            var formatt = new string[] { "(0시 ~ 5시)", "(5시 ~ 9시)", "(9시 ~ 17시)", "(17시 ~ 21시)", "(21시 ~ 24시)" };
            L6.Text = format[day.ToList().IndexOf(day.Max())];
            L7.Text = formatt[day.ToList().IndexOf(day.Max())];

            L8.Text = TalkInstance.Instance.Manager.Members.OrderByDescending(x => x.Talks.Where(y => y.State == TalkState.Message).Count()).First().Name;
            L9.Text = "(" + TalkInstance.Instance.Manager.Members.Max(x => x.Talks.Where(y => y.State == TalkState.Message).Count()).ToString("#,0") + " 개)";

            var total_length = TalkInstance.Instance.Manager.Talks.Where(x => x.State == TalkState.Append || x.State == TalkState.Message).Select(x => x.Content.Length).Sum();
            L10.Text = total_length.ToString("#,0");
            L11.Text = (total_length / (double)TalkInstance.Instance.Manager.Talks.Where(x => x.State == TalkState.Message).Count()).ToString("#,0.##");

            var type = new string[] { "메시지", "", "들어옴", "나감", "오류" };
            var talklistdc = TalkList.DataContext as TalkListViewModel;
            foreach (var talk in TalkInstance.Instance.Manager.Talks)
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

            var memberlistdc = MemberList.DataContext as MemberListViewModel;
            foreach (var member in TalkInstance.Instance.Manager.Members)
            {
                var y = member.Talks.Where(x => x.State == TalkState.Message).Count();
                var ft = "";
                var lt = "";
                var atl = "";

                if (y > 0)
                {
                    ft = member.Talks.First(x => x.State == TalkState.Message).Time.ToString();
                    lt = member.Talks.Last(x => x.State == TalkState.Message).Time.ToString();
                    atl = (member.Talks.Where(x => x.State == TalkState.Append || x.State == TalkState.Message).Select(x => x.Content.Length).Sum() / (double)y).ToString("#,0.##");
                }

                memberlistdc.Items.Add(new MemberListItemViewModel
                {
                    아이디 = member.Id.ToString(),
                    이름 = member.Name,
                    대화수 = y.ToString("#,0"),
                    평균대화길이 = atl,
                    첫번째대화 = ft,
                    마지막대화 = lt,
                });
            }
            
            MainWindow.Instance.Close();
        }
        
        private void TalkList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void MemberList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void keyword_control(bool isenabled)
        {
            KeywordButton.IsEnabled = isenabled;
            KeywordButton2.IsEnabled = isenabled;
            WAOption1.IsEnabled = isenabled;
            WAOption2.IsEnabled = isenabled;
            WAOption3.IsEnabled = isenabled;
            WAOption4.IsEnabled = isenabled;
            WAOption5.IsEnabled = isenabled;
        }

        private async void KeywordButton_Click(object sender, RoutedEventArgs e)
        {
            keyword_control(false);

            var keywordlistdc = KeywordList.DataContext as KeywordListViewModel;
            keywordlistdc.Items.Clear();

            Progress.Maximum = TalkInstance.Instance.Manager.Talks.Count;

            var op1 = WAOption1.IsChecked.Value;
            var op3 = WAOption3.IsChecked.Value;
            var op4 = WAOption4.IsChecked.Value;
            var op5 = WAOption5.IsChecked.Value;

            await Task.Run(() =>
            {
                TalkInstance.Instance.Manager.StartAllMessageAnalyze(x => 
                {
                    Extends.Post(() =>
                    {
                        Progress.Value = x;
                        ProgressLabel.Text = $"[{Progress.Value.ToString("#,0")}/{Progress.Maximum.ToString("#,0")}]";
                    });
                }, op1, op5, op3, op4);
            });


            var ll = TalkInstance.Instance.Manager.Words.ToList();
            ll.Sort((x, y) => y.Value.CompareTo(x.Value));

            for (int i = 0; i < ll.Count; i++)
            {
                keywordlistdc.Items.Add(new KeywordListItemViewModel
                {
                    순위 = (i+1).ToString(),
                    키워드 = ll[i].Key,
                    개수 = ll[i].Value.ToString("#,0"),
                });
            }

            keyword_control(true);
        }

        private async void KeywordButton2_Click(object sender, RoutedEventArgs e)
        {
            keyword_control(false);

            Progress.Maximum = TalkInstance.Instance.Manager.Talks.Count;

            var op1 = WAOption1.IsChecked.Value;
            var op3 = WAOption3.IsChecked.Value;
            var op4 = WAOption4.IsChecked.Value;
            var op5 = WAOption5.IsChecked.Value;

            await Task.Run(() =>
            {
                TalkInstance.Instance.Manager.StartAllMessageAnalyzeByDate(x =>
                {
                    Extends.Post(() =>
                    {
                        Progress.Value = x;
                        ProgressLabel.Text = $"[{Progress.Value.ToString("#,0")}/{Progress.Maximum.ToString("#,0")}]";
                    });
                }, op1, op5, op3, op4);
            });

            (new MonthlyKeywords()).Show();

            keyword_control(true);
        }
    }
}
