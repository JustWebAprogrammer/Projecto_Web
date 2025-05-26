<?php
// Verificar se a sessão já não está ativa antes de iniciar
if (session_status() == PHP_SESSION_NONE) {
    session_start();
}

require_once __DIR__ . '/../config/database.php';
require_once __DIR__ . '/../controllers/AdminController.php';

try {
    $database = new Database();
    $db = $database->getConnection();
    
    if (!$db) {
        throw new Exception("Falha na conexão com o banco de dados");
    }
    
    $controller = new AdminController($db);

    if ($_SERVER['REQUEST_METHOD'] === 'POST') {
        $email = $_POST['email'] ?? '';
        $senha = $_POST['senha'] ?? '';
        
        if (empty($email) || empty($senha)) {
            $erro = "Email e senha são obrigatórios";
        } else {
            $resultado = $controller->login($email, $senha);
            
            if ($resultado['sucesso']) {
                $_SESSION['admin_logado'] = $resultado['usuario'];
                header("Location: ../../dashboard.html");
                exit;
            } else {
                $erro = $resultado['erro'];
            }
        }
    }
} catch (Exception $e) {
    $erro = "Erro interno do servidor: " . $e->getMessage();
}

// Se chegou aqui, houve erro
if (isset($erro)) {
    header("Location: ../../admin-login.html?erro=" . urlencode($erro));
    exit;
}
?>