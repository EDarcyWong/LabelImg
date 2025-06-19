using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabelImg.Models
{
    public class MyMessage : ValueChangedMessage<string>
    {
        public MyMessage(string value) : base(value) { }
    }

}
