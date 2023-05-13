using System;
using System.Collections.Generic;
using System.Linq;
using Core.Utility;
using UnityEngine;

namespace Game.Asset
{
	/// <summary>
	/// 추후에 폴더 경로를 옮길지도 모르나 현재는 ScriptableObject는 에셋이기 때문에
	/// 현재 폴더(Game/Asset)에 같이 넣어 둔다. 
	/// </summary>
	[CreateAssetMenu(menuName = "RAMG Project/Asset/Setting/SystemOrder", fileName = "SystemOrderSettingData")]
	public class SystemOrderSettingData : ScriptableObject
	{
		/// <summary>
		/// 내부에는 Type의 FullName이 들어가있다.
		/// </summary>
		[SerializeField, HideInInspector]
		private string[] systemOrders = Array.Empty<string>();

		public List<Type> SystemOrders
		{
			get
			{
				var result = new List<Type>();
				var types = TypeUtility.GetTypesByFullNames(systemOrders).ToList();

				foreach (var typeFullName in systemOrders)
				{
					var index = types.FindIndex(x => x.FullName == typeFullName);

					if (index >= 0)
					{
						result.Add(types[index]);
						types.RemoveAt(index);
					}
					else
					{
						Debug.LogError($"SystemOrderSettingData Error. {typeFullName} is Missing Type");
					}
				}

				return result;
			}
		}
	}
}
