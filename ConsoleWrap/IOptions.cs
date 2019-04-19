using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace ConsoleWrap
{
    interface IOptions
    {
        [Option('f', "file",
            HelpText = "Input file to be execute.",
            Required = true)]
        string FilePath { get; set; }

        [Option('t', "title",
            Default = "Program execute",
            HelpText = "Describe email title")]
        string Title { get; set; }
    }

    /// <summary>
    /// 子命令需要加入Verb，如果是单独的命令可不需要
    /// </summary>
    class HeadOptions : IOptions
    {
        public string FilePath { get; set; }
        public string Title { get; set; }

        [Usage(ApplicationAlias = "ReadText.Demo.exe")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("normal scenario", new HeadOptions { FilePath = "file.exe", Title = "执行 file.exe"});
            }
        }
    }
}