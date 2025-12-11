# FraudHunter - Real-Time Fraud Detection System

> Um sistema de detecção de fraudes em alta performance usando **Machine Learning**, **Arquitetura Orientada a Eventos** e **Observabilidade em Tempo Real**.

![.NET](https://img.shields.io/badge/.NET%208-512BD4?style=flat&logo=dotnet&logoColor=white)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-FF6600?style=flat&logo=rabbitmq&logoColor=white)
![ML.NET](https://img.shields.io/badge/ML.NET-Machine%20Learning-blue)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=flat&logo=docker&logoColor=white)
![Grafana](https://img.shields.io/badge/Grafana-F46800?style=flat&logo=grafana&logoColor=white)

## 🧠 Sobre o Projeto

O **FraudHunter** simula um ecossistema bancário capaz de processar milhares de transações e identificar comportamentos suspeitos em milissegundos.

Diferente de sistemas baseados apenas em regras (`if value > x`), este projeto utiliza um **Motor Híbrido de Decisão**:
1.  **Regras Determinísticas (Hard Rules):** Bloqueia valores que excedem limites de segurança.
2.  **Inteligência Artificial (Soft Rules):** Um modelo de **Anomaly Detection (Randomized PCA)** treinado com ML.NET analisa 28 variáveis comportamentais (`V1`..`V28`) para identificar padrões invisíveis a humanos.

## 🏗️ Arquitetura

O sistema segue uma arquitetura de microsserviços distribuídos:

1.  **Producer (Simulador):** Gera fluxo contínuo de transações (Normais e Fraudes sintéticas).
2.  **Message Broker (RabbitMQ):** Garante o desacoplamento e a resiliência do sistema, aguentando picos de carga.
3.  **Detector (Worker Service):** Consome as mensagens, carrega o modelo de IA em memória e processa a predição.
4.  **Observabilidade (Prometheus + Grafana):** Monitoramento em tempo real de TPS (Transações por Segundo) e Alertas de Fraude.

---

## 🚀 Como Rodar o Projeto

### Pré-requisitos
* [Docker Desktop](https://www.docker.com/) instalado.
* [.NET 8 SDK](https://dotnet.microsoft.com/download) instalado.

### 1. Subir a Infraestrutura
Na raiz do projeto, execute o Docker Compose para subir o RabbitMQ, Prometheus e Grafana:

```bash
docker-compose up -d

2. Executar o Sistema
Abra dois terminais na raiz do projeto:

Terminal 1 - O Detector (Cérebro):

dotnet run --project FraudHunter.Detector


Terminal 2 - O Simulador (Gerador de Carga):

dotnet run --project FraudHunter.Producer

3. Acessar o Dashboard
Acesse http://localhost:3000 (Login: admin / Senha: admin).

Se o Dashboard não aparecer automaticamente, vá em Dashboards > Import e carregue o arquivo dashboard.json disponível neste repositório.