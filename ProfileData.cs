using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.Security.Cryptography;
using System.Text;


public static class ProfileData
{
	#region Typed Data List

	class DataList<T>
	{
		Dictionary<string, List<ValueChangedDelegate>> valueChangeListeners = new Dictionary<string, List<ValueChangedDelegate>>();
		
		public delegate void ValueChangedDelegate(string key, T value);
		public void AddValueChangeListener(string key, ValueChangedDelegate callback)
		{
			if (valueChangeListeners.ContainsKey(key) == false)
			{
				valueChangeListeners.Add(key, new List<ValueChangedDelegate>());
			}
			valueChangeListeners[key].Add(callback);
		}
		public void RemoveValueChangeListener(string key, ValueChangedDelegate callback)
		{
			if (valueChangeListeners.ContainsKey(key))
			{
				valueChangeListeners[key].Remove(callback);
			}
		}

		public Dictionary<string, T> Values = new Dictionary<string, T>();

		public T Get(string key, T defaultValue)
		{
			return Values.ContainsKey(key) ? Values[key] : defaultValue;
		}

		public void Set(string key, T value)
		{
			if (Values.ContainsKey(key))
			{
				if (Values[key].Equals(value)) return;

				Values[key] = value;
			}
			else
			{
				Values.Add(key, value);
			}

			if (valueChangeListeners.ContainsKey(key))
			{
				for (int i = 0; i < valueChangeListeners[key].Count; i++)
				{
					if (valueChangeListeners[key][i] != null)
					{
						valueChangeListeners[key][i](key, value);
					}
				}
			}
		}

		public void Clear()
		{
			Values.Clear();
			foreach (var i in valueChangeListeners.Keys)
			{
				valueChangeListeners[i].Clear();
			}
			valueChangeListeners.Clear();
		}

		public void print()
		{
			foreach (KeyValuePair<string, T> kvp in Values)
			{
				Debug.Log(string.Concat(kvp.Key, ": ", kvp.Value.ToString(), " (", typeof(T).ToString(), ")"));
			}
		}
	}
		
	#endregion



	#region Profiles

	public const string PROFILE_LIST_KEY = "profiles";

	static List<string> ProfileList = null;
	static void LoadProfileList()
	{
		ProfileList = new List<string>(PlayerPrefs.GetString(PROFILE_LIST_KEY, "").Split(','));
		for (int i = ProfileList.Count - 1; i >= 0; i--)
		{
			if (string.IsNullOrEmpty(ProfileList[i]))
			{
				ProfileList.RemoveAt(i);
			}
			else for (int c = 0; c < ProfileList[i].Length; c++)
			{
				if (char.IsLetterOrDigit(ProfileList[i][c]) == false)
				{
					ProfileList.RemoveAt(i);
				}
			}
		}
	}

	static void SaveProfileList()
	{
		if (ProfileList == null || ProfileList.Count == 0)
		{
			PlayerPrefs.DeleteKey(PROFILE_LIST_KEY);
		}
		else
		{
			stringBuilder.Clear();
			for (int i = 0; i < ProfileList.Count; i++)
			{
				if (i > 0) stringBuilder.Append(',');
				stringBuilder.Append(ProfileList[i]);
			}
			PlayerPrefs.SetString(PROFILE_LIST_KEY, stringBuilder.ToString());
		}

		PlayerPrefs.Save();
	}

	public static string[] GetProfileList()
	{
		if (ProfileList == null) LoadProfileList();
		return ProfileList.ToArray();
	}

    public static int profileCount
    {
        get
        {
            if (ProfileList == null) LoadProfileList();
            return ProfileList.Count;
        }
    }

	public class InvalidProfileNameException : System.Exception
	{
		public InvalidProfileNameException() {}
		public InvalidProfileNameException(string message) : base(message) {}
		public InvalidProfileNameException(string message, System.Exception inner) : base(message, inner) {}
	}

