using System;
using System.IO;
using System.Net;
using System.Text;

public static class CrashReportSender
{
    private const string URL = "http://debug.triongames.com/";

    internal static bool Send(string crashReportFilePath)
    {
        try
        {
            Log.Info("Attempting to build URL to send crash report at path {0}", crashReportFilePath);
            string fileName = Path.GetFileName(crashReportFilePath);
            string str = URL + "v2/archive/" + fileName;
            Log.Info("Attempting to start WebClient to send crash report to URL {0}", str);
            using (WebClient webClient = new WebClient())
            {
                webClient.Headers.Add("User-Agent", "TrionHTTP/1.0");
                webClient.Headers.Add("Content-Type", "application/zip");
                Log.Info("Attempting to read file bytes for crash report from {0}", crashReportFilePath);
                byte[] reportBytes = File.ReadAllBytes(crashReportFilePath);
                Log.Info(
                    "Attempting to upload {0} bytes to crash report URL {1}",
                    reportBytes == null ? "NULL" : reportBytes.Length.ToString(),
                    str);
                byte[] response = webClient.UploadData(str, "POST", reportBytes);
                Log.Info("\nResponse from Crash Service received was {0}", Encoding.ASCII.GetString(response));
                return true;
            }
        }
        catch (WebException ex)
        {
            string arg = string.Empty;
            if (ex != null && ex.Response != null)
            {
                using (Stream responseStream = ex.Response.GetResponseStream())
                {
                    if (responseStream != null)
                    {
                        using (StreamReader streamReader = new StreamReader(responseStream))
                        {
                            arg = streamReader.ReadToEnd();
                        }
                    }
                }
            }

            Log.Error($"{ex}, status {ex.Status.ToString()}, response: {arg}");
            return false;
        }
        catch (Exception exception)
        {
            Log.Exception(exception);
            return false;
        }
    }
}