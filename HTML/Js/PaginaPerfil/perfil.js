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
    
    reservas.forEach((reserva, index) => {
        const podeModificar = canModifyReservation(reserva.data, reserva.horario);
        const tempoRestante = formatTimeRemaining(reserva.data, reserva.horario);
        
        const reservaCard = document.createElement('div');
        reservaCard.className = 'reserva-card';
        
        const statusClass = podeModificar ? '' : 'disabled';
        const statusMessage = podeModificar ? '' : `<p class="tempo-limite">${tempoRestante || 'Não é mais possível editar/cancelar'}</p>`;
        
        reservaCard.innerHTML = `
            <div class="reserva-info">
                <p><strong>Data:</strong> ${reserva.data}</p>
                <p><strong>Horário:</strong> ${reserva.horario}</p>
                <p><strong>Pessoas:</strong> ${reserva.pessoas}</p>
                <p><strong>Status:</strong> ${reserva.status}</p>
                ${statusMessage}
            </div>
            <div class="reserva-acoes">
                <button class="editar-reserva ${statusClass}" 
                        onclick="editarReserva(${index})" 
                        ${!podeModificar ? 'disabled title="Não é possível editar com menos de 2h de antecedência"' : ''}>
                    Editar
                </button>
                <button class="cancelar-reserva ${statusClass}" 
                        onclick="cancelarReserva(${index})" 
                        ${!podeModificar ? 'disabled title="Não é possível cancelar com menos de 2h de antecedência"' : ''}>
                    Cancelar
                </button>
            </div>
        `;
        reservasLista.appendChild(reservaCard);
    });
}

function editarReserva(index) {
    const reservas = [
        { data: '28/05/2025', horario: '19:30', pessoas: 4, status: 'Confirmada' },
        { data: '30/05/2025', horario: '20:00', pessoas: 2, status: 'Pendente' },
        { data: '02/06/2025', horario: '18:30', pessoas: 6, status: 'Confirmada' }
    ];
    
    const reserva = reservas[index];
    
    if (!canModifyReservation(reserva.data, reserva.horario)) {
        alert('Não é possível editar a reserva com menos de 2 horas de antecedência.');
        return;
    }
    
    // Redirecionar para página de reserva com parâmetros de edição
    const params = new URLSearchParams({
        edit: 'true',
        id: index,
        data: reserva.data,
        horario: reserva.horario,
        pessoas: reserva.pessoas
    });
    
    window.location.href = `Reserva.html?${params.toString()}`;
}

function cancelarReserva(index) {
    const reservas = [
        { data: '28/05/2025', horario: '19:30', pessoas: 4, status: 'Confirmada' },
        { data: '30/05/2025', horario: '20:00', pessoas: 2, status: 'Pendente' },
        { data: '02/06/2025', horario: '18:30', pessoas: 6, status: 'Confirmada' }
    ];
    
    const reserva = reservas[index];
    
    if (!canModifyReservation(reserva.data, reserva.horario)) {
        alert('Não é possível cancelar a reserva com menos de 2 horas de antecedência.');
        return;
    }
    
    if (confirm('Tem certeza que deseja cancelar esta reserva?')) {
        alert('Reserva cancelada com sucesso!');
        loadReservations(); // Recarregar a lista
    }
}
function navigateTo(page) {
    window.location.href = page;
}



function canModifyReservation(reservaData, reservaHorario) {
    const agora = new Date();
    const dataReserva = new Date(reservaData.split('/').reverse().join('-') + 'T' + reservaHorario);
    const diferencaHoras = (dataReserva - agora) / (1000 * 60 * 60); // Diferença em horas
    
    return diferencaHoras >= 2; // Permite modificação apenas se faltam 2+ horas
}

function formatTimeRemaining(reservaData, reservaHorario) {
    const agora = new Date();
    const dataReserva = new Date(reservaData.split('/').reverse().join('-') + 'T' + reservaHorario);
    const diferencaHoras = (dataReserva - agora) / (1000 * 60 * 60);
    
    if (diferencaHoras < 2) {
        const minutosRestantes = Math.floor((dataReserva - agora) / (1000 * 60));
        if (minutosRestantes > 0) {
            return `Faltam ${minutosRestantes} minutos`;
        } else {
            return 'Reserva expirada';
        }
    }
    return '';
}


// Event listeners
document.getElementById('salvar-btn').addEventListener('click', saveChanges);
document.getElementById('cancelar-btn').addEventListener('click', cancelEdit);