	public static bool AddProfile(string profileName)
	{
		if (string.IsNullOrEmpty(profileName))
		{
            if (TooltipController.Instance != null)
            {
                TooltipController.Instance.show("Invalid profile name: Profile names cannot be empty.", 3);
            }
            else
            {
                throw new InvalidProfileNameException("Invalid profile name: Profile names cannot be empty.");
            }
			return false;
		}
		for (int i = 0; i < profileName.Length; i++)
		{
			if (char.IsLetterOrDigit(profileName[i]) == false)
			{
                if (TooltipController.Instance != null)
                {
                    TooltipController.Instance.show("Invalid profile name: Profile names cannot contain non-alphanumeric characters.", 3);
                }
                else
                {
                    throw new InvalidProfileNameException("Invalid profile name: Profile names cannot contain non-alphanumeric characters.");
                }
                return false;
			}
		}

		if (ProfileList == null) LoadProfileList();

		if (ProfileList.Contains(profileName)) return false;

		ProfileList.Add(profileName);
		SaveProfileList();

		return true;
	}

	public static bool ProfileExists(string profileName)
	{
		if (string.IsNullOrEmpty(profileName)) return false;
		if (ProfileList == null) LoadProfileList();
		return ProfileList.Contains(profileName);
	}


	public static void DeleteProfile(string profileName)
	{
		if (ProfileList == null) LoadProfileList();

		if (ProfileList.Contains(profileName))
		{
			PlayerPrefs.DeleteKey(profileName + "_IntData");
			PlayerPrefs.DeleteKey(profileName + "_FloatData");
			PlayerPrefs.DeleteKey(profileName + "_StringData");
			PlayerPrefs.DeleteKey(profileName + "_Vector3Data");
			PlayerPrefs.DeleteKey(profileName + "_ColorData");
			ProfileList.Remove(profileName);
			SaveProfileList();
		}
	}

	#endregion



	#region Saving and Loading

	static StringBuilder stringBuilder = new StringBuilder();
	static string[] items = null;

    public static string CurrentProfile { get; private set; }

	public static System.Action OnClear;
	public static void Clear()
	{
		ClearWithoutEvent();

        CurrentProfile = null;

		if (OnClear != null) OnClear();
	}

	static void ClearWithoutEvent()
	{
		IntValues.Clear();
		FloatValues.Clear();
		StringValues.Clear();
		Vector3Values.Clear();
		ColorValues.Clear();
	}

	public static void Save()
	{
		if (string.IsNullOrEmpty(CurrentProfile) == false)
		{
			Save(CurrentProfile);
		}
		else
		{
			Debug.LogWarning("Could not save Game State since CurrentProfile is empty or null.");
		}
	}

	public static System.Action OnSave;
	public static void Save(string profileName)
	{
		bool result = false;
		try { result = ProfileExists(profileName) || AddProfile(profileName); }
		catch (InvalidProfileNameException e) { Debug.LogError(e.Message); }
		if (!result) return;

		stringBuilder.Clear();
		foreach (KeyValuePair<string, int> kvp in IntValues.Values)
		{
			if (stringBuilder.Length > 0) stringBuilder.Append('\n');
			stringBuilder.Append(SerializeInt(kvp.Key, kvp.Value));
		}
		PlayerPrefs.SetString(profileName + "_IntData", Encrypt(stringBuilder.ToString()));

		stringBuilder.Clear();
		foreach (KeyValuePair<string, float> kvp in FloatValues.Values)
		{
			if (stringBuilder.Length > 0) stringBuilder.Append('\n');
			stringBuilder.Append(SerializeFloat(kvp.Key, kvp.Value));
		}
		PlayerPrefs.SetString(profileName + "_FloatData", Encrypt(stringBuilder.ToString()));

		stringBuilder.Clear();
		foreach (KeyValuePair<string, string> kvp in StringValues.Values)
		{
			if (stringBuilder.Length > 0) stringBuilder.Append('\n');
			stringBuilder.Append(SerializeString(kvp.Key, kvp.Value));
		}
		PlayerPrefs.SetString(profileName + "_StringData", Encrypt(stringBuilder.ToString()));

		stringBuilder.Clear();
		foreach (KeyValuePair<string, Vector3> kvp in Vector3Values.Values)
		{
			if (stringBuilder.Length > 0) stringBuilder.Append('\n');
			stringBuilder.Append(SerializeVector3(kvp.Key, kvp.Value));
		}
		PlayerPrefs.SetString(profileName + "_Vector3Data", Encrypt(stringBuilder.ToString()));

		stringBuilder.Clear();
		foreach (KeyValuePair<string, Color> kvp in ColorValues.Values)
		{
			if (stringBuilder.Length > 0) stringBuilder.Append('\n');
			stringBuilder.Append(SerializeColor(kvp.Key, kvp.Value));
		}
		PlayerPrefs.SetString(profileName + "_ColorData", Encrypt(stringBuilder.ToString()));

		PlayerPrefs.Save();

		if (OnSave != null) OnSave();
	}

