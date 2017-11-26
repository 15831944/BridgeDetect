using BridgeDetectSystem.service;
using System;
using System.Collections.Generic;
using System.Threading;
using BridgeDetectSystem.entity;


namespace BridgeDetectSystem.adam
{
    public class AdamHelper
    {
        #region  字段

        private List<AdamOperation> adamList;
        private Dictionary<int, Dictionary<int, string>> allDataDic;

        public volatile bool hasData;
        public System.Threading.Timer readTimer { get; set; }
        //吊杆力和位移
        public Dictionary<int, Steeve> steeveDic { get; }
        public Dictionary <int,Steeve> steeveCopy { get; set; }
        //锚杆力
        public Dictionary<int, Anchor> anchorDic { get; set; }
        //前支点位移
        public Dictionary<int, FrontPivot> frontPivotDic { get; }

        //吊杆基准点
        public List<double> standardlist { get; set; }//四个基准点

        public double steeveDisStandard { get; set; }
        //前支架基准点
        public double first_frontPivotDisStandard { get; set; }
        public double second_frontPivotDisStandard { get; set; }
        public double three_standard { get; set; }
        public double four_standard { get; set; }
        public double steevedisdifflimit;
        #endregion

        #region 单例
        private static volatile AdamHelper instance = null;
        private AdamHelper(List<AdamOperation> list)
        {
            this.adamList = list;
            this.allDataDic = new Dictionary<int, Dictionary<int, string>>();
            this.steeveDic = new Dictionary<int, Steeve>();
            this.anchorDic = new Dictionary<int, Anchor>();
            this.frontPivotDic = new Dictionary<int, FrontPivot>();
            this.standardlist = new List<double>();
            this.hasData = false;
            try
            {
                //初始化每个研华模块
                foreach (AdamOperation oper in list)
                {
                    oper.Init();
                }
            }
            catch (AdamException ex)
            {
                throw ex;
            }

            //后台每隔一段时间读取一次数据
            this.readTimer = new Timer(_ =>
            {
                try
                {
                    ReadAllReadableValues();
                }
                catch (Exception ex)
                {
                    readTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    hasData = false;
                    //throw ex;
                    System.Windows.Forms.MessageBox.Show("研华模块掉线，请检查硬件连接，确保连接完全正确，重新启动软件");
                }
            }, null, Timeout.Infinite,Timeout.Infinite);
        }


        public static AdamHelper Initialize(List<AdamOperation> list)
        {
            if (instance != null)
            {
                throw new AdamHelperException("AdamHelper数据接收模块重复初始化报错");
            }
            instance = new AdamHelper(list);//
            return instance;
        }

        public static AdamHelper GetInstance()
        {
            if (instance == null)
            {
                throw new AdamHelperException("AdamHelper数据接收模块未初始化，实例不存在报错！");
            }
            return instance;
        }

        #endregion

        #region 方法
        private static readonly object obj = new object(); //锁对象
        private void ReadAllReadableValues()
        {
            lock (obj)
            {
                foreach (AdamOperation oper in adamList)
                {
                    allDataDic[oper.id] = oper.Read();//读取所有电流值
                }

                ConvertToRealValue();
                hasData = true;
            }
        }

