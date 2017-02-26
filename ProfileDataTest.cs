using UnityEngine;
using System.Collections;

public class ProfileDataTest : MonoBehaviour
{
	[SerializeField] string profileName;

	[SerializeField] bool clearPlayerPrefs = false;
	
	const string TEST_INT = "test_int";
	const string TEST_FLOAT = "test_float";
	const string TEST_STRING = "test_string";
	const string TEST_VECTOR3 = "test_vector3";
	const string TEST_COLOR = "test_color";


	void Start()
	{
		if (clearPlayerPrefs)
		{
			Debug.Log("Clearing PlayerPrefs...");
			PlayerPrefs.DeleteAll();
		}

		Debug.Log("Adding value change listeners...");
		ProfileData.AddIntChangeListener(TEST_INT, handleIntChanged);
		ProfileData.AddFloatChangeListener(TEST_FLOAT, handleFloatChanged);
		ProfileData.AddStringChangeListener(TEST_STRING, handleStringChanged);
		ProfileData.AddVector3ChangeListener(TEST_VECTOR3, handleVector3Changed);
		ProfileData.AddColorChangeListener(TEST_COLOR, handleColorChanged);

		Debug.Log("Adding save, load, and clear event listeners...");
		ProfileData.OnSave += handleSaved;
		ProfileData.OnLoad += handleLoaded;
		ProfileData.OnClear += handleCleared;

		Debug.Log("Setting values...");
		setValues();

		Debug.Log("Saving to profile: " + profileName + "...");
		ProfileData.Save(profileName);
		Debug.Log("Clearing all data...");
		ProfileData.Clear();
		Debug.Log("Printing all ProfileData values...");
		ProfileData.PrintAll();
		Debug.Log("End of ProfileData values list");

		Debug.Log("Loading from profile: " + profileName + "...");
		ProfileData.Load(profileName);
		//printValues();
		Debug.Log("Printing all ProfileData values...");
		ProfileData.PrintAll();
		Debug.Log("End of ProfileData values list");

		Debug.Log("Deleting profile: " + profileName + "...");
		ProfileData.DeleteProfile(profileName);
		Debug.Log("Loading from deleted profile: " + profileName + "...");
		ProfileData.Load(profileName);

		//printValues();
		Debug.Log("Printing all ProfileData values...");
		ProfileData.PrintAll();
		Debug.Log("End of ProfileData values list");


		Debug.Log("Removing value change listeners...");
		ProfileData.RemoveIntChangeListener(TEST_INT, handleIntChanged);
		ProfileData.RemoveFloatChangeListener(TEST_FLOAT, handleFloatChanged);
		ProfileData.RemoveStringChangeListener(TEST_STRING, handleStringChanged);
		ProfileData.RemoveVector3ChangeListener(TEST_VECTOR3, handleVector3Changed);
		ProfileData.RemoveColorChangeListener(TEST_COLOR, handleColorChanged);

		Debug.Log("Removing save, load, and clear event listeners...");
		ProfileData.OnSave -= handleSaved;
		ProfileData.OnLoad -= handleLoaded;
		ProfileData.OnClear -= handleCleared;
	}

	void setValues()
	{
		ProfileData.SetInt(TEST_INT, 100);
		ProfileData.SetInt(TEST_INT + "_1", 1);
		ProfileData.SetInt(TEST_INT + "_2", 2);
		ProfileData.SetInt(TEST_INT + "_3", 3);

		ProfileData.SetFloat(TEST_FLOAT, 99.9f);
		ProfileData.SetFloat(TEST_FLOAT + "_1", 1.1f);
		ProfileData.SetFloat(TEST_FLOAT + "_2", 1.2f);
		ProfileData.SetFloat(TEST_FLOAT + "_3", 1.3f);

		ProfileData.SetString(TEST_STRING, "test");
		ProfileData.SetString(TEST_STRING + "_1", "test 1");
		ProfileData.SetString(TEST_STRING + "_2", "test 2");
		ProfileData.SetString(TEST_STRING + "_3", "test 3");

		ProfileData.SetVector3(TEST_VECTOR3, Vector3.one);
		ProfileData.SetVector3(TEST_VECTOR3 + "_1", Vector3.right);
		ProfileData.SetVector3(TEST_VECTOR3 + "_2", Vector3.up);
		ProfileData.SetVector3(TEST_VECTOR3 + "_3", Vector3.forward);

		ProfileData.SetColor(TEST_COLOR, Color.white);
		ProfileData.SetColor(TEST_COLOR + "_1", Color.red);
		ProfileData.SetColor(TEST_COLOR + "_2", Color.green);
		ProfileData.SetColor(TEST_COLOR + "_3", Color.blue);
	}

