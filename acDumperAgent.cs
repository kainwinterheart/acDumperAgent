using System;
using Ini;
using acDumperAgentMain;
using unixtime;

namespace acDumperAgent
{
    public struct taskInfo
    {
        public string name;
        public bool active;
        public string lastRun;
        public string nextRun;
    }

    public class acDumperAgentClass
    {
        private IniFile agentConfig;
        private IniFile dumperConfig;
        private IniFile dumperTaskList;

        public string acDumperPath
        {
            get { return System.IO.Path.GetDirectoryName(dumperConfig.path); }
        }

        public static string PWD = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        private string CONFIG = System.IO.Path.Combine(PWD, "agent.conf");
        private string LOG = System.IO.Path.Combine(PWD, "agent.log");

        /* These values must be exactly the same as acDumper' definition */
        private string JOB_STATUS_ACTIVE = "active";
        private string CMD_KILL = "DIEPLZ";
        /* ************************************************************* */

        public bool gotConfig = false;

        // This is a rewriting of "bool acDumper::isItNow(string jobTime, unsigned int lastTime)" from acDumper
        private string parseTaskTime(string jobTime, long lastTime)
        {
	        string[] dateBits = jobTime.Split(' ');
	        if (dateBits.Length != 5) return "Unknown.";

            DateTime hrTime = DateTime.Now.ToUniversalTime();
            long startTime = (uint)(new UnixTime(hrTime)).Value;
            
	        bool[] approved = {false, false, false, false, false};
	        bool[] isInTestTime = {false, false, false, false};
            string[] monthNames = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

	        long timePassed = startTime - lastTime;
	        long testTime = 0;

            bool finalApproval = true;
            string result = "";

	        // Minute
	        string min = dateBits[0];
	        if (min == "*") approved[0] = true;
	        else {
		        if ( min.IndexOf('/') > -1 ) 
                {
			        min = min.Split('/')[1];
			        testTime += (uint.Parse(min) * 60);
			        isInTestTime[0] = true;
                } 
                else if (int.Parse(min) == hrTime.Minute) approved[0] = true;
	        }

	        // Hour
	        string hour = dateBits[1];
	        if (hour == "*") approved[1] = true;
	        else {
                if (hour.IndexOf('/') > -1)
                {
                    hour = hour.Split('/')[1];
                    testTime += (uint.Parse(hour) * 60 * 60);
			        isInTestTime[1] = true;
                }
                else if (int.Parse(hour) == hrTime.Hour) approved[1] = true;
	        }

	        // Day of month
	        string dom = dateBits[2];
	        if (dom == "*") approved[2] = true;
	        else {
                if (dom.IndexOf('/') > -1)
                {
                    dom = dom.Split('/')[1];
                    testTime += (uint.Parse(dom) * 24 * 60 * 60);
			        isInTestTime[2] = true;
                }
                else if (int.Parse(dom) == hrTime.Day) approved[2] = true;
	        }

	        // Month
	        string mon = dateBits[3];
	        if (mon == "*") approved[3] = true;
	        else {
                if (mon.IndexOf('/') > -1)
                {
                    mon = mon.Split('/')[1];
                    testTime += (uint.Parse(mon) * 30 * 24 * 60 * 60);
			        isInTestTime[3] = true;
                }
                else if (int.Parse(mon) == hrTime.Month) approved[3] = true;

	        }

	        // Day of week
	        string dow = dateBits[4];
	        if (dow == "*") approved[4] = true;
	        else if (dow == hrTime.ToString("ddd")) approved[4] = true;

            if (timePassed > testTime)
                for (int i = 0; i < 4; i++)
                    if (isInTestTime[i]) approved[i] = true;

            for (int i = 0; i < 5; i++) {
		        if (finalApproval) finalApproval = approved[i];
		        if (!finalApproval) break;
	        }

            if (finalApproval) result = "Right in the time.";
            else if (!approved[4]) result = "Next " + dow + ".";
            else if (testTime > 0)
            {
                for (int i = 3; i >= 0; i--)
                {
                    if ((!isInTestTime[i]) && (!approved[i]))
                    {
                        switch (i)
                        {
                            case 0: result = "While it will be " + min.ToString() + " minute(s) o'clock."; break;
                            case 1: result = "Only in " + hour.ToString() + " o'clock."; break;
                            case 2: result = "On " + dom.ToString() + " of current month."; break;
                            default: result = "Someday in " + monthNames[int.Parse(mon) - 1].ToLower() +"."; break;
                        }
                        break;
                    }
                }

                if (result == "")
                {
                    secondParser timeTeller = new secondParser(testTime - timePassed);

                    uint days = timeTeller.toDays;
                    uint hours = timeTeller.toHours;
                    uint minutes = timeTeller.toMinutes;

                    if (days > 0) result = "In " + days.ToString() + " days.";
                    else if (hours > 0) result = "In " + hours.ToString() + " hours.";
                    else if (minutes > 0) result = "In " + minutes.ToString() + " minutes.";
                    else result = "In " + (testTime - timePassed).ToString() + " seconds.";
                }
            }

            return result;
        }

