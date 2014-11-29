namespace MMDataStructures {
    /// <summary>
    /// 
    /// </summary>
    public enum PersistenceMode {
        /// <summary>
        /// Store file on disk
        /// </summary>
        Persist = 0,
        /// <summary>
        /// Store file on disk while process is running
        /// </summary>
        TemporaryPersist = 1,
        /// <summary>
        /// In-memory file for IPC
        /// </summary>
        Ephemeral = 2
    }
}