        /// <summary>
        /// 增加模块，只需要改变这个方法中的算法就行了。
        /// </summary>
        private void ConvertToRealValue()
        {
            Dictionary<int, string> tempDic;
            string forceData;
            string disData;
            Sensor forceSensor;
            Sensor disSensor;

            for (int i = 0; i < adamList.Count; i++)
            {
                allDataDic.TryGetValue(adamList[i].id, out tempDic);

                if (i == 0)
                {
                    ////读取模块192.168.1.3数据，吊杆位移0，1，2，3与吊杆力4，5，6，7
                    //for (int j = 0; j < 4; j++)
                    //{//
                    //    forceSensor = new Sensor(SensorType.forceSensor, 4, 20, 300, 1, 0);
                    //    tempDic.TryGetValue(j + 4, out forceData);
                    //    forceSensor.readValue = double.Parse(forceData);
                    //    disSensor = new Sensor(SensorType.displaceSensor, 4, 20, 29.8, 100, 20);
                    //    tempDic.TryGetValue(j, out disData);
                    //    disSensor.readValue = double.Parse(disData);
                    //    Steeve steeves = new Steeve(j, forceSensor, disSensor);

                    //    steeveDic[steeves.id] = steeves;
                    //}

                    ////代替循环--每个力传感器校正
                    //  j = 0;
                    // List<Sensor> sss = new List<Sensor>();
                    //List<Sensor> ds = new List<Sensor>();
                    //var fs1 = new Sensor(SensorType.forceSensor, 4, 20, 300, 1, 0);
                    //var fs2 = new Sensor(SensorType.forceSensor, 4, 20, 300, 1, 0);
                    //var fs3 = new Sensor(SensorType.forceSensor, 4, 20, 300, 1, 0);
                    //var fs4 = new Sensor(SensorType.forceSensor, 4, 20, 300, 1, 0);
                    //var ds1 = new Sensor(SensorType.displaceSensor, 4, 20, 29.9, 100, 20);
                    //var ds2 = new Sensor(SensorType.displaceSensor, 4, 20, 29.9, 100, 20);
                    //var ds3 = new Sensor(SensorType.displaceSensor, 4, 20, 29.9, 100, 20);
                    //var ds4 = new Sensor(SensorType.displaceSensor, 4, 20, 29.9, 100, 20);
                    //sss.Add(fs1);
                    //sss.Add(fs2);
                    //sss.Add(fs3);
                    //sss.Add(fs4);
                    //ds.Add(ds1);
                    //ds.Add(ds2);
                    //ds.Add(ds3);
                    //ds.Add(ds4);
                    //for (int k = 0; k < 4; k++)
                    //{ tempDic.TryGetValue(k + 4, out forceData);
                    //    tempDic.TryGetValue(k, out disData);
                    //    ds[k].readValue = double.Parse(disData);
                    //    sss[k].readValue = double.Parse(forceData);
                    //}
                    
                    //j=0
                    forceSensor = new Sensor(SensorType.forceSensor, 4, 20, 300, 1, 0);
                    tempDic.TryGetValue(4, out forceData);
                    forceSensor.readValue = double.Parse(forceData);
                    disSensor = new Sensor(SensorType.displaceSensor, 4, 20, 29.8, 100, 20);
                    tempDic.TryGetValue(0, out disData);
                    disSensor.readValue = double.Parse(disData);
                    Steeve steeve0 = new Steeve(0, forceSensor, disSensor);
                    steeveDic[steeve0.id] = steeve0;
                    //j=1
                    forceSensor = new Sensor(SensorType.forceSensor, 4, 20, 300, 1, 0);
                    tempDic.TryGetValue(5, out forceData);
                    forceSensor.readValue = double.Parse(forceData);
                    disSensor = new Sensor(SensorType.displaceSensor, 4, 20, 29.8, 100, 20);
                    tempDic.TryGetValue(1, out disData);
                    disSensor.readValue = double.Parse(disData);
                    Steeve steeve1 = new Steeve(1, forceSensor, disSensor);
                    steeveDic[steeve1.id] = steeve1;
                    //j=2
                    forceSensor = new Sensor(SensorType.forceSensor, 4, 20, 300, 1, 0);
                    tempDic.TryGetValue(6, out forceData);
                    forceSensor.readValue = double.Parse(forceData);
                    disSensor = new Sensor(SensorType.displaceSensor, 4, 20, 29.8, 100, 20);
                    tempDic.TryGetValue(2, out disData);
                    disSensor.readValue = double.Parse(disData);
                    Steeve steeve2 = new Steeve(2, forceSensor, disSensor);
                    steeveDic[steeve2.id] = steeve2;
                    //j=3
                    forceSensor = new Sensor(SensorType.forceSensor, 4, 20, 300, 1, 0);
                    tempDic.TryGetValue(7, out forceData);
                    forceSensor.readValue = double.Parse(forceData);
                    disSensor = new Sensor(SensorType.displaceSensor, 4, 20, 29.8, 100, 20);
                    tempDic.TryGetValue(3, out disData);
                    disSensor.readValue = double.Parse(disData);
                    Steeve steeve3= new Steeve(3, forceSensor, disSensor);
                    steeveDic[steeve3.id] = steeve3;

                }
                
                
                else if (i == 1)
                {
                    int j = 0;
                   // int count = 0;
                   
                    for (j = 0; j < 4; j++)//前支架位移
                    {
                        disSensor = new Sensor(SensorType.displaceSensor, 4, 20,3.8, 100,20);//传感器量程0.2-4 m；
                        tempDic.TryGetValue(j, out disData);
                        disSensor.readValue = double.Parse(disData);

                        FrontPivot pivot = new FrontPivot(j, disSensor);
                        frontPivotDic[pivot.id] = pivot;
                    }
                    #region 锚杆力，接收但不显示，不保存，不后台报警
                    //  for (j = 4; j < 8; j++)//锚杆力

                    forceSensor = new Sensor(SensorType.forceSensor,4 , 20, 300, 1, 0);
                        tempDic.TryGetValue(4, out forceData);
                        forceSensor.readValue = double.Parse(forceData);

                        Anchor anchor0 = new Anchor(0, forceSensor);
                        anchorDic[anchor0.id] = anchor0;

                    forceSensor = new Sensor(SensorType.forceSensor, 4, 20, 300, 1, 0);
                    tempDic.TryGetValue(5, out forceData);
                    forceSensor.readValue = double.Parse(forceData);

                    Anchor anchor1 = new Anchor(1, forceSensor);
                    anchorDic[anchor1.id] = anchor1;

                    forceSensor = new Sensor(SensorType.forceSensor, 4, 20, 300, 1, 0);
                    tempDic.TryGetValue(6, out forceData);
                    forceSensor.readValue = double.Parse(forceData);
                    Anchor anchor2 = new Anchor(2, forceSensor);
                    anchorDic[anchor2.id] = anchor2;

                    forceSensor = new Sensor(SensorType.forceSensor, 4, 20, 300, 1, 0);
                    tempDic.TryGetValue(7, out forceData);
                    forceSensor.readValue = double.Parse(forceData);

                    Anchor anchor3 = new Anchor(3, forceSensor);
                    anchorDic[anchor3.id] = anchor3;
                    #endregion                
                }
            }
        }

