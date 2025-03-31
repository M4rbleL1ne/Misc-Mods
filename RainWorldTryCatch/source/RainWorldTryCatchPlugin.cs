using BepInEx;
using System;
using System.Security.Permissions;
using System.Security;
using System.IO;
using UnityEngine;
using BepInEx.Logging;

#pragma warning disable CS0618 // ignore false message
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace RainWorldTryCatch;

[BepInPlugin("lb-fgf-m4r-ik.rw-trycatch", nameof(RainWorldTryCatch), "10.0.0")]
public sealed class RainWorldTryCatchPlugin : BaseUnityPlugin
{
    public static bool LogToBep = File.Exists("logToBep.txt"), LogBaseEx = File.Exists("logBaseEx.txt");
    static ManualLogSource s_logger;

    public void OnEnable()
    {
        s_logger = Logger;
        On.RainWorld.Update += RainWorldUpdate;
    }

    static void RainWorldUpdate(On.RainWorld.orig_Update orig, RainWorld self)
    {
        try
        {
            orig(self);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            if (LogToBep)
                s_logger.LogError(ex);
            if (LogBaseEx && ex?.GetBaseException() is Exception e)
            {
                Debug.LogException(e);
                if (LogToBep) 
                    s_logger.LogError(e);
            }
        }
    }

    public void OnDisable() => s_logger = null!;
}