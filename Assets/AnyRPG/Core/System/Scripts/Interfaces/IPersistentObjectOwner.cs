using UnityEngine;

namespace AnyRPG {

    /// <summary>
    /// provide access to a UUID component
    /// </summary>
    public interface IPersistentObjectOwner {

        IUUID UUID { get; }
        Transform transform { get; }
        GameObject gameObject { get; }
        PersistentObjectComponent PersistentObjectComponent { get; }

        void PopulatePersistentObjectSaveData(PersistentObjectSaveData persistentObjectSaveData);
    }

}