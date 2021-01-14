using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public interface IUUID {

        string ID { get; set; }
        string IDBackup { get; set; }
        bool IgnoreDuplicateUUID { get; set; }
        bool ForceUpdateUUID { get; }
    }

}