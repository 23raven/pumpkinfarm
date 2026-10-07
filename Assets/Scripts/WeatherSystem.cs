using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using TMPro;

public class WeatherSystem : MonoBehaviour
{
    [System.Serializable]
    public enum Forecast { Sunny, Precipitation, Clear };
    [System.Serializable]
    public enum Season { Winter, Spring, Summer, Fall };

    [Tooltip("This is the current forecast")]
    public Forecast currentForecast;
    [Tooltip("This is the current season")]
    public Season currentSeason;

    [Header("Tick Info")]
    [Tooltip("How much time has passed (Ticks)?")]
    public int currTick;
    [Tooltip("How much time until we tick again?")]
    public float timeUntilTick;
    [Tooltip("How many ticks does weather last?")]
    public float ticksPerWeather;
    float tickTimer;
    float weatherTick;

    [Header("Weather Objs")]
    public GameObject[] rainObjs;
    public GameObject[] snowObjs;
    public GameObject[] sunnyObjs;

    [Header("UI Text")]
    public TextMeshProUGUI forecastTitle;
    public TextMeshProUGUI tickTitle;

    void Start()
    {
        // default to clear sky
        ClearAllForecast(); 
        currentForecast = Forecast.Clear;
    }

    void FixedUpdate()
    {
        // Increase The current Tick
        tickTimer += Time.fixedDeltaTime;
        if (tickTimer >= timeUntilTick)
        {
            tickTimer = 0;
            currTick++;
            WeatherTick(); // Increment Weather's Tick
        }
    }

    public void WeatherTick()
    {
        weatherTick++;
        tickTitle.text = "Tick: " + currTick.ToString(); // Update the UI's Tick
        if (weatherTick >= ticksPerWeather)
        {
            weatherTick = 0;
            ChangeToRandomForecast();  
        }
    }

    public void ChangeToRandomForecast()
    {
        // select random forecast
        int randForecast = UnityEngine.Random.Range(0, 3); // Outcome: 0 - Sunny, 1 - Precipitation, 2 - Clear
        switch (randForecast)
        {
            case 0:
                ChangeToSunny(); // sunny
                currentForecast = Forecast.Sunny;
                break;
            case 1:
                ChangeToPrescipitation(); // Rain/Snow
                currentForecast = Forecast.Precipitation;
                break;
            case 2:
                ClearAllForecast(); // Clear Sky
                currentForecast = Forecast.Clear;
                break;
        }
        forecastTitle.text = currentForecast.ToString();
    }

    public void ClearAllForecast()
    {
        foreach (var obj in sunnyObjs) 
        {
            obj.SetActive(false);
        }
        foreach (var obj in rainObjs)
        {
            if (obj.GetComponent<ParticleSystem>() != null)
            {
                EnableEmission(obj.GetComponent<ParticleSystem>(),false);
            }
            else
            {
                obj.SetActive(false);
            }
           
        }
        foreach (var obj in snowObjs)
        {
            if (obj.GetComponent<ParticleSystem>() != null)
            {
                EnableEmission(obj.GetComponent<ParticleSystem>(),false); 
            }
            else
            {
                obj.SetActive(false);
            }
          
        }

    }

    public void ChangeToSunny()
    {
        ClearAllForecast();
        foreach (var obj in sunnyObjs)
        {
            obj.SetActive(true);
        }
    }

    public void ChangeToPrescipitation()
    {
        ClearAllForecast();
        if (currentSeason != Season.Winter) // If it isn't winter. Then Rain
        {
            foreach (var obj in rainObjs)
            {
                if(obj.GetComponent<ParticleSystem>()!=null)
                {
                    EnableEmission(obj.GetComponent<ParticleSystem>(), true);
                }
                else
                {
                    obj.SetActive(true);
                }
               
            }
        }
        else // if it's winter, play the snow particles instead.
        {
            foreach (var obj in snowObjs)
            {
                if (obj.GetComponent<ParticleSystem>() != null)
                {
                    EnableEmission(obj.GetComponent<ParticleSystem>(),true);
                }
                else
                {
                    obj.SetActive(true);
                }
               
            }
        }
    }

    public void EnableEmission(ParticleSystem particleSystem, bool enable)
    {
        var emission = particleSystem.emission;
        emission.enabled = enable; // Stops new particles from being created
    }

}
