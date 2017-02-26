using UnityEngine;
using System.Collections;

public class ProfileDataProxy : MonoBehaviour
{
	[SerializeField] string _key;
	public string key { get { return _key; } }

	public void SetInt(int value) { ProfileData.SetInt(_key, value); }
	public void IncInt() { ProfileData.SetInt(_key, ProfileData.GetInt(_key) + 1); }
	public void IncInt(int amount) { ProfileData.SetInt(_key, ProfileData.GetInt(_key) + amount); }
	public void DecInt() { ProfileData.SetInt(_key, ProfileData.GetInt(_key) - 1); }
	public void DecInt(int amount) { ProfileData.SetInt(_key, ProfileData.GetInt(_key) - amount); }

	public void SetFloat(float value) { ProfileData.SetFloat(_key, value); }
	public void SetString(string value) { ProfileData.SetString(_key, value); }
	public void SetVector3(Vector3 value) { ProfileData.SetVector3(_key, value); }
	public void SetColor(Color value) { ProfileData.SetColor(_key, value); }

}
