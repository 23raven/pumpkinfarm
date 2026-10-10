using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    public weather weather;
    public int chance;

    [SerializeField] private GameObject SunnyLayer;
    [SerializeField] private GameObject RainyLayer;
    [SerializeField] private GameObject RainyParticles;

    public void Update()
    {
        int ran = Random.Range(0, chance);

        if (ran == 1)
        {
            System.Array values = System.Enum.GetValues(typeof(weather));

            int randomIndex = Random.Range(0, values.Length);

            weather = (weather)values.GetValue(randomIndex);

            Clear();

            if (weather == weather.Sunny)
            {
                SunnyLayer.SetActive(true);
            }
            else if (weather == weather.Rain)
            {
                RainyLayer.SetActive(true);
                RainyParticles.SetActive(true);
            }
        }
    }

    private void Clear()
    {
        SunnyLayer.SetActive(false);
        RainyLayer.SetActive(false);
        RainyParticles.SetActive(false);
    }
}

public enum weather
{
    Sunny,
    Clear,
    Rain
}