	public static System.Action OnLoad;
	public static void Load(string profileName)
	{
		ClearWithoutEvent();

		items = Decrypt(PlayerPrefs.GetString(profileName + "_IntData", "")).Split('\n');
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i].Length == 0) continue;
			string key = "";
			int intValue = 0;
			if (DeserializeInt(items[i], out key, out intValue))
			{
				IntValues.Set(key, intValue);
			}
		}

		items = Decrypt(PlayerPrefs.GetString(profileName + "_FloatData", "")).Split('\n');
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i].Length == 0) continue;
			string key = "";
			float floatValue = 0;
			if (DeserializeFloat(items[i], out key, out floatValue))
			{
				FloatValues.Set(key, floatValue);
			}
		}

		items = Decrypt(PlayerPrefs.GetString(profileName + "_StringData", "")).Split('\n');
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i].Length == 0) continue;
			string key = "";
			string stringValue = "";
			if (DeserializeString(items[i], out key, out stringValue))
			{
				StringValues.Set(key, stringValue);
			}
		}

		items = Decrypt(PlayerPrefs.GetString(profileName + "_Vector3Data", "")).Split('\n');
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i].Length == 0) continue;
			string key = "";
			Vector3 Vector3Value = Vector3.zero;
			if (DeserializeVector3(items[i], out key, out Vector3Value))
			{
				Vector3Values.Set(key, Vector3Value);
			}
		}

		items = Decrypt(PlayerPrefs.GetString(profileName + "_ColorData", "")).Split('\n');
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i].Length == 0) continue;
			string key = "";
			Color ColorValue = Color.white;
			if (DeserializeColor(items[i], out key, out ColorValue))
			{
				ColorValues.Set(key, ColorValue);
			}
		}

        CurrentProfile = profileName;

		if (OnLoad != null) OnLoad();
	}

    #endregion



    #region Parsing

    public static string ParseString(string s)
    {
        if (s.Length <= 0) return "";

        int currentIndex = 0;

        while (currentIndex < s.Length)
        {
            string key = "";
            string format = "";

            int keyStartIndex = s.IndexOf('[', currentIndex);
            int keyEndIndex = s.IndexOf(']', currentIndex);

            string wholeKey = s.Substring(keyStartIndex + 1, keyEndIndex - keyStartIndex - 1);

            int formatStartIndex = wholeKey.IndexOf('(');
            int formatEndIndex = wholeKey.IndexOf(')');

            if (keyStartIndex != -1 && keyEndIndex != -1)
            {
                string replacedString = "";

                if (formatStartIndex != -1 && keyEndIndex != -1)
                {
                    key = wholeKey.Substring(0, formatStartIndex);
                    format = wholeKey.Substring(formatStartIndex + 1, formatEndIndex - formatStartIndex - 1);

                    Debug.Log(key + " + " + format);

                    if (HasInt(key))
                    {
                        replacedString = GetInt(key).ToString(format);

                        s = replacedString + s.Substring(keyEndIndex + 1, s.Length);

                        currentIndex = keyStartIndex + replacedString.Length;
                    }
                    else
                    {
                        currentIndex = keyEndIndex + 1;
                    }
                }
                else
                {
                    if (HasString(key))
                    {
                        replacedString = GetString(key);

                        s = replacedString + s.Substring(keyEndIndex + 1, s.Length);

                        currentIndex = keyStartIndex + replacedString.Length;
                    }
                    else
                    {
                        currentIndex = keyEndIndex + 1;
                    }
                }
            }
            else
            {
                break;
            }
        }

        return s;
    }

    public static bool HasInt(string key)
    {
        return GetInt(key, 0) != 0;
    }

    public static bool HasString(string key)
    {
        return GetString(key, "") != "";
    }

    public static bool HasFloat(string key)
    {
        return GetFloat(key, 0) != 0;
    }

    public static bool HasVector3(string key)
    {
        return GetVector3(key, Vector3.zero) != Vector3.zero;
    }

    public static bool HasColor(string key)
    {
        return GetColor(key, Color.black) != Color.black;
    }

    #endregion



    #region Encryption

    // http://msdn.microsoft.com/en-us/library/system.security.cryptography.ciphermode.aspx

    public static string PrivateKey { get { throw new System.NotImplementedException("Enter your private key here."); } }

	static string Encrypt(string data)
	{
		string result = "";
		if (!string.IsNullOrEmpty(data))
		{
			byte[] keyArray = UTF8Encoding.UTF8.GetBytes(PrivateKey);
			byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(data);
			RijndaelManaged rDel = new RijndaelManaged();
			rDel.Key = keyArray;
			rDel.Mode = CipherMode.ECB;
			rDel.Padding = PaddingMode.PKCS7;
			ICryptoTransform cTransform = rDel.CreateEncryptor();
			byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
			result = System.Convert.ToBase64String(resultArray, 0, resultArray.Length);
		}

		return result;
	}

	static string Decrypt(string encryptedData)
	{
		string result = "";
		if (!string.IsNullOrEmpty(encryptedData))
		{
			try
			{
				byte[] keyArray = UTF8Encoding.UTF8.GetBytes(PrivateKey);
				byte[] toEncryptArray = System.Convert.FromBase64String(encryptedData);
				RijndaelManaged rDel = new RijndaelManaged();
				rDel.Key = keyArray;
				rDel.Mode = CipherMode.ECB;
				rDel.Padding = PaddingMode.PKCS7;
				ICryptoTransform cTransform = rDel.CreateDecryptor();
				byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
				result = UTF8Encoding.UTF8.GetString(resultArray);
			}
			catch{}
		}
		return result;
	}

	#endregion



	#region Serialization

	static string SerializeInt(string key, int value)
	{
		return string.Concat(key, ":", value.ToString());
	}
	static bool DeserializeInt(string serializedString, out string key, out int value)
	{
		key = null;
		value = 0;
		if (string.IsNullOrEmpty(serializedString))
		{
			Debug.LogError("Failed to deserialize a null or empty string.");
			return false;
		}

		int splitIndex = serializedString.IndexOf(':');
		if (splitIndex < 0)
		{
			Debug.LogError("Failed to deserialize a string without a separator.");
			return false;
		}
		if (splitIndex == 0)
		{
			Debug.LogError("Failed to deserialize a string without a key.");
			return false;
		}
		if (splitIndex == serializedString.Length - 1)
		{
			Debug.LogError("Failed to deserialize a string without a key.");
			return false;
		}

		key = serializedString.Substring(0, splitIndex);
		return int.TryParse(serializedString.Substring(splitIndex + 1), out value);
	}

	static string SerializeFloat(string key, float value)
	{
		return string.Concat(key, ":", value.ToString());
	}
	static bool DeserializeFloat(string serializedString, out string key, out float value)
	{
		key = null;
		value = 0f;
		if (string.IsNullOrEmpty(serializedString))
		{
			Debug.LogError("Failed to deserialize a null or empty string.");
			return false;
		}

		int splitIndex = serializedString.IndexOf(':');
		if (splitIndex < 0)
		{
			Debug.LogError("Failed to deserialize a string without a separator.");
			return false;
		}
		if (splitIndex == 0)
		{
			Debug.LogError("Failed to deserialize a string without a key.");
			return false;
		}
		if (splitIndex == serializedString.Length - 1)
		{
			Debug.LogError("Failed to deserialize a string without a key.");
			return false;
		}

		key = serializedString.Substring(0, splitIndex);
		return float.TryParse(serializedString.Substring(splitIndex + 1), out value);
	}

	static string SerializeString(string key, string value)
	{
		return string.Concat(key, ":", value);
	}
	static bool DeserializeString(string serializedString, out string key, out string value)
	{
		key = null;
		value = null;
		if (string.IsNullOrEmpty(serializedString))
		{
			Debug.LogError("Failed to deserialize a null or empty string.");
			return false;
		}

		int splitIndex = serializedString.IndexOf(':');
		if (splitIndex < 0)
		{
			Debug.LogError("Failed to deserialize a string without a separator.");
			return false;
		}
		if (splitIndex == 0)
		{
			Debug.LogError("Failed to deserialize a string without a key.");
			return false;
		}
		if (splitIndex == serializedString.Length - 1)
		{
			Debug.LogError("Failed to deserialize a string without a key.");
			return false;
		}

		key = serializedString.Substring(0, splitIndex);
		value = serializedString.Substring(splitIndex + 1);
		return true;
	}

	static string SerializeVector3(string key, Vector3 value)
	{
		return string.Concat(key, ':', value.x.ToString(), ',', value.y.ToString(), ',', value.z.ToString());
	}
	static bool DeserializeVector3(string serializedString, out string key, out Vector3 value)
	{
		key = null;
		value = Vector3.zero;
		if (string.IsNullOrEmpty(serializedString))
		{
			Debug.LogError("Failed to deserialize a null or empty string.");
			return false;
		}

		int splitIndex = serializedString.IndexOf(':');
		if (splitIndex < 0)
		{
			Debug.LogError("Failed to deserialize a string without a separator.");
			return false;
		}
		if (splitIndex == 0)
		{
			Debug.LogError("Failed to deserialize a string without a key.");
			return false;
		}
		if (splitIndex == serializedString.Length - 1)
		{
			Debug.LogError("Failed to deserialize a string without a key.");
			return false;
		}

		key = serializedString.Substring(0, splitIndex);
		string[] values = serializedString.Substring(splitIndex + 1).Split(',');
		float x = 0, y = 0, z = 0;
		if (values.Length >= 1 && float.TryParse(values[0], out x) == false)
		{
			Debug.LogError("Failed to deserialize an invalid x value: " + values[0]);
			return false;
		}
		if (values.Length >= 2 && float.TryParse(values[1], out y) == false)
		{
			Debug.LogError("Failed to deserialize an invalid y value: " + values[1]);
			return false;
		}
		if (values.Length >= 3 && float.TryParse(values[2], out z) == false)
		{
			Debug.LogError("Failed to deserialize an invalid z value: " + values[2]);
			return false;
		}
		value.x = x;
		value.y = y;
		value.z = z;
		return true;
	}

	static string SerializeColor(string key, Color value)
	{
		return string.Concat(key, ':', value.r.ToString(), ',', value.g.ToString(), ',', value.b.ToString(), ',', value.a.ToString());
	}
	static bool DeserializeColor(string serializedString, out string key, out Color value)
	{
		key = null;
		value = new Color(0, 0, 0, 0);
		if (string.IsNullOrEmpty(serializedString))
		{
			Debug.LogError("Failed to deserialize a null or empty string.");
			return false;
		}

		int splitIndex = serializedString.IndexOf(':');
		if (splitIndex < 0)
		{
			Debug.LogError("Failed to deserialize a string without a separator.");
			return false;
		}
		if (splitIndex == 0)
		{
			Debug.LogError("Failed to deserialize a string without a key.");
			return false;
		}
		if (splitIndex == serializedString.Length - 1)
		{
			Debug.LogError("Failed to deserialize a string without a key.");
			return false;
		}

		key = serializedString.Substring(0, splitIndex);
		string[] values = serializedString.Substring(splitIndex + 1).Split(',');
		float r = 0, g = 0, b = 0, a = 0;
		if (values.Length >= 1 && float.TryParse(values[0], out r) == false)
		{
			Debug.LogError("Failed to deserialize an invalid r value: " + values[0]);
			return false;
		}
		if (values.Length >= 2 && float.TryParse(values[1], out g) == false)
		{
			Debug.LogError("Failed to deserialize an invalid g value: " + values[1]);
			return false;
		}
		if (values.Length >= 3 && float.TryParse(values[2], out b) == false)
		{
			Debug.LogError("Failed to deserialize an invalid b value: " + values[2]);
			return false;
		}
		if (values.Length >= 4 && float.TryParse(values[3], out a) == false)
		{
			Debug.LogError("Failed to deserialize an invalid a value: " + values[3]);
			return false;
		}
		value.r = r;
		value.g = g;
		value.b = b;
		value.a = a;
		return true;
	}

	#endregion



	#region Int Values

	static DataList<int> IntValues = new DataList<int>();

	public static void SetInt(string key, int value) { IntValues.Set(key, value); }
	public static int GetInt(string key, int defaultValue = 0) { return IntValues.Get(key, defaultValue); }

	public delegate void IntChangedDelegate(string key, int value);
	public static void AddIntChangeListener(string key, IntChangedDelegate callback) { IntValues.AddValueChangeListener(key, new DataList<int>.ValueChangedDelegate(callback)); }
	public static void RemoveIntChangeListener(string key, IntChangedDelegate callback) { IntValues.RemoveValueChangeListener(key, new DataList<int>.ValueChangedDelegate(callback)); }

  	#endregion


	#region Float Values

	static DataList<float> FloatValues = new DataList<float>();

	public static void SetFloat(string key, float value) { FloatValues.Set(key, value); }
	public static float GetFloat(string key, float defaultValue = 0) { return FloatValues.Get(key, defaultValue); }

	public delegate void FloatChangedDelegate(string key, float value);
	public static void AddFloatChangeListener(string key, FloatChangedDelegate callback)
	{
		FloatValues.AddValueChangeListener(key, new DataList<float>.ValueChangedDelegate(callback));
	}
	public static void RemoveFloatChangeListener(string key, FloatChangedDelegate callback)
	{
		FloatValues.RemoveValueChangeListener(key, new DataList<float>.ValueChangedDelegate(callback));
	}

	#endregion


	#region String Values

	static DataList<string> StringValues = new DataList<string>();

	public static void SetString(string key, string value) { StringValues.Set(key, value); }
	public static string GetString(string key, string defaultValue = "") { return StringValues.Get(key, defaultValue); }

	public delegate void StringChangedDelegate(string key, string value);
	public static void AddStringChangeListener(string key, StringChangedDelegate callback)
	{
		StringValues.AddValueChangeListener(key, new DataList<string>.ValueChangedDelegate(callback));
	}
	public static void RemoveStringChangeListener(string key, StringChangedDelegate callback)
	{
		StringValues.RemoveValueChangeListener(key, new DataList<string>.ValueChangedDelegate(callback));
	}

	#endregion


	#region Vector3 Values

	static DataList<Vector3> Vector3Values = new DataList<Vector3>();

	public static void SetVector3(string key, Vector3 value) { Vector3Values.Set(key, value); }
	public static Vector3 GetVector3(string key) { return Vector3Values.Get(key, Vector3.zero); }
	public static Vector3 GetVector3(string key, Vector3 defaultValue) { return Vector3Values.Get(key, defaultValue); }

	public delegate void Vector3ChangedDelegate(string key, Vector3 value);
	public static void AddVector3ChangeListener(string key, Vector3ChangedDelegate callback)
	{
		Vector3Values.AddValueChangeListener(key, new DataList<Vector3>.ValueChangedDelegate(callback));
	}
	public static void RemoveVector3ChangeListener(string key, Vector3ChangedDelegate callback)
	{
		Vector3Values.RemoveValueChangeListener(key, new DataList<Vector3>.ValueChangedDelegate(callback));
	}

	#endregion


	#region Color Values

	static DataList<Color> ColorValues = new DataList<Color>();

	public static void SetColor(string key, Color value) { ColorValues.Set(key, value); }
	public static Color GetColor(string key) { return ColorValues.Get(key, Color.white); }
	public static Color GetColor(string key, Color defaultValue) { return ColorValues.Get(key, defaultValue); }

	public delegate void ColorChangedDelegate(string key, Color value);
	public static void AddColorChangeListener(string key, ColorChangedDelegate callback)
	{
		ColorValues.AddValueChangeListener(key, new DataList<Color>.ValueChangedDelegate(callback));
	}
	public static void RemoveColorChangeListener(string key, ColorChangedDelegate callback)
	{
		ColorValues.RemoveValueChangeListener(key, new DataList<Color>.ValueChangedDelegate(callback));
	}

	#endregion



	#region Debug

	public static void PrintAll()
	{
		IntValues.print();
		FloatValues.print();
		StringValues.print();
		Vector3Values.print();
		ColorValues.print();
	}

	#endregion
}