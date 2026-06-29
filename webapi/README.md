# SGC API - Backend

API REST desenvolvida com ASP.NET Core 8.0.4 seguindo Clean Architecture.

## Stack

- **Framework**: ASP.NET Core 8.0.4 (Web API)
- **Linguagem**: C# 12
- **ORM**: Entity Framework Core
- **Banco**: MySQL
- **Autenticação**: JWT Bearer Token

## Arquitetura (Clean Architecture)

```
webapi/
├── Controllers/      # Endpoints REST
├── Models/
│   ├── Entities/     # Entidades de domínio
│   ├── DTOs/         # Data Transfer Objects (input)
│   └── ViewModels/  # Retorno para frontend
├── Services/         # Regras de negócio
├── Database/
│   ├── Context/      # DbContext EF Core
│   └── Mappings/     # Configurações de entidades
├── Extensions/       # Métodos de extensão
├── Settings/         # JWT, CORS
└── Utils/            # Helpers
```

## Regras de Desenvolvimento

### Input/Output
- DTOs para entrada, ViewModels para saída
- Entidades do Domain **nunca** expostas diretamente

### Listagens
- Paginação obrigatória: `pageNumber`, `pageSize`
- Retorno: `PagedResult<T>`

### Soft Delete
- Filtro `where deleted_at IS NULL` em todas as consultas
- Registros deletados são considerados inexistentes

### Performance
- `.AsNoTracking()` em consultas read-only
- Async/await em todo I/O

### Identificadores
- GUID/UUID para todas as chaves expostas na API

## Setup

### 1. Ambientes de Configuração

O projeto suporta múltiplos ambientes através de arquivos `appsettings`:

| Arquivo | Ambiente | Uso |
|--------|----------|-----|
| `appsettings.json` | Padrão | Configuração base |
| `appsettings.Development.json` | Development | Desenvolvimento local |
| `appsettings.Local.json` | Local | Configurações sensíveis (não commitado) |

O ambiente é controlado pela variável `ASPNETCORE_ENVIRONMENT` no `launchSettings.json`.

### 2. Configuração Local (appsettings.Local.json)

Crie o arquivo `appsettings.Local.json` na raiz do projeto com as configurações sensíveis:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=sistema_academico;User=root;Password=root;"
  },
  "JwtSettings": {
    "Issuer": "sgc-api",
    "Audience": "sgc_client",
    "SecretKey": "SUA_CHAVE_SECRETA_AQUI"
  },
  "SmtpSettings": {
    "SendGridApiKey": "SG.XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
    "FromEmail": "seu-email@dominio.com",
    "FromName": "Nome do Remetente"
  },
  "FrontendUrl": "http://localhost:3000"
}
```

**Nota**: O arquivo `appsettings.Local.json` já está no `.gitignore` para evitar commit de dados sensíveis.

### Configuração do SendGrid

1. Crie uma conta em [SendGrid](https://app.sendgrid.com/)
2. Gere uma API Key em Settings → API Keys
3. Configure o remetente em Settings → Sender Authentication → Verify a Single Sender
4. Atualize o `appsettings.Local.json` com:
   - `SendGridApiKey`: Sua API key
   - `FromEmail`: Email verificado no SendGrid
   - `FromName`: Nome que aparecerá no email

### 3. Aplicar Migrations
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 4. Executar
```bash
dotnet run
```

O ambiente padrão ao executar `dotnet run` é definido no `launchSettings.json` (atualmente configurado como `Local`).

API disponível em: `http://localhost:5070`

## Endpoints Principais

| Recurso | Endpoints |
|---------|-----------|
| Autenticação | `/api/auth/{login,register,logout,profile}` |
| Usuários | `/api/usuarios` |
| Cursos | `/api/cursos` |
| Turmas | `/api/turmas` |
| Aulas | `/api/aulas` |
| Avaliações | `/api/avaliacoes` |
| Matrículas | `/api/matriculas` |

## Checklist de Validação

- [ ] Filtro de deleção aplicado
- [ ] DTO para entrada, ViewModel para saída
- [ ] Paginação em listagens
- [ ] Logs em blocos catch
- [ ] Anotações Swagger nos endpoints

## Docker
Para criar o ambiente do mysql em container do docker

``` bash
docker run -d \
  --name mysql-sistema-academico \
  -e MYSQL_ROOT_PASSWORD=root \
  -e MYSQL_DATABASE=sistema_academico \
  -p 3306:3306 \
  mysql:8.0
```