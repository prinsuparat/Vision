using Inspection.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication2
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var winLoad = new WinLoad();
            winLoad.Show();
            winLoad.Activate();
            var form1 = new Form1();
            // 订阅 Form1 的 Load 事件，在加载完成后关闭 WinLoad 窗口
            form1.Load += (sender, e) =>
            {
                winLoad.Close();
            };
            Application.Run(form1);
        }
    }
}
