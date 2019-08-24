/***

   Copyright (C) 2019. rollrat. All Rights Reserved.
   
   Author: HyunJun Jeong

***/

using Moda.Korean.TwitterKoreanProcessorCS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kakaotalk_analyzer.Core
{
    public class Member
    {
        public int Id;
        public string Name;
        public List<Talk> Talks;
    }

    public class TalkManager
    {
        public string Title { get; set; }
        public List<Talk> Talks { get; set; }

        public TalkManager(string title, List<Talk> talks)
        {
            Title = title;
            Talks = talks;
        }

        #region Message Word Analyzer

        public Dictionary<string, int> Words { get; set; }

        #region Console
        Console.Console.ConsoleProgressBar cpb;
        public void StartAllMessageAnalyzeForConsole()
        {
            Words = new Dictionary<string, int>();
            cpb = new Console.Console.ConsoleProgressBar();
            cnt = 0;

            var thread = Environment.ProcessorCount;
            var term = Talks.Count / thread;
            var tasks = new List<Task>();

            if (term >= 1)
            {
                for (int i = 0; i < thread; i++)
                {
                    int k = i;
                    if (i != thread - 1)
                        tasks.Add(new Task(() => samafc_internal(k * term, k * term + term)));
                    else
                        tasks.Add(new Task(() => samafc_internal(k * term, Talks.Count)));
                }
            }
            else
            {
                tasks.Add(new Task(() => samafc_internal(0, Talks.Count)));
            }

            Parallel.ForEach(tasks, task => task.Start());
            Task.WhenAll(tasks).Wait();

            cpb.Dispose();
        }

        int cnt = 0;
        private void samafc_internal(int starts, int ends)
        {
            var cc = (ends - starts) / 100;
            var mwords = new Dictionary<string, int>();
            for (int i = starts; i < ends; i++)
            {
                try
                {
                    if (Talks[i].State == TalkState.Message || Talks[i].State == TalkState.Append)
                    {
                        var tokens = TwitterKoreanProcessorCS.Tokenize(Talks[i].Content);
                        var stem = TwitterKoreanProcessorCS.Stem(tokens);

                        foreach (var word in stem)
                        {
                            if (word.Pos == KoreanPos.ProperNoun)/* || word.Pos == KoreanPos.Noun)*/
                            {
                                if (!mwords.ContainsKey(word.Text))
                                    mwords.Add(word.Text, 1);
                                else
                                    mwords[word.Text] += 1;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.Console.Instance.WriteLine(Talks[i].Content + "\r\n" + e.Message);
                }

                var x = System.Threading.Interlocked.Increment(ref cnt);

                if ((i % cc == 0) || i == ends)
                    lock (cpb)
                        cpb.SetProgress(x / (float)Talks.Count * 100);
            }

            lock (Words)
            {
                foreach (var ww in mwords)
                {
                    if (!Words.ContainsKey(ww.Key))
                        Words.Add(ww.Key, 1);
                    else
                        Words[ww.Key] += ww.Value;
                }
            }
        }
        #endregion

        public void StartAllMessageAnalyze(Action<int> progress, bool using_tokenizer, bool include_noun, bool ignore_symbols, bool ignore_numeric)
        {
            StartSpecificMessageAnalyze(x => true, progress, using_tokenizer, include_noun, ignore_symbols, ignore_numeric);
        }
        
        public void StartSpecificMessageAnalyze(Func<Talk, bool> filter, Action<int> progress, bool using_tokenizer, bool include_noun, bool ignore_symbols, bool ignore_numeric)
        {
            Words = new Dictionary<string, int>();
            cnt = 0;

            var thread = Environment.ProcessorCount;
            var term = Talks.Count / thread;
            var tasks = new List<Task>();

            if (term >= 1)
            {
                for (int i = 0; i < thread; i++)
                {
                    int k = i;
                    if (i != thread - 1)
                        tasks.Add(new Task(() => samaf_internal(k * term, k * term + term, filter, progress, using_tokenizer, include_noun, ignore_symbols, ignore_numeric)));
                    else
                        tasks.Add(new Task(() => samaf_internal(k * term, Talks.Count, filter, progress, using_tokenizer, include_noun, ignore_symbols, ignore_numeric)));
                }
            }
            else
            {
                tasks.Add(new Task(() => samaf_internal(0, Talks.Count, filter, progress, using_tokenizer, include_noun, ignore_symbols, ignore_numeric)));
            }

            Parallel.ForEach(tasks, task => task.Start());
            Task.WhenAll(tasks).Wait();
        }

        private void samaf_internal(int starts, int ends, Func<Talk, bool> filter, Action<int> progress, bool using_tokenizer, bool include_noun, bool ignore_symbols, bool ignore_numeric)
        {
            var cc = (ends - starts) / 100;
            var mwords = new Dictionary<string, int>();
            for (int i = starts; i < ends; i++)
            {
                if (Talks[i].State == TalkState.Message || Talks[i].State == TalkState.Append)
                {
                    if (!filter(Talks[i]))
                        continue;
                
                    if (using_tokenizer)
                    {
                        try
                        {
                            var tokens = TwitterKoreanProcessorCS.Tokenize(Talks[i].Content);
                            var stem = TwitterKoreanProcessorCS.Stem(tokens);

                            foreach (var word in stem)
                            {
                                if (word.Pos == KoreanPos.ProperNoun || (include_noun && word.Pos == KoreanPos.Noun))
                                {
                                    if (!mwords.ContainsKey(word.Text))
                                        mwords.Add(word.Text, 1);
                                    else
                                        mwords[word.Text] += 1;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            ;
                        }
                    }
                    else
                    {
                        var split = Talks[i].Content.Split(' ');
                
                        split.Where(y =>
                        {
                            double n;
                            return !double.TryParse(y, out n);
                        }).ToList().ForEach(y =>
                        {
                            var str = y.Trim();
                            if (str == "") return;
                            if ("?!.,".Contains(str.Last()))
                                str = str.Remove(str.Length - 1);
                            if (str == "") return;
                            if (!mwords.ContainsKey(str))
                                mwords.Add(str, 1);
                            else
                                mwords[str] += 1;
                        });
                    }
                }

                var x = System.Threading.Interlocked.Increment(ref cnt);

                if (cc == 0 || (i % cc == 0))
                   progress(x);
            }

            progress(cnt);

            lock (Words)
            {
                foreach (var ww in mwords)
                {
                    if (!Words.ContainsKey(ww.Key))
                        Words.Add(ww.Key, ww.Value);
                    else
                        Words[ww.Key] += ww.Value;
                }
            }
        }
        
        /*********************************************************************************************************/

        public Dictionary<int, Dictionary<string, int>> DateWords { get; set; }

        public void StartAllMessageAnalyzeByDate(Action<int> progress, bool using_tokenizer, bool include_noun, bool ignore_symbols, bool ignore_numeric)
        {
            StartSpecificMessageAnalyzeByDate(x => true, progress, using_tokenizer, include_noun, ignore_symbols, ignore_numeric);
        }

        public void StartSpecificMessageAnalyzeByDate(Func<Talk, bool> filter, Action<int> progress, bool using_tokenizer, bool include_noun, bool ignore_symbols, bool ignore_numeric)
        {
            DateWords = new Dictionary<int, Dictionary<string, int>>();
            cnt = 0;

            var thread = Environment.ProcessorCount;
            var term = Talks.Count / thread;
            var tasks = new List<Task>();

            if (term >= 1)
            {
                for (int i = 0; i < thread; i++)
                {
                    int k = i;
                    if (i != thread - 1)
                        tasks.Add(new Task(() => samaf_internal_by_date(k * term, k * term + term, filter, progress, using_tokenizer, include_noun, ignore_symbols, ignore_numeric)));
                    else
                        tasks.Add(new Task(() => samaf_internal_by_date(k * term, Talks.Count, filter, progress, using_tokenizer, include_noun, ignore_symbols, ignore_numeric)));
                }
            }
            else
            {
                tasks.Add(new Task(() => samaf_internal_by_date(0, Talks.Count, filter, progress, using_tokenizer, include_noun, ignore_symbols, ignore_numeric)));
            }

            Parallel.ForEach(tasks, task => task.Start());
            Task.WhenAll(tasks).Wait();
        }

        private void samaf_internal_by_date(int starts, int ends, Func<Talk, bool> filter, Action<int> progress, bool using_tokenizer, bool include_noun, bool ignore_symbols, bool ignore_numeric)
        {
            var cc = (ends - starts) / 100;
            var mwords = new Dictionary<int, Dictionary<string, int>>();
            for (int i = starts; i < ends; i++)
            {
                if (Talks[i].State == TalkState.Message || Talks[i].State == TalkState.Append)
                {
                    if (!filter(Talks[i]))
                        continue;

                    var date = Talks[i].Time.Year * 100 + Talks[i].Time.Month;

                    if (!mwords.ContainsKey(date))
                        mwords.Add(date, new Dictionary<string, int>());

                    if (using_tokenizer)
                    {
                        try
                        {
                            var tokens = TwitterKoreanProcessorCS.Tokenize(Talks[i].Content);
                            var stem = TwitterKoreanProcessorCS.Stem(tokens);

                            foreach (var word in stem)
                            {
                                if (word.Pos == KoreanPos.ProperNoun || (include_noun && word.Pos == KoreanPos.Noun))
                                {
                                    if (!mwords[date].ContainsKey(word.Text))
                                        mwords[date].Add(word.Text, 1);
                                    else
                                        mwords[date][word.Text] += 1;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            ;
                        }
                    }
                    else
                    {
                        var split = Talks[i].Content.Split(' ');

                        split.Where(y =>
                        {
                            double n;
                            return !double.TryParse(y, out n);
                        }).ToList().ForEach(y =>
                        {
                            var str = y.Trim();
                            if (str == "") return;
                            if ("?!.,".Contains(str.Last()))
                                str = str.Remove(str.Length - 1);
                            if (str == "") return;
                            if (!mwords[date].ContainsKey(str))
                                mwords[date].Add(str, 1);
                            else
                                mwords[date][str] += 1;
                        });
                    }
                }

                var x = System.Threading.Interlocked.Increment(ref cnt);

                if (cc == 0 || (i % cc == 0))
                    progress(x);
            }

            progress(cnt);

            lock (DateWords)
            {
                foreach (var ww in mwords)
                {
                    if (!DateWords.ContainsKey(ww.Key))
                        DateWords.Add(ww.Key, new Dictionary<string, int>());

                    foreach (var w in ww.Value)
                    {
                        if (!DateWords[ww.Key].ContainsKey(w.Key))
                            DateWords[ww.Key].Add(w.Key, w.Value);
                        else
                            DateWords[ww.Key][w.Key] += w.Value;
                    }
                }
            }
        }

        #endregion

        #region Member

        public List<Member> Members { get; set; }

        public void ExtractMember()
        {
            var count = 0;
            var members = new Dictionary<string, Member>();

            foreach (var talk in Talks)
            {
                if (talk.Name != null)
                {
                    if (!members.ContainsKey(talk.Name))
                        members.Add(talk.Name, new Member { Id = count++, Name = talk.Name, Talks = new List<Talk>() });
                    members[talk.Name].Talks.Add(talk);
                }
            }

            Members = members.Select(x => x.Value).ToList();
            Members.Sort((x, y) => x.Id.CompareTo(y.Id));
        }

        #endregion

        public int GetExactTalksCount()
        {
            return Talks.Where(x => x.State == TalkState.Message).Count();
        }

        public DateTime FirstAvailableTalksTime()
        {
            return Talks.First(x => x.State == TalkState.Message).Time;
        }

        public DateTime LastAvailableTalksTime()
        {
            return Talks.Last(x => x.State == TalkState.Message || x.State == TalkState.Append).Time;
        }
    }
}
