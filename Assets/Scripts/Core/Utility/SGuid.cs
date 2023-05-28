using System;
using UnityEngine;

namespace Core.Utility
{
	/// <summary>
	/// 시리얼라이즈 가능한 Guid 구조체
	/// </summary>
	[Serializable]
	public struct SGuid
	{
		[SerializeField]
		private byte[] guidBytes;

		private Guid guid;

		public Guid Guid
		{
			get
			{
				if (guid == Guid.Empty)
				{
					guid = new Guid(guidBytes);
				}
				
				return guid;
			}
			set
			{
				guidBytes = value.ToByteArray();
				guid = value;
			}
		}

		public SGuid(Guid guid)
		{
			this.guid = guid;
			this.guidBytes = guid.ToByteArray();
		}

		public static SGuid Empty => new SGuid(Guid.Empty);

		public override string ToString()
		{
			return Guid.ToString();
		}
	}
}
