﻿using BridgeDetectSystem.dao;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BridgeDetectSystem.service
{
   public class OperateSql
    {
       
       
        private static volatile OperateSql instance;
     

        public static OperateSql GetInstance()
        {
            if (instance == null)
            {
                throw new Exception("实例未初始化。");
            }
            return instance;
        }
        /// <summary>
        /// 加载数据库中的数据，显示
        /// </summary>
        /// <param name="dgv">datagridview控件</param>
        public static void LoadData(DataGridView dgv)                 
        {
            string sql = "select * from SteeveDisplacement";
            DBHelper dbhelper = DBHelper.GetInstance();
            DataTable dt = dbhelper.ExecuteSqlDataAdapter(sql, null, 0);
            dgv.DataSource = dt;
            dgv.AutoGenerateColumns =false;
            dgv.Invalidate();
        }
        /// <summary>
        /// 插入一万条数据
        /// </summary>
        public static void InsertData()
        {
            int n = 10000;
            int r = -1;
            string insertSql = "insert into test2 values(newid(),1.01,2.0,3,4,5,6,7,8,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,'操作员',getdate())";
            DBHelper dbheler = DBHelper.GetInstance();
            while (n > 0)
            {
                r = dbheler.ExecuteNonQuery(insertSql, null);
                n = n - 1;
            }
            if (r > 0)
            {
                MessageBox.Show("插入一万条数据成功");
            }
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        public static void DeleteData()
        {
            string sql = "delete  from test2";
            DBHelper dbheler = DBHelper.GetInstance();
            int r = dbheler.ExecuteNonQuery(sql, null);
            if (r > 0)
            {
                MessageBox.Show("删除成功");
            }
        }
        
        

    }
}
