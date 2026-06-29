#!/bin/bash
# sgc.sh — Script unificado do SGC
# Uso: ./docker/sgc.sh <comando> [opções]
#
# Comandos:
#   --up      [--prod]          Sobe os containers (default: local)
#   --down                      Para e remove containers
#   --build   [--prod]          Build das imagens (sem subir)
#   --update  [--prod]          Git pull + rebuild + restart
#   --backup  [keep_count]      Backup do MySQL (default: mantém 7)
#   --restore <arquivo.sql>     Restaura backup no MySQL
#   --status                    Status dos containers
#   --logs    [serviço]         Logs (api, webapp, mysql, nginx)
#   --restart [serviço]         Reinicia containers (ou um específico)
#   --nginx                     Regenera nginx.conf a partir do template
#   --help                      Mostra esta ajuda
#
# Exemplos:
#   ./docker/sgc.sh --up                  # Sobe ambiente local
#   ./docker/sgc.sh --up --prod           # Sobe ambiente de produção
#   ./docker/sgc.sh --backup 14           # Backup mantendo 14 cópias
#   ./docker/sgc.sh --logs api            # Logs só da API

set -e

DOCKER_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_ROOT="$(dirname "$(dirname "$DOCKER_DIR")")"
MYSQL_CONTAINER="sgc-mysql"

# Cores
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m'

log()  { echo -e "${GREEN}[SGC]${NC} $1"; }
warn() { echo -e "${YELLOW}[SGC]${NC} $1"; }
err()  { echo -e "${RED}[SGC]${NC} $1" >&2; }

# -----------------------------------------------
# Helpers
# -----------------------------------------------

load_env() {
    local ENV_FILE="$DOCKER_DIR/.env"

    # Se --prod foi passado, usa .env.production
    if [ "${ENV:-}" = "prod" ]; then
        ENV_FILE="$DOCKER_DIR/.env.production"
    fi

    if [ ! -f "$ENV_FILE" ]; then
        err "Arquivo não encontrado: $ENV_FILE"
        exit 1
    fi

    # shellcheck disable=SC1090
    source "$ENV_FILE"
    export ENV_FILE
}

compose_cmd() {
    local ENV_NAME="${ENV:-local}"
    local OVERRIDE="$DOCKER_DIR/docker-compose.${ENV_NAME}.yml"
    local ENV_FILE="${ENV_FILE:-$DOCKER_DIR/.env}"

    if [ ! -f "$OVERRIDE" ]; then
        err "Override não encontrado: docker-compose.${ENV_NAME}.yml"
        exit 1
    fi

    docker compose \
        --env-file "$ENV_FILE" \
        -f "$DOCKER_DIR/docker-compose.yml" \
        -f "$OVERRIDE" \
        "$@"
}

generate_nginx_conf() {
    local DOMAIN="${DOMAIN:-localhost}"

    log "Gerando nginx.conf (DOMAIN=$DOMAIN)..."

    # envsubst só substitui as variáveis listadas, preservando $http_upgrade etc.
    DOMAIN="$DOMAIN" envsubst '${DOMAIN}' \
        < "$DOCKER_DIR/nginx/nginx.conf.template" \
        > "$DOCKER_DIR/nginx.conf"

    log "nginx.conf gerado com sucesso."
}

get_mysql_pass() {
    local PASS="root_password"
    if [ -f "$DOCKER_DIR/.env" ]; then
        local FROM_ENV
        FROM_ENV=$(grep -oP 'MYSQL_ROOT_PASSWORD=\K.*' "$DOCKER_DIR/.env" 2>/dev/null || true)
        if [ -n "$FROM_ENV" ]; then
            PASS="$FROM_ENV"
        fi
    fi
    echo "$PASS"
}

check_prod_flag() {
    for arg in "$@"; do
        if [ "$arg" = "--prod" ]; then
            export ENV=prod
            return
        fi
    done
}

# -----------------------------------------------
# Comandos
# -----------------------------------------------

