using UnityEngine;
using Unity.Services.LevelPlay;

public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance;

    [Header("LevelPlay")]
    [SerializeField] private string appKey = "COLOQUE_SEU_APP_KEY_AQUI";

    [Header("Ad Units - Android")]
    [SerializeField] private string adUnitMoedas = "SEU_ID_REWARDED_MOEDAS";
    [SerializeField] private string adUnitRevive = "SEU_ID_REWARDED_REVIVE";

    private LevelPlayRewardedAd rewardedMoedas;
    private LevelPlayRewardedAd rewardedRevive;

    private bool sdkInicializado = false;
    private bool reviveFoiRecompensado = false;

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
        InicializarLevelPlay();
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

        // Mock Ads do Unity Editor
        LevelPlay.Init("editor");

#else

        // Android / aparelho real
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
            "[Ads] Falha ao inicializar LevelPlay: " +
            error
        );
    }

    // =========================================================
    // CRIAR REWARDED ADS
    // =========================================================

    private void CriarAds()
    {
#if UNITY_EDITOR

        // Mock Rewarded para testar dentro do Unity
        rewardedMoedas =
            new LevelPlayRewardedAd("editor_rewarded");

        rewardedRevive =
            new LevelPlayRewardedAd("editor_rewarded");

#else

        // Ad Units reais configurados no Dashboard
        rewardedMoedas =
            new LevelPlayRewardedAd(adUnitMoedas);

        rewardedRevive =
            new LevelPlayRewardedAd(adUnitRevive);

#endif

        // =====================================================
        // MOEDAS
        // =====================================================

        rewardedMoedas.OnAdLoaded += OnMoedasLoaded;
        rewardedMoedas.OnAdLoadFailed += OnMoedasLoadFailed;
        rewardedMoedas.OnAdDisplayed += OnAdDisplayed;
        rewardedMoedas.OnAdDisplayFailed += OnAdDisplayFailed;
        rewardedMoedas.OnAdRewarded += OnMoedasRewarded;
        rewardedMoedas.OnAdClosed += OnMoedasClosed;

        // =====================================================
        // REVIVE
        // =====================================================

        rewardedRevive.OnAdLoaded += OnReviveLoaded;
        rewardedRevive.OnAdLoadFailed += OnReviveLoadFailed;
        rewardedRevive.OnAdDisplayed += OnAdDisplayed;
        rewardedRevive.OnAdDisplayFailed += OnAdDisplayFailed;
        rewardedRevive.OnAdRewarded += OnReviveRewarded;
        rewardedRevive.OnAdClosed += OnReviveClosed;

        // Carrega os dois
        rewardedMoedas.LoadAd();
        rewardedRevive.LoadAd();

        Debug.Log(
            "[Ads] Rewarded Ads criados e carregando."
        );
    }

    // =========================================================
    // REWARDED - MOEDAS
    // =========================================================

    public void MostrarAdMoedas()
    {
        if (!sdkInicializado)
        {
            Debug.LogWarning(
                "[Ads] SDK ainda não inicializado."
            );

            return;
        }

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
                "[Ads] Rewarded de moedas não está pronto."
            );

            rewardedMoedas.LoadAd();

            return;
        }

        Debug.Log(
            "[Ads] Mostrando Rewarded de moedas."
        );

        rewardedMoedas.ShowAd();
    }

    private void OnMoedasLoaded(LevelPlayAdInfo adInfo)
    {
        Debug.Log(
            "[Ads] Rewarded moedas carregado."
        );
    }

    private void OnMoedasLoadFailed(LevelPlayAdError error)
    {
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

    private void OnMoedasClosed(LevelPlayAdInfo adInfo)
    {
        Debug.Log(
            "[Ads] Rewarded moedas fechado."
        );

        rewardedMoedas.LoadAd();
    }

    // =========================================================
    // REWARDED - REVIVE
    // =========================================================

    public void MostrarAdRevive()
    {
        if (!sdkInicializado)
        {
            Debug.LogWarning(
                "[Ads] SDK ainda não inicializado."
            );

            return;
        }

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
                "[Ads] Rewarded de revive não está pronto."
            );

            rewardedRevive.LoadAd();

            return;
        }

        reviveFoiRecompensado = false;

        Debug.Log(
            "[Ads] Mostrando Rewarded de revive."
        );

        // O jogo pode estar parado na tela de morte.
        // O anúncio precisa poder executar.
        Time.timeScale = 1f;

        rewardedRevive.ShowAd();
    }

    private void OnReviveLoaded(LevelPlayAdInfo adInfo)
    {
        Debug.Log(
            "[Ads] Rewarded revive carregado."
        );
    }

    private void OnReviveLoadFailed(LevelPlayAdError error)
    {
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

    private void OnReviveClosed(LevelPlayAdInfo adInfo)
    {
        Debug.Log(
            "[Ads] Rewarded revive fechado."
        );

        if (!reviveFoiRecompensado)
        {
            // O jogador recusou/fechou sem recompensa.
            // Volta para a tela de morte.
            Time.timeScale = 0f;

            Debug.Log(
                "[Ads] Anúncio fechado sem recompensa."
            );
        }

        rewardedRevive.LoadAd();
    }

    // =========================================================
    // EVENTOS GERAIS
    // =========================================================

    private void OnAdDisplayed(LevelPlayAdInfo adInfo)
    {
        Debug.Log("[Ads] Anúncio exibido.");
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

    private void OnDestroy()
    {
        LevelPlay.OnInitSuccess -= OnInitSuccess;
        LevelPlay.OnInitFailed -= OnInitFailed;
    }
}