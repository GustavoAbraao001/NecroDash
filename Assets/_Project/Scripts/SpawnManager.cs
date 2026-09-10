using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class SpawnManager : MonoBehaviour
{
    [Header("Configurações do Pool ")]
    public GameObject inimigoPrefab;
    public int quantidadeNoPool = 50;

    private List<GameObject> poolDeInimigos = new List<GameObject>();

    [Header("Configurações de Spawn")]
    public Transform player;
    private float distanciaMinimaParaSpawn = 10f;
    private float distanciaMaximaParaSpawn = 25f;
    public float raioDeSpawn = 15f;
    public float tempoEntreSpawns = 2f;

    private void Start()
    {
        for (int i = 0; i < quantidadeNoPool; i++)
        {
            GameObject inimigo = Instantiate(inimigoPrefab);
            inimigo.SetActive(false);
            poolDeInimigos.Add(inimigo);
        }

        InvokeRepeating(nameof(SpawnarInimigo), 1f, tempoEntreSpawns);
    }

    void SpawnarInimigo()
    {
        if (player == null)
            return;

        GameObject inimigoDisponivel = ObterInimigoDoPool();

        if (inimigoDisponivel == null)
            return;

        Vector3 pontoValido;

        if (!EncontrarPontoAleatorioNoNavMesh(
            player.position,
            distanciaMaximaParaSpawn,
            out pontoValido))
        {
            return;
        }

        float distancia =
            Vector3.Distance(player.position, pontoValido);

        if (distancia < distanciaMinimaParaSpawn)
            return;

        NavMeshAgent agent =
            inimigoDisponivel.GetComponent<NavMeshAgent>();

        inimigoDisponivel.transform.position = pontoValido;

        inimigoDisponivel.SetActive(true);

        if (agent != null)
        {
            agent.enabled = true;
            agent.Warp(pontoValido);
        }
    }

    GameObject ObterInimigoDoPool()
    {
        for(int i =0; i < poolDeInimigos.Count; i++)
        {
            if(!poolDeInimigos[i].activeInHierarchy)
            {
                return poolDeInimigos[i];
            }
        }

        return null;
    }

    bool EncontrarPontoAleatorioNoNavMesh(Vector3 centro, float raio, out Vector3 resultado)
    {
       
        for(int i = 0; i < 30; i++)
        {
            Vector3 pontoAleatorio = centro + Random.insideUnitSphere * raio;
            NavMeshHit hit;

            if (NavMesh.SamplePosition(pontoAleatorio, out hit, 1.0f, NavMesh.AllAreas))
            {
                resultado = hit.position;
                return true;
            }
        }
        resultado = Vector3.zero;
        return false;
    }










}
