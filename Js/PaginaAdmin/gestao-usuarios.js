// Dados simulados
let clientes = [
    {
        id: 1,
        nome: 'João Silva',
        email: 'joao@email.com',
        telemovel: '912345678',
        data_criacao: '2024-01-15',
        totalReservas: 5,
        ultimaReserva: '2024-12-20'
    },
    {
        id: 2,
        nome: 'Maria Santos',
        email: 'maria@email.com',
        telemovel: '923456789',
        data_criacao: '2024-02-10',
        totalReservas: 3,
        ultimaReserva: '2024-12-18'
    },
    {
        id: 3,
        nome: 'Pedro Costa',
        email: 'pedro@email.com',
        telemovel: '934567890',
        data_criacao: '2024-03-05',
        totalReservas: 8,
        ultimaReserva: '2024-12-22'
    }
];

let usuariosSistema = [
    {
        id: 1,
        nome: 'Admin Principal',
        email: 'admin@donaxinga.ao',
        tipo: 'Administrador',
        data_criacao: '2024-01-01'
    },
    {
        id: 2,
        nome: 'Ana Rececionista',
        email: 'ana@donaxinga.ao',
        tipo: 'Rececionista',
        data_criacao: '2024-01-15'
    }
];

let usuarioEditando = null;

// Carregar dados iniciais
document.addEventListener('DOMContentLoaded', function() {
    carregarClientes();
    carregarUsuariosSistema();
});

// Funções de Tabs
function mostrarTab(tab) {
    // Remover classe active de todos
    document.querySelectorAll('.tab-btn').forEach(btn => btn.classList.remove('active'));
    document.querySelectorAll('.tab-content').forEach(content => content.classList.remove('active'));
    
    // Adicionar classe active ao selecionado
    event.target.classList.add('active');
    document.getElementById(`tab-${tab}`).classList.add('active');
}

// Funções de Clientes
function carregarClientes() {
    const listaClientes = document.getElementById('lista-clientes');
    
    if (clientes.length === 0) {
        listaClientes.innerHTML = '<div class="lista-vazia">Nenhum cliente registrado.</div>';
        return;
    }
    
    listaClientes.innerHTML = clientes.map(cliente => `
        <div class="usuario-item">
            <div class="usuario-info">
                <h4>${cliente.nome}</h4>
                <p><strong>Email:</strong> ${cliente.email}</p>
                <p><strong>Telefone:</strong> +244 ${cliente.telemovel}</p>
                <p><strong>Registrado em:</strong> ${formatarData(cliente.data_criacao)}</p>
                <span class="usuario-badge badge-cliente">Cliente</span>
            </div>
            <div class="usuario-acoes">
                <button class="btn-detalhes" onclick="verDetalhesCliente(${cliente.id})">Detalhes</button>
            </div>
        </div>
    `).join('');
}

function buscarClientes() {
    const termo = document.getElementById('buscar-cliente').value.toLowerCase().trim();
    
    if (termo === '') {
        carregarClientes();
        return;
    }
    
    const clientesFiltrados = clientes.filter(cliente => 
        cliente.nome.toLowerCase().includes(termo) || 
        cliente.email.toLowerCase().includes(termo)
    );
    
    const listaClientes = document.getElementById('lista-clientes');
    
    if (clientesFiltrados.length === 0) {
        listaClientes.innerHTML = '<div class="lista-vazia">Nenhum cliente encontrado.</div>';
        return;
    }
    
    listaClientes.innerHTML = clientesFiltrados.map(cliente => `
        <div class="usuario-item">
            <div class="usuario-info">
                <h4>${cliente.nome}</h4>
                <p><strong>Email:</strong> ${cliente.email}</p>
                <p><strong>Telefone:</strong> +244 ${cliente.telemovel}</p>
                <p><strong>Registrado em:</strong> ${formatarData(cliente.data_criacao)}</p>
                <span class="usuario-badge badge-cliente">Cliente</span>
            </div>
            <div class="usuario-acoes">
                <button class="btn-detalhes" onclick="verDetalhesCliente(${cliente.id})">Detalhes</button>
            </div>
        </div>
    `).join('');
}

function verDetalhesCliente(clienteId) {
    const cliente = clientes.find(c => c.id === clienteId);
    
    const detalhes = `
        <div class="cliente-detalhes">
            <h4>Informações Pessoais</h4>
            <p><strong>Nome:</strong> ${cliente.nome}</p>
            <p><strong>Email:</strong> ${cliente.email}</p>
            <p><strong>Telefone:</strong> +244 ${cliente.telemovel}</p>
            <p><strong>Data de Registro:</strong> ${formatarData(cliente.data_criacao)}</p>
            
            <h4>Histórico de Reservas</h4>
            <p><strong>Total de Reservas:</strong> ${cliente.totalReservas}</p>
            <p><strong>Última Reserva:</strong> ${formatarData(cliente.ultimaReserva)}</p>
            
            <h4>Status</h4>
            <p><strong>Cliente Ativo:</strong> Sim</p>
            <p><strong>Classificação:</strong> ${cliente.totalReservas >= 5 ? 'Cliente Frequente' : 'Cliente Regular'}</p>
        </div>
    `;
    
    document.getElementById('detalhes-cliente').innerHTML = detalhes;
    document.getElementById('modal-cliente').style.display = 'block';
}

