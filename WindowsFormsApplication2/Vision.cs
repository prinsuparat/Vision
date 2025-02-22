using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cognex;
using Cognex.VisionPro;
using Cognex.VisionPro.ToolBlock;
using Cognex.VisionPro.CalibFix;
using System.Net;
using System.Net.Sockets;
using INIOperationClass;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace WindowsFormsApplication2
{
    public  class Vision
    {
        public CogAcqFifoTool[] camera;//取向
        public CogToolBlock[] RunTB;//运行作业
        public CogToolBlock[] CalibateTB;//标定作业
        public CogCalibNPointToNPointTool[] CalibCamera;//标定工具
        public CogRecordDisplay[] display;
        public static string[] camera_Path;
        public static string[] RunTB_Path;
        public static string[] CalibateTB_Path;
        public static string[] CalibCamera_Path;

        public  int Cameranum, producenum;//相机、产品数量
        public static string[] CameraIP;//相机IP
        public static string[] ProduceName;
        public string[] Cameraname;
        public static string[] Cameraboard;
        public static string ChoiceCamera, choiceboard, choiceWorkTB;
        public Label[] cameraTxtlabel;
        public Label[] cameraResultlabel;
        public CheckBox[] checkbox;
        public bool loginStation = false;

        public static string Config_Path = Application.StartupPath + "\\Config\\config.ini";//配置文件地址
        public static string user_path = Application.StartupPath + "\\User.ini";
        //public static string imgSavePath = @"D:\Image";
        public static string imgSavePath = Application.StartupPath + "\\Image";
        public static int imgSaveDays = 10;
        public string [] 相机品牌;
        public string[] 相机曝光;
        public bool[] 旋转中心;
        public double [] 基准X;
        public double [] 基准Y;
        public double [] 基准A;
        public double[] 运行X;
        public double[] 运行Y;
        public double[] 运行A;
        public double 坐标X;
        public double 坐标Y;
        public double 坐标A;
        public double 补偿X;
        public double 补偿Y;
        public double 补偿A;
        public double[] 偏移X;
        public double[] 偏移Y;
        public double[] 偏移A;
        public string 机械手数量;
        public string[] 机械手IP;
        public string[] 拍照信号;
        public string[] 相机功能;
        public string[] 显示图层;
        public double[] 中心X;
        public double[] 中心Y;
        public double 机械手X;
        public double 机械手Y;
        public double 机械手A;
        
        public string BayNo="null";
        public string ProductModel = "null";
        public string Version = "null";
        public string ProcessStation = "null";
        public string JGlue_GRN = "null";
        public string JLens_GRN = "null";
        public string JLens_QR_code = "null";

        public Int64  JGlue_GRN_count =0;
        public Int64 JLens_GRN_count =0;
        public Int64 JLens_QR_code_count =0;
        public int JGlue_GRN_length = 0;
        public int JLens_GRN_length = 0;
        public int JLens_QR_code_length = 0;
        public static string Produce_Date = Application.StartupPath + "\\Config\\Produce Message.ini";//配置文件地址



        #region 服务器
        public Socket m_Listener=null;
        public IPEndPoint ipe = null;
        public string  SocketIP ="192.168.1.1";
        public int Port ;
        public bool m_bServerOnLine = false;
        public Socket m_Handler = null;
       

        #endregion

        #region 客户端
        public Socket tcpClient;
        public   string clientIP = "192.168.255.1";
        public   string clientPort = "11000";
        #endregion

        #region 写入CSV类

        /// <summary>
        ///写入CSV类
        /// </summary>
        public class CSV
        {
            private static object _lock = new object();
            public static void WriteCSV(string DataFilepath, string text)
            {
                System.IO.StreamWriter sw = null;
                if (!Directory.Exists(DataFilepath))
                {
                    Directory.CreateDirectory(DataFilepath);
                }
                string fileFullFileName = DataFilepath + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".csv";

                if (!File.Exists(fileFullFileName))
                {
                    using (sw = System.IO.File.AppendText(fileFullFileName))
                    {
                        //标题自定义
                        string str = "Date,Time,Result_X,Result_Y,Result_A";
                        sw.WriteLine(str);
                    }
                }
                lock (_lock)
                {
                    try
                    {
                        using (sw = System.IO.File.AppendText(fileFullFileName))
                        {
                            sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd,HH:mm:ss:,") + text);
                        }
                    }
                    catch
                    {
                        foreach (Process process in System.Diagnostics.Process.GetProcesses())
                        {
                            if (process.ProcessName.ToUpper().Equals("EXCEL"))
                                process.Kill();
                        }
                        GC.Collect();
                        Thread.Sleep(200);
                        using (sw = System.IO.File.AppendText(fileFullFileName))
                        {
                            sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd,HH:mm:ss:,") + text);
                        }
                    }
                }
            }
        }

        //此处再抄一个读文件

        #endregion


       
       
    }
}
