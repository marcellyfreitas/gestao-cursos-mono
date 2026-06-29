# SGC - Sistema de Gestão de Cursos

## Requisitos do Sistema

---

## 1. Requisitos de Autenticação e Autorização

### 1.1 Autenticação

| ID | Descrição | Prioridade |
|----|-----------|------------|
| AUT-001 | O sistema deve permitir registro de novos usuários com nome, e-mail, CPF e senha | Alta |
| AUT-002 | A senha deve ser armazenada utilizando hash seguro (BCrypt) | Alta |
| AUT-003 | O sistema deve autenticar usuários via e-mail e senha, retornando um token JWT | Alta |
| AUT-004 | O token JWT deve conter as claims: id do usuário, nome, e-mail e papel (role) | Alta |
| AUT-005 | O token JWT deve ter expiração configurável (padrão: 8 horas) | Alta |
| AUT-006 | O sistema deve validar o e-mail do usuário através de link enviado por e-mail via SendGrid | Alta |
| AUT-007 | O sistema deve permitir recuperação de senha via e-mail com token de redefinição | Média |
| AUT-008 | O e-mail de recuperação deve conter um link com token de uso único e expiração | Média |
| AUT-009 | O sistema deve permitir logout invalidando o token do lado do servidor | Média |
| AUT-010 | Usuários recém-registrados devem ser criados com papel ALUNO e status pendente | Alta |
| AUT-011 | A conta do usuário deve ser ativada manualmente por um administrador | Alta |

### 1.2 Autorização

| ID | Descrição | Prioridade |
|----|-----------|------------|
| AUT-012 | O sistema deve ter dois papéis (roles): ADMIN e ALUNO | Alta |
| AUT-013 | Rotas administrativas devem ser protegidas por política ADMIN | Alta |
| AUT-014 | Rotas do aluno devem ser protegidas por política USER (ALUNO ou ADMIN) | Alta |
| AUT-015 | O middleware do frontend deve redirecionar usuários não autenticados para a página de login | Alta |
| AUT-016 | O frontend deve redirecionar usuários ALUNO para a área do aluno e ADMIN para o dashboard | Alta |
| AUT-017 | Cada requisição autenticada deve validar o token JWT no header `Authorization: Bearer <token>` | Alta |

### 1.3 Segurança

| ID | Descrição | Prioridade |
|----|-----------|------------|
| AUT-018 | A senha deve ter no mínimo 6 caracteres | Alta |
| AUT-019 | O CPF deve ser validado quanto ao formato e dígitos verificadores | Alta |
| AUT-020 | O sistema deve limitar taxa de requisições de login (rate limiting) | Média |
| AUT-021 | Todas as senhas em texto plano devem ser efêmeras e nunca registradas em log | Alta |

---

## 2. Requisitos por Módulo

### 2.1 Módulo de Cursos (Curso)

| ID | Descrição | Prioridade |
|----|-----------|------------|
| CUR-001 | O sistema deve permitir criar cursos informando nome, descrição, carga horária e um indicador de ativo | Alta |
| CUR-002 | O sistema deve permitir editar os dados de um curso existente | Alta |
| CUR-003 | O sistema deve permitir excluir logicamente (soft delete) um curso | Alta |
| CUR-004 | O sistema deve listar cursos com paginação (pageNumber, pageSize) | Alta |
| CUR-005 | O sistema deve permitir consultar um curso por ID | Alta |
| CUR-006 | Cursos podem ter 0 ou N pré-requisitos de outros cursos | Média |
| CUR-007 | Ao criar um curso, deve ser possível informar seus pré-requisitos | Média |
| CUR-008 | Ao excluir um curso, seus pré-requisitos devem ser removidos em cascata | Média |
| CUR-009 | A listagem de cursos deve retornar também os pré-requisitos de cada curso | Média |
| CUR-010 | Nome do curso deve ser único no sistema | Alta |

### 2.2 Módulo de Turmas (Turma)

