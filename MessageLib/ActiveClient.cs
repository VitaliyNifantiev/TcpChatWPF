using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MessageLib
{
    [Serializable]
   public class ActiveClient:INotifyPropertyChanged
    {
        string image;
        public string Id { get; set; }
        public string Name { get; set; }
        public string Image
        {
            get => image;
            set
            {
                image = value;
                Notify();
            }
        }
        #region Notify()
        public event PropertyChangedEventHandler PropertyChanged;
        void Notify([CallerMemberName]string propName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        #endregion
    }
}
