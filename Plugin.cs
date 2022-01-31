using BepInEx;
using HarmonyLib;
using Sunless.Game.Scripts.UI;
using UnityEngine;
using UnityEngine.UI;
using Sunless.Game.ApplicationProviders;
using Sunless.Game.Entities;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;

namespace SSSoftcoded
{
    [BepInPlugin("mod.clevercrumbish.SSSoftcoded", "Sunless Sea Softcoded", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Logger.LogInfo($"Loaded {PluginInfo.PLUGIN_GUID}!");
            DoPatching();
        }

        private void DoPatching()
        {
            Harmony.CreateAndPatchAll(typeof(SetupBackgroundPatch));
        }
    }

    [HarmonyPatch(typeof(LoadingScreen), "SetupBackground")]
    class SetupBackgroundPatch
    {
        static bool Prefix(LoadingScreen __instance)
        {
            GameObject gameObject = GameObject.Find("Wallpaper");
            //The numbers 1-10 in the folder will overwrite the 1-10 loading screens in the asset. All other filenames will be added to the list.
            //If Zubmariner is installed, 11-14 will overwrite. If not, they will add.
            List<string> knownLoadingScreens = GameProvider.Instance.IsZubmariner ? new List<string> { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14" } : new List<string> { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
            List<LoadableResource> loadingScreens = new List<LoadableResource>();
            foreach (string screen in LoadingHelper.GetLoadingScreens())
            {
                if (knownLoadingScreens.Contains(screen))
                {
                    loadingScreens.Add(new LoadableResource(screen, true, true));
                    knownLoadingScreens.Remove(screen);
                }
                else
                {
                    loadingScreens.Add(new LoadableResource(screen, false, true));
                }
            }
            foreach(string screen in knownLoadingScreens)
            {
                loadingScreens.Add(new LoadableResource(screen, true, false));
            }
            System.Console.WriteLine("Loaded " + loadingScreens.Count + " loading screen backgrounds to choose from.");
            int chosenLoadingScreen = UnityEngine.Random.Range(0, loadingScreens.Count - 1);
            Texture2D texture2D;
            if (loadingScreens[chosenLoadingScreen].GetFileResource())
            {
                texture2D = LoadingHelper.GetBackgroundTexture(loadingScreens[chosenLoadingScreen].GetName());
            } else
            {
                texture2D = UnityEngine.Resources.Load(LoadingHelper.GetLoadingScreenAssetPath() + loadingScreens[chosenLoadingScreen].GetName()) as Texture2D;
            }
            System.Console.WriteLine("All good before assigning texture.");
            gameObject.GetComponent<RawImage>().texture = (Texture)texture2D;
            GameObject.Find("Tip").GetComponent<Text>().text = StaticEntities.LoadingScreenTips[UnityEngine.Random.Range(0, ((IEnumerable<string>)StaticEntities.LoadingScreenTips).Count<string>())];
            __instance._spinner = GameObject.Find("Spinner").GetComponent<RectTransform>();
            return false;
        }
    }

    public static class LoadingHelper
    {
        private static string loadingScreenAbsoluteFilePath = GameProvider.Instance.GetApplicationPath("images/sn/loading screens/");
        private static string loadingScreenAssetPath = "UI/Loading/Backgrounds/";
        public static List<string> GetLoadingScreens()
        {
            if (!Directory.Exists(loadingScreenAbsoluteFilePath))
            {
                return new List<string>();
            }
            return ((IEnumerable<string>)Directory.GetFiles(loadingScreenAbsoluteFilePath, "*", SearchOption.TopDirectoryOnly)).Select<string, string>((Func<string, string>)(file => file.Replace(loadingScreenAbsoluteFilePath, "").Replace(".png", ""))).OrderBy<string, string>((Func<string, string>)(x => x)).ToList<string>();
        }

        public static Texture2D GetBackgroundTexture(string textureName)
        {
            string text = GetLoadingScreenFilePath() + textureName + ".png";
            System.Console.WriteLine("The path I am trying to load a loading screen image from is: " + text);
            Texture2D result;
            byte[] data = File.ReadAllBytes(text);
            Texture2D texture2D = new Texture2D(2048, 1024);
            texture2D.LoadImage(data);
            result = texture2D;
            return result;
        }

        public static string GetLoadingScreenFilePath()
        {
            return loadingScreenAbsoluteFilePath;
        }

        public static string GetLoadingScreenAssetPath()
        {
            return loadingScreenAssetPath;
        }
    }

    public class LoadableResource
    {
        private string _name = "";
        private bool _isAssetResource = false;
        private bool _isFileResource = false; 

        public LoadableResource(string name, bool isAssetResource, bool isFileResource)
        {
            _name = name;
            _isAssetResource = isAssetResource;
            _isFileResource = isFileResource;
        }

        public string GetName()
        {
            return _name;
        }

        public void SetName(string name)
        {
            _name = name;
        }

        public bool GetAssetResource()
        {
            return _isAssetResource;
        }

        public bool GetFileResource()
        {
            return _isFileResource;
        }

        public void SetAssetResource(bool state)
        {
            _isAssetResource = state;
        }

        public void SetFileResource(bool state)
        {
            _isFileResource = state;
        }
    }
}
