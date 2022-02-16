using UnityEngine;
using System.Collections;
using Sunless.Game.Audio;

namespace SSSoftcoded
{
	public class SSSAudioPlayer : MonoBehaviour
	{
		public IEnumerator PlayExternalAmbientSFXOnce(AudioSource source, AmbientFXManager aFXM)
		{
			source.Play();
			while ((UnityEngine.Object)source != (UnityEngine.Object)null && source.isPlaying)
				yield return (object)new WaitForSeconds(1f);
			aFXM._currentFX.Remove(source);
			if ((UnityEngine.Object)source != (UnityEngine.Object)null)
			{
				SSSLoadingHelper.UnloadAmbientSFX(source.clip.name);
				UnityEngine.Object.Destroy((UnityEngine.Object)source);
			}
		}

		public IEnumerator PlayExternalRegularSFXOnce(AudioSource source, SFXManager sFXM)
		{
			source.Play();
			while ((UnityEngine.Object)source != (UnityEngine.Object)null && source.isPlaying)
				yield return (object)new WaitForSeconds(1f);
			sFXM._currentFX.Remove(source);
			if ((UnityEngine.Object)source != (UnityEngine.Object)null)
			{
				SSSLoadingHelper.UnloadRegularSFX(source.clip.name);
				UnityEngine.Object.Destroy((UnityEngine.Object)source);
			}
		}
	}
}