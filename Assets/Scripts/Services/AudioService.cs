using System;
using System.Linq;
using UnityEngine;

namespace LunraGames.SpaceFarm
{
	public class AudioService
	{
		GameObject audioRoot;

		public AudioService(GameObject audioRoot) 
		{
			this.audioRoot = audioRoot;
		}

		// TODO: Don't return an AudioSource, return something wrapping it instead so this service can safely null it.
		public AudioSource PlayClip(AudioClip clip, bool looping = false) 
		{
			// TODO: Non leaky audio.
			Debug.LogWarning("Proper audio system not created yet!");
			var source = audioRoot.AddComponent<AudioSource>();
			source.clip = clip;
			source.Play();
			return source;
		}
	}
}