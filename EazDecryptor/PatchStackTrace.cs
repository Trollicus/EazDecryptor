using System.Diagnostics;
using System.Reflection;
using HarmonyLib;

namespace EazDecryptor;

[HarmonyPatch] 
public class PatchStackTrace
{
    [HarmonyPatch(typeof(StackFrame), "GetMethod")]
    [HarmonyPostfix] 
    public static void Postfix(ref MethodBase? __result)
    {
        if (__result?.DeclaringType == typeof(RuntimeMethodHandle))
        {
            __result = MethodBase.GetCurrentMethod();
        }
    }
}