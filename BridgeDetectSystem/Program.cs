﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BridgeDetectSystem.service;
using BridgeDetectSystem.util;
using BridgeDetectSystem.adam;

namespace BridgeDetectSystem
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Initialize();

            Login login = new Login();
            if (login.ShowDialog() == DialogResult.OK)
            {
                login.Close();
                Application.Run(new MainWin());
            }

            //  TestForm testform = new TestForm();
            // Application.Run(testform);
        }

        private static void Initialize()
        {
            //操作日志初始化
            log4net.Config.XmlConfigurator.Configure();

            //数据库初始化
            try
            {
                bool recreate = false;
                if (recreate)
                {
                    RecreateRecordManager.InitialDataBase();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("初始化数据库表报错:" + ex.Message);
            }
            //配置初始化
            try
            {
                DBHelper dbhelper = DBHelper.GetInstance();
                ConfigManager.Initialize(dbhelper, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show("配置初始化错误" + ex.Message);
            }


            //浇筑状态接收线程初始化
            List<AdamOperation> list = new List<AdamOperation>
            {
                new Adam6217Operation("192.168.1.3", 0)
            };

            try
            {
                AdamHelper.Initialize(list);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            //行走状态接收线程初始化




            //数据保存类初始化
            try
            {
                DataStoreManager.Initialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

    }
}
