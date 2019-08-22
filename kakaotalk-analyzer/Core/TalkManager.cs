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

        public void StartAllMessageAnalyze(Action<int> progress)
        {
            StartSpecificMessageAnalyze(x => true, progress);
        }
        
        public void StartSpecificMessageAnalyze(Func<Talk, bool> filter, Action<int> progress)
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
                        tasks.Add(new Task(() => samaf_internal(k * term, k * term + term, filter, progress)));
                    else
                        tasks.Add(new Task(() => samaf_internal(k * term, Talks.Count, filter, progress)));
                }
            }
            else
            {
                tasks.Add(new Task(() => samaf_internal(0, Talks.Count, filter, progress)));
            }

            Parallel.ForEach(tasks, task => task.Start());
            Task.WhenAll(tasks).Wait();
        }

        private void samaf_internal(int starts, int ends, Func<Talk, bool> filter, Action<int> progress)
        {
            var cc = (ends - starts) / 100;
            var mwords = new Dictionary<string, int>();
            for (int i = starts; i < ends; i++)
            {
                try
                {
                    if (Talks[i].State == TalkState.Message || Talks[i].State == TalkState.Append)
                    {
                        if (!filter(Talks[i]))
                            continue;

                        var tokens = TwitterKoreanProcessorCS.Tokenize(Talks[i].Content);
                        var stem = TwitterKoreanProcessorCS.Stem(tokens);

                        foreach (var word in stem)
                        {
                            if (word.Pos == KoreanPos.ProperNoun)
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
                    ;
                }

                var x = System.Threading.Interlocked.Increment(ref cnt);

                if (cc == 0 || (i % cc == 0) || i == ends)
                   progress(x);
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
