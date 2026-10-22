
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

    // Indica se já havia um salvamento local válido.
    private bool existeSaveLocal;

    public event Action<int> OnMoedasAlteradas;
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
                "Quantidade inválida de moedas: " + quantidade
            );
            return;
        }

        dados.moedas += quantidade;

        SalvarDados();
        OnMoedasAlteradas?.Invoke(dados.moedas);

        Debug.Log(
            "Moedas adicionadas: +" + quantidade +
            " | Total: " + dados.moedas
        );
    }

    public bool GastarMoedas(int quantidade)
    {
        if (quantidade <= 0)
        {
            Debug.LogWarning(
                "Quantidade inválida para gastar: " + quantidade
            );
            return false;
        }

        if (dados.moedas < quantidade)
        {
            Debug.Log(
                "Moedas insuficientes! Atual: " +
                dados.moedas + " | Necessário: " + quantidade
            );

            return false;
        }

        dados.moedas -= quantidade;

        SalvarDados();
        OnMoedasAlteradas?.Invoke(dados.moedas);

        Debug.Log(
            "Moedas gastas: -" + quantidade +
            " | Restantes: " + dados.moedas
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
        if (dados.duploDashComprado)
        {
            Debug.Log("Duplo Dash já foi comprado.");
            return false;
        }

        if (!GastarMoedas(preco))
        {
            Debug.Log(
                "Não foi possível comprar o Duplo Dash."
            );
            return false;
        }

        dados.duploDashComprado = true;

        SalvarDados();
        OnUpgradeAlterado?.Invoke();

        Debug.Log("DUPLO DASH COMPRADO!");

        return true;
    }

    // =========================================================
    // SAVE LOCAL + ENVIO PARA A NUVEM
    // =========================================================

    public void SalvarDados()
    {
        if (dados == null)
        {
            dados = new GameData();
        }

        // Mantém uma data de atualização crescente.
        long agora = DateTime.UtcNow.Ticks;

        if (agora <= dados.dataAtualizacaoUtcTicks)
        {
            agora = dados.dataAtualizacaoUtcTicks + 1;
        }

        dados.dataAtualizacaoUtcTicks = agora;

        // Primeiro salva localmente.
        string json = JsonUtility.ToJson(dados);

        PlayerPrefs.SetString(CHAVE_DADOS, json);
        PlayerPrefs.Save();

        existeSaveLocal = true;

        Debug.Log("Dados salvos localmente: " + json);

        // Depois tenta enviar para a nuvem.
        // Se não houver login, o salvamento local continua funcionando.
        if (PlayFabLoginManager.Instance != null &&
            PlayFabLoginManager.Instance.IsLoggedIn)
        {
            PlayFabLoginManager.Instance.SalvarDadosNaNuvem(dados);
        }
    }

    private void CarregarDados()
    {
        if (PlayerPrefs.HasKey(CHAVE_DADOS))
        {
            string json = PlayerPrefs.GetString(CHAVE_DADOS);

            dados = JsonUtility.FromJson<GameData>(json);

            if (dados == null)
            {
                dados = new GameData();
                existeSaveLocal = false;

                Debug.LogWarning(
                    "Save local inválido. Um novo será preparado."
                );
            }
            else
            {
                existeSaveLocal = true;
                Debug.Log("Dados locais carregados: " + json);
            }
        }
        else
        {
            dados = new GameData();
            existeSaveLocal = false;

            Debug.Log(
                "Nenhum save local encontrado."
            );
        }

        OnMoedasAlteradas?.Invoke(dados.moedas);
    }

    // =========================================================
    // SINCRONIZAÇÃO COM O PLAYFAB
    // =========================================================

    public void SincronizarDadosComNuvem()
    {
        if (PlayFabLoginManager.Instance == null ||
            !PlayFabLoginManager.Instance.IsLoggedIn)
        {
            Debug.LogWarning(
                "[PlayFab] Sincronização cancelada: jogador não autenticado."
            );
            return;
        }

        Debug.Log(
            "[PlayFab] Consultando os dados do jogador na nuvem..."
        );

        PlayFabLoginManager.Instance.CarregarDadosDaNuvem(
            (dadosNuvem, consultaOk, possuiSaveNuvem) =>
            {
                // Falha de conexão ou dados inválidos:
                // preserva o salvamento local.
                if (!consultaOk)
                {
                    Debug.LogWarning(
                        "[PlayFab] Não foi possível sincronizar. " +
                        "Os dados locais foram preservados."
                    );
                    return;
                }

                // Primeira sincronização: não há dados na nuvem.
                // Envia o progresso que já existe no dispositivo.
                if (!possuiSaveNuvem || dadosNuvem == null)
                {
                    Debug.Log(
                        "[PlayFab] Nenhum salvamento na nuvem. " +
                        "Enviando os dados locais."
                    );

                    SalvarDados();
                    return;
                }

                // Sem salvamento local, a nuvem será usada.
                // Se ambos existem, compara a última atualização.
                bool usarDadosNuvem =
                    !existeSaveLocal ||
                    dadosNuvem.dataAtualizacaoUtcTicks >=
                    dados.dataAtualizacaoUtcTicks;

                if (usarDadosNuvem)
                {
                    dados = dadosNuvem;

                    Debug.Log(
                        "[PlayFab] Dados da nuvem selecionados."
                    );

                    // Atualiza interfaces e sistema de upgrades.
                    OnMoedasAlteradas?.Invoke(dados.moedas);
                    OnUpgradeAlterado?.Invoke();

                    // Grava localmente e atualiza a nuvem.
                    SalvarDados();
                }
                else
                {
                    Debug.Log(
                        "[PlayFab] O salvamento local é mais recente."
                    );

                    // Mantém o progresso local e o envia.
                    SalvarDados();
                }
            }
        );
    }

    // =========================================================
    // RESET LOCAL
    // =========================================================

    public void ApagarDados()
    {
        PlayerPrefs.DeleteKey(CHAVE_DADOS);
        PlayerPrefs.Save();

        dados = new GameData();
        existeSaveLocal = false;

        OnMoedasAlteradas?.Invoke(dados.moedas);
        OnUpgradeAlterado?.Invoke();

        Debug.Log(
            "Dados locais do jogador apagados. " +
            "Os dados da nuvem não foram removidos."
        );
    }
}