	void printValues()
	{
		Debug.Log("int \"" + TEST_INT + "\" = " + ProfileData.GetInt(TEST_INT, -1));
		Debug.Log("int \"" + TEST_INT + "_1\" = " + ProfileData.GetInt(TEST_INT + "_1", -1));
		Debug.Log("int \"" + TEST_INT + "_2\" = " + ProfileData.GetInt(TEST_INT + "_2", -1));
		Debug.Log("int \"" + TEST_INT + "_3\" = " + ProfileData.GetInt(TEST_INT + "_3", -1));

		Debug.Log("float \"" + TEST_FLOAT + "\" = " + ProfileData.GetFloat(TEST_FLOAT, -1f));
		Debug.Log("float \"" + TEST_FLOAT + "_1\" = " + ProfileData.GetFloat(TEST_FLOAT + "_1", -1f));
		Debug.Log("float \"" + TEST_FLOAT + "_2\" = " + ProfileData.GetFloat(TEST_FLOAT + "_2", -1f));
		Debug.Log("float \"" + TEST_FLOAT + "_3\" = " + ProfileData.GetFloat(TEST_FLOAT + "_3", -1f));

		Debug.Log("string \"" + TEST_STRING + "\" = " + ProfileData.GetString(TEST_STRING, "fail"));
		Debug.Log("string \"" + TEST_STRING + "_1\" = " + ProfileData.GetString(TEST_STRING + "_1", "fail"));
		Debug.Log("string \"" + TEST_STRING + "_2\" = " + ProfileData.GetString(TEST_STRING + "_2", "fail"));
		Debug.Log("string \"" + TEST_STRING + "_3\" = " + ProfileData.GetString(TEST_STRING + "_3", "fail"));

		Debug.Log("Vector3 \"" + TEST_VECTOR3 + "\" = " + ProfileData.GetVector3(TEST_VECTOR3, Vector3.zero));
		Debug.Log("Vector3 \"" + TEST_VECTOR3 + "_1\" = " + ProfileData.GetVector3(TEST_VECTOR3 + "_1", Vector3.zero));
		Debug.Log("Vector3 \"" + TEST_VECTOR3 + "_2\" = " + ProfileData.GetVector3(TEST_VECTOR3 + "_2", Vector3.zero));
		Debug.Log("Vector3 \"" + TEST_VECTOR3 + "_3\" = " + ProfileData.GetVector3(TEST_VECTOR3 + "_3", Vector3.zero));

		Debug.Log("Color \"" + TEST_COLOR + "\" = " + ProfileData.GetColor(TEST_COLOR, Color.black));
		Debug.Log("Color \"" + TEST_COLOR + "_1\" = " + ProfileData.GetColor(TEST_COLOR + "_1", Color.black));
		Debug.Log("Color \"" + TEST_COLOR + "_2\" = " + ProfileData.GetColor(TEST_COLOR + "_2", Color.black));
		Debug.Log("Color \"" + TEST_COLOR + "_3\" = " + ProfileData.GetColor(TEST_COLOR + "_3", Color.black));
	}


	void handleIntChanged(string key, int value)
	{
		Debug.Log("handleIntChanged called with key=" + key + " and value=" + value);
	}

	void handleFloatChanged(string key, float value)
	{
		Debug.Log("handleFloatChanged called with key=" + key + " and value=" + value);
	}

	void handleStringChanged(string key, string value)
	{
		Debug.Log("handleStringChanged called with key=" + key + " and value=" + value);
	}

	void handleVector3Changed(string key, Vector3 value)
	{
		Debug.Log("handleVector3Changed called with key=" + key + " and value=" + value);
	}

	void handleColorChanged(string key, Color value)
	{
		Debug.Log("handleColorChanged called with key=" + key + " and value=" + value);
	}

	void handleSaved()
	{

	}

	void handleLoaded()
	{

	}

	void handleCleared()
	{

	}
}
