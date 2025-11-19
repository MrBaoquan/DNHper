using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;

namespace DNHper
{
    public static class NLogger
    {
        const string configName = "NLog.config.xml";
        const string defaultLogFileName = "${shortdate}.log";

        private static string logFileDir = string.Empty;
        public static string LogFileDir
        {
            get { return logFileDir; }
            set { logFileDir = value; }
        }

        private static string logFileName = string.Empty;
        public static string LogFileName
        {
            get { return logFileName; }
            set { logFileName = value; }
        }

        static string LogFilePath
        {
            get
            {
                var fileName = string.IsNullOrEmpty(logFileName) ? defaultLogFileName : logFileName;
                return Path.Combine(LogFileDir, fileName);
            }
        }

        static string ArchiveFilePath
        {
            get
            {
                // 如果logFileName为空，使用默认日期格式
                var fileName = string.IsNullOrEmpty(logFileName) ? defaultLogFileName : logFileName;
                // 获取文件名（不含扩展名）和扩展名
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                var extension = Path.GetExtension(fileName);
                // 如果logFileName包含NLog变量（如${shortdate}），则直接使用日期时间格式
                if (fileNameWithoutExt.Contains("${"))
                {
                    return Path.Combine(
                        LogFileDir,
                        "${date:format=yyyy.MM.dd_HH.mm.ss}" + extension
                    );
                }
                // 否则使用 LogFileName-日期时间.扩展名 格式
                return Path.Combine(
                    LogFileDir,
                    $"{fileNameWithoutExt}-${{date:format=yyyy.MM.dd_HH.mm.ss}}{extension}"
                );
            }
        }

        static NLogger() { }

        private static NLog.Logger? _logger;
        private static NLog.Logger logger
        {
            get
            {
                if (_logger == null)
                {
                    _logger = NLog.LogManager.GetCurrentClassLogger();
                }
                return _logger;
            }
        }

        public static void Info(object InMessage)
        {
            logger.Info(InMessage);
        }

        public static void Info(string InMessage)
        {
            logger.Info(InMessage);
        }

        public static void Info(string InFormat, params object[] InParams)
        {
            logger.Info(InFormat, InParams);
        }

        public static void Debug(object InMessage)
        {
            logger.Debug(InMessage);
        }

        public static void Debug(string InMessage)
        {
            logger.Debug(InMessage);
        }

        public static void Debug(string InFormat, params object[] InParams)
        {
            logger.Debug(InFormat, InParams);
        }

        public static void Warn(string InMessage)
        {
            logger.Warn(InMessage);
        }

        public static void Warn(string InFormat, params object[] InParams)
        {
            logger.Warn(InFormat, InParams);
        }

        public static void Error(string InMessage)
        {
            logger.Error(InMessage);
        }

        public static void Error(string InFormat, params object[] InParams)
        {
            logger.Error(InFormat, InParams);
        }

        public static void Error(Exception InEx, string InMessage = "")
        {
            logger.Error(InEx, InMessage);
        }

        public static void Initialize()
        {
            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("logfile")
            {
                FileName = LogFilePath,
                ArchiveFileName = ArchiveFilePath,
                ArchiveOldFileOnStartup = true,
                MaxArchiveFiles = 10,
                ArchiveEvery = NLog.Targets.FileArchivePeriod.Day,
                Layout =
                    "${longdate} ${level} ${message} ${exception:format=Message} ${exception:format=StackTrace:exceptionDataSeparator=\r\n}"
            };
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, logfile);

            var _memoryTarget = new NLog.Targets.MemoryTarget("memoryTarget")
            {
                Layout =
                    "${longdate} [${level}]: ${message} ${exception:format=Message} ${exception:format=StackTrace:exceptionDataSeparator=\r\n}"
            };
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, _memoryTarget);
            NLog.LogManager.Configuration = config;
        }

        public static List<string> FetchMessage(int MsgCount = -1)
        {
            var _memoryTarget =
                LogManager.Configuration?.FindTargetByName<NLog.Targets.MemoryTarget>(
                    "memoryTarget"
                );
            if (_memoryTarget == null)
            {
                return new List<string>();
            }
            var _messages = _memoryTarget.Logs.ToList();
            if (MsgCount > 0)
            {
                return _messages.Skip(Math.Max(_messages.Count - MsgCount, 0)).ToList();
            }
            return _messages;
        }

        public static void Shutdown()
        {
            NLog.LogManager.Shutdown();
        }
    }
}
