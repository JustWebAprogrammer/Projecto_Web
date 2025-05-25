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
            $this->reserva->cliente_id = $dados['cliente_id'] ?? 1; // Temporário
            $this->reserva->mesa_id = $mesasDisponiveis[$i]['id'];
            $this->reserva->data = $dados['data'];
            $this->reserva->hora = $dados['hora'];
            $this->reserva->num_pessoas = $i == 0 ? $dados['num_pessoas'] : 0; // Só a primeira reserva tem o total
            $this->reserva->status = 'Reservado';

            if ($this->reserva->criar()) {
                $reservasCriadas[] = $this->db->lastInsertId();
            } else {
                // Se falhar, cancelar reservas já criadas
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

    private function validarDadosReserva($dados) {
        $validator = new ReservaValidator();
        
        // Validar data
        if (!$validator->validarData($dados['data'])) {
            return ['valido' => false, 'erro' => 'Data inválida.'];
        }

        // Validar horário
        if (!$validator->validarHorario($dados['hora'])) {
            return ['valido' => false, 'erro' => 'Horário deve ser entre 09:00 e 22:00.'];
        }

        // Validar número de pessoas
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