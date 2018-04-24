using System;
using System.IO;

namespace XenoEngine.Utilities
{
#warning LogSystem: needs to be made multithreaded and also threadsafe
    //This class is written so that debug can performed 
    static public class LogSystem
    {
        private static FileStream m_sFileStream;
        private static StreamWriter m_sStreamWriter;

        static LogSystem()
        {
#if LOG
            AppDomain appDomain = AppDomain.CurrentDomain;
            appDomain.DomainUnload += new EventHandler(appDomain_DomainUnload);

            m_sFileStream = new FileStream("XenoLog.txt", FileMode.Truncate, FileAccess.Write);
            m_sStreamWriter = new StreamWriter(m_sFileStream);
#endif
        }

        private static void appDomain_DomainUnload(object sender, EventArgs e)
        {
#if LOG
            m_sStreamWriter.Dispose();
            m_sFileStream.Dispose();
#endif
        }

        public static void LogOnCondition(bool bCondition, string szMessage, bool bLogSourceData)
        {
#if LOG

            if (!bCondition)
            {
                WriteLog(szMessage, bLogSourceData);
            }
#endif
        }

        public static void Log(string szMessage, bool bLogSourceData)
        {
#if LOG
            WriteLog(szMessage, bLogSourceData);
#endif
        }

        public static void LogStackTrace(int? nFrameCount, string szMessage)
        {
#if LOG
            StringBuilder szBuilder = new StringBuilder();
            szBuilder.Append("Stack Trace");

            if (szMessage != null) szBuilder.Append(szMessage);
            
            WriteLog(szBuilder.ToString(), true);

            StackTrace(nFrameCount);
#endif
        }

        private static void StackTrace(int? nFrameCount)
        {
#if LOG
            StackTrace stackTrace = new StackTrace(true);
            StackFrame[] frames = stackTrace.GetFrames();
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("//===============================================================");

            //The index starts at to so that we only log where there error occurs and not these functions
            if (nFrameCount != null)
            {
                for (int nIndex = 2; nIndex < (nFrameCount + 2); ++nIndex )
                {
                    StackFrame frame = frames[nIndex];
                    stringBuilder.AppendLine(frame.GetFileName());
                    stringBuilder.AppendLine(frame.GetFileLineNumber().ToString());
                    stringBuilder.AppendLine(frame.GetMethod().ToString());
                    stringBuilder.AppendLine(frame.GetFileColumnNumber().ToString());
                    stringBuilder.AppendLine("******************************************************************");
                }
            }
            else
            {
                for (int nIndex = 2; nIndex < frames.Length; ++nIndex )
                {
                    StackFrame frame = frames[nIndex];
                    stringBuilder.AppendLine(frame.GetFileName());
                    stringBuilder.AppendLine(frame.GetFileLineNumber().ToString());
                    stringBuilder.AppendLine(frame.GetMethod().ToString());
                    stringBuilder.AppendLine(frame.GetFileColumnNumber().ToString());
                    stringBuilder.AppendLine("******************************************************************");
                }
            }

            stringBuilder.AppendLine("//=================================================================");

            ThreadPool.QueueUserWorkItem(WriteToFile, stringBuilder.ToString());
            //WriteToFile(stringBuilder.ToString());
            //TaskHandle handle = TaskManager.Instance.CreateTask<object>(stringBuilder.ToString(), WriteToFile, false);
            //TaskManager.Instance.ExecuteTask(handle);

#endif
            
        }

        private static void WriteLog(string szMessage, bool bLogSourceData)
        {
#if LOG
            StringBuilder szString = new StringBuilder();
            StackTrace stackTrace = new StackTrace(true);
            StackFrame stackFrame = stackTrace.GetFrame(2);

            if(bLogSourceData)
            {
                szString.AppendLine("????????????????????????????????????????????????????????????????????????????");
                szString.AppendLine(DateTime.Now.ToString());
                szString.Append("File: ");
                szString.AppendLine(stackFrame.GetFileName());
                szString.Append("Line: ");
                szString.AppendLine(stackFrame.GetFileLineNumber().ToString());
                szString.Append("Method: ");
                szString.AppendLine(stackFrame.GetMethod().ToString());
                szString.Append("Message: ");
                szString.AppendLine(szMessage);
                szString.AppendLine("????????????????????????????????????????????????????????????????????????????");
            }
            else
            {
                szString.AppendLine(szMessage);
            }

            ThreadPool.QueueUserWorkItem(WriteToFile, szString.ToString());
            /*WriteToFile(szString.ToString());*/
            //TaskHandle handle = TaskManager.Instance.CreateTask<object>(szString.ToString(), WriteToFile, false);
            //TaskManager.Instance.ExecuteTask(handle);
#endif
        }

        private static void WriteToFile(object data)
        {
#if LOG
            string szString = data as string;
            m_sStreamWriter.Write(szString);
            m_sStreamWriter.Flush();
#endif
        }
    }
}
