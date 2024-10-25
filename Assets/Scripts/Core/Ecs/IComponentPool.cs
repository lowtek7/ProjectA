namespace BlitzEcs {
    public interface IComponentPool {
	    System.Type ComponentType { get; }
        int Count { get; }
        int PoolId { get; }

        int HighestEntityId { get; }

        bool Contains(int entityId);
        void Add(int entityId);
        void Add(int entityId, object component);
        object GetWithBoxing(int entityId);
        void Remove(int entityId);
        void ExecuteBufferedRemoves();

        // Dense array
        int[] RawEntityIds { get; }
        // Sparse array
        int[] RawEntityIdsToComponentIdx { get; }
        PoolEntityIdEnumerator EntityIds { get; }
    }
}
