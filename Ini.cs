using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Ini
{
    /// <summary>

    /// Create a New INI file to store or load data

    /// </summary>

    public class IniFile
    {
        public string path;
        private static int MAXBUF = Int16.MaxValue;

        [DllImport("kernel32", EntryPoint = "WritePrivateProfileStringW", CharSet = CharSet.Auto)]
        private static extern long WritePrivateProfileString(string section,
            string key, string val, string filePath);
        [DllImport("kernel32", EntryPoint = "GetPrivateProfileStringW", CharSet = CharSet.Auto)]
        private static extern int GetPrivateProfileString(string section,
            string key, string def, string retVal, int size, string filePath);

        /// <summary>

        /// INIFile Constructor.

        /// </summary>

        /// <PARAM name="INIPath"></PARAM>

        public IniFile(string INIPath)
        {
            path = INIPath;
        }
        /// <summary>

        /// Write Data to the INI File

        /// </summary>

        /// <PARAM name="Section"></PARAM>

        /// Section name

        /// <PARAM name="Key"></PARAM>

        /// Key Name

        /// <PARAM name="Value"></PARAM>

        /// Value Name

        public void IniWriteValue(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, this.path);
        }

        /// <summary>

        /// Read Data Value From the Ini File

        /// </summary>

        /// <PARAM name="Section"></PARAM>

        /// <PARAM name="Key"></PARAM>

        /// <PARAM name="Path"></PARAM>

        /// <returns></returns>

        private string IniReadValue(string Section, string Key)
        {
            string temp = new string(' ', MAXBUF);
            int i = GetPrivateProfileString(Section, Key, "", temp,
                                            MAXBUF, this.path);
            temp = temp.ToString().Trim();
            return ((temp == "") ? "" : temp.Substring(0, temp.Length - 1));

        }

        public string getStringValue(string Section, string Key)
        {
            return IniReadValue(Section, Key);
        }

        public int getIntValue(string Section, string Key)
        {
            int intValue = 0;

            try
            {
                intValue = int.Parse(IniReadValue(Section, Key));
            }
            catch (System.Exception) { }

            return intValue;
        }

        public long getLongValue(string Section, string Key)
        {
            long longValue = 0;

            try
            {
                longValue = long.Parse(IniReadValue(Section, Key));
            }
            catch (System.Exception) { }

            return longValue;
        }

        public double getDoubleValue(string Section, string Key)
        {
            double doubleValue = 0;

            try
            {
                doubleValue = double.Parse(IniReadValue(Section, Key));
            }
            catch (System.Exception) { }

            return doubleValue;
        }

        public bool getBoolValue(string Section, string Key)
        {
            bool boolValue = false;

            try
            {
                string stringValue = IniReadValue(Section, Key);

                if (stringValue == "true") boolValue = true;
                else if (stringValue != "false") boolValue = (getIntValue(Section, Key) == 1);

            }
            catch (System.Exception) { }

            return boolValue;
        }

        public string getJobTimeValue(string Section, string Key)
        {

            System.Text.RegularExpressions.Regex something =
                new System.Text.RegularExpressions.Regex(
                    "(\\*|[0-9]|[0-9][0-9]|\\*/[0-9]|\\*/[0-9][0-9]) " +
                    "(\\*|[0-9]|[0-9][0-9]|\\*/[0-9]|\\*/[0-9][0-9]) " +
                    "(\\*|[0-9]|[0-9][0-9]|\\*/[0-9]|\\*/[0-9][0-9]) " +
                    "(\\*|[0-9]|[0-9][0-9]|\\*/[0-9]|\\*/[0-9][0-9]) " +
                    "(\\*|[A-Z][a-z][a-z])",
                    System.Text.RegularExpressions.RegexOptions.Compiled
                    );

            string jobTime = IniReadValue(Section, Key);
            string result = something.Replace(jobTime, "");

            return ((result == "") ? jobTime : "");
        }

        public System.Collections.Generic.List<string> getSectionNames()
        {
            string returnString = new string(' ', MAXBUF);
            GetPrivateProfileString(null, null, null, returnString, MAXBUF, this.path);
            System.Collections.Generic.List<string> result = new System.Collections.Generic.List<string>(returnString.Split('\0'));
            result.RemoveRange(result.Count - 2, 2);
            return result;
        }

        public int getNumSections()
        {
            return getSectionNames().Count;
        }
    }
}
