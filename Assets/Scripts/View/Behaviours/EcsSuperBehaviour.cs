using BlitzEcs;
using UnityEngine;

namespace View.Behaviours
{
	/// <summary>
	/// 엔티티와 연결 하는 모노 비헤이비어들의 슈퍼 클래스
	/// </summary>
	public abstract class EcsSuperBehaviour : MonoBehaviour
	{
		private Entity selfEntity;
		
		protected Entity SelfEntity
		{
			get => selfEntity;
			set => selfEntity = value;
		}

		public virtual void SetupEntity(Entity entity)
		{
			selfEntity = entity;
		}
	}

	public interface IComponentBinder<T> where T : struct
	{
		
	}
}
