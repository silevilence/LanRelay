using System.Net;
using System.Net.Sockets;

namespace LanRelay.Core.Network;

/// <summary>
/// Represents information about a Network Interface Card (NIC).
/// </summary>
public record NicInfo
{
    /// <summary>
    /// The name/description of the network interface.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The IPv4 address assigned to this NIC.
    /// </summary>
    public required IPAddress IPAddress { get; init; }

    /// <summary>
    /// The subnet mask for this NIC's IP address.
    /// </summary>
    public required IPAddress SubnetMask { get; init; }

    /// <summary>
    /// The calculated broadcast address for the subnet.
    /// </summary>
    public IPAddress BroadcastAddress => CalculateBroadcastAddress();

    private IPAddress CalculateBroadcastAddress()
    {
        if (IPAddress.AddressFamily != AddressFamily.InterNetwork)
        {
            throw new InvalidOperationException("Broadcast address calculation only supports IPv4.");
        }

        byte[] ipBytes = IPAddress.GetAddressBytes();
        byte[] maskBytes = SubnetMask.GetAddressBytes();
        byte[] broadcastBytes = new byte[4];

        for (int i = 0; i < 4; i++)
        {
            // Broadcast = (IP & Mask) | (~Mask)
            broadcastBytes[i] = (byte)((ipBytes[i] & maskBytes[i]) | (~maskBytes[i] & 0xFF));
        }

        return new IPAddress(broadcastBytes);
    }
}
