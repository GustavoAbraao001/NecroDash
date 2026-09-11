using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.LevelPlay;

public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance;

    [Header("LevelPlay")]
    [SerializeField] private string appKey = "COLOQUE_SEU_APP_KEY_AQUI";

    [Header("Ad Units - Android")]
    [SerializeField] private string adUnitMoedas = "SEU_ID_REWARDED_MOEDAS";
    [SerializeField] private string adUnitRevive = "SEU_ID_REWARDED_REVIVE";

    [Header("Recompensas")]
    [SerializeField] private int moedasPorAds = 50;

    private LevelPlayRewardedAd rewardedMoedas;
    private LevelPlayRewardedAd rewardedRevive;

    // No Editor usamos apenas um Mock Rewarded.
    private LevelPlayRewardedAd rewardedMock;

    private bool sdkInicializado = false;

    // =========================================================
    // CONTROLE DE CARREGAMENTO
    // =========================================================

    private bool mockCarregando = false;
    private bool moedasCarregando = false;
    private bool reviveCarregando = false;

    // =========================================================
    // CONTROLE DO REVIVE
    // =========================================================

    // True = jogador já usou o revive nesta partida.
    private bool reviveJaUsado = false;

    // Indica que o anúncio de revive entregou a recompensa.
    private bool reviveFoiRecompensado = false;

    // =========================================================
    // CONTROLE DO MOCK
    // =========================================================

    private enum TipoReward
    {
        Nenhum,
        Moedas,
        Revive
    }

    private TipoReward rewardAtual = TipoReward.Nenhum;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Detecta troca de cenas.
        SceneManager.sceneLoaded += OnSceneLoaded;

        InicializarLevelPlay();
    }

    // =========================================================
    // TROCA DE CENAS
    // =========================================================

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("[Ads] Cena carregada: " + scene.name);

        // Sempre que a JogoScene começa,
        // o jogador recebe novamente 1 oportunidade de revive.
        if (scene.name == "JogoScene")
        {
            reviveJaUsado = false;
            reviveFoiRecompensado = false;

            Debug.Log("[Ads] Nova partida iniciada. Revive liberado.");
        }

        // Garante que os anúncios estejam carregados.
        if (sdkInicializado)
        {
            CarregarAds();
        }
    }

    // =========================================================
    // INICIALIZAÇÃO
    // =========================================================

    private void InicializarLevelPlay()
    {
        Debug.Log("[Ads] Inicializando LevelPlay...");

        LevelPlay.OnInitSuccess += OnInitSuccess;
        LevelPlay.OnInitFailed += OnInitFailed;

#if UNITY_EDITOR

        // Mock oficial do LevelPlay para o Editor.
        LevelPlay.Init("editor");

#else

        // App Key real para Android.
        LevelPlay.Init(appKey);

#endif
    }

    private void OnInitSuccess(LevelPlayConfiguration configuration)
    {
        Debug.Log("[Ads] LevelPlay inicializado com sucesso!");

        sdkInicializado = true;

        CriarAds();
    }

    private void OnInitFailed(LevelPlayInitError error)
    {
        sdkInicializado = false;

        Debug.LogError(
            "[Ads] Falha ao inicializar LevelPlay: " + error
        );
    }

    // =========================================================
    // CRIAÇÃO DOS ADS
    // =========================================================

    private void CriarAds()
    {
#if UNITY_EDITOR

        CriarAdMock();

#else

        CriarAdMoedas();
        CriarAdRevive();

#endif
    }

    // =========================================================
    // EDITOR - MOCK
    // =========================================================

    private void CriarAdMock()
    {
        if (rewardedMock != null)
        {
            Debug.Log("[Ads] Mock já existe. Apenas garantindo carregamento.");
            CarregarMock();
            return;
        }

        rewardedMock =
            new LevelPlayRewardedAd("editor_rewarded");

        rewardedMock.OnAdLoaded +=
            OnMockAdLoaded;

        rewardedMock.OnAdLoadFailed +=
            OnMockAdLoadFailed;

        rewardedMock.OnAdDisplayed +=
            OnMockAdDisplayed;

        rewardedMock.OnAdDisplayFailed +=
            OnMockAdDisplayFailed;

        rewardedMock.OnAdRewarded +=
            OnMockAdRewarded;

        rewardedMock.OnAdClosed +=
            OnMockAdClosed;

        Debug.Log("[Ads] Mock Rewarded criado.");

        CarregarMock();
    }

    // =========================================================
    // ANDROID - MOEDAS
    // =========================================================

    private void CriarAdMoedas()
    {
        if (rewardedMoedas != null)
        {
            CarregarAdMoedas();
            return;
        }

        rewardedMoedas =
            new LevelPlayRewardedAd(adUnitMoedas);

        rewardedMoedas.OnAdLoaded +=
            OnMoedasLoaded;

        rewardedMoedas.OnAdLoadFailed +=
            OnMoedasLoadFailed;

        rewardedMoedas.OnAdDisplayed +=
            OnAdDisplayed;

        rewardedMoedas.OnAdDisplayFailed +=
            OnAdDisplayFailed;

        rewardedMoedas.OnAdRewarded +=
            OnMoedasRewarded;

        rewardedMoedas.OnAdClosed +=
            OnMoedasClosed;

        Debug.Log("[Ads] Rewarded de moedas criado.");

        CarregarAdMoedas();
    }

    // =========================================================
    // ANDROID - REVIVE
    // =========================================================

    private void CriarAdRevive()
    {
        if (rewardedRevive != null)
        {
            CarregarAdRevive();
            return;
        }

        rewardedRevive =
            new LevelPlayRewardedAd(adUnitRevive);

        rewardedRevive.OnAdLoaded +=
            OnReviveLoaded;

        rewardedRevive.OnAdLoadFailed +=
            OnReviveLoadFailed;

        rewardedRevive.OnAdDisplayed +=
            OnAdDisplayed;

        rewardedRevive.OnAdDisplayFailed +=
            OnAdDisplayFailed;

        rewardedRevive.OnAdRewarded +=
            OnReviveRewarded;

        rewardedRevive.OnAdClosed +=
            OnReviveClosed;

        Debug.Log("[Ads] Rewarded de revive criado.");

        CarregarAdRevive();
    }

    // =========================================================
    // CARREGAR TODOS
    // =========================================================

    private void CarregarAds()
    {
#if UNITY_EDITOR

        CarregarMock();

#else

        CarregarAdMoedas();
        CarregarAdRevive();

#endif
    }

    // =========================================================
    // CARREGAR MOCK
    // =========================================================

    private void CarregarMock()
    {
        if (rewardedMock == null)
        {
            Debug.LogWarning("[Ads] Mock ainda não existe.");
            return;
        }

        if (rewardedMock.IsAdReady())
        {
            Debug.Log("[Ads] Mock já está pronto.");
            return;
        }

        if (mockCarregando)
        {
            Debug.Log("[Ads] Mock já está carregando.");
            return;
        }

        mockCarregando = true;

        Debug.Log("[Ads] Carregando novo Mock Rewarded...");

        rewardedMock.LoadAd();
    }

    // =========================================================
    // CARREGAR MOEDAS
    // =========================================================

    private void CarregarAdMoedas()
    {
        if (rewardedMoedas == null)
            return;

        if (rewardedMoedas.IsAdReady())
        {
            Debug.Log("[Ads] Rewarded de moedas já está pronto.");
            return;
        }

        if (moedasCarregando)
        {
            Debug.Log("[Ads] Rewarded de moedas já está carregando.");
            return;
        }

        moedasCarregando = true;

        Debug.Log("[Ads] Carregando novo Rewarded de moedas...");

        rewardedMoedas.LoadAd();
    }

    // =========================================================
    // CARREGAR REVIVE
    // =========================================================

    private void CarregarAdRevive()
    {
        if (rewardedRevive == null)
            return;

        if (rewardedRevive.IsAdReady())
        {
            Debug.Log("[Ads] Rewarded de revive já está pronto.");
            return;
        }

        if (reviveCarregando)
        {
            Debug.Log("[Ads] Rewarded de revive já está carregando.");
            return;
        }

        reviveCarregando = true;

        Debug.Log("[Ads] Carregando novo Rewarded de revive...");

        rewardedRevive.LoadAd();
    }

    // =========================================================
    // MOSTRAR ADS - MOEDAS
    // =========================================================

    public void MostrarAdMoedas()
    {
        Debug.Log("[Ads] Botão de moedas acionado.");

        if (!sdkInicializado)
        {
            Debug.LogWarning(
                "[Ads] SDK ainda não foi inicializado."
            );

            return;
        }

#if UNITY_EDITOR

        if (rewardedMock == null)
        {
            Debug.LogWarning(
                "[Ads] Mock Rewarded não existe."
            );

            return;
        }

        if (!rewardedMock.IsAdReady())
        {
            Debug.Log(
                "[Ads] Mock ainda não está pronto. Carregando..."
            );

            CarregarMock();

            return;
        }

        rewardAtual = TipoReward.Moedas;

        Debug.Log(
            "[Ads] Mostrando Mock Rewarded para moedas."
        );

        rewardedMock.ShowAd();

#else

        if (rewardedMoedas == null)
        {
            Debug.LogWarning(
                "[Ads] Rewarded de moedas não existe."
            );

            return;
        }

        if (!rewardedMoedas.IsAdReady())
        {
            Debug.LogWarning(
                "[Ads] Rewarded de moedas ainda não está pronto."
            );

            CarregarAdMoedas();

            return;
        }

        Debug.Log(
            "[Ads] Mostrando Rewarded de moedas."
        );

        rewardedMoedas.ShowAd();

#endif
    }

    // =========================================================
    // MOSTRAR ADS - REVIVE
    // =========================================================

    public void MostrarAdRevive()
    {
        Debug.Log("[Ads] Botão de revive acionado.");

        if (!sdkInicializado)
        {
            Debug.LogWarning(
                "[Ads] SDK ainda não foi inicializado."
            );

            return;
        }

        // Impede múltiplos revives na mesma partida.
        if (reviveJaUsado)
        {
            Debug.LogWarning(
                "[Ads] O revive já foi usado nesta partida."
            );

            return;
        }

        reviveFoiRecompensado = false;

#if UNITY_EDITOR

        if (rewardedMock == null)
        {
            Debug.LogWarning(
                "[Ads] Mock Rewarded não existe."
            );

            return;
        }

        if (!rewardedMock.IsAdReady())
        {
            Debug.Log(
                "[Ads] Mock ainda não está pronto. Carregando..."
            );

            CarregarMock();

            return;
        }

        rewardAtual = TipoReward.Revive;

        Debug.Log(
            "[Ads] Mostrando Mock Rewarded para revive."
        );

        Time.timeScale = 1f;

        rewardedMock.ShowAd();

#else

        if (rewardedRevive == null)
        {
            Debug.LogWarning(
                "[Ads] Rewarded de revive não existe."
            );

            return;
        }

        if (!rewardedRevive.IsAdReady())
        {
            Debug.LogWarning(
                "[Ads] Rewarded de revive ainda não está pronto."
            );

            CarregarAdRevive();

            return;
        }

        Debug.Log(
            "[Ads] Mostrando Rewarded de revive."
        );

        Time.timeScale = 1f;

        rewardedRevive.ShowAd();

#endif
    }

    // =========================================================
    // EDITOR - MOCK CARREGADO
    // =========================================================

    private void OnMockAdLoaded(
        LevelPlayAdInfo adInfo)
    {
        mockCarregando = false;

        Debug.Log(
            "[Ads] Mock Rewarded carregado e pronto."
        );
    }

    private void OnMockAdLoadFailed(
        LevelPlayAdError error)
    {
        mockCarregando = false;

        Debug.LogWarning(
            "[Ads] Falha ao carregar Mock Rewarded: " +
            error
        );
    }

    private void OnMockAdDisplayed(
        LevelPlayAdInfo adInfo)
    {
        Debug.Log(
            "[Ads] Mock Rewarded exibido."
        );
    }

    private void OnMockAdDisplayFailed(
        LevelPlayAdInfo adInfo,
        LevelPlayAdError error)
    {
        Debug.LogError(
            "[Ads] Falha ao exibir Mock Rewarded: " +
            error
        );

        rewardAtual = TipoReward.Nenhum;

        mockCarregando = false;

        CarregarMock();
    }

    private void OnMockAdRewarded(
        LevelPlayAdInfo adInfo,
        LevelPlayReward recompensa)
    {
        Debug.Log(
            "[Ads] MOCK REWARDED ENTREGOU RECOMPENSA."
        );

        ConcederRecompensa(recompensa);
    }

    private void OnMockAdClosed(
        LevelPlayAdInfo adInfo)
    {
        Debug.Log(
            "[Ads] Mock Rewarded fechado."
        );

        if (rewardAtual == TipoReward.Revive &&
            !reviveFoiRecompensado)
        {
            Debug.Log(
                "[Ads] Revive fechado sem recompensa."
            );
        }

        // Não zeramos rewardAtual aqui.
        // Isso evita conflito caso OnAdRewarded
        // chegue depois de OnAdClosed.

        reviveFoiRecompensado = false;

        // Carrega imediatamente outro anúncio.
        CarregarMock();
    }

    // =========================================================
    // RECOMPENSA CENTRAL - EDITOR
    // =========================================================

    private void ConcederRecompensa(
        LevelPlayReward recompensa)
    {
        switch (rewardAtual)
        {
            case TipoReward.Moedas:

                Debug.Log(
                    "[Ads] RECOMPENSA: +" +
                    moedasPorAds +
                    " moedas."
                );

                if (CoinManager.Instance != null)
                {
                    CoinManager.Instance.AdicionarMoedas(
                        moedasPorAds
                    );
                }
                else
                {
                    Debug.LogWarning(
                        "[Ads] CoinManager não encontrado."
                    );
                }

                break;

            case TipoReward.Revive:

                Debug.Log(
                    "[Ads] RECOMPENSA: REVIVER."
                );

                reviveFoiRecompensado = true;

                // Marca que o revive desta partida foi consumido.
                reviveJaUsado = true;

                if (DeathManager.Instance != null)
                {
                    DeathManager.Instance.Reviver();
                }
                else
                {
                    Debug.LogWarning(
                        "[Ads] DeathManager não encontrado."
                    );
                }

                break;

            default:

                Debug.LogWarning(
                    "[Ads] Recompensa recebida sem tipo definido."
                );

                break;
        }
    }

    // =========================================================
    // ANDROID - MOEDAS
    // =========================================================

    private void OnMoedasLoaded(
        LevelPlayAdInfo adInfo)
    {
        moedasCarregando = false;

        Debug.Log(
            "[Ads] Rewarded moedas carregado e pronto."
        );
    }

    private void OnMoedasLoadFailed(
        LevelPlayAdError error)
    {
        moedasCarregando = false;

        Debug.LogWarning(
            "[Ads] Falha ao carregar moedas: " +
            error
        );
    }

    private void OnMoedasRewarded(
        LevelPlayAdInfo adInfo,
        LevelPlayReward recompensa)
    {
        Debug.Log(
            "[Ads] RECOMPENSA DE MOEDAS: " +
            recompensa.Name +
            " x" +
            recompensa.Amount
        );

        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.AdicionarMoedas(
                recompensa.Amount
            );
        }
    }

    private void OnMoedasClosed(
        LevelPlayAdInfo adInfo)
    {
        Debug.Log(
            "[Ads] Rewarded moedas fechado."
        );

        // Depois de consumir um anúncio,
        // já começamos a carregar o próximo.
        CarregarAdMoedas();
    }

    // =========================================================
    // ANDROID - REVIVE
    // =========================================================

    private void OnReviveLoaded(
        LevelPlayAdInfo adInfo)
    {
        reviveCarregando = false;

        Debug.Log(
            "[Ads] Rewarded revive carregado e pronto."
        );
    }

    private void OnReviveLoadFailed(
        LevelPlayAdError error)
    {
        reviveCarregando = false;

        Debug.LogWarning(
            "[Ads] Falha ao carregar revive: " +
            error
        );
    }

    private void OnReviveRewarded(
        LevelPlayAdInfo adInfo,
        LevelPlayReward recompensa)
    {
        Debug.Log(
            "[Ads] RECOMPENSA DE REVIVE: " +
            recompensa.Name +
            " x" +
            recompensa.Amount
        );

        reviveFoiRecompensado = true;

        // Um revive por partida.
        reviveJaUsado = true;

        if (DeathManager.Instance != null)
        {
            DeathManager.Instance.Reviver();
        }
        else
        {
            Debug.LogWarning(
                "[Ads] DeathManager não encontrado."
            );
        }
    }

    private void OnReviveClosed(
        LevelPlayAdInfo adInfo)
    {
        Debug.Log(
            "[Ads] Rewarded revive fechado."
        );

        if (!reviveFoiRecompensado)
        {
            Debug.Log(
                "[Ads] Revive fechado sem recompensa."
            );
        }

        // Carrega o próximo anúncio imediatamente.
        CarregarAdRevive();

        // Limpa apenas o controle da recompensa atual.
        reviveFoiRecompensado = false;
    }

    // =========================================================
    // EVENTOS GERAIS - ANDROID
    // =========================================================

    private void OnAdDisplayed(
        LevelPlayAdInfo adInfo)
    {
        Debug.Log(
            "[Ads] Anúncio exibido."
        );
    }

    private void OnAdDisplayFailed(
        LevelPlayAdInfo adInfo,
        LevelPlayAdError error)
    {
        Debug.LogError(
            "[Ads] Falha ao exibir anúncio: " +
            error
        );
    }

    // =========================================================
    // VERIFICAÇÃO PÚBLICA
    // =========================================================

    public bool ReviveDisponivel()
    {
        return !reviveJaUsado;
    }

    // =========================================================
    // CLEANUP
    // =========================================================

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        LevelPlay.OnInitSuccess -= OnInitSuccess;
        LevelPlay.OnInitFailed -= OnInitFailed;
    }
}