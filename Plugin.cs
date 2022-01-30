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
            int maxKnownLoadingScreens = GameProvider.Instance.IsZubmariner ? 14 : 10;
            //List<string> loadingScreens = LoadingHelper.GetLoadingScreens();
            List<string> loadingScreens = new List<string>();
            int firstAssetScreen = loadingScreens.Count; //First of the original
            for (int i = 1; i <= maxKnownLoadingScreens; i++)
            {
                string prospectiveFilename = LoadingHelper.GetLoadingScreenFilePath() + i.ToString();
                if (!loadingScreens.Contains(prospectiveFilename))
                {
                    loadingScreens.Add(i.ToString());
                }
            }
            System.Console.WriteLine("Loaded " + loadingScreens.Count + " loading screen backgrounds to choose from.");
            int chosenLoadingScreen = UnityEngine.Random.Range(0, loadingScreens.Count - 1);
            Texture2D texture2D;
            //if (chosenLoadingScreen < firstAssetScreen)
            //{
                texture2D = LoadingHelper.GetBackgroundTexture(loadingScreens[chosenLoadingScreen]);
            /*} else
            //{
            //    texture2D = UnityEngine.Resources.Load(loadingScreens[chosenLoadingScreen]) as Texture2D;
            }*/
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
}
