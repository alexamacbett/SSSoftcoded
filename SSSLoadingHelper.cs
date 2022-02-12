using UnityEngine;
using Sunless.Game.ApplicationProviders;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;

namespace SSSoftcoded
{
    public static class SSSLoadingHelper
    {
        private static string loadingScreenAbsoluteFilePath = GameProvider.Instance.GetApplicationPath("images/sn/wallpapers/");
        private static string loadingScreenAssetPath = "UI/Loading/Backgrounds/";
        private static string musicAbsoluteFilePath = GameProvider.Instance.GetApplicationPath("audio/music/");
        private static string musicAssetPath = "Audio/Music/";
        private static string ambientSFXAbsoluteFilePath = GameProvider.Instance.GetApplicationPath("audio/ambient sfx/");
        private static string ambientSFXAssetPath = "Audio/AmbientFX/";

        public static List<string> GetLoadingScreens()
        {
            if (!Directory.Exists(loadingScreenAbsoluteFilePath))
            {
                return new List<string>();
            }
            return ((IEnumerable<string>)Directory.GetFiles(loadingScreenAbsoluteFilePath, "*.png", SearchOption.TopDirectoryOnly)).Select<string, string>((Func<string, string>)(file => file.Replace(loadingScreenAbsoluteFilePath, "").Replace(".png", ""))).OrderBy<string, string>((Func<string, string>)(x => x)).ToList<string>();
        }

        public static Texture2D GetBackgroundTexture(string textureName)
        {
            string text = GetLoadingScreenFilePath() + textureName + ".png";
            Texture2D result;
            byte[] data = File.ReadAllBytes(text);
            Texture2D texture2D = new Texture2D(2048, 1024);
            texture2D.LoadImage(data);
            result = texture2D;
            return result;
        }

        public static AudioClip GetMusic(string clipName)
        {
            if (Directory.Exists(GetMusicFilePath()))
            {
                string[] fileNames = GetFileNamesByExtension(GetMusicFilePath(), new string[] { ".wav", ".ogg", ".mp3" });
                foreach (string f in fileNames)
                {
                    string fName =  f.Substring(f.LastIndexOf('/') + 1);
                    fName = fName.Substring(0, fName.LastIndexOf('.'));
                    System.Console.WriteLine("fName is " + fName + ". clipName is " + clipName);
                    if (fName == clipName)
                    { 
                        return SSSAudioLoader.LoadSound(f);
                    }
                }
            }
            return null;
        }

        public static string[] GetFileNamesByExtension(string directoryPath, string[] extensions)
        {
            string[] allFileNames = Directory.GetFiles(directoryPath);
            List<string> returnAbleFileNames = new List<string>();
            foreach (string f in allFileNames)
            {
                foreach (string e in extensions)
                {
                    if (f.EndsWith(e))
                    {
                        returnAbleFileNames.Add(f);
                        break;
                    }
                }
            }
            return returnAbleFileNames.ToArray();
        }

        public static string GetLoadingScreenFilePath()
        {
            return loadingScreenAbsoluteFilePath;
        }

        public static string GetLoadingScreenAssetPath()
        {
            return loadingScreenAssetPath;
        }

        public static string GetMusicFilePath()
        {
            return musicAbsoluteFilePath;
        }

        public static string GetMusicAssetPath()
        {
            return musicAssetPath;
        }

        public static string GetAmbientSFXFilePath()
        {
            return ambientSFXAbsoluteFilePath;
        }

        public static string GetAmbientSFXAssetPath()
        {
            return ambientSFXAssetPath;
        }

        public static string IsolateFileName(string filePath)
        {
            string name = filePath.Substring(filePath.LastIndexOf('/') + 1);
            name = name.Substring(0, name.LastIndexOf('.'));

            return name;
        }
    }
}
