namespace Still.MAX31855
{
    public enum ProbeState
    {
        Faulted,
        VCCShort,
        GNDShort,
        OpenCircuit,
        Disconnected,
        Connected,
        DeviceNotFound
    }
}