        /// <summary>
        /// 读取初始数值作为基准，只读一次。。。。？？？？？？
        /// </summary>
        public void ReadStandardValue()
        {
          
            List<double> disList = new List<double>();
            standardlist = new List<double>();
            Sensor sensor = new Sensor(SensorType.displaceSensor, 4, 20, 29.8, 100,20);

            for (int i = 0; i < 4; i++)
            {
                sensor.readValue = double.Parse(adamList[0].Read(i));//???位移加+4？？
                standardlist.Add(sensor.GetRealValue());  //每个吊杆的基准值
                disList.Add(sensor.GetRealValue());
            }
          
            double sum = 0;
            foreach (double val in disList)
            {
                sum += val;
            }
         
            //平均值作基准值
            steeveDisStandard =Math.Round(sum / disList.Count, 3);


            //前支点基准值
            
            Sensor sensor1 = new Sensor(SensorType.displaceSensor, 4, 20, 3.8, 100,20);
            sensor1.readValue = double.Parse(adamList[1].Read(0));
            first_frontPivotDisStandard = sensor1.GetRealValue();

            Sensor sensor2 = new Sensor(SensorType.displaceSensor, 4, 20, 3.8, 100,20);
            sensor2.readValue = double.Parse(adamList[1].Read(1));
            second_frontPivotDisStandard = sensor2.GetRealValue();

            Sensor sensor3 = new Sensor(SensorType.displaceSensor, 4, 20, 3.8, 100, 20);
            sensor3.readValue = double.Parse(adamList[1].Read(2));
            three_standard = sensor3.GetRealValue();

            Sensor sensor4 = new Sensor(SensorType.displaceSensor, 4, 20, 3.8, 100, 20);
            sensor4.readValue = double.Parse(adamList[1].Read(3));
            four_standard = sensor4.GetRealValue();
           
        }

        /// <summary>
        /// 取消后台数据接收线程
        /// </summary>
        public void StopTimer()
        {
            readTimer.Change(Timeout.Infinite, Timeout.Infinite);
            hasData = false;
        }
        /// <summary>
        /// 开始后台接收数据线程
        /// </summary>
        /// <param name="period"></param>
        public void StartTimer(int period)
        {
            //读取初始的值一次，并记录在字段steeveDisStandard、frontPivotDisStandard中
            //之后作为报警的基准
            ReadStandardValue();
            readTimer.Change(0, period);
        }

        #endregion
    }
    public class AdamHelperException : AdamException
    {
        public AdamHelperException(string message) : base(message) { }
        public AdamHelperException(string message, Exception innerException) : base(message, innerException) { }
    }


}
