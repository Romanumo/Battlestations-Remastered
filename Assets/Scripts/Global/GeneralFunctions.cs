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

    public static List<GameObject> effectsIcons;
    static List<ProjectileEffect> effects;

    public const float halfScreen = 920f;
    const float effectIconsOffset = 120f;

    const float keyTipDefaultTime = 3f;

    public static void Start()
    {
        PerksFactory.Start();
        BulletFactory.Start();
        timers = new List<Timer>();
        effectsIcons = new List<GameObject>();
        effects = new List<ProjectileEffect>();
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

    public static void AddProgressiveTimer(Action onFinished, Action<float> onUpdate, float timer)
    {
        timers.Add(new ProgressiveTimer(onFinished, timer, onUpdate));
    }

    public static void AddProgressiveTimer(Action onFinished, Action onUpdate, float timer)
    {
        timers.Add(new ProgressiveTimer(onFinished, timer, onUpdate));
    }

    public static void RemoveTimer(Timer timer)
    {
        timers.Remove(timer);
    }

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
        if (effects.Contains(profile.effect))
            return;

        GameObject effectIcon = GameObject.Instantiate(GlobalLibrary.effectsIcons.gameObject, GlobalLibrary.effectsParent.position + new Vector3(effectIconsOffset * effectsIcons.Count + GlobalLibrary.effectsParent.position.x, 0, 0), new Quaternion());

        effects.Add(profile.effect);
        effectIcon.transform.parent = GlobalLibrary.effectsParent;
        effectsIcons.Add(effectIcon);
        effectIcon.GetComponent<RawImage>().texture = profile.icon;

        //Offset all icons in the right to the left, when this effect dissapears
        AddTimer(delegate ()
        {
            for (int i = effectsIcons.IndexOf(effectIcon); i < effectsIcons.Count; i++)
            {
                effectsIcons[i].transform.position -= new Vector3(effectIconsOffset, 0, 0);
            }
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