| ID | Descrição | Prioridade |
|----|-----------|------------|
| TUR-001 | O sistema deve permitir criar turmas vinculadas a um curso existente | Alta |
| TUR-002 | Uma turma deve ter: código, curso, período (data início/fim), nota mínia, máximo de faltas, turno, vagas disponíveis e professor responsável | Alta |
| TUR-003 | O sistema deve permitir editar os dados de uma turma existente | Alta |
| TUR-004 | O sistema deve permitir excluir logicamente uma turma | Alta |
| TUR-005 | O sistema deve listar turmas com paginação, incluindo dados do curso e professor vinculados | Alta |
| TUR-006 | O sistema deve permitir consultar uma turma por ID | Alta |
| TUR-007 | Uma turma deve pertencer exatamente a um curso | Alta |
| TUR-008 | Professor responsável é opcional no cadastro da turma | Média |

### 2.3 Módulo de Aulas (Aula)

| ID | Descrição | Prioridade |
|----|-----------|------------|
| AUL-001 | O sistema deve permitir criar aulas vinculadas a uma turma existente | Alta |
| AUL-002 | Uma aula deve ter: turma, data, horário início, horário fim, conteúdo e professor responsável | Alta |
| AUL-003 | O sistema deve permitir editar os dados de uma aula existente | Alta |
| AUL-004 | O sistema deve permitir excluir logicamente uma aula | Alta |
| AUL-005 | O sistema deve listar aulas de uma turma específica com paginação | Alta |
| AUL-006 | O sistema deve permitir consultar uma aula por ID | Alta |
| AUL-007 | Uma aula deve pertencer exatamente a uma turma | Alta |

### 2.4 Módulo de Professores (Professor)

| ID | Descrição | Prioridade |
|----|-----------|------------|
| PRO-001 | O sistema deve permitir cadastrar professores vinculados a um usuário existente | Alta |
| PRO-002 | Um professor deve ter: usuário vinculado, formação, especialidade e currículo resumido | Alta |
| PRO-003 | O sistema deve permitir editar os dados de um professor existente | Alta |
| PRO-004 | O sistema deve permitir excluir logicamente um professor | Alta |
| PRO-005 | O sistema deve listar professores com paginação | Alta |
| PRO-006 | O sistema deve permitir consultar um professor por ID | Alta |
| PRO-007 | Deve haver no máximo um registro de professor por usuário | Alta |

### 2.5 Módulo de Usuários (Usuario)

| ID | Descrição | Prioridade |
|----|-----------|------------|
| USR-001 | O sistema deve permitir que administradores listem todos os usuários com paginação | Alta |
| USR-002 | O sistema deve permitir consultar um usuário por ID | Alta |
| USR-003 | O sistema deve permitir que administradores editem dados de qualquer usuário | Alta |
| USR-004 | O sistema deve permitir exclusão lógica de usuários | Alta |
| USR-005 | O sistema deve permitir que administradores ativem/desativem contas de usuários | Alta |
| USR-006 | O sistema deve permitir que o próprio usuário edite seus dados (minha conta) | Alta |
| USR-007 | E-mail deve ser único no sistema | Alta |
| USR-008 | CPF deve ser único no sistema | Alta |

### 2.6 Módulo de Matrículas (Matricula)

| ID | Descrição | Prioridade |
|----|-----------|------------|
| MAT-001 | O sistema deve permitir matricular um aluno em uma turma | Alta |
| MAT-002 | Uma matrícula deve ter: aluno (usuário), turma e situação (CURSANDO, APROVADO, REPROVADO_NOTA, REPROVADO_FREQUENCIA) | Alta |
| MAT-003 | Não deve ser permitido matricular o mesmo aluno mais de uma vez na mesma turma | Alta |
| MAT-004 | O sistema deve permitir editar a situação de uma matrícula | Alta |
| MAT-005 | O sistema deve permitir excluir logicamente uma matrícula | Alta |
| MAT-006 | O sistema deve listar matrículas com paginação, incluindo dados do aluno e da turma | Alta |
| MAT-007 | O sistema deve permitir consultar uma matrícula por ID | Alta |
| MAT-008 | O sistema deve recalcular automaticamente a situação do aluno (APROVADO/REPROVADO) com base na nota mínima e máximo de faltas da turma | Média |

### 2.7 Módulo de Avaliações (Avaliacao)

| ID | Descrição | Prioridade |
|----|-----------|------------|
| AVA-001 | O sistema deve permitir criar avaliações vinculadas a uma turma | Alta |
| AVA-002 | Uma avaliação deve ter: turma, nome, descrição, data, peso e valor máximo de nota | Alta |
| AVA-003 | O sistema deve permitir editar os dados de uma avaliação existente | Alta |
| AVA-004 | O sistema deve permitir excluir logicamente uma avaliação | Alta |
| AVA-005 | O sistema deve listar avaliações de uma turma com paginação | Alta |
| AVA-006 | O sistema deve permitir consultar uma avaliação por ID | Alta |
| AVA-007 | Uma avaliação deve pertencer exatamente a uma turma | Alta |
| AVA-008 | O peso da avaliação deve ser um valor positivo | Alta |

