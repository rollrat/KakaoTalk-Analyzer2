/***

   Copyright (C) 2019. rollrat. All Rights Reserved.
   
   Author: HyunJun Jeong

***/

using kakaotalk_analyzer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kakaotalk_analyzer
{
    public class TalkInstance : ILazy<TalkInstance>
    {
        public TalkManager Manager { get; set; }

        KakaoTalkParser parser;

        public void Open(string filename)
        {
            parser = new KakaoTalkParser(filename);
        }

        public string Title { get { return parser.Title; } }

        public void Parse()
        {
            parser.ParseStart();
            Manager = new TalkManager(parser.Title, parser.Talks);
        }
    }
}
