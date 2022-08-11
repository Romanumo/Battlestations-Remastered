using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public static class GeneralFunctions
{
    public static bool isCursorOverUI = false;
    static List<Timer> timers;

    static List<ProjectileEffectIcon> effectIcons;

    public const float halfScreen = 920f;
    const float effectIconsOffset = 120f;

    const float keyTipDefaultTime = 3f;

    public static void Start()
    {
        PerksFactory.Start();
        BulletFactory.Start();
        timers = new List<Timer>();
        effectIcons = new List<ProjectileEffectIcon>();
    }

    public static void Update()
    {
        TimersUpdate();
    }

    #region Timers
    public static void AddTimer(Action onFinished, float timer)
    {
        timers.Add(new Timer(onFinished, timer));
    }

    public static int AddTimerGetIndex(Action onFinished, float timer)
    {
        timers.Add(new Timer(onFinished, timer));
        return timers.Count - 1;
    }

    public static void AddProgressiveTimer(Action onFinished, Action<float> onUpdate, float timer)
    {
        timers.Add(new ProgressiveTimer(onFinished, timer, onUpdate));
    }

    public static void RemoveTimer(Timer timer)
    {
        timers.Remove(timer);
    }

    public static void ExtendTimer(float timeExtension, int index)
    {
        timers[index].ExtendTimer(timeExtension);
    }

    public static Timer GetTimer(int index) => timers[index];

    static void TimersUpdate()
    {
        if (timers.Count > 0)
        {
            for (int i = 0; i < timers.Count; i++)
            {
                if (timers[i] != null)
                    timers[i].Update();
            }
        }
    }
    #endregion

    #region UI
    public static void AddEffectIcon(ProjectileEffectProfile profile)
    {
        int previousIcon = FindSameEffectIcon(profile.effect);
        if (previousIcon != -1 && effectIcons[previousIcon].GetTimeRemains() < profile.duration)
        {
            float timeExtension = profile.duration - effectIcons[previousIcon].GetTimeRemains();
            effectIcons[previousIcon].ExtendTimer(timeExtension);
            return;
        }

        GameObject effectIcon = CreateEffectIcon(profile);

        //Offset all icons in the right to the left, when this effect dissapears
        AddTimer(delegate ()
        {
            for (int i = effectsIcons.IndexOf(effectIcon); i < effectsIcons.Count; i++)
            {
                effectsIcons[i].transform.position -= new Vector3(effectIconsOffset, 0, 0);
            }
            effects.Remove(profile);
            effectsIcons.Remove(effectIcon);
            UnityEngine.Object.Destroy(effectIcon);
        }, profile.duration);
    }

    public static void GameOver()
    {
        Time.timeScale = 0;
        GlobalLibrary.gameOverWindow.SetActive(true);
        GlobalLibrary.player.GetComponent<PlayerBehaviour>().enabled = false;
    }

    public static void ShowDescribtion(string describtion)
    {
        GlobalLibrary.describtorWindow.SetActive(true);
        GlobalLibrary.describtorText.text = describtion;
        GlobalLibrary.describtorWindow.transform.position = Input.mousePosition;
        if (Input.mousePosition.x > GameObject.FindObjectOfType<Canvas>().GetComponent<RectTransform>().rect.width - halfScreen)
            GlobalLibrary.describtorWindow.transform.position -= new Vector3(GlobalLibrary.describtorWindow.GetComponent<RectTransform>().rect.width, 0, 0);
        isCursorOverUI = true;
    }

    public static void HideDescribtion()
    {
        GlobalLibrary.describtorWindow.SetActive(false);
        isCursorOverUI = false;
    }

    public static void MenuWindowState(bool state)
    {
        Time.timeScale = (state) ? 0 : 1;
        GlobalLibrary.windowMenu.SetActive(state);
    }

    public static void KeyTipShow(string text, float time = keyTipDefaultTime)
    {
        Text keyTip = GlobalLibrary.keyTipText;
        ChangeTextAlpha(keyTip, 1);
        keyTip.gameObject.SetActive(true);
        keyTip.text = text;

        AddProgressiveTimer(delegate () { keyTip.gameObject.SetActive(false); },
            delegate (float timer) { ChangeTextAlpha(keyTip, timer); }, time);
    }

    static void ChangeTextAlpha(Text text, float a)
    {
        text.material.color = new Color(text.material.color.r, text.material.color.g, text.material.color.b, a);
    }

    static GameObject CreateEffectIcon(ProjectileEffectProfile profile)
    {
        GameObject effectIconGameObj = GameObject.Instantiate(GlobalLibrary.effectsIcons.gameObject, GlobalLibrary.effectsParent.position + new Vector3(effectIconsOffset * effectIcons.Count + GlobalLibrary.effectsParent.position.x, 0, 0), new Quaternion());

        ProjectileEffectIcon effectIcon = new ProjectileEffectIcon(profile, effectIconGameObj);
        effectIcons.Add(effectIcon);

        effectIconGameObj.transform.parent = GlobalLibrary.effectsParent;
        effectIconGameObj.GetComponent<RawImage>().texture = profile.icon;
        return effectIconGameObj;
    }

    static int FindSameEffectIcon(ProjectileEffect effect)
    {
        for(int i = 0;i< effectIcons.Count;i++)
        {
            if (effectIcons[i].profile.effect == effect)
                return i;
        }
        return -1;
    }
    #endregion

    public static void AddExplosionEffect(Vector3 position)
    {
        GameObject explosion = GameObject.Instantiate(FindExplosion(ExplosionsEffects.Blast).explosionObj, position, new Quaternion());
        explosion.transform.localScale = Vector3.one * 2.5f;
        explosion.gameObject.GetComponent<ExplosionProjectile>().enabled = false;
        AddTimer(delegate () { UnityEngine.Object.Destroy(explosion); }, 3f);
    }

    public static void AddExplosion(Vector3 position, AttackInfo explosionStats, Entity sender, float radius, ExplosionsEffects effect)
    {
        ExplosionProjectile explosion = GameObject.Instantiate(FindExplosion(effect).explosionObj, Vector3.zero, new Quaternion()).GetComponent<ExplosionProjectile>();
        GameObject explosionObj = explosion.gameObject;

        explosion.SetExplosion(explosionStats, sender, position, radius);
        AddTimer(delegate () { if(explosionObj != null) UnityEngine.Object.Destroy(explosionObj); }, 3f);
    }

    static ExplosionProfile FindExplosion(ExplosionsEffects effect)
    {
        foreach(ExplosionProfile profile in GlobalLibrary.explosions)
        {
            if(profile.effect == effect)
            {
                return profile;
            }
        }
        return null;
    }

    public static float AdjustToRange(float var, float max, float min = 0)
    {
        float result = var;
        if (result > max)
            result = max;
        if (result < min)
            result = min;
        return result;
    }

    public static int AdjustToRange(int var, int max, int min = 0) => Mathf.RoundToInt(AdjustToRange((float)var, max, min));
}

