# Autenticação

Serviço responsável pela criação de conta, login e validação de sessão.

# Endpoints

- /Conta/Criar (POST)

Rota para criação de conta, recebe os dados do usuário

- /Conta/Login (POST)

Rota para login, recebe email, senha e opção de manter conectado, retorna o token

- /Conta (GET)

Rota para o usuário visualizar os dados de sua conta (necessário token "authorize" no header)

- /Conta/{id} (GET)

Rota para ser utilizada por outros serviços, retorna os dados da conta de um usuário pelo seu id

- /Auth (ANY)

Rota para ser utilizada no middleware, retorna o status 200 caso o token seja válido, caso contrário retorna o status 403