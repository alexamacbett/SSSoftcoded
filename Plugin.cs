using BepInEx;
using HarmonyLib;
using Sunless.Game.Scripts.UI;
using Sunless.Game.Audio;
using UnityEngine;
using UnityEngine.UI;
using Sunless.Game.ApplicationProviders;
using Sunless.Game.Entities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Audio;

namespace SSSoftcoded
{
    [BepInPlugin("mod.clevercrumbish.SSSoftcoded", "Sunless Sea Softcoded", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private void Start()
        {
            //SSSLoadingHelper.LoadMusic();
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
            foreach (string screen in SSSLoadingHelper.GetLoadingScreens())
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
                texture2D = SSSLoadingHelper.GetBackgroundTexture(loadingScreens[chosenLoadingScreen].GetName());
            } else
            {
                texture2D = UnityEngine.Resources.Load(SSSLoadingHelper.GetLoadingScreenAssetPath() + loadingScreens[chosenLoadingScreen].GetName()) as Texture2D;
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
            AudioSource audioSource = (__instance._emptyTrack == 1) ? __instance._track02 : __instance._track01;
            AudioClip audioClip;
            audioClip = SSSLoadingHelper.GetMusic(clipName);
            if (audioClip == null)
            {
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
                        __instance.StartCoroutine(__instance.WaitAndPlayQueued(audioSource, loops, trackPriority, delayPeriod));
                    }
                    else
                    {
                        audioSource.clip = __instance.QueuedAudioclip;
                        audioSource.loop = loops;
                        bool flag4 = !__instance._trackWaitingToPlay;
                        if (flag4)
                        {
                            __instance.StartCoroutine(__instance.DelayAndPlay(audioSource, 0.2f));
                        }
                        __instance.CurrentTrackPriority = trackPriority;
                    }
                }
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(AmbientFXManager), "Play")]
    class PlayAmbientFXPatch {
        static bool Prefix(AmbientFXManager __instance, ref AudioSource __result, string clipName, bool loops, GameObject targetGameOjbect, float? maxDistance = null, float? panLevel = null)
        {
            AudioSource audioSource = targetGameOjbect.AddComponent<AudioSource>();
            audioSource.outputAudioMixerGroup = __instance.MasterMixer.FindMatchingGroups(AmbientFXManager.SFX_MIXER_NAME).FirstOrDefault<AudioMixerGroup>();
            audioSource.loop = loops;
            audioSource.dopplerLevel = 0f;
            audioSource.rolloffMode = AudioRolloffMode.Custom;
            audioSource.maxDistance = (maxDistance ?? 400f);
            audioSource.spatialBlend = (panLevel ?? 1f);
            //Check if the Ambient SFX is loaded from elsewhere
            audioSource.clip = (Resources.Load("Audio/AmbientFX/" + clipName) as AudioClip);
            if (loops)
            {
                audioSource.Play();
            }
            else
            {
                __instance.StartCoroutine(__instance.PlayOnce(audioSource));
            }
            __instance._currentFX.Add(audioSource);
            __result = audioSource;
            return false;
        }
    }
}
