using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FFmpeg.NET;
using StepmaniaUtils;
using StepmaniaUtils.Enums;

namespace Fano.StepmaniaUtils.Audio.Core
{
    public static class StepmaniaUtilsExt
    {
        public static async Task CreatePreviewClips(string songsFolderPath, string ffmpegPath)
        {
            // Parse .sm files asynchronously
            var fileQueue = new BlockingCollection<string>();

            var producer = Task.Run(() =>
            {
                foreach (var file in Directory.EnumerateFiles(songsFolderPath, "*.sm", SearchOption.AllDirectories).AsParallel())
                {
                    fileQueue.Add(file);
                }

                fileQueue.CompleteAdding();
            });
            
            var ffmpeg = new Engine(ffmpegPath);
            var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "preview-clip-output");
            Directory.CreateDirectory(outputPath);
            
            // Parse .sm file and cut preview clip in the same step
            var consumers = Enumerable.Range(0, Environment.ProcessorCount * 2)
                .Select(_ => Task.Run(() =>
                {
                    foreach (var file in fileQueue.GetConsumingEnumerable())
                    {
                        try
                        {
                            var song = new SmFile(file);
                            var songPath = Path.Combine(songsFolderPath, song.Group, song.Directory, song[SmFileAttribute.MUSIC]);
                            Console.WriteLine($"Creating Preview Clip for:{songPath}");

                            var inputFile = new InputFile(songPath);
                            var outputFile = new OutputFile(Path.Combine(outputPath, song.Group, $"{song.SongTitle}{inputFile.FileInfo.Extension}"));

                            if (!Directory.Exists(outputFile.FileInfo.Directory?.FullName))
                            {
                                Directory.CreateDirectory(outputFile.FileInfo.Directory?.FullName);
                            }

                            Console.WriteLine($"Output File Path: {outputFile.FileInfo.FullName}");

                            var sampleStart = double.Parse(song[SmFileAttribute.SAMPLESTART]);
                            var sampleLength = double.Parse(song[SmFileAttribute.SAMPLELENGTH]);
                
                            var options = new ConversionOptions
                            {
                                Seek = TimeSpan.FromSeconds(sampleStart),
                                MaxVideoDuration = TimeSpan.FromSeconds(sampleLength),
                            };
                            
                            ffmpeg.ConvertAsync(inputFile, outputFile, options, CancellationToken.None);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Error creating preview clip for file at: {file}\n{e.Message}");
                        }
                    }
                }));
            
            await Task.WhenAll(consumers);
        }
    }
}

