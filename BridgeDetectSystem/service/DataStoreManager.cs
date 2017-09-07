﻿using BridgeDetectSystem.adam;
using BridgeDetectSystem.entity;
using BridgeDetectSystem.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace BridgeDetectSystem.service
{
    public class DataStoreManager
    {
        System.Threading.Timer storeTimer { get; set; }
        DBHelper dbhelper;
        AdamHelper adamHelper;
        string name;


        private DataStoreManager()
        {
            dbhelper = DBHelper.GetInstance();
            adamHelper = AdamHelper.GetInstance();
            name = UserRightManager.user.userName;//得到操作人的名字
          //  name = "admin";
            storeTimer = new System.Threading.Timer(_ =>
            {
                if (adamHelper.hasData)
                {
                    storeData();
                }
            }, null, Timeout.Infinite, Timeout.Infinite);
        }

        private static volatile DataStoreManager instance;
        public static DataStoreManager Initialize()
        {
            if (instance != null)
            {
                throw new Exception("初始化数据保存类失败，实例已存在.");
            }

            instance = new DataStoreManager();
            return instance;
        }

        public static DataStoreManager GetInstance()
        {
            if (instance == null)
            {
                throw new Exception("数据保存类实例不存在报错！");
            }
            return instance;
        }

        /// <summary>
        /// 将吊杆数据，力和位移存入数据库
        /// </summary>
        public void InsertSteeveData()
        {
           
            Dictionary<int, Steeve> dicSteeve = adamHelper.steeveDic;       //吊杆
            string sqlSteeveForce = string.Format("insert into SteeveForce values(newid(),getdate(),'{0}',{1},{2},{3},{4})", name, dicSteeve[0].GetForce(), dicSteeve[1].GetForce(), dicSteeve[2].GetForce(), dicSteeve[3].GetForce());                      //吊杆力
            string sqlSteeveDis = string.Format("insert into SteeveDisplacement values(newid(),getdate(),'{0}',{1},{2},{3},{4})", name, dicSteeve[0].GetDisplace(), dicSteeve[1].GetDisplace(), dicSteeve[2].GetDisplace(), dicSteeve[3].GetDisplace());          //吊杆位移
            try
            {
                int r1 = dbhelper.ExecuteNonQuery(sqlSteeveForce);
                int r2 = dbhelper.ExecuteNonQuery(sqlSteeveDis);
            }
            catch (Exception ex)
            {
                throw new Exception("将吊杆数据，力和位移存入数据库报错：" + ex.Message);
            }
        }
        /// <summary>
        /// 将锚杆力记录存入数据库
        /// </summary>
        public void InsertAnchorData()
        {
            Dictionary<int, Anchor> dicAnchor = adamHelper.anchorDic;
            string sql = string.Format("insert into AnchorForce values(newid(),getdate(),'{0}',{1},{2},{3},{4})", name, dicAnchor[0].GetForce(), dicAnchor[1].GetForce(), dicAnchor[2].GetForce(), dicAnchor[3].GetForce());
            try
            {
                int r = dbhelper.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                throw new Exception("将锚杆力记录存入数据库报错：" + ex.Message);
            }
        }
        /// <summary>
        /// 将前支点位移存入数据库
        /// </summary>
        public void InsertFrontPivotDis()
        {
            double firstStandard;
            double secondStandard;
            firstStandard = adamHelper.first_frontPivotDisStandard;
            secondStandard = adamHelper.second_frontPivotDisStandard;
            Dictionary<int, FrontPivot> dicFrontPivot = adamHelper.frontPivotDic;
            double[] frontPivotDis = new double[dicFrontPivot.Count];


            frontPivotDis[0] = dicFrontPivot[0].GetDisplace() - firstStandard;//数组存位移
            frontPivotDis[1] = dicFrontPivot[1].GetDisplace() - secondStandard;
          //  Dictionary<int, FrontPivot> dicFrontPivot = adamHelper.frontPivotDic;
            string sql = string.Format("insert into FrontPivotDis values(newid(),getdate(),'{0}',{1},{2})", name, frontPivotDis[0], frontPivotDis[1]);
            try
            {
                int r = dbhelper.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                throw new Exception("将前支点位移存入数据库报错：" + ex.Message);
            }
        }
        /// <summary>
        /// 指定时间间隔插入数据到数据库
        /// </summary>
        public void storeData()
        {
            try
            {
                InsertSteeveData();
                InsertAnchorData();
                InsertFrontPivotDis();
            }
            catch (Exception ex)
            {
                storeTimer.Change(Timeout.Infinite, Timeout.Infinite);
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 开始数据保存线程
        /// </summary>
        /// <param name="period"></param>
        public void StartTimer(int dueTime,int period)
        {
            storeTimer.Change(dueTime, period);
        }
        /// <summary>
        /// 停止数据接收线程
        /// </summary>
        public void StopTimer()
        {
            storeTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }
}
