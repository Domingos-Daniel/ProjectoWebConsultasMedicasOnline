# Documentação da API de Pagamentos

Esta documentação descreve detalhadamente os endpoints disponíveis na API de Pagamentos, incluindo parâmetros necessários, formatos de requisição e resposta, e exemplos de uso.
url: BASE_URL

## Índice

1. [Autenticação](#autenticação)
2. [Endpoints de Pagamento](#endpoints-de-pagamento)
    - [Processar Pagamento](#processar-pagamento)
    - [Depósito de Fundos](#depósito-de-fundos)
3. [Endpoints de Usuário](#endpoints-de-usuário)
    - [Informações do Usuário](#informações-do-usuário)
    - [Consulta de Saldo](#consulta-de-saldo)
4. [Endpoints Administrativos](#endpoints-administrativos)
    - [Criar Usuário](#criar-usuário)
    - [Listar Usuários](#listar-usuários)
    - [Atualizar Saldos](#atualizar-saldos)
    - [Listar Transações](#listar-transações)
5. [Códigos de Resposta](#códigos-de-resposta)
6. [Estruturas de Dados](#estruturas-de-dados)
7. [Exemplos de Uso](#exemplos-de-uso)

## Autenticação

A API utiliza autenticação simples via campos `Username` e `Password` em cada requisição. Não é utilizado OAuth ou tokens JWT.

## Endpoints de Pagamento

### Processar Pagamento

Este endpoint processa um pagamento debitando o valor da conta do usuário.

**Endpoint:** `POST /api/payment/process`

**Cabeçalhos da Requisição:**
```
Content-Type: application/json
```

**Corpo da Requisição:**
```json
{
  "Username": "string", // Nome de usuário (obrigatório)
  "Password": "string", // Senha (obrigatório)
  "Amount": 0.0,        // Valor a debitar (obrigatório, número decimal)
  "Reference": "string" // Referência do pagamento (obrigatório)
}
```

**Resposta de Sucesso (200 OK):**
```json
{
  "Success": true,
  "Message": "Payment processed successfully",
  "NewBalance": 250000.0,
  "TransactionId": 123
}
```

**Resposta de Falha (200 OK com indicador de falha):**
```json
{
  "Success": false,
  "Message": "Insufficient balance", // ou "Invalid username or password"
  "NewBalance": 100000.0,
  "TransactionId": 124
}
```

**Resposta de Erro de Validação (400 Bad Request):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "traceId": "00-...",
  "errors": {
    "Username": [
      "The Username field is required."
    ]
  }
}
```

**Notas:**
- Uma transação será registrada mesmo em caso de falha (com status "Failed")
- O valor (`Amount`) deve ser positivo
- O saldo do usuário deve ser suficiente para realizar a operação

### Depósito de Fundos

Este endpoint processa um depósito creditando o valor na conta do usuário.

**Endpoint:** `POST /api/payment/deposit`

**Cabeçalhos da Requisição:**
```
Content-Type: application/json
```

**Corpo da Requisição:**
```json
{
  "Username": "string",  // Nome de usuário (obrigatório)
  "Password": "string",  // Senha (obrigatório)
  "Amount": 0.0,         // Valor a creditar (obrigatório, número decimal)
  "Reference": "string"  // Referência do depósito (opcional)
}
```

**Resposta de Sucesso (200 OK):**
```json
{
  "Success": true,
  "Message": "Depósito efetuado com sucesso",
  "NewBalance": 350000.0,
  "TransactionId": 125
}
```

**Resposta de Falha (200 OK com indicador de falha):**
```json
{
  "Success": false,
  "Message": "Usuário ou senha inválidos"
}
```

**Resposta de Erro de Validação (400 Bad Request):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "traceId": "00-...",
  "errors": {
    "Amount": [
      "The Amount field is required."
    ]
  }
}
```

**Notas:**
- O campo `Reference` é opcional. Se não fornecido, será usado "Depósito" como valor padrão
- O valor (`Amount`) deve ser positivo
- Uma transação com status "Completed" é criada automaticamente

## Endpoints de Usuário

### Informações do Usuário

Este endpoint retorna informações do usuário e histórico de transações.

**Endpoint:** `GET /api/user/{username}/{password}`

**Parâmetros de URL:**
- `username`: Nome de usuário
- `password`: Senha

**Resposta de Sucesso (200 OK):**
```json
{
  "Username": "user1",
  "Balance": 300000.0,
  "Transactions": [
    {
      "Id": 123,
      "Amount": 50000.0,
      "Reference": "Compra online",
      "Timestamp": "2025-06-15T14:30:45.123Z",
      "Status": "Completed"
    },
    {
      "Id": 122,
      "Amount": 20000.0,
      "Reference": "Depósito",
      "Timestamp": "2025-06-14T10:15:30.456Z",
      "Status": "Completed"
    }
  ]
}
```

**Resposta de Falha (404 Not Found):**
```json
"Invalid username or password"
```

### Consulta de Saldo

Este endpoint retorna apenas o saldo atual do usuário.

**Endpoint:** `GET /api/user/balance/{username}/{password}`

**Parâmetros de URL:**
- `username`: Nome de usuário
- `password`: Senha

**Resposta de Sucesso (200 OK):**
```json
{
  "Username": "user1",
  "Balance": 300000.0
}
```

**Resposta de Falha (404 Not Found):**
```json
"Usuário ou senha inválidos"
```

## Endpoints Administrativos

### Criar Usuário

Este endpoint cria um novo usuário com saldo inicial de 300.000 KZ.

**Endpoint:** `POST /api/admin/create-user`

**Cabeçalhos da Requisição:**
```
Content-Type: application/json
```

**Corpo da Requisição:**
```json
{
  "Username": "string", // Nome de usuário (obrigatório)
  "Password": "string"  // Senha (obrigatório)
}
```

**Resposta de Sucesso (200 OK):**
```json
{
  "Message": "Usuário criado com sucesso!",
  "User": {
    "Id": 3,
    "Username": "user3",
    "Balance": 300000.0
  }
}
```

### Listar Usuários

Este endpoint lista todos os usuários cadastrados.

**Endpoint:** `GET /api/admin/list-users`

**Resposta de Sucesso (200 OK):**
```json
[
  {
    "Id": 1,
    "Username": "user1",
    "Balance": 300000.0
  },
  {
    "Id": 2,
    "Username": "user2",
    "Balance": 300000.0
  },
  {
    "Id": 3,
    "Username": "user3",
    "Balance": 300000.0
  }
]
```

### Atualizar Saldos

Este endpoint atualiza o saldo de todos os usuários para 300.000 KZ.

**Endpoint:** `GET /api/admin/update-balances`

**Resposta de Sucesso (200 OK):**
```json
{
  "Message": "Saldos atualizados para 300.000 KZs com sucesso!",
  "Users": [
    {
      "Id": 1,
      "Username": "user1",
      "Balance": 300000.0
    },
    {
      "Id": 2,
      "Username": "user2",
      "Balance": 300000.0
    },
    {
      "Id": 3,
      "Username": "user3",
      "Balance": 300000.0
    }
  ]
}
```

### Listar Transações

Este endpoint lista todas as transações realizadas no sistema.

**Endpoint:** `GET /api/admin/transactions`

**Resposta de Sucesso (200 OK):**
```json
[
  {
    "Id": 125,
    "Amount": 50000.0,
    "Reference": "Depósito",
    "Timestamp": "2025-06-15T15:20:10.789Z",
    "Status": "Completed",
    "User": {
      "Id": 1,
      "Username": "user1"
    }
  },
  {
    "Id": 124,
    "Amount": 75000.0,
    "Reference": "Pagamento de serviço",
    "Timestamp": "2025-06-15T12:45:30.123Z",
    "Status": "Failed",
    "User": {
      "Id": 2,
      "Username": "user2"
    }
  }
]
```

## Códigos de Resposta

- **200 OK**: A requisição foi bem sucedida
- **400 Bad Request**: A requisição contém erros de validação ou formato inválido
- **404 Not Found**: O recurso solicitado não foi encontrado (ex: usuário não existe)

## Estruturas de Dados

### User
```json
{
  "Id": 1,
  "Username": "string",
  "Password": "string", // Não retornado nas consultas
  "Balance": 300000.0
}
```

### Transaction
```json
{
  "Id": 123,
  "UserId": 1,
  "Amount": 50000.0,
  "Reference": "string",
  "Timestamp": "2025-06-15T14:30:45.123Z",
  "Status": "Completed" // Valores possíveis: "Pending", "Completed", "Failed"
}
```

### PaymentRequest
```json
{
  "Username": "string",
  "Password": "string",
  "Amount": 0.0,
  "Reference": "string"
}
```

### DepositRequest
```json
{
  "Username": "string",
  "Password": "string",
  "Amount": 0.0,
  "Reference": "string" // Opcional
}
```

### PaymentResponse
```json
{
  "Success": true,
  "Message": "string",
  "NewBalance": 0.0,
  "TransactionId": 0
}
```

## Exemplos de Uso

### Exemplo 1: Processamento de um Pagamento

**Requisição:**
```http
POST /api/payment/process HTTP/1.1
Host: localhost:5000
Content-Type: application/json

{
  "Username": "user1",
  "Password": "pass1",
  "Amount": 50000.0,
  "Reference": "Pagamento de fatura"
}
```

**Resposta:**
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "Success": true,
  "Message": "Payment processed successfully",
  "NewBalance": 250000.0,
  "TransactionId": 126
}
```

### Exemplo 2: Depósito de Fundos

**Requisição:**
```http
POST /api/payment/deposit HTTP/1.1
Host: localhost:5000
Content-Type: application/json

{
  "Username": "user1",
  "Password": "pass1",
  "Amount": 100000.0,
  "Reference": "Depósito via transferência"
}
```

**Resposta:**
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "Success": true,
  "Message": "Depósito efetuado com sucesso",
  "NewBalance": 350000.0,
  "TransactionId": 127
}
```

### Exemplo 3: Consulta de Saldo

**Requisição:**
```http
GET /api/user/balance/user1/pass1 HTTP/1.1
Host: localhost:5000
```

**Resposta:**
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "Username": "user1",
  "Balance": 350000.0
}
```

### Exemplo 4: Consulta de Histórico de Transações

**Requisição:**
```http
GET /api/user/user1/pass1 HTTP/1.1
Host: localhost:5000
```

**Resposta:**
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "Username": "user1",
  "Balance": 350000.0,
  "Transactions": [
    {
      "Id": 127,
      "Amount": 100000.0,
      "Reference": "Depósito via transferência",
      "Timestamp": "2025-06-15T16:45:12.345Z",
      "Status": "Completed"
    },
    {
      "Id": 126,
      "Amount": 50000.0,
      "Reference": "Pagamento de fatura",
      "Timestamp": "2025-06-15T15:30:25.678Z",
      "Status": "Completed"
    }
  ]
}
```

### Exemplo 5: Tentativa de Pagamento com Saldo Insuficiente

**Requisição:**
```http
POST /api/payment/process HTTP/1.1
Host: localhost:5000
Content-Type: application/json

{
  "Username": "user1",
  "Password": "pass1",
  "Amount": 500000.0,
  "Reference": "Tentativa de pagamento grande"
}
```

**Resposta:**
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "Success": false,
  "Message": "Insufficient balance",
  "NewBalance": 350000.0,
  "TransactionId": 128
}
```

---
