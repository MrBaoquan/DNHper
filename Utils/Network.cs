using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DNHper
{
    public class Network
    {
        public static string GetMainIPAddress()
        {
            var interfaces = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up)
                .OrderByDescending(nic => GetInterfacePriority(nic.NetworkInterfaceType));

            foreach (var nic in interfaces)
            {
                var ipAddress = nic.GetIPProperties()
                    .UnicastAddresses.FirstOrDefault(
                        ip => ip.Address.AddressFamily == AddressFamily.InterNetwork
                    )
                    ?.Address;

                if (ipAddress != null)
                {
                    return ipAddress.ToString();
                }
            }

            return string.Empty;
        }

        private static int GetInterfacePriority(NetworkInterfaceType type)
        {
            switch (type)
            {
                case NetworkInterfaceType.Ethernet:
                    return 3;
                case NetworkInterfaceType.Wireless80211:
                    return 2;
                default:
                    return 1;
            }
        }
    }
}
