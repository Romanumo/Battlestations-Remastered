using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ProjectileEffectManager : MonoBehaviour
{
    [SerializeField] ProjectileEffectProfile[] effectsProfiles;

    public ProjectileEffectProfile FindProfile(ProjectileEffect effectType)
    {
        foreach(ProjectileEffectProfile profile in effectsProfiles)
        {
            if(profile.effect.effectType == effectType)
            {
                return profile;
            }
        }
        return ProjectileEffectProfile.Null();
    }

    public Dictionary<ProjectileEffect, ProjectileEffectProfile> AssignIconToEffect()
    {
        Dictionary<ProjectileEffect, ProjectileEffectProfile> availabilityList = new Dictionary<ProjectileEffect, ProjectileEffectProfile>();
        foreach(ProjectileEffect effectType in Enum.GetValues(typeof(ProjectileEffect)))
        {
            ProjectileEffectProfile profile = FindProfile(effectType);
            availabilityList.Add(effectType, profile);
        }
        return availabilityList;
    }
}