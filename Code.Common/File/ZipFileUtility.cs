using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;

namespace Code.Common.File
{
    public class ZipFileUtility
    {
        public void ZipFile(string fileToZip, string sourceDirectory)
        {
            //如果文件没有找到，则报错   
            if (!System.IO.Directory.Exists(sourceDirectory))
            {
                throw new System.IO.FileNotFoundException("The specified file " + sourceDirectory + " could not be found. Zipping aborderd");
            }
            var zip = new FastZip();
            zip.CreateZip(fileToZip, sourceDirectory, true, "");
        }

    }
}
