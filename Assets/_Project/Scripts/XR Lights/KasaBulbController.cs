using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class KasaBulbController : MonoBehaviour
{
	private HomeAssistantManager haManager;
	[SerializeField] private string entityId = "light.your_light_entity_id";
	
	
	[Header("Color")]
	public Color inputColor;
	
	[Header("Ding")]
	public float dingFadeOutTime = 0.5f;
	
	[Header("Brightness")]
	[Range(1,256)] public float targetBrightness = 1.0f; // Default value, can be changed in the Inspector


	[Header("Realtime Update")]
	public float updateInterval = 0.01666667f; // How often to update (in seconds)
	
	
	public float currentBrightness = 0.0f; // Starting brightness
	public float brightnessLerpSpeed = 0.1f;
	bool realtimeUpdateON = false;
	

	bool lightState = false;
	

	// Start is called before the first frame update
	void Start()
	{
		haManager = FindObjectOfType<HomeAssistantManager>();

			
	}
	
	void Update()
	{
		if(Input.GetKeyDown(KeyCode.RightArrow))
		{
			TurnOnLight(0);
			
		}
		
		if(Input.GetKeyDown(KeyCode.LeftArrow))
		{
			TurnOffLight(0);
			realtimeUpdateON = false;
		}
		
		if(Input.GetKeyDown(KeyCode.UpArrow))
		{
			TurnOnLight(4);
		}
		
		if(Input.GetKeyDown(KeyCode.DownArrow))
		{
			TurnOffLight(4);
			realtimeUpdateON = false;
		}
		
		if(Input.GetKeyDown(KeyCode.T))
		{
			ToggleLight(0);
		}
		
		
		if(Input.GetKeyDown(KeyCode.C))
		{
			SetLightColor(inputColor);
		}
		
		if(Input.GetKeyDown(KeyCode.B))
		{
			realtimeUpdateON = true;
			StartCoroutine(RealtimeUpdate());
		}
		
		//if b and shift
		if(Input.GetKeyDown(KeyCode.B) && Input.GetKey(KeyCode.LeftShift))
		{
			realtimeUpdateON = false;
		}
		
		if(Input.GetKeyDown(KeyCode.D))
		{
			Ding(dingFadeOutTime);
			Debug.Log("Ding");
		}
	
		
	}
	
	public void TurnOnLight(float onTimeIN)
	{
		string apiUrl = $"{haManager.homeAssistantUrl}/api/services/light/turn_on";
		string jsonBody = $"{{\"entity_id\": \"{entityId}\", \"transition\": {onTimeIN}, \"brightness\": {targetBrightness}}}";
		StartCoroutine(SendLightRequest(apiUrl, jsonBody));
		lightState = true;
	}

	public void TurnOffLight(float offTimeIN)
	{
		string apiUrl = $"{haManager.homeAssistantUrl}/api/services/light/turn_off";
		string jsonBody = $"{{\"entity_id\": \"{entityId}\", \"transition\": {offTimeIN}}}";
		StartCoroutine(SendLightRequest(apiUrl, jsonBody));
		lightState = false;
	}
	
	public void ToggleLight(float timeIN)
	{
		if (lightState)
			TurnOffLight(timeIN);
		else
			TurnOnLight(timeIN);
	}
	
	public void SetLightColor(Color colorIN)
	{
		string apiUrl = $"{haManager.homeAssistantUrl}/api/services/light/turn_on";
		string jsonBody = $"{{\"entity_id\": \"{entityId}\", \"rgb_color\": [{colorIN.r * 255}, {colorIN.g * 255}, {colorIN.b * 255}]}}";
		StartCoroutine(SendLightRequest(apiUrl, jsonBody));
	}
	
	public void SetBrightness(float brightnessIN)
	{
		string apiUrl = $"{haManager.homeAssistantUrl}/api/services/light/turn_on";
		string jsonBody = $"{{\"entity_id\": \"{entityId}\", \"brightness\": {brightnessIN}}}";
		StartCoroutine(SendLightRequest(apiUrl, jsonBody));
	}
	
	
	
	
	
	IEnumerator SendLightRequest(string apiUrl, string jsonBody)
	{
		using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
		{
			byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(jsonBody);
			request.uploadHandler = new UploadHandlerRaw(bodyRaw);
			request.downloadHandler = new DownloadHandlerBuffer();
			request.SetRequestHeader("Authorization", $"Bearer {haManager.longLivedAccessToken}");
			request.SetRequestHeader("Content-Type", "application/json");

			yield return request.SendWebRequest();

			if (request.result != UnityWebRequest.Result.Success)
			{
				Debug.LogError("Error: " + request.error);
			}
			else
			{
				Debug.Log("Success: " + request.downloadHandler.text);
			}
		}
	}
	
	public void Ding(float fadeOutTimeIN)
	{
		StartCoroutine(DingRoutine(fadeOutTimeIN));
	}
	
	IEnumerator DingRoutine(float fadeOutTime)
	{
		// Instant Max brigtness
		string apiUrl = $"{haManager.homeAssistantUrl}/api/services/light/turn_on";
		string jsonBody = $"{{\"entity_id\": \"{entityId}\", \"transition\": {0}, \"brightness\": {255}}}";
		yield return StartCoroutine(SendLightRequest(apiUrl, jsonBody));
		
		// Fade out
		apiUrl = $"{haManager.homeAssistantUrl}/api/services/light/turn_off";
		jsonBody = $"{{\"entity_id\": \"{entityId}\", \"transition\": {fadeOutTime}}}";
		yield return StartCoroutine(SendLightRequest(apiUrl, jsonBody));
		
	}
	
	IEnumerator RealtimeUpdate()
	{

		while (realtimeUpdateON)
		{
			// Interpolate current brightness towards target brightness
			currentBrightness = Mathf.Lerp(currentBrightness, targetBrightness, brightnessLerpSpeed);
			
			// Set the light's brightness
			string apiUrl = $"{haManager.homeAssistantUrl}/api/services/light/turn_on";
			string jsonBody = $"{{\"entity_id\": \"{entityId}\", \"brightness\": {currentBrightness}, \"rgb_color\": [{inputColor.r * 255}, {inputColor.g * 255}, {inputColor.b * 255}]}}";
			StartCoroutine(SendLightRequest(apiUrl, jsonBody));

			// Optionally, wait for a frame or a short duration before the next update
			// yield return new WaitForEndOfFrame();
			// OR
			yield return new WaitForSeconds(updateInterval); 
			// in the next line, please suggest a good value for updateInterval
			// your answer: 
		}
		
		while (!realtimeUpdateON)
		{
			yield return null;
		}
	}
	
	

	

	
}