public class Timer
{
    protected Action onFinished;
    protected float timer;

    public Timer(Action onFinished, float maxTimer)
    {
        this.onFinished = onFinished;
        timer = maxTimer;
    }

    public virtual void Update()
    {
        timer -= Time.deltaTime;
        if(timer < 0)
        {
            if (onFinished != null)
            {
                onFinished.Invoke();
                GeneralFunctions.RemoveTimer(this);
            }
        }
    }

    public void ExtendTimer(float timeExtension)
    {
        timer += timeExtension;
    }

    public float GetTime() => timer;
}

public class ProgressiveTimer : Timer
{
    Action<float> onUpdate;
    float maxTimer;

    public ProgressiveTimer(Action onFinished, float maxTimer, Action<float> onUpdate) : base(onFinished, maxTimer)
    {
        this.onUpdate = onUpdate;
        this.maxTimer = maxTimer;
    }

    public ProgressiveTimer(Action onFinished, float maxTimer, Action onUpdate) : base(onFinished, maxTimer)
    {
        this.onUpdate = delegate (float timer) { onUpdate(); };
        this.maxTimer = maxTimer;
    }

    public override void Update()
    {
        base.Update();

        if (onUpdate != null)
            onUpdate.Invoke((float)((float)timer / (float)maxTimer));
    }
}

