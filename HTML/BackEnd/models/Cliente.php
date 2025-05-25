<?php
class Cliente {
    private $conn;
    private $table_name = "clientes";

    public $id;
    public $nome;
    public $email;
    public $telemovel;
    public $senha;

    public function __construct($db) {
        $this->conn = $db;
    }

    public function buscarPorEmail($email) {
        $query = "SELECT id, nome, email, telemovel, senha 
                  FROM " . $this->table_name . " 
                  WHERE email = :email 
                  LIMIT 1";

        $stmt = $this->conn->prepare($query);
        $stmt->bindParam(":email", $email);
        $stmt->execute();

        if($stmt->rowCount() > 0) {
            return $stmt->fetch(PDO::FETCH_ASSOC);
        }
        return false;
    }

    public function criar() {
        $query = "INSERT INTO " . $this->table_name . " 
                  (nome, email, telemovel, senha) 
                  VALUES (:nome, :email, :telemovel, :senha)";

        $stmt = $this->conn->prepare($query);

        // Hash da senha
        $senhaHash = password_hash($this->senha, PASSWORD_DEFAULT);

        $stmt->bindParam(":nome", $this->nome);
        $stmt->bindParam(":email", $this->email);
        $stmt->bindParam(":telemovel", $this->telemovel);
        $stmt->bindParam(":senha", $senhaHash);

        if($stmt->execute()) {
            return true;
        }
        return false;
    }
}
?>