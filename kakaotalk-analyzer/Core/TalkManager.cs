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
    public class TalkManager
    {
        public string Title { get; set; }
        public List<Talk> Talks { get; set; }

        public TalkManager(string title, List<Talk> talks)
        {
            Title = title;
            Talks = talks;
        }

        public Dictionary<string, int> Words { get; set; }
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
        public void samafc_internal(int starts, int ends)
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
                    ;
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

        public void StartSpecificMessageAnalyze(Func<Talk, bool> filter)
        {

        }
    }
}
