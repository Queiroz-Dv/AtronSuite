# 🧰 Atron Suite

> **Otimize, Atualize e Monitore.** A suíte definitiva de manutenção e automação para Windows.

![Platform](https://img.shields.io/badge/platform-Windows_10%2F11-0078D6.svg) ![.NET](https://img.shields.io/badge/.NET-9.0-512BD4.svg) ![License](https://img.shields.io/badge/license-MIT-green.svg)

## 📖 Sobre o Projeto

O **Atron Suite** é uma ferramenta centralizada desenvolvida em **C# (Windows Forms)** para otimizar o sistema operacional. Diferente de limpadores comuns, ele integra uma engine de monitoramento passivo ("Sentinel") e utiliza o poder nativo do **Winget** da Microsoft para gerenciar softwares e drivers de forma inteligente.

**📥 Download Oficial:** [atron-suite-page.vercel.app](https://atron-suite-page.vercel.app/)

---

## ✨ Funcionalidades (Módulos)

O sistema é dividido em quatro ferramentas essenciais acessíveis via painel unificado:

### 🧹 1. Cleaner Monitor
Focado em privacidade e recuperação de espaço em disco.
* **Limpeza Profunda:** Remove arquivos de `Windows\Temp`, `Prefetch` e `AppData\Local\Temp`.
* **Integração Shell32:** Esvazia a Lixeira de todas as unidades via API nativa do Windows (`SHEmptyRecycleBin`).
* **Logs Detalhados:** Exibe relatório em tempo real do espaço recuperado.

### 🔄 2. Intelligent Updater
Interface gráfica (GUI) otimizada para o gerenciador de pacotes **Winget**.
* **Filtro Inteligente:** Separa automaticamente aplicativos comuns de drivers e componentes de sistema.
* **Bulk Update:** Atualize múltiplos programas simultaneamente com um único clique.
* **Modo Silencioso:** As atualizações rodam em background sem janelas intrusivas (`--accept-package-agreements`).

### 🚀 3. Driver Engine
Módulo especializado para manutenção de hardware e periféricos.
* **Busca Direcionada:** Utiliza palavras-chave específicas (`nvidia`, `amd`, `intel`, `realtek`, `chipset`, etc.) para encontrar apenas drivers.
* **Segurança:** Garante que você está baixando drivers assinados diretamente dos repositórios oficiais validados pela Microsoft.

### 🛡️ 4. Sentinel Patrol
Automação de monitoramento em segundo plano.
* **Always-on:** Minimiza para a bandeja do sistema (System Tray) e monitora o crescimento de arquivos temporários.
* **Configurável:** Defina limites de disparo (ex: alertar se passar de 5GB) e intervalos de verificação (1min a 2 horas).
* **Histórico JSON:** Mantém um log persistente (`sentinel_history.json`) de todas as análises e limpezas automáticas.

---

## 🛠️ Tecnologias Utilizadas

* **Linguagem:** C#
* **Framework:** .NET 9.0 (Windows Forms)
* **CLI Integration:** Wrapper para Microsoft Winget
* **Data:** JSON Nativo (`System.Runtime.Serialization.Json`)
* **System API:** `Shell32.dll` (Lixeira) e `System.Diagnostics.Process`

---

## 💻 Instalação e Uso

1. **Baixe o executável:** Acesse a [Página Oficial](https://atron-suite-page.vercel.app/).
2. **Extraia:** Descompacte o arquivo `.zip` em uma pasta de sua preferência.
3. **Execute como Admin:** Clique com o botão direito em `AtronSuite.exe` e selecione **"Executar como Administrador"** (necessário para limpar pastas do sistema como `Windows\Temp`).

---

## 🤝 Contato e Autoria

Desenvolvido e mantido por **Eduardo Queiroz**.

<div align="left">
  <a href="https://www.linkedin.com/in/eduardoqueirozdev/" target="_blank">
    <img src="https://img.shields.io/badge/LinkedIn-0077B5?style=for-the-badge&logo=linkedin&logoColor=white" alt="LinkedIn" />
  </a>
  <a href="https://github.com/Queiroz-Dv" target="_blank">
    <img src="https://img.shields.io/badge/GitHub-100000?style=for-the-badge&logo=github&logoColor=white" alt="GitHub" />
  </a>
  <a href="https://eduardo-queiroz.vercel.app/" target="_blank">
    <img src="https://img.shields.io/badge/Portfolio-20232A?style=for-the-badge&logo=react&logoColor=61DAFB" alt="Portfolio" />
  </a>
  <a href="https://www.instagram.com/qrz.queiroz/" target="_blank">
    <img src="https://img.shields.io/badge/Instagram-E4405F?style=for-the-badge&logo=instagram&logoColor=white" alt="Instagram" />
  </a>
</div>

<br>

📧 **Email:** teacher.eduardo.queiroz@gmail.com

---
<br>
*Nota: O Atron Suite utiliza ferramentas da Microsoft (Winget). Todas as marcas registradas pertencem aos seus respectivos proprietários.*
