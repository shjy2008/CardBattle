using Assets.Scripts.utility.Common;
using Assets.Scripts.utility.CommonInterface;
using Assets.Scripts.utility.Singleton;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.managers.timermgr
{
    public enum DelayType
    {
        Ms,
        Frame,
    }

    public class DelayTimeInfo
    {
        public DelayType delayType;
        public float delayMs;
        public int delayFrameNum;

        public DelayTimeInfo(DelayType _delayType, float _delayMs, int _delayFrameNum)
        {
            delayType = _delayType;
            delayMs = _delayMs;
            delayFrameNum = _delayFrameNum;
        }
    }

    public class TimerManager : TSingleton<TimerManager>, IManager
    {
        private bool delayLocker;
        private uint delayId;

        private List<CTuple4<uint, DelayTimeInfo, Action<object[]>, object[]>> dataList;//以毫秒作为延时基准/以帧数为延时基准
        private List<CTuple4<uint, DelayTimeInfo, Action<object[]>, object[]>> addDataList;//遍历list过程中增加cb会先存放这里
        private List<uint> removeDataList;//遍历list过程中移除cb会先存放这里

        private TimerManager() { }

        public void Init()
        {
            delayLocker = false;
            delayId = 0;
            dataList = new List<CTuple4<uint, DelayTimeInfo, Action<object[]>, object[]>>();
            addDataList = new List<CTuple4<uint, DelayTimeInfo, Action<object[]>, object[]>>();
            removeDataList = new List<uint>();
        }

        public void Update()
        {
            //先增加,后删除,最后遍历执行
            if (addDataList.Count > 0)
            {
                dataList.AddRange(addDataList);
                addDataList.Clear();
            }

            if (removeDataList.Count > 0)
            {
                for (int i = dataList.Count - 1; i >= 0; i--)
                {
                    if (removeDataList.Contains(dataList[i].arg1))
                    {
                        dataList.RemoveAt(i);
                    }
                }
                removeDataList.Clear();
            }

            delayLocker = true;
            for (int i = dataList.Count - 1; i >= 0; i--)
            {
                bool exec = false;
                //注意别用var data = dataList[i]-->这种写法因为是struct相当于new了一个复制操作.这样永远改不了原来的数据
                if (dataList[i].arg2.delayType == DelayType.Ms)
                {
                    exec = dataList[i].arg2.delayMs <= Time.time * 1000;
                }
                else
                {
                    dataList[i].arg2.delayFrameNum = dataList[i].arg2.delayFrameNum - 1;
                    exec = dataList[i].arg2.delayFrameNum <= 0;
                }
                if (exec)
                {
                    try
                    {
                        dataList[i].arg3(dataList[i].arg4);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                    dataList.RemoveAt(i);
                }
            }
            delayLocker = false;
        }

        public void Destroy()
        {
            dataList.Clear();
            addDataList.Clear();
            removeDataList.Clear();
        }

        public uint DelayExecByMs(float ms, Action<object[]> cb, params object[] param)
        {
            delayId += 1;
            var data = new CTuple4<uint, DelayTimeInfo, Action<object[]>, object[]>();
            data.arg1 = delayId;
            data.arg2 = new DelayTimeInfo(DelayType.Ms, ms + Time.time * 1000, 0);
            data.arg3 = cb;
            data.arg4 = param;
            if (delayLocker)
            {
                addDataList.Add(data);
            }
            else
            {
                dataList.Add(data);
            }
            return delayId;
        }

        public uint DelayExecByFrame(int framNum, Action<object[]> cb, params object[] param)
        {
            delayId += 1;
            var data = new CTuple4<uint, DelayTimeInfo, Action<object[]>, object[]>();
            data.arg1 = delayId;
            data.arg2 = new DelayTimeInfo(DelayType.Frame, 0, framNum);
            data.arg3 = cb;
            data.arg4 = param;
            if (delayLocker)
            {
                addDataList.Add(data);
            }
            else
            {
                dataList.Add(data);
            }
            return delayId;
        }

        public void CancelDelayExec(uint delayId)
        {
            if (delayLocker)
            {
                removeDataList.Add(delayId);
            }
            else
            {
                for (int i = dataList.Count - 1; i >= 0; i--)
                {
                    if (dataList[i].arg1 == delayId)
                    {
                        dataList.RemoveAt(i);
                        break;
                    }
                }
            }
        }
    }
}
