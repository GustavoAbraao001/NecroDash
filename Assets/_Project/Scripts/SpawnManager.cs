using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpawnManager : MonoBehaviour
{
    [Header("Prefabs dos Inimigos")]
    public GameObject prefabCaveira;
    public GameObject prefabZumbi;

    [Header("Quantidade no Pool")]
    public int quantidadeCaveiras = 25;
    public int quantidadeZumbis = 25;

    private List<GameObject> poolCaveiras =
        new List<GameObject>();

    private List<GameObject> poolZumbis =
        new List<GameObject>();

    [Header("Configurações de Spawn")]
    public Transform player;

    public float distanciaMinimaParaSpawn = 10f;
    public float distanciaMaximaParaSpawn = 25f;
    public float tempoEntreSpawns = 2f;

    [Header("Distribuição")]
    [Range(0f, 1f)]
    public float chanceDeCaveira = 0.5f;

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
                "Prefab não configurado no SpawnManager."
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
        if (player == null)
            return;

        // Decide qual inimigo será criado
        bool spawnarCaveira =
            Random.value <= chanceDeCaveira;

        GameObject inimigoDisponivel;

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

        // Se não encontrou desse tipo,
        // tenta o outro tipo.
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

        if (inimigoDisponivel == null)
        {
            Debug.Log(
                "Todos os inimigos do pool estão ativos."
            );

            return;
        }

        Vector3 pontoValido;

        if (!EncontrarPontoAleatorioNoNavMesh(
            player.position,
            distanciaMaximaParaSpawn,
            out pontoValido))
        {
            return;
        }

        float distancia =
            Vector3.Distance(
                player.position,
                pontoValido
            );

        if (distancia < distanciaMinimaParaSpawn)
            return;

        inimigoDisponivel.transform.position =
            pontoValido;

        inimigoDisponivel.transform.rotation =
            Quaternion.identity;

        inimigoDisponivel.SetActive(true);

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