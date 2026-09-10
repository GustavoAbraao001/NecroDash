using UnityEngine;
using UnityEngine.UI;

public class vidaPlayer : MonoBehaviour
{
    [Header("Atributos")]
    public int vidaMaxima = 100;

    private int vidaAtual;

    [Header("Interface")]
    public Slider barraDeVida;

    private PlayerMove playerMove;

    private void Awake()
    {
        playerMove = GetComponent<PlayerMove>();
        vidaAtual = vidaMaxima;
    }

    private void Start()
    {
        AtualizarBarraVida();
    }

    public void TomarDano(int quantidadeDeDano)
    {
        if (playerMove != null && playerMove.isInvulneravel)
            return;

        vidaAtual -= quantidadeDeDano;
        vidaAtual = Mathf.Clamp(vidaAtual, 0, vidaMaxima);

        Debug.Log("Vida atual: " + vidaAtual);

        AtualizarBarraVida();

        if (vidaAtual <= 0)
        {
            Morrer();
        }
    }

    private void AtualizarBarraVida()
    {
        if (barraDeVida == null)
        {
            Debug.LogError("A barraDeVida NÃO está atribuída!");
            return;
        }

        barraDeVida.minValue = 0;
        barraDeVida.maxValue = vidaMaxima;
        barraDeVida.value = vidaAtual;

        Debug.Log(
            "Slider atualizado: " +
            barraDeVida.value +
            "/" +
            barraDeVida.maxValue
        );
    }

    private void Morrer()
    {
        Debug.Log("O jogador morreu!");

        if (DeathManager.Instance != null)
        {
            DeathManager.Instance.MostrarTelaDeMorte();
        }
        else
        {
            Time.timeScale = 0f;
        }
    }
}
