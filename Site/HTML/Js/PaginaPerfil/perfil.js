// Dados simulados do usuário (serão substituídos pelos dados do backend)
const userData = {
    nomeUsuario: 'João Silva',
    email: 'joao.silva@email.com',
    telefone: '915 630 555'
};

// Carregar dados do usuário ao carregar a página
document.addEventListener('DOMContentLoaded', function() {
    loadUserData();
    loadReservations();
});

function loadUserData() {
    document.getElementById('nome-usuario').value = userData.nomeUsuario;
    document.getElementById('email').value = userData.email;
    document.getElementById('telefone').value = userData.telefone;
}

function toggleEdit(fieldId) {
    const input = document.getElementById(fieldId);
    const salvarBtn = document.getElementById('salvar-btn');
    const cancelarBtn = document.getElementById('cancelar-btn');
    
    if (input.readOnly) {
        input.readOnly = false;
        input.focus();
        salvarBtn.style.display = 'block';
        cancelarBtn.style.display = 'block';
    }
}

function cancelEdit() {
    // Recarregar dados originais
    loadUserData();
    
    // Tornar campos read-only novamente
    document.getElementById('nome-usuario').readOnly = true;
    document.getElementById('email').readOnly = true;
    document.getElementById('telefone').readOnly = true;
    
    // Esconder botões
    document.getElementById('salvar-btn').style.display = 'none';
    document.getElementById('cancelar-btn').style.display = 'none';
}

function saveChanges() {
    // Validar dados antes de salvar
    const nomeUsuario = document.getElementById('nome-usuario').value.trim();
    const email = document.getElementById('email').value.trim();
    const telefone = document.getElementById('telefone').value.trim();
    
    // Validação básica
    if (nomeUsuario.length < 3) {
        alert('Nome de usuário deve ter pelo menos 3 caracteres.');
        return;
    }
    
    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailPattern.test(email)) {
        alert('Por favor, insira um e-mail válido.');
        return;
    }
    
    // Aqui você faria a chamada para o backend para salvar os dados
    console.log('Salvando dados:', { nomeUsuario, email, telefone });
    
    // Simular sucesso
    alert('Dados salvos com sucesso!');
    
    // Tornar campos read-only novamente
    document.getElementById('nome-usuario').readOnly = true;
    document.getElementById('email').readOnly = true;
    document.getElementById('telefone').readOnly = true;
    
    // Esconder botões
    document.getElementById('salvar-btn').style.display = 'none';
    document.getElementById('cancelar-btn').style.display = 'none';
}

function filterReservas(tipo) {
    // Remover classe active de todos os botões
    document.querySelectorAll('.historico-btn').forEach(btn => {
        btn.classList.remove('active');
    });
    
    // Adicionar classe active ao botão clicado
    event.target.classList.add('active');
    
    // Aqui você filtraria as reservas baseado no tipo
    console.log('Filtrando reservas por:', tipo);
    loadReservations(tipo);
}

function loadReservations(filtro = 'proximas') {
    // Dados simulados de reservas
    const reservas = [
        {
            data: '28/05/2025',
            horario: '19:30',
            pessoas: 4,
            status: 'Confirmada'
        },
        {
            data: '30/05/2025',
            horario: '20:00',
            pessoas: 2,
            status: 'Pendente'
        },
        {
            data: '02/06/2025',
            horario: '18:30',
            pessoas: 6,
            status: 'Confirmada'
        }
    ];
    
    const reservasLista = document.getElementById('reservas-lista');
    reservasLista.innerHTML = '';
    
    reservas.forEach(reserva => {
        const reservaCard = document.createElement('div');
        reservaCard.className = 'reserva-card';
        reservaCard.innerHTML = `
            <div class="reserva-info">
                <p><strong>Data:</strong> ${reserva.data}</p>
                <p><strong>Horário:</strong> ${reserva.horario}</p>
                <p><strong>Pessoas:</strong> ${reserva.pessoas}</p>
                <p><strong>Status:</strong> ${reserva.status}</p>
            </div>
            <div class="reserva-acoes">
                <button class="editar-reserva" onclick="editarReserva()">Editar</button>
                <button class="cancelar-reserva" onclick="cancelarReserva()">Cancelar</button>
            </div>
        `;
        reservasLista.appendChild(reservaCard);
    });
}

function editarReserva() {
    alert('Redirecionando para edição de reserva...');
    // Aqui você redirecionaria para a página de edição
}

function cancelarReserva() {
    if (confirm('Tem certeza que deseja cancelar esta reserva?')) {
        alert('Reserva cancelada com sucesso!');
        // Aqui você faria a chamada para o backend
    }
}

function navigateTo(page) {
    window.location.href = page;
}

// Event listeners
document.getElementById('salvar-btn').addEventListener('click', saveChanges);
document.getElementById('cancelar-btn').addEventListener('click', cancelEdit);