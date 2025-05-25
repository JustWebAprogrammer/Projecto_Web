<?php
require_once '../config/database.php';

class Reserva {
    private $conn;
    private $table_name = "reservas";

    public $id;
    public $cliente_id;
    public $mesa_id;
    public $data;
    public $hora;
    public $num_pessoas;
    public $status;

    public function __construct($db) {
        $this->conn = $db;
    }

    public function criar() {
        $query = "INSERT INTO " . $this->table_name . " 
                  (cliente_id, mesa_id, data, hora, num_pessoas, status) 
                  VALUES (:cliente_id, :mesa_id, :data, :hora, :num_pessoas, :status)";

        $stmt = $this->conn->prepare($query);

        // Limpar dados
        $this->cliente_id = htmlspecialchars(strip_tags($this->cliente_id));
        $this->mesa_id = htmlspecialchars(strip_tags($this->mesa_id));
        $this->data = htmlspecialchars(strip_tags($this->data));
        $this->hora = htmlspecialchars(strip_tags($this->hora));
        $this->num_pessoas = htmlspecialchars(strip_tags($this->num_pessoas));
        $this->status = htmlspecialchars(strip_tags($this->status));

        // Bind dos parâmetros
        $stmt->bindParam(":cliente_id", $this->cliente_id);
        $stmt->bindParam(":mesa_id", $this->mesa_id);
        $stmt->bindParam(":data", $this->data);
        $stmt->bindParam(":hora", $this->hora);
        $stmt->bindParam(":num_pessoas", $this->num_pessoas);
        $stmt->bindParam(":status", $this->status);

        if($stmt->execute()) {
            return true;
        }
        return false;
    }

    public function buscarMesasDisponiveis($data, $hora, $num_pessoas) {
        $mesasNecessarias = ceil($num_pessoas / 4);
        
        $query = "SELECT m.id, m.capacidade 
                  FROM mesas m 
                  WHERE m.estado = 'Livre' 
                  AND m.id NOT IN (
                      SELECT r.mesa_id 
                      FROM reservas r 
                      WHERE r.data = :data 
                      AND r.hora = :hora 
                      AND r.status = 'Reservado'
                  )
                  ORDER BY m.capacidade ASC 
                  LIMIT :mesas_necessarias";

        $stmt = $this->conn->prepare($query);
        $stmt->bindParam(":data", $data);
        $stmt->bindParam(":hora", $hora);
        $stmt->bindParam(":mesas_necessarias", $mesasNecessarias, PDO::PARAM_INT);
        
        $stmt->execute();
        return $stmt->fetchAll(PDO::FETCH_ASSOC);
    }

    public function atualizar() {
        $query = "UPDATE " . $this->table_name . " 
                  SET data = :data, hora = :hora, num_pessoas = :num_pessoas 
                  WHERE id = :id";

        $stmt = $this->conn->prepare($query);

        $this->data = htmlspecialchars(strip_tags($this->data));
        $this->hora = htmlspecialchars(strip_tags($this->hora));
        $this->num_pessoas = htmlspecialchars(strip_tags($this->num_pessoas));
        $this->id = htmlspecialchars(strip_tags($this->id));

        $stmt->bindParam(":data", $this->data);
        $stmt->bindParam(":hora", $this->hora);
        $stmt->bindParam(":num_pessoas", $this->num_pessoas);
        $stmt->bindParam(":id", $this->id);

        if($stmt->execute()) {
            return true;
        }
        return false;
    }
}


?>