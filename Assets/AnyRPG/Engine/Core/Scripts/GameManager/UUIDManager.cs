using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class UUIDManager {
        static Dictionary<IUUID, string> m_ObjToUUID = new Dictionary<IUUID, string>();
        static Dictionary<string, IUUID> m_UUIDtoObj = new Dictionary<string, IUUID>();

        public static void RegisterUUID(IUUID aID) {
            string UID;
            if (m_ObjToUUID.TryGetValue(aID, out UID)) {
                //Debug.Log("found object instance, update ID from: " + aID.ID + "; to: " + UID);
                aID.ID = UID;
                aID.IDBackup = aID.ID;
                if (!m_UUIDtoObj.ContainsKey(UID)) {
                    //Debug.Log("found object instance, reverse lookup did not contain " + UID);
                    m_UUIDtoObj.Add(UID, aID);
                }
                return;
            }

            if (string.IsNullOrEmpty(aID.ID)) {
                //Debug.Log("No ID yet, generate a new one.");
                aID.ID = System.Guid.NewGuid().ToString();
                aID.IDBackup = aID.ID;
                m_UUIDtoObj.Add(aID.ID, aID);
                m_ObjToUUID.Add(aID, aID.ID);
                return;
            }

            IUUID tmp;
            if (!m_UUIDtoObj.TryGetValue(aID.ID, out tmp)) {
                m_UUIDtoObj.Add(aID.ID, aID);
                m_ObjToUUID.Add(aID, aID.ID);
                //Debug.Log("ID not known to the DB, so just register it: UUID: " + aID.ID);
                return;
            }
            if (aID.ForceUpdateUUID) {
                if (m_UUIDtoObj.ContainsKey(aID.ID)) {
                    m_UUIDtoObj[aID.ID] = aID;
                } else {
                    m_UUIDtoObj.Add(aID.ID, aID);
                }
                if (m_ObjToUUID.ContainsKey(aID)) {
                    m_ObjToUUID[aID] = aID.ID;
                } else {
                    m_ObjToUUID.Add(aID, aID.ID);

                }
                //Debug.Log("Force update enabled register UUID: " + aID.ID);
                return;
            }

            if (tmp == aID) {
                //Debug.Log("DB inconsistency aid: " + aID.ID + "; tmp: " + tmp.ID);
                m_ObjToUUID.Add(aID, aID.ID);
                return;
            }
            if (tmp == null) {
                //Debug.Log("object in DB got destroyed, replace with new : " + aID.ID);
                m_UUIDtoObj[aID.ID] = aID;
                m_ObjToUUID.Add(aID, aID.ID);
                return;
            }

            // duplicates should never be ignored at edit time, only run time
            if (aID.IgnoreDuplicateUUID && Application.isPlaying) {
                return;
            }
            //Debug.Log("we got a duplicate, generate new ID from: " + aID.ID);
            aID.ID = System.Guid.NewGuid().ToString();
            aID.IDBackup = aID.ID;
            m_UUIDtoObj.Add(aID.ID, aID);
            m_ObjToUUID.Add(aID, aID.ID);
            //Debug.Log("we got a duplicate, generated new ID: " + aID.ID);
        }
        public static void UnregisterUUID(IUUID aID) {
            m_UUIDtoObj.Remove(aID.ID);
            m_ObjToUUID.Remove(aID);
        }

       
    }

}