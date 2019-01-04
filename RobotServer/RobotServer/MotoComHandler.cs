using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MotoComES_CS;

namespace YASKAWA
{
    public class MotoComHandler
    {
        public static int _writeAddrOffset;
        public static int _readAddrOffset;

        #region 机器人操作变量区
        public static Encoding _ECode = Encoding.Default;   //编码

        public static IntPtr _Handle = new IntPtr();        //处理

        public static int res = -1; //函数执行结果

        //public static string _IPAddress = "192.168.255.1";  //配置信息：IP地址

        //public static int _ControllerType = 1;              //配置信息：控制器类型

        public static string _ErrorMessages = "";   //错误信息

        public static bool _IsMotoConnect = false;
        #endregion

        #region 错误信息字符串定义区
        public static string ESOPENERROR = "机器人通信开启失败";
        public static string ESCLOSEERROR = "机器人通信关闭失败";
        public static string ESSERVOERROR = "机器人Servo设置失败";
        public static string ESSELECTJOBERROR = "机器人Job选择失败";
        public static string ESSTARTJOBERROR = "机器人Job启动失败";
        public static string ESGETVARDATAMIERROR = "机器人I型变量获取失败";
        public static string ESSETVARDATAMIERROR = "机器人I型变量设置失败";
        public static string ESSETVARDATAMIMULTIERROR = "机器人多个I型变量设置失败";
        public static string ESGETVARDATAMDERROR = "机器人D型变量获取失败";
        public static string ESSETVARDATAMDERROR = "机器人D型变量设置失败";
        public static string ESGETPOSITIONDATAERROR = "机器人P型变量获取失败";
        public static string ESSETPOSITIONDATAERROR = "机器人P型变量设置失败";
        public static string ESCANCELERROR = "机器人取消失败";
        public static string ESHOLDERROR = "机器人HOLD失败";
        public static string ESRESETERROR = "机器人重置失败";

        public static string ESFILELISTFIRSTERROR = "机器人文件列举First失败";
        public static string ESFILELISTNEXTERROR = "机器人文件列举Next失败";
        public static string ESSAVEFILEERROR = "机器人保存文件失败";
        public static string ESLOADFILEERROR = "机器人加载文件失败";
        public static string ESDELETEJOBERROR = "机器人删除Job失败";

        public static string ESSETTIMEOUTERROR = "机器人设置超时时间失败";
        #endregion

        public static void SetConfigure_WriteAddrOffset(int val)
        {
            _writeAddrOffset = val;
        }

        public static void SetConfigure_ReadAddrOffset(int val)
        {
            _readAddrOffset = val;
        }

        /// <summary>
        /// 基础操作，不更新错误信息
        /// </summary>
        /// <returns></returns>
        public static bool SetMotoCom_ESOpen(string ipaddress = "192.168.255.1", int controllerType = 3)
        {
            MotoComHandler.res = -1;

            // IPアドレス文字列をバイト配列へ|IP地址字符串为一个字节数组
            int iByteCount = MotoComES._ECode.GetByteCount(ipaddress) + 1;
            byte[] bIPAdd = MotoComES.StringToByteArray(ipaddress, iByteCount);

            // 関数を実行|运行功能
            MotoComHandler.res = MotoComES.ESOpen(controllerType, ref bIPAdd[0], ref MotoComHandler._Handle);
            
            // 戻り値が MotoComES.OK(0):正常処理
            //return MotoComHandler.res == MotoComES.OK;
            _IsMotoConnect = MotoComHandler.res == MotoComES.OK;
            return _IsMotoConnect;
        }

        /// <summary>
        /// 基础操作，不更新错误信息
        /// </summary>
        /// <returns></returns>
        public static bool SetMotoCom_ESClose()
        {
            MotoComHandler.res = -1;

            // 関数を実行
            MotoComHandler.res = MotoComES.ESClose(MotoComHandler._Handle);

            // 戻り値が MotoComES.OK(0):正常処理
            MotoComHandler._Handle = IntPtr.Zero;

            return MotoComHandler.res == MotoComES.OK;
        }

