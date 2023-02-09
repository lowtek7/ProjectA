using System;
using Core.Utility;
using UnityEngine;

namespace Game.Unit
{
	public class UnitBase : MonoBehaviour
	{
		/// <summary>
		/// 이 객체의 근원의 유니크 ID
		/// </summary>
		[SerializeField, HideInInspector]
		private string sourceGuid = string.Empty;

		public Guid SourceGuid
		{
			get
			{
				if (Guid.TryParse(sourceGuid, out var result))
				{
					return result;
				}

				return Guid.Empty;
			}
		}
	}
}
