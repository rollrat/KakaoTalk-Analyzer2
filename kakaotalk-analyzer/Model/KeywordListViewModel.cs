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
    public class KeywordListItemViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        private string _0;
        public string 순위
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
        public string 키워드
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
        public string 개수
        {
            get { return _3; }
            set
            {
                if (_3 == value) return;
                _3 = value;
                OnPropertyChanged();
            }
        }
    }

    public class KeywordListViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<KeywordListItemViewModel> _items;
        public ObservableCollection<KeywordListItemViewModel> Items => _items;

        public KeywordListViewModel()
        {
            _items = new ObservableCollection<KeywordListItemViewModel>();
        }
    }
}
