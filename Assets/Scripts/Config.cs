using FoldergeistAssets.Singleton;

public sealed class Config : SCSingletonMB<Config>
{
    public const string FirebaseURL = "https://monsexter.firebaseio.com/";
    public const int MaxPlayer = 6;

    public override void OnInstantiated()
    {

    }
}
