using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cognex;
using Cognex.VisionPro;
using Cognex.VisionPro.ToolBlock;


namespace WindowsFormsApplication2
{
    public partial class CameraAcq : Form
    {
        Form1 f;
        public CameraAcq(Form1 f1)
        {
            InitializeComponent();
            f = f1;
        }
        private void CameraAcq_Load(object sender, EventArgs e)
        {
            Invoke(new MethodInvoker(delegate
                {
                    this.label2.Text = f.ChoiceCamera;
                }));
            cogAcqFifoEditV21.Subject = f.ChoiceAcq;
            

        }

        private void button1_Click(object sender, EventArgs e)
        {
            cogAcqFifoEditV21.Subject.Run();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                CogSerializer.SaveObjectToFile(f.ChoiceAcq, f.ChoiceAcq_Path);
                MessageBox.Show("保存成功");
            }
            catch(Exception ex)
            {
                MessageBox.Show("保存失败");
            }
           
        }
       
    }
}
