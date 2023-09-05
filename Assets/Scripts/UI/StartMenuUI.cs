using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button firingRangeButton;
    [SerializeField] private Button forestButton;

    private void Awake()
    {
        firingRangeButton.onClick.AddListener(() =>
        {
            Loader.Load(Loader.Scene.FiringRange);
        });
        forestButton.onClick.AddListener(() =>
        {
            Loader.Load(Loader.Scene.ForestDemoScene);
        });

        Time.timeScale = 1f;
    }
}
