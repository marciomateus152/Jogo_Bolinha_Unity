using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using System.Collections.Generic;

public class MotorDoJogo : MonoBehaviour
{
    private enum EstadoJogo { MenuPrincipal, Jogando, FimDeJogo }
    private EstadoJogo estadoAtual;

    private GameObject telaCanvas;
    private GameObject painelMenu;
    private GameObject painelJogo;
    private GameObject painelFim;
    private Text textoPontuacao;
    private Text textoTitulo;

    private GameObject jogador;
    private Camera cameraPrincipal;
    private List<GameObject> inimigos = new List<GameObject>();
    private List<GameObject> projeteis = new List<GameObject>();
    private List<GameObject> particulas = new List<GameObject>();

    private int pontuacao = 0;
    private float taxaDeGeracao = 1.5f;
    private float proximaGeracao = 0f;
    private float tempoTremor = 0f;
    private float intensidadeTremor = 0f;
    private float escalaPontuacao = 1f;

    void Start()
    {
        cameraPrincipal = Camera.main;
        if (cameraPrincipal == null)
        {
            GameObject objCamera = new GameObject("CameraPrincipal");
            cameraPrincipal = objCamera.AddComponent<Camera>();
            objCamera.tag = "MainCamera";
        }
        
        cameraPrincipal.orthographic = true;
        cameraPrincipal.orthographicSize = 12f;
        cameraPrincipal.backgroundColor = new Color(0.02f, 0.02f, 0.04f);
        cameraPrincipal.clearFlags = CameraClearFlags.SolidColor;

        CriarSistemaDeEventos();
        ConstruirInterface();
        MudarEstado(EstadoJogo.MenuPrincipal);
    }

    void Update()
    {
        LidarComTremorDaCamera();
        AnimarInterface();

        if (estadoAtual == EstadoJogo.Jogando)
        {
            LidarMovimentoJogador();
            LidarTiroJogador();
            GerenciarInimigos();
            AtualizarEntidades();
            ChecarColisoes();
        }
        else if (estadoAtual == EstadoJogo.MenuPrincipal)
        {
            if (textoTitulo != null)
            {
                float escala = 1f + Mathf.Sin(Time.time * 3f) * 0.05f;
                textoTitulo.transform.localScale = new Vector3(escala, escala, 1f);
            }
        }
    }

    void ConstruirInterface()
    {
        telaCanvas = new GameObject("TelaPrincipal");
        Canvas canvas = telaCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        telaCanvas.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        telaCanvas.AddComponent<GraphicRaycaster>();

        painelMenu = CriarPainelUI("PainelMenu", new Color(0.02f, 0.02f, 0.04f, 1f));
        textoTitulo = CriarTextoUI(painelMenu.transform, "NEON\nSOBREVIVÊNCIA", 70, new Vector2(0, 150), new Color(0f, 1f, 0.8f));
        CriarBotaoUI(painelMenu.transform, "INICIAR CONEXÃO", new Vector2(0, -80), IniciarJogo);

        painelJogo = CriarPainelUI("PainelJogo", Color.clear);
        textoPontuacao = CriarTextoUI(painelJogo.transform, "PONTOS: 0", 35, new Vector2(0, 350), Color.white);

        painelFim = CriarPainelUI("PainelFim", new Color(0.7f, 0f, 0.2f, 0.9f));
        CriarTextoUI(painelFim.transform, "SINAL PERDIDO", 70, new Vector2(0, 150), Color.white);
        CriarBotaoUI(painelFim.transform, "TENTAR NOVAMENTE", new Vector2(0, -80), () => MudarEstado(EstadoJogo.MenuPrincipal));
    }

    void MudarEstado(EstadoJogo novoEstado)
    {
        estadoAtual = novoEstado;
        painelMenu.SetActive(estadoAtual == EstadoJogo.MenuPrincipal);
        painelJogo.SetActive(estadoAtual == EstadoJogo.Jogando);
        painelFim.SetActive(estadoAtual == EstadoJogo.FimDeJogo);

        if (novoEstado == EstadoJogo.MenuPrincipal) LimparArena();
    }

    void IniciarJogo()
    {
        pontuacao = 0;
        taxaDeGeracao = 1.5f;
        AtualizarTextoPontuacao();
        LimparArena();
        GerarJogador();
        MudarEstado(EstadoJogo.Jogando);
    }

    void GerarJogador()
    {
        jogador = new GameObject("Jogador");
        jogador.transform.position = Vector3.zero;
        
        SpriteRenderer sr = jogador.AddComponent<SpriteRenderer>();
        sr.sprite = CriarSpriteCirculo(new Color(0f, 1f, 0.8f), 64);
        
        TrailRenderer rastro = jogador.AddComponent<TrailRenderer>();
        rastro.time = 0.15f;
        rastro.startWidth = 0.5f;
        rastro.endWidth = 0f;
        rastro.material = new Material(Shader.Find("Sprites/Default"));
        rastro.startColor = new Color(0f, 1f, 0.8f);
        rastro.endColor = new Color(0f, 1f, 0.8f, 0f);
    }

