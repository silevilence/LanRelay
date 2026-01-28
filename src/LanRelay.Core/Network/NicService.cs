using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace LanRelay.Core.Network;

/// <summary>
/// Service for enumerating and managing Network Interface Cards (NICs).
/// </summary>
public class NicService
{
    /// <summary>
    /// Gets all active NICs with valid IPv4 addresses.
    /// Excludes loopback and disabled interfaces.
    /// </summary>
    /// <returns>A list of NicInfo for each active NIC.</returns>
    public IReadOnlyList<NicInfo> GetActiveNicInfos()
    {
        var result = new List<NicInfo>();

        var interfaces = NetworkInterface.GetAllNetworkInterfaces();

        foreach (var nic in interfaces)
        {
            // Skip non-operational interfaces
            if (nic.OperationalStatus != OperationalStatus.Up)
            {
                continue;
            }

            // Skip loopback interfaces
            if (nic.NetworkInterfaceType == NetworkInterfaceType.Loopback)
            {
                continue;
            }

            var ipProperties = nic.GetIPProperties();

            foreach (var unicast in ipProperties.UnicastAddresses)
            {
                // Only process IPv4 addresses
                if (unicast.Address.AddressFamily != AddressFamily.InterNetwork)
                {
                    continue;
                }

                // Skip loopback addresses
                if (IPAddress.IsLoopback(unicast.Address))
                {
                    continue;
                }

                // Skip link-local addresses (169.254.x.x)
                var addressBytes = unicast.Address.GetAddressBytes();
                if (addressBytes[0] == 169 && addressBytes[1] == 254)
                {
                    continue;
                }

                var subnetMask = unicast.IPv4Mask;

                result.Add(new NicInfo
                {
                    Name = nic.Name,
                    IPAddress = unicast.Address,
                    SubnetMask = subnetMask
                });
            }
        }

        return result;
    }
}
