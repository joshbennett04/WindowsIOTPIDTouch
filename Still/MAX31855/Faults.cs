namespace Still.MAX31855
{
    public enum Faults
    {
        /// <summary>
        ///     No faults detected.
        /// </summary>
        Ok = 0,

        /// <summary>
        ///     Thermocouple is short-circuited to VCC.
        /// </summary>
        ShortToVcc = 1,

        /// <summary>
        ///     Thermocouple is short-circuited to GND..
        /// </summary>
        ShortToGnd = 2,

        /// <summary>
        ///     Thermocouple is open (no connections).
        /// </summary>
        OpenCircuit = 4,

        /// <summary>
        ///     Problem with thermocouple.
        /// </summary>
        GeneralFault = 5,

        //OutOfRange = 6
    }
}
