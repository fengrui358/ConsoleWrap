using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommandLine;
using NLog;

namespace ConsoleWrap
{
    /// <summary>
    /// 控制台程序的基础框架
    /// </summary>
    class Program
    {
        private static string _filePath;
        private static string _title;

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        static async Task Main(string[] args)
        {
            //处理参数
            void Header(IOptions opts)
            {
                if (!string.IsNullOrEmpty(opts.FilePath))
                {
                    if (File.Exists(opts.FilePath))
                    {
                        _filePath = opts.FilePath;
                    }
                    else
                    {
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), opts.FilePath);
                        if (File.Exists(opts.FilePath))
                        {
                            _filePath = opts.FilePath;
                        }
                    }
                }

                _title = opts.Title;
            }

            var result = Parser.Default.ParseArguments<HeadOptions>(args);
            result.WithParsed(Header);

            if (_filePath != null && File.Exists(_filePath))
            {
                try
                {
                    //清理目录下的原有日志
                    var directory = new FileInfo(_filePath).Directory;
                    var logs = directory?.GetFiles("natapp.log*", SearchOption.TopDirectoryOnly);

                    foreach (var fileInfo in logs)
                    {
                        fileInfo.Delete();
                    }

                    var sb = new StringBuilder();

                    RunProcess(_filePath);

                    sb.AppendLine(_title);
                    sb.AppendLine($"执行{Path.GetFileName(_filePath)}");
                    //sb.AppendLine(s);

                    //延迟5秒读取日志
                    await Task.Delay(5000);

                    var log = directory?.GetFiles("natapp.log*", SearchOption.TopDirectoryOnly).FirstOrDefault();
                    if (log == null)
                    {
                        throw new Exception("读取日志失败");
                    }
                    else
                    {
                        var regex = new Regex("\"Url\":\"(.+?)\"");

                        var fs = new FileStream(log.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        using (var sr = new StreamReader(fs))
                        {
                            var content = sr.ReadToEnd();

                            var match = regex.Match(content);
                            if (match.Success && match.Groups.Count == 2)
                            {
                                var url = match.Groups[1].Value;
                                sb.AppendLine(url);
                            }
                        }
                    }

                    Logger.Info(sb.ToString);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Logger.Error(e);
                }
            }
            else
            {
                Console.WriteLine($"文件不存在:{_filePath}");
            }
        }

        /// <summary>
        /// 获取Cmd进程
        /// </summary>
        /// <returns></returns>
        private static Process RunProcess(string filePath)
        {
            var proc = new Process
            {
                StartInfo =
                {
                    FileName = filePath,
                    UseShellExecute = false,
                    //RedirectStandardInput = true,
                    //RedirectStandardError = true,
                    //RedirectStandardOutput = true,
                    CreateNoWindow = false
                }
            };

            proc.Start();
            return proc;
        }
    }
}
