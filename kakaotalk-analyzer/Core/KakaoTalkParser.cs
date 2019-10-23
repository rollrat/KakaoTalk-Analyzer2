/***

   Copyright (C) 2019. rollrat. All Rights Reserved.
   
   Author: HyunJun Jeong

***/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace kakaotalk_analyzer.Core
{
    public enum TalkState
    {
        Message = 0,
        Append = 1,
        Enter = 2,
        Leave = 3,
        Error = 4,
    }

    public class Talk
    {
        public TalkState State { get; set; }
        public int Index { get; set; }
        public DateTime Time { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
    }
    
    public class KakaoTalkParser
    {
        public KakaoTalkParser(string filename)
        {
            lines = File.ReadAllLines(filename);
            Title = lines[0].Replace(" 님과 카카오톡 대화", "");
        }

        public string Title { get; }

        string[] lines;

        public List<Talk> Talks { get; set; }

        public void ParseStart()
        {
            Talks = new List<Talk>();
            var index_count = 1;
            var current_year = 0;
            var current_month = 0;
            var current_day = 0;
            var current_name = "";

            var latest_time = DateTime.Now;

            var date_regex = new Regex(@"--------------- (\d+)년 (\d+)월 (\d+)일 \w+ ---------------");
            var message_regex = new Regex(@"\s*\[(\w+) (\d+)\:(\d+)\]([\w\W]+)");

            var invite_regex = new Regex("(.*?)님이 (.*?)님을 초대하였습니다.");
            var in_regex = new Regex("(.*?)님이 들어왔습니다.");
            var out_regex = new Regex("(.*?)님이 나갔습니다.");
            var ban_regex = new Regex("(.*?)님을 내보냈습니다.");

            for (int i = 3; i < lines.Length; i++, index_count++)
            {
                var line = lines[i];
                try
                {
                    if (line.StartsWith("["))
                    {
                        try
                        {
                            var builder = new StringBuilder();
                            var ptr = 1;

                            // Name
                            var deep = 0;
                            for (; ptr < line.Length; ptr++)
                            {
                                if (line[ptr] == '[')
                                    deep++;
                                else if (line[ptr] == ']')
                                {
                                    if (deep == 0)
                                        break;
                                    deep--;
                                }
                                builder.Append(line[ptr]);
                            }

                            var tt = reg(line.Substring(ptr + 1), message_regex);

                            var time = tt[1].ToInt();

                            if (tt[0] == "오후")
                            {
                                if (time != 12)
                                    time += 12;
                            }
                            else if (time == 12)
                                time = 0;

                            latest_time = new DateTime(current_year, current_month, current_day, time, tt[2].ToInt(), 0);
                            Talks.Add(new Talk
                            {
                                State = TalkState.Message,
                                Index = index_count,
                                Name = builder.ToString(),
                                Content = tt[3].Trim(),
                                Time = latest_time
                            });
                            current_name = builder.ToString();
                        }
                        catch (Exception e)
                        {
                            if (Talks.Last().State == TalkState.Message || Talks.Last().State == TalkState.Append)
                                Talks.Add(new Talk { State = TalkState.Append, Index = index_count, Content = line, Name = current_name, Time = latest_time });
                            else
                                throw e;
                        }
                    }
                    else if(line.StartsWith("---------------") && date_regex.Match(line).Success)
                    {
                        var dt = reg(line, date_regex);
                        current_year = dt[0].ToInt();
                        current_month = dt[1].ToInt();
                        current_day = dt[2].ToInt();
                    }
                    else
                    {
                        if (line.Contains("님을 초대하였습니다"))
                        {
                            var pp = reg(line, invite_regex);
                            if (pp[1].Contains(','))
                            {
                                pp[1].Split(',').ToList().ForEach(x => {
                                    if (x.Trim().EndsWith("님"))
                                        Talks.Add(new Talk { Index = index_count, State = TalkState.Enter, Name = x.Remove(x.Length - 1, 1).Trim() });
                                    else
                                        Talks.Add(new Talk { Index = index_count, State = TalkState.Enter, Name = x.Trim() });
                                });
                            }
                            else
                                Talks.Add(new Talk { Index = index_count, State = TalkState.Enter, Name = pp[1] });
                        }
                        else if (line.Contains("님이 들어왔습니다."))
                        {
                            var pp = reg(line, in_regex);
                            Talks.Add(new Talk { Index = index_count, State = TalkState.Enter, Name = pp[0] });
                        }
                        else if (line.Contains("님이 나갔습니다."))
                        {
                            var pp = reg(line, out_regex);
                            Talks.Add(new Talk { Index = index_count, State = TalkState.Leave, Name = pp[0] });
                        }
                        else if (line.Contains("님을 내보냈습니다."))
                        {
                            var pp = reg(line, ban_regex);
                            Talks.Add(new Talk { Index = index_count, State = TalkState.Leave, Name = pp[0] });
                        }
                        else if (line.Contains("채팅방 관리자가 메시지를 가렸습니다."))
                        {
                            // Nothing
                        }
                        else
                        {
                            if (Talks.Last().State == TalkState.Message || Talks.Last().State == TalkState.Append)
                                Talks.Add(new Talk { State = TalkState.Append, Index = index_count, Content = line, Name = current_name, Time = latest_time });
                            else
                                throw new Exception();
                        }
                    }
                }
                catch (Exception e)
                {
                    Talks.Add(new Talk { State = TalkState.Error, Index = index_count, Content = e.Message });
                    Monitor.Instance.Push("[Kakao] Message Parsing Error! Target=" + line + " Msg=" + e.Message + "\r\n" + e.StackTrace);
                    throw e;
                }
            }

            lines = null;
        }

        private List<string> reg(string src, Regex pattern)
        {
            var result = new List<string>();
            var match = pattern.Match(src);
            for (int i = 1; i < match.Groups.Count; i++)
                result.Add(match.Groups[i].Value);
            return result;
        }
    }
}
