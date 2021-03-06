#load "common.cake"
#load "runhelpers.cake"

using System.Collections.Generic;

/// <summary>
///  Generate the scripts which target the OmniSharp binaries.
/// </summary>
/// <param name="outputRoot">The root folder where the publised (or installed) binaries are located</param>
void CreateRunScript(string outputRoot, string scriptFolder)
{
    CreateScript(outputRoot, scriptFolder, "net46");
    CreateScript(outputRoot, scriptFolder, "netcoreapp1.1");
}

private void CreateScript(string outputRoot, string scriptFolder, string framework)
{
    var scriptPath = GetScriptPath(scriptFolder, framework);
    var omniSharpPath = GetOmniSharpPath(outputRoot, framework);
    var content = GetScriptContent(omniSharpPath, framework);

    if (FileHelper.Exists(scriptPath))
    {
        FileHelper.Delete(scriptPath);
    }

    FileHelper.WriteAllLines(scriptPath, content);

    if (!Platform.Current.IsWindows)
    {
        Run("chmod", $"+x \"{scriptPath}\"");
    }
}

private string GetScriptPath(string scriptFolder, string framework)
{
    var result = CombinePaths(scriptFolder, "OmniSharp");

    if (IsCore(framework))
    {
        result += ".Core";
    }

    if (Platform.Current.IsWindows)
    {
        result += ".cmd";
    }

    return result;
}

private string GetOmniSharpPath(string outputRoot, string framework)
{
    var result = CombinePaths(PathHelper.GetFullPath(outputRoot), framework, "OmniSharp");

    if (!IsCore(framework))
    {
        result += ".exe";
    }

    return result;
}

private string[] GetScriptContent(string omniSharpPath, string framework)
{
    var lines = new List<string>();

    if (Platform.Current.IsWindows)
    {
        lines.Add("SETLOCAL");
    }
    else
    {
        lines.Add("#!/bin/bash");
    }

    lines.Add("");

    var arguments = Platform.Current.IsWindows
        ? "%*"
        : "\"$@\"";

    if (IsCore(framework) || Platform.Current.IsWindows)
    {
        lines.Add($"\"{omniSharpPath}\" {arguments}");
    }
    else // !isCore && !Platform.Current.IsWindows, i.e. Mono
    {
        lines.Add($"mono --assembly-loader=strict \"{omniSharpPath}\" {arguments}");
    }

    return lines.ToArray();
}

private bool IsCore(string framework)
{
    return framework.StartsWith("netcoreapp");
}