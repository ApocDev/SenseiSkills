using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SenseiSkills
{
    public static class Helper
    {
        static string path = @"d:\skills.txt";

        public static void writeToFile(String text)
        {
            Log.Info("Writing: " + path + " " + text);
            File.WriteAllText(path, text);
                
        }

        private static ILog Log = LogManager.GetLogger("SenseiSkills");

        public static String readFromFile()
        {
       

            string readText = File.ReadAllText(path);

            Log.Info("Read: " + path + " " + readText);
            return readText;
        }

        public static Keys textToKey(String key)
        {
            if (key.Equals("Z"))
                return Keys.Z;
            if (key.Equals("X"))
                return Keys.X;
            if (key.Equals("C"))
                return Keys.C;
            if (key.Equals("V"))
                return Keys.V;
            if (key.Equals("F"))
                return Keys.F;
            if (key.Equals("Tab"))
                return Keys.Tab;
            if (key.Equals("1"))
                return Keys.D1;
            if (key.Equals("2"))
                return Keys.D2;
            if (key.Equals("3"))
                return Keys.D3;
            if (key.Equals("4"))
                return Keys.D4;
            if (key.Equals("R"))
                return Keys.R;
            if (key.Equals("T"))
                return Keys.T;


            return Keys.R;

        }
    }
    
}
