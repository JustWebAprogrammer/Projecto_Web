// Dados simulados
let mesasAdmin = [
    { id: 1, capacidade: 4, estado: 'Livre' },
    { id: 2, capacidade: 4, estado: 'Ocupada' },
    { id: 3, capacidade: 4, estado: 'Livre' },
    { id: 4, capacidade: 6, estado: 'Livre' },
    { id: 5, capacidade: 2, estado: 'Livre' }
];

let estatisticas = {
    totalClientes: 45,
    reservasHoje: 12,
    mesasOcupadas: 1,
    taxaOcupacao: 20
};

// Carregar dados iniciais
document.addEventListener('DOMContentLoaded', function() {
    carregarEstatisticas();
    carregarMesas();
});

function carregarEstatisticas() {
    document.getElementById('total-clientes').textContent = estatisticas.totalClientes;
    document.getElementById('reservas-hoje').textContent = estatisticas.reservasHoje;
    document.getElementById('mesas-ocupadas').textContent = estatisticas.mesasOcupadas;
    document.getElementById('taxa-ocupacao').textContent = estatisticas.taxaOcupacao + '%';
}

function carregarMesas() {
    const mesasLista = document.getElementById('mesas-lista');
    
    if (mesasAdmin.length === 0) {
        mesasLista.innerHTML = '<p>Nenhuma mesa cadastrada.</p>';
        return;
    }
    
    mesasLista.innerHTML = mesasAdmin.map(mesa => `
        <div class="mesa-item">
            <div class="mesa-info">
                <h4>Mesa ${mesa.id}</h4>
                <p>Capacidade: ${mesa.capacidade} pessoas</p>
            </div>
            <div class="mesa-controles">
                <span class="mesa-status ${mesa.estado === 'Livre' ? 'status-livre' : 'status-ocupada'}">
                    ${mesa.estado}
                </span>
                <button class="remover-btn" onclick="removerMesa(${mesa.id})">Remover</button>
            </div>
        </div>
    `).join('');
}

function abrirModalMesa() {
    document.getElementById('modal-mesa').style.display = 'block';
}

function fecharModalMesa() {
    document.getElementById('modal-mesa').style.display = 'none';
}

function adicionarMesa() {
    const capacidade = parseInt(document.getElementById('capacidade-mesa').value);
    const novoId = Math.max(...mesasAdmin.map(m => m.id), 0) + 1;
    
    mesasAdmin.push({
        id: novoId,
        capacidade: capacidade,
        estado: 'Livre'
    });
    
    alert(`Mesa ${novoId} adicionada com sucesso!`);
    fecharModalMesa();
    carregarMesas();
}

function removerMesa(mesaId) {
    const mesa = mesasAdmin.find(m => m.id === mesaId);
    
    if (mesa.estado === 'Ocupada') {
        alert('Não é possível remover uma mesa ocupada.');
        return;
    }
    
    if (confirm(`Tem certeza que deseja remover a Mesa ${mesaId}?`)) {
        mesasAdmin = mesasAdmin.filter(m => m.id !== mesaId);
        alert(`Mesa ${mesaId} removida com sucesso!`);
        carregarMesas();
    }
}

function gerarRelatorio() {
    const hoje = new Date().toLocaleDateString('pt-BR');
    const mesasOcupadas = mesasAdmin.filter(m => m.estado === 'Ocupada').length;
    const totalMesas = mesasAdmin.length;
    const ocupacao = totalMesas > 0 ? Math.round((mesasOcupadas / totalMesas) * 100) : 0;
    
    const relatorio = {
        data: hoje,
        num_reservas: estatisticas.reservasHoje,
        num_mesas_ocupadas: mesasOcupadas,
        num_expiracoes: 2,
        num_cancelamentos: 1,
        percent_ocupacao: ocupacao
    };
    
    alert(`Relatório do dia ${hoje} gerado!\n\nReservas: ${relatorio.num_reservas}\nMesas ocupadas: ${relatorio.num_mesas_ocupadas}\nTaxa de ocupação: ${relatorio.percent_ocupacao}%\nExpirações: ${relatorio.num_expiracoes}\nCancelamentos: ${relatorio.num_cancelamentos}`);
}

// Fechar modal clicando fora
window.onclick = function(event) {
    const modal = document.getElementById('modal-mesa');
    if (event.target === modal) {
        fecharModalMesa();
    }
}