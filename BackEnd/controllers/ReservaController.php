<?php
require_once '../models/Reserva.php';
require_once '../utils/validation.php';

class ReservaController {
    private $db;
    private $reserva;

    public function __construct($database) {
        $this->db = $database;
        $this->reserva = new Reserva($this->db);
    }

    public function criarReserva($dados) {
        // Validar dados
        $validacao = $this->validarDadosReserva($dados);
        if (!$validacao['valido']) {
            return [
                'sucesso' => false,
                'erro' => $validacao['erro']
            ];
        }

        // Verificar disponibilidade de mesas
        $mesasDisponiveis = $this->reserva->buscarMesasDisponiveis(
            $dados['data'],
            $dados['hora'],
            $dados['num_pessoas']
        );

        $mesasNecessarias = ceil($dados['num_pessoas'] / 4);
        
        if (count($mesasDisponiveis) < $mesasNecessarias) {
            return [
                'sucesso' => false,
                'erro' => 'Não há mesas suficientes disponíveis para este horário.'
            ];
        }

        // Criar reservas (uma para cada mesa necessária)
        $reservasCriadas = [];
        for ($i = 0; $i < $mesasNecessarias; $i++) {
            $this->reserva->cliente_id = $dados['cliente_id'];
            $this->reserva->mesa_id = $mesasDisponiveis[$i]['id'];
            $this->reserva->data = $dados['data'];
            $this->reserva->hora = $dados['hora'];
            $this->reserva->num_pessoas = $i == 0 ? $dados['num_pessoas'] : 0;
            $this->reserva->status = 'Reservado';

            if ($this->reserva->criar()) {
                $reservasCriadas[] = $this->db->lastInsertId();
            } else {
                $this->cancelarReservas($reservasCriadas);
                return [
                    'sucesso' => false,
                    'erro' => 'Erro ao criar reserva.'
                ];
            }
        }

        return [
            'sucesso' => true,
            'mensagem' => 'Reserva criada com sucesso!',
            'reservas_ids' => $reservasCriadas
        ];
    }

    public function editarReserva($dados) {
        // Verificar se a reserva existe e pode ser editada
        $reservaExistente = $this->buscarReservaPorId($dados['reserva_id']);
        if (!$reservaExistente) {
            return [
                'sucesso' => false,
                'erro' => 'Reserva não encontrada.'
            ];
        }

        // Verificar se ainda é possível editar (2 horas antes)
        $dataHoraReserva = new DateTime($reservaExistente['data'] . ' ' . $reservaExistente['hora']);
        $agora = new DateTime();
        $diferenca = $dataHoraReserva->diff($agora);
        $horasRestantes = ($diferenca->days * 24) + $diferenca->h;

        if ($horasRestantes < 2) {
            return [
                'sucesso' => false,
                'erro' => 'Não é possível editar a reserva com menos de 2 horas de antecedência.'
            ];
        }

        // Validar novos dados
        $validacao = $this->validarDadosReserva($dados);
        if (!$validacao['valido']) {
            return [
                'sucesso' => false,
                'erro' => $validacao['erro']
            ];
        }

        // Atualizar reserva
        $this->reserva->id = $dados['reserva_id'];
        $this->reserva->data = $dados['data'];
        $this->reserva->hora = $dados['hora'];
        $this->reserva->num_pessoas = $dados['num_pessoas'];

        if ($this->reserva->atualizar()) {
            return [
                'sucesso' => true,
                'mensagem' => 'Reserva atualizada com sucesso!'
            ];
        } else {
            return [
                'sucesso' => false,
                'erro' => 'Erro ao atualizar reserva.'
            ];
        }
    }

    private function buscarReservaPorId($id) {
        $query = "SELECT * FROM reservas WHERE id = :id LIMIT 1";
        $stmt = $this->db->prepare($query);
        $stmt->bindParam(":id", $id);
        $stmt->execute();
        
        if ($stmt->rowCount() > 0) {
            return $stmt->fetch(PDO::FETCH_ASSOC);
        }
        return false;
    }

