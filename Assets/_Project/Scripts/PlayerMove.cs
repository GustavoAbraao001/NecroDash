using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMove : MonoBehaviour
{
    [Header("Movimentação Base")]
    public Joystick joystick;
    public float velocidade = 5f;

    [Header("Dash 1")]
    public float velocidadeDash1 = 25f;
    public float duracaoDash1 = 0.15f;
    public float tempoRecargaDash1 = 10f;

    [Header("Dash 2 - Duplo Dash")]
    public float velocidadeDash2 = 25f;
    public float duracaoDash2 = 0.15f;
    public float tempoRecargaDash2 = 20f;

    [HideInInspector]
    public bool isInvulneravel = false;

    [Header("Interface")]
    public Slider barraCooldownDash1;
    public Slider barraCooldownDash2;

    private Rigidbody rb;

    private Vector3 direcaoMove;
    private Vector3 ultimaDirecao;

    private bool isDashing = false;

    // Controle das duas cargas
    private bool dash1Disponivel = true;
    private bool dash2Disponivel = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        ultimaDirecao = transform.forward;

        ConfigurarInterface();
        ConfigurarDash();
    }

    private void Update()
    {
        if (!isDashing)
        {
            direcaoMove = new Vector3(
                joystick.Horizontal,
                0f,
                joystick.Vertical
            ).normalized;

            if (direcaoMove.magnitude >= 0.1f)
            {
                // Guarda a última direção válida
                ultimaDirecao = direcaoMove;

                // Faz o modelo olhar para onde está andando
                transform.forward = direcaoMove;
            }
        }
    }

    private void FixedUpdate()
    {
        if (isDashing)
            return;

        if (direcaoMove.magnitude >= 0.1f)
        {
            rb.MovePosition(
                rb.position +
                direcaoMove * velocidade * Time.fixedDeltaTime
            );
        }
    }

    // =========================================================
    // CONFIGURAÇÃO
    // =========================================================

    private void ConfigurarInterface()
    {
        if (barraCooldownDash1 != null)
        {
            barraCooldownDash1.minValue = 0f;
            barraCooldownDash1.maxValue = 1f;
            barraCooldownDash1.value = 1f;
        }

        if (barraCooldownDash2 != null)
        {
            barraCooldownDash2.minValue = 0f;
            barraCooldownDash2.maxValue = 1f;
            barraCooldownDash2.value = 1f;

            // Só aparece se possuir Duplo Dash
            barraCooldownDash2.gameObject.SetActive(false);
        }
    }

    private void ConfigurarDash()
    {
        if (CoinManager.Instance != null &&
            CoinManager.Instance.PossuiDuploDash())
        {
            dash2Disponivel = true;

            if (barraCooldownDash2 != null)
            {
                barraCooldownDash2.gameObject.SetActive(true);
            }

            Debug.Log("Duplo Dash ativo!");
        }
        else
        {
            dash2Disponivel = false;
        }
    }

    // =========================================================
    // ATIVAR DASH
    // =========================================================

    public void AtivarDash()
    {
        if (isDashing)
            return;

        // Primeiro Dash
        if (dash1Disponivel)
        {
            dash1Disponivel = false;

            StartCoroutine(
                ExecutarDash(
                    ultimaDirecao,
                    velocidadeDash1,
                    duracaoDash1
                )
            );

            StartCoroutine(
                RecarregarDash1()
            );

            return;
        }

        // Segundo Dash
        if (dash2Disponivel)
        {
            dash2Disponivel = false;

            StartCoroutine(
                ExecutarDash(
                    ultimaDirecao,
                    velocidadeDash2,
                    duracaoDash2
                )
            );

            StartCoroutine(
                RecarregarDash2()
            );

            return;
        }

        Debug.Log("Nenhum Dash disponível.");
    }
    // =========================================================
    // EXECUTAR DASH
    // =========================================================

    private IEnumerator ExecutarDash(
        Vector3 direcao,
        float velocidadeDoDash,
        float duracaoDoDash
    )
    {
        isDashing = true;
        isInvulneravel = true;

        Vector3 direcaoDash;

        if (direcao.magnitude >= 0.1f)
        {
            direcaoDash = direcao.normalized;
        }
        else
        {
            direcaoDash = transform.forward;
        }

        float tempoDash = 0f;

        while (tempoDash < duracaoDoDash)
        {
            float distancia =
                velocidadeDoDash *
                Time.fixedDeltaTime;

            // Verifica se existe uma parede/objeto sólido
            // antes de mover o Rigidbody.
            if (rb.SweepTest(
                direcaoDash,
                out RaycastHit hit,
                distancia,
                QueryTriggerInteraction.Ignore))
            {
                // Encontrou algo sólido.
                // Para o Dash imediatamente.
                Debug.Log(
                    "Dash bloqueado por: " +
                    hit.collider.name
                );

                break;
            }

            rb.MovePosition(
                rb.position +
                direcaoDash * distancia
            );

            tempoDash += Time.fixedDeltaTime;

            yield return new WaitForFixedUpdate();
        }

        isDashing = false;
        isInvulneravel = false;
    }

    // =========================================================
    // RECARREGAR DASH 1
    // =========================================================

    private IEnumerator RecarregarDash1()
    {
        float tempo = 0f;

        while (tempo < tempoRecargaDash1)
        {
            tempo += Time.deltaTime;

            float progresso =
                tempo / tempoRecargaDash1;

            if (barraCooldownDash1 != null)
            {
                barraCooldownDash1.value =
                    Mathf.Clamp01(progresso);
            }

            yield return null;
        }

        dash1Disponivel = true;

        if (barraCooldownDash1 != null)
        {
            barraCooldownDash1.value = 1f;
        }

        Debug.Log("Dash 1 recarregado!");
    }

    // =========================================================
    // RECARREGAR DASH 2
    // =========================================================

    private IEnumerator RecarregarDash2()
    {
        float tempo = 0f;

        while (tempo < tempoRecargaDash2)
        {
            tempo += Time.deltaTime;

            float progresso =
                tempo / tempoRecargaDash2;

            if (barraCooldownDash2 != null)
            {
                barraCooldownDash2.value =
                    Mathf.Clamp01(progresso);
            }

            yield return null;
        }

        dash2Disponivel = true;

        if (barraCooldownDash2 != null)
        {
            barraCooldownDash2.value = 1f;
        }

        Debug.Log("Dash 2 recarregado!");
    }

    // =========================================================
    // DANO DO DASH
    // =========================================================

    private void OnTriggerStay(Collider outro)
    {
        if (!isDashing)
            return;

        Enemy inimigo =
            outro.GetComponentInParent<Enemy>();

        if (inimigo != null)
        {
            Debug.Log(
                "DASH ACERTOU O INIMIGO!"
            );

            inimigo.ReceberDanoFatal();
        }
    }
}