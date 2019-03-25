using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using YASKAWA;

namespace RobotServer
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] result = new byte[1024];
            int myProt = 8765;   //端口 

            IPAddress ip = IPAddress.Parse("127.0.0.1");
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(ip, myProt));  //绑定IP地址：端口 
            serverSocket.Listen(1);                         //设定最多1个排队连接请求 
            serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            Console.WriteLine("启动监听{0}成功", serverSocket.LocalEndPoint.ToString());
            //通过Clientsoket发送数据 
            Socket myClientSocket = serverSocket.Accept();

            while (true)
            {
                try
                {
                    //通过clientSocket接收数据 
                    int receiveNumber = myClientSocket.Receive(result);
                    string cmd_str = Encoding.UTF8.GetString(result, 0, receiveNumber);
                    Console.WriteLine("接收客户端{0}消息{1}", myClientSocket.RemoteEndPoint.ToString(), cmd_str);

                    string[] cmd_parts = cmd_str.Split();
                    RobotControlCommand cmd = (RobotControlCommand)Enum.Parse(typeof(RobotControlCommand), cmd_parts[0]);
                    bool res_b = false; short res_s = 0;
                    string result_str = "";
                    switch (cmd)
                    {
                        case RobotControlCommand.Configure_WriteAddrOffset:
                            MotoComHandler.SetConfigure_WriteAddrOffset(Int32.Parse(cmd_parts[1]));
                            result_str = "True"; break;
                        case RobotControlCommand.Configure_ReadAddrOffset:
                            MotoComHandler.SetConfigure_ReadAddrOffset(Int32.Parse(cmd_parts[1]));
                            result_str = "True"; break;
                        case RobotControlCommand.ESSaveFile:
                            res_b = MotoComHandler.GetMotoCom_ESSaveFile(cmd_parts[1], cmd_parts[2], cmd_parts[3]);
                            result_str = res_b.ToString(); break;
                        case RobotControlCommand.ESLoadFile:
                            res_b = MotoComHandler.SetMotoCom_ESLoadFile(cmd_parts[1], cmd_parts[2]);
                            result_str = res_b.ToString(); break;
                        case RobotControlCommand.ESDeleteJob:
                            res_b = MotoComHandler.SetMotoCom_ESDeleteJob(cmd_parts[1], cmd_parts[2]);
                            result_str = res_b.ToString(); break;
                        case RobotControlCommand.ESServo:
                            res_b = MotoComHandler.SetMotoCom_ESServo(Int32.Parse(cmd_parts[1]), cmd_parts[2]);
                            result_str = res_b.ToString(); break;
                        case RobotControlCommand.ESSelectJob:
                            res_b = MotoComHandler.SetMotoCom_ESSelectJob(cmd_parts[1], ipaddress: cmd_parts[4]);
                            result_str = res_b.ToString(); break;
                        case RobotControlCommand.ESStartJob:
                            res_b = MotoComHandler.SetMotoCom_ESStartJob(cmd_parts[1]);
                            result_str = res_b.ToString(); break;
                        case RobotControlCommand.ESCancel:
                            res_b = MotoComHandler.SetMotoCom_ESCancel();
                            result_str = res_b.ToString(); break;
                        case RobotControlCommand.ESHold:
                            res_b = MotoComHandler.SetMotoCom_ESHold(Int32.Parse(cmd_parts[1]), cmd_parts[2]);
                            result_str = res_b.ToString(); break;
                        case RobotControlCommand.ESReset:
                            res_b = MotoComHandler.SetMotoCom_ESReset();
                            result_str = res_b.ToString(); break;
                        case RobotControlCommand.ESGetVarDataMI:
                            res_s = MotoComHandler.GetMotoCom_ESGetVarDataMI(Int32.Parse(cmd_parts[1]), cmd_parts[2]);
                            result_str = res_s.ToString(); break;
                        case RobotControlCommand.ESSetVarDataMI:
                            res_b = MotoComHandler.SetMotoCom_ESSetVarDataMI(Int32.Parse(cmd_parts[1]), short.Parse(cmd_parts[2]), cmd_parts[3]);
                            result_str = res_b.ToString(); break;
                        case RobotControlCommand.ESSetVarDataMI_Multi:
                            List<short> valArray = new List<short>();
                            string[] vals_str = cmd_parts[2].Split('_');
                            foreach (string val in vals_str) valArray.Add(short.Parse(val));
                            res_b = MotoComHandler.SetMotoCom_ESSetVarDataMI_Multi(Int32.Parse(cmd_parts[1]), valArray);
                            result_str = res_b.ToString(); break;
                        default: break;
                    }

                    myClientSocket.Send(Encoding.UTF8.GetBytes(result_str));
                    Console.WriteLine("send to client: " + result_str);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    myClientSocket.Shutdown(SocketShutdown.Both);
                    myClientSocket.Close();
                    break;
                }
            }
        }
    }
}
