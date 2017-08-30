using BridgeDetectSystem.util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BridgeDetectSystem.service
{
   public static class OperateSql
    {   
        /// <summary>
        /// 加载数据表
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="dgv">控件datagridview</param>
        public static DataTable LoadData(string sql ,DataGridView dgv)
        {
            DBHelper dbhelper = DBHelper.GetInstance();
            DataTable dt = dbhelper.ExecuteSqlDataAdapter(sql, null, 0);
            dgv.DataSource = dt;
            OperateSql.RemoveNULL(dgv);
            dgv.AutoGenerateColumns = false;
            dgv.Invalidate();
            return dt;
        }     
        /// <summary>
        /// 空列视为不可见
        /// </summary>
        /// <param name="dgv"></param>
        public static void RemoveNULL(DataGridView dgv)
        {
            for (int i = 0; i < dgv.Columns.Count; i++)
            {
                if (dgv.Rows[0].Cells[i].Value == System.DBNull.Value)
                {
                    dgv.Columns[i].Visible = false;
                }
            }
        }
     
       
    
        
        

    }
}
