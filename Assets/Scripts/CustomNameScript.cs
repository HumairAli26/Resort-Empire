using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Reads the resort name typed into a TMP_InputField and shows it in another TMP_Text.
/// Attach to any GameObject, then assign the references in the inspector.
/// </summary>
public class ResortNameInput : MonoBehaviour
{
    private const string SaveKey = "ResortName";

    [Header("UI References")]
    [Tooltip("The input field the player types into.")]
    [SerializeField] private TMP_InputField nameInput;

    [Tooltip("The text that will display the resort name.")]
    [SerializeField] private TMP_Text nameDisplay;

    [Tooltip("Optional: confirm button. If empty, pressing Enter / deselecting confirms.")]
    [SerializeField] private Button confirmButton;

    [Tooltip("Optional: shows validation errors (e.g. 'Name too short').")]
    [SerializeField] private TMP_Text errorText;

    [Header("Rules")]
    [SerializeField] private int minLength = 3;
    [SerializeField] private int maxLength = 24;
    [SerializeField] private string defaultName = "My Resort";

    [Tooltip("Update the display text live while the player types.")]
    [SerializeField] private bool livePreview = false;

    [Tooltip("Save the confirmed name with PlayerPrefs and load it on start.")]
    [SerializeField] private bool persist = true;

    [Header("Events")]
    public UnityEvent<string> onNameConfirmed;

    /// <summary>The last confirmed resort name.</summary>
    public string ResortName { get; private set; }

    private void Awake()
    {
        if (nameInput != null) nameInput.characterLimit = maxLength;
        ResortName = persist ? PlayerPrefs.GetString(SaveKey, defaultName) : defaultName;
    }

    private void OnEnable()
    {
        if (nameInput != null)
        {
            nameInput.onValueChanged.AddListener(OnValueChanged);
            nameInput.onSubmit.AddListener(_ => Confirm());
            if (confirmButton == null) nameInput.onEndEdit.AddListener(OnEndEdit);
        }
        if (confirmButton != null) confirmButton.onClick.AddListener(Confirm);
    }

    private void OnDisable()
    {
        if (nameInput != null)
        {
            nameInput.onValueChanged.RemoveListener(OnValueChanged);
            nameInput.onSubmit.RemoveAllListeners();
            nameInput.onEndEdit.RemoveListener(OnEndEdit);
        }
        if (confirmButton != null) confirmButton.onClick.RemoveListener(Confirm);
    }

    private void Start()
    {
        // Show the saved / default name right away
        if (nameDisplay != null) nameDisplay.text = ResortName;
        if (nameInput != null) nameInput.SetTextWithoutNotify(ResortName);
        SetError(string.Empty);
    }

    private void OnValueChanged(string value)
    {
        if (livePreview && nameDisplay != null)
            nameDisplay.text = string.IsNullOrWhiteSpace(value) ? defaultName : value;
    }

    private void OnEndEdit(string _) => Confirm();

    /// <summary>Copies the input field text into the display text after validating it.</summary>
    public void Confirm()
    {
        if (nameInput == null || nameDisplay == null)
        {
            Debug.LogWarning("[ResortNameInput] Input or display reference missing.", this);
            return;
        }

        string cleaned = nameInput.text.Trim();

        if (cleaned.Length < minLength)
        {
            SetError($"Name must be at least {minLength} characters.");
            return;
        }

        if (cleaned.Length > maxLength)
            cleaned = cleaned.Substring(0, maxLength);

        SetError(string.Empty);

        ResortName = cleaned;
        nameDisplay.text = cleaned;
        nameInput.SetTextWithoutNotify(cleaned);

        if (persist)
        {
            PlayerPrefs.SetString(SaveKey, cleaned);
            PlayerPrefs.Save();
        }

        onNameConfirmed?.Invoke(cleaned);
    }

    private void SetError(string message)
    {
        if (errorText != null) errorText.text = message;
    }
}