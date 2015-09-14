using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Configuration
{
    public class FileChangedEventArgs : System.EventArgs
    {
        public string FileName;
        public FileChangedEventArgs(string fileName)
        {
            this.FileName = fileName;
        }
    }
}
