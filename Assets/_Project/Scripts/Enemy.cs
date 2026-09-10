using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    private Transform player;
    private NavMeshAgent agent;

  

    [Header("Recompensas e Combate")]
    public int danoAoPlayer = 10;
    public float tempoEntreAtaques = 1f;

    [Header("Drop de Moeda")]
    public GameObject prefabMoeda;
    public float alturaDropMoeda = 0.8f;

    private float tempoUltimoAtaque = 0f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        GameObject playerObj = GameObject.FindWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    private void OnEnable()
    {
        if (agent != null)
            agent.enabled = true;

        InvokeRepeating(nameof(AtualizarRota), 0f, 0.2f);
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(AtualizarRota));
    }

    void AtualizarRota()
    {
        if (player != null &&
            agent != null &&
            agent.enabled &&
            agent.isOnNavMesh)
        {
            agent.SetDestination(player.position);
        }
    }

    public void ReceberDanoFatal()
    {
        Debug.Log("Inimigo atingido pelo DASH!");

        if (prefabMoeda != null)
        {
            Vector3 posicaoDrop =
            transform.position + Vector3.up * 0.5f;

            Vector3 posicaoMoeda =
             transform.position + Vector3.up * alturaDropMoeda;

            Instantiate(
                prefabMoeda,
                posicaoMoeda,
                Quaternion.Euler(-90f, 0f, 0f)
            );

        }

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.enabled = false;
        }

        gameObject.SetActive(false);
    }
    private void OnTriggerStay(Collider outro)
    {
        if (!outro.CompareTag("Player"))
            return;

        if (Time.time < tempoUltimoAtaque + tempoEntreAtaques)
            return;

        vidaPlayer vidaDoPlayer =
            outro.GetComponentInParent<vidaPlayer>();

        if (vidaDoPlayer != null)
        {
            vidaDoPlayer.TomarDano(danoAoPlayer);

            tempoUltimoAtaque = Time.time;

            Debug.Log("Inimigo atacou o jogador!");
        }
    }
}