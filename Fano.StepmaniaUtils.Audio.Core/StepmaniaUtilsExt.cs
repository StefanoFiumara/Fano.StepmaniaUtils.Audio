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
            var smFiles = new ConcurrentBag<SmFile>();
            
            var producer = Task.Run(() =>
            {
                foreach (var file in Directory.EnumerateFiles(songsFolderPath, "*.sm", SearchOption.AllDirectories).AsParallel())
                {
                    fileQueue.Add(file);
                }

                fileQueue.CompleteAdding();
            });
            
            var consumers = Enumerable.Range(0, Environment.ProcessorCount * 2)
                .Select(_ => Task.Run(() =>
                {
                    foreach (var file in fileQueue.GetConsumingEnumerable())
                    {
                        try
                        {
                            var song = new SmFile(file);
                            smFiles.Add(song);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Error loading file at: {file}\n{e.Message}");
                        }
                    }
                }));
            
            await Task.WhenAll(consumers);

            var ffmpeg = new Engine(ffmpegPath);
            var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "preview-clip-output");

            Directory.CreateDirectory(outputPath);

            foreach (var smFile in smFiles)
            {
                var songPath = Path.Combine(songsFolderPath, smFile.Group, smFile.Directory, smFile[SmFileAttribute.MUSIC]);
                // Console.WriteLine($"Creating Preview Clip for:{songPath}");

                var inputFile = new InputFile(songPath);
                var outputFile = new OutputFile(Path.Combine(outputPath, smFile.Group, $"{smFile.SongTitle}{inputFile.FileInfo.Extension}"));

                if (!Directory.Exists(outputFile.FileInfo.Directory?.FullName))
                {
                    Directory.CreateDirectory(outputFile.FileInfo.Directory?.FullName);
                }

                Console.WriteLine($"Output File Path: {outputFile.FileInfo.FullName}");

                var sampleStart = double.Parse(smFile[SmFileAttribute.SAMPLESTART]);
                var sampleLength = double.Parse(smFile[SmFileAttribute.SAMPLELENGTH]);
                
                var options = new ConversionOptions
                {
                    Seek = TimeSpan.FromSeconds(sampleStart),
                    MaxVideoDuration = TimeSpan.FromSeconds(sampleLength),
                };

                await ffmpeg.ConvertAsync(inputFile, outputFile, options, CancellationToken.None);
            }
            
            
            // TODO: Parse preview clip start and length
            // TODO: Use FFMPEG to create new audio clip from SmFile mp3
            // TODO: Save new preview audio clip to new folder
        }
    }
}