### 2.8 Módulo de Notas (Nota)

| ID | Descrição | Prioridade |
|----|-----------|------------|
| NOT-001 | O sistema deve permitir lançar nota de um aluno em uma avaliação específica | Alta |
| NOT-002 | Uma nota deve ter: matrícula, avaliação, valor da nota (0 a valor máximo da avaliação) | Alta |
| NOT-003 | Não deve ser permitido lançar duas notas para o mesmo aluno na mesma avaliação | Alta |
| NOT-004 | O sistema deve permitir editar o valor de uma nota existente | Alta |
| NOT-005 | O sistema deve permitir excluir logicamente uma nota | Alta |
| NOT-006 | O sistema deve listar notas por avaliação ou por matrícula com paginação | Alta |
| NOT-007 | O sistema deve permitir consultar uma nota por ID | Alta |
| NOT-008 | A nota lançada não pode exceder o valor máximo definido na avaliação | Alta |

### 2.9 Módulo de Frequências (Frequencia)

| ID | Descrição | Prioridade |
|----|-----------|------------|
| FRE-001 | O sistema deve permitir registrar frequência de um aluno em uma aula | Alta |
| FRE-002 | Uma frequência deve ter: matrícula, aula e status (PRESENTE, FALTA, FALTA_JUSTIFICADA) | Alta |
| FRE-003 | Não deve ser permitido registrar duas frequências para o mesmo aluno na mesma aula | Alta |
| FRE-004 | O sistema deve permitir editar o status de uma frequência existente | Alta |
| FRE-005 | O sistema deve permitir excluir logicamente uma frequência | Alta |
| FRE-006 | O sistema deve listar frequências por aula ou por matrícula com paginação | Alta |
| FRE-007 | O sistema deve permitir consultar uma frequência por ID | Alta |

### 2.10 Módulo do Aluno (Área do Aluno)

| ID | Descrição | Prioridade |
|----|-----------|------------|
| ALU-001 | O aluno deve ter acesso a uma página de resumo acadêmico com suas turmas, notas e frequências | Alta |
| ALU-002 | O resumo acadêmico deve exibir os dados consolidados do aluno (turmas em que está matriculado, notas por avaliação, frequência por aula) | Alta |
| ALU-003 | O aluno deve poder visualizar os detalhes de cada turma em que está matriculado | Alta |
| ALU-004 | O aluno deve poder editar seus próprios dados de perfil (nome, e-mail) | Alta |
| ALU-005 | O aluno não deve ter acesso a nenhuma rota administrativa | Alta |

### 2.11 Módulo de Dashboard (Painel Administrativo)

| ID | Descrição | Prioridade |
|----|-----------|------------|
| DSH-001 | O dashboard deve exibir cards com totais (cursos, turmas, alunos, professores) | Alta |
| DSH-002 | O dashboard deve exibir gráficos de distribuição de alunos por turma | Média |
| DSH-003 | O dashboard deve exibir tabelas com dados recentes | Média |
| DSH-004 | O dashboard deve ter navegação lateral com links para todos os módulos de gestão | Alta |
| DSH-005 | O dashboard deve permitir busca global | Baixa |

---

## 3. Requisitos de API (Rotas)

### 3.1 Convenções Gerais

| ID | Descrição |
|----|-----------|
| API-001 | Todas as rotas devem ser prefixadas com `/api/v1/` |
| API-002 | Todas as respostas devem seguir o formato JSON |
| API-003 | Rotas de listagem devem suportar paginação com parâmetros `pageNumber` (>= 1) e `pageSize` (>= 1, <= 100) |
| API-004 | Respostas paginadas devem incluir metadados: `items`, `totalItems`, `totalPages`, `pageNumber`, `pageSize` |
| API-005 | Rotas de criação devem retornar HTTP 201 (Created) com o recurso criado |
| API-006 | Rotas de consulta por ID devem retornar HTTP 200 com o recurso |
| API-007 | Rotas de atualização devem retornar HTTP 200 com o recurso atualizado |
| API-008 | Rotas de exclusão devem retornar HTTP 204 (No Content) |
| API-009 | Recursos não encontrados devem retornar HTTP 404 com mensagem descritiva |
| API-010 | Erros de validação devem retornar HTTP 400 com detalhes dos campos inválidos |
| API-011 | Erros de autenticação devem retornar HTTP 401 |
| API-012 | Erros de autorização devem retornar HTTP 403 |
| API-013 | Conflitos (ex: registro duplicado) devem retornar HTTP 409 |
| API-014 | Todas as listagens devem filtrar automaticamente registros com soft delete |

