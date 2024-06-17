using Corale.Colore.Razer.Keyboard;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    internal enum KeyActionType
    {
        KeyHeld,
        KeyDown,
        KeyUp
    }

    internal struct KeyCodeCacheEntry
    {
        public int m_primary;
        public int m_modifierKey;
        public int m_additionalModifierKey;
    }

    internal bool KeyMappingInitialized;
    public Dictionary<int, KeyCodeData> m_keyCodeMapping;
    public Dictionary<int, ControlpadInputValue> m_controlPadMapping;

    private static InputManager s_instance;
    private Dictionary<KeyCode, Key> m_razerKeyMapping;
    private Dictionary<KeyCodeCacheEntry, List<KeyCodeCacheEntry>> m_heaviliyModifiedCommandCache;

    private void Awake()
    {
        s_instance = this;
        m_keyCodeMapping = new Dictionary<int, KeyCodeData>();
        m_controlPadMapping = new Dictionary<int, ControlpadInputValue>();
        m_heaviliyModifiedCommandCache = new Dictionary<KeyCodeCacheEntry, List<KeyCodeCacheEntry>>();
    }

    private void Start()
    {
        SetDefaultKeyMapping();
        ClientGameManager.Get().OnAccountDataUpdated += HandleAccountDataUpdated;
    }

    private void OnDestroy()
    {
        s_instance = null;
        if (ClientGameManager.Get() != null)
        {
            ClientGameManager.Get().OnAccountDataUpdated -= HandleAccountDataUpdated;
        }
    }

    public static InputManager Get()
    {
        return s_instance;
    }

    public void SetDefaultKeyMapping()
    {
        m_keyCodeMapping.Clear();
        m_controlPadMapping.Clear();
        m_heaviliyModifiedCommandCache.Clear();
        for (int i = 0; i < AccountPreferences.Get().GetKeyDefaultsSize(); i++)
        {
            AccountPreferences.KeyCodeDefault keyCodeDefault = AccountPreferences.Get().GetKeyCodeDefault(i);
            m_keyCodeMapping.Add(
                (int)keyCodeDefault.m_preference,
                new KeyCodeData
                {
                    m_primary = (int)keyCodeDefault.m_primary,
                    m_modifierKey1 = (int)keyCodeDefault.m_modifierKey1,
                    m_additionalModifierKey1 = (int)keyCodeDefault.m_additionalModifierKey1,
                    m_secondary = (int)keyCodeDefault.m_secondary,
                    m_modifierKey2 = (int)keyCodeDefault.m_modifierKey2,
                    m_additionalModifierKey2 = (int)keyCodeDefault.m_additionalModifierKey2
                });
        }

        m_controlPadMapping[(int)KeyPreference.ToggleInfo] = ControlpadInputValue.Button_X;
        m_controlPadMapping[(int)KeyPreference.LockIn] = ControlpadInputValue.Button_Y;
        m_controlPadMapping[(int)KeyPreference.CancelAction] = ControlpadInputValue.Button_B;
        m_controlPadMapping[(int)KeyPreference.CameraCenterOnAction] = ControlpadInputValue.Button_leftStickIn;
        m_controlPadMapping[(int)KeyPreference.ShowAllyAbilityInfo] = ControlpadInputValue.Button_rightShoulder;
        m_controlPadMapping[(int)KeyPreference.ToggleSystemMenu] = ControlpadInputValue.Button_start;
        m_controlPadMapping[(int)KeyPreference.CameraToggleAutoCenter] = ControlpadInputValue.Button_rightStickIn;
    }

    public void ClearKeyBind(KeyPreference m_preference, bool primary)
    {
        if (!m_keyCodeMapping.TryGetValue((int)m_preference, out KeyCodeData mapping))
        {
            return;
        }

        if (primary)
        {
            mapping.m_primary = (int)KeyCode.None;
            mapping.m_modifierKey1 = (int)KeyCode.None;
            mapping.m_additionalModifierKey1 = (int)KeyCode.None;
        }
        else
        {
            mapping.m_secondary = (int)KeyCode.None;
            mapping.m_modifierKey2 = (int)KeyCode.None;
            mapping.m_additionalModifierKey2 = (int)KeyCode.None;
        }

        m_heaviliyModifiedCommandCache.Clear();
    }

    public bool SetCustomKeyBind(
        KeyPreference preference,
        KeyCode keyCode,
        KeyCode modifierKey,
        KeyCode additionalModifierKey,
        bool primary)
    {
        KeyBindingCommand keyBindingCommand = GameWideData.Get().GetKeyBindingCommand(preference.ToString());
        if (keyBindingCommand == null)
        {
            return false;
        }

        if (!m_keyCodeMapping.TryGetValue((int)preference, out KeyCodeData mapping))
        {
            return false;
        }

        if (primary)
        {
            mapping.m_primary = (int)keyCode;
            mapping.m_modifierKey1 = (int)modifierKey;
            mapping.m_additionalModifierKey1 = (int)additionalModifierKey;
        }
        else
        {
            mapping.m_secondary = (int)keyCode;
            mapping.m_modifierKey2 = (int)modifierKey;
            mapping.m_additionalModifierKey2 = (int)additionalModifierKey;
        }

        foreach (KeyValuePair<int, KeyCodeData> keyCodeMapping in m_keyCodeMapping)
        {
            KeyPreference key = (KeyPreference)keyCodeMapping.Key;
            KeyBindingCommand mappedCommand = GameWideData.Get().GetKeyBindingCommand(key.ToString());

            if (mappedCommand == null)
            {
                Log.Error("Could not find KeyBindingCommand for {0} in GameWideData", key.ToString());
                continue;
            }

            if (!mappedCommand.Settable)
            {
                continue;
            }

            if (keyBindingCommand.Category != KeyBindCategory.Global
                && mappedCommand.Category != KeyBindCategory.Global
                && mappedCommand.Category != keyBindingCommand.Category)
            {
                continue;
            }

            if (key != preference || !primary)
            {
                if (keyCodeMapping.Value.m_primary == (int)keyCode
                    && keyCodeMapping.Value.m_modifierKey1 == (int)modifierKey
                    && keyCodeMapping.Value.m_additionalModifierKey1 == (int)additionalModifierKey)
                {
                    keyCodeMapping.Value.m_primary = (int)KeyCode.None;
                    keyCodeMapping.Value.m_modifierKey1 = (int)KeyCode.None;
                    keyCodeMapping.Value.m_additionalModifierKey1 = (int)KeyCode.None;
                }
            }

            if (key != preference || primary)
                if (keyCodeMapping.Value.m_secondary == (int)keyCode
                    && keyCodeMapping.Value.m_modifierKey2 == (int)modifierKey
                    && keyCodeMapping.Value.m_additionalModifierKey2 == (int)additionalModifierKey)
                {
                    keyCodeMapping.Value.m_secondary = (int)KeyCode.None;
                    keyCodeMapping.Value.m_modifierKey2 = (int)KeyCode.None;
                    keyCodeMapping.Value.m_additionalModifierKey2 = (int)KeyCode.None;
                }
        }

        m_heaviliyModifiedCommandCache.Clear();
        return true;
    }

    public void SaveAllKeyBinds()
    {
        ClientGameManager.Get().NotifyCustomKeyBinds(m_keyCodeMapping);
    }

    public void HandleAccountDataUpdated(PersistedAccountData accountData)
    {
        if (KeyMappingInitialized)
        {
            return;
        }

        m_keyCodeMapping.Clear();
        m_heaviliyModifiedCommandCache.Clear();
        bool flag = false;
        for (int i = 0; i < AccountPreferences.Get().GetKeyDefaultsSize(); i++)
        {
            AccountPreferences.KeyCodeDefault keyCodeDefault = AccountPreferences.Get().GetKeyCodeDefault(i);
            int preference = (int)keyCodeDefault.m_preference;
            if (accountData.AccountComponent.KeyCodeMapping.TryGetValue(preference, out KeyCodeData value))
            {
                m_keyCodeMapping.Add(preference, value);
                continue;
            }

            m_keyCodeMapping.Add(
                preference,
                new KeyCodeData
                {
                    m_primary = (int)keyCodeDefault.m_primary,
                    m_modifierKey1 = (int)keyCodeDefault.m_modifierKey1,
                    m_additionalModifierKey1 = (int)keyCodeDefault.m_additionalModifierKey1,
                    m_secondary = (int)keyCodeDefault.m_secondary,
                    m_modifierKey2 = (int)keyCodeDefault.m_modifierKey2,
                    m_additionalModifierKey2 = (int)keyCodeDefault.m_additionalModifierKey2
                });
            flag = true;
        }

        if (flag)
        {
            SaveAllKeyBinds();
        }

        KeyMappingInitialized = true;
    }

    public bool IsModifierKey(KeyCode key)
    {
        return key == KeyCode.RightControl
               || key == KeyCode.LeftControl
               || key == KeyCode.RightShift
               || key == KeyCode.LeftShift
               || key == KeyCode.RightAlt
               || key == KeyCode.LeftAlt;
    }

    private KeyCode CombineLeftRightModifiers(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.RightControl:
            case KeyCode.LeftControl:
                return KeyCode.LeftControl;
            case KeyCode.RightShift:
            case KeyCode.LeftShift:
                return KeyCode.LeftShift;
            case KeyCode.RightAlt:
            case KeyCode.LeftAlt:
                return KeyCode.LeftAlt;
            default:
                return key;
        }
    }

    public bool IsControlDown()
    {
        return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
    }

    public bool IsAltDown()
    {
        return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
    }

    public bool IsShiftDown()
    {
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }

    public void GetModifierKeys(out KeyCode modifier, out KeyCode additionalModifier)
    {
        modifier = KeyCode.None;
        additionalModifier = KeyCode.None;

        if (IsControlDown())
        {
            modifier = KeyCode.LeftControl;
        }
        else if (IsAltDown())
        {
            modifier = KeyCode.LeftAlt;
        }
        else if (IsShiftDown())
        {
            modifier = KeyCode.LeftShift;
        }

        if (modifier == KeyCode.LeftControl)
        {
            if (IsAltDown())
            {
                additionalModifier = KeyCode.LeftAlt;
            }
            else if (IsShiftDown())
            {
                additionalModifier = KeyCode.LeftShift;
            }
        }
        else if (modifier == KeyCode.LeftAlt && IsShiftDown())
        {
            additionalModifier = KeyCode.LeftShift;
        }
    }

    public bool IsUnbindableKey(KeyCode key)
    {
        return key == KeyCode.Pause
               || key == KeyCode.ScrollLock
               || key == KeyCode.Break
               || key == KeyCode.Mouse0
               || key == KeyCode.Mouse1
               || key == KeyCode.Menu
               || key == KeyCode.Slash
               || key == KeyCode.Return
               || key == KeyCode.KeypadEnter;
    }

    private bool CheckModifierDown(KeyCode modifierKey)
    {
        switch (modifierKey)
        {
            case KeyCode.RightControl:
            case KeyCode.LeftControl:
                return IsControlDown();
            case KeyCode.RightShift:
            case KeyCode.LeftShift:
                return IsShiftDown();
            case KeyCode.RightAlt:
            case KeyCode.LeftAlt:
                return IsAltDown();
            default:
                return false;
        }
    }

    private bool IsModifierDown(KeyCode modifierKey, KeyCode additionalModifierKey)
    {
        bool flag = true;
        if (modifierKey != KeyCode.None)
        {
            flag = CheckModifierDown(modifierKey);
        }

        if (flag && additionalModifierKey != KeyCode.None)
        {
            flag = CheckModifierDown(additionalModifierKey);
        }

        return flag;
    }

    private bool IsMoreHeavilyModifiedKeyCommandDown(KeyCode key, KeyCode modifierKey, KeyCode additionalModifierKey)
    {
        KeyCodeCacheEntry cacheKey = new KeyCodeCacheEntry
        {
            m_primary = (int)key,
            m_modifierKey = (int)modifierKey,
            m_additionalModifierKey = (int)additionalModifierKey
        };

        if (!m_heaviliyModifiedCommandCache.ContainsKey(cacheKey))
        {
            List<KeyCodeCacheEntry> cacheEntries = new List<KeyCodeCacheEntry>();

            foreach (KeyCodeData value in m_keyCodeMapping.Values)
            {
                if (value.m_primary == (int)key
                    && ((modifierKey == KeyCode.None && value.m_modifierKey1 != (int)KeyCode.None)
                        || (additionalModifierKey == KeyCode.None
                            && value.m_additionalModifierKey1 != (int)KeyCode.None)))
                {
                    cacheEntries.Add(
                        new KeyCodeCacheEntry
                        {
                            m_primary = value.m_primary,
                            m_modifierKey = value.m_modifierKey1,
                            m_additionalModifierKey = value.m_additionalModifierKey1
                        });
                }

                if (value.m_secondary == (int)key
                    && ((modifierKey == KeyCode.None && value.m_modifierKey2 != (int)KeyCode.None)
                        || (additionalModifierKey == KeyCode.None
                            && value.m_additionalModifierKey2 != (int)KeyCode.None)))
                {
                    cacheEntries.Add(
                        new KeyCodeCacheEntry
                        {
                            m_primary = value.m_primary,
                            m_modifierKey = value.m_modifierKey2,
                            m_additionalModifierKey = value.m_additionalModifierKey2
                        });
                }
            }

            m_heaviliyModifiedCommandCache[cacheKey] = cacheEntries;
        }

        foreach (KeyCodeCacheEntry cacheEntry in m_heaviliyModifiedCommandCache[cacheKey])
        {
            if (IsModifierDown((KeyCode)cacheEntry.m_modifierKey, (KeyCode)cacheEntry.m_additionalModifierKey))
            {
                return true;
            }
        }

        return false;
    }

    private bool CheckKeyAction(
        KeyCode key,
        KeyCode modifierKey,
        KeyCode additionalModifierKey,
        KeyActionType keyDownType)
    {
        bool flag = false;
        if (key != KeyCode.None)
        {
            switch (keyDownType)
            {
                case KeyActionType.KeyHeld:
                    flag = Input.GetKey(key);
                    break;
                case KeyActionType.KeyDown:
                    flag = Input.GetKeyDown(key);
                    break;
                case KeyActionType.KeyUp:
                    flag = Input.GetKeyUp(key);
                    break;
            }
        }

        return flag
               && IsModifierDown(modifierKey, additionalModifierKey)
               && !IsMoreHeavilyModifiedKeyCommandDown(key, modifierKey, additionalModifierKey);
    }

    internal bool IsKeyBindingHeld(KeyPreference actionName)
    {
        if (actionName == KeyPreference.NullPreference)
        {
            return false;
        }

        if (UIUtils.InputFieldHasFocus()
            || UIUtils.SettingKeybindCommand()
            || !AccountPreferences.DoesApplicationHaveFocus())
        {
            return false;
        }

        bool flag = false;
        m_keyCodeMapping.TryGetValue((int)actionName, out KeyCodeData mapping);
        if (mapping != null)
        {
            flag = CheckKeyAction(
                       (KeyCode)mapping.m_primary,
                       (KeyCode)mapping.m_modifierKey1,
                       (KeyCode)mapping.m_additionalModifierKey1,
                       KeyActionType.KeyHeld)
                   || CheckKeyAction(
                       (KeyCode)mapping.m_secondary,
                       (KeyCode)mapping.m_modifierKey2,
                       (KeyCode)mapping.m_additionalModifierKey2,
                       KeyActionType.KeyHeld);
        }

        if (!flag && m_controlPadMapping.ContainsKey((int)actionName))
        {
            ControlpadInputValue controlpadInputValue = m_controlPadMapping[(int)actionName];
            if (controlpadInputValue != ControlpadInputValue.INVALID)
            {
                flag = ControlpadGameplay.Get().GetButton(controlpadInputValue);
            }
        }

        return flag;
    }

    internal bool IsKeyBindingNewlyHeld(KeyPreference actionName)
    {
        if (actionName == KeyPreference.NullPreference)
        {
            return false;
        }

        if (UIUtils.InputFieldHasFocus()
            || UIUtils.SettingKeybindCommand()
            || !AccountPreferences.DoesApplicationHaveFocus())
        {
            return false;
        }

        bool flag = false;
        m_keyCodeMapping.TryGetValue((int)actionName, out KeyCodeData mapping);
        if (mapping != null)
        {
            flag = CheckKeyAction(
                       (KeyCode)mapping.m_primary,
                       (KeyCode)mapping.m_modifierKey1,
                       (KeyCode)mapping.m_additionalModifierKey1,
                       KeyActionType.KeyDown)
                   || CheckKeyAction(
                       (KeyCode)mapping.m_secondary,
                       (KeyCode)mapping.m_modifierKey2,
                       (KeyCode)mapping.m_additionalModifierKey2,
                       KeyActionType.KeyDown);
        }

        if (!flag && m_controlPadMapping.ContainsKey((int)actionName))
        {
            m_controlPadMapping.TryGetValue((int)actionName, out ControlpadInputValue controlpadInputValue);
            if (controlpadInputValue != ControlpadInputValue.INVALID)
            {
                flag = ControlpadGameplay.Get().GetButtonDown(controlpadInputValue);
            }
        }

        return flag;
    }

    internal bool IsKeyBindingNewlyReleased(KeyPreference actionName)
    {
        if (actionName == KeyPreference.NullPreference)
        {
            return false;
        }

        if (UIUtils.InputFieldHasFocus()
            || UIUtils.SettingKeybindCommand()
            || !AccountPreferences.DoesApplicationHaveFocus())
        {
            return false;
        }

        bool flag = false;
        m_keyCodeMapping.TryGetValue((int)actionName, out KeyCodeData mapping);
        if (mapping != null)
        {
            flag = CheckKeyAction(
                       (KeyCode)mapping.m_primary,
                       (KeyCode)mapping.m_modifierKey1,
                       (KeyCode)mapping.m_additionalModifierKey1,
                       KeyActionType.KeyUp)
                   || CheckKeyAction(
                       (KeyCode)mapping.m_secondary,
                       (KeyCode)mapping.m_modifierKey2,
                       (KeyCode)mapping.m_additionalModifierKey2,
                       KeyActionType.KeyUp);
        }

        if (!flag && m_controlPadMapping.ContainsKey((int)actionName))
        {
            m_controlPadMapping.TryGetValue((int)actionName, out ControlpadInputValue controlpadInputValue);
            if (controlpadInputValue != ControlpadInputValue.INVALID)
            {
                flag = ControlpadGameplay.Get().GetButtonUp(controlpadInputValue);
            }
        }

        return flag;
    }

    internal bool GetAcceptButtonDown()
    {
        return ControlpadGameplay.Get().GetButtonDown(ControlpadInputValue.Button_A);
    }

    internal bool GetCancelButtonDown()
    {
        return ControlpadGameplay.Get().GetButtonDown(ControlpadInputValue.Button_B);
    }

    internal bool IsKeyCodeMatchKeyBind(KeyPreference actionName, KeyCode code)
    {
        return actionName != KeyPreference.NullPreference
               && m_keyCodeMapping.TryGetValue((int)actionName, out KeyCodeData mapping)
               && (mapping.m_primary == (int)code || mapping.m_secondary == (int)code);
    }

    public string GetFullKeyString(KeyPreference actionName, bool primaryKey, bool shortStr = false)
    {
        return m_keyCodeMapping.TryGetValue((int)actionName, out KeyCodeData mapping)
            ? primaryKey
                ? GetFullKeyString(
                    (KeyCode)mapping.m_primary,
                    (KeyCode)mapping.m_modifierKey1,
                    (KeyCode)mapping.m_additionalModifierKey1,
                    shortStr)
                : GetFullKeyString(
                    (KeyCode)mapping.m_secondary,
                    (KeyCode)mapping.m_modifierKey2,
                    (KeyCode)mapping.m_additionalModifierKey2,
                    shortStr)
            : string.Empty;
    }

    private string GetFullKeyString(
        KeyCode key,
        KeyCode modifierKey,
        KeyCode additionalModifierKey,
        bool shortStr = false)
    {
        string keyString = GetKeyString(key, shortStr).ToUpper();
        string modifierKeyString = GetModifierKeyString(modifierKey, shortStr);
        string additionalModifierKeyString = GetModifierKeyString(additionalModifierKey, shortStr);
        if (keyString.IsNullOrEmpty())
        {
            return string.Empty;
        }

        if (modifierKeyString.IsNullOrEmpty() && additionalModifierKeyString.IsNullOrEmpty())
        {
            return keyString;
        }

        if (shortStr)
        {
            return $"{modifierKeyString}{additionalModifierKeyString}-{keyString}";
        }

        if (modifierKeyString.IsNullOrEmpty() || !additionalModifierKeyString.IsNullOrEmpty())
        {
            return $"{modifierKeyString} {additionalModifierKeyString} {keyString}";
        }

        return $"{modifierKeyString} {keyString}";
    }

    private string GetModifierKeyString(KeyCode key, bool shortStr)
    {
        switch (key)
        {
            case KeyCode.RightControl:
            case KeyCode.LeftControl:
                return StringUtil.TR(shortStr ? "CtrlShort" : "Ctrl", "Keyboard");
            case KeyCode.RightShift:
            case KeyCode.LeftShift:
                return StringUtil.TR(shortStr ? "ShiftShort" : "Shift", "Keyboard");
            case KeyCode.RightAlt:
            case KeyCode.LeftAlt:
                return StringUtil.TR(shortStr ? "AltShort" : "Alt", "Keyboard");
            default:
                return string.Empty;
        }
    }

    private string GetKeyString(KeyCode key, bool shortStr)
    {
        switch (key)
        {
            case KeyCode.UpArrow:
                return StringUtil.TR("UpArrow", "Keyboard");
            case KeyCode.LeftArrow:
                return StringUtil.TR("LeftArrow", "Keyboard");
            case KeyCode.DownArrow:
                return StringUtil.TR("DownArrow", "Keyboard");
            case KeyCode.RightArrow:
                return StringUtil.TR("RightArrow", "Keyboard");
            case KeyCode.RightControl:
                return StringUtil.TR(shortStr ? "RightCtrlShort" : "RightCtrl", "Keyboard");
            case KeyCode.LeftControl:
                return StringUtil.TR(shortStr ? "LeftCtrlShort" : "LeftCtrl", "Keyboard");
            case KeyCode.RightShift:
                return StringUtil.TR(shortStr ? "RightShiftShort" : "RightShift", "Keyboard");
            case KeyCode.LeftShift:
                return StringUtil.TR(shortStr ? "LeftShiftShort" : "LeftShift", "Keyboard");
            case KeyCode.RightAlt:
                return StringUtil.TR(shortStr ? "RightAltShort" : "RightAlt", "Keyboard");
            case KeyCode.LeftAlt:
                return StringUtil.TR(shortStr ? "LeftAltShort" : "LeftAlt", "Keyboard");
            case KeyCode.Tab:
                return StringUtil.TR("Tab", "Keyboard");
            case KeyCode.Escape:
                return StringUtil.TR("Escape", "Keyboard");
            case KeyCode.Backspace:
                return StringUtil.TR("Backspace", "Keyboard");
            case KeyCode.Numlock:
                return StringUtil.TR("NumLock", "Keyboard");
            case KeyCode.Insert:
                return StringUtil.TR("Insert", "Keyboard");
            case KeyCode.Home:
                return StringUtil.TR("Home", "Keyboard");
            case KeyCode.End:
                return StringUtil.TR("End", "Keyboard");
            case KeyCode.KeypadMultiply:
                return StringUtil.TR("NumPadMultiply", "Keyboard");
            case KeyCode.KeypadPlus:
                return StringUtil.TR("NumPadPlus", "Keyboard");
            case KeyCode.KeypadMinus:
                return StringUtil.TR("NumPadMinus", "Keyboard");
            case KeyCode.KeypadDivide:
                return StringUtil.TR("NumPadDivide", "Keyboard");
            case KeyCode.PageUp:
                return StringUtil.TR("PageUp", "Keyboard");
            case KeyCode.PageDown:
                return StringUtil.TR("PageDown", "Keyboard");
            case KeyCode.Pause:
                return StringUtil.TR("Pause", "Keyboard");
            case KeyCode.Return:
                return StringUtil.TR("Return", "Keyboard");
            case KeyCode.CapsLock:
                return StringUtil.TR("CapsLock", "Keyboard");
            case KeyCode.Print:
                return StringUtil.TR("PrintScreen", "Keyboard");
            case KeyCode.Keypad0:
                return StringUtil.TR("NumPad0", "Keyboard");
            case KeyCode.Keypad1:
                return StringUtil.TR("NumPad1", "Keyboard");
            case KeyCode.Keypad2:
                return StringUtil.TR("NumPad2", "Keyboard");
            case KeyCode.Keypad3:
                return StringUtil.TR("NumPad3", "Keyboard");
            case KeyCode.Keypad4:
                return StringUtil.TR("NumPad4", "Keyboard");
            case KeyCode.Keypad5:
                return StringUtil.TR("NumPad5", "Keyboard");
            case KeyCode.Keypad6:
                return StringUtil.TR("NumPad6", "Keyboard");
            case KeyCode.Keypad7:
                return StringUtil.TR("NumPad7", "Keyboard");
            case KeyCode.Keypad8:
                return StringUtil.TR("NumPad8", "Keyboard");
            case KeyCode.Keypad9:
                return StringUtil.TR("NumPad9", "Keyboard");
            case KeyCode.Alpha1:
                return "1";
            case KeyCode.Alpha2:
                return "2";
            case KeyCode.Alpha3:
                return "3";
            case KeyCode.Alpha4:
                return "4";
            case KeyCode.Alpha5:
                return "5";
            case KeyCode.Alpha6:
                return "6";
            case KeyCode.Alpha7:
                return "7";
            case KeyCode.Alpha8:
                return "8";
            case KeyCode.Alpha9:
                return "9";
            case KeyCode.Alpha0:
                return "0";
            case KeyCode.Mouse2:
            case KeyCode.Mouse3:
            case KeyCode.Mouse4:
            case KeyCode.Mouse5:
            case KeyCode.Mouse6:
            {
                return string.Format(
                    shortStr
                        ? StringUtil.TR("MouseButtonShort", "Keyboard")
                        : StringUtil.TR("MouseButton", "Keyboard"),
                    key - KeyCode.Mouse0 + 1);
            }
            case KeyCode.BackQuote:
                return "`";
            case KeyCode.Minus:
                return "-";
            case KeyCode.Equals:
                return "=";
            case KeyCode.LeftBracket:
                return "[";
            case KeyCode.RightBracket:
                return "]";
            case KeyCode.Backslash:
                return "\\";
            case KeyCode.Comma:
                return ",";
            case KeyCode.Period:
                return ".";
            case KeyCode.None:
            case KeyCode.Space:
                return StringUtil.TR("Space", "Keyboard");
            case KeyCode.Quote:
                return "'";
            case KeyCode.Delete:
                return StringUtil.TR("Delete", "Keyboard");
            default:
                return key.ToString();
        }
    }

    public void KeyCommandDisplay(KeyPreference keyPreference)
    {
        string arg = StringUtil.TR_KeyBindCommand(keyPreference.ToString());
        string fullKeyString = GetFullKeyString(keyPreference, true);
        TextConsole.Get().Write($"key:{arg} maps to:{fullKeyString}");
    }

    private void BuildRazorKeyLookupMap()
    {
        m_razerKeyMapping = new Dictionary<KeyCode, Key>
        {
            [KeyCode.Alpha0] = Key.D0,
            [KeyCode.Alpha1] = Key.D1,
            [KeyCode.Alpha2] = Key.D2,
            [KeyCode.Alpha3] = Key.D3,
            [KeyCode.Alpha4] = Key.D4,
            [KeyCode.Alpha5] = Key.D5,
            [KeyCode.Alpha6] = Key.D6,
            [KeyCode.Alpha7] = Key.D7,
            [KeyCode.Alpha8] = Key.D8,
            [KeyCode.Alpha9] = Key.D9,
            [KeyCode.Keypad0] = Key.Num0,
            [KeyCode.Keypad1] = Key.Num1,
            [KeyCode.Keypad2] = Key.Num2,
            [KeyCode.Keypad3] = Key.Num3,
            [KeyCode.Keypad4] = Key.Num4,
            [KeyCode.Keypad5] = Key.Num5,
            [KeyCode.Keypad6] = Key.Num6,
            [KeyCode.Keypad7] = Key.Num7,
            [KeyCode.Keypad8] = Key.Num8,
            [KeyCode.Keypad9] = Key.Num9,
            [KeyCode.KeypadPeriod] = Key.NumDecimal,
            [KeyCode.KeypadDivide] = Key.NumDivide,
            [KeyCode.KeypadMultiply] = Key.NumMultiply,
            [KeyCode.KeypadMinus] = Key.NumSubtract,
            [KeyCode.KeypadPlus] = Key.NumAdd,
            [KeyCode.KeypadEnter] = Key.NumEnter,
            [KeyCode.UpArrow] = Key.Up,
            [KeyCode.DownArrow] = Key.Down,
            [KeyCode.RightArrow] = Key.Right,
            [KeyCode.LeftArrow] = Key.Left,
            [KeyCode.Numlock] = Key.NumLock
        };

        foreach (KeyCode unityKey in Enum.GetValues(typeof(KeyCode)))
        {
            foreach (Key razerKey in Enum.GetValues(typeof(Key)))
            {
                if (unityKey.ToString() == razerKey.ToString()
                    || unityKey.ToString() == razerKey.ToString().Replace("Oem", string.Empty))
                {
                    m_razerKeyMapping[unityKey] = razerKey;
                    break;
                }
            }
        }
    }

    public bool GetRazorKey(KeyPreference actionName, out Key razerKey)
    {
        if (m_razerKeyMapping.IsNullOrEmpty())
        {
            BuildRazorKeyLookupMap();
        }

        razerKey = Key.Invalid;

        m_keyCodeMapping.TryGetValue((int)actionName, out KeyCodeData mapping);
        if (mapping == null)
        {
            return false;
        }

        KeyCode keyCode;
        if (mapping.m_primary != (int)KeyCode.None)
        {
            keyCode = (KeyCode)mapping.m_primary;
        }
        else if (mapping.m_secondary != (int)KeyCode.None)
        {
            keyCode = (KeyCode)mapping.m_secondary;
        }
        else
        {
            return false;
        }

        return m_razerKeyMapping.TryGetValue(keyCode, out razerKey);
    }
}