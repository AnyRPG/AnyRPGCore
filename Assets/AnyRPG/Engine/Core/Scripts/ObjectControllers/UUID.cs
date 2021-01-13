
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class UUID : MonoBehaviour, ISerializationCallbackReceiver, IUUID {

        [SerializeField]
        private string m_UUID = null;

        [Tooltip("If true, this object will overwrite any references to any other objects with the same UUID in the UUID manager.  This option should be used for non static (spawned) objects")]
        [SerializeField]
        private bool forceUpdateUUID = false;

        // since this is attached to a gameObject, it should always be false
        // we always want new instances of these objects to get unique UUIDs
        private bool ignoreDuplicateUUID = false;

        private string m_IDBackup = null;

        public string ID { get => m_UUID; set => m_UUID = value; }
        public string IDBackup { get => m_IDBackup; set => m_IDBackup = value; }
        public bool ForceUpdateUUID { get => forceUpdateUUID; set => forceUpdateUUID = value; }
        public bool IgnoreDuplicateUUID { get => ignoreDuplicateUUID; set => ignoreDuplicateUUID = value; }

        public void OnAfterDeserialize() {
            if (m_UUID == null || m_UUID != m_IDBackup) {
                UUIDManager.RegisterUUID(this);
            }
        }
        public void OnBeforeSerialize() {
            if (m_UUID == null || m_UUID != m_IDBackup) {
                UUIDManager.RegisterUUID(this);
            }
        }
        void OnDestroy() {
            UUIDManager.UnregisterUUID(this);
            m_UUID = null;
        }
    }

}