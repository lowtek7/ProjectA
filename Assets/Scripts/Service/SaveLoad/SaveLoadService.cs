using System.IO;
using System.Text;
using Core.Virtual;
using UnityEngine;

namespace Service.SaveLoad
{
	/// <summary>
	/// 임시적인 서비스 클래스.
	/// 서비스 자체의 구조를 반영하지 않았음.
	/// 일단 사용을 위해서 먼저 만들음
	/// </summary>
	public static class SaveLoadService
	{
		public static VirtualWorld LoadWorld(string worldPath)
		{
			var utf8WithOutBom = new UTF8Encoding(false);
			var json = File.ReadAllText(worldPath, utf8WithOutBom);

			if (string.IsNullOrEmpty(json))
			{
				return new VirtualWorld();
			}

			return JsonUtility.FromJson<VirtualWorld>(json);
		}

		public static void SaveWorld(string worldPath, VirtualWorld virtualWorld)
		{
			var utf8WithoutBom = new UTF8Encoding(false);
			var json = JsonUtility.ToJson(virtualWorld, true);
			File.WriteAllText(worldPath, json, utf8WithoutBom);
		}
	}
}
