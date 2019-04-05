using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Audio;

namespace LunraGames.SubLight
{
	public class AudioService
	{
		public enum Groups
		{
			Unknown = 0,
			User = 10,

			Ambient = 100,

			Effects = 200,
			EffectsInterface = 210,

			Music = 300
		}

		public enum States
		{
			Unknown = 0,
			Playing = 10,
			Stopped = 20
		}

		public interface IAudioInstance
		{
			string Id { get; }
			AudioClip Clip { get; }
			Groups Group { get; }
			bool Looping { get; }
			States State { get; }

			//void Stop(float duration, Action<RequestResult> done);
		}

		class AudioInstance : IAudioInstance
		{
			public string Id { get; set; }
			public AudioClip Clip { get; set; }
			public Groups Group { get; set; }
			public bool Looping { get; set; }
			public States State { get; set; }

			public AudioSource Source { get; set; }
			public Action<IAudioInstance> Done { get; set; }

			public AudioService AudioServiceInstance;

			//public void Stop(float duration, Action<RequestResult> done)
			//{

			//}
		}

		const int AudioSourcePoolCount = 8;

		GameObject audioRoot;
		AudioConfiguration audioConfiguration;
		Heartbeat heartbeat;
		MetaKeyValueService metaKeyValues;
		BuildPreferences buildPreferences;

		List<AudioSource> pool = new List<AudioSource>();
		List<AudioInstance> activeInstances = new List<AudioInstance>();

		public AudioService(
			GameObject audioRoot,
			AudioConfiguration audioConfiguration,
			Heartbeat heartbeat,
			MetaKeyValueService metaKeyValues,
			BuildPreferences buildPreferences
		)
		{
			if (audioRoot == null) throw new ArgumentNullException("audioRoot");
			if (heartbeat == null) throw new ArgumentNullException("heartbeat");
			if (metaKeyValues == null) throw new ArgumentNullException("metaKeyValues");
			if (buildPreferences == null) throw new ArgumentNullException("buildPreferences");

			this.audioRoot = audioRoot;
			this.audioConfiguration = audioConfiguration;
			this.heartbeat = heartbeat;
			this.metaKeyValues = metaKeyValues;
			this.buildPreferences = buildPreferences;
		}

		public void Initialize(Action<RequestStatus> done)
		{
			for (var i = 0; i < AudioSourcePoolCount; i++)
			{
				Pool(Create());
			}

			heartbeat.Update += OnUpdate;

			var audioLevelsInitializedVersionLast = metaKeyValues.Get(KeyDefines.Preferences.AudioLevelsInitializedVersion);

			if (audioLevelsInitializedVersionLast == 0)
			{
				// Never been initialized
				metaKeyValues.Set(KeyDefines.Preferences.UserAudioLevel, 1f);
				metaKeyValues.Set(KeyDefines.Preferences.AmbientAudioLevel, 1f);
				metaKeyValues.Set(KeyDefines.Preferences.EffectsAudioLevel, 1f);
				metaKeyValues.Set(KeyDefines.Preferences.MusicAudioLevel, 1f);
			}

			metaKeyValues.Set(KeyDefines.Preferences.AudioLevelsInitializedVersion, buildPreferences.Info.Version);

			UpdateLevels();

			// TODO: Have gamemodes control this somehow...
			Play(
				audioConfiguration.Music.First(),
				Groups.Music,
				done: OnMusicDone
			);

			done(RequestStatus.Success);
		}

		#region Events
		void OnUpdate(float delta)
		{
			var newActiveInstances = new List<AudioInstance>(activeInstances.Count);
			var dones = new List<Action>();

			foreach (var instance in activeInstances)
			{
				if (instance.Source.isPlaying) newActiveInstances.Add(instance);
				else
				{
					instance.State = States.Stopped;
					Pool(instance.Source);
					if (instance.Done != null) dones.Add(() => instance.Done(instance));
				}
			}

			activeInstances = newActiveInstances;

			foreach (var done in dones) done();
		}

