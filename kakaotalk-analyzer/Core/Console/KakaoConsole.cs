/***

   Copyright (C) 2019. rollrat. All Rights Reserved.
   
   Author: HyunJun Jeong

***/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kakaotalk_analyzer.Core.Console
{
    /// <summary>
    /// 카카오톡 콘솔 옵션입니다.
    /// </summary>
    public class KakaoConsoleOption : IConsoleOption
    {
        [CommandLine("--help", CommandType.OPTION, Default = true)]
        public bool Help;

        [CommandLine("-load", CommandType.ARGUMENTS, Help = "use -load <File Name>",
            Info = "Load kakao-talk file.")]
        public string[] Load;
        [CommandLine("-parse", CommandType.OPTION, Help = "use -parse",
            Info = "Parse kakao-talk talks.")]
        public bool Parse;

        [CommandLine("-message-analyze", CommandType.OPTION, Help = "use -message-analyze",
            Info = "Analyze messages.")]
        public bool MessageAnalyze;
    }
    
    public class KakaoConsole : IConsole
    {
        /// <summary>
        /// 콘솔 리다이렉트
        /// </summary>
        static bool Redirect(string[] arguments, string contents)
        {
            KakaoConsoleOption option = CommandLineParser<KakaoConsoleOption>.Parse(arguments, contents != "", contents);

            if (option.Error)
            {
                Console.Instance.WriteLine(option.ErrorMessage);
                if (option.HelpMessage != null)
                    Console.Instance.WriteLine(option.HelpMessage);
                return false;
            }
            else if (option.Help)
            {
                PrintHelp();
            }
            else if (option.Load != null)
            {
                ProcessLoad(option.Load);
            }
            else if (option.Parse)
            {
                ProcessParse();
            }
            else if (option.MessageAnalyze)
            {
                ProcessMessageAnalyze();
            }

            return true;
        }

        bool IConsole.Redirect(string[] arguments, string contents)
        {
            return Redirect(arguments, contents);
        }

        static void PrintHelp()
        {
            Console.Instance.WriteLine(
                "KakaoTalk Console Core\r\n" +
                "\r\n"
                );

            var builder = new StringBuilder();
            CommandLineParser<KakaoConsoleOption>.GetFields().ToList().ForEach(
                x =>
                {
                    if (!string.IsNullOrEmpty(x.Value.Item2.Help))
                        builder.Append($" {x.Key} ({x.Value.Item2.Help}) : {x.Value.Item2.Info} [{x.Value.Item1}]\r\n");
                    else
                        builder.Append($" {x.Key} : {x.Value.Item2.Info} [{x.Value.Item1}]\r\n");
                });
            Console.Instance.WriteLine(builder.ToString());
        }

        static KakaoTalkParser parser;
        static TalkManager manager;
        static void ProcessLoad(string[] args)
        {
            parser = new KakaoTalkParser(args[0]);

            Console.Instance.WriteLine($"Loaded - {parser.Title}");
        }

        static void ProcessParse()
        {
            if (parser == null)
            {
                Console.Instance.WriteErrorLine("Load talks file after parsing!");
                return;
            }

            parser.ParseStart();
            manager = new TalkManager(parser.Title, parser.Talks);

            Console.Instance.WriteLine($"{parser.Talks.Where(x => x.State == TalkState.Message).Count().ToString("#,#")} talks found!");
        }

        static void ProcessMessageAnalyze()
        {
            if (manager == null)
            {
                Console.Instance.WriteErrorLine("Parse talks file after parsing!");
                return;
            }

            manager.StartAllMessageAnalyzeForConsole();

            var ll = manager.Words.ToList();
            ll.Sort((x, y) => y.Value.CompareTo(x.Value));

            for (int i = 0; i < 1000; i++)
                Console.Instance.WriteLine(ll[i].Key + $" ({ll[i].Value})");
        }
    }
}