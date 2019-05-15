using FoldergeistAssets.Singleton;
using System;

public sealed class Config : SCSingletonMB<Config>
{
    public const string FirebaseURL = "https://monsexter-3b9e3.firebaseio.com/";
    public const int MaxPlayer = 6;
    public const int TimeCapture = 6;
    public readonly long TrickFromTimeCapture = TimeSpan.FromMinutes(6).Ticks;

    public override void OnInstantiated()
    {

    }
}
