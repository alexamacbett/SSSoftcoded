using BepInEx;
using HarmonyLib;
using Sunless.Game.Scripts.UI;
using Sunless.Game.Audio;
using UnityEngine;
using UnityEngine.UI;
using Sunless.Game.Entities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Audio;
using Sunless.Game.ApplicationProviders;
using System;

namespace SSSoftcoded
{
    [BepInPlugin("mod.clevercrumbish.SSSoftcoded", "Sunless Sea Softcoded", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private void Start()
        {
            SSSLoadingHelper.Initialise();
            SSSLoadingHelper.DocumentAllCustomContent();
        }
        private void Awake()
        {
            Logger.LogInfo($"Loaded {PluginInfo.PLUGIN_GUID} {PluginInfo.PLUGIN_VERSION}!");
            DoPatching();
        }

        private void DoPatching()
        {
            Harmony.CreateAndPatchAll(typeof(SetupBackgroundPatch));
            Harmony.CreateAndPatchAll(typeof(QueueTrackPatch));
            Harmony.CreateAndPatchAll(typeof(PlayAmbientFXPatch));
            Harmony.CreateAndPatchAll(typeof(StopAmbientFXPatch));
            Harmony.CreateAndPatchAll(typeof(PlayRegularSFXPatch));
            Harmony.CreateAndPatchAll(typeof(StopRegularSFXPatch));
        }
    }

    [HarmonyPatch(typeof(LoadingScreen), "SetupBackground")]
    class SetupBackgroundPatch
    {
        static bool Prefix(LoadingScreen __instance)
        {
            GameObject gameObject = GameObject.Find("Wallpaper");
            SSSLoadableResource[] wallpapers = SSSLoadingHelper.GetCustomWallPapers();
            SSSLoadableResource chosenWallpaper = wallpapers[UnityEngine.Random.Range(0, wallpapers.Length - 1)];
            Texture2D texture2D;
            if (chosenWallpaper.GetExtension() != "")
            {
                texture2D = SSSLoadingHelper.LoadWallpaperTexture(chosenWallpaper);
            } else
            {
                texture2D = UnityEngine.Resources.Load("UI/Loading/Backgrounds/" + chosenWallpaper.GetName()) as Texture2D;
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
            audioSource.clip = SSSLoadingHelper.GetAmbientSFX(clipName);
            if (audioSource.clip == null)
            {
                audioSource.clip = (Resources.Load("Audio/AmbientFX/" + clipName) as AudioClip);
                if (loops)
                {
                    audioSource.Play();
                }
                else
                {
                    __instance.StartCoroutine(__instance.PlayOnce(audioSource));
                }
            } else
            {
                if (loops)
                {
                    audioSource.Play();
                }
                else
                {
                    SSSAudioPlayer audioPlayer = __instance.gameObject.AddComponent<SSSAudioPlayer>();
                    audioPlayer.StartCoroutine(audioPlayer.PlayExternalAmbientSFXOnce(audioSource, __instance));
                    UnityEngine.Object.Destroy(audioPlayer);
                }
            }
            __instance._currentFX.Add(audioSource);
            __result = audioSource;
            return false;
        }
    }

    [HarmonyPatch(typeof(SFXManager), "Play", new Type[] { typeof(string), typeof(bool)})]
    class PlayRegularSFXPatch
    {
        static bool Prefix(string clipName, bool loops, SFXManager __instance, ref AudioSource __result)
        {
            AudioSource audioSource = __instance.gameObject.AddComponent<AudioSource>();
            audioSource.outputAudioMixerGroup = __instance.MasterMixer.FindMatchingGroups(SFXManager.SFX_MIXER_NAME).FirstOrDefault<AudioMixerGroup>();
            audioSource.loop = loops;
            audioSource.bypassEffects = true;
            audioSource.spatialBlend = 0f;
            audioSource.clip = SSSLoadingHelper.GetRegularSFX(clipName);
            if (audioSource.clip == null)
            {
                audioSource.clip = (Resources.Load("Audio/SFX/" + clipName) as AudioClip);
                if (loops)
                {
                    audioSource.Play();
                }
                else
                {
                    __instance.StartCoroutine(__instance.PlayOnce(audioSource));
                }
            } else
            {
                if (loops)
                {
                    audioSource.Play();
                }
                else
                {
                    SSSAudioPlayer audioPlayer = __instance.gameObject.AddComponent<SSSAudioPlayer>();
                    audioPlayer.StartCoroutine(audioPlayer.PlayExternalRegularSFXOnce(audioSource, __instance));
                    UnityEngine.Object.Destroy(audioPlayer);
                }
            }
            __instance._currentFX.Add(audioSource);
            __result = audioSource;
            return false;
        }
    }

    [HarmonyPatch(typeof(AmbientFXManager), "Stop")]
    class StopAmbientFXPatch
    {
        static bool Prefix(AudioSource source, AmbientFXManager __instance)
        {
            __instance._currentFX.Remove(source);
            bool flag = source == null || source.gameObject == null;
            if (!flag)
            {
                bool custom = SSSLoadingHelper.CustomAmbientSFXExists(source.clip.name);
                bool isPlaying = source.isPlaying;
                if (isPlaying)
                {
                    source.Stop();
                }
                if (custom)
                {
                    SSSLoadingHelper.UnloadAmbientSFX(source.clip.name);
                }
                else
                {
                    Resources.UnloadAsset(source.clip);
                }
                UnityEngine.Object.Destroy(source);
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(SFXManager), "Stop")]
    class StopRegularSFXPatch
    {
        static bool Prefix(AudioSource source, SFXManager __instance)
        {
            bool flag = GameProvider.Instance.EditorMode && source == null;
            if (!flag)
            {
                __instance._currentFX.Remove(source);
                bool flag2 = source == null || source.gameObject == null;
                if (!flag2)
                {
                    bool custom = SSSLoadingHelper.CustomRegularSFXExists(source.clip.name);
                    bool isPlaying = source.isPlaying;
                    if (isPlaying)
                    {
                        source.Stop();
                    }
                    if (custom)
                    {
                        SSSLoadingHelper.UnloadRegularSFX(source.clip.name);
                    }
                    else
                    {
                        Resources.UnloadAsset(source.clip);
                    }
                    UnityEngine.Object.Destroy(source);
                }
            }
            return false;
        }
    }
}
