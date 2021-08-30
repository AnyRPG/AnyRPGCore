using System.Collections.Generic;

namespace AnyRPG.DefineSymbolsManager {
    public class CoreDefineSymbols : DefineSymbols {
        public override List<string> GetSymbols {
            get {
                return new List<string>() { "ANYRPG_CORE" };
            }
        }
    }
}

