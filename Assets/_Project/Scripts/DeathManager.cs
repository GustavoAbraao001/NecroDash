using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathManager : MonoBehaviour
{
    public static DeathManager Instance;

    [Header("Interface")]
    public GameObject painelMorte;

    [Header("Player")]
    public vidaPlayer playerVida;

    [Range(0.1f, 1f)]
    public float porcentagemVidaAoReviver = 0.5f;

    public float tempoInvulnerabilidadeRevive = 2f;

    private bool podeReviver = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (playerVida == null)
        {
            GameObject player =
                GameObject.FindWithTag("Player");

            if (player != null)
            {
                playerVida =
                    player.GetComponent<vidaPlayer>();
            }
        }
    }

    public void MostrarTelaDeMorte()
    {
        podeReviver = true;

        if (painelMorte != null)
        {
            painelMorte.SetActive(true);
        }

        Debug.Log("Tela de morte aberta.");
    }

    public void VoltarAoMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("MenuScene");
    }

    public void Reviver()
    {
        if (!podeReviver)
            return;

        podeReviver = false;

        Time.timeScale = 1f;

        if (playerVida != null)
        {
            playerVida.Reviver(
                porcentagemVidaAoReviver
            );
        }

        if (painelMorte != null)
        {
            painelMorte.SetActive(false);
        }

        StartCoroutine(
            InvulnerabilidadeRevive()
        );

        Debug.Log("PLAYER REVIVIDO!");
    }

    private IEnumerator InvulnerabilidadeRevive()
    {
        if (playerVida == null)
            yield break;

        PlayerMove playerMove =
            playerVida.GetComponent<PlayerMove>();

        if (playerMove == null)
            yield break;

        playerMove.isInvulneravel = true;

        yield return new WaitForSeconds(
            tempoInvulnerabilidadeRevive
        );

        playerMove.isInvulneravel = false;
    }

    public void ReviverTeste()
    {
        Reviver();
    }
}