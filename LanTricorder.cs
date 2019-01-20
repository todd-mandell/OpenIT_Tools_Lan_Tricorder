using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;  

namespace OpenITTools_LanTricorder
{
    class Program
    {
        static void Main(string[] args)
        {  
            Console.WriteLine("Enter loop wait time in seconds--");
            int timeoutPeriodS = Convert.ToInt32(Console.ReadLine());
            int timeoutPeriodMS = timeoutPeriodS * 1000;
            int timeoutPeriodMIN = (timeoutPeriodS / 60);
            Console.WriteLine("Wait time is set to " + timeoutPeriodS + " seconds or " + timeoutPeriodMIN + " minutes");

            loopit:
            bool pingable = false;
            Ping pinger = null;
            string FileName = "c:\\lantricorder-iplist.txt";

            //ip list enumeration
            int TotalLines = File.ReadAllLines(FileName).Count();

            for (int i = 0; i < TotalLines; i++)
            {
              string textRedd = File.ReadLines(FileName).Skip(i).Take(1).First();    
                 try
                {
                    pingable = false;
                    pinger = null;
                    pinger = new Ping();
                    PingReply reply = pinger.Send(textRedd);
                    pingable = reply.Status == IPStatus.Success;                
                    Console.WriteLine(i + " - " + textRedd + " " + pingable);

                    if (pingable == false)
                    {
                        Console.WriteLine(i + " - FAILURE IP " + textRedd);
                        try
                        {
                            //send the event data over udp to pre-specified message receiver at ipadd.parse address and port below
                            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,ProtocolType.Udp);
                            IPAddress serverAddr = IPAddress.Parse("192.168.50.205");
                            IPEndPoint endPoint = new IPEndPoint(serverAddr, 162);
                            string messedge = "PING FAILURE - SEQUENCE " + i + " - FAILURE IP " + textRedd;
                            byte[] send_buffer = Encoding.ASCII.GetBytes(messedge);
                            sock.SendTo(send_buffer, endPoint);
                            sock.Close();
                        }
                        catch{}
                    }
                }
                catch (PingException)
                {
                    Console.WriteLine(i + " - ERROR " + textRedd);
                    
                    try
                    {
                        //send the event data over udp to pre-specified message receiver at ipadd.parse address and port below
                        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,ProtocolType.Udp);
                        IPAddress serverAddr = IPAddress.Parse("192.168.50.205");
                        IPEndPoint endPoint = new IPEndPoint(serverAddr, 162);
                        string messedge = "PING ERROR - SEQUENCE " + i + " - FAILURE IP " + textRedd;
                        byte[] send_buffer = Encoding.ASCII.GetBytes(messedge);
                        sock.SendTo(send_buffer, endPoint);
                        sock.Close();
                    }
                    catch{}
                }
                finally
                {
                    if (pinger != null)
                    {
                        pinger.Dispose();
                    }
                }
             }
            System.Threading.Thread.Sleep(timeoutPeriodMS);
            goto loopit;
        }
    }
}
