using CommunityToolkit.Mvvm.ComponentModel;
using LabelImg.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabelImg.ViewModels
{
  public partial  class LGViewModel : ObservableObject
    {

        [ObservableProperty]
        private ObservableCollection<CutLabelModel> cutList;

        public LGViewModel()
        {

            CutList = new ObservableCollection<CutLabelModel>
            {
            };
        }
    }
}
