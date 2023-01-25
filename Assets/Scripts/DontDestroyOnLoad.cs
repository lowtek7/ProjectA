using UnityEngine;

/// <summary>
/// 씬이 변경되어도 삭제되지 않는 오브젝트를 지정하기 위한 컴포넌트
/// </summary>
public class DontDestroyOnLoad : MonoBehaviour
{
	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}
}