    void LidarMovimentoJogador()
    {
        if (jogador == null || Keyboard.current == null || Mouse.current == null) return;

        Vector2 direcaoTecla = Vector2.zero;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) direcaoTecla.y += 1;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) direcaoTecla.y -= 1;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) direcaoTecla.x -= 1;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) direcaoTecla.x += 1;

        Vector3 movimento = new Vector3(direcaoTecla.x, direcaoTecla.y, 0).normalized;
        jogador.transform.position += movimento * 12f * Time.deltaTime;

        Vector2 posicaoMouseTela = Mouse.current.position.ReadValue();
        Vector3 posicaoMouseMundo = cameraPrincipal.ScreenToWorldPoint(new Vector3(posicaoMouseTela.x, posicaoMouseTela.y, 0));
        posicaoMouseMundo.z = 0;
        
        Vector3 direcaoOlhar = posicaoMouseMundo - jogador.transform.position;
        jogador.transform.up = direcaoOlhar;
    }

    void LidarTiroJogador()
    {
        if (jogador == null || Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            AtivarTremor(0.04f, 0.15f);
            
            GameObject projetil = new GameObject("Projetil");
            projetil.transform.position = jogador.transform.position + jogador.transform.up * 0.8f;
            projetil.transform.up = jogador.transform.up;
            
            SpriteRenderer sr = projetil.AddComponent<SpriteRenderer>();
            sr.sprite = CriarSpriteCirculo(new Color(1f, 0.9f, 0f), 20);
            
            TrailRenderer rastro = projetil.AddComponent<TrailRenderer>();
            rastro.time = 0.1f;
            rastro.startWidth = 0.2f;
            rastro.endWidth = 0f;
            rastro.material = new Material(Shader.Find("Sprites/Default"));
            rastro.startColor = Color.yellow;
            rastro.endColor = Color.clear;

            projeteis.Add(projetil);
        }
    }

    void GerenciarInimigos()
    {
        if (Time.time >= proximaGeracao)
        {
            CriarInimigo();
            proximaGeracao = Time.time + taxaDeGeracao;
            taxaDeGeracao = Mathf.Max(0.25f, taxaDeGeracao - 0.03f);
        }
    }

    void CriarInimigo()
    {
        GameObject inimigo = new GameObject("Inimigo");
        Vector3 posicaoGeracao = new Vector3(Random.Range(-18f, 18f), 14f, 0);
        if (Random.value > 0.5f) posicaoGeracao = new Vector3(20f, Random.Range(-12f, 12f), 0);
        if (Random.value > 0.75f) posicaoGeracao *= -1;

        inimigo.transform.position = posicaoGeracao;
        SpriteRenderer sr = inimigo.AddComponent<SpriteRenderer>();
        sr.sprite = CriarSpriteCirculo(new Color(1f, 0.1f, 0.4f), 50);
        
        inimigos.Add(inimigo);
    }

    void AtualizarEntidades()
    {
        for (int i = projeteis.Count - 1; i >= 0; i--)
        {
            GameObject p = projeteis[i];
            p.transform.position += p.transform.up * 30f * Time.deltaTime;
            if (Mathf.Abs(p.transform.position.x) > 30f || Mathf.Abs(p.transform.position.y) > 25f)
            {
                Destroy(p);
                projeteis.RemoveAt(i);
            }
        }

        for (int i = inimigos.Count - 1; i >= 0; i--)
        {
            GameObject ini = inimigos[i];
            if (ini != null && jogador != null)
            {
                Vector3 direcao = (jogador.transform.position - ini.transform.position).normalized;
                ini.transform.position += direcao * 5.5f * Time.deltaTime;
            }
        }

        for (int i = particulas.Count - 1; i >= 0; i--)
        {
            GameObject part = particulas[i];
            SpriteRenderer sr = part.GetComponent<SpriteRenderer>();
            Color cor = sr.color;
            cor.a -= Time.deltaTime * 2.5f;
            sr.color = cor;
            
            part.transform.position += part.transform.up * 3f * Time.deltaTime;
            part.transform.localScale -= Vector3.one * Time.deltaTime * 2f;

            if (cor.a <= 0 || part.transform.localScale.x <= 0)
            {
                Destroy(part);
                particulas.RemoveAt(i);
            }
        }
    }

    void ChecarColisoes()
    {
        for (int i = inimigos.Count - 1; i >= 0; i--)
        {
            GameObject ini = inimigos[i];
            bool destruido = false;

            for (int j = projeteis.Count - 1; j >= 0; j--)
            {
                GameObject proj = projeteis[j];
                if (Vector3.Distance(ini.transform.position, proj.transform.position) < 1.3f)
                {
                    GerarExplosao(ini.transform.position, new Color(1f, 0.1f, 0.4f));
                    Destroy(ini);
                    Destroy(proj);
                    inimigos.RemoveAt(i);
                    projeteis.RemoveAt(j);
                    AdicionarPontos(100);
                    AtivarTremor(0.08f, 0.3f);
                    destruido = true;
                    break;
                }
            }

            if (!destruido && jogador != null && Vector3.Distance(jogador.transform.position, ini.transform.position) < 1.2f)
            {
                GerarExplosao(jogador.transform.position, new Color(0f, 1f, 0.8f));
                AtivarTremor(0.5f, 0.8f);
                MudarEstado(EstadoJogo.FimDeJogo);
                return;
            }
        }
    }

    void GerarExplosao(Vector3 posicao, Color corBase)
    {
        int quantidade = Random.Range(6, 12);
        for (int i = 0; i < quantidade; i++)
        {
            GameObject fragmento = new GameObject("Fragmento");
            fragmento.transform.position = posicao;
            fragmento.transform.eulerAngles = new Vector3(0, 0, Random.Range(0f, 360f));
            
            SpriteRenderer sr = fragmento.AddComponent<SpriteRenderer>();
            sr.sprite = CriarSpriteCirculo(corBase, Random.Range(15, 30));
            particulas.Add(fragmento);
        }
    }

    void AdicionarPontos(int valor)
    {
        pontuacao += valor;
        AtualizarTextoPontuacao();
        escalaPontuacao = 1.4f; 
    }

    void AtualizarTextoPontuacao()
    {
        if (textoPontuacao != null) textoPontuacao.text = "PONTOS: " + pontuacao;
    }

    void AnimarInterface()
    {
        if (textoPontuacao != null)
        {
            escalaPontuacao = Mathf.Lerp(escalaPontuacao, 1f, Time.deltaTime * 10f);
            textoPontuacao.transform.localScale = new Vector3(escalaPontuacao, escalaPontuacao, 1f);
        }
    }

    void LimparArena()
    {
        if (jogador != null) Destroy(jogador);
        foreach (GameObject i in inimigos) Destroy(i);
        foreach (GameObject p in projeteis) Destroy(p);
        foreach (GameObject pt in particulas) Destroy(pt);
        inimigos.Clear();
        projeteis.Clear();
        particulas.Clear();
    }

    void AtivarTremor(float duracao, float intensidade)
    {
        tempoTremor = duracao;
        intensidadeTremor = intensidade;
    }

    void LidarComTremorDaCamera()
    {
        if (tempoTremor > 0)
        {
            cameraPrincipal.transform.position = new Vector3(
                Random.Range(-intensidadeTremor, intensidadeTremor), 
                Random.Range(-intensidadeTremor, intensidadeTremor), 
                -10f);
            tempoTremor -= Time.deltaTime;
        }
        else
        {
            cameraPrincipal.transform.position = new Vector3(0, 0, -10f);
        }
    }

    Sprite CriarSpriteCirculo(Color cor, int tamanho)
    {
        Texture2D textura = new Texture2D(tamanho, tamanho);
        Color[] pixels = new Color[tamanho * tamanho];
        float raio = tamanho / 2f;
        
        for (int y = 0; y < tamanho; y++)
        {
            for (int x = 0; x < tamanho; x++)
            {
                float distancia = Vector2.Distance(new Vector2(x, y), new Vector2(raio, raio));
                pixels[y * tamanho + x] = distancia <= raio ? cor : Color.clear;
            }
        }
        
        textura.SetPixels(pixels);
        textura.Apply();
        return Sprite.Create(textura, new Rect(0, 0, tamanho, tamanho), new Vector2(0.5f, 0.5f), tamanho);
    }

    void CriarSistemaDeEventos()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject sistemaEventos = new GameObject("SistemaDeEventos");
            sistemaEventos.AddComponent<EventSystem>();
            sistemaEventos.AddComponent<InputSystemUIInputModule>();
        }
    }

    GameObject CriarPainelUI(string nome, Color corFundo)
    {
        GameObject painel = new GameObject(nome);
        painel.transform.SetParent(telaCanvas.transform, false);
        Image img = painel.AddComponent<Image>();
        img.color = corFundo;
        RectTransform rect = painel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        return painel;
    }

    Text CriarTextoUI(Transform pai, string texto, int tamanhoFonte, Vector2 posicao, Color corTexto)
    {
        GameObject objTexto = new GameObject("Texto");
        objTexto.transform.SetParent(pai, false);
        Text txt = objTexto.AddComponent<Text>();
        txt.text = texto;
        
        // Changed to LegacyRuntime.ttf here
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); 
        
        txt.fontSize = tamanhoFonte;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = corTexto;
        RectTransform rect = objTexto.GetComponent<RectTransform>();
        rect.anchoredPosition = posicao;
        rect.sizeDelta = new Vector2(800, 200);
        return txt;
    }

    void CriarBotaoUI(Transform pai, string texto, Vector2 posicao, UnityEngine.Events.UnityAction acaoClique)
    {
        GameObject objBotao = new GameObject("Botao");
        objBotao.transform.SetParent(pai, false);
        Image img = objBotao.AddComponent<Image>();
        img.color = new Color(0.1f, 0.7f, 0.5f);
        Button btn = objBotao.AddComponent<Button>();
        btn.onClick.AddListener(acaoClique);
        RectTransform rect = objBotao.GetComponent<RectTransform>();
        rect.anchoredPosition = posicao;
        rect.sizeDelta = new Vector2(350, 70);
        CriarTextoUI(objBotao.transform, texto, 24, Vector2.zero, Color.white);
    }
}