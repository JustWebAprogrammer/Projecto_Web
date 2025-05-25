<?php
session_start();
require_once __DIR__ . '/../config/database.php';
require_once __DIR__ . '/../models/Cliente.php';
require_once __DIR__ . '/../verificar_sessao.php';

// Verificar se o usuário está logado
if (!verificarLogin()) {
    http_response_code(401);
    echo json_encode(["erro" => "Usuário não autenticado"]);
    exit;
}

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    try {
        $database = new Database();
        $db = $database->getConnection();
        
        if (!$db) {
            throw new Exception("Falha na conexão com o banco de dados");
        }
        
        $cliente_logado = obterClienteLogado();
        
        // Receber dados do formulário
        $nome = $_POST['nome'] ?? '';
        $email = $_POST['email'] ?? '';
        $telemovel = $_POST['telemovel'] ?? '';
        
        // Validações básicas
        if (empty($nome) || empty($email) || empty($telemovel)) {
            throw new Exception("Todos os campos são obrigatórios");
        }
        
        if (strlen($nome) < 3) {
            throw new Exception("Nome deve ter pelo menos 3 caracteres");
        }
        
        if (!filter_var($email, FILTER_VALIDATE_EMAIL)) {
            throw new Exception("Email inválido");
        }
        
        // Limpar telefone e validar
        $phone_clean = preg_replace('/\D/', '', $telemovel);
        if (strlen($phone_clean) !== 9) {
            throw new Exception("Telefone deve ter exatamente 9 dígitos");
        }
        
        $cliente = new Cliente($db);
        
        // Verificar se email já existe (para outro usuário)
        $clienteExistente = $cliente->buscarPorEmail($email);
        if ($clienteExistente && $clienteExistente['id'] != $cliente_logado['id']) {
            throw new Exception("Este email já está sendo usado por outro usuário");
        }
        
        // Verificar se telefone já existe (para outro usuário)
        $clienteExistente = $cliente->buscarPorTelefone($phone_clean);
        if ($clienteExistente && $clienteExistente['id'] != $cliente_logado['id']) {
            throw new Exception("Este telefone já está sendo usado por outro usuário");
        }
        
        // Atualizar dados
        $cliente->id = $cliente_logado['id'];
        $cliente->nome = $nome;
        $cliente->email = $email;
        $cliente->telemovel = $phone_clean;
        
        if ($cliente->atualizar()) {
            // Atualizar dados na sessão
            $_SESSION['cliente_logado'] = [
                'id' => $cliente_logado['id'],
                'nome' => $nome,
                'email' => $email,
                'telemovel' => $phone_clean
            ];
            
            header("Location: ../../Perfil.php?sucesso=" . urlencode("Dados atualizados com sucesso!"));
            exit;
        } else {
            throw new Exception("Erro ao atualizar dados. Tente novamente.");
        }
        
    } catch (Exception $e) {
        header("Location: ../../Perfil.php?erro=" . urlencode($e->getMessage()));
        exit;
    }
} else {
    http_response_code(405);
    echo json_encode(["erro" => "Método não permitido"]);
}