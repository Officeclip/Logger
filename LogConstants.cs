using System;
using System.IO;
using System.Xml.Serialization;

namespace OpenNetTools.Logger
{
    public enum LogState
    {
        None = 0,
        Pass = 1,
        Fail = 2,
        Warning = 3,
        Debug = 4,
        Title = 5,
        SubTitle = 6,
        Error = 7,
        FatalError = 8,
        Info = 9,
        Event = 10,
        Property = 11,
        Function = 12,
        Method = 13
    }

    public enum LogLevel
    {
        OFF = 0,
        FATAL = 1,
        ERROR = 2,
        WARNING = 3,
        INFO = 4,
        DEBUG = 5
    }

    public class LogConstants
    {
        public static LogLevel GetLogLevel(LogState logState)
        {
            LogLevel logLevel = LogLevel.OFF;
            switch (logState)
            {
                case LogState.None:
                    logLevel = LogLevel.OFF;
                    break;
                case LogState.Event:
                case LogState.Function:
                case LogState.Info:
                case LogState.Pass:
                case LogState.SubTitle:
                case LogState.Title:
                    logLevel = LogLevel.INFO;
                    break;
                case LogState.Error:
                case LogState.Fail:
                    logLevel = LogLevel.ERROR;
                    break;
                case LogState.FatalError:
                    logLevel = LogLevel.FATAL;
                    break;
                case LogState.Debug:
                case LogState.Property:
                case LogState.Method:
                    logLevel = LogLevel.DEBUG;
                    break;
            }
            return logLevel;
        }

        public static string GetSetupDir()
        {
            System.Reflection.Assembly Asm = System.Reflection.Assembly.GetExecutingAssembly();
            return Path.GetDirectoryName(Asm.Location);
        }

        public static void ProvideFullAccess(string folder)
        {
            // In vista this does not work as we need to get the SeSecurityPrivilege access. In the meantime
            // we have to ignore this in case of any exception
            try
            {
                System.Security.AccessControl.DirectorySecurity sec
                = Directory.GetAccessControl(folder);
                sec.SetSecurityDescriptorSddlForm("D:PAI(A;OICI;FA;;;WD)"); // Gives full access to all the files in this folder
                Directory.SetAccessControl(folder, sec);
            }
            catch
            {
            }
        }

        public static LogConfig DeserializeLogConfig(string fileName)
        {
            LogConfig logConfig = null;
            if (File.Exists(fileName))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(LogConfig));
                using (FileStream fs = new FileStream(
                    fileName,
                    FileMode.Open, 
                    FileAccess.Read, 
                    FileShare.ReadWrite))
                {
                    logConfig = (LogConfig)serializer.Deserialize(fs);
                }
            }
            else
            {
                throw new Exception("log instantiation needs the correct config file");
            }
            return logConfig;           
        }
    }

}
