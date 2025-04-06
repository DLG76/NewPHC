using UnityEngine;
using System.Runtime.InteropServices;

public class DeviceCheck
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern int IsMobileDevice();
#endif

    public static bool IsMobile()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return IsMobileDevice() == 1;
#else
        return Application.isMobilePlatform;
#endif
    }
}