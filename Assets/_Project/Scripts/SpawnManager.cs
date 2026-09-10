using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpawnManager : MonoBehaviour
{
    // =========================================================
    // PREFABS
    // =========================================================

    [Header("Prefabs dos Inimigos")]
    public GameObject prefabCaveira;
    public GameObject prefabZumbi;

    // =========================================================
    // POOL
    // =========================================================

    [Header("Quantidade no Pool")]
    public int quantidadeCaveiras = 25;
    public int quantidadeZumbis = 25;

    private List<GameObject> poolCaveiras =
        new List<GameObject>();

    private List<GameObject> poolZumbis =
        new List<GameObject>();

    // =========================================================
    // PLAYER / DISTÂNCIAS
    // =========================================================

    [Header("Configurações de Spawn")]
    public Transform player;

    public float distanciaMinimaParaSpawn = 10f;

    public float distanciaMaximaParaSpawn = 25f;

    public float tempoEntreSpawns = 2f;

    // =========================================================
    // DISTRIBUIÇÃO DOS INIMIGOS
    // =========================================================

    [Header("Distribuição dos Inimigos")]
    [Range(0f, 1f)]
    public float chanceDeCaveira = 0.5f;

    // 0.5 = 50%
    // 0.7 = 70%
    // 0.3 = 30%

    // =========================================================
    // ESCALONAMENTO
    // =========================================================

    [Header("Escalonamento de Dificuldade")]
    public int inimigosIniciaisPorSpawn = 1;

    public int aumentoDeInimigos = 1;

    public float intervaloDeAumento = 30f;

    public int limiteDeInimigosPorSpawn = 10;

    // =========================================================
    // LIMITE DE INIMIGOS ATIVOS
    // =========================================================

    [Header("Limite de Inimigos Ativos")]
    public int limiteInimigosAtivos = 20;

    // =========================================================
    // CONTROLE DO TEMPO
    // =========================================================

    private float tempoDePartida = 0f;

    // =========================================================
    // INICIALIZAÇÃO
    // =========================================================

    private void Start()
    {
        CriarPool(
            prefabCaveira,
            quantidadeCaveiras,
            poolCaveiras
        );

        CriarPool(
            prefabZumbi,
            quantidadeZumbis,
            poolZumbis
        );

        InvokeRepeating(
            nameof(SpawnarInimigo),
            1f,
            tempoEntreSpawns
        );
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        tempoDePartida += Time.deltaTime;
    }

    // =========================================================
    // CRIAR POOL
    // =========================================================

    private void CriarPool(
        GameObject prefab,
        int quantidade,
        List<GameObject> pool
    )
    {
        if (prefab == null)
        {
            Debug.LogError(
                "SpawnManager: prefab não configurado!"
            );

            return;
        }

        for (int i = 0; i < quantidade; i++)
        {
            GameObject inimigo =
                Instantiate(prefab);

            inimigo.SetActive(false);

            pool.Add(inimigo);
        }
    }

    // =========================================================
    // SPAWN
    // =========================================================

    private void SpawnarInimigo()
    {
        Debug.Log("Tentando spawnar inimigo...");

        if (player == null)
        {
            Debug.LogWarning(
                "SpawnManager: Player não configurado!"
            );

            return;
        }

        // Verifica o limite de inimigos ativos
        int inimigosAtivos =
            ContarInimigosAtivos();

        if (inimigosAtivos >= limiteInimigosAtivos)
        {
            return;
        }

        // Descobre quantos inimigos devem nascer
        int quantidade =
            CalcularInimigosPorSpawn();

        // Não deixa ultrapassar o limite
        int espacoDisponivel =
            limiteInimigosAtivos - inimigosAtivos;

        quantidade =
            Mathf.Min(
                quantidade,
                espacoDisponivel
            );

        // Cria os inimigos
        for (int i = 0; i < quantidade; i++)
        {
            SpawnarUmInimigo();
        }
    }

    // =========================================================
    // SPAWN DE UM INIMIGO
    // =========================================================

    private void SpawnarUmInimigo()
    {
        // Decide qual tipo de inimigo será criado
        bool spawnarCaveira =
            Random.value <= chanceDeCaveira;

        GameObject inimigoDisponivel;

        // =====================================================
        // TENTA PEGAR O TIPO ESCOLHIDO
        // =====================================================

        if (spawnarCaveira)
        {
            inimigoDisponivel =
                ObterInimigoDoPool(poolCaveiras);
        }
        else
        {
            inimigoDisponivel =
                ObterInimigoDoPool(poolZumbis);
        }

        // =====================================================
        // SE NÃO EXISTIR DESSE TIPO,
        // TENTA O OUTRO
        // =====================================================

        if (inimigoDisponivel == null)
        {
            if (spawnarCaveira)
            {
                inimigoDisponivel =
                    ObterInimigoDoPool(poolZumbis);
            }
            else
            {
                inimigoDisponivel =
                    ObterInimigoDoPool(poolCaveiras);
            }
        }

        // Se não existe inimigo disponível
        if (inimigoDisponivel == null)
        {
            Debug.Log(
                "SpawnManager: pool de inimigos esgotado."
            );

            return;
        }

        // =====================================================
        // ENCONTRA POSIÇÃO
        // =====================================================

        Vector3 pontoValido;

        if (!EncontrarPontoAleatorioNoNavMesh(
            player.position,
            distanciaMaximaParaSpawn,
            out pontoValido))
        {
            return;
        }

        // =====================================================
        // CONFIRMA DISTÂNCIA MÍNIMA
        // =====================================================

        float distancia =
            Vector3.Distance(
                player.position,
                pontoValido
            );

        if (distancia < distanciaMinimaParaSpawn)
        {
            return;
        }

        // =====================================================
        // POSICIONA
        // =====================================================

        inimigoDisponivel.transform.position =
            pontoValido;

        inimigoDisponivel.transform.rotation =
            Quaternion.identity;

        // =====================================================
        // ATIVA
        // =====================================================

        inimigoDisponivel.SetActive(true);

        // =====================================================
        // NAVMESH AGENT
        // =====================================================

        NavMeshAgent agent =
            inimigoDisponivel.GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            if (!agent.enabled)
            {
                agent.enabled = true;
            }

            agent.Warp(pontoValido);
        }
    }

    // =========================================================
    // PEGAR INIMIGO DO POOL
    // =========================================================

    private GameObject ObterInimigoDoPool(
        List<GameObject> pool
    )
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].activeInHierarchy)
            {
                return pool[i];
            }
        }

        return null;
    }

    // =========================================================
    // CALCULAR QUANTIDADE DE INIMIGOS
    // =========================================================

    private int CalcularInimigosPorSpawn()
    {
        int nivel =
            Mathf.FloorToInt(
                tempoDePartida /
                intervaloDeAumento
            );

        int quantidade =
            inimigosIniciaisPorSpawn +
            (nivel * aumentoDeInimigos);

        quantidade =
            Mathf.Min(
                quantidade,
                limiteDeInimigosPorSpawn
            );

        return quantidade;
    }

    // =========================================================
    // CONTAR INIMIGOS ATIVOS
    // =========================================================

    private int ContarInimigosAtivos()
    {
        int quantidade = 0;

        // Caveiras
        for (int i = 0; i < poolCaveiras.Count; i++)
        {
            if (poolCaveiras[i].activeInHierarchy)
            {
                quantidade++;
            }
        }

        // Zumbis
        for (int i = 0; i < poolZumbis.Count; i++)
        {
            if (poolZumbis[i].activeInHierarchy)
            {
                quantidade++;
            }
        }

        return quantidade;
    }

    // =========================================================
    // ENCONTRAR PONTO NO NAVMESH
    // =========================================================

    private bool EncontrarPontoAleatorioNoNavMesh(
        Vector3 centro,
        float raio,
        out Vector3 resultado
    )
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 pontoAleatorio =
                centro +
                Random.insideUnitSphere * raio;

            NavMeshHit hit;

            if (NavMesh.SamplePosition(
                pontoAleatorio,
                out hit,
                1.0f,
                NavMesh.AllAreas
            ))
            {
                resultado = hit.position;

                return true;
            }
        }

        resultado = Vector3.zero;

        return false;
    }
}