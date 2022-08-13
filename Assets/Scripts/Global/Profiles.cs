using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ExplosionProfile
{
    public string name;
    public ExplosionsEffects effect;
    public GameObject explosionObj;
}

public abstract class Profile
{
    public string name;
    [TextArea(minLines:1, maxLines:6)] public string describtion;
    public Texture2D icon;
}

[System.Serializable]
public class PerkProfile : Profile
{
    public PerkType type;
}

[System.Serializable]
public class ProjectileEffectProfile : Profile
{
    public string keyTipText;
    public Effect effect;

    public bool IsNull() => ProjectileEffectProfile.IsNull(this);

    public static ProjectileEffectProfile Null()
    {
        ProjectileEffectProfile profile = new ProjectileEffectProfile();
        profile.name = "NullEffect";
        return profile;
    }

    public static bool IsNull(ProjectileEffectProfile profile)
    {
        if (profile.name == "NullEffect")
            return true;
        return false;
    }
}

public class ProjectileEffectIcon
{
    public ProjectileEffectProfile profile;
    public GameObject iconGameObj;

    public ProjectileEffectIcon(ProjectileEffectProfile profile, GameObject iconGameObj)
    {
        this.profile = profile;
        this.iconGameObj = iconGameObj;
    }

    public void ExtendTimer(float timeExtension) => profile.effect.timer.ExtendTimer(timeExtension);
}