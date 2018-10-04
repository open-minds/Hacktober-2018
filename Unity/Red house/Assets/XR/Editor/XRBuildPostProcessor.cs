using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

public class XRBuildPostProcessor {
  [PostProcessBuild]
  public static void XcodeProjectSettings(BuildTarget buildTarget, string pathToBuiltProject) {
    if (buildTarget == BuildTarget.iOS) {
      Type pbxProjectType = Type.GetType("UnityEditor.iOS.Xcode.PBXProject, UnityEditor.iOS.Extensions.Xcode, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
      var proj = Activator.CreateInstance(pbxProjectType);
      MethodInfo readFromFileMethod = pbxProjectType.GetMethod("ReadFromFile");
      MethodInfo targetGuidByNameMethod = pbxProjectType.GetMethod("TargetGuidByName");
      MethodInfo addFrameworkToProjectMethod = pbxProjectType.GetMethod("AddFrameworkToProject");
      MethodInfo writeToFileMethod = pbxProjectType.GetMethod("WriteToFile");
      string projPath = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";
      readFromFileMethod.Invoke(proj, new[] {projPath});
      string unityTarget = (string) targetGuidByNameMethod.Invoke(proj, new[] {"Unity-iPhone"});
      addFrameworkToProjectMethod.Invoke(proj, new object[] {unityTarget, "Accelerate.framework", true});
      addFrameworkToProjectMethod.Invoke(proj, new object[] {unityTarget, "AVFoundation.framework", true});
      addFrameworkToProjectMethod.Invoke(proj, new object[] {unityTarget, "UIKit.framework", true});
      addFrameworkToProjectMethod.Invoke(proj, new object[] {unityTarget, "ARKit.framework", true});
      addFrameworkToProjectMethod.Invoke(proj, new object[] {unityTarget, "CoreVideo.framework", true});
      addFrameworkToProjectMethod.Invoke(proj, new object[] {unityTarget, "CoreMotion.framework", true});
      addFrameworkToProjectMethod.Invoke(proj, new object[] {unityTarget, "CoreGraphics.framework", true});
      addFrameworkToProjectMethod.Invoke(proj, new object[] {unityTarget, "CoreImage.framework", true});
      addFrameworkToProjectMethod.Invoke(proj, new object[] {unityTarget, "Metal.framework", true});
      addFrameworkToProjectMethod.Invoke(proj, new object[] {unityTarget, "CoreMedia.framework", true});
      addFrameworkToProjectMethod.Invoke(proj, new object[] {unityTarget, "OpenGLES.framework", true});
      writeToFileMethod.Invoke(proj, new[] {projPath});
    }
  }
}
