using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;

class ProcessorInfo {
    public ProcessorInfo()
    {
        ManagementObjectSearcher mosProcessor = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
        int i = 0;
        foreach (ManagementObject m in mosProcessor.Get())
        {
            if (m["Name"] != null)
            {
                CPUName = m["Name"].ToString().Trim();
            }
            if (m["NumberOfCores"] != null)
            {
                Cores = m["NumberOfCores"].ToString().Trim();
            }
            if (m["NumberOfLogicalProcessors"] != null)
            {
                Threads = m["NumberOfLogicalProcessors"].ToString().Trim();
            }
            if (m["CurrentClockSpeed"] != null)
            {
                MHz = m["CurrentClockSpeed"].ToString().Trim();
            }
        }
        if (i > 1)
        {
            Threads += " (Note: " + i + " processors in system)";
        }
    }

    public static ProcessorInfo sharedInstance = new ProcessorInfo();

    public String CPUName = "unknown";
    public String Cores = "?";
    public String Threads = "?";
    public String MHz = "????";
}
