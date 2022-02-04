using BepInEx;
using HarmonyLib;
using Sunless.Game.Scripts.UI;
using Sunless.Game.Audio;
using UnityEngine;
using UnityEngine.UI;
using Sunless.Game.ApplicationProviders;
using Sunless.Game.Entities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;

namespace SSSoftcoded
{
    [BepInPlugin("mod.clevercrumbish.SSSoftcoded", "Sunless Sea Softcoded", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private void Start()
        {
            LoadingHelper.LoadMusic();
        }
        private void Awake()
        {
            Logger.LogInfo($"Loaded {PluginInfo.PLUGIN_GUID}!");
            DoPatching();
        }

        private void DoPatching()
        {
            Harmony.CreateAndPatchAll(typeof(SetupBackgroundPatch));
            Harmony.CreateAndPatchAll(typeof(QueueTrackPatch));
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
            foreach (string screen in knownLoadingScreens)
            {
                loadingScreens.Add(new LoadableResource(screen, true, false));
            }
            int chosenLoadingScreen = UnityEngine.Random.Range(0, loadingScreens.Count - 1);
            Texture2D texture2D;
            if (loadingScreens[chosenLoadingScreen].GetFileResource())
            {
                texture2D = LoadingHelper.GetBackgroundTexture(loadingScreens[chosenLoadingScreen].GetName());
            } else
            {
                texture2D = UnityEngine.Resources.Load(LoadingHelper.GetLoadingScreenAssetPath() + loadingScreens[chosenLoadingScreen].GetName()) as Texture2D;
            }
            gameObject.GetComponent<RawImage>().texture = (Texture)texture2D;
            GameObject.Find("Tip").GetComponent<Text>().text = StaticEntities.LoadingScreenTips[UnityEngine.Random.Range(0, ((IEnumerable<string>)StaticEntities.LoadingScreenTips).Count<string>())];
            __instance._spinner = GameObject.Find("Spinner").GetComponent<RectTransform>();
            return false;
        }
    }

    [HarmonyPatch(typeof(MusicManager), "QueueTrack")]
    class QueueTrackPatch
    {
        static bool Prefix(string clipName, bool loops, int trackPriority, float delayPeriod, MusicManager __instance)
        {
            System.Console.WriteLine("Attempting to play music.");
            AudioSource audioSource = (__instance._emptyTrack == 1) ? __instance._track02 : __instance._track01;
            //Replace this bit:
            AudioClip audioClip = null;
            //Here:
            audioClip = LoadingHelper.GetMusic(clipName);
            if (audioClip == null)
            {
                System.Console.WriteLine("Getting track " + clipName + " was a bust. Reverting to vanilla.");
                audioClip = Resources.Load("Audio/Music/" + clipName) as AudioClip;
            }
            bool flag = audioClip == null;
            if (!flag)
            {
                bool flag2 = audioSource.clip == audioClip;
                if (!flag2)
                {
                    bool flag3 = !audioSource.isPlaying && trackPriority >= __instance.CurrentTrackPriority;
                    if (flag3)
                    {
                        audioSource.clip = audioClip;
                        audioSource.loop = loops;
                    }
                    __instance.QueuedAudioclip = audioClip;
                    bool isPlaying = audioSource.isPlaying;
                    if (isPlaying)
                    {
                        //These originally said base. instead of this. and got replaced with __instance. It might not work.
                        __instance.StartCoroutine(__instance.WaitAndPlayQueued(audioSource, loops, trackPriority, delayPeriod));
                    }
                    else
                    {
                        audioSource.clip = __instance.QueuedAudioclip;
                        audioSource.loop = loops;
                        bool flag4 = !__instance._trackWaitingToPlay;
                        if (flag4)
                        {
                            //These originally said base. instead of this. and got replaced with __instance. It might not work.
                            __instance.StartCoroutine(__instance.DelayAndPlay(audioSource, 0.2f));
                        }
                        __instance.CurrentTrackPriority = trackPriority;
                    }
                }
            }
            return false;
        }
    }

    public static class LoadingHelper
    {
        private static string loadingScreenAbsoluteFilePath = GameProvider.Instance.GetApplicationPath("images/sn/loading screens/");
        private static string loadingScreenAssetPath = "UI/Loading/Backgrounds/";
        private static string musicAbsoluteFilePath = GameProvider.Instance.GetApplicationPath("audio/music/");
        private static string musicAssetPath = "Audio/Music/";
        private static GameObject wwwLoaderHolder = new GameObject();
        private static WWWLoader wwwLoader = wwwLoaderHolder.AddComponent<WWWLoader>();
        public static List<AudioClip> additionalMusic = new List<AudioClip>();


        public static void LoadMusic()
        {
            System.Console.WriteLine("Loading music");
            if (Directory.Exists(LoadingHelper.GetMusicFilePath())) {
                String[] fileNames = Directory.GetFiles(GetMusicFilePath());
                foreach (string f in fileNames)
                {
                    System.Console.WriteLine("Attempting to get music track " + f);
                    wwwLoader.GetAudio(f);
                }
                foreach (AudioClip clip in additionalMusic)
                {
                    if (clip.loadState == AudioDataLoadState.Failed)
                    {
                        clip.name = "";
                    }
                }
                additionalMusic.RemoveAll(clip => clip.name == "");
            }
        }

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
            System.Console.WriteLine("Trying to load a loading screen wallpaper from: " + text);
            Texture2D result;
            byte[] data = File.ReadAllBytes(text);
            Texture2D texture2D = new Texture2D(2048, 1024);
            texture2D.LoadImage(data);
            result = texture2D;
            return result;
        }

        public static AudioClip GetMusic(string clipName)
        {
            foreach (AudioClip clip in additionalMusic)
            {
                System.Console.WriteLine("Checking track name " + clip.name + " to see if it's the same as " + clipName);
                if (clip.name == clipName)
                {
                    return clip;
                }
            }
            return null;
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
    }

    public class WWWLoader : MonoBehaviour {
        private string _url;
        public WWW www;
        public void GetAudio(string filePath)
        {
            _url = filePath;
            StartCoroutine(LoadAudio());
        }
        IEnumerator LoadAudio()
        {
            WWW www = new WWW("file:///" + _url);
            yield return www;

            if (www.error != null)
                Debug.Log(www.error);

            AudioClip audioClip = www.GetAudioClip(false, false);
            audioClip.LoadAudioData();
            string name = _url.Substring(_url.LastIndexOf('/') + 1);
            name = name.Substring(0, name.LastIndexOf('.'));
            audioClip.name = name;

            while (audioClip.loadState == AudioDataLoadState.Loading || audioClip.loadState == AudioDataLoadState.Unloaded)
            {
                yield return 0;
            }

            if (audioClip.loadState == AudioDataLoadState.Failed)
            {
                Debug.LogError("Unable to load wav file: " + audioClip.name);
            }
            else
            {
                LoadingHelper.additionalMusic.Add(audioClip);
            }
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
