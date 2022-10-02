using System;
using System.Threading.Tasks;
using Fano.StepmaniaUtils.Audio.Core;
using Xunit;

namespace Fano.StepmaniaUtils.Audio.Tests
{
    public class StepmaniaUtilsAudioTests
    {
        [Fact]
        public async Task FfmpegTest()
        {
            await StepmaniaUtilsExt.CreatePreviewClips(@"G:\Stepmania\Songs", @"ffmpeg.exe");
        }
    }
}
