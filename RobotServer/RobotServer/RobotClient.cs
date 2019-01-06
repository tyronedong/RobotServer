using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;

namespace RobotServer
{
    public enum RobotControlCommand     // enum 类型定义在 class 外面 
    {
        Configure_WriteAddrOffset,
        Configure_ReadAddrOffset,
        ESSaveFile,
        ESLoadFile,
        ESDeleteJob,
        ESServo,
        ESSelectJob,
        ESStartJob,
        ESHold,
        ESReset,
        ESGetVarDataMI,
        ESSetVarDataMI,
        ESSetVarDataMI_Multi,
    }

    public class RobotClient
    {
        public static bool IsRobotConnected = false;
        public static bool IsForceKill = false;

        public const string bool_true_str = "True";
        public const string bool_false_str = "False";
        public const short ERROR = 0;

        //创建 1个客户端套接字
        private static Socket client = null;

        public RobotClient()
        {

        }

        #region socket相关函数区
        private static bool Reconnect()
        {
            return StartServer() && Connect();
        }

        private static bool StartServer()
        {
            try
            {
                #region 检查是否有历史进程，若有则kill
                Process[] alsons = Process.GetProcessesByName("RobotServer");
                if (!IsForceKill && alsons.Length == 1)   //已有一个进程则返回
                    return true;
                //if (phos.Length > 1)    //有多个进程则kill
                //{
                foreach (var alson in alsons)
                    alson.Kill();
                //}
                #endregion

                #region 开启新进程
                ProcessStartInfo startInfo = new ProcessStartInfo("RobotServer.exe");
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = false;
                //startInfo.RedirectStandardOutput = true;

                Process p = Process.Start(startInfo);
                //p.OutputDataReceived += (s, _e) => sb.AppendLine(_e.Data);
                //p.BeginOutputReadLine();
                #endregion

                return true;
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());

                return false;
            }
        }

        private static bool Connect()
        {
            if (client != null && client.Connected)
                return true;

            //定义一个套接字监听  
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //socketClient.ReceiveTimeout = 5000;   //Receive用来阻塞程序，不需要设置timeout
            client.SendTimeout = 5000;        //Send正常是秒传，需要设置timeout

            //定义IP地址  
            IPAddress address = IPAddress.Parse("127.0.0.1");

            //将获取的IP地址和端口号绑定在网络节点上  
            IPEndPoint point = new IPEndPoint(address, 8765);

            try
            {
                //客户端套接字连接到网络节点上，用的是Connect  
                client.Connect(point);
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                return false;
            }

            return true;
        }

        private static int Send(string sendMsg)
        {
            try
            {
                IsForceKill = false;

                //将输入的内容字符串转换为机器可以识别的字节数组     
                byte[] arrClientSendMsg = Encoding.UTF8.GetBytes(sendMsg);
                //调用客户端套接字发送字节数组     
                return client.Send(arrClientSendMsg);
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                IsForceKill = true;
                return -1;
            }
        }

        private static string Recv()
        {
            try
            {
                IsForceKill = false;

                //定义一个1K的内存缓冲区，用于临时性存储接收到的消息  
                byte[] arrRecvmsg = new byte[1024];

                //将客户端套接字接收到的数据存入内存缓冲区，并获取长度  
                int length = client.Receive(arrRecvmsg);

                //将套接字获取到的字符数组转换为人可以看懂的字符串  
                string strRevMsg = Encoding.UTF8.GetString(arrRecvmsg, 0, length);

                return strRevMsg;
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                IsForceKill = true;
                return null;
            }
        }

        private static void Despose()
        {
            client.Disconnect(false);
            client.Dispose();

            Process[] ps = Process.GetProcesses();
            foreach (Process pp in ps)
            {
                try
                {
                    if (pp.ProcessName == "RobotServer.exe")
                        pp.Kill();
                }
                catch (Exception e)
                {
                    ;
                }
            }
        }
        #endregion

