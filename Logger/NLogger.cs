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
        public static string LogFileName { get; set; } = "${shortdate}.log";
        public static string LogFileDir { get; set; } = string.Empty;

        static string LogFilePath
        {
            get { return Path.Combine(LogFileDir, LogFileName); }
        }

        static NLogger() { }

        private static NLog.Logger _logger = null;
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
            Archive();
            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("logfile")
            {
                FileName = LogFilePath,
                ArchiveNumbering = NLog.Targets.ArchiveNumberingMode.Date,
                ArchiveFileName = Path.Combine(
                    LogFileDir,
                    "Player-${date:format=yyyy.MM.dd-HH.mm.ss}.log"
                ),
                MaxArchiveFiles = 10,
                ArchiveEvery = NLog.Targets.FileArchivePeriod.Day,
                Layout = "${longdate} [${level}] ${message} ${exception:format=ToString} "
            };

            config.AddRule(LogLevel.Trace, LogLevel.Fatal, logfile);
            LogManager.Configuration = config;
        }

        // 手动归档
        public static void Archive()
        {
            if (!File.Exists(LogFilePath))
                return;

            var archiveFilePath = Path.Combine(
                LogFileDir,
                "Player-" + DateTime.Now.ToString("yyyy.MM.dd-HH.mm.ss") + ".log"
            );

            File.Move(LogFilePath, archiveFilePath);
        }

        // public static List<string> FetchMessage(int MsgCount = -1)
        // {
        //     var _memoryTarget =
        //         LogManager.Configuration.FindTargetByName<NLog.Targets.MemoryTarget>(
        //             "memoryTarget"
        //         );
        //     var _messages = _memoryTarget.Logs.ToList();
        //     if (MsgCount > 0)
        //     {
        //         return _messages.Skip(Math.Max(_messages.Count - MsgCount, 0)).ToList();
        //     }
        //     return _messages;
        // }

        public static void Shutdown()
        {
            NLog.LogManager.Shutdown();
        }
    }
}
