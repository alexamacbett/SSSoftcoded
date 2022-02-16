using UnityEngine;
using Sunless.Game.ApplicationProviders;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Sunless.Game.Audio;

namespace SSSoftcoded
{
    public static class SSSLoadingHelper
    {
        private static readonly string[] supportedWallpaperFormats = { ".png" };
        private static readonly string[] supportedSoundFormats = { ".wav", ".mp3", ".ogg" };

        private static readonly string wallpaperFilePath = GameProvider.Instance.GetApplicationPath("images/sn/wallpapers/");
        private static readonly string musicAbsoluteFilePath = GameProvider.Instance.GetApplicationPath("audio/music/");
        private static readonly string ambientSFXAbsoluteFilePath = GameProvider.Instance.GetApplicationPath("audio/ambient sfx/");
        private static readonly string regularSFXFilePath = GameProvider.Instance.GetApplicationPath("audio/sfx/");

        private static SSSLoadableResource[] customWallpapers;
        private static SSSLoadableResource[] customMusicTracks;
        private static SSSLoadableResource[] customAmbientSFX;
        private static SSSLoadableResource[] customRegularSFX;

        private static List<AudioClip> loadedAmbientSFX = new List<AudioClip>();
        private static List<AudioClip> loadedRegularSFX = new List<AudioClip>();

        public static void DocumentAllCustomContent()
        {
            //load string arrays of all custom loading screens, custom music tracks, and custom ambientfx, to be referred to at runtime
            customWallpapers = GetCustomWallpapers();
            customMusicTracks = GetCustomAssetArray(GetMusicFilePath(), supportedSoundFormats);
            customAmbientSFX = GetCustomAssetArray(GetAmbientSFXFilePath(), supportedSoundFormats);
            customRegularSFX = GetCustomAssetArray(GetRegularSFXFilePath(), supportedSoundFormats);
        }

        public static SSSLoadableResource[] GetCustomWallpapers()
        {
            List<SSSLoadableResource> wallpapers = GetCustomAssetArray(wallpaperFilePath, supportedWallpaperFormats).ToList<SSSLoadableResource>();
            //Now we want to add asset references to any of the 14 (or 10) original wallpapers that haven't been replaced
            for (int i = 1; i < (GameProvider.Instance.IsZubmariner ? 14 : 10); i++)
            {
                if (!wallpapers.Any(w => w.GetName() == i.ToString()))
                {
                    wallpapers.Add(new SSSLoadableResource(i.ToString(), ""));
                }
            }
            return wallpapers.ToArray();
        }

        public static SSSLoadableResource[] GetCustomAssetArray(string directoryPath, string[] extensions)
        {
            if (!Directory.Exists(directoryPath))
            {
                return new SSSLoadableResource[] { };
            }
            string[] allstrings = Directory.GetFiles(directoryPath, "*.*", SearchOption.TopDirectoryOnly);
            List<string> uniqueNames = new List<string>();
            List<SSSLoadableResource> returnResources = new List<SSSLoadableResource>();
            // If there are multiple files with the same name with different extensions, load the first one and then complain about the rest
            foreach (string s in allstrings)
            {
                string name = IsolateFileName(s);
                string ext = IsolateExtension(s);

                if (extensions.Contains(ext) && !uniqueNames.Contains(name))
                {
                    if (uniqueNames.Contains(name))
                    {
                        System.Console.WriteLine("Tried to document a file named " + name + ext + " for mod loading, but there is already a file in" + directoryPath + " named " + name + " with a different extension so the new file will be ignored.");
                    }
                    else
                    {
                        uniqueNames.Add(name);
                        returnResources.Add(new SSSLoadableResource(name, ext));
                    }
                }
            }
            return returnResources.ToArray();
        }

        public static Texture2D LoadWallpaperTexture(SSSLoadableResource texture)
        {
            string text = GetWallpaperFilePath() + texture.GetAddress();
            Texture2D result;
            byte[] data = File.ReadAllBytes(text);
            Texture2D texture2D = new Texture2D(2048, 1024);
            texture2D.LoadImage(data);
            result = texture2D;
            return result;
        }

