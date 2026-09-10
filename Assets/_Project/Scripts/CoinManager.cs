using System;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    [Header("Desenvolvimento")]
    public bool apagarDadosAoIniciar = false;

    [Header("Dados do Jogador")]
    public GameData dados = new GameData();

    private const string CHAVE_DADOS = "NECRODASH_SAVE";

    // Avisará a UI sempre que as moedas mudarem
    public event Action<int> OnMoedasAlteradas;

    // Avisará sistemas quando os upgrades mudarem
    public event Action OnUpgradeAlterado;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        if (apagarDadosAoIniciar)
        {
            ApagarDados();
        }
        else
        {
            CarregarDados();
        }
    }

    // =========================================================
    // MOEDAS
    // =========================================================

    public int ObterMoedas()
    {
        return dados.moedas;
    }

    public void AdicionarMoedas(int quantidade)
    {
        if (quantidade <= 0)
        {
            Debug.LogWarning(
                "Tentativa de adicionar uma quantidade inválida de moedas: "
                + quantidade
            );

            return;
        }

        dados.moedas += quantidade;

        SalvarDados();

        OnMoedasAlteradas?.Invoke(dados.moedas);

        Debug.Log(
            "Moedas adicionadas: +" +
            quantidade +
            " | Total: " +
            dados.moedas
        );
    }

    public bool GastarMoedas(int quantidade)
    {
        if (quantidade <= 0)
        {
            Debug.LogWarning(
                "Tentativa de gastar uma quantidade inválida: "
                + quantidade
            );

            return false;
        }

        if (dados.moedas < quantidade)
        {
            Debug.Log(
                "Moedas insuficientes! " +
                "Atual: " +
                dados.moedas +
                " | Necessário: " +
                quantidade
            );

            return false;
        }

        dados.moedas -= quantidade;

        SalvarDados();

        OnMoedasAlteradas?.Invoke(dados.moedas);

        Debug.Log(
            "Moedas gastas: -" +
            quantidade +
            " | Restantes: " +
            dados.moedas
        );

        return true;
    }

    // =========================================================
    // DUPLO DASH
    // =========================================================

    public bool PossuiDuploDash()
    {
        return dados.duploDashComprado;
    }

    public bool ComprarDuploDash(int preco)
    {
        // Já possui o upgrade
        if (dados.duploDashComprado)
        {
            Debug.Log("Duplo Dash já foi comprado.");
            return false;
        }

        // Verifica se pode pagar
        if (!GastarMoedas(preco))
        {
            Debug.Log("Não foi possível comprar o Duplo Dash.");
            return false;
        }

        // Libera o upgrade
        dados.duploDashComprado = true;

        SalvarDados();

        OnUpgradeAlterado?.Invoke();

        Debug.Log("DUPLO DASH COMPRADO!");

        return true;
    }

    // =========================================================
    // SAVE / LOAD
    // =========================================================

    public void SalvarDados()
    {
        string json = JsonUtility.ToJson(dados);

        PlayerPrefs.SetString(CHAVE_DADOS, json);
        PlayerPrefs.Save();

        Debug.Log("Dados salvos: " + json);
    }

    private void CarregarDados()
    {
        if (PlayerPrefs.HasKey(CHAVE_DADOS))
        {
            string json =
                PlayerPrefs.GetString(CHAVE_DADOS);

            dados =
                JsonUtility.FromJson<GameData>(json);

            // Segurança caso o JSON antigo esteja incompleto
            if (dados == null)
            {
                dados = new GameData();
            }

            Debug.Log("Dados carregados: " + json);
        }
        else
        {
            dados = new GameData();

            Debug.Log("Nenhum save encontrado. Criando novo jogador.");
        }

        // Atualiza interfaces depois de carregar
        OnMoedasAlteradas?.Invoke(dados.moedas);
    }

    // =========================================================
    // RESET DE DADOS
    // =========================================================

    public void ApagarDados()
    {
        PlayerPrefs.DeleteKey(CHAVE_DADOS);
        PlayerPrefs.Save();

        dados = new GameData();

        OnMoedasAlteradas?.Invoke(dados.moedas);
        OnUpgradeAlterado?.Invoke();

        Debug.Log("Dados do jogador apagados.");
    }
}