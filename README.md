# Sistema de Gestão de Cursos (SGC)

Sistema SaaS para gerenciamento de cursos online com autenticação JWT, gestão de turmas, aulas, avaliações e controle de desempenho acadêmico.

## Visão Geral

| Componente | Stack |
|------------|-------|
| **Frontend** | Next.js 15, TypeScript, Tailwind CSS |
| **Backend** | ASP.NET Core 8.0.4, EF Core, MySQL |
| **Autenticação** | JWT Bearer Token |

## Estrutura do Projeto

```
codigo-fonte/
├── webapi/           # API REST (.NET 8)
├── webapi.Tests/     # Testes unitários da API (.NET 8, xUnit)
├── webapp/           # Aplicação Web (Next.js)
└── README.md         # Este arquivo
```

## Perfis de Usuário

- **ADMIN**: Acesso completo ao sistema
- **ALUNO**: Consultar matrículas, notas e frequência

## Funcionalidades Principais

- Gestão de cursos com pré-requisitos
- Turmas, aulas e avaliações
- Matrículas e controle de frequência
- Cálculo automático de aprovação
- Soft delete em todas as entidades

## Documentação Detalhada

- [Frontend](/webapp/README.md) - Setup e detalhes do Next.js
- [Backend](/webapi/README.md) - Setup e detalhes da API .NET

## Quick Start

### Backend
```bash
cd webapi
dotnet restore
dotnet ef database update
dotnet run
```

### Testes (Backend)
```bash
# A partir da raiz do projeto (codigo-fonte/)
dotnet test webapi.Tests/webapi.Tests.csproj --no-build -v n

# Executar um teste específico pelo nome
dotnet test webapi.Tests/webapi.Tests.csproj --no-build -v n --filter "FullyQualifiedName~NomeDoTeste"
```

### Frontend
```bash
cd webapp
npm install
npm run dev
```

## License

MIT
