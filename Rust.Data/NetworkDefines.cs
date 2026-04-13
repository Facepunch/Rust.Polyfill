public class NetworkDefines
{
    // TODO: Unify all of these
    public const int MinNetWriteBufferSize = 1024 * 2;
    public const int MaxNetWriteBufferSize = 1024 * 1024 * 4;
    public const int MaxNetReadPacketSize = 1024 * 1024 * 3 * 2;
    public const int MinNetReadBufferSize = 1024 * 2;
    public const int MaxNetReadBufferSize = 1024 * 1024 * 4 * 2; // Larger than MaxPacketSize for things like EAC metadata
    public const int MaxServerPacketSize = 1000 * 1000 * 5 * 2;
}