        public static int ESFileListFirst(ref string psFileName)
        {
            MotoComHandler.res = -1;

            // 引数となる変数の定義
            byte[] fileName = new byte[MotoComES.FileName_Length];
            //byte[] fileName = new byte[10000];

            // 関数を実行
            MotoComHandler.res = MotoComES.ESFileListFirst(MotoComHandler._Handle, 1, ref fileName[0]);

            if (MotoComHandler.res != MotoComES.OK)
            {
                MotoComHandler._ErrorMessages = MotoComHandler.ESFILELISTFIRSTERROR;
                return MotoComHandler.res;
            }

            psFileName = MotoComES.ByteArrayToString(fileName);

            return MotoComHandler.res;
        }

        public static int ESFileListNext(ref string psFileName)
        {
            MotoComHandler.res = -1;

            // 引数となる変数の定義
            byte[] fileName = new byte[MotoComES.FileName_Length];

            // 関数を実行
            MotoComHandler.res = MotoComES.ESFileListNext(MotoComHandler._Handle, ref fileName[0]);

            if (MotoComHandler.res != MotoComES.OK)
            {
                MotoComHandler._ErrorMessages = MotoComHandler.ESFILELISTNEXTERROR;
                return MotoComHandler.res;
            }

            psFileName = MotoComES.ByteArrayToString(fileName);

            return MotoComHandler.res;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static List<String> GetMotoCom_ESFileList()
        {
            if (!MotoComHandler.SetMotoCom_ESOpen())
            {
                //错误处理
                MotoComHandler._ErrorMessages = MotoComHandler.ESOPENERROR;
                return null;
            }

            //if (!MotoComHandler.ESSetTimeOut(10000, 0))
            //{
            //    return null;
            //}

            #region ESFileList操作区
            List<String> fileList = new List<string>();
            // 引数となる構造体変数の定義
            string sFileName = string.Empty;

            // ESFileListFirst
            MotoComHandler.res = MotoComHandler.ESFileListFirst(ref sFileName);

            if (MotoComHandler.res != MotoComES.OK)
            {
                return null;
            }

            // 戻り値が MotoComES.OK(0):正常処理
            fileList.Add(sFileName);

            // ESFileListNext
            for (; ; )
            {
                MotoComHandler.res = MotoComHandler.ESFileListNext(ref sFileName);

                // 戻り値が MotoComES.OK(0):正常処理
                if (MotoComHandler.res == MotoComES.OK)
                {
                    fileList.Add(sFileName);
                }
                else
                {
                    break;
                }
            }
            #endregion

            if (!MotoComHandler.SetMotoCom_ESClose())
            {
                //错误处理
                MotoComHandler._ErrorMessages = MotoComHandler.ESCLOSEERROR;
                return null;
            }

            // 結果を表示
            return fileList;
        }

        /// <summary>
        /// 把机器人上指定的文件保存到本地指定的地址
        /// </summary>
        /// <param name="sSavePath">指定保存到本地的路径</param>
        /// <param name="sFileName">指定机器人上的文件名</param>
        /// <returns></returns>
        public static bool GetMotoCom_ESSaveFile(string sSavePath, string sFileName)
        {
            if (!MotoComHandler.SetMotoCom_ESOpen())
            {
                //错误处理
                MotoComHandler._ErrorMessages = MotoComHandler.ESOPENERROR;
                return false;
            }

            #region ESSaveFile操作区域
            MotoComHandler.res = -1;

            // 引数となる変数の定義
            //string sSavePath = "C:\\Temp";
            //string sFileName = "TEST.jbi";
            byte[] savePath = MotoComES.StringToByteArray(sSavePath, MotoComHandler._ECode.GetByteCount(sSavePath) + 1);
            byte[] fileName = MotoComES.StringToByteArray(sFileName, MotoComHandler._ECode.GetByteCount(sFileName) + 1);

            // 関数を実行
            MotoComHandler.res = MotoComES.ESSaveFile(MotoComHandler._Handle, ref savePath[0], ref fileName[0]);
            //MotoComHandler.res = MotoComES.ESSaveFile(MotoComHandler._Handle, ref fileName[0], ref savePath[0]);

            if (MotoComHandler.res != MotoComES.OK)
            {
                MotoComHandler._ErrorMessages = MotoComHandler.ESSAVEFILEERROR;
                return false;
            }
            #endregion

            if (!MotoComHandler.SetMotoCom_ESClose())
            {
                //错误处理
                MotoComHandler._ErrorMessages = MotoComHandler.ESCLOSEERROR;
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
            if (!MotoComHandler.SetMotoCom_ESOpen())
            {
                //错误处理
                MotoComHandler._ErrorMessages = MotoComHandler.ESOPENERROR;
                return false;
            }

            #region ESLoadFile操作区域
            MotoComHandler.res = -1;

            // 引数となる変数の定義
            //byte[] fPath = MotoComES.StringToByteArray("C:\\Temp\\TEST.JBI",
            //    MotoComES._ECode.GetByteCount("C:\\Temp\\TEST.JBI") + 1);
            byte[] fPath = MotoComES.StringToByteArray(sLoadPath, MotoComES._ECode.GetByteCount(sLoadPath) + 1);

            // 関数を実行
            MotoComHandler.res = MotoComES.ESLoadFile(MotoComHandler._Handle, ref fPath[0]);

            if (MotoComHandler.res != MotoComES.OK)
            {
                MotoComHandler._ErrorMessages = MotoComHandler.ESLOADFILEERROR;
                return false;
            }
            #endregion

            if (!MotoComHandler.SetMotoCom_ESClose())
            {
                //错误处理
                MotoComHandler._ErrorMessages = MotoComHandler.ESCLOSEERROR;
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
            if (!MotoComHandler.SetMotoCom_ESOpen())
            {
                //错误处理
                MotoComHandler._ErrorMessages = MotoComHandler.ESOPENERROR;
                return false;
            }

            #region ESDeleteJob区域
            MotoComHandler.res = -1;

            // 引数となる変数の定義
            byte[] fPath = MotoComES.StringToByteArray(jobName, MotoComES._ECode.GetByteCount(jobName) + 1);

            // 関数を実行
            MotoComHandler.res = MotoComES.ESDeleteJob(MotoComHandler._Handle, ref fPath[0]);

            if (MotoComHandler.res != MotoComES.OK)
            {
                MotoComHandler._ErrorMessages = MotoComHandler.ESDELETEJOBERROR;
                MotoComHandler.SetMotoCom_ESClose();    //此种情况下通常连接是建立成功的，因此需要先关闭连接
                return false;
            }
            #endregion

            if (!MotoComHandler.SetMotoCom_ESClose())
            {
                //错误处理
                MotoComHandler._ErrorMessages = MotoComHandler.ESCLOSEERROR;
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
            if (!MotoComHandler.SetMotoCom_ESOpen())
            {
                //错误处理
                MotoComHandler._ErrorMessages = MotoComHandler.ESOPENERROR;
                return false;
            }

            #region ESServo操作区域
            MotoComHandler.res = -1;
            // 関数を実行
            MotoComHandler.res = MotoComES.ESServo(MotoComHandler._Handle, onOff); //Servo On

            if(MotoComHandler.res != MotoComES.OK)
            {
                MotoComHandler._ErrorMessages = MotoComHandler.ESSERVOERROR;
                return false;
            }
            #endregion

            if (!MotoComHandler.SetMotoCom_ESClose())
            {
                //错误处理
                MotoComHandler._ErrorMessages = MotoComHandler.ESCLOSEERROR;
                return false;
            }

            return true;
        }

        public static bool SetMotoCom_ESSelectJob(string jobNameStr, int jobType = 1, int lineNo = 1)
        {
            if (!MotoComHandler.SetMotoCom_ESOpen())
            {
                //错误处理
                MotoComHandler._ErrorMessages = MotoComHandler.ESOPENERROR;
                return false;
            }

            #region ESSelectJob操作区域
            MotoComHandler.res = -1;

            // 引数となる変数の定義
            byte[] jobName = MotoComES.StringToByteArray(jobNameStr, MotoComES._ECode.GetByteCount(jobNameStr) + 1);

            // 関数を実行
            MotoComHandler.res = MotoComES.ESSelectJob(MotoComHandler._Handle, jobType, lineNo, ref jobName[0]);

            if (MotoComHandler.res != MotoComES.OK)
            {
                MotoComHandler._ErrorMessages = MotoComHandler.ESSELECTJOBERROR;
                return false;
            }
            #endregion

            if (!MotoComHandler.SetMotoCom_ESClose())
            {
                //错误处理
                MotoComHandler._ErrorMessages = MotoComHandler.ESCLOSEERROR;
                return false;
            }

            return true;
        }

        public static bool SetMotoCom_ESStartJob()
        {
            if (!MotoComHandler.SetMotoCom_ESOpen())
            {
                //错误处理
                MotoComHandler._ErrorMessages = MotoComHandler.ESOPENERROR;
                return false;
            }

            #region ESStartJob操作区域
            MotoComHandler.res = -1;

            // 関数を実行
            MotoComHandler.res = MotoComES.ESStartJob(MotoComHandler._Handle);

            if (MotoComHandler.res != MotoComES.OK)
            {
                MotoComHandler._ErrorMessages = MotoComHandler.ESSTARTJOBERROR;
                return false;
            }
            #endregion

            if (!MotoComHandler.SetMotoCom_ESClose())
            {
                //错误处理
                MotoComHandler._ErrorMessages = MotoComHandler.ESCLOSEERROR;
                return false;
            }

            return true;
        }

        public static bool SetMotoCom_ESCancel()
        {
            if (!MotoComHandler.SetMotoCom_ESOpen())
            {
                //错误处理
                MotoComHandler._ErrorMessages = MotoComHandler.ESOPENERROR;
                return false;
            }

            #region ESCancel操作区域
            MotoComHandler.res = -1;

            // 関数を実行
            MotoComHandler.res = MotoComES.ESCancel(MotoComHandler._Handle);

            if (MotoComHandler.res != MotoComES.OK)
            {
                MotoComHandler._ErrorMessages = MotoComHandler.ESCANCELERROR;
                return false;
            }
            #endregion

            if (!MotoComHandler.SetMotoCom_ESClose())
            {
                //错误处理
                MotoComHandler._ErrorMessages = MotoComHandler.ESCLOSEERROR;
                return false;
            }

            return true;
        }

        public static bool SetMotoCom_ESHold(int onOff)
        {
            if (!MotoComHandler.SetMotoCom_ESOpen())
            {
                //错误处理
                MotoComHandler._ErrorMessages = MotoComHandler.ESOPENERROR;
                return false;
            }

            #region ESHold操作区域
            MotoComHandler.res = -1;

            // 関数を実行
            MotoComHandler.res = MotoComES.ESHold(MotoComHandler._Handle, onOff);

            if (MotoComHandler.res != MotoComES.OK)
            {
                MotoComHandler._ErrorMessages = MotoComHandler.ESHOLDERROR;
                return false;
            }
            #endregion

            if (!MotoComHandler.SetMotoCom_ESClose())
            {
                //错误处理
                MotoComHandler._ErrorMessages = MotoComHandler.ESCLOSEERROR;
                return false;
            }

            return true;
        }

        public static bool SetMotoCom_ESReset()
        {
            if (!MotoComHandler.SetMotoCom_ESOpen())
            {
                //错误处理
                MotoComHandler._ErrorMessages = MotoComHandler.ESOPENERROR;
                return false;
            }

            #region ESHold操作区域
            MotoComHandler.res = -1;

            // 関数を実行
            MotoComHandler.res = MotoComES.ESReset(MotoComHandler._Handle);

            if (MotoComHandler.res != MotoComES.OK)
            {
                MotoComHandler._ErrorMessages = MotoComHandler.ESRESETERROR;
                return false;
            }
            #endregion

            if (!MotoComHandler.SetMotoCom_ESClose())
            {
                //错误处理
                MotoComHandler._ErrorMessages = MotoComHandler.ESCLOSEERROR;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 获取机器人的一个I型数值
        /// </summary>
        public static short GetMotoCom_ESGetVarDataMI(int varNo)
        {
            varNo += _readAddrOffset;

            if (!MotoComHandler.SetMotoCom_ESOpen())
            {
                //错误处理
                MotoComHandler._ErrorMessages = MotoComHandler.ESOPENERROR;
                return -1;
            }

            #region ESGetVarDataMI操作区域
            MotoComHandler.res = -1;

            // 引数となる構造体変数の定義
            MotoComES.ESMultiShortData multiData = new MotoComES.ESMultiShortData();
            multiData.Init();

            // 関数を実行
            MotoComHandler.res = MotoComES.ESGetVarDataMI(MotoComHandler._Handle, varNo, 1, ref multiData); //RS022=0

            if (MotoComHandler.res != MotoComES.OK)
            {
                MotoComHandler._ErrorMessages = MotoComHandler.ESGETVARDATAMIERROR;
                return -1;
            }
            #endregion

            if (!MotoComHandler.SetMotoCom_ESClose())
            {
                //错误处理
                MotoComHandler._ErrorMessages = MotoComHandler.ESCLOSEERROR;
                return -1;
            }

            return multiData.data[0];
        }

        /// <summary>
        /// 设置机器人的一个I型数值
        /// </summary>
        public static bool SetMotoCom_ESSetVarDataMI(int varNo, short val)
        {
            varNo += _writeAddrOffset;

            if (!MotoComHandler.SetMotoCom_ESOpen())
            {
                //错误处理
                MotoComHandler._ErrorMessages = MotoComHandler.ESOPENERROR;
                return false;
            }

            #region ESSetVarDataMI操作区域
            MotoComHandler.res = -1;

            // 引数となる構造体変数の定義
            MotoComES.ESMultiShortData multiData = new MotoComES.ESMultiShortData();
            multiData.Init();

            //multiDataに値をセット
            multiData.data[0] = val;
            
            // 関数を実行
            MotoComHandler.res = MotoComES.ESSetVarDataMI(MotoComHandler._Handle, varNo, 1, multiData); //RS022=0

            if (MotoComHandler.res != MotoComES.OK)
            {
                MotoComHandler._ErrorMessages = MotoComHandler.ESSETVARDATAMIERROR;
                return false;
            }
            #endregion

            if (!MotoComHandler.SetMotoCom_ESClose())
            {
                //错误处理
                MotoComHandler._ErrorMessages = MotoComHandler.ESCLOSEERROR;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 设置机器人的多个I型数值
        /// </summary>
        public static bool SetMotoCom_ESSetVarDataMI_Multi(int varNo, List<short> val)
        {
            varNo += _writeAddrOffset;

            if (!MotoComHandler.SetMotoCom_ESOpen())
            {
                //错误处理
                MotoComHandler._ErrorMessages = MotoComHandler.ESOPENERROR;
                return false;
            }

            #region ESSetVarDataMI操作区域
            MotoComHandler.res = -1;

            // 引数となる構造体変数の定義
            MotoComES.ESMultiShortData multiData = new MotoComES.ESMultiShortData();
            multiData.Init();

            //multiDataに値をセット
            for (int i = 0; i < val.Count; i++)
                multiData.data[i] = val[i];

            // 関数を実行
            MotoComHandler.res = MotoComES.ESSetVarDataMI(MotoComHandler._Handle, varNo, val.Count, multiData); //RS022=0

            if (MotoComHandler.res != MotoComES.OK)
            {
                MotoComHandler._ErrorMessages = MotoComHandler.ESSETVARDATAMIMULTIERROR;
                return false;
            }
            #endregion

            if (!MotoComHandler.SetMotoCom_ESClose())
            {
                //错误处理
                MotoComHandler._ErrorMessages = MotoComHandler.ESCLOSEERROR;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 设置机器人的多个I型数值
        /// </summary>
        public static bool SetMotoCom_ESSetVarDataMI(int startVarNo, short[] valArray)
        {
            startVarNo += _writeAddrOffset;

            if (!MotoComHandler.SetMotoCom_ESOpen())
            {
                //错误处理
                MotoComHandler._ErrorMessages = MotoComHandler.ESOPENERROR;
                return false;
            }

            #region ESSetVarDataMI操作区域
            MotoComHandler.res = -1;

            // 引数となる構造体変数の定義
            MotoComES.ESMultiShortData multiData = new MotoComES.ESMultiShortData();
            multiData.Init();

            //multiDataに値をセット
            int number = valArray.Count();
            for (int i = 0; i < number; i++)
                multiData.data[i] = valArray[i];

            // 関数を実行
            MotoComHandler.res = MotoComES.ESSetVarDataMI(MotoComHandler._Handle, startVarNo, number, multiData); //RS022=0

            if (MotoComHandler.res != MotoComES.OK)
            {
                MotoComHandler._ErrorMessages = MotoComHandler.ESSETVARDATAMIERROR;
                return false;
            }
            #endregion

            if (!MotoComHandler.SetMotoCom_ESClose())
            {
                //错误处理
                MotoComHandler._ErrorMessages = MotoComHandler.ESCLOSEERROR;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 获取机器人的一个D型数值
        /// </summary>
        public static int GetMotoCom_ESGetVarDataMD(int varNo)
        {
            if (!MotoComHandler.SetMotoCom_ESOpen())
            {
                //错误处理
                MotoComHandler._ErrorMessages = MotoComHandler.ESOPENERROR;
                return -1;
            }

            #region ESGetVarDataMD操作区域
            MotoComHandler.res = -1;

            // 引数となる構造体変数の定義
            MotoComES.ESMultiLongData multiData = new MotoComES.ESMultiLongData();
            multiData.Init();

            // 関数を実行
            MotoComHandler.res = MotoComES.ESGetVarDataMD(MotoComHandler._Handle, varNo, 1, ref multiData); //RS022=0

            if (MotoComHandler.res != MotoComES.OK)
            {
                MotoComHandler._ErrorMessages = MotoComHandler.ESGETVARDATAMDERROR;
                return -1;
            }
            #endregion

            if (!MotoComHandler.SetMotoCom_ESClose())
            {
                //错误处理
                MotoComHandler._ErrorMessages = MotoComHandler.ESCLOSEERROR;
                return -1;
            }

            return multiData.data[0];
        }

        /// <summary>
        /// 设置机器人的一个D型数值
        /// </summary>
        public static bool SetMotoCom_ESSetVarDataMD(int varNo, int val)
        {
            if (!MotoComHandler.SetMotoCom_ESOpen())
            {
                //错误处理
                MotoComHandler._ErrorMessages = MotoComHandler.ESOPENERROR;
                return false;
            }

            #region ESSetVarDataMI操作区域
            MotoComHandler.res = -1;

            // 引数となる構造体変数の定義
            MotoComES.ESMultiLongData multiData = new MotoComES.ESMultiLongData();
            multiData.Init();

            //multiDataに値をセット
            multiData.data[0] = val;

            // 関数を実行
            MotoComHandler.res = MotoComES.ESSetVarDataMD(MotoComHandler._Handle, varNo, 1, multiData); //RS022=0

            if (MotoComHandler.res != MotoComES.OK)
            {
                MotoComHandler._ErrorMessages = MotoComHandler.ESSETVARDATAMDERROR;
                return false;
            }
            #endregion

            if (!MotoComHandler.SetMotoCom_ESClose())
            {
                //错误处理
                MotoComHandler._ErrorMessages = MotoComHandler.ESCLOSEERROR;
                return false;
            }

            return true;
        }

        public static bool ESSetTimeOut(int time, int retry)
        {
            MotoComHandler.res = -1;

            // 関数を実行
            //MotoComHandler.res = MotoComES.ESFileListFirst(MotoComHandler._Handle, 1, ref fileName[0]);
            MotoComHandler.res = MotoComES.ESSetTimeOut(MotoComHandler._Handle, time, retry);

            if (MotoComHandler.res != MotoComES.OK)
            {
                MotoComHandler._ErrorMessages = MotoComHandler.ESSETTIMEOUTERROR;
                return false;
            }

            return true;
        }

        #region 错误代码映射区
        //// 摘要: 
        ////     変数データ最大領域数(バイト数) : 13 + 1
        //public const int DCI_VarData_Length = 14;
        ////
        //// 摘要: 
        ////     コマンド受信エラー : 9003
        //public const int ES_COMMAND_RECEIVE_ERROR = 36867;
        ////
        //// 摘要: 
        ////     未接続エラー : 9000
        //public const int ES_CONNECTION_ERROR = 36864;
        ////
        //// 摘要: 
        ////     受信タイムアウトエラー : 9001
        //public const int ES_CONNECTION_TIMEOUT_ERROR = 36865;
        ////
        //// 摘要: 
        ////     DCI bindエラー : 9010
        //public const int ES_DCI_BIND_ERROR = 36880;
        ////
        //// 摘要: 
        ////     DCI 受信コマンド不一致エラー : 9015
        //public const int ES_DCI_COMMAND_UNMATCHING_ERROR = 36885;
        ////
        //// 摘要: 
        ////     DCI イベントエラー : 9012
        //public const int ES_DCI_EVENT_ERROR2 = 36882;
        ////
        //// 摘要: 
        ////     DCI イベントエラー : 9013
        //public const int ES_DCI_EVENT_ERROR3 = 36883;
        ////
        //// 摘要: 
        ////     DCI ファイルオープンエラー : 9017
        //public const int ES_DCI_FILE_OPEN_ERROR = 36887;
        ////
        //// 摘要: 
        ////     DCI 受信procエラー : 9016
        //public const int ES_DCI_PROC_UNMATCHING_ERROR = 36886;
        ////
        //// 摘要: 
        ////     DCI 受信サイズエラー : 9014
        //public const int ES_DCI_RECEIVE_SIZE_ERROR = 36884;
        ////
        //// 摘要: 
        ////     DCI タイムアウト : 9011
        //public const int ES_DCI_TIMEOUT = 36881;
        ////
        //// 摘要: 
        ////     使用不可コマンドエラー : 9200
        //public const int ES_DISABLED_COMMAND_ERROR = 37376;
        ////
        //// 摘要: 
        ////     ファイル読み込みエラー : 9006
        //public const int ES_FILE_LOAD_ERROR = 36870;
        ////
        //// 摘要: 
        ////     ファイル受信エラー : 9005
        //public const int ES_FILE_RECEIVE_ERROR = 36869;
        ////
        //// 摘要: 
        ////     ファイル保存エラー : 9006
        //public const int ES_FILE_SAVE_ERROR = 36870;
        ////
        //// 摘要: 
        ////     ファイル送信エラー : 9004
        //public const int ES_FILE_SEND_ERROR = 36868;
        ////
        //// 摘要: 
        ////     パラメータ指定エラー : 9100
        //public const int ES_PARAM_ERROR = 37120;
        ////
        //// 摘要: 
        ////     送信エラー : 9002
        //public const int ES_SEND_ERROR = 36866;
        ////
        //// 摘要: 
        ////     SERVICE未実装エラー : 08
        //public const int ES_SERVICE_NOT_SUPPORTED = 8;
        ////
        //// 摘要: 
        ////     ファイル名最大文字数(バイト数) : 32 + 1
        //public const int FileName_Length = 33;
        ////
        //// 摘要: 
        ////     正常処理 : 0
        //public const int OK = 0;
        ////
        //// 摘要: 
        ////     文字列最大文字数(バイト数) : 16 + 1
        //public const int String_Length = 17;
        ////
        //// 摘要: 
        ////     文字列最大文字数[DX200](バイト数) : 32 + 1
        //public const int String_Length2 = 33;
        #endregion
    }
}
