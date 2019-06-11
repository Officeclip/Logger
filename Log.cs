using System;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Linq;

namespace OfficeClip.OpenSource.Logger
{
    public class Log
    {
        public FileLog FileLog;
        public DbLog DbLog;

        private List<Logger> LoggerList;

        public string DefaultCategory { get; set; }

        public string DefaultUser { get; set; }

        private readonly object thisLock = new object();

        private string StackFilter = string.Empty;

        private string StackRejectFilter = string.Empty;

        private Logger DefaultLogger
        {
            get
            {
                Logger logger = LoggerList.FirstOrDefault(x => x.Name == DefaultCategory);
                if (logger != null)
                {
                    return logger;
                }
                else if (LoggerList.Count > 0)
                {
                    return LoggerList[0];
                }
                throw new ArgumentException($"Error: Logger - {DefaultCategory} in the code does not match xml file and there is no default logger");
            }
        }

        public LogLevel LogLevel
        {
            get
            {
                return (LogLevel)DefaultLogger.Level;
            }
            set
            {
                DefaultLogger.Level = (int)value;
            }
        }

        /// <summary>
        /// Restricts the log level if set by the application configuration file. This allows the
        /// application configuration file (web.config etc.) to control the loglevel of the
        /// overall logging when the application software is released.
        /// </summary>
        /// <param name="configurationLogLevel"></param>
        public void RestrictLogLevel(LogLevel configurationLogLevel)
        {
            LogLevel = (LogLevel < configurationLogLevel)
                                ? LogLevel
                                : configurationLogLevel;
        }

        public bool IsCategoryExist(string category)
        {
            return LoggerList.Exists(x => x.Name == category);
        }

        public Log(LogConfig logConfig)
        {
            Initialize(logConfig);
        }

        public Log()
        {
            LogConfig logConfig = new LogConfig();
            Initialize(logConfig);
        }

        public Log(string configFilePath)
        {
            LogConfig logConfig = LogConstants.DeserializeLogConfig(configFilePath);
            Initialize(logConfig);
        }

        /// <summary>
        /// Initilizes the parameters
        /// </summary>
        protected void Initialize(LogConfig logConfig)
        {
            LoggerList = new List<Logger>();
            foreach (Logger logger in logConfig.Loggers)
            {
                LoggerList.Add(logger);
            }
            if (logConfig.PurgeDays == 0)
            {
                logConfig.PurgeDays = 7;
            }
            if (!string.IsNullOrEmpty(logConfig.StackLineMatchPattern))
            {
                StackFilter = logConfig.StackLineMatchPattern;
            }
            if (!string.IsNullOrEmpty(logConfig.StackLineRejectPattern))
            {
                StackRejectFilter = logConfig.StackLineRejectPattern;
            }
            FileLog = new FileLog(logConfig);
            if (FileLog.IsEnabled)
            {
                FileLog.Initialize();
            }
            DbLog = new DbLog(logConfig);
            if (DbLog.IsEnabled)
            {
                DbLog.Initialize();
            }

            WriteSubTitle("*** New Log ***", "A new log is started");
        }

        /// <summary>
        /// Used to find the log state from sql server security levels
        /// </summary>
        /// <param name="severity"></param>
        /// <returns></returns>
        protected LogState GetLogStateFromSqlServerSeverity(int severity)
        {
            LogState state = LogState.Info; // for severity level upto 10
            switch (severity)
            {
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                case 17:
                case 18:
                    state = LogState.Error;
                    break;
                case 19:
                case 20:
                case 21:
                case 22:
                case 23:
                case 24:
                case 25:
                    state = LogState.FatalError;
                    break;
            }
            return state;
        }

        #region Write Methods

        public void WriteSqlLog(
                            int lineNum,
                            int errorClass,
                            int errorNum,
                            string errorString,
                            string procedure,
                            string source,
                            string additionalInfo,
                            string user = "",
                            string category = "",
                            string custom = "")
        {
            LogState state = GetLogStateFromSqlServerSeverity(errorClass);
            const string topic = "Sql Server";
            string description = string.Format(
               "Line#: {0}, Error Class: {1}, Error Number: {2}, Procedure: {3}, Source: {4}, Mesage: {5} - {6}",
               lineNum,
               errorClass,
               errorNum,
               procedure,
               source,
               errorString,
               additionalInfo
            );

            Write(
                state,
                topic,
                description,
                null,
                user,
                category,
                custom);
        }

        public void WriteProperty(string propertyName, string propertyDescription, string user = "", string category = "", string custom = "")
        {
            Write(LogState.Property, propertyName, propertyDescription, null, user, category, custom);
        }

        public void WriteFunction(
            string functionName,
            string arg1,
            string arg2,
            string arg3,
            string argRest,
            string user = "",
            string category = "",
            string custom = "")
        {
            string desc = $"({arg1}, {arg2}, {arg3}, {argRest})";
            Write(LogState.Function, functionName, desc, null, user, category, custom);
        }