        public static AudioClip GetMusic(string clipName)
        {
            foreach (SSSLoadableResource r in customMusicTracks)
            {
                if (r.GetName().ToLower() == clipName.ToLower())
                {
                    AudioClip clip = SSSAudioLoader.LoadSound(GetMusicFilePath() + r.GetAddress());
                    loadedAmbientSFX.Add(clip);
                    return clip;
                }
            }
            return null;
        }

        public static AudioClip GetAmbientSFX(string clipName)
        {
            //First check if we've already got this sound effect loaded, which could happen if it's played very frequently
            foreach (AudioClip a in loadedAmbientSFX)
            {
                if (a.name.ToLower() == clipName.ToLower())
                {
                    return a;
                }
            }
            //Otherwise, find it in the array and load it
            foreach (SSSLoadableResource r in customAmbientSFX)
            {
                if (r.GetName().ToLower() == clipName.ToLower())
                {
                    AudioClip clip = SSSAudioLoader.LoadSound(GetAmbientSFXFilePath() + r.GetAddress());
                    loadedAmbientSFX.Add(clip);
                    return clip;
                }
            }
            return null;
        }

        public static AudioClip GetRegularSFX(string clipName)
        {
            //First check if we've already got this sound effect loaded, which could happen if it's played very frequently
            foreach (AudioClip a in loadedRegularSFX)
            {
                if (a.name.ToLower() == clipName.ToLower())
                {
                    return a;
                }
            }
            //Otherwise, find it in the array and load it
            foreach (SSSLoadableResource r in customRegularSFX)
            {
                if (r.GetName().ToLower() == clipName.ToLower())
                {
                    AudioClip clip = SSSAudioLoader.LoadSound(GetRegularSFXFilePath() + r.GetAddress());
                    loadedRegularSFX.Add(clip);
                    return clip;
                }
            }
            return null;
        }

        public static bool CustomAmbientSFXExists(string name)
        {
            if (customAmbientSFX.Any(s => s.GetName().ToLower() == name.ToLower()))
            {
                return true;
            }
            return false;
        }

        public static bool CustomRegularSFXExists(string name)
        {
            if (customRegularSFX.Any(s => s.GetName().ToLower() == name.ToLower()))
            {
                return true;
            }
            return false;
        }

        public static bool UnloadAmbientSFX(string clipName)
        {
            for (int i = 0; i < loadedAmbientSFX.Count; i++)
            {
                if (loadedAmbientSFX[i].name == clipName)
                {
                    loadedAmbientSFX.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public static bool UnloadRegularSFX(string clipName)
        {
            for (int i = 0; i < loadedRegularSFX.Count; i++)
            {
                if (loadedRegularSFX[i].name == clipName)
                {
                    loadedRegularSFX.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public static SSSLoadableResource[] GetCustomWallPapers()
        {
            return customWallpapers;
        }

        public static SSSLoadableResource[] GetCustomMusicTracks()
        {
            return customMusicTracks;
        }

        public static SSSLoadableResource[] GetCustomAmbientSFX()
        {
            return customAmbientSFX;
        }

        public static string GetWallpaperFilePath()
        {
            return wallpaperFilePath;
        }

        public static string GetMusicFilePath()
        {
            return musicAbsoluteFilePath;
        }

        public static string GetAmbientSFXFilePath()
        {
            return ambientSFXAbsoluteFilePath;
        }

        public static string GetRegularSFXFilePath()
        {
            return regularSFXFilePath;
        }

        public static string IsolateFileName(string filePath)
        {
            string name = filePath.Substring(filePath.LastIndexOf('/') + 1);
            name = name.Substring(0, name.LastIndexOf('.'));

            return name;
        }

        public static string IsolateExtension(string filePath)
        {
            return filePath.Substring(filePath.LastIndexOf('.'));
        }
    }
}
