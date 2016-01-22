using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CM3D2.Utils.Common
{
    public static class Utilities
    {
        public static void CreateBackup(string fullPath)
        {
            if (!File.Exists(fullPath))
                return;
            int count = 0;

            string fileNameOnly = Path.GetFileName(fullPath);
            string path = Path.GetDirectoryName(fullPath);
            string newFullPath = fullPath;

            while (File.Exists(newFullPath))
            {
                string tempFileName = $"{fileNameOnly}.bak{count++}";
                newFullPath = Path.Combine(path, tempFileName);
            }

            File.Move(fullPath, newFullPath);
        }
    }
}
