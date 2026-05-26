namespace AnyRPG {
    public interface ICapabilityProvider {

        /// <summary>
        /// common capabilities that apply to all characters of this capabilty provider type
        /// </summary>
        //CapabilityProps Capabilities { get; }

            //string DisplayName { get; }

        /// <summary>
        /// common capabilities, plus capabilites that only apply based on character configuration
        /// </summary>
        /// <param name="capabilityConsumer"></param>
        /// <returns></returns>
        CapabilityProps GetFilteredCapabilities(ICapabilityConsumer capabilityConsumer, bool returnAll = true);

    }

}