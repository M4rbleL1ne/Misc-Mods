using BepInEx;
using System;
using System.Security.Permissions;
using System.Security;
using System.IO;
using UnityEngine;

#pragma warning disable CS0618 // ignore false message
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace RainWorldTryCatch;

[BepInPlugin("lb-fgf-m4r-ik.rw-trycatch", nameof(RainWorldTryCatch), "1.0.3")]
public class RainWorldTryCatchPlugin : BaseUnityPlugin
{
    public static readonly bool logToBep = File.Exists("logToBep.txt"), logBaseEx = File.Exists("logBaseEx.txt");

    public void OnEnable() => On.RainWorld.Update += RainWorldUpdate;

    void RainWorldUpdate(On.RainWorld.orig_Update orig, RainWorld self)
    {
        try
        {
            orig(self);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            if (logToBep)
                Logger.LogError(ex);
            if (logBaseEx && ex?.GetBaseException() is Exception e)
            {
                Debug.LogException(e);
                if (logToBep) 
                    Logger.LogError(e);
            }
        }
    }
}