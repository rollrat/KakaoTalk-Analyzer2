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
    public class MemberListItemViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        private string _0;
        public string 아이디
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
        public string 이름
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
        public string 대화수
        {
            get { return _3; }
            set
            {
                if (_3 == value) return;
                _3 = value;
                OnPropertyChanged();
            }
        }

        private string _6;
        public string 평균대화길이
        {
            get { return _6; }
            set
            {
                if (_6 == value) return;
                _6 = value;
                OnPropertyChanged();
            }
        }

        private string _4;
        public string 첫번째대화
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
        public string 마지막대화
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

    public class MemberListViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<MemberListItemViewModel> _items;
        public ObservableCollection<MemberListItemViewModel> Items => _items;

        public MemberListViewModel()
        {
            _items = new ObservableCollection<MemberListItemViewModel>();
        }
    }
}
