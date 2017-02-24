using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace MayatoniaDisplay
{
    class Common
    {

        /// <summary>
        /// Test connection between this application and another host
        /// </summary>
        /// <param name="Host">The fully qualifying hostname or IP address</param>
        /// <param name="Port">The Port, defaults to 80</param>
        /// <param name="Timeout">The connection Timeout Value, default sto 15 seconds</param>
        /// <returns></returns>
        public static bool TestTCPConnection(String Host, int Port, int Timeout)
        {
            bool RC = false;
            if (Timeout == 0)
                Timeout = 5000;// 15000;

            if (Port == 0)
                Port = 80;

            try
            {
                using (TcpClient client = new TcpClient())
                {
                    client.SendTimeout = Timeout;
                    client.SendTimeout = Timeout;

                    client.Connect(Host, Port);
                    RC = client.Connected;
                 
                }
            }
            catch(Exception ex)
            {
                ex.ToString();
            }

            return RC;
        }

        /// <summary>
        /// Test Local Server
        /// </summary>
        /// <returns></returns>
        public static bool TestLocalConnection()
        {
            return TestTCPConnection("sju-vm-www1", 0, 0);
        }

        public static bool TestRegionInternalConnection()
        {
            return TestTCPConnection("wwwdev.srh.noaa.gov", 0, 0);
        }

        public static bool TestRegionConnection()
        {
            return TestTCPConnection("www.srh.noaa.gov", 0, 0);
        }

        public static bool TestNationalConnection()
        {
            return TestTCPConnection("www.noaa.gov", 0, 0);
        }

        public static bool TestInternetConnection()
        {
            return TestTCPConnection("www.google.com", 0, 0);

        }



    }


}
