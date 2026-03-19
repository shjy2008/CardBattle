using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FrameWork
{
    public class MainLoop : MonoBehaviour
    {
        private string lastLogString = "";
        private string lastLogStackTrace = "";
        private LogType lastLogType = LogType.Log;


        void Awake()
        {


        }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void HandleMessage(string logString, string logStackTrace, LogType logType)
        {
            if (string.IsNullOrEmpty(logString) && string.IsNullOrEmpty(logStackTrace))
                return;

            if (lastLogString.Equals(logString) && lastLogStackTrace.Equals(logStackTrace) && lastLogType == logType)
                return;

            if (logType == LogType.Log || logType == LogType.Warning)
                return;

            lastLogString = logString;
            lastLogStackTrace = logStackTrace;
            lastLogType = logType;

            string tempLog = "";
            switch (logType)
            {
                case LogType.Assert:
                    tempLog = string.Format("Assert: {0}\nstack trace: {1}", logString, logStackTrace);
                    break;
                case LogType.Error:
                    tempLog = string.Format("Error: {0}\nstack trace: {1}", logString, logStackTrace);
                    break;
                case LogType.Exception:
                    tempLog = string.Format("deviceName: {0}\ndeviceModel: {1}\noperatingSystem:{2}\ngraphicsDeviceName: {3}\n", SystemInfo.deviceName, SystemInfo.deviceModel, SystemInfo.operatingSystem, SystemInfo.graphicsDeviceName);
                    tempLog = string.Format("device: {0}\nException: {1}\nstack trace: {2}", tempLog, logString, logStackTrace);
                    break;
                default:
                    tempLog = string.Format("default: {0}\nstack trace: {1}", logString, logStackTrace);
                    break;
            }

            if (!string.IsNullOrEmpty(tempLog))
            {
#if !UNITY_EDITOR
                
                //ServerDataMgr.Instance.OnErrorStackTraceRequest(tempLog);
#endif
            }
        }

        private void Update()
        {
            //GameMgr.Instance.Update();
            //SceneMgr.Instance.Update();
        }

    }
}

