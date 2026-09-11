using UnityEngine;
using UnityEngine.UI;

public class vidaPlayer : MonoBehaviour
{
    [Header("Atributos")]
    public int vidaMaxima = 100;

    private int vidaAtual;

    private bool estaMorto = false;

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
        if (estaMorto)
            return;

        if (playerMove != null &&
            playerMove.isInvulneravel)
        {
            return;
        }

        vidaAtual -= quantidadeDeDano;

        vidaAtual =
            Mathf.Clamp(
                vidaAtual,
                0,
                vidaMaxima
            );

        Debug.Log(
            "Vida atual: " +
            vidaAtual
        );

        AtualizarBarraVida();

        if (vidaAtual <= 0)
        {
            Morrer();
        }
    }

    private void Morrer()
    {
        if (estaMorto)
            return;

        estaMorto = true;

        Debug.Log(
            "O jogador morreu!"
        );

        if (DeathManager.Instance != null)
        {
            DeathManager.Instance.MostrarTelaDeMorte();
        }
    }

    public void Reviver(float porcentagemVida)
    {
        vidaAtual =
            Mathf.RoundToInt(
                vidaMaxima *
                porcentagemVida
            );

        vidaAtual =
            Mathf.Clamp(
                vidaAtual,
                1,
                vidaMaxima
            );

        estaMorto = false;

        AtualizarBarraVida();

        Debug.Log(
            "Player reviveu com " +
            vidaAtual +
            "/" +
            vidaMaxima +
            " de vida."
        );
    }

    private void AtualizarBarraVida()
    {
        if (barraDeVida == null)
        {
            Debug.LogError(
                "A barraDeVida NÃO está atribuída!"
            );

            return;
        }

        barraDeVida.minValue = 0;
        barraDeVida.maxValue = vidaMaxima;
        barraDeVida.value = vidaAtual;
    }
}