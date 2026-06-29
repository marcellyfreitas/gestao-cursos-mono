# SGC - Deploy em Produção

## Requisitos

- VPS com Ubuntu 20.04+ ou Debian 11+
- Docker + Docker Compose (plugin)
- Domínio apontando para o IP da VPS
- Mínimo 2GB RAM

## Passo 1: Preparar o Servidor

```bash
sudo apt update && sudo apt upgrade -y

# Instalar Docker (já inclui docker compose como plugin)
curl -fsSL https://get.docker.com | sh
sudo usermod -aG docker $USER

# Verificar
docker compose version

# Firewall
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw enable
```

## Passo 2: Configurar Domínio (DNS)

No painel do seu provedor, adicione:
- Tipo A → `@` ou `www` → IP da VPS
- Tipo A → `api` → IP da VPS

## Passo 3: Configurar e Subir

```bash
cd /var/www/sgc

# Criar .env e editar com credenciais reais
cp docker/.env.example docker/.env
nano docker/.env
```

No `.env`, altere:
```env
ENV=prod
DOMAIN=seudominio.com.br
MYSQL_ROOT_PASSWORD=senha_segura_aqui
JwtSettings__SecretKey=chave_aleatoria_32_chars
# ... demais credenciais
```

As URLs são geradas automaticamente pelo script com base no `DOMAIN`.

```bash
# Subir em produção
chmod +x docker/sgc.sh
./docker/sgc.sh --up --prod
```

## Passo 4: SSL (Let's Encrypt)

```bash
docker exec -it sgc-nginx sh
certbot certonly --webroot -w /usr/share/nginx/html \
  -d www.seudominio.com.br -d api.seudominio.com.br
```

## Comandos do sgc.sh

```bash
# Ambiente local (desenvolvimento)
./docker/sgc.sh --up                  # Sobe local (localhost)
./docker/sgc.sh --down                # Para containers

# Ambiente de produção
./docker/sgc.sh --up --prod           # Sobe produção
./docker/sgc.sh --update --prod       # Git pull + rebuild + restart
./docker/sgc.sh --build --prod        # Só build (sem subir)

# Operações gerais
./docker/sgc.sh --status              # Status dos containers
./docker/sgc.sh --logs                # Todos os logs
./docker/sgc.sh --logs api            # Logs só da API
./docker/sgc.sh --restart             # Reinicia tudo
./docker/sgc.sh --restart mysql       # Reinicia só o MySQL

# Backup e restore
./docker/sgc.sh --backup              # Backup (mantém 7)
./docker/sgc.sh --backup 14           # Backup (mantém 14)
./docker/sgc.sh --restore backup_20260101_120000.sql

# Nginx
./docker/sgc.sh --nginx               # Regenera nginx.conf e recarrega
```

## Estrutura

```
docker/
├── .env.example              # Template de variáveis
├── .env                      # Suas credenciais (NÃO COMMITAR)
├── docker-compose.yml        # Base (serviços comuns)
├── docker-compose.local.yml  # Override: local (portas expostas, Swagger)
├── docker-compose.prod.yml   # Override: produção (SSL, limites)
├── nginx.conf.template       # Template nginx (usa ${DOMAIN})
├── nginx.conf                # Gerado automaticamente (NÃO EDITAR)
├── sgc.sh                    # Script unificado
├── ssl/                      # Certificados SSL (produção)
├── backups/                  # Backups MySQL
└── PRODUCAO.md               # Este arquivo
```

## Ambiente Local vs Produção

| | Local | Produção |
|---|---|---|
| DOMAIN | `localhost` | `seudominio.com.br` |
| Portas expostas | MySQL 3306, API 5000, Web 3000, Nginx 80 | Só Nginx 80/443 |
| Swagger | Habilitado | Desabilitado |
| SSL | Não | Sim |
| Limites de recursos | Sem limites | CPU/memória limitados |
| ASPNETCORE_ENVIRONMENT | Local | Production |

## Troubleshooting

```bash
# Ver logs de um serviço
./docker/sgc.sh --logs nginx
./docker/sgc.sh --logs api

# Verificar rede
docker network inspect docker_sgc-network

# Verificar saúde do MySQL
docker inspect sgc-mysql | grep -A5 Health

# Regenerar nginx após mudar DOMAIN
./docker/sgc.sh --nginx
```

## Variáveis de Ambiente

| Variável | Descrição |
|---|---|
| ENV | `local` ou `prod` |
| DOMAIN | Domínio (localhost ou seudominio.com.br) |
| MYSQL_ROOT_PASSWORD | Senha root do MySQL |
| MYSQL_DATABASE | Nome do banco (sistema_academico) |
| JwtSettings__SecretKey | Chave JWT (32+ caracteres) |
| SmtpSettings__SendGridApiKey | API Key do SendGrid |
| NEXT_PUBLIC_API_URL | URL da API (gerada pelo script) |
| FrontendUrl | URL do frontend (gerada pelo script) |
