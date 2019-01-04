using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
