using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Unity
{
	/// <summary>
	/// 컴포넌트 디자이너에게 정보를 주기위해서 상속받아야하는 인터페이스
	/// </summary>
	public interface IComponent
	{
		IComponent Clone();
	}
}