// Funções de Usuários do Sistema
function carregarUsuariosSistema() {
    const listaSistema = document.getElementById('lista-sistema');
    
    if (usuariosSistema.length === 0) {
        listaSistema.innerHTML = '<div class="lista-vazia">Nenhum usuário do sistema cadastrado.</div>';
        return;
    }
    
    listaSistema.innerHTML = usuariosSistema.map(usuario => `
        <div class="usuario-item">
            <div class="usuario-info">
                <h4>${usuario.nome}</h4>
                <p><strong>Email:</strong> ${usuario.email}</p>
                <p><strong>Criado em:</strong> ${formatarData(usuario.data_criacao)}</p>
                <span class="usuario-badge badge-${usuario.tipo.toLowerCase()}">${usuario.tipo}</span>
            </div>
            <div class="usuario-acoes">
                <button class="btn-editar" onclick="editarUsuario(${usuario.id})">Editar</button>
                <button class="btn-remover" onclick="removerUsuario(${usuario.id})">Remover</button>
            </div>
        </div>
    `).join('');
}

function abrirModalUsuario() {
    usuarioEditando = null;
    document.getElementById('modal-titulo').textContent = 'Adicionar Usuário';
    document.getElementById('form-usuario').reset();
    document.getElementById('modal-usuario').style.display = 'block';
}

function editarUsuario(usuarioId) {
    usuarioEditando = usuariosSistema.find(u => u.id === usuarioId);
    
    document.getElementById('modal-titulo').textContent = 'Editar Usuário';
    document.getElementById('nome-usuario').value = usuarioEditando.nome;
    document.getElementById('email-usuario').value = usuarioEditando.email;
    document.getElementById('tipo-usuario').value = usuarioEditando.tipo;
    document.getElementById('senha-usuario').value = ''; // Não mostrar senha
    
    document.getElementById('modal-usuario').style.display = 'block';
}

function salvarUsuario() {
    const nome = document.getElementById('nome-usuario').value.trim();
    const email = document.getElementById('email-usuario').value.trim();
    const senha = document.getElementById('senha-usuario').value;
    const tipo = document.getElementById('tipo-usuario').value;
    
    // Validações básicas
    if (!nome || !email || !tipo) {
        alert('Por favor, preencha todos os campos obrigatórios.');
        return;
    }
    
    if (!senha && !usuarioEditando) {
        alert('A senha é obrigatória para novos usuários.');
        return;
    }
    
    // Verificar email único
    const emailExiste = usuariosSistema.some(u => 
        u.email === email && (!usuarioEditando || u.id !== usuarioEditando.id)
    );
    
    if (emailExiste) {
        alert('Este email já está sendo usado por outro usuário.');
        return;
    }
    
    if (usuarioEditando) {
        // Editar usuário existente
        usuarioEditando.nome = nome;
        usuarioEditando.email = email;
        usuarioEditando.tipo = tipo;
        if (senha) {
            usuarioEditando.senha = senha; // Em produção, seria hasheada
        }
        alert('Usuário atualizado com sucesso!');
    } else {
        // Adicionar novo usuário
        const novoUsuario = {
            id: Math.max(...usuariosSistema.map(u => u.id), 0) + 1,
            nome: nome,
            email: email,
            tipo: tipo,
            senha: senha, // Em produção, seria hasheada
            data_criacao: new Date().toISOString().split('T')[0]
        };
        
        usuariosSistema.push(novoUsuario);
        alert('Usuário adicionado com sucesso!');
    }
    
    fecharModalUsuario();
    carregarUsuariosSistema();
}

function removerUsuario(usuarioId) {
    const usuario = usuariosSistema.find(u => u.id === usuarioId);
    
    if (usuario.tipo === 'Administrador' && usuariosSistema.filter(u => u.tipo === 'Administrador').length === 1) {
        alert('Não é possível remover o último administrador do sistema.');
        return;
    }
    
    if (confirm(`Tem certeza que deseja remover o usuário "${usuario.nome}"?`)) {
        usuariosSistema = usuariosSistema.filter(u => u.id !== usuarioId);
        alert('Usuário removido com sucesso!');
        carregarUsuariosSistema();
    }
}

// Funções de Modal
function fecharModalUsuario() {
    document.getElementById('modal-usuario').style.display = 'none';
    usuarioEditando = null;
}

function fecharModalCliente() {
    document.getElementById('modal-cliente').style.display = 'none';
}

// Funções Utilitárias
function formatarData(dataString) {
    const data = new Date(dataString);
    return data.toLocaleDateString('pt-BR');
}

// Event Listeners
document.getElementById('buscar-cliente').addEventListener('keypress', function(e) {
    if (e.key === 'Enter') {
        buscarClientes();
    }
});

// Fechar modal clicando fora
window.onclick = function(event) {
    const modalUsuario = document.getElementById('modal-usuario');
    const modalCliente = document.getElementById('modal-cliente');
    
    if (event.target === modalUsuario) {
        fecharModalUsuario();
    }
    if (event.target === modalCliente) {
        fecharModalCliente();
    }
}