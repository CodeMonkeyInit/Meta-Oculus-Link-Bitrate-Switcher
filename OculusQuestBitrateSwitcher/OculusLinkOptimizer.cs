using System.ServiceProcess;
using Microsoft.Win32;

namespace OculusQuestBitrateSwitcher;
#pragma warning disable CA1416

public class OculusLinkOptimizer
{
    public void OptimizeOculusLink(OculusLinkMode linkMode)
    {
        using var oculusSettings = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Oculus\\RemoteHeadset", true);

        oculusSettings?.SetValue("BitrateMbps", linkMode == OculusLinkMode.AirLink ? 0 : 500);
        oculusSettings?.SetValue("DBR", linkMode == OculusLinkMode.AirLink ? 1 : 0);
        oculusSettings?.SetValue("HEVC", linkMode == OculusLinkMode.AirLink ? 1 : 0);

        RestartWindowsService("OVRService");
    }
    
    private void RestartWindowsService(string name)
        {
            ServiceController serviceController = new ServiceController(name);
            try
            {
                if(serviceController.Status is ServiceControllerStatus.Running or ServiceControllerStatus.StartPending)
                {
                    serviceController.Stop();
                }
    
                serviceController.WaitForStatus(ServiceControllerStatus.Stopped);
                serviceController.Start();
                serviceController.WaitForStatus(ServiceControllerStatus.Running);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
        }
}

#pragma warning restore CA1416