		void OnMusicDone(IAudioInstance instance)
		{
			Debug.Log("done playing music track, starting next one");
			Play(
				audioConfiguration.Music.First(),
				Groups.Music,
				done: OnMusicDone
			);
		}
		#endregion

		AudioSource Create()
		{
			return Reset(audioRoot.AddComponent<AudioSource>());
		}

		void Pool(AudioSource audioSource)
		{
			if (audioSource.isPlaying) Debug.LogError("Trying to pool a audio source that is still playing, this should never happen");

			pool.Add(Reset(audioSource));
		}

		AudioSource UnPool()
		{
			if (pool.None()) return Create();

			var instance = pool.First();
			pool.RemoveAt(0);

			return instance;
		}

		AudioSource Reset(AudioSource audioSource)
		{
			audioSource.clip = null;
			audioSource.volume = 1f;
			audioSource.outputAudioMixerGroup = null;
			audioSource.playOnAwake = false;
			audioSource.enabled = false;
			audioSource.loop = false;
			audioSource.Stop();

			return audioSource;
		}

		public IAudioInstance Play(
			AudioClip clip,
			Groups group,
			bool looping = false,
			float volume = 1f,
			Action<IAudioInstance> done = null
		)
		{
			if (clip == null) throw new ArgumentNullException("clip");
			if (group == Groups.Unknown) throw new ArgumentOutOfRangeException("group", "Unknown is not a valid audio group");

			var instance = new AudioInstance
			{
				Id = Guid.NewGuid().ToString(),
				Clip = clip,
				Group = group,
				Looping = looping,
				State = States.Playing,
				AudioServiceInstance = this,
				Source = UnPool(),
				Done = done
			};

			instance.Source.clip = clip;
			instance.Source.volume = volume;
			instance.Source.loop = looping;
			instance.Source.enabled = true;

			var audioGroup = audioConfiguration.AudioGroups.FirstOrDefault(g => g.Group == group).Target;

			if (audioGroup == null) throw new Exception("Cannot find audio group of type " + group);

			instance.Source.outputAudioMixerGroup = audioGroup;

			instance.Source.Play();

			activeInstances.Add(instance);

			return instance;
		}

		public void UpdateLevels(
			float user = 1f,
			float ambient = 1f,
			float effects = 1f,
			float music = 1f
		)
		{
			audioConfiguration.AudioGroups.First(g => g.Group == Groups.User).Target.audioMixer.SetFloat(
				"UserVolume",
				ToDecibel(user * metaKeyValues.Get(KeyDefines.Preferences.UserAudioLevel))
			);
			audioConfiguration.AudioGroups.First(g => g.Group == Groups.Ambient).Target.audioMixer.SetFloat(
				"AmbientVolume",
				ToDecibel(ambient * metaKeyValues.Get(KeyDefines.Preferences.AmbientAudioLevel))
			);
			audioConfiguration.AudioGroups.First(g => g.Group == Groups.Effects).Target.audioMixer.SetFloat(
				"EffectsVolume",
				ToDecibel(effects * metaKeyValues.Get(KeyDefines.Preferences.EffectsAudioLevel))
			);
			audioConfiguration.AudioGroups.First(g => g.Group == Groups.Music).Target.audioMixer.SetFloat(
				"MusicVolume",
				ToDecibel(music * metaKeyValues.Get(KeyDefines.Preferences.MusicAudioLevel))
			);

		}

		public void SetSnapshot(string snapshot, float transitionTime = 1f)
		{
			var instance = audioConfiguration.MasterMixer.FindSnapshot(snapshot);
			if (instance == null) Debug.LogError("No snapshot found named " + snapshot);
			else instance.TransitionTo(transitionTime);
		}

		#region Utility
		float ToDecibel(float normalized)
		{
			if (Mathf.Approximately(normalized, 0f)) return -144.0f;
			return 20.0f * Mathf.Log10(normalized);
		}
		#endregion
	}
}