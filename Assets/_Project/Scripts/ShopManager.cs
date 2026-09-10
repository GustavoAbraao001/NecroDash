using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [Header("Duplo Dash")]
    public int precoDuploDash = 20;

    [Header("Interface")]
    public TMP_Text textoPrincipal;
    public TMP_Text textoPreco;
    public TMP_Text textoMensagem;

    private void Start()
    {
        AtualizarInterface();
    }

    private void AtualizarInterface()
    {
        if (CoinManager.Instance == null)
        {
            Debug.LogError("CoinManager não encontrado!");
            return;
        }

        if (textoPreco != null)
        {
            textoPreco.text = "Preço: " + precoDuploDash;
        }

        if (CoinManager.Instance.PossuiDuploDash())
        {
            if (textoPrincipal != null)
            {
                textoPrincipal.text = "COMPRADO";
            }
        }
        else
        {
            if (textoPrincipal != null)
            {
                textoPrincipal.text = "DUPLO DASH";
            }
        }
    }

    public void ComprarDuploDash()
    {
        if (CoinManager.Instance == null)
        {
            Debug.LogError("CoinManager não encontrado!");
            return;
        }

        if (CoinManager.Instance.PossuiDuploDash())
        {
            MostrarMensagem("Você já possui o Duplo Dash!");
            return;
        }

        bool sucesso =
            CoinManager.Instance.ComprarDuploDash(precoDuploDash);

        if (sucesso)
        {
            MostrarMensagem("Duplo Dash comprado!");
        }
        else
        {
            MostrarMensagem("Moedas insuficientes!");
        }

        AtualizarInterface();
    }

    private void MostrarMensagem(string mensagem)
    {
        if (textoMensagem != null)
        {
            textoMensagem.text = mensagem;
        }

        Debug.Log(mensagem);
    }

    public void VoltarAoMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuScene");
    }
}