        /// <summary>
        /// 把机器人上指定的文件保存到本地指定的地址
        /// </summary>
        /// <param name="sSavePath">指定保存到本地的路径</param>
        /// <param name="sFileName">指定机器人上的文件名</param>
        /// <returns></returns>
        public static bool GetMotoCom_ESSaveFile(string sSavePath, string sFileName)
        {
            string cmd = string.Format("{0} {1} {2}", 
                RobotControlCommand.ESSaveFile.ToString(), 
                sSavePath, 
                sFileName);

            if (Send(cmd) < 0)
            {
                IsRobotConnected = false;
                return false;
            }

            string res = Recv();
            if (res == null || res == bool_false_str)
            {
                IsRobotConnected = false;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 把本地保存的文件load到机器人上
        /// </summary>
        /// <param name="sLoadPath"></param>
        /// <returns></returns>
        public static bool SetMotoCom_ESLoadFile(string sLoadPath)
        {
            string cmd = string.Format("{0} {1}",
               RobotControlCommand.ESLoadFile.ToString(),
               sLoadPath);

            if (Send(cmd) < 0)
            {
                IsRobotConnected = false;
                return false;
            }

            string res = Recv();
            if (res == null || res == bool_false_str)
            {
                IsRobotConnected = false;
                return false;
            }

            return true;
        }

        /// <summary>
        /// ESDeleteJob
        /// </summary>
        /// <returns></returns>
        public static bool SetMotoCom_ESDeleteJob(string jobName)
        {
            string cmd = string.Format("{0} {1}",
             RobotControlCommand.ESDeleteJob.ToString(),
             jobName);

            if (Send(cmd) < 0)
            {
                IsRobotConnected = false;
                return false;
            }

            string res = Recv();
            if (res == null || res == bool_false_str)
            {
                IsRobotConnected = false;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="onOff">1代表on,2代表off</param>
        /// <returns></returns>
        public static bool SetMotoCom_ESServo(int onOff)
        {
            string cmd = string.Format("{0} {1}",
             RobotControlCommand.ESServo.ToString(),
             onOff);

            if (Send(cmd) < 0)
            {
                IsRobotConnected = false;
                return false;
            }

            string res = Recv();
            if (res == null || res == bool_false_str)
            {
                IsRobotConnected = false;
                return false;
            }

            return true;
        }

        public static bool SetMotoCom_ESSelectJob(string jobNameStr, int jobType = 1, int lineNo = 1)
        {
            string cmd = string.Format("{0} {1} {2}",
             RobotControlCommand.ESSelectJob.ToString(),
             jobType,
             lineNo);

            if (Send(cmd) < 0)
            {
                IsRobotConnected = false;
                return false;
            }

            string res = Recv();
            if (res == null || res == bool_false_str)
            {
                IsRobotConnected = false;
                return false;
            }

            return true;
        }

        public static bool SetMotoCom_ESStartJob()
        {
            string cmd = string.Format("{0}",
             RobotControlCommand.ESStartJob.ToString());

            if (Send(cmd) < 0)
            {
                IsRobotConnected = false;
                return false;
            }

            string res = Recv();
            if (res == null || res == bool_false_str)
            {
                IsRobotConnected = false;
                return false;
            }

            return true;
        }

        public static bool SetMotoCom_ESHold(int onOff)
        {
            string cmd = string.Format("{0} {1}",
             RobotControlCommand.ESHold.ToString(),
             onOff);

            if (Send(cmd) < 0)
            {
                IsRobotConnected = false;
                return false;
            }

            string res = Recv();
            if (res == null || res == bool_false_str)
            {
                IsRobotConnected = false;
                return false;
            }

            return true;
        }

        public static bool SetMotoCom_ESReset()
        {
            string cmd = string.Format("{0}",
              RobotControlCommand.ESReset.ToString());

            if (Send(cmd) < 0)
            {
                IsRobotConnected = false;
                return false;
            }

            string res = Recv();
            if (res == null || res == bool_false_str)
            {
                IsRobotConnected = false;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 获取机器人的一个I型数值
        /// </summary>
        public static short GetMotoCom_ESGetVarDataMI(int varNo)
        {
            string cmd = string.Format("{0} {1}",
             RobotControlCommand.ESGetVarDataMI.ToString(),
             varNo);

            if (Send(cmd) < 0)
            {
                IsRobotConnected = false;
                return ERROR;
            }

            string res = Recv();
            if (res == null || res == bool_false_str)
            {
                IsRobotConnected = false;
                return ERROR;
            }

            return short.Parse(res);
        }

        /// <summary>
        /// 设置机器人的一个I型数值
        /// </summary>
        public static bool SetMotoCom_ESSetVarDataMI(int varNo, short val)
        {
            string cmd = string.Format("{0} {1} {2}",
            RobotControlCommand.ESSetVarDataMI.ToString(),
            varNo,
            val);

            if (Send(cmd) < 0)
            {
                IsRobotConnected = false;
                return false;
            }

            string res = Recv();
            if (res == null || res == bool_false_str)
            {
                IsRobotConnected = false;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 设置机器人的多个I型数值
        /// </summary>
        public static bool SetMotoCom_ESSetVarDataMI_Multi(int varNo, List<short> val)
        {
            string cmd = string.Format("{0} {1} {2}",
            RobotControlCommand.ESSetVarDataMI_Multi.ToString(),
            varNo,
            string.Join("_", val));

            if (Send(cmd) < 0)
            {
                IsRobotConnected = false;
                return false;
            }

            string res = Recv();
            if (res == null || res == bool_false_str)
            {
                IsRobotConnected = false;
                return false;
            }

            return true;
        }
    }
}
