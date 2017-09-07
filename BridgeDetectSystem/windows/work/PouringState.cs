using BridgeDetectSystem.adam;
using BridgeDetectSystem.entity;
using BridgeDetectSystem.service;
using MetroFramework.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BridgeDetectSystem
{
    public partial class PouringState : MetroFramework.Forms.MetroForm
    {
        AdamHelper adamHelper;
        DataStoreManager dataStoreManager;
        WarningManager warningManager;
        ConfigManager config;
        double steeveForceLimit;
        double steeveForceDiffLimit;
        double steeveDisLimit;
        double steeveDisDiffLimit;
        double anchorForceLimit;
        double anchorForceDiffLimit;
        double FrontDisLimit;
        double basketupDisLimit;
        double allowDisDiffLimit;
         double firstStandard;
        double secondStanard;
        public PouringState()
        {
            InitializeComponent();
            adamHelper = AdamHelper.GetInstance();
           
            dataStoreManager = DataStoreManager.GetInstance();
            warningManager = WarningManager.GetInstance();
            config = ConfigManager.GetInstance();
          
        }

        private void PourState_Load(object sender, EventArgs e)
        {
            this.panel1.BackColor = Color.FromArgb(255, 50, 161, 206);
            this.panel2.Width = this.panel1.Width / 2;
            this.panel4.Height = (this.panel1.Height - menuStrip1.Height) / 2;
            this.panel6.Height = (this.panel1.Height - menuStrip1.Height) / 2;
            this.panel8.Width = this.panel7.Width / 2;
           
            //得到配置项的值
            steeveForceLimit = config.Get(ConfigManager.ConfigKeys.steeve_ForceLimit);
            steeveForceDiffLimit = config.Get(ConfigManager.ConfigKeys.steeve_ForceDiffLimit);
            basketupDisLimit = config.Get(ConfigManager.ConfigKeys.basket_upDisLimit);
            allowDisDiffLimit = config.Get(ConfigManager.ConfigKeys.basket_allowDisDiffLimit);
            steeveDisLimit = basketupDisLimit + allowDisDiffLimit;
            steeveDisDiffLimit = config.Get(ConfigManager.ConfigKeys.steeve_DisDiffLimit);
            anchorForceLimit = config.Get(ConfigManager.ConfigKeys.anchor_ForceLimit);
            anchorForceDiffLimit = config.Get(ConfigManager.ConfigKeys.anchor_ForceDiffLimit);
            FrontDisLimit = config.Get(ConfigManager.ConfigKeys.frontPivot_DisLimit);
            //开始接收数据
            adamHelper.StartTimer(250);
            dataStoreManager.StartTimer(500, 1000);
            warningManager.BgStart();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            
            RefreshSteeveText();
            RefreshAnchorText();
            RefreshFrontPivotText();
        }
        /// <summary>
/// 关闭窗体时关闭线程
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>
        private void PouringState_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (dataStoreManager != null)
            {
                dataStoreManager.StopTimer();
            }
            if (adamHelper != null)
            {
                adamHelper.StopTimer();
            }
            if (warningManager != null && warningManager.isStart)
            {
                warningManager.BgCancel();
            }
        }

        #region 数据显示和重置按钮
        /// <summary>
        /// 给吊杆力和吊杆位移文本框赋值
        /// </summary>
        private void RefreshSteeveText()
        {
            Dictionary<int, Steeve> dicSteeve = adamHelper.steeveDic;//得到吊杆的字典集合，用方法得到力和位移
            double[] steeveForce = new double[dicSteeve.Count];//吊杆力数组，元素为double
            double[] steeveDis = new double[dicSteeve.Count];//吊杆位移数组，元素为double
            for (int i = 0; i < 4; i++)
            {
                steeveForce[i] = dicSteeve[i].GetForce();//为吊杆力数组赋值值
                steeveDis[i] = dicSteeve[i].GetDisplace();//为吊杆力位移数组赋值
            }
            SetTextValueManager.SetValueToText(steeveForce, ref txtSteeveF1, ref txtSteeveF2, ref txtSteeveF3, ref txtSteeveF4, ref txtMaxSteeveForce, ref txtMaxSteeveForceDiff);
            txtSteeveForceLimit.Text = steeveForceLimit.ToString();
            txtSteeveForceDiffLimit.Text = steeveForceDiffLimit.ToString();
            SetTextValueManager.SetValueToText(steeveDis, ref txtSteeveDis1, ref txtSteeveDis2, ref txtSteeveDis3, ref txtSteeveDis4, ref txtMaxSteeveDis, ref txtMaxSteeveDisDiff);
            txtSteeveDisLimit.Text = steeveDisLimit.ToString();
            txtSteeveDisDiffLimit.Text = steeveDisDiffLimit.ToString();
        }

        /// <summary>
        /// 给锚杆力文本框赋值
        /// </summary>
        private void RefreshAnchorText()
        {
            Dictionary<int, Anchor> dicAnchor = adamHelper.anchorDic;
            double[] anchorForce = new double[dicAnchor.Count];
            for (int i = 0; i < 4; i++)
            {
                anchorForce[i] = dicAnchor[i].GetForce();
            }
            SetTextValueManager.SetValueToText(anchorForce, ref txtAnchorF1, ref txtAnchorF2, ref txtAnchorF3, ref txtAnchorF4, ref txtMaxAnchorForce, ref txtMaxAnchorForceDiff);
            txtAnchorForceLimit.Text = anchorForceLimit.ToString();
            txtAnchorForceDiffLimit.Text = anchorForceDiffLimit.ToString();

        }

        /// <summary>
        /// 给前支点位移文本框赋值
        /// </summary>
        private void RefreshFrontPivotText()
        {
            firstStandard = adamHelper.first_frontPivotDisStandard;
            secondStanard = adamHelper.second_frontPivotDisStandard;
            Dictionary<int, FrontPivot> dicFrontPivot = adamHelper.frontPivotDic;
            double[] frontPivotDis = new double[dicFrontPivot.Count];
            
            
                frontPivotDis[0] = dicFrontPivot[0].GetDisplace()-firstStandard;//数组存位移
            frontPivotDis[1] = dicFrontPivot[1].GetDisplace() - secondStanard;
            txtFrontPivotDis1.Text = frontPivotDis[0].ToString();
            txtFrontPivotDis2.Text = frontPivotDis[1].ToString();
            txtFrontDIsDiffLimit.Text = FrontDisLimit.ToString();
        }

        /// <summary>
        /// 行走后重置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReset_Click(object sender, EventArgs e)
        {
            if (dataStoreManager != null)
            {
                dataStoreManager.StopTimer();
            }
            if (adamHelper != null)
            {
                adamHelper.StopTimer();
            }


           
            try
            {
                adamHelper.StartTimer(250);
                dataStoreManager.StartTimer(500, 1000);
            }
            catch(Exception ex)
            {
                MessageBox.Show("重置发生错误" + ex.Message);
            }
           
            //try
            //{
            //    adamHelper.ReadStandardValue();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("重置发生错误：" + ex.Message);
            //}
        }
        #endregion

        #region 菜单栏按钮功能方法
        private void btnExit_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("你确定退出吗？ ",
                                   " 提示",
                                  MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                System.Environment.Exit(0);
            }
        }

        private void btnFirstPage_Click(object sender, EventArgs e)
        {
            this.Close();
        }

       

        #endregion

    }
}
