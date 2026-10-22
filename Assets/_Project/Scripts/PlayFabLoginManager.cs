
using System;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class PlayFabLoginManager : MonoBehaviour
{
    private const string CHAVE_CUSTOM_ID =
        "NECRODASH_PLAYFAB_CUSTOM_ID";

    // Chave usada somente no armazenamento do PlayFab.
    private const string CHAVE_DADOS_NUVEM =
        "NecroDashGameData";

    public static PlayFabLoginManager Instance { get; private set; }

    public bool IsLoggedIn { get; private set; }

    public string CurrentPlayFabId { get; private set; }

    private string jsonSavePendente;
    private bool enviandoSave;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Login();
    }

    // =========================================================
    // LOGIN
    // =========================================================

    public void Login()
    {
        if (IsLoggedIn)
        {
            Debug.Log("[PlayFab] Jogador já autenticado.");
            return;
        }

        string customId =
            PlayerPrefs.GetString(CHAVE_CUSTOM_ID, "");

        if (string.IsNullOrWhiteSpace(customId))
        {
            customId = Guid.NewGuid().ToString("N");

            PlayerPrefs.SetString(CHAVE_CUSTOM_ID, customId);
            PlayerPrefs.Save();

            Debug.Log(
                "[PlayFab] Identificador local criado."
            );
        }

        LoginWithCustomIDRequest request =
            new LoginWithCustomIDRequest
            {
                CustomId = customId,
                CreateAccount = true
            };

        Debug.Log("[PlayFab] Tentando realizar login...");

        PlayFabClientAPI.LoginWithCustomID(
            request,

            result =>
            {
                IsLoggedIn = true;
                CurrentPlayFabId = result.PlayFabId;

                Debug.Log(
                    "[PlayFab] Login realizado com sucesso."
                );

                Debug.Log(
                    "[PlayFab] PlayFabId: " + CurrentPlayFabId
                );

                Debug.Log(
                    "[PlayFab] Conta recém-criada: " +
                    result.NewlyCreated
                );

                // Só sincroniza depois de autenticar.
                if (CoinManager.Instance != null)
                {
                    CoinManager.Instance.SincronizarDadosComNuvem();
                }
                else
                {
                    Debug.LogWarning(
                        "[PlayFab] CoinManager não encontrado."
                    );
                }
            },

            error =>
            {
                IsLoggedIn = false;

                Debug.LogError(
                    "[PlayFab] Falha no login:\n" +
                    error.GenerateErrorReport()
                );
            }
        );
    }

    // =========================================================
    // ENVIAR DADOS PARA A NUVEM
    // =========================================================

    public void SalvarDadosNaNuvem(GameData gameData)
    {
        if (!IsLoggedIn)
        {
            Debug.LogWarning(
                "[PlayFab] Salvamento na nuvem ignorado: " +
                "jogador não autenticado."
            );
            return;
        }

        if (gameData == null)
        {
            Debug.LogError(
                "[PlayFab] Não foi possível salvar dados nulos."
            );
            return;
        }

        // Se já há uma requisição em andamento, conserva
        // apenas a versão mais recente para o próximo envio.
        jsonSavePendente = JsonUtility.ToJson(gameData);

        if (!enviandoSave)
        {
            EnviarSavePendente();
        }
    }

    private void EnviarSavePendente()
    {
        if (!IsLoggedIn ||
            string.IsNullOrEmpty(jsonSavePendente))
        {
            enviandoSave = false;
            return;
        }

        string json = jsonSavePendente;
        jsonSavePendente = null;

        enviandoSave = true;

        UpdateUserDataRequest request =
            new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string>
                {
                    { CHAVE_DADOS_NUVEM, json }
                },

                // Somente o próprio jogador deve ler esses dados.
                Permission = UserDataPermission.Private
            };

        PlayFabClientAPI.UpdateUserData(
            request,

            result =>
            {
                Debug.Log(
                    "[PlayFab] Dados enviados para a nuvem."
                );

                Debug.Log(
                    "[PlayFab] JSON enviado: " + json
                );

                enviandoSave = false;

                // Se houve uma alteração durante a requisição,
                // envia a versão mais recente em seguida.
                if (!string.IsNullOrEmpty(jsonSavePendente))
                {
                    EnviarSavePendente();
                }
            },

            error =>
            {
                Debug.LogError(
                    "[PlayFab] Falha ao enviar os dados:\n" +
                    error.GenerateErrorReport()
                );

                enviandoSave = false;

                // Não descarta o progresso local.
                // Se houver outra versão pendente, tenta enviá-la.
                if (!string.IsNullOrEmpty(jsonSavePendente))
                {
                    EnviarSavePendente();
                }
            }
        );
    }

    // =========================================================
    // RECUPERAR DADOS DA NUVEM
    // =========================================================

    // callback:
    // dados recuperados, consulta bem-sucedida, existe save na nuvem
    public void CarregarDadosDaNuvem(
        Action<GameData, bool, bool> callback)
    {
        if (!IsLoggedIn)
        {
            callback?.Invoke(null, false, false);
            return;
        }

        GetUserDataRequest request =
            new GetUserDataRequest
            {
                Keys = new List<string>
                {
                    CHAVE_DADOS_NUVEM
                }
            };

        PlayFabClientAPI.GetUserData(
            request,

            result =>
            {
                if (result.Data == null ||
                    !result.Data.ContainsKey(CHAVE_DADOS_NUVEM))
                {
                    Debug.Log(
                        "[PlayFab] Nenhum dado de jogo encontrado na nuvem."
                    );

                    callback?.Invoke(null, true, false);
                    return;
                }

                string json =
                    result.Data[CHAVE_DADOS_NUVEM].Value;

                GameData dadosNuvem = null;

                try
                {
                    dadosNuvem =
                        JsonUtility.FromJson<GameData>(json);
                }
                catch (Exception exception)
                {
                    Debug.LogError(
                        "[PlayFab] Erro ao interpretar o JSON: " +
                        exception.Message
                    );
                }

                if (dadosNuvem == null)
                {
                    Debug.LogError(
                        "[PlayFab] O salvamento da nuvem é inválido."
                    );

                    callback?.Invoke(null, false, true);
                    return;
                }

                Debug.Log(
                    "[PlayFab] Dados recuperados da nuvem: " + json
                );

                callback?.Invoke(dadosNuvem, true, true);
            },

            error =>
            {
                Debug.LogError(
                    "[PlayFab] Falha ao recuperar os dados:\n" +
                    error.GenerateErrorReport()
                );

                // O chamador sabe que a consulta falhou e preserva
                // o salvamento local.
                callback?.Invoke(null, false, false);
            }
        );
    }

    [ContextMenu("TESTE: Apagar progresso local e da nuvem")]
    public void ApagarProgressoCompleto()
    {
        if (!IsLoggedIn)
        {
            Debug.LogWarning(
                "[PlayFab] Não é possível resetar: jogador não autenticado."
            );
            return;
        }

        if (enviandoSave)
        {
            Debug.LogWarning(
                "[PlayFab] Aguarde o salvamento atual terminar antes do reset."
            );
            return;
        }

        jsonSavePendente = null;

        UpdateUserDataRequest request = new UpdateUserDataRequest
        {
            KeysToRemove = new List<string>
            {
                CHAVE_DADOS_NUVEM
            }
        };

        Debug.Log("[PlayFab] Apagando progresso da nuvem...");

        PlayFabClientAPI.UpdateUserData(
            request,

            result =>
            {
                if (CoinManager.Instance != null)
                {
                    CoinManager.Instance.ApagarDados();
                }

                Debug.Log(
                    "[PlayFab] Progresso removido da nuvem e do dispositivo."
                );
            },

            error =>
            {
                Debug.LogError(
                    "[PlayFab] Não foi possível apagar o progresso da nuvem:\n" +
                    error.GenerateErrorReport()
                );

                Debug.LogWarning(
                    "[PlayFab] Os dados locais foram preservados."
                );
            }
        );
    }
}
