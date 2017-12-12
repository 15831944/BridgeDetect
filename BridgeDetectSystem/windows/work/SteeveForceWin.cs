using BridgeDetectSystem.adam;
using BridgeDetectSystem.entity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BridgeDetectSystem.windows.work
{
    public partial class SteeveForceWin : MetroFramework.Forms.MetroForm
    {
        public SteeveForceWin()
        {
            InitializeComponent();
        }
        AdamHelper adamhelper;

        private void SteeveForceWin_Load(object sender, EventArgs e)
        {
            adamhelper = AdamHelper.GetInstance();
        }
        /// <summary>
        /// 刷新数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            DisplayData();
        }
        private void DisplayData()
        {
            try
            {
                Dictionary<int, Steeve> dicSteeve = adamhelper.steeveDic;//得到吊杆的字典集合，用方法得到力和位移
                double[] steeveForce = new double[4];//吊杆力数组，元素为double

                for (int i = 0; i < 4; i++)
                {
                    steeveForce[i] = dicSteeve[i].GetForce();//为吊杆力数组赋值值
                }

                lblF1.Text = steeveForce[0].ToString();
                lblF2.Text = steeveForce[1].ToString();
                lblF3.Text = steeveForce[2].ToString();
                lblF4.Text = steeveForce[3].ToString();
             }
            catch (Exception ex)
            {
                timer1.Enabled = false;
                MessageBox.Show("采集吊杆力，位移数据失败，请检查硬件后重启软件。" + ex.Message);
            }
        }
    }
}