show_help() {
    echo ""
    echo -e "${CYAN}SGC — Sistema de Gestão de Cursos${NC}"
    echo ""
    echo "Uso: ./docker/sgc.sh <comando> [opções]"
    echo ""
    echo "Comandos:"
    echo "  --up      [--prod]          Sobe os containers (default: local)"
    echo "  --down                      Para e remove containers"
    echo "  --build   [--prod]          Build das imagens (sem subir)"
    echo "  --update  [--prod]          Git pull + rebuild + restart"
    echo "  --backup  [keep_count]      Backup do MySQL (default: mantém 7)"
    echo "  --restore <arquivo.sql>     Restaura backup no MySQL"
    echo "  --status                    Status dos containers"
    echo "  --logs    [serviço]         Logs (api, webapp, mysql, nginx)"
    echo "  --restart [serviço]         Reinicia containers (ou um específico)"
    echo "  --nginx                     Regenera nginx.conf a partir do template"
    echo "  --help                      Mostra esta ajuda"
    echo ""
    echo "Ambiente:"
    echo "  Por padrão usa ENV do .env (local/prod)."
    echo "  Use --prod para forçar produção."
    echo ""
    echo "Exemplos:"
    echo "  ./docker/sgc.sh --up                  # Local"
    echo "  ./docker/sgc.sh --up --prod           # Produção"
    echo "  ./docker/sgc.sh --backup 14"
    echo "  ./docker/sgc.sh --logs api"
    echo ""
}

cmd_up() {
    check_prod_flag "$@"
    load_env
    local ENV_NAME="${ENV:-local}"

    log "Subindo SGC [${ENV_NAME}]"
    log "DOMAIN=${DOMAIN:-localhost}"
    echo ""

    # Gerar nginx.conf
    generate_nginx_conf

    # Criar diretório SSL se produção
    if [ "$ENV_NAME" = "prod" ]; then
        mkdir -p "$DOCKER_DIR/ssl"
    fi

    log "1/3 Build das imagens..."
    compose_cmd build

    log "2/3 Subindo containers..."
    compose_cmd up -d

    log "3/3 Aguardando inicialização..."
    sleep 15

    echo ""
    compose_cmd ps
    echo ""
    log "SGC rodando!"

    if [ "$ENV_NAME" = "local" ]; then
        echo ""
        echo -e "  Frontend: ${CYAN}http://localhost${NC}"
        echo -e "  API:      ${CYAN}http://localhost:5070${NC}"
        echo -e "  Swagger:  ${CYAN}http://localhost:5070/swagger${NC}"
        echo -e "  MySQL:    ${CYAN}localhost:3306${NC}"
    else
        echo ""
        echo -e "  Frontend: ${CYAN}http://${DOMAIN}${NC}"
        echo -e "  API:      ${CYAN}http://api.${DOMAIN}${NC}"
        echo ""
        echo "  Próximos passos (se SSL ainda não configurado):"
        echo "  docker exec sgc-nginx certbot --nginx -d www.${DOMAIN} -d api.${DOMAIN}"
    fi
}

cmd_down() {
    check_prod_flag "$@"
    load_env
    log "Parando containers..."
    compose_cmd down
    log "Containers parados."
}

cmd_build() {
    check_prod_flag "$@"
    load_env
    local ENV_NAME="${ENV:-local}"

    log "Build SGC [${ENV_NAME}]..."
    generate_nginx_conf
    compose_cmd build --no-cache
    log "Build concluído!"
}

cmd_update() {
    check_prod_flag "$@"
    load_env
    local ENV_NAME="${ENV:-local}"

    log "Atualizando SGC [${ENV_NAME}]..."
    echo ""

    log "1/4 Git pull..."
    git -C "$PROJECT_ROOT" pull

    log "2/4 Regenerando nginx.conf..."
    generate_nginx_conf

    log "3/4 Rebuild das imagens..."
    compose_cmd build --no-cache

    log "4/4 Recriando containers..."
    compose_cmd up -d

    echo ""
    compose_cmd ps
    log "Atualização concluída!"
}

