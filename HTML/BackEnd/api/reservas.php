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
        // Aceitar dados tanto de JSON quanto de formulário
        if (isset($_POST['data']) && isset($_POST['hora']) && isset($_POST['num_pessoas'])) {
            // Dados do formulário
            $data = [
                'data' => $_POST['data'],
                'hora' => $_POST['hora'],
                'num_pessoas' => (int)$_POST['num_pessoas'],
                'cliente_id' => $_POST['cliente_id'] ?? 1
            ];
        } else {
            // Dados JSON (mantém compatibilidade)
            $json = json_decode(file_get_contents("php://input"), true);
            if (!$json) {
                http_response_code(400);
                echo json_encode(["erro" => "Dados inválidos"]);
                break;
            }
            $data = $json;
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
        // Buscar reservas por cliente
        $cliente_id = $_GET['cliente_id'] ?? null;
        if ($cliente_id) {
            $reservas = $controller->buscarReservasPorCliente($cliente_id);
            echo json_encode($reservas);
        } else {
            http_response_code(400);
            echo json_encode(["erro" => "ID do cliente necessário"]);
        }
        break;

    case 'PUT':
        // Edição de reservas
        if (isset($_POST['reserva_id'])) {
            // Dados do formulário para edição
            $data = [
                'reserva_id' => $_POST['reserva_id'],
                'data' => $_POST['data'],
                'hora' => $_POST['hora'],
                'num_pessoas' => (int)$_POST['num_pessoas']
            ];
        } else {
            // Dados JSON
            $json = json_decode(file_get_contents("php://input"), true);
            if (!$json || !isset($json['reserva_id'])) {
                http_response_code(400);
                echo json_encode(["erro" => "Dados inválidos para edição"]);
                break;
            }
            $data = $json;
        }

        $resultado = $controller->editarReserva($data);
        
        if ($resultado['sucesso']) {
            echo json_encode($resultado);
        } else {
            http_response_code(400);
            echo json_encode($resultado);
        }
        break;

    case 'DELETE':
        // Cancelamento de reservas
        $reserva_id = $_GET['reserva_id'] ?? null;
        if ($reserva_id) {
            $resultado = $controller->cancelarReserva($reserva_id);
            echo json_encode($resultado);
        } else {
            http_response_code(400);
            echo json_encode(["erro" => "ID da reserva necessário"]);
        }
        break;

    default:
        http_response_code(405);
        echo json_encode(["erro" => "Método não permitido"]);
        break;
}
?>