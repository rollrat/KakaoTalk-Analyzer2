/***

   Copyright (C) 2019. rollrat. All Rights Reserved.
   
   Author: HyunJun Jeong

***/

using kakaotalk_analyzer.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace kakaotalk_analyzer.Model
{
    public class TalkListItemViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Talk Talk { get; set; }

        private string _0;
        public string 인덱스
        {
            get { return _0; }
            set
            {
                if (_0 == value) return;
                _0 = value;
                OnPropertyChanged();
            }
        }

        private string _1;
        public string 유형
        {
            get { return _1; }
            set
            {
                if (_1 == value) return;
                _1 = value;
                OnPropertyChanged();
            }
        }

        private string _3;
        public string 작성자
        {
            get { return _3; }
            set
            {
                if (_3 == value) return;
                _3 = value;
                OnPropertyChanged();
            }
        }

        private string _4;
        public string 내용
        {
            get { return _4; }
            set
            {
                if (_4 == value) return;
                _4 = value;
                OnPropertyChanged();
            }
        }

        private string _5;
        public string 날짜
        {
            get { return _5; }
            set
            {
                if (_5 == value) return;
                _5 = value;
                OnPropertyChanged();
            }
        }
    }

    public class TalkListViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<TalkListItemViewModel> _items;
        public ObservableCollection<TalkListItemViewModel> Items => _items;

        public TalkListViewModel()
        {
            _items = new ObservableCollection<TalkListItemViewModel>();
        }
    }
}
