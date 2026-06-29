# SGC - Sistema de Gestão de Cursos

Sistema SaaS para gestão de cursos online, desenvolvido com ASP.NET Core 8.0 (Backend) e Next.js 15 (Frontend).

---

## Stack Tecnológica

| Camada | Tecnologia |
|--------|------------|
| **Backend** | ASP.NET Core 8.0.4, C# 12, Entity Framework Core |
| **Frontend** | Next.js 15, TypeScript, Tailwind CSS |
| **Banco de Dados** | MySQL |
| **Autenticação** | JWT Bearer Token |
| **ORM** | Entity Framework Core |

---

## Arquitetura

### Backend (Clean Architecture)

```
webapi/
├── Controllers/      # Endpoints da API
├── Models/           # Entidades, DTOs, ViewModels
├── Services/         # Regras de negócio
├── Database/         # Contexto EF Core, Migrations
├── Extensions/       # Métodos de extensão
├── Settings/         # Configurações (JWT, CORS)
├── Utils/            # Utilitários
└── Migrations/      # Migrations EF Core
```

### Frontend (Next.js App Router)

```
webapp/
├── app/              # Pages (App Router)
│   ├── api/          # API Routes
│   ├── auth/         # Autenticação
│   ├── dashboard/    # Painel Admin
│   └── aluno/        # Área do Aluno
├── components/      # Componentes reutilizáveis
├── contexts/         # React Context (Auth)
├── hooks/            # Custom Hooks
├── services/         # Chamadas à API
├── types/            # Tipos TypeScript
└── lib/              # Utilitários
```

---

## Perfis de Acesso

### ADMIN
- Acesso completo ao sistema
- CRUD em todas as entidades
- Dashboard administrativo

### ALUNO
- Atualizar próprios dados
- Consultar matrículas, notas e frequências
- Visualizar desempenho acadêmico

---

## Regras de Negócio

### Soft Delete
Entidades com `deleted_at`: usuario, professor, curso, turma, aula, avaliacao, matricula.

Registros deletados são ocultados de todas as consultas padrão.

### Auditoria
- `created_at`: preenchido automaticamente na criação
- `updated_at`: atualizado a cada modificação

### Cursos e Pré-requisitos
- `media_minima` pode ser NULL (curso não exige nota)
- `frequencia_minima` default: 75%
- Cursos podem ter 0..N pré-requisitos

### Estrutura Acadêmica
- **Turma**: pertence a um curso, possui nome
- **Aula**: 0..N por turma (título, número, data opcional)
- **Avaliação**: 0..N por turma (peso default 1.0, nota_maxima default 10)

### Matrícula
- Um usuário = uma matrícula por turma (UNIQUE constraint)
- Status: CURSANDO, APROVADO, REPROVADO_NOTA, REPROVADO_FREQUENCIA

### Regras de Aprovação
| Condição | Resultado |
|----------|-----------|
| Frequência ≥ mínimo | Verifica nota |
| Sem avaliações | Aprovado por frequência |
| Com avaliações | Média ponderada ≥ media_minima |

---

## Regras de Desenvolvimento (Backend)

### Input/Output
- **Entrada**: DTOs obrigatórios para create/update
- **Saída**: ViewModels (entidades do Domain jamais expostas)

### Listagens
- Paginação obrigatória com `pageNumber` e `pageSize`
- Retorno: `PagedResult<T>`
- Exceção: endpoints de lookup (combos/selects)

### Resiliência
- Try-catch com `ILogger<T>` em pontos de falha externa
- Result Pattern para fluxo de negócio
- Global Exception Middleware

### Persistência (EF Core)
- Soft Delete: `where deleted_at IS NULL`
- Read-only queries: `.AsNoTracking()`
- Async/await em todo I/O

---

## Getting Started

### Pré-requisitos
- Node.js 18+
- .NET 8 SDK
- MySQL 8.0+

### Backend

```bash
cd webapi
dotnet restore
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run --project api-sgc.csproj
```

### Frontend

```bash
cd webapp
npm install
npm run dev
```

### Variáveis de Ambiente

**Backend** (`webapi/appsettings.json`):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=sgc;User=root;Password=senha;"
  },
  "Jwt": {
    "Key": "sua-chave-secreta-mínimo-32-caracteres",
    "Issuer": "SGC-API",
    "Audience": "SGC-Frontend"
  }
}
```

**Frontend** (`webapp/.env`):
```
NEXT_PUBLIC_API_URL=http://localhost:5070/api
```

---

## API Endpoints

### Autenticação
- `POST /api/auth/register` - Cadastro
- `POST /api/auth/login` - Login
- `POST /api/auth/logout` - Logout
- `GET /api/auth/profile` - Perfil

### Recursos Protegidos
- `/api/usuarios` - CRUD de usuários (ADMIN)
- `/api/cursos` - CRUD de cursos
- `/api/turmas` - CRUD de turmas
- `/api/aulas` - CRUD de aulas
- `/api/avaliacoes` - CRUD de avaliações
- `/api/matriculas` - Matrículas

---

## Navegação

### Admin
- Dashboard
- Usuários
- Professores
- Cursos e Turmas
- Gestão Acadêmica

### Aluno
- Início
- Meus Cursos
- Notas e Frequência
- Configurações

---

## Identificadores

Uso obrigatório de **GUID/UUID** para todas as chaves primárias e estrangeiras expostas na API.

---

## License

MIT