    // MÉTODO ATUALIZADO PARA INCLUIR FILTRO
    public function buscarReservasPorCliente($cliente_id, $filtro = 'proximas') {
        $hoje = date('Y-m-d');
        $agora = date('H:i:s');
        
        switch($filtro) {
            case 'proximas':
                $query = "SELECT r.*, m.capacidade 
                          FROM reservas r 
                          JOIN mesas m ON r.mesa_id = m.id 
                          WHERE r.cliente_id = :cliente_id 
                          AND r.status = 'Reservado'
                          AND (r.data > :hoje OR (r.data = :hoje AND r.hora >= :agora))
                          ORDER BY r.data ASC, r.hora ASC";
                break;
                
            case 'passadas':
                $query = "SELECT r.*, m.capacidade 
                          FROM reservas r 
                          JOIN mesas m ON r.mesa_id = m.id 
                          WHERE r.cliente_id = :cliente_id 
                          AND (r.status = 'Concluído' OR 
                               (r.status = 'Reservado' AND (r.data < :hoje OR (r.data = :hoje AND r.hora < :agora))))
                          ORDER BY r.data DESC, r.hora DESC";
                break;
                
            case 'canceladas':
                $query = "SELECT r.*, m.capacidade 
                          FROM reservas r 
                          JOIN mesas m ON r.mesa_id = m.id 
                          WHERE r.cliente_id = :cliente_id 
                          AND r.status = 'Cancelado'
                          ORDER BY r.data DESC, r.hora DESC";
                break;
                
            default:
                $query = "SELECT r.*, m.capacidade 
                          FROM reservas r 
                          JOIN mesas m ON r.mesa_id = m.id 
                          WHERE r.cliente_id = :cliente_id 
                          ORDER BY r.data DESC, r.hora DESC";
        }
        
        $stmt = $this->db->prepare($query);
        $stmt->bindParam(":cliente_id", $cliente_id);
        
        if ($filtro === 'proximas' || $filtro === 'passadas') {
            $stmt->bindParam(":hoje", $hoje);
            $stmt->bindParam(":agora", $agora);
        }
        
        $stmt->execute();
        
        return [
            'sucesso' => true,
            'reservas' => $stmt->fetchAll(PDO::FETCH_ASSOC)
        ];
    }

    public function cancelarReserva($reserva_id) {
        $query = "UPDATE reservas SET status = 'Cancelado' WHERE id = :id";
        $stmt = $this->db->prepare($query);
        $stmt->bindParam(":id", $reserva_id);
        
        if ($stmt->execute()) {
            return [
                'sucesso' => true,
                'mensagem' => 'Reserva cancelada com sucesso!'
            ];
        } else {
            return [
                'sucesso' => false,
                'erro' => 'Erro ao cancelar reserva.'
            ];
        }
    }

    private function validarDadosReserva($dados) {
        $validator = new ReservaValidator();
        
        if (!$validator->validarData($dados['data'])) {
            return ['valido' => false, 'erro' => 'Data inválida.'];
        }

        if (!$validator->validarHorario($dados['hora'])) {
            return ['valido' => false, 'erro' => 'Horário deve ser entre 09:00 e 22:00.'];
        }

        if (!$validator->validarNumPessoas($dados['num_pessoas'])) {
            return ['valido' => false, 'erro' => 'Número de pessoas deve ser entre 1 e 60.'];
        }

        return ['valido' => true];
    }

    private function cancelarReservas($ids) {
        foreach ($ids as $id) {
            $query = "UPDATE reservas SET status = 'Cancelado' WHERE id = :id";
            $stmt = $this->db->prepare($query);
            $stmt->bindParam(":id", $id);
            $stmt->execute();
        }
    }
}
?>