### 3.2 Autenticação

| Método | Rota | Descrição | Autenticação |
|--------|------|-----------|--------------|
| POST | `/api/v1/auth/login` | Autenticar usuário e retornar token JWT | Não |
| POST | `/api/v1/auth/register` | Registrar novo usuário | Não |
| POST | `/api/v1/auth/logout` | Invalidar token do usuário | Sim |
| GET | `/api/v1/auth/profile` | Retornar perfil do usuário autenticado | Sim |

### 3.3 Cursos

| Método | Rota | Descrição | Autenticação |
|--------|------|-----------|--------------|
| GET | `/api/v1/cursos` | Listar cursos (paginado) | ADMIN |
| POST | `/api/v1/cursos` | Criar curso | ADMIN |
| GET | `/api/v1/cursos/{id}` | Consultar curso por ID | ADMIN |
| PUT | `/api/v1/cursos/{id}` | Atualizar curso | ADMIN |
| DELETE | `/api/v1/cursos/{id}` | Excluir logicamente curso | ADMIN |

### 3.4 Turmas

| Método | Rota | Descrição | Autenticação |
|--------|------|-----------|--------------|
| GET | `/api/v1/turmas` | Listar turmas (paginado) | ADMIN |
| POST | `/api/v1/turmas` | Criar turma | ADMIN |
| GET | `/api/v1/turmas/{id}` | Consultar turma por ID | ADMIN |
| PUT | `/api/v1/turmas/{id}` | Atualizar turma | ADMIN |
| DELETE | `/api/v1/turmas/{id}` | Excluir logicamente turma | ADMIN |

### 3.5 Aulas

| Método | Rota | Descrição | Autenticação |
|--------|------|-----------|--------------|
| GET | `/api/v1/aulas` | Listar aulas (paginado, opcionalmente por turma) | ADMIN |
| POST | `/api/v1/aulas` | Criar aula | ADMIN |
| GET | `/api/v1/aulas/{id}` | Consultar aula por ID | ADMIN |
| PUT | `/api/v1/aulas/{id}` | Atualizar aula | ADMIN |
| DELETE | `/api/v1/aulas/{id}` | Excluir logicamente aula | ADMIN |

### 3.6 Professores

| Método | Rota | Descrição | Autenticação |
|--------|------|-----------|--------------|
| GET | `/api/v1/professores` | Listar professores (paginado) | ADMIN |
| POST | `/api/v1/professores` | Criar professor | ADMIN |
| GET | `/api/v1/professores/{id}` | Consultar professor por ID | ADMIN |
| PUT | `/api/v1/professores/{id}` | Atualizar professor | ADMIN |
| DELETE | `/api/v1/professores/{id}` | Excluir logicamente professor | ADMIN |

### 3.7 Usuários

| Método | Rota | Descrição | Autenticação |
|--------|------|-----------|--------------|
| GET | `/api/v1/usuarios` | Listar usuários (paginado) | ADMIN |
| POST | `/api/v1/usuarios` | Criar usuário | ADMIN |
| GET | `/api/v1/usuarios/{id}` | Consultar usuário por ID | ADMIN |
| PUT | `/api/v1/usuarios/{id}` | Atualizar usuário | ADMIN |
| DELETE | `/api/v1/usuarios/{id}` | Excluir logicamente usuário | ADMIN |

### 3.8 Matrículas

| Método | Rota | Descrição | Autenticação |
|--------|------|-----------|--------------|
| GET | `/api/v1/matriculas` | Listar matrículas (paginado) | ADMIN |
| POST | `/api/v1/matriculas` | Criar matrícula | ADMIN |
| GET | `/api/v1/matriculas/{id}` | Consultar matrícula por ID | ADMIN |
| PUT | `/api/v1/matriculas/{id}` | Atualizar matrícula | ADMIN |
| DELETE | `/api/v1/matriculas/{id}` | Excluir logicamente matrícula | ADMIN |

