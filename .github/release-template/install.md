## Install

Download the attached `.nupkg`, place it in a local package folder, then add that folder as a NuGet source:

```powershell
dotnet nuget add source <folder> --name WKOpenVRFaceSdk
```

Reference `WKOpenVR.FaceTracking.Sdk` from module projects using the release package version.
