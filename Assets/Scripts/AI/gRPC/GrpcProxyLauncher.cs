using System.Diagnostics;
using System.IO;
using UnityEngine;

public static class GrpcProxyLauncher
{
    private static Process? _process;

    public static void LaunchProxyIfNeeded()
    {
        if (_process != null && !_process.HasExited)
        {
            UnityEngine.Debug.Log("[GrpcProxyLauncher] Proxy already running.");
            return;
        }

        var proxyPath = Path.Combine(Application.streamingAssetsPath, "GrpcHost.exe");

        if (!File.Exists(proxyPath))
        {
            UnityEngine.Debug.LogError($"[GrpcProxyLauncher] Proxy not found at {proxyPath}");
            return;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = proxyPath,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        try
        {
            _process = Process.Start(startInfo);
            UnityEngine.Debug.Log("[GrpcProxyLauncher] Proxy started.");
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"[GrpcProxyLauncher] Failed to start proxy: {e.Message}");
        }
    }

    public static void KillProxy()
    {
        if (_process != null && !_process.HasExited)
        {
            _process.Kill();
            _process = null;
            UnityEngine.Debug.Log("[GrpcProxyLauncher] Proxy killed.");
        }
    }
}
