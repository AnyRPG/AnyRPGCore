/* * * * *
 * Unity Unique ID
 * ----------------
 * 
 * This component tries to solve the problem of generating a GUID for an object
 * that persists during development and at runtime and prevents any duplicates
 * of the ID. That means if an object is copied, cloned or instantiated the new
 * object should get a new ID. This also prevents accidental changes of an assigned
 * ID through "revert to prefab" or "apply". This is done by two static dictionaries
 * which track both, the ID as well as the components. Even after an assembly reload
 * all currently loaded objects / prefabs get immediately registrated again.
 * 
 * I've done several test to check if the ID correctly persists and it seems to work
 * in all cases. Though if you found a reproducible bug feel free to file an issue
 * https://github.com/Bunny83/UUID/issues
 * 
 * The MIT License (MIT)
 * 
 * Copyright (c) 2012-2017 Markus Göbel (Bunny83)
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * 
 * * * * */
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class UUID : MonoBehaviour, ISerializationCallbackReceiver {
        static Dictionary<UUID, string> m_ObjToUUID = new Dictionary<UUID, string>();
        static Dictionary<string, UUID> m_UUIDtoObj = new Dictionary<string, UUID>();

        static void RegisterUUID(UUID aID) {
            string UID;
            if (m_ObjToUUID.TryGetValue(aID, out UID)) {
                // found object instance, update ID
                aID.m_UUID = UID;
                aID.m_IDBackup = aID.m_UUID;
                if (!m_UUIDtoObj.ContainsKey(UID))
                    m_UUIDtoObj.Add(UID, aID);
                return;
            }

            if (string.IsNullOrEmpty(aID.m_UUID)) {
                // No ID yet, generate a new one.
                aID.m_UUID = System.Guid.NewGuid().ToString();
                aID.m_IDBackup = aID.m_UUID;
                m_UUIDtoObj.Add(aID.m_UUID, aID);
                m_ObjToUUID.Add(aID, aID.m_UUID);
                return;
            }

            UUID tmp;
            if (!m_UUIDtoObj.TryGetValue(aID.m_UUID, out tmp)) {
                // ID not known to the DB, so just register it
                m_UUIDtoObj.Add(aID.m_UUID, aID);
                m_ObjToUUID.Add(aID, aID.m_UUID);
                return;
            }
            if (tmp == aID) {
                // DB inconsistency
                m_ObjToUUID.Add(aID, aID.m_UUID);
                return;
            }
            if (tmp == null) {
                // object in DB got destroyed, replace with new
                m_UUIDtoObj[aID.m_UUID] = aID;
                m_ObjToUUID.Add(aID, aID.m_UUID);
                return;
            }
            // we got a duplicate, generate new ID
            aID.m_UUID = System.Guid.NewGuid().ToString();
            aID.m_IDBackup = aID.m_UUID;
            m_UUIDtoObj.Add(aID.m_UUID, aID);
            m_ObjToUUID.Add(aID, aID.m_UUID);
        }
        static void UnregisterUUID(UUID aID) {
            m_UUIDtoObj.Remove(aID.m_UUID);
            m_ObjToUUID.Remove(aID);
        }

        [SerializeField]
        private string m_UUID = null;
        private string m_IDBackup = null;

        public string ID { get { return m_UUID; } }

        public void OnAfterDeserialize() {
            if (m_UUID == null || m_UUID != m_IDBackup)
                RegisterUUID(this);
        }
        public void OnBeforeSerialize() {
            if (m_UUID == null || m_UUID != m_IDBackup)
                RegisterUUID(this);
        }
        void OnDestroy() {
            UnregisterUUID(this);
            m_UUID = null;
        }
    }

}