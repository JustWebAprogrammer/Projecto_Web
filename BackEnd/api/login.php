<?php

// Verificar se a sessão já não está ativa antes de iniciar
if (session_status() == PHP_SESSION_NONE) {
    session_start();
}

require_once __DIR__ . '/../config/database.php';
require_once __DIR__ . '/../controllers/AuthController.php';

try {
    $database = new Database();
    $db = $database->getConnection();
    
    if (!$db) {
        throw new Exception("Falha na conexão com o banco de dados");
    }
    
    $controller = new AuthController($db);

    if ($_SERVER['REQUEST_METHOD'] === 'POST') {
        // Receber dados do formulário
        $email = $_POST['email'] ?? '';
        $senha = $_POST['senha'] ?? '';
        
        if (empty($email) || empty($senha)) {
            $erro = "Email e senha são obrigatórios";
        } else {
            $resultado = $controller->login($email, $senha);
            
            if ($resultado['sucesso']) {
                $_SESSION['cliente_logado'] = $resultado['cliente'];
                header("Location: ../../Perfil.php");
                exit;
            } else {
                $erro = $resultado['erro'];
            }
        }
    }
} catch (Exception $e) {
    $erro = "Erro interno do servidor: " . $e->getMessage();
}

// If we reach here, there was an error — redirect back with error message
if (isset($erro)) {
    header("Location: ../../Login.html?erro=" . urlencode($erro));
    exit;
}

