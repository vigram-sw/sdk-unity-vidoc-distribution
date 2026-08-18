#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

public static class IOSPostProcessBuild
{
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string pathToBuiltProject)
    {
        if (buildTarget != BuildTarget.iOS)
            return;

        string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");

        PlistDocument plist = new PlistDocument();
        plist.ReadFromFile(plistPath);

        PlistElementDict rootDict = plist.root;

        rootDict.SetBoolean("UIFileSharingEnabled", true);
        rootDict.SetBoolean("LSSupportsOpeningDocumentsInPlace", true);
        rootDict.SetString("NSBluetoothPeripheralUsageDescription","Needed to work with viGram");
        rootDict.SetString("NSBluetoothAlwaysUsageDescription","Needed to work with viGram");

        plist.WriteToFile(plistPath);
    }
}
#endif
