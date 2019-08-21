/***

   Copyright (C) 2019. rollrat. All Rights Reserved.
   
   Author: HyunJun Jeong

***/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kakaotalk_analyzer.Core.Console.CoreUtils
{
    /// <summary>
    /// Out 콘솔 옵션입니다.
    /// </summary>
    public class OutConsoleOption : IConsoleOption
    {
        [CommandLine("--help", CommandType.OPTION, Default = true)]
        public bool Help;

        [CommandLine("-o", CommandType.ARGUMENTS, Pipe = true, DefaultArgument = true,
            Info = "Set output file name.")]
        public string[] Output;

        [CommandLine("-i", CommandType.OPTION, Info = "Set overwrite regardless of file existing.")]
        public bool Overwrite;
    }

    /// <summary>
    /// 파일 쓰기에 관여하는 콘솔 클래스입니다.
    /// </summary>
    public class OutConsole : IConsole
    {
        /// <summary>
        /// Out 콘솔 리다이렉트
        /// </summary>
        static bool Redirect(string[] arguments, string contents)
        {
            arguments = CommandLineUtil.InsertWeirdArguments<OutConsoleOption>(arguments, contents != "", "-o");
            OutConsoleOption option = CommandLineParser<OutConsoleOption>.Parse(arguments);

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
            else if (option.Output != null)
            {
                ProcessOut(option.Output, contents, option.Overwrite);
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
                "Out Console Pipeline\r\n" +
                "This command must be used with pipe.\r\n" +
                "\r\n" +
                " -o (<pipe>|<address>): Set output file address.\r\n" +
                " -i : overwrite"
                );
        }

        /// <summary>
        /// 콘텐츠를 파일에 씁니다.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="contents"></param>
        static void ProcessOut(string[] args, string contents, bool overwrite = false)
        {
            if (contents == "")
            {
                Console.Instance.WriteErrorLine(
                    $"Pipe is null or empty.");
                return;
            }

            if (!overwrite && File.Exists(args[0]))
            {
                Console.Instance.WriteLine(
                    $"'{args[0]}' file already exists.");
                return;
            }

            Monitor.Instance.Push($"Write file: {args[0]}");
            File.WriteAllText(args[0], contents);
        }
    }
}
