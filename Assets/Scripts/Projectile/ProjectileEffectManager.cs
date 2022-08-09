using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ProjectileEffectManager : MonoBehaviour
{
    [SerializeField] ProjectileEffectProfile[] effectsProfiles;

    public ProjectileEffectProfile FindProfile(ProjectileEffect effect)
    {
        foreach(ProjectileEffectProfile profile in effectsProfiles)
        {
            if(profile.effect == effect)
            {
                return profile;
            }
        }
        return ProjectileEffectProfile.Null();
    }

    public Dictionary<ProjectileEffect, ProjectileEffectProfile> AssignIconToEffect()
    {
        Dictionary<ProjectileEffect, ProjectileEffectProfile> availabilityList = new Dictionary<ProjectileEffect, ProjectileEffectProfile>();
        foreach(ProjectileEffect effect in Enum.GetValues(typeof(ProjectileEffect)))
        {
            ProjectileEffectProfile profile = FindProfile(effect);
            availabilityList.Add(effect, profile);
        }
        return availabilityList;
    }
}