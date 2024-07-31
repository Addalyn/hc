using System;
using System.Collections;
using System.IO;
using LobbyGameClientMessages;
using UnityEngine;

public class ClientCrashReportDetector : MonoBehaviour
{
    private ClientCrashReportThreadedJob m_threadedJob;
    private string m_crashDumpDirectoryPath;
    private static ClientCrashReportDetector s_instance;
    internal UIReportBugDialogBox m_crashDialog;

    internal static ClientCrashReportDetector Get()
    {
        return s_instance;
    }

    private void Start()
    {
        s_instance = this;
        try
        {
            DirectoryInfo parent = Directory.GetParent(Application.dataPath);
            string path = parent?.FullName ?? string.Empty;
            string[] directories = Directory.GetDirectories(path);

            foreach (string dir in directories)
            {
                if (Directory.GetFiles(dir, "crash.dmp").Length > 0)
                {
                    m_crashDumpDirectoryPath = dir;
                    break;
                }

                if (m_crashDumpDirectoryPath != null)
                {
                    break;
                }
            }

            if (m_crashDumpDirectoryPath != null)
            {
                Log.Warning("Detected crash dump directory: " + m_crashDumpDirectoryPath);
                if (UIDialogPopupManager.Ready)
                {
                    CreateFirstDialog();
                }
                else
                {
                    UIDialogPopupManager.OnReady += HandleUIDialogPopupManagerReady;
                }
            }
        }
        catch (Exception exception)
        {
            Log.Exception(exception);
        }
    }

    private void Update()
    {
        if (m_threadedJob != null)
        {
            m_threadedJob.Update();
        }
    }

    private void HandleUIDialogPopupManagerReady()
    {
        CreateFirstDialog();
    }

    private void CreateFirstDialog()
    {
        if (ClientMinSpecDetector.BelowMinSpecDetected)
        {
            UIDialogPopupManager.OpenOneButtonDialog(
                StringUtil.TR("RecoveredFromCrash", "Global"),
                StringUtil.TR("BelowMinimumSpec", "Global"),
                StringUtil.TR("Ok", "Global"));
            DeleteCrashDumpDirectory();
        }
        else
        {
            if (ClientGameManager.Get() != null && ClientGameManager.Get().IsConnectedToLobbyServer)
            {
                ClientGameManager.Get().SendStatusReport(
                    new ClientStatusReport
                    {
                        Status = ClientStatusReport.ClientStatusReportType.Crash,
                        StatusDetails = m_crashDumpDirectoryPath,
                        DeviceIdentifier = SystemInfo.deviceUniqueIdentifier
                    });
                m_crashDialog = UIDialogPopupManager.OpenReportBugDialog(
                    StringUtil.TR("RecoveredFromCrash", "Global"),
                    StringUtil.TR("UploadCrashReport", "Global"),
                    StringUtil.TR("Ok", "Global"),
                    StringUtil.TR("Cancel", "Global"),
                    HandleDialogOKButton,
                    HandleDialogCancelButton);
            }

            if (m_threadedJob == null)
            {
                string sessionToken = ClientGameManager.Get() != null && ClientGameManager.Get().SessionInfo != null
                    ? ClientGameManager.Get().SessionInfo.SessionToken.ToString()
                    : "unknown";
                m_threadedJob = new ClientCrashReportThreadedJob(
                    m_crashDumpDirectoryPath,
                    BugReportType.Crash,
                    $"SessionToken: {sessionToken}");
            }
        }
    }

    private void HandleDialogOKButton(UIDialogBox boxReference)
    {
        if (ClientGameManager.Get() != null && ClientGameManager.Get().IsConnectedToLobbyServer)
        {
            ClientGameManager.Get().SendStatusReport(
                new ClientStatusReport
                {
                    Status = ClientStatusReport.ClientStatusReportType.CrashUserMessage,
                    StatusDetails = m_crashDumpDirectoryPath,
                    DeviceIdentifier = SystemInfo.deviceUniqueIdentifier,
                    UserMessage = m_crashDialog.m_descriptionBoxInputField.text
                });
        }

        m_crashDialog = null;
    }

    private void HandleDialogCancelButton(UIDialogBox boxReference)
    {
        DeleteCrashDumpDirectory();
        m_crashDialog = null;
    }

    private void DeleteCrashDumpDirectory()
    {
        if (Directory.Exists(m_crashDumpDirectoryPath))
        {
            try
            {
                Directory.Delete(m_crashDumpDirectoryPath, true);
            }
            catch (Exception exception)
            {
                Log.Exception(exception);
            }
        }
    }

    private void OnDestroy()
    {
        if (m_threadedJob != null)
        {
            m_threadedJob.Cancel();
        }

        s_instance = null;
    }

    internal void UploadArchive(string crashServerAndArchiveURL, byte[] crashReportBytes, Action<bool> endEvent)
    {
        StartCoroutine(UploadArchiveCoroutine(crashServerAndArchiveURL, crashReportBytes, endEvent));
    }

    private IEnumerator UploadArchiveCoroutine(
        string crashServerAndArchiveURL,
        byte[] crashReportBytes,
        Action<bool> endEvent)
    {
        Log.Info(
            "Attempting to start WWW to post {0} crash report bytes to URL {1}",
            crashReportBytes.Length,
            crashServerAndArchiveURL);
        using (WWW client = new WWW(crashServerAndArchiveURL, crashReportBytes))
        {
            yield return client;
            if (string.IsNullOrEmpty(client.error))
            {
                Log.Info("\nResponse from Crash Service received was {0}", client.text ?? "NULL");
                endEvent(true);
            }
            else
            {
                Log.Error("\nError from Crash Service received was {0}", client.error);
                endEvent(false);
            }
        }
    }
}