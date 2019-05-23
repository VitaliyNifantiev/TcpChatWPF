using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft;

namespace MessageLib
{
    [Serializable]
   public class MyMessage
    {
        public MyMessage() { }
        public MyMessage(string str) => Msg = str;
        public string Id { get; set; }
        public string Name { get; set; }
        public string Msg { get; set; }
        public string ReceiverId { get; set; } = "";
    }
}
