using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Events that can cause a screen to open.
/// Add more here as your game grows.
/// </summary>
public enum ScreenTrigger
{
    None,
    NewGamePressed,
    GameLost,
    GameWon,
    GamePaused,
    GameResumed,
    MainMenu,
    Custom // uses ScreenEntry.customTrigger string
}

[Serializable]
public class ScreenEntry
{
    [Tooltip("Unique name, used by Open(id) / Close(id).")]
    public string id;

    [Tooltip("The screen GameObject (panel/canvas) to show.")]
    public GameObject screen;

    [Header("When to open")]
    public ScreenTrigger trigger = ScreenTrigger.None;

    [Tooltip("Only used when trigger = Custom.")]
    public string customTrigger;

    [Tooltip("Optional: clicking this button fires the trigger above (e.g. New Game button).")]
    public Button triggerButton;

    [Header("Behaviour")]
    [Tooltip("Seconds to wait (real time) before opening.")]
    public float delay = 0f;

    [Tooltip("Close every other managed screen when this one opens.")]
    public bool closeOthers = true;

    [Tooltip("Freeze the game (Time.timeScale = 0) while this screen is open.")]
    public bool pauseTime = false;

    [Tooltip("Visible when the scene starts.")]
    public bool startActive = false;

    public UnityEvent onOpened;
    public UnityEvent onClosed;
}

/// <summary>
/// Opens/closes screen GameObjects based on game conditions.
/// Attach to an empty GameObject, drag screens into the list, pick a trigger for each.
/// From game code call: ScreenScheduler.Instance.Trigger(ScreenTrigger.GameLost);
/// </summary>
public class Scheduler : MonoBehaviour
{
    public static Scheduler Instance { get; private set; }

    [SerializeField] private List<ScreenEntry> screens = new List<ScreenEntry>();

    [Header("Polled conditions (optional)")]
    [Tooltip("Check registered condition functions every N seconds. 0 = every frame.")]
    [SerializeField] private float pollInterval = 0.1f;

    // Runtime state
    private readonly Dictionary<string, ScreenEntry> byId = new Dictionary<string, ScreenEntry>();
    private readonly Dictionary<ScreenEntry, Coroutine> pending = new Dictionary<ScreenEntry, Coroutine>();
    private readonly List<(Func<bool> condition, ScreenTrigger trigger, string custom, bool once, bool fired)> conditions
        = new List<(Func<bool>, ScreenTrigger, string, bool, bool)>();
    private readonly List<UnityAction> buttonListeners = new List<UnityAction>();
    private float pollTimer;
    private float cachedTimeScale = 1f;
    private ScreenEntry current;

    /// <summary>Fired every time a screen opens (id, entry).</summary>
    public event Action<ScreenEntry> ScreenOpened;
    public event Action<ScreenEntry> ScreenClosed;

    public ScreenEntry CurrentScreen => current;

