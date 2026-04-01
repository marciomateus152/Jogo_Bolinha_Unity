# 🎮 NEON SOBREVIVÊNCIA

## 📝 Descrição

Projeto desenvolvido em **Unity (C#)** que implementa um jogo 2D de sobrevivência com geração procedural de entidades, controle em tempo real via **Unity Input System** e interface construída integralmente por código.

O núcleo do sistema está concentrado na classe `MotorDoJogo`, responsável por orquestrar o ciclo de vida da aplicação, gerenciamento de estados, entrada do usuário, renderização básica e lógica de jogo.

---

## 🏗️ Arquitetura

O projeto segue uma estrutura monolítica orientada a um controlador central:

### `MotorDoJogo.cs` ⚙️

Responsabilidades principais:

* 🔹 Gerenciamento de estados (`MenuPrincipal`, `Jogando`, `FimDeJogo`)
* 🔹 Criação dinâmica da UI (`Canvas`, `Text`, `Button`)
* 🔹 Controle de input via `Keyboard` e `Mouse` (Input System)
* 🔹 Spawn e atualização de entidades (`GameObject`)
* 🔹 Sistema de colisão baseado em distância (`Vector3.Distance`)
* 🔹 Sistema de partículas procedural
* 🔹 Feedback visual (TrailRenderer, camera shake)
* 🔹 Controle de pontuação e progressão de dificuldade

---

## 🔄 Fluxo de Execução

```mermaid
flowchart TD
    %% Estilização para o Tema Neon
    classDef startup fill:#1a1a1a,stroke:#00f2ff,stroke-width:2px,color:#fff;
    classDef state fill:#2d333b,stroke:#adbac7,stroke-width:1px,color:#adbac7;
    classDef loop fill:#004d40,stroke:#00c853,stroke-width:1px,color:#fff;
    classDef critical fill:#4a0000,stroke:#ff5252,stroke-width:1px,color:#fff;

    subgraph Boot [Ciclo Inicial - Start]
        A([EntryPoint: Start]) --> B[Setup: Câmera & EventSystem]
        B --> C[Construção Programática da UI]
        C --> D[[Estado: MenuPrincipal]]
    end

    D -->|Evento de Botão| E[IniciarJogo]

    subgraph Gameplay [Ciclo Principal - Update]
        E --> F[Reset de Variáveis & Pontuação]
        F --> G[Instanciar Player & Inimigos]
        G --> H[[Estado: Jogando]]
        
        H --> I[Leitura de Input: Teclado/Mouse]
        I --> J[Orquestração de Spawners]
        J --> K[Atualizar Transformações/Física]
        K --> L{Cálculo de Colisão}
        
        L -- "Distância > Limiar" --> H
    end

    subgraph Encerramento [Fim de Ciclo]
        L -- "Distância <= Limiar" --> M[Trigger: FimDeJogo]
        M --> N[[Estado: Tela de Game Over]]
        N -->|Input Reinício| D
    end

    class A,B,C startup;
    class D,H,N state;
    class I,J,K loop;
    class L,M critical;
```
---

## 🛠️ Principais Componentes Técnicos

### ⌨️ Entrada de Dados
* **Sistema:** Utilização do pacote `UnityEngine.InputSystem`.
* **Leitura:** Acesso direto aos dispositivos via `Keyboard.current` e `Mouse.current`.
* **Arquitetura:** Separação clara entre o input de movimento e ações de disparo.

### 📦 Sistema de Entidades
* **Instanciação:** Gerenciamento dinâmico via `new GameObject()`.
* **Gerenciamento:** Armazenamento centralizado em listas (`List<GameObject>`).
* **Ciclo de Vida:** Atualização manual por frame através do método `Update()`, garantindo controle total sobre a execução.

### 🖼️ Renderização
* **Sprites:** Uso de `SpriteRenderer` com texturas geradas proceduralmente via `Texture2D`.
* **Feedback:** Implementação de `TrailRenderer` para rastro visual dos objetos.

### 🖥️ UI (Interface do Usuário)
Construção feita de forma programática, sem dependência excessiva do Inspector:
* **Estrutura:** `Canvas`, `CanvasScaler` e `GraphicRaycaster`.
* **Texto:** Gerenciamento de fontes via `Resources`.
* **Interação:** `Button` utilizando `UnityAction`.

### ⚛️ Física e Colisão
* **Modelo:** Sistema simplificado baseado em **distância euclidiana**.
* **Otimização:** Não utiliza `Collider` ou `Rigidbody` nativos, reduzindo o overhead do motor de física para cálculos manuais de alta performance.

### 🎆 Efeitos Visuais
* **Partículas:** Sistema customizado gerado via múltiplos `GameObject`.
* **Dinâmica:** Decaimento de *alpha* (transparência) e escala ao longo do tempo.
* **Polimento:** Efeito de *Camera Shake* (tremor de câmera) implementado via deslocamento aleatório de coordenadas.

### 📈 Progressão de Dificuldade
* **Escalabilidade:** Redução progressiva do intervalo de spawn (`taxaDeGeracao`).
* **Controle:** Definição de limites mínimos para manter a jogabilidade balanceada.

---

## 🚀 Como Executar

Para rodar o projeto localmente, siga os passos abaixo:

1.  **Clonar o repositório:**
    ```bash
    git clone [https://github.com/marciomateus152/Jogo_Bolinha_Unity.git](https://github.com/marciomateus152/Jogo_Bolinha_Unity.git)
    ```
2.  **Abrir no Unity:**
    * Abra o **Unity Hub**.
    * Clique em `Add` e selecione a pasta do projeto clonado.
    * Certifique-se de usar a versão do Unity recomendada.
3.  **Rodar o Jogo:**
    * Abra a cena principal (`MainScene` ou similar) e clique no botão **Play**.

---

## 💡 Considerações de Design

O projeto foi construído com os seguintes pilares:

* ✅ **Baixo acoplamento externo:** Mínima dependência de assets da Asset Store.
* ✅ **Geração procedural:** Elementos visuais criados via código.
* ✅ **Ciclo de atualização explícito:** Menos dependência do "Magic Methods" ocultos da Unity.
* ✅ **Estrutura simples:** Ideal para prototipação rápida e fácil expansão de funcionalidades.
