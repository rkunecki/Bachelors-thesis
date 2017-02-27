using System;
using System.Runtime.InteropServices;
using System.IO;

namespace PCBFrez
{
    public class ConfigRead
    {
        #region DLL methods
        [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileStringA", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern int GetPrivateProfileString(string lpApplicationName, string lpKeyName, string lpDefault, System.Text.StringBuilder lpReturnedString, int nSize, string lpFileName);
        [DllImport("kernel32.dll", EntryPoint = "WritePrivateProfileStringA", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern int WritePrivateProfileString(string lpApplicationName, string lpKeyName, string lpString, string lpFileName);
        [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileIntA", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern int GetPrivateProfileInt(string lpApplicationName, string lpKeyName, int nDefault, string lpFileName);
        [DllImport("kernel32.dll", EntryPoint = "WritePrivateProfileStringA", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]

        private static extern int FlushPrivateProfileString(int lpApplicationName, int lpKeyName, int lpString, string lpFileName);
        private static string strFilename;
        private static void iniFile(string Filename)
        {
            strFilename = Filename;
        }
        private static string FileName
        {
            get
            {
                return strFilename;
            }
        }
        private static string GetString(string Section, string Key, string @Default)
        {
            string returnValue = "";
            int intCharCount;
            System.Text.StringBuilder objResult = new System.Text.StringBuilder(256);
            intCharCount = GetPrivateProfileString(Section, Key, @Default, objResult, objResult.Capacity, strFilename);
            if (intCharCount > 0)
            {
                returnValue = objResult.ToString().Substring(0, intCharCount);
            }
            return returnValue;
        }
        private static bool GetBoolean(string Section, string Key, bool @Default)
        {
            return (GetPrivateProfileInt(Section, Key, System.Convert.ToInt32(@Default), strFilename) == 1);
        }
        private static void WriteString(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, strFilename);
            Flush();
        }
        private static void WriteInteger(string Section, string Key, int Value)
        {
            WriteString(Section, Key, Value.ToString());
            Flush();
        }
        private static void WriteBoolean(string Section, string Key, bool Value)
        {
            WriteString(Section, Key, System.Convert.ToInt32(Value).ToString());
            Flush();
        }
        private static void Flush()
        {
            FlushPrivateProfileString(0, 0, 0, strFilename);
        }
        #endregion

        public static int[] ReadAndWriteInt(string configPatch, string logPatch)
        {
            int[] result = new int[5];
            iniFile(configPatch + @"\config.ini");

            result[0] = Convert.ToInt32(GetString("PortDefaultsValue", "Baudrate", "(none)"));
            result[1] = Convert.ToInt32(GetString("PortDefaultsValue", "StopBits", "(none)"));
            result[2] = Convert.ToInt32(GetString("PortDefaultsValue", "parityBits", "(none)"));
            result[3] = Convert.ToInt32(GetString("PortDefaultsValue", "DataBits", "(none)"));
            result[4] = Convert.ToInt32(GetString("MillingMachineDefaultsValue", "Steps", "(none)"));              
            return result;
        }

        public static string ReadAndWriteString(string configPatch)
        {
            iniFile(configPatch + @"\config.ini");
            return GetString("Log", "LogPatch", "(none)");          
        }

        public static void SaveErrorToLog(string logPatch, string errorMessage)
        {
            try
            {
                logPatch = logPatch + @"\(" + DateTime.Today.Day.ToString() + "-" + DateTime.Today.Month.ToString() + "-" + DateTime.Today.Year.ToString() + ")ErrorLog.txt";

                FileStream fs = new FileStream(logPatch, FileMode.Append, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs);

                sw.WriteLine("[" + DateTime.Today.Day.ToString() + "." + DateTime.Today.Month.ToString() + "." + DateTime.Today.Year.ToString() + " " + DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString() + ":" + DateTime.Now.Second.ToString() + "]Error Message: " + errorMessage);

                sw.Close();
            }
            catch { }
        }
    }
}