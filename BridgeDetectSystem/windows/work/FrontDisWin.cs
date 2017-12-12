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
    public partial class FrontDisWin : MetroFramework.Forms.MetroForm
    {
        public FrontDisWin()
        {
            InitializeComponent();
        }
        AdamHelper adamhelper;
        private double firstStandard;
        private double secondStanard;
        private double threeStandard;
        private double fourStandard;

        private void FrontDisWin_Load(object sender, EventArgs e)
        {
            adamhelper = AdamHelper.GetInstance();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            DisplayData();
        }
        private void DisplayData()
        {
            try
            {
                firstStandard = adamhelper.first_frontPivotDisStandard;
                secondStanard = adamhelper.second_frontPivotDisStandard;
                threeStandard = adamhelper.three_standard;
                fourStandard = adamhelper.four_standard;//四个基准
                Dictionary<int, FrontPivot> dicFrontPivot = adamhelper.frontPivotDic;
                double[] frontPivotDis = new double[4];//沉降位移数组           
                frontPivotDis[0] = Math.Round(firstStandard - dicFrontPivot[0].GetDisplace(), 1);
                frontPivotDis[1] = Math.Round(secondStanard - dicFrontPivot[1].GetDisplace(), 1);
                frontPivotDis[2] = Math.Round(threeStandard - dicFrontPivot[2].GetDisplace(), 1);
                frontPivotDis[3] = Math.Round(fourStandard - dicFrontPivot[3].GetDisplace(), 1);
                for (int k = 0; k < 4; k++)
                {
                    frontPivotDis[k] = Math.Abs(frontPivotDis[k]);
                }
                lblS1.Text = frontPivotDis[0].ToString();
                lblS2.Text = frontPivotDis[1].ToString();
                lblS3.Text = frontPivotDis[2].ToString();
                lblS4.Text = frontPivotDis[3].ToString();

              
             


            }

            catch (Exception ex)
            {
                timer1.Enabled = false;
                MessageBox.Show("采集前支点位移数据失败，请检查硬件后重启软件。" + ex.Message);
            }

        }
    }
}
