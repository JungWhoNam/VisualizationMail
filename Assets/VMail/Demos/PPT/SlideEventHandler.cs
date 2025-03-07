using UnityEngine;

public class SlideEventHandler : MonoBehaviour
{
    [SerializeField] private NetMQClient netMQClient;
    [SerializeField] private DummyVis dummyVis;
    [SerializeField] private int colorCount = 10; // Number of colors to generate

    private Color[] colors;

    void Start()
    {
        if (netMQClient == null)
        {
            Debug.LogWarning("⚠️ NetMQClient is not assigned!");
            return;
        }

        if (dummyVis == null)
        {
            Debug.LogWarning("⚠️ DummyVis is not assigned!");
            return;
        }

        netMQClient.OnSlideChanged.AddListener(OnSlideChanged);
        InitializeColors();
    }

    private void InitializeColors()
    {
        colors = new Color[colorCount];

        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Random.ColorHSV();
        }
    }

    private void OnSlideChanged(int slideNumber)
    {
        if (slideNumber < 0) return; // Ignore invalid slide numbers

        Debug.Log($"🎯 Slide Change Detected in Unity: {slideNumber}");
        dummyVis.SetColor(colors[slideNumber % colors.Length]);
    }
}
