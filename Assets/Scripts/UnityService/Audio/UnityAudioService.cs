using System;
using System.Collections.Generic;
using BlitzEcs;
using Core.Unity;
using Service.Audio;
using UnityEngine;

namespace UnityService.Audio
{
	[Serializable]
	struct AudioClipInfo
	{
		public string name;

		public AudioClip clip;
	}
	
	[UnityService(typeof(IAudioService))]
	public class UnityAudioService : CustomBehaviour, IAudioService
	{
		[SerializeField]
		private AudioListener audioListener;

		[SerializeField]
		private AudioClipInfo[] clipInfos;

		private readonly Dictionary<string, AudioClip> _clipMap = new Dictionary<string, AudioClip>();

		public void Init(World world)
		{
			_clipMap.Clear();

			foreach (var clipInfo in clipInfos)
			{
				_clipMap.Add(clipInfo.name, clipInfo.clip);
			}
		}

		public void TestPlayOneShotSFX(string sfxName, Vector3 pos)
		{
			if (_clipMap.TryGetValue(sfxName, out var clip))
			{
				AudioSource.PlayClipAtPoint(clip, pos);
			}
		}
	}
}
