using System;
using System.IO;
using System.Threading.Tasks;
using Fano.StepmaniaUtils.Audio.Core;

namespace Fano.StepmaniaUtils.Audio.Runner
{
    public static class Program
    {
        private static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("USAGE: preview-music.exe <path-to-songs-directory>");
            }
            else if (args.Length == 1)
            {
                if (!File.Exists(@"ffmpeg.exe"))
                {
                    Console.WriteLine("Please include ffmpeg.exe in the current directory");
                }
                else if (!Directory.Exists(args[0]))
                {
                    Console.WriteLine($"given song directory {args[0]} does not exist.");
                }
                else
                {
                    await StepmaniaUtilsExt.CreatePreviewClips(args[0], @"ffmpeg.exe");
                }
            }
        }
    }
}
