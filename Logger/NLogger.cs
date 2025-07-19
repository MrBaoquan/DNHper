using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;

namespace DNHper
{
    public static class NLogger
    {
        public static string LogFileName { get; set; } = "${shortdate}.log";
        public static string LogFileDir { get; set; } = AppDomain.CurrentDomain.BaseDirectory;

        private static string LogFilePath => Path.Combine(LogFileDir, LogFileName);

        private static Logger _logger = null;
        private static Logger LoggerInstance
        {
            get
            {
                if (_logger == null)
                {
                    _logger = LogManager.GetCurrentClassLogger();
                }
                return _logger;
            }
        }

        /// <summary>
        /// 初始化日志配置，包含FileTarget和MemoryTarget
        /// </summary>
        public static void Initialize()
        {
            // 确保日志目录存在
            if (!Directory.Exists(LogFileDir))
            {
                Directory.CreateDirectory(LogFileDir);
            }

            var config = new NLog.Config.LoggingConfiguration();

            // 文件日志目标
            var logfile = new NLog.Targets.FileTarget("logfile")
            {
                FileName = LogFilePath,
                ArchiveNumbering = NLog.Targets.ArchiveNumberingMode.Date,
                ArchiveFileName = Path.Combine(
                    LogFileDir,
                    "Archive",
                    "Player-${date:format=yyyy.MM.dd-HH.mm.ss}.log"
                ),
                MaxArchiveFiles = 10,
                ArchiveEvery = NLog.Targets.FileArchivePeriod.Day,
                Layout = "${longdate} [${level}] - ${message} ${exception:format=ToString}"
            };

            // 内存日志目标
            var memoryTarget = new NLog.Targets.MemoryTarget("memoryTarget")
            {
                Layout = "${longdate} [${level}] - ${message} ${exception:format=ToString}"
            };

            // 添加targets
            config.AddTarget(logfile);
            config.AddTarget(memoryTarget);

            // 设置规则，所有日志同时写入文件和内存
            config.AddRuleForAllLevels(logfile);
            config.AddRuleForAllLevels(memoryTarget);

            // 应用配置
            LogManager.Configuration = config;
        }

        /// <summary>
        /// 获取内存中缓存的日志消息，可以指定数量，默认全部
        /// </summary>
        /// <param name="msgCount">要获取的最近日志条数，默认为全部</param>
        /// <returns>日志文本列表</returns>
        public static List<string> FetchMessage(int msgCount = -1)
        {
            var memoryTarget =
                LogManager.Configuration?.FindTargetByName<NLog.Targets.MemoryTarget>(
                    "memoryTarget"
                );
            if (memoryTarget == null)
            {
                // 内存目标未找到，返回空列表
                return new List<string>();
            }

            var allMessages = memoryTarget.Logs;
            if (msgCount > 0 && allMessages.Count > msgCount)
            {
                return allMessages.Skip(allMessages.Count - msgCount).ToList();
            }

            return new List<string>(allMessages);
        }

        // 简单封装日志写入接口
        public static void Info(string message) => LoggerInstance.Info(message);

        public static void Info(string format, params object[] args) =>
            LoggerInstance.Info(format, args);

        public static void Debug(string message) => LoggerInstance.Debug(message);

        public static void Debug(string format, params object[] args) =>
            LoggerInstance.Debug(format, args);

        public static void Warn(string message) => LoggerInstance.Warn(message);

        public static void Warn(string format, params object[] args) =>
            LoggerInstance.Warn(format, args);

        public static void Error(string message) => LoggerInstance.Error(message);

        public static void Error(string format, params object[] args) =>
            LoggerInstance.Error(format, args);

        public static void Fatal(string message) => LoggerInstance.Fatal(message);

        public static void Fatal(string format, params object[] args) =>
            LoggerInstance.Fatal(format, args);

        /// <summary>
        /// 关闭日志管理器释放资源
        /// </summary>
        public static void Shutdown()
        {
            LogManager.Shutdown();
            _logger = null;
        }
    }
}
