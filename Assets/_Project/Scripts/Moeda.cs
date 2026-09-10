using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Valor")]
    public int valor = 1;

    [Header("Flutuação")]
    public float alturaFlutuacao = 0.15f;
    public float velocidadeFlutuacao = 2f;

    [Header("Rotação")]
    public float velocidadeRotacao = 40f;

    [Header("Coleta")]
    public float raioColeta = 1.5f;
    public float tempoAntesDeColetar = 0.8f;

    private Vector3 posicaoInicial;
    private Transform player;

    private float tempoVida = 0f;
    private bool coletada = false;

    private void Start()
    {
        posicaoInicial = transform.position;

        GameObject playerObj = GameObject.FindWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    private void Update()
    {
        tempoVida += Time.deltaTime;

        Flutuar();
        Girar();

        // Ainda não pode ser coletada
        if (tempoVida < tempoAntesDeColetar)
            return;

        VerificarColeta();
    }

    private void Flutuar()
    {
        float novaAltura =
            Mathf.Sin(Time.time * velocidadeFlutuacao) *
            alturaFlutuacao;

        transform.position =
            posicaoInicial + Vector3.up * novaAltura;
    }

    private void Girar()
    {
        transform.Rotate(
           0f,
           0f,
           velocidadeRotacao * Time.deltaTime
        
       );
    }

    private void VerificarColeta()
    {
        if (coletada || player == null)
            return;

        float distancia =
            Vector3.Distance(transform.position, player.position);

        if (distancia <= raioColeta)
        {
            Coletar();
        }
    }

    private void Coletar()
    {
        if (coletada)
            return;

        coletada = true;

        CoinManager.Instance.AdicionarMoedas(valor);

        Destroy(gameObject);
    }
}



