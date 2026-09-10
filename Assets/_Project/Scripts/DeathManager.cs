using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathManager : MonoBehaviour
{
    public static DeathManager Instance;

    [Header("Interface")]
    public GameObject painelMorte;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void MostrarTelaDeMorte()
    {
        Time.timeScale = 0f;

        if (painelMorte != null)
        {
            painelMorte.SetActive(true);
        }
    }

    public void VoltarAoMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("MenuScene");
    }

    // TEMPORÁRIO
    // Futuramente será chamado após Rewarded Ad
    public void ReviverTeste()
    {
        Time.timeScale = 1f;

        Debug.Log("REVIVER TESTE");

        SceneManager.LoadScene("JogoScene");
    }
}