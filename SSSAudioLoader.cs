using UnityEngine;
using NAudio.Wave;
using NVorbis.NAudioSupport;

namespace SSSoftcoded
{
	public static class SSSAudioLoader
	{
		public static AudioClip LoadSound(string filePath)
		{
			if (filePath.EndsWith(".ogg"))
            {
				return LoadOgg(filePath);
            } else
            {
				return LoadWavOrMp3(filePath);
            }
		}

		public static AudioClip LoadWavOrMp3(string filePath)
        {
			AudioFileReader aud = new AudioFileReader(filePath);
			float[] audioData = new float[aud.Length / 4];
			aud.Read(audioData, 0, (int)aud.Length);
			AudioClip clip = AudioClip.Create(SSSLoadingHelper.IsolateFileName(filePath), (int)(audioData.Length / aud.WaveFormat.Channels), aud.WaveFormat.Channels, aud.WaveFormat.SampleRate, false);
			clip.SetData(audioData, 0);
			return clip;
		}

		public static AudioClip LoadOgg(string filePath)
        {
			/* There is an easier way of doing this with NVorbis alone but since NVorbis is horrendously undocumented, 
			 * nobody answered my question about how it works on StackOverflow and I don't know enough about how streams
			 * are meant to work to intuit it myself, we'll use NVorbis.NAudioSupport instead because NAudio has actual
			 * documentation that you can follow and implement from.*/
			AudioClip clip;
			VorbisWaveReader stream = new VorbisWaveReader(filePath);
			int channels = stream.WaveFormat.Channels;
			int sampleRate = stream.WaveFormat.SampleRate;
			float[] audioData = new float[stream.Length / 4];
			while (true)
			{
				int bytesRead = stream.Read(audioData, 0, audioData.Length);
				if (bytesRead == 0)
					break;
			}
			clip = AudioClip.Create(SSSLoadingHelper.IsolateFileName(filePath), (int)(audioData.Length / channels), channels, sampleRate, false);
			clip.SetData(audioData, 0);
			return clip;
		}

	}
}
