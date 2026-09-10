using TMPro;
using UnityEngine;

public class CoinUI : MonoBehaviour
{
    [Header("Interface")]
    public TMP_Text textoMoedas;

    private void Start()
    {
        AtualizarSaldoInicial();

        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.OnMoedasAlteradas += AtualizarTexto;
        }
    }

    private void OnDestroy()
    {
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.OnMoedasAlteradas -= AtualizarTexto;
        }
    }

    private void AtualizarSaldoInicial()
    {
        if (CoinManager.Instance == null)
        {
            Debug.LogWarning("CoinManager não encontrado.");
            return;
        }

        AtualizarTexto(
            CoinManager.Instance.ObterMoedas()
        );
    }

    private void AtualizarTexto(int quantidade)
    {
        if (textoMoedas == null)
        {
            Debug.LogWarning(
                "CoinUI: textoMoedas não foi configurado."
            );

            return;
        }

        textoMoedas.text = quantidade.ToString("000");
    }
}