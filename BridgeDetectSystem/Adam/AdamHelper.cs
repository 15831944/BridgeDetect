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
        public List<double> standardlist { get;  }//四个基准点

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
                    ////读取模块192.168.1.3数据，吊杆力0，1，2，3与吊杆位移4，5，6，7
                    for (int j = 0; j < 4; j++)
                    {//
                        forceSensor = new Sensor(SensorType.forceSensor, 4, 20, 300, 1, 0);
                        tempDic.TryGetValue(j+4, out forceData);
                        forceSensor.readValue = double.Parse(forceData);
                        disSensor = new Sensor(SensorType.displaceSensor, 4, 20, 29.8, 100, 20);
                        tempDic.TryGetValue(j , out disData);
                        disSensor.readValue = double.Parse(disData);
                        Steeve steeves = new Steeve(j, forceSensor, disSensor);
                        steeveDic[steeves.id] = steeves;
                    }

                    ////代替循环--每个力传感器校正

                    ////steeveDic[0]
                    //forceSensor = new Sensor(SensorType.forceSensor, 4, 20, 300, 1, 0);//力1
                    //tempDic.TryGetValue(0, out forceData);
                    //forceSensor.readValue = double.Parse(forceData);
                    //disSensor = new Sensor(SensorType.displaceSensor, 4, 20, 29.8, 100, 20);
                    //tempDic.TryGetValue(4, out disData);
                    //disSensor.readValue = double.Parse(disData);
                    //Steeve steeve= new Steeve(0, forceSensor, disSensor);
                    //steeveDic[0] = steeve;
                    ////steeveDic[1]
                    //forceSensor = new Sensor(SensorType.forceSensor, 4, 20, 300, 1, 0);//力2
                    //tempDic.TryGetValue(1, out forceData);
                    //forceSensor.readValue = double.Parse(forceData);
                    //disSensor = new Sensor(SensorType.displaceSensor, 4, 20, 29.8, 100, 20);
                    //tempDic.TryGetValue(5, out disData);
                    //disSensor.readValue = double.Parse(disData);
                    //Steeve steeve1 = new Steeve(1, forceSensor, disSensor);
                    //steeveDic[1] = steeve1;

                    ////steeveDic[2]=
                    //forceSensor = new Sensor(SensorType.forceSensor, 4, 20, 300, 1, 0);//力3
                    //tempDic.TryGetValue(2, out forceData);
                    //forceSensor.readValue = double.Parse(forceData);
                    //disSensor = new Sensor(SensorType.displaceSensor, 4, 20, 29.8, 100, 20);
                    //tempDic.TryGetValue(6, out disData);
                    //disSensor.readValue = double.Parse(disData);
                    //Steeve steeve2 = new Steeve(2, forceSensor, disSensor);
                    //steeveDic[2] = steeve2;
                    ////steeveDic[3]=steeve3
                    //forceSensor = new Sensor(SensorType.forceSensor, 4, 20, 300, 1, 0);//力4
                    //tempDic.TryGetValue(3, out forceData);
                    //forceSensor.readValue = double.Parse(forceData);
                    //disSensor = new Sensor(SensorType.displaceSensor, 4, 20, 29.8, 100, 20);
                    //tempDic.TryGetValue(7, out disData);
                    //disSensor.readValue = double.Parse(disData);
                    //Steeve steeve3 = new Steeve(3, forceSensor, disSensor);
                    //steeveDic[3] = steeve3;
                    //for ( int n = 0; n < 4; i++)
                    //{
                    //    steeveCopy[n] = steeveDic[n];
                    //}
                    ////copy
                }
                
                
                else if (i == 1)
                {
                    int j = 0;
                    int count = 0;
                   
                    for (j = 0; j < 4; j++)//前支架位移
                    {
                        disSensor = new Sensor(SensorType.displaceSensor, 4, 20,3.8, 100,20);//传感器量程0.2-4 m；
                        tempDic.TryGetValue(j, out disData);
                        disSensor.readValue = double.Parse(disData);

                        FrontPivot pivot = new FrontPivot(j, disSensor);
                        frontPivotDic[pivot.id] = pivot;
                    }
                    for (j = 4; j < 8; j++)//锚杆力
                    {
                        forceSensor = new Sensor(SensorType.forceSensor, 4, 20, 300, 1, 0);
                        tempDic.TryGetValue(j, out forceData);
                        forceSensor.readValue = double.Parse(forceData);

                        Anchor anchor = new Anchor(j-4, forceSensor);
                        anchorDic[anchor.id] = anchor;
                    }
                    //count = 0;
                    //if (j == 6)
                    //{
                    //    disSensor = new Sensor(SensorType.displaceSensor, 4, 20, 30, 100);
                    //    tempDic.TryGetValue(j, out disData);
                    //    disSensor.readValue = double.Parse(disData);

                    //    RailWay railway = new RailWay(count++, disSensor);
                    //    //railWayDic[railway.id] = railway;
                    //}
                }
            }
        }

        /// <summary>
        /// 读取初始数值作为基准，只读一次。。。。？？？？？？
        /// </summary>
        public void ReadStandardValue()
        {
          
            List<double> disList = new List<double>();
            Sensor sensor = new Sensor(SensorType.displaceSensor, 4, 20, 29.8, 100,20);

            for (int i = 0; i < 4; i++)
            {
                sensor.readValue = double.Parse(adamList[0].Read(i));//???位移加+4？？
                standardlist.Add(sensor.GetRealValue());  //基准值
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
            //for (int i = 0; i < 4; i++)
            //{
            //    Sensor sensors = new Sensor(SensorType.displaceSensor,4,20,3.8,100,20);
            //    sensors.readValue = double.Parse(adamList[1].Read(i+4));
            //standards[i]=sensors.getrealvalue();
            //}
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
