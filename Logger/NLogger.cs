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
                ArchiveFileName = ArchiveFilePath,
                ArchiveOldFileOnStartup = true,
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
        public static void Info(string message) => logger.Info(message);

        public static void Info(string format, params object[] args) => logger.Info(format, args);

        public static void Debug(string message) => logger.Debug(message);

        public static void Debug(string format, params object[] args) => logger.Debug(format, args);

        public static void Warn(string message) => logger.Warn(message);

        public static void Warn(string format, params object[] args) => logger.Warn(format, args);

        public static void Error(string message) => logger.Error(message);

        public static void Error(string format, params object[] args) => logger.Error(format, args);

        public static void Fatal(string message) => logger.Fatal(message);

        public static void Fatal(string format, params object[] args) => logger.Fatal(format, args);

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
