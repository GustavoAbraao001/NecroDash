using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Cenas")]
    public string cenaJogo = "Jogo";
    public string cenaLoja = "Loja";

    public void Jogar()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(cenaJogo);
    }

    public void AbrirLoja()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(cenaLoja);
    }

    public void SairDoJogo()
    {
        Application.Quit();
    }
}