    // ------------------------------------------------------------------ Unity

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        foreach (var e in screens)
        {
            if (!string.IsNullOrEmpty(e.id)) byId[e.id] = e;
            if (e.screen != null) e.screen.SetActive(e.startActive);
            if (e.startActive) current = e;
        }
    }

    private void OnEnable()
    {
        // Hook up buttons
        buttonListeners.Clear();
        foreach (var e in screens)
        {
            var entry = e; // capture
            UnityAction action = () => Trigger(entry.trigger, entry.customTrigger);
            buttonListeners.Add(action);
            if (entry.triggerButton != null) entry.triggerButton.onClick.AddListener(action);
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < screens.Count && i < buttonListeners.Count; i++)
        {
            if (screens[i].triggerButton != null)
                screens[i].triggerButton.onClick.RemoveListener(buttonListeners[i]);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (conditions.Count == 0) return;

        pollTimer += Time.unscaledDeltaTime;
        if (pollTimer < pollInterval) return;
        pollTimer = 0f;

        for (int i = 0; i < conditions.Count; i++)
        {
            var c = conditions[i];
            if (c.once && c.fired) continue;
            if (!c.condition()) continue;

            conditions[i] = (c.condition, c.trigger, c.custom, c.once, true);
            Trigger(c.trigger, c.custom);
        }
    }

    // ------------------------------------------------------------- Public API

    /// <summary>Open every screen registered for this trigger.</summary>
    public void Trigger(ScreenTrigger trigger, string custom = null)
    {
        if (trigger == ScreenTrigger.None) return;

        foreach (var e in screens)
        {
            if (e.trigger != trigger) continue;
            if (trigger == ScreenTrigger.Custom && e.customTrigger != custom) continue;
            Schedule(e);
        }
    }

    /// <summary>Convenience for UnityEvents / buttons (enum can't be passed from the inspector as int easily).</summary>
    public void TriggerCustom(string customName) => Trigger(ScreenTrigger.Custom, customName);

    // Handy wrappers you can hook to UI buttons or call from a GameManager
    public void NewGamePressed() => Trigger(ScreenTrigger.NewGamePressed);
    public void GameLost()       => Trigger(ScreenTrigger.GameLost);
    public void GameWon()        => Trigger(ScreenTrigger.GameWon);

    /// <summary>Open a screen directly by its id.</summary>
    public void Open(string id)
    {
        if (byId.TryGetValue(id, out var e)) Schedule(e);
        else Debug.LogWarning($"[ScreenScheduler] No screen with id '{id}'.", this);
    }

    public void Close(string id)
    {
        if (byId.TryGetValue(id, out var e)) CloseEntry(e);
        else Debug.LogWarning($"[ScreenScheduler] No screen with id '{id}'.", this);
    }

    public void CloseAll()
    {
        foreach (var e in screens) CloseEntry(e);
    }

    /// <summary>
    /// Register a condition that is polled automatically, e.g.
    /// scheduler.RegisterCondition(() => player.Health <= 0, ScreenTrigger.GameLost);
    /// </summary>
    public void RegisterCondition(Func<bool> condition, ScreenTrigger trigger,
                                  string custom = null, bool once = true)
    {
        conditions.Add((condition, trigger, custom, once, false));
    }

    public void ClearConditions() => conditions.Clear();

    /// <summary>Re-arm "once" conditions (call when restarting a level).</summary>
    public void ResetConditions()
    {
        for (int i = 0; i < conditions.Count; i++)
        {
            var c = conditions[i];
            conditions[i] = (c.condition, c.trigger, c.custom, c.once, false);
        }
    }

    // --------------------------------------------------------------- Internals

    private void Schedule(ScreenEntry e)
    {
        if (e.screen == null) return;

        // Cancel an already pending open for the same screen
        if (pending.TryGetValue(e, out var running) && running != null)
            StopCoroutine(running);

        if (e.delay <= 0f)
        {
            OpenEntry(e);
            pending.Remove(e);
        }
        else
        {
            pending[e] = StartCoroutine(OpenAfterDelay(e));
        }
    }

    private IEnumerator OpenAfterDelay(ScreenEntry e)
    {
        yield return new WaitForSecondsRealtime(e.delay);
        pending.Remove(e);
        OpenEntry(e);
    }

    private void OpenEntry(ScreenEntry e)
    {
        if (e.closeOthers)
        {
            foreach (var other in screens)
                if (other != e) CloseEntry(other);
        }

        if (e.pauseTime)
        {
            if (Time.timeScale > 0f) cachedTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        e.screen.SetActive(true);
        current = e;
        e.onOpened?.Invoke();
        ScreenOpened?.Invoke(e);
    }

    private void CloseEntry(ScreenEntry e)
    {
        if (e.screen == null || !e.screen.activeSelf) return;

        e.screen.SetActive(false);
        if (current == e) current = null;

        if (e.pauseTime) Time.timeScale = cachedTimeScale;

        e.onClosed?.Invoke();
        ScreenClosed?.Invoke(e);
    }
}