<?php
header("Access-Control-Allow-Origin: *");
header("Content-Type: application/json; charset=UTF-8");
header("Access-Control-Allow-Methods: POST, GET, PUT, DELETE");
header("Access-Control-Allow-Headers: Content-Type, Access-Control-Allow-Headers, Authorization, X-Requested-With");

require_once '../config/database.php';
require_once '../controllers/ReservaController.php';

$database = new Database();
$db = $database->getConnection();
$controller = new ReservaController($db);

$method = $_SERVER['REQUEST_METHOD'];

switch($method) {
    case 'POST':
        $data = json_decode(file_get_contents("php://input"), true);
        
        if (!$data) {
            http_response_code(400);
            echo json_encode(["erro" => "Dados inválidos"]);
            break;
        }

        $resultado = $controller->criarReserva($data);
        
        if ($resultado['sucesso']) {
            http_response_code(201);
            echo json_encode($resultado);
        } else {
            http_response_code(400);
            echo json_encode($resultado);
        }
        break;

    case 'GET':
        // Implementar busca de reservas
        break;

    case 'PUT':
        // Implementar edição de reservas
        break;

    case 'DELETE':
        // Implementar cancelamento de reservas
        break;

    default:
        http_response_code(405);
        echo json_encode(["erro" => "Método não permitido"]);
        break;
}
?>