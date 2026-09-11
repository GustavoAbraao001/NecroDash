using UnityEngine;
using Unity.Services.LevelPlay;

public class AdsLojaManager : MonoBehaviour
{
    public static AdsLojaManager Instance;

    [Header("LevelPlay")]
    [SerializeField] private string appKey = "COLOQUE_SEU_APP_KEY_AQUI";

    [Header("Rewarded Loja - Android")]
    [SerializeField] private string adUnitMoedas = "SEU_ID_REWARDED_MOEDAS";

    [Header("Recompensa")]
    [SerializeField] private int moedasPorAds = 50;

    // Android
    private LevelPlayRewardedAd rewardedMoedas;

    // Editor
    private LevelPlayRewardedAd rewardedMock;

    private bool sdkInicializado = false;
    private bool anuncioCarregando = false;

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
        InicializarLevelPlay();
    }

    // =========================================================
    // LEVELPLAY
    // =========================================================

    private void InicializarLevelPlay()
    {
        Debug.Log("[AdsLoja] Inicializando LevelPlay...");

        LevelPlay.OnInitSuccess += OnInitSuccess;
        LevelPlay.OnInitFailed += OnInitFailed;

#if UNITY_EDITOR

        LevelPlay.Init("editor");

#else

        LevelPlay.Init(appKey);

#endif
    }

    private void OnInitSuccess(LevelPlayConfiguration configuration)
    {
        Debug.Log("[AdsLoja] LevelPlay inicializado.");

        sdkInicializado = true;

        CriarAnuncio();
    }

    private void OnInitFailed(LevelPlayInitError error)
    {
        sdkInicializado = false;

        Debug.LogError(
            "[AdsLoja] Falha ao inicializar LevelPlay: " +
            error
        );
    }

    // =========================================================
    // CRIAR ANÚNCIO
    // =========================================================

    private void CriarAnuncio()
    {
#if UNITY_EDITOR

        rewardedMock =
            new LevelPlayRewardedAd("editor_loja_rewarded");

        rewardedMock.OnAdLoaded +=
            OnMockLoaded;

        rewardedMock.OnAdLoadFailed +=
            OnMockLoadFailed;

        rewardedMock.OnAdDisplayed +=
            OnAdDisplayed;

        rewardedMock.OnAdDisplayFailed +=
            OnMockDisplayFailed;

        rewardedMock.OnAdRewarded +=
            OnMockRewarded;

        rewardedMock.OnAdClosed +=
            OnMockClosed;

        CarregarAnuncio();

#else

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

        CarregarAnuncio();

#endif
    }

    // =========================================================
    // CARREGAR ANÚNCIO
    // =========================================================

    private void CarregarAnuncio()
    {
#if UNITY_EDITOR

        if (rewardedMock == null)
            return;

        if (rewardedMock.IsAdReady())
        {
            Debug.Log("[AdsLoja] Mock já está pronto.");
            return;
        }

#else

        if (rewardedMoedas == null)
            return;

        if (rewardedMoedas.IsAdReady())
        {
            Debug.Log("[AdsLoja] Rewarded da loja já está pronto.");
            return;
        }

#endif

        if (anuncioCarregando)
        {
            Debug.Log("[AdsLoja] Anúncio já está carregando.");
            return;
        }

        anuncioCarregando = true;

        Debug.Log("[AdsLoja] Carregando anúncio da loja...");

#if UNITY_EDITOR

        rewardedMock.LoadAd();

#else

        rewardedMoedas.LoadAd();

#endif
    }

    // =========================================================
    // MOSTRAR ANÚNCIO
    // =========================================================

    public void MostrarAnuncio()
    {
        Debug.Log("[AdsLoja] Botão de anúncio acionado.");

        if (!sdkInicializado)
        {
            Debug.LogWarning(
                "[AdsLoja] SDK ainda não inicializado."
            );

            return;
        }

#if UNITY_EDITOR

        if (rewardedMock == null)
        {
            Debug.LogWarning(
                "[AdsLoja] Mock Rewarded não existe."
            );

            return;
        }

        if (!rewardedMock.IsAdReady())
        {
            Debug.Log(
                "[AdsLoja] Anúncio ainda não está pronto. Carregando..."
            );

            CarregarAnuncio();

            return;
        }

        Debug.Log(
            "[AdsLoja] Exibindo anúncio da loja."
        );

        rewardedMock.ShowAd();

#else

        if (rewardedMoedas == null)
        {
            Debug.LogWarning(
                "[AdsLoja] Rewarded não existe."
            );

            return;
        }

        if (!rewardedMoedas.IsAdReady())
        {
            Debug.Log(
                "[AdsLoja] Anúncio ainda não está pronto. Carregando..."
            );

            CarregarAnuncio();

            return;
        }

        Debug.Log(
            "[AdsLoja] Exibindo Rewarded da loja."
        );

        rewardedMoedas.ShowAd();

#endif
    }

    // =========================================================
    // EDITOR - MOCK
    // =========================================================

    private void OnMockLoaded(LevelPlayAdInfo adInfo)
    {
        anuncioCarregando = false;

        Debug.Log(
            "[AdsLoja] Mock Rewarded carregado e pronto."
        );
    }

    private void OnMockLoadFailed(LevelPlayAdError error)
    {
        anuncioCarregando = false;

        Debug.LogWarning(
            "[AdsLoja] Falha ao carregar Mock: " +
            error
        );
    }

    private void OnMockDisplayFailed(
        LevelPlayAdInfo adInfo,
        LevelPlayAdError error)
    {
        anuncioCarregando = false;

        Debug.LogError(
            "[AdsLoja] Falha ao exibir Mock: " +
            error
        );

        CarregarAnuncio();
    }

    private void OnMockRewarded(
        LevelPlayAdInfo adInfo,
        LevelPlayReward recompensa)
    {
        Debug.Log(
            "[AdsLoja] RECOMPENSA RECEBIDA: +" +
            moedasPorAds +
            " moedas."
        );

        DarMoedas();
    }

    private void OnMockClosed(LevelPlayAdInfo adInfo)
    {
        Debug.Log(
            "[AdsLoja] Mock fechado."
        );

        // Assim que o anúncio terminar,
        // começa a carregar o próximo.
        anuncioCarregando = false;

        CarregarAnuncio();
    }

    // =========================================================
    // ANDROID
    // =========================================================

    private void OnMoedasLoaded(LevelPlayAdInfo adInfo)
    {
        anuncioCarregando = false;

        Debug.Log(
            "[AdsLoja] Rewarded da loja carregado e pronto."
        );
    }

    private void OnMoedasLoadFailed(LevelPlayAdError error)
    {
        anuncioCarregando = false;

        Debug.LogWarning(
            "[AdsLoja] Falha ao carregar Rewarded: " +
            error
        );
    }

    private void OnMoedasRewarded(
        LevelPlayAdInfo adInfo,
        LevelPlayReward recompensa)
    {
        Debug.Log(
            "[AdsLoja] RECOMPENSA RECEBIDA: " +
            recompensa.Name +
            " x" +
            recompensa.Amount
        );

        DarMoedas();
    }

    private void OnMoedasClosed(LevelPlayAdInfo adInfo)
    {
        Debug.Log(
            "[AdsLoja] Rewarded fechado."
        );

        anuncioCarregando = false;

        // Carrega automaticamente outro.
        CarregarAnuncio();
    }

    // =========================================================
    // DAR MOEDAS
    // =========================================================

    private void DarMoedas()
    {
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.AdicionarMoedas(
                moedasPorAds
            );
        }
        else
        {
            Debug.LogWarning(
                "[AdsLoja] CoinManager não encontrado."
            );
        }
    }

    // =========================================================
    // EVENTOS
    // =========================================================

    private void OnAdDisplayed(LevelPlayAdInfo adInfo)
    {
        Debug.Log(
            "[AdsLoja] Anúncio exibido."
        );
    }

    private void OnAdDisplayFailed(
        LevelPlayAdInfo adInfo,
        LevelPlayAdError error)
    {
        anuncioCarregando = false;

        Debug.LogError(
            "[AdsLoja] Falha ao exibir anúncio: " +
            error
        );
    }

    // =========================================================
    // CLEANUP
    // =========================================================

    private void OnDestroy()
    {
        LevelPlay.OnInitSuccess -= OnInitSuccess;
        LevelPlay.OnInitFailed -= OnInitFailed;
    }
}