        public bool isDumperRunning()
        {
            foreach (System.Diagnostics.Process clsProcess in System.Diagnostics.Process.GetProcesses())
                if (clsProcess.ProcessName == "acDumper") return true;

            return false;
        }

        public void killDumper()
        {
            string connFileName = dumperConfig.getStringValue("win32", "connectionFile");

            System.Text.RegularExpressions.Regex something =
                new System.Text.RegularExpressions.Regex("\\\\",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase |
                    System.Text.RegularExpressions.RegexOptions.Compiled);

            if (!something.Match(connFileName).Success)
                connFileName = System.IO.Path.Combine(acDumperPath, connFileName);

            // And now hope that this will be done really fast!
            System.IO.TextWriter connFile = new System.IO.StreamWriter(connFileName, true);
            connFile.WriteLine(CMD_KILL);
            connFile.Close();
            isDumperRunning();
        }

        public void startDumper()
        {
            string dumperExeName = System.IO.Path.Combine(acDumperPath, "acDumper.exe");
            /*System.Diagnostics.ProcessStartInfo si =
                new System.Diagnostics.ProcessStartInfo(dumperExeName);
            //si.CreateNoWindow = true;
            si.RedirectStandardError = false;
            si.RedirectStandardOutput = false;
            si.RedirectStandardInput = false;
            si.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            //si.UseShellExecute = false;
            si.WorkingDirectory = acDumperPath;
            System.Diagnostics.Process.Start(si);*/

            System.Diagnostics.Process p = null;
            try
            {
                p = new System.Diagnostics.Process();
                p.StartInfo.WorkingDirectory = acDumperPath;
                p.StartInfo.FileName = "acDumper.exe";

                p.StartInfo.Arguments = string.Format("Console application");
                p.StartInfo.CreateNoWindow = false;
                p.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                p.Start();
                //p.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception Occurred :{0},{1}",
                          ex.Message, ex.StackTrace.ToString());
            }
        }
        
        public acDumperAgentClass()
        {
            if (System.IO.File.Exists(CONFIG))
            {
                agentConfig = new IniFile(CONFIG);

                string dumperConfigFile = agentConfig.getStringValue("main", "dumperConfig");

                if (System.IO.File.Exists(dumperConfigFile))
                {
                    dumperConfig = new IniFile(dumperConfigFile);
                    string dumperTaskListFile = System.IO.Path.GetDirectoryName(dumperConfigFile);
                    dumperTaskListFile = System.IO.Path.Combine(dumperTaskListFile, "acDumperTasks.conf");

                    if (System.IO.File.Exists(dumperTaskListFile))
                    {
                        dumperTaskList = new IniFile(dumperTaskListFile);
                        gotConfig = true;
                    }
                }
            }
        }

        public string[] getRunningTasks()
        {
            System.Collections.Generic.List<string> taskList = dumperTaskList.getSectionNames();
            string runningTasks = "";

            for (int i = 0; i < taskList.Count; i++)
            {
                string taskStatus = dumperTaskList.getStringValue(taskList[i], "status");
                if (taskStatus == JOB_STATUS_ACTIVE) runningTasks += taskList[i] + '\0';
            }

            if (runningTasks != "")
                return runningTasks.Substring(0, runningTasks.Length - 1).Split('\0');
            else
                return (new string[0]);
        }

        public taskInfo[] getTasks() 
        {
            System.Collections.Generic.List<string> taskList = dumperTaskList.getSectionNames();
            taskInfo[] structTaskList = new taskInfo[taskList.Count];

            for (int i = 0; i < taskList.Count; i++)
            {
                taskInfo task;
                task.name = taskList[i];

                string taskStatus = dumperTaskList.getStringValue(task.name, "status");
                task.active = (taskStatus == JOB_STATUS_ACTIVE);

                if (task.active) task.lastRun = (isDumperRunning() ? "Currently running." : "Bad config, you need to remove \"status\" field from this task.");
                else task.lastRun = ((taskStatus == "") ? (new UnixTime(0)).ToString() : (new UnixTime(long.Parse(taskStatus))).ToString());

                task.nextRun = parseTaskTime(dumperTaskList.getJobTimeValue(taskList[i], "jobtime"),
                    (task.active ? (long)(new UnixTime()).Value : ((taskStatus == "") ? 0 : long.Parse(taskStatus))));

                structTaskList[i] = task;
            }

            return structTaskList;
        }

        public int getNumRunningTasks()
        {
            return getRunningTasks().Length;
        }

        public int getRefreshRate()
        {
            int refreshRate = agentConfig.getIntValue("main", "refreshRate");
            return ((refreshRate == 0) ? 1 : refreshRate);
        }

        public void log(string msg)
        {
            System.IO.TextWriter logFile = new System.IO.StreamWriter(LOG, true);
            logFile.WriteLine(DateTime.Now + ": " + msg);
            logFile.Close();
        }
    }
}