cmd_backup() {
    local KEEP="${1:-7}"
    local DATE
    DATE=$(date +%Y%m%d_%H%M%S)
    local BACKUP_DIR="$DOCKER_DIR/backups"
    local BACKUP_FILE="$BACKUP_DIR/backup_$DATE.sql"

    log "Backup MySQL SGC"
    mkdir -p "$BACKUP_DIR"

    local MYSQL_PASS
    MYSQL_PASS=$(get_mysql_pass)

    log "Criando backup: backup_$DATE.sql"
    docker exec "$MYSQL_CONTAINER" mysqldump -u root -p"$MYSQL_PASS" sistema_academico > "$BACKUP_FILE"

    local SIZE
    SIZE=$(du -h "$BACKUP_FILE" | cut -f1)
    log "Tamanho: $SIZE"

    # Limpar backups antigos
    local OLD_COUNT
    OLD_COUNT=$(ls -1 "$BACKUP_DIR"/backup_*.sql 2>/dev/null | wc -l)
    if [ "$OLD_COUNT" -gt "$KEEP" ]; then
        warn "Removendo backups antigos (mantendo últimos $KEEP)..."
        ls -t "$BACKUP_DIR"/backup_*.sql | tail -n +$((KEEP + 1)) | xargs -r rm
    fi

    echo ""
    log "Backups disponíveis:"
    ls -lh "$BACKUP_DIR"/backup_*.sql 2>/dev/null || echo "  Nenhum backup encontrado."
    log "Backup concluído!"
}

cmd_restore() {
    local SQL_FILE="$1"

    if [ -z "$SQL_FILE" ]; then
        err "Uso: ./docker/sgc.sh --restore <arquivo.sql>"
        exit 1
    fi

    if [ ! -f "$SQL_FILE" ]; then
        # Tentar caminho relativo ao diretório de backups
        if [ -f "$DOCKER_DIR/backups/$SQL_FILE" ]; then
            SQL_FILE="$DOCKER_DIR/backups/$SQL_FILE"
        else
            err "Arquivo não encontrado: $SQL_FILE"
            exit 1
        fi
    fi

    local MYSQL_PASS
    MYSQL_PASS=$(get_mysql_pass)

    warn "Restaurando $SQL_FILE no banco sistema_academico..."
    docker exec -i "$MYSQL_CONTAINER" mysql -u root -p"$MYSQL_PASS" sistema_academico < "$SQL_FILE"
    log "Restore concluído!"
}

cmd_status() {
    load_env
    log "Status dos containers:"
    echo ""
    compose_cmd ps
}

cmd_logs() {
    load_env
    local SERVICE="$1"
    if [ -n "$SERVICE" ]; then
        compose_cmd logs -f "$SERVICE"
    else
        compose_cmd logs -f
    fi
}

cmd_restart() {
    load_env
    local SERVICE="$1"
    if [ -n "$SERVICE" ]; then
        log "Reiniciando $SERVICE..."
        compose_cmd restart "$SERVICE"
    else
        log "Reiniciando todos os containers..."
        compose_cmd restart
    fi
    compose_cmd ps
}

cmd_nginx() {
    load_env
    generate_nginx_conf
    log "Recarregando nginx..."
    docker exec sgc-nginx nginx -s reload 2>/dev/null || warn "Nginx não está rodando. Suba os containers primeiro."
}

# -----------------------------------------------
# Main
# -----------------------------------------------

if [ $# -eq 0 ]; then
    show_help
    exit 0
fi

case "$1" in
    --up)       shift; cmd_up "$@" ;;
    --down)     shift; cmd_down "$@" ;;
    --build)    shift; cmd_build "$@" ;;
    --update)   shift; cmd_update "$@" ;;
    --backup)   cmd_backup "$2" ;;
    --restore)  cmd_restore "$2" ;;
    --status)   cmd_status ;;
    --logs)     cmd_logs "$2" ;;
    --restart)  cmd_restart "$2" ;;
    --nginx)    cmd_nginx ;;
    --help)     show_help ;;
    *)
        err "Comando desconhecido: $1"
        show_help
        exit 1
        ;;
esac
