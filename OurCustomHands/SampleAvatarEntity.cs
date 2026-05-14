using Oculus.Avatar2;
using UnityEngine.Events;

public class SampleAvatarEntity : OvrAvatarEntity
{
    public readonly UnityEvent<OvrAvatarEntity> OnDefaultAvatarLoadedEvent = new();
    public readonly UnityEvent<OvrAvatarEntity> OnUserAvatarLoadedEvent = new();

    public ulong _userId { get; private set; }

    public void LoadRemoteUserCdnAvatar(ulong userId)
    {
        _userId = userId;

        // Newer SDKs usually load through OvrAvatarEntity APIs here.
        // If your SDK has LoadUser(ulong), use:
        LoadUser(userId);
    }

    protected override void OnUserAvatarLoaded()
    {
        base.OnUserAvatarLoaded();
        OnUserAvatarLoadedEvent.Invoke(this);
    }

    protected override void OnDefaultAvatarLoaded()
    {
        base.OnDefaultAvatarLoaded();
        OnDefaultAvatarLoadedEvent.Invoke(this);
    }
}