### 3.9 Avaliações

| Método | Rota | Descrição | Autenticação |
|--------|------|-----------|--------------|
| GET | `/api/v1/avaliacoes` | Listar avaliações (paginado, opcionalmente por turma) | ADMIN |
| POST | `/api/v1/avaliacoes` | Criar avaliação | ADMIN |
| GET | `/api/v1/avaliacoes/{id}` | Consultar avaliação por ID | ADMIN |
| PUT | `/api/v1/avaliacoes/{id}` | Atualizar avaliação | ADMIN |
| DELETE | `/api/v1/avaliacoes/{id}` | Excluir logicamente avaliação | ADMIN |

### 3.10 Notas

| Método | Rota | Descrição | Autenticação |
|--------|------|-----------|--------------|
| GET | `/api/v1/notas` | Listar notas (paginado, por avaliação ou matrícula) | ADMIN |
| POST | `/api/v1/notas` | Lançar nota | ADMIN |
| GET | `/api/v1/notas/{id}` | Consultar nota por ID | ADMIN |
| PUT | `/api/v1/notas/{id}` | Atualizar nota | ADMIN |
| DELETE | `/api/v1/notas/{id}` | Excluir logicamente nota | ADMIN |

### 3.11 Frequências

| Método | Rota | Descrição | Autenticação |
|--------|------|-----------|--------------|
| GET | `/api/v1/frequencias` | Listar frequências (paginado, por aula ou matrícula) | ADMIN |
| POST | `/api/v1/frequencias` | Registrar frequência | ADMIN |
| GET | `/api/v1/frequencias/{id}` | Consultar frequência por ID | ADMIN |
| PUT | `/api/v1/frequencias/{id}` | Atualizar frequência | ADMIN |
| DELETE | `/api/v1/frequencias/{id}` | Excluir logicamente frequência | ADMIN |

### 3.12 Área do Aluno

| Método | Rota | Descrição | Autenticação |
|--------|------|-----------|--------------|
| GET | `/api/v1/aluno/resumo` | Obter resumo acadêmico do aluno autenticado | USER |
| GET | `/api/v1/aluno/turmas/{id}` | Obter detalhes de uma turma do aluno autenticado | USER |

---

## 4. Requisitos de Testes

### 4.1 Testes Unitários (Backend - xUnit)

| ID | Descrição | Prioridade |
|----|-----------|------------|
| TES-001 | Todos os serviços devem ter testes unitários cobrindo cenários de sucesso e erro | Alta |
| TES-002 | Testes devem usar Moq para isolar dependências (DbContext, serviços externos) | Alta |
| TES-003 | Testes de banco de dados devem usar EF Core InMemory database | Alta |
| TES-004 | Cada método de teste deve seguir o padrão AAA (Arrange, Act, Assert) | Alta |
| TES-005 | Testes de autenticação devem cobrir: login válido, login inválido, registro, duplicidade de e-mail | Alta |
| TES-006 | Testes de serviços devem cobrir: criação, consulta, atualização, exclusão e validações | Alta |
| TES-007 | Testes de controle de concorrência (unique constraints) devem ser implementados | Média |
| TES-008 | Testes de preservação de propriedades (imutabilidade de CreatedAt, atualização de UpdatedAt) | Média |
| TES-009 | Cobertura de código mínima de 70% nas camadas de serviço | Média |
| TES-010 | Testes de controllers devem validar retornos HTTP corretos (201, 200, 204, 404, 400) | Alta |

### 4.2 Testes E2E (Frontend - Playwright)

| ID | Descrição | Prioridade |
|----|-----------|------------|
| TES-011 | Deve haver um setup global que autentica como administrador antes dos testes E2E | Alta |
| TES-012 | Testes E2E devem utilizar Page Object Model para organização | Alta |
| TES-013 | Testes devem validar o fluxo completo de CRUD de turmas | Alta |
| TES-014 | Testes devem validar regras de validação de formulário no frontend | Alta |
| TES-015 | Testes devem capturar screenshot em caso de falha | Alta |
| TES-016 | Testes E2E devem ser executados contra ambiente de desenvolvimento | Média |

### 4.3 Estratégia de Testes

| ID | Descrição |
|----|-----------|
| TES-017 | Testes unitários devem ser executados em pipeline de CI a cada push |
| TES-018 | Testes E2E devem ser executados antes de deployment em produção |
| TES-019 | Relatórios de cobertura devem ser gerados utilizando Coverlet |

---

## 5. Requisitos Não Funcionais

### 5.1 Desempenho

| ID | Descrição | Prioridade |
|----|-----------|------------|
| NF-001 | O tempo de resposta da API para consultas paginadas não deve exceder 2 segundos sob carga normal | Alta |
| NF-002 | A API deve suportar pelo menos 50 requisições simultâneas sem degradação significativa | Média |
| NF-003 | Consultas ao banco de dados devem utilizar índices apropriados nas colunas de busca e junção | Alta |
| NF-004 | Aplicação deve suportar paginação server-side em todas as listagens para evitar sobrecarga | Alta |

### 5.2 Segurança

| ID | Descrição | Prioridade |
|----|-----------|------------|
| NF-005 | Todas as comunicações entre frontend e backend devem ser criptografadas via HTTPS em produção | Alta |
| NF-006 | Tokens JWT devem ser armazenados de forma segura no frontend (httpOnly cookies ou localStorage com proteção XSS) | Alta |
| NF-007 | Senhas devem ser armazenadas utilizando BCrypt com fator de custo >= 10 | Alta |
| NF-008 | O sistema deve sanitizar entradas do usuário para prevenir injeção SQL e XSS | Alta |
| NF-009 | Rotas administrativas não devem ser acessíveis a usuários com papel ALUNO | Alta |
| NF-010 | O sistema deve implementar proteção CORS adequada | Alta |

### 5.3 Disponibilidade e Confiabilidade

| ID | Descrição | Prioridade |
|----|-----------|------------|
| NF-011 | O sistema deve utilizar soft delete em todas as entidades para prevenção de perda acidental de dados | Alta |
| NF-012 | O banco de dados deve ter backup automatizado (diário) | Média |
| NF-013 | O sistema deve registrar logs de erros e operações críticas | Alta |
| NF-014 | Em caso de falha no envio de e-mail, o sistema não deve interromper o fluxo principal | Média |
| NF-015 | A aplicação deve ser executada em contêineres Docker com restart automático em caso de falha | Alta |

### 5.4 Manutenibilidade

| ID | Descrição | Prioridade |
|----|-----------|------------|
| NF-016 | O backend deve seguir o padrão Clean Architecture com separação clara entre Controllers, Services e Models | Alta |
| NF-017 | O frontend deve utilizar componentes tipados com TypeScript | Alta |
| NF-018 | O frontend deve utilizar um design system baseado em shadcn/ui com componentes reutilizáveis | Alta |
| NF-019 | A API deve ser documentada com Swagger/OpenAPI disponível em ambiente de desenvolvimento | Alta |
| NF-020 | Variáveis de configuração (conexão de banco, JWT secrets, chave de e-mail) devem ser externalizadas em variáveis de ambiente | Alta |

### 5.5 Escalabilidade

| ID | Descrição | Prioridade |
|----|-----------|------------|
| NF-021 | A API deve ser stateless para permitir escalabilidade horizontal | Média |
| NF-022 | O frontend Next.js deve ser construído como imagem Docker otimizada (multi-stage build) para deploy escalável | Média |
| NF-023 | O proxy reverso Nginx deve servir arquivos estáticos e fazer balanceamento de carga quando necessário | Média |

### 5.6 Compatibilidade

| ID | Descrição | Prioridade |
|----|-----------|------------|
| NF-024 | O frontend deve ser compatível com navegadores modernos (Chrome, Firefox, Edge, Safari - últimas 2 versões) | Alta |
| NF-025 | A interface deve ser responsiva (desktop e tablet) | Média |
| NF-026 | O banco de dados deve ser MySQL 8.0 ou superior | Alta |
| NF-027 | O backend deve ser executado em .NET 8.0 ou superior | Alta |
| NF-028 | O frontend deve ser executado em Node.js 20 LTS ou superior | Alta |

### 5.7 Infraestrutura

