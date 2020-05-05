using System;
using System.Net;
using System.Text;
using System.Net.Sockets;

namespace LR2_CSaN
{
    class Program
    {
        const byte TYPE_ECHO_REQUEST = 8;
        const short MESSAGE_MAX_SIZE = 1024;

        static void Main(string[] args)
        {
            Socket mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
            Console.Write("tracert ");
            string name = Console.ReadLine();
            try
            {
                IPHostEntry ipHostEntry = Dns.GetHostEntry(name);
                //IP address and port number
                IPEndPoint ipEndPoint = new IPEndPoint(ipHostEntry.AddressList[0], 0);

                //To see in WireShark
                string myPacketsLabel;
                myPacketsLabel = "MY_LAB";

                ICMPPack packet = new ICMPPack(TYPE_ECHO_REQUEST, Encoding.ASCII.GetBytes(myPacketsLabel));
                Traceroute(mySocket, packet, ipEndPoint);
            }
            catch (SocketException)
            {
                Console.WriteLine("Не удается разрешить системное имя узла {0}.", name);
            }
            mySocket.Close();
        }

        static void Traceroute(Socket socket, ICMPPack packet, IPEndPoint destIPEndPoint)
        {
            int timeStart, timeEnd, responseSize, errorCount = 0;
            byte[] responseMessage;

            EndPoint endPoint = destIPEndPoint;
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 3000);

            //Send packets
            Console.WriteLine("Трассировка маршрута к {0} с максимальным числом прыжков 30:", destIPEndPoint);
            for (int i = 1; i <= 30; i++)
            {
                Console.Write("{0}\t", i);
                for (int j = 0; j < 3; j++)
                {
                    socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, i);
                    timeStart = Environment.TickCount;
                    socket.SendTo(packet.getBytes(), packet.PackSize, SocketFlags.None, destIPEndPoint);
                    try
                    {
                        responseMessage = new byte[MESSAGE_MAX_SIZE];
                        responseSize = socket.ReceiveFrom(responseMessage, ref endPoint);
                        timeEnd = Environment.TickCount;
                        ICMPPack response = new ICMPPack(responseMessage, responseSize);
                        if ((response.Type == 0) || (response.Type == 11))
                        {
                            Console.Write("{0} мс\t", timeEnd - timeStart);
                            if (j == 2)
                            {
                                Console.WriteLine("{0}", endPoint.ToString());
                            }
                        }
                        if ((response.Type == 0) && (j == 2))
                        {
                            Console.WriteLine("Трассировка завершена.");
                            return;
                        }
                        errorCount = 0;
                    }
                    catch (SocketException)
                    {
                        Console.Write("*\t");
                        errorCount++;
                        if (errorCount % 3 == 0 && errorCount != 0)
                        {
                            Console.Write("Превышен интервал ожидания для запроса.");
                        }
                        if (j == 2)
                        {
                            Console.WriteLine();
                        }
                        if (errorCount == 30)
                        {
                            Console.WriteLine("Хост недоступен");
                            return;
                        }
                    }
                }
            }
        }
    }
}
