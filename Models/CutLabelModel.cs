using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LabelImg.Models
{
    public partial class CutLabelModel : INotifyPropertyChanged
    {
        private string name;
        private string content;
        private double width;
        private double height;
        private int classIndex;
        private double xCenter;
        private double yCenter;
        private double xWeight;
        private double yWeight;

        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        public string Content
        {
            get => content;
            set => SetProperty(ref content, value);
        }

        public double Width
        {
            get => width;
            set => SetProperty(ref width, value);
        }

        public double Height
        {
            get => height;
            set => SetProperty(ref height, value);
        }

        public int ClassIndex
        {
            get => classIndex;
            set => SetProperty(ref classIndex, value);
        }

        public double XCenter
        {
            get => xCenter;
            set
            {
                if (SetProperty(ref xCenter, value))
                {
                    OnPropertyChanged(nameof(XCenterStr));
                }
            }
        }

        public double YCenter
        {
            get => yCenter;
            set
            {
                if (SetProperty(ref yCenter, value))
                {
                    OnPropertyChanged(nameof(YCenterStr));
                }
            }
        }

        public double XWeight
        {
            get => xWeight;
            set
            {
                if (SetProperty(ref xWeight, value))
                {
                    OnPropertyChanged(nameof(XWeightStr));
                }
            }
        }

        public double YWeight
        {
            get => yWeight;
            set
            {
                if (SetProperty(ref yWeight, value))
                {
                    OnPropertyChanged(nameof(YWeightStr));
                }
            }
        }

        public string XCenterStr => Math.Round(XCenter, 6).ToString(CultureInfo.InvariantCulture);
        public string YCenterStr => Math.Round(YCenter, 6).ToString(CultureInfo.InvariantCulture);
        public string XWeightStr => Math.Round(XWeight, 6).ToString(CultureInfo.InvariantCulture);
        public string YWeightStr => Math.Round(YWeight, 6).ToString(CultureInfo.InvariantCulture);

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
