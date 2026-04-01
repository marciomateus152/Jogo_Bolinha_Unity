# 🎮 NEON SOBREVIVÊNCIA

## Descrição

Projeto desenvolvido em **Unity (C#)** que implementa um jogo 2D de sobrevivência com geração procedural de entidades, controle em tempo real via **Unity Input System** e interface construída integralmente por código.

O núcleo do sistema está concentrado na classe `MotorDoJogo`, responsável por orquestrar o ciclo de vida da aplicação, gerenciamento de estados, entrada do usuário, renderização básica e lógica de jogo.

---

## Arquitetura

O projeto segue uma estrutura monolítica orientada a um controlador central:

### `MotorDoJogo.cs`

Responsabilidades principais:

* 🔹Gerenciamento de estados (`MenuPrincipal`, `Jogando`, `FimDeJogo`)
* 🔹Criação dinâmica da UI (`Canvas`, `Text`, `Button`)
* 🔹Controle de input via `Keyboard` e `Mouse` (Input System)
* 🔹Spawn e atualização de entidades (`GameObject`)
* 🔹Sistema de colisão baseado em distância (`Vector3.Distance`)
* 🔹Sistema de partículas procedural
* 🔹Feedback visual (TrailRenderer, camera shake)
* 🔹Controle de pontuação e progressão de dificuldade

---

## Fluxo de Execução

```mermaid
flowchart TD
    A[Start()] --> B[Inicialização da Câmera]
    B --> C[Criação do EventSystem]
    C --> D[Construção da Interface]
    D --> E[Estado: MenuPrincipal]

    E -->|Input do botão| F[IniciarJogo()]
    F --> G[Reset de variáveis]
    G --> H[Spawn do Jogador]
    H --> I[Estado: Jogando]

    I --> J[Update()]
    J --> K[Input do jogador]
    J --> L[Disparo de projéteis]
    J --> M[Spawn de inimigos]
    J --> N[Atualização de entidades]
    J --> O[Detecção de colisões]

    O -->|Colisão com inimigo| P[FimDeJogo]
    P --> Q[Estado: FimDeJogo]
    Q -->|Reinício| E
```

---

## Principais Componentes Técnicos

### Entrada de Dados

* Utilização do pacote **UnityEngine.InputSystem**
* Leitura direta de dispositivos (`Keyboard.current`, `Mouse.current`)
* Separação entre input de movimento e ação (disparo)

### Sistema de Entidades

* Instanciação dinâmica via `new GameObject()`
* Armazenamento em listas (`List<GameObject>`)
* Atualização manual por frame (`Update()`)

### Renderização

* `SpriteRenderer` com sprites gerados proceduralmente (`Texture2D`)
* Uso de `TrailRenderer` para feedback visual

### UI

* Construção programática com:

  * `Canvas`
  * `CanvasScaler`
  * `GraphicRaycaster`
  * `Text` (fonte padrão via `Resources`)
  * `Button` com `UnityAction`

### Física e Colisão

* Modelo simplificado baseado em distância euclidiana
* Sem uso de `Collider` ou `Rigidbody`

### Efeitos Visuais

* Partículas geradas via múltiplos `GameObject`
* Decaimento de alpha e escala ao longo do tempo
* Efeito de tremor de câmera via deslocamento aleatório

### Progressão de Dificuldade

* Redução progressiva do intervalo de spawn (`taxaDeGeracao`)
* Limite mínimo para controle de escalabilidade

---

## Execução

```bash
git clone https://github.com/marciomateus152/Jogo_Bolinha_Unity.git
```

Abrir o projeto no **Unity Hub** e executar a cena principal.

---

## Considerações

O projeto prioriza:

* Baixo acoplamento externo (mínima dependência de assets)
* Geração procedural de elementos
* Controle explícito do ciclo de atualização
* Estrutura simples para prototipação rápida e expansão futura