| ID | Descrição | Prioridade |
|----|-----------|------------|
| NF-029 | A aplicação deve ser orquestrada via Docker Compose com perfis local e produção | Alta |
| NF-030 | O ambiente de produção deve configurar SSL via Let's Encrypt com renovação automática | Média |
| NF-031 | O script de gerenciamento (`sgc.sh`) deve suportar operações de start, stop, restart, backup, restore e logs | Média |
| NF-032 | Limites de recursos (CPU/memória) devem ser configurados para cada contêiner em produção | Média |

### 5.8 Experiência do Usuário

| ID | Descrição | Prioridade |
|----|-----------|------------|
| NF-033 | O sistema deve oferecer feedback visual imediato para ações do usuário (loading, sucesso, erro) | Alta |
| NF-034 | Formulários devem exibir validação inline com mensagens claras | Alta |
| NF-035 | O sistema deve usar temas de cores consistentes (suporte a temas claro/escuro) | Baixa |
| NF-036 | Mensagens de erro devem ser amigáveis e não expor detalhes técnicos | Alta |
| NF-037 | Todas as listagens devem exibir contagem total de registros | Média |

---

## 6. Regras de Negócio

| ID | Descrição | Prioridade |
|----|-----------|------------|
| RN-001 | Um aluno só pode ser matriculado uma única vez na mesma turma (unique constraint entre aluno e turma) | Alta |
| RN-002 | A situação do aluno é calculada com base na nota final (média ponderada) vs nota mínima da turma e total de faltas vs máximo de faltas da turma | Média |
| RN-003 | A nota final do aluno é calculada pela média ponderada de todas as avaliações da turma | Média |
| RN-004 | O total de faltas é calculado pela soma de frequências com status FALTA e FALTA_JUSTIFICADA | Média |
| RN-005 | Um curso pode ter múltiplos pré-requisitos, formando uma estrutura de dependências | Média |
| RN-006 | A exclusão de um curso, turma, aula ou avaliação deve ser lógica (soft delete), preservando o registro no banco com timestamp de exclusão | Alta |
| RN-007 | O sistema deve registrar timestamps de criação (`CreatedAt`) e atualização (`UpdatedAt`) em todas as entidades | Alta |
| RN-008 | Usuários com papel ALUNO não podem acessar rotas administrativas | Alta |
| RN-009 | Apenas administradores podem criar, editar ou excluir registros nos módulos de gestão | Alta |
| RN-010 | Alunos só podem visualizar seus próprios dados (matrículas, notas, frequências) | Alta |

---

## 7. Entidades e Relacionamentos

| Entidade | Relacionamentos |
|----------|-----------------|
| Usuario | 1:N com Matricula (como aluno), 1:1 com Professor, 1:N com Nota (via Matricula), 1:N com Frequencia (via Matricula) |
| Curso | 1:N com Turma, N:M com Curso (auto-referência para pré-requisitos via CursoPrerequisito) |
| Turma | N:1 com Curso, N:1 com Professor, 1:N com Matricula, 1:N com Aula, 1:N com Avaliacao |
| Professor | 1:1 com Usuario, 1:N com Turma (como responsável), 1:N com Aula (como responsável) |
| Aula | N:1 com Turma, N:1 com Professor, 1:N com Frequencia |
| Matricula | N:1 com Usuario (aluno), N:1 com Turma, 1:N com Nota, 1:N com Frequencia |
| Avaliacao | N:1 com Turma, 1:N com Nota |
| Nota | N:1 com Matricula, N:1 com Avaliacao |
| Frequencia | N:1 com Matricula, N:1 com Aula |
| CursoPrerequisito | N:1 com Curso (curso atual), N:1 com Curso (pré-requisito) |

---

## 8. Glossário

| Termo | Definição |
|-------|-----------|
| Turma | Uma oferta específica de um curso em um período, com professor, vagas, nota mínima e máximo de faltas |
| Matrícula | Vínculo de um aluno a uma turma, com situação acadêmica |
| Avaliação | Instrumento de avaliação de uma turma (prova, trabalho, etc.) com peso e valor máximo |
| Nota | Valor atribuído a um aluno em uma avaliação específica |
| Frequência | Registro de presença de um aluno em uma aula |
| Soft Delete | Exclusão lógica onde o registro permanece no banco com um timestamp de exclusão |
| JWT | JSON Web Token utilizado para autenticação stateless |
| ALUNO | Papel de usuário com acesso restrito à sua própria área acadêmica |
| ADMIN | Papel de usuário com acesso completo a todas as funcionalidades do sistema |
