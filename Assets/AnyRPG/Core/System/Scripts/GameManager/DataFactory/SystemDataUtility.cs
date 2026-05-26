namespace AnyRPG {

    public static class SystemDataUtility {

        /// <summary>
        /// remove spaces and single quotes from the string
        /// </summary>
        /// <param name="oldString"></param>
        /// <returns></returns>
        public static string PrepareStringForMatch(string oldString) {
            return oldString.ToLower().Replace(" ", string.Empty).Replace("'", string.Empty);
        }

        /// <summary>
        /// check if resourceMatchName fully or partially matches resourceName
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="resourceMatchName"></param>
        /// <param name="partialMatch"></param>
        /// <returns></returns>
        public static bool MatchResource(string resourceName, string resourceMatchName, bool partialMatch = false) {
            if (resourceName != null && resourceMatchName != null) {
                if (partialMatch == true) {
                    if (PrepareStringForMatch(resourceName).Contains(PrepareStringForMatch(resourceMatchName))) {
                        return true;
                    }
                } else {
                    if (PrepareStringForMatch(resourceName) == PrepareStringForMatch(resourceMatchName)) {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// return true if resourceName is null or empty string
        /// </summary>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        public static bool RequestIsEmpty(string resourceName) {
            if (resourceName == null || resourceName == string.Empty) {
                //Debug.Log("SystemDataFactory.RequestIsEmpty(" + resourceName + "): EMPTY RESOURCE REQUESTED.  FIX THIS! DO NOT COMMENT THIS LINE");
                return true;
            }
            return false;
        }

    }

}