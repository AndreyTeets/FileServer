﻿namespace FileServer;

public static class LogMessages
{
    public static readonly string UsingCertificate =
        "Using Certificate:" + Environment.NewLine +
        "{Certificate}";

    public static readonly string UsingSettings =
        "Using Settings:" + Environment.NewLine +
        "{Settings}";

    public static readonly string SettingsChanged =
        "Settings changed. New Settings:" + Environment.NewLine +
        "{Settings}";

    public static readonly string InvalidSettings =
        "Invalid Settings:" + Environment.NewLine +
        "{Settings}" + Environment.NewLine +
        "Problems:" + Environment.NewLine +
        "{Problems}";
}