        public void WriteMethod(
            string user = "",
            string category = "",
            string custom = "",
            params object[] args)
        {
            StringBuilder sb = new StringBuilder();
            StackTrace callStack = new StackTrace(1, true);
            StackFrame callingMethodFrame = callStack.GetFrame(1);
            MethodBase callingMethod = callingMethodFrame.GetMethod();
            sb.Append($"{callingMethodFrame.GetFileName()}:{callingMethodFrame.GetFileLineNumber()} ");
            sb.Append(callingMethod.Name);
            sb.Append("(");
            ParameterInfo[] parameters = callingMethod.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                sb.Append(parameters[i].Name);
                sb.Append("=");
                sb.Append(
                    (i < args.Length) && (args.Length > 0) ?
                    args[i].ToString().Trim() :
                    "???");
                sb.Append(",");
            }
            string output = sb.ToString().TrimEnd(',') + ")";
            Write(
                LogState.Method,
                $"{callingMethod.ReflectedType.Name}:{callingMethod.Name}",
                output,
                null,
                user,
                category,
                custom);
        }

        public void WriteEvent(string title, string description, string user = "", string category = "", string custom = "")
        {
            Write(LogState.Event, title, description, null, user, category, custom);
        }

        public void WriteInfo(string title, string description, string user = "", string category = "", string custom = "")
        {
            Write(LogState.Info, title, description, null, user, category, custom);
        }

        private List<string> StackInfo(LogState logState, Exception exception)
        {
            var stackLines = new List<string>();
            if (
                (logState != LogState.Error) && (logState != LogState.FatalError) && (logState != LogState.Debug))
            {
                return stackLines;
            }

            var callStack = GetCallStack(exception);

            using (StringReader reader = new StringReader(callStack.ToString()))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!string.IsNullOrEmpty(StackFilter) &&
                        !IsFilterPresentInLine(line, StackFilter))
                    {
                        continue;
                    }
                    if (
                        !string.IsNullOrEmpty(StackRejectFilter)
                        && IsFilterPresentInLine(line, StackRejectFilter))
                    {
                        continue;
                    }
                    stackLines.Add(line);
                }
            }
            return stackLines;
        }

        private static StackTrace GetCallStack(Exception exception)
        {
            StackTrace callStack;
            if (exception != null)
            {
                while (exception.InnerException != null) exception = exception.InnerException;
                callStack = new StackTrace(exception, true);
            }
            else
            {
                callStack = new StackTrace(
                    0,
                    true);
            }

            return callStack;
        }

        private bool IsFilterPresentInLine(
            string line, string filterName)
        {
            if (string.IsNullOrEmpty(filterName))
            {
                return true;
            }
            foreach (string filter in filterName.Split(
                                                        ','))
            {
                if (line.Contains(
                                   filter))
                {
                    return true;
                }
            }
            return false;
        }

        public void WriteDebug(
            string title, 
            string description, 
            Exception exception = null,
            string user = "", 
            string category = "", 
            string custom = "")
        {
            Write(
                LogState.Debug,
                title,
                description,
                exception,
                user,
                category,
                custom);
        }

        public void WriteError(
            string title,
            string description,
            Exception exception = null,
            string user = "",
            string category = "",
            string custom = "")
        {
            Write(
                LogState.Error,
                title,
                description,
                exception,
                user,
                category,
                custom);
        }

        public void WriteFatalError(
            string title, 
            string description,
            Exception exception = null,
            string user = "", 
            string category = "", 
            string custom = "")
        {
            Write(
                LogState.FatalError,
                title,
                description,
                exception,
                user,
                category,
                custom);
        }

        public void WriteWarning(
            string title, 
            string description,
            Exception exception = null,
            string user = "", 
            string category = "", 
            string custom = "")
        {
            Write(
                LogState.Warning, 
                title, 
                description, 
                exception, 
                user, 
                category, 
                custom);
        }

        public void WritePass(
            string title, 
            string description, 
            string user = "", 
            string category = "", 
            string custom = "")
        {
            Write(
                LogState.Pass, 
                title, 
                description, 
                null, 
                user, 
                category, custom);
        }

        public void WriteSubTitle(
            string title, 
            string description, 
            string user = "", 
            string category = "", 
            string custom = "")
        {
            Write(
                LogState.SubTitle, 
                title, 
                description, 
                null, 
                user, 
                category, 
                custom);
        }

        #endregion Write Methods

        public void Write(
            LogState state,
            string topic,
            string description,
            Exception exception = null,
            string user = "",
            string category = "",
            string custom = "")
        {
            string categoryValue = category;
            Logger logger = DefaultLogger;
            if (logger != null)
            {
                if (
                    (logger.Name == categoryValue)
                    &&
                    (logger.Level < (int)LogConstants.GetLogLevel(state)))
                {
                    return;
                }
                categoryValue = logger.Name;
            }
            if (user == string.Empty)
            {
                user = DefaultUser;
            }
            if (FileLog.IsEnabled)
            {
                lock (thisLock)
                {
                    FileLog.WriteLog(
                                     state,
                                     StackInfo(state, exception),
                                     topic,
                                     description,
                                     categoryValue,
                                     user,
                                     custom);
                }
            }
            if (DbLog.IsEnabled)
            {
                DbLog.WriteLog(
                               state,
                               StackInfo(state, exception),
                               topic,
                               description,
                               user,
                               categoryValue,
                               custom);
            }
        }
    }
}
