using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CloudsManager : MonoBehaviour {

    private class CloudColor
    {
        public static void SetRainyColor(CloudsToy settings)
        {
            settings.CloudPreset = CloudsToy.TypePreset.Stormy;
            settings.TypeClouds = CloudsToy.Type.MixNimbus;
            settings.CloudColor = new Color(0.7f, 0.7f, 0.7f, 0.8f);
            settings.MainColor = new Color(0.6f, 0.6f, 0.6f, 0.8f);
            settings.SecondColor = new Color(0.54f, 0.53f, 0.53f, 0.8f);
        }

        public static void SetStormyColor(CloudsToy settings)
        {
            settings.CloudPreset = CloudsToy.TypePreset.Stormy;
            settings.TypeClouds = CloudsToy.Type.Cirrus1;
            settings.CloudColor = new Color(0.9f, 0.9f, 0.9f, 0.8f);
            settings.MainColor = new Color(0.62f, 0.62f, 0.62f, 0.8f);
            settings.SecondColor = new Color(0.45f, 0.45f, 0.45f, 0.8f);
        }
        public static void SetCloudyColor(CloudsToy settings)
        {
            settings.CloudPreset = CloudsToy.TypePreset.Stormy;
            settings.TypeClouds = CloudsToy.Type.MixCirrus;
            settings.CloudColor = new Color(0.71f, 0.73f, 0.74f, 0.9f);
            settings.MainColor = new Color(0.61f, 0.63f, 0.64f, 0.9f);
            settings.SecondColor = new Color(0.71f, 0.71f, 0.74f, 0.9f);
        }
    }

    private void setCloudSize(CloudsToy settings, CloudSize size)
    {
        switch (size)
        {
            case CloudSize.Small:
                settings.EmissionMult = 0.1f;
                settings.SizeFactorPart = 0.25f;
                break;
            case CloudSize.Medium:
                settings.EmissionMult = 0.35f;
                settings.SizeFactorPart = 0.5f;
                settings.MaxWithCloud = 1000;
                settings.MaxDepthCloud = 1000;
                settings.MaxTallCloud = 70;
                break;
            case CloudSize.Large:
                settings.EmissionMult = 0.5f;
                settings.SizeFactorPart = 0.6f;
                settings.MaxWithCloud = 2500;
                settings.MaxDepthCloud = 2500;
                settings.MaxTallCloud = 800;
                break;
            case CloudSize.XXL:
                settings.MaxWithCloud = 1350;
                settings.MaxDepthCloud = 1950;
                settings.MaxTallCloud = 10;
                settings.EmissionMult = 1.5f;
                settings.SizeFactorPart = 1.5f;
            break;
        }
    }

	// Different settings for cloud systems.
	//public enum Weather  {Sunny, Cloudy, Stormy, Rainy};
	public int weather = 0; 
    private enum CloudSize { Small, Medium, Large, XXL };
	//public Weather weather = Weather.Sunny;

    private GameObject topLevel;
    private GameObject midLevel;
    private GameObject mountainLevel;
	
	private Netz netz;
	private Player player;
	private int weatherByNetwork = 0;

    private CloudsToy GetSettingsFrom(GameObject obj)
    {
        return obj.GetComponentInChildren<CloudsToy>();
    }

    private void setSunny()
    {
        topLevel.SetActive(false);
        midLevel.SetActive(false);
        mountainLevel.SetActive(false);
    }

    private void setCloudy()
    {
        topLevel.SetActive(true);
        CloudsToy topLevelSettings = GetSettingsFrom(topLevel);
        setCloudSize(topLevelSettings, CloudSize.Medium);
        CloudColor.SetCloudyColor(topLevelSettings);
        topLevelSettings.NumberClouds = 66;

        midLevel.SetActive(false);
        mountainLevel.SetActive(false);
    }

    private void setStormy()
    {
        topLevel.SetActive(true);
        CloudsToy topLevelScript = GetSettingsFrom(topLevel);
        topLevelScript.NumberClouds = 80;
        setCloudSize(topLevelScript, CloudSize.Large);
        CloudColor.SetStormyColor(topLevelScript);

        midLevel.SetActive(false);
        CloudsToy midLevelScript = GetSettingsFrom(midLevel);
        midLevelScript.NumberClouds = 80;
        setCloudSize(midLevelScript, CloudSize.Small);
        CloudColor.SetStormyColor(midLevelScript);

        mountainLevel.SetActive(false);

    }

    private void setRainy()
    {
        topLevel.SetActive(true);

        const int RAINY_CLOUD_AMOUNT = 150;

        CloudsToy topLevelScript = GetSettingsFrom(topLevel);
        topLevelScript.NumberClouds = RAINY_CLOUD_AMOUNT;
        setCloudSize(topLevelScript, CloudSize.XXL);
        CloudColor.SetRainyColor(topLevelScript);

        midLevel.SetActive(true);
        CloudsToy midLevelScript = GetSettingsFrom(midLevel);
        midLevelScript.NumberClouds = RAINY_CLOUD_AMOUNT / 5;
        setCloudSize(midLevelScript, CloudSize.Large);
        CloudColor.SetRainyColor(midLevelScript);

        mountainLevel.SetActive(true);
    }

	void Start () {

        topLevel = GameObject.Find ("TopLevel");
		midLevel = GameObject.Find ("MidLevel");
		mountainLevel = GameObject.Find ("MountainClouds");
		
		// Find networkManager
		netz = (Netz)GameObject.FindGameObjectWithTag("Network").GetComponent("Netz");

		// Default and initial weather is sunny. 
		/*switch(weather) {
			// Only skydome clouds.
		case Weather.Sunny :
                setSunny();
			break;
			// Top and midLevel clouds.
		case Weather.Cloudy :
            setCloudy();
			break;
			// All cloud systems.
		case Weather.Stormy :
            setStormy();
			break;
        case Weather.Rainy:
            setRainy();
            break;
		default: weather = Weather.Sunny;
			break;
		}*/
		
		switch(weather) {
			// Only skydome clouds.
		case 0 :
                setSunny();
			break;
			// Top and midLevel clouds.
		case 1 :
            setCloudy();
			break;
			// All cloud systems.
		case 2 :
            setStormy();
			break;
        case 3:
            setRainy();
            break;
		default: weather = 0;
			break;
		}
	}

	void Update () {
		if (netz.serverJoined) {
			try {
				player = (Player)GameObject.FindGameObjectWithTag ("Player").GetComponent("Player");
				weatherByNetwork = player.finWeather; 
				Debug.Log(weatherByNetwork);
				
				switch(weatherByNetwork) {
						// Only skydome clouds.
					case 0 :
							setSunny();
						break;
						// Top and midLevel clouds.
					case 1 :
						setCloudy();
						break;
						// All cloud systems.
					case 2 :
						setStormy();
						break;
					case 3:
						setRainy();
						break;
					default: weatherByNetwork = 0;
						break;
					}
				} catch (UnityException e) {
					Debug.Log(e.Message);
				}
			
			
			
		}
		
	}
}
