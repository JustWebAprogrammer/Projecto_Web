function validateForm(event) {
    event.preventDefault(); // Impede o envio até a validação

    // Limpar mensagem de erro anterior
    const errorMessage = document.getElementById('error-message');
    errorMessage.textContent = '';

    // Obter valores dos campos
    const dateInput = document.getElementById('reservation-date').value;
    const numPeopleInput = document.getElementById('num-people').value;
    const timeInput = document.getElementById('reservation-time').value;

    // Validar Data Desejada
    const selectedDate = new Date(dateInput);
    const today = new Date();
    today.setHours(0, 0, 0, 0); // Zera a hora pra evitar tretas de milissegundos

    const maxDate = new Date();
    maxDate.setDate(today.getDate() + 7); // 7 dias no futuro
    maxDate.setHours(0, 0, 0, 0); // Zera também pra manter a comparação justa

    if (selectedDate < today) {
    errorMessage.textContent = 'A data tem que ser hoje ou no futuro. A menos que você tenha uma máquina do tempo.';
    return false;
    }

    if (selectedDate > maxDate) {
    errorMessage.textContent = 'Você só pode marcar até 7 dias no futuro. Mais que isso, nem o Supremo Senhor Kaio sabe o que vai acontecer.';
    return false;
    }
    
    // Validar Número de Pessoas
    const numPeople = parseInt(numPeopleInput);
    if (isNaN(numPeople)) {
        errorMessage.textContent = 'Isso nem é um número. Tenta de novo.';
        return false;
    }
    if (numPeople <= 0) {
        errorMessage.textContent = 'O número de pessoas deve ser maior que 0.';
        return false;
    }
    if (numPeople > 60) {
        errorMessage.textContent = 'O número de pessoas deve ser menor que 60';
        return false;
    }


    // Validar Melhor Horário
    const [hours, minutes] = timeInput.split(':').map(Number);
    if (hours < 9 || hours > 22) {
        errorMessage.textContent = 'O horário deve estar entre 09:00 e 22:00.';
        return false;
    }

    // Se todas as validações passarem, exibir sucesso (pode ser substituído por envio ao servidor depois)
    errorMessage.textContent = 'Formulário válido! Pronto para prosseguir.';
    errorMessage.style.color = 'green';
    return true;
}

function setDateLimits() {
    const dateInput = document.getElementById('reservation-date');
    const today = new Date();
    const maxDate = new Date();
    
    // Set min to today
    dateInput.min = today.toISOString().split('T')[0];
    
    // Set max to 7 days from today
    maxDate.setDate(today.getDate() + 7);
    dateInput.max = maxDate.toISOString().split('T')[0];

     // Add time limits
     const timeInput = document.getElementById('reservation-time');
     timeInput.min = "09:00";
     timeInput.max = "22:00";
}

// Call this when the page loads
document.addEventListener('DOMContentLoaded', setDateLimits);

// Verificar se estamos em modo de edição
document.addEventListener('DOMContentLoaded', function() {
    setDateLimits();
    checkEditMode();
});

function checkEditMode() {
    const urlParams = new URLSearchParams(window.location.search);
    const isEdit = urlParams.get('edit') === 'true';
    
    if (isEdit) {
        // Preencher campos com dados existentes
        const data = urlParams.get('data');
        const horario = urlParams.get('horario');
        const pessoas = urlParams.get('pessoas');
        const reservaId = urlParams.get('id');
        document.getElementById('edit-notice').style.display = 'block';
        // Converter data de DD/MM/YYYY para YYYY-MM-DD
        if (data) {
            const [dia, mes, ano] = data.split('/');
            document.getElementById('reservation-date').value = `${ano}-${mes}-${dia}`;
        }
        
        if (horario) {
            document.getElementById('reservation-time').value = horario;
        }
        
        if (pessoas) {
            document.getElementById('num-people').value = pessoas;
        }
        
        // Alterar título e botão
        document.querySelector('.reserva-section h2').textContent = 'Editar Reserva';
        document.querySelector('.reserva-section p').textContent = 'Modifique os detalhes da sua reserva';
        document.querySelector('#reserva-form button[type="submit"]').textContent = 'Salvar Alterações';
        
        // Armazenar ID da reserva para uso posterior
        document.getElementById('reserva-form').dataset.editId = reservaId;
    }
}

// Modificar a função validateForm para lidar com edição
function validateForm(event) {
    event.preventDefault();

    const errorMessage = document.getElementById('error-message');
    errorMessage.textContent = '';

    const dateInput = document.getElementById('reservation-date').value;
    const numPeopleInput = document.getElementById('num-people').value;
    const timeInput = document.getElementById('reservation-time').value;
    const isEdit = document.getElementById('reserva-form').dataset.editId;

    // Validações existentes...
    const selectedDate = new Date(dateInput);
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    const maxDate = new Date();
    maxDate.setDate(today.getDate() + 7);
    maxDate.setHours(0, 0, 0, 0);

    // Validação especial para edição: verificar se ainda é possível editar
    if (isEdit) {
        const reservaDateTime = new Date(dateInput + 'T' + timeInput);
        const agora = new Date();
        const diferencaHoras = (reservaDateTime - agora) / (1000 * 60 * 60);
        
        if (diferencaHoras < 2) {
            errorMessage.textContent = 'Não é possível editar a reserva com menos de 2 horas de antecedência.';
            return false;
        }
    }

    // Suas validações existentes aqui...
    if (selectedDate < today) {
        errorMessage.textContent = 'A data tem que ser hoje ou no futuro.';
        return false;
    }

    if (selectedDate > maxDate) {
        errorMessage.textContent = 'Você só pode marcar até 7 dias no futuro.';
        return false;
    }

    const numPeople = parseInt(numPeopleInput);
    if (isNaN(numPeople) || numPeople <= 0 || numPeople > 60) {
        errorMessage.textContent = 'Número de pessoas deve estar entre 1 e 60.';
        return false;
    }

    const [hours, minutes] = timeInput.split(':').map(Number);
    if (hours < 9 || hours > 22) {
        errorMessage.textContent = 'O horário deve estar entre 09:00 e 22:00.';
        return false;
    }

    // Sucesso
    const mensagem = isEdit ? 'Reserva editada com sucesso!' : 'Reserva criada com sucesso!';
    alert(mensagem);
    
    // Redirecionar de volta ao perfil
    window.location.href = 'Perfil.html';
    
    return true;
}

function calcularMesasNecessarias(numPessoas) {
    if (numPessoas <= 4) {
        return 1;
    }
    return Math.ceil(numPessoas / 4);
}

// Função para distribuir pessoas nas mesas
function distribuirPessoas(numPessoas, numMesas) {
    const distribuicao = [];
    const pessoasPorMesa = Math.floor(numPessoas / numMesas);
    const pessoasRestantes = numPessoas % numMesas;
    
    for (let i = 0; i < numMesas; i++) {
        let pessoas = pessoasPorMesa;
        if (i < pessoasRestantes) {
            pessoas += 1;
        }
        distribuicao.push(Math.min(pessoas, 4));
    }
    
    return distribuicao;
}

// Função para mostrar informações sobre as mesas necessárias
function mostrarInfoMesas(numPessoas) {
    // Remover aviso anterior se existir
    const avisoExistente = document.querySelector('.info-mesas-necessarias');
    if (avisoExistente) {
        avisoExistente.remove();
    }
    
    if (numPessoas <= 0) return;
    
    const mesasNecessarias = calcularMesasNecessarias(numPessoas);
    const distribuicao = distribuirPessoas(numPessoas, mesasNecessarias);
    
    // Criar elemento de informação
    const infoDiv = document.createElement('div');
    infoDiv.className = 'info-mesas-necessarias';
    
    let avisoUniao = '';
    if (mesasNecessarias > 1) {
        avisoUniao = `
            <div class="aviso-uniao-cliente">
                <strong>📝 Importante:</strong> Para ${numPessoas} pessoas, será necessário unir ${mesasNecessarias} mesas no restaurante. 
                Entraremos em contato caso não seja possível acomodar o grupo em mesas próximas.
            </div>
        `;
    }
    
    infoDiv.innerHTML = `
        <div class="calculo-mesas-cliente">
            <h4>📊 Sua reserva para ${numPessoas} pessoa${numPessoas > 1 ? 's' : ''}</h4>
            <div class="mesas-info">
                <p><strong>Mesas necessárias:</strong> ${mesasNecessarias} mesa${mesasNecessarias > 1 ? 's' : ''}</p>
                ${avisoUniao}
                <div class="distribuicao-visual">
                    <p><strong>Como será organizado:</strong></p>
                    <div class="mesas-distribuicao">
                        ${distribuicao.map((pessoas, index) => `
                            <div class="mesa-visual">
                                <div class="mesa-icon">🪑</div>
                                <span>Mesa ${index + 1}<br><strong>${pessoas} pessoa${pessoas > 1 ? 's' : ''}</strong></span>
                            </div>
                        `).join('')}
                    </div>
                </div>
            </div>
        </div>
    `;
    
    // Inserir após o campo de número de pessoas
    const numPeopleGroup = document.querySelector('#num-people').closest('.form-group');
    numPeopleGroup.insertAdjacentElement('afterend', infoDiv);
}

// Atualizar a função validateForm para incluir a nova lógica
function validateForm(event) {
    event.preventDefault();

    const errorMessage = document.getElementById('error-message');
    errorMessage.textContent = '';
    errorMessage.style.color = '#ff4d4d'; // Reset cor

    const dateInput = document.getElementById('reservation-date').value;
    const numPeopleInput = document.getElementById('num-people').value;
    const timeInput = document.getElementById('reservation-time').value;
    const isEdit = document.getElementById('reserva-form').dataset.editId;

    // Validações existentes...
    const selectedDate = new Date(dateInput);
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    const maxDate = new Date();
    maxDate.setDate(today.getDate() + 7);
    maxDate.setHours(0, 0, 0, 0);

    if (selectedDate < today) {
        errorMessage.textContent = 'A data tem que ser hoje ou no futuro.';
        return false;
    }

    if (selectedDate > maxDate) {
        errorMessage.textContent = 'Você só pode marcar até 7 dias no futuro.';
        return false;
    }

    const numPeople = parseInt(numPeopleInput);
    if (isNaN(numPeople) || numPeople <= 0) {
        errorMessage.textContent = 'O número de pessoas deve ser maior que 0.';
        return false;
    }
    if (numPeople > 60) {
        errorMessage.textContent = 'O número de pessoas deve ser menor que 60.';
        return false;
    }

    const [hours, minutes] = timeInput.split(':').map(Number);
    if (hours < 9 || hours > 22) {
        errorMessage.textContent = 'O horário deve estar entre 09:00 e 22:00.';
        return false;
    }

    // Validação especial para edição
    if (isEdit) {
        const reservaDateTime = new Date(dateInput + 'T' + timeInput);
        const agora = new Date();
        const diferencaHoras = (reservaDateTime - agora) / (1000 * 60 * 60);
        
        if (diferencaHoras < 2) {
            errorMessage.textContent = 'Não é possível editar a reserva com menos de 2 horas de antecedência.';
            return false;
        }
    }

    // Mostrar confirmação especial para grupos grandes
    const mesasNecessarias = calcularMesasNecessarias(numPeople);
    if (mesasNecessarias > 1) {
        const confirmar = confirm(
            `Sua reserva requer ${mesasNecessarias} mesas para ${numPeople} pessoas.\n\n` +
            `As mesas serão unidas no restaurante conforme disponibilidade do espaço.\n\n` +
            `Confirma a reserva?`
        );
        
        if (!confirmar) {
            return false;
        }
    }

    // Sucesso
    const mensagem = isEdit ? 'Reserva editada com sucesso!' : 'Reserva criada com sucesso!';
    alert(mensagem);
    
    // Redirecionar
    if (document.querySelector('nav button[aria-label*="reservas"]')) {
        window.location.href = 'Perfil.html';
    } else {
        window.location.href = 'PaginaIncial.html';
    }
    
    return true;
}

// Adicionar event listener para o campo de número de pessoas
document.addEventListener('DOMContentLoaded', function() {
    setDateLimits();
    checkEditMode();
    
    // Monitorar mudanças no número de pessoas
    const numPeopleInput = document.getElementById('num-people');
    if (numPeopleInput) {
        numPeopleInput.addEventListener('input', function() {
            const numPessoas = parseInt(this.value) || 0;
            mostrarInfoMesas(numPessoas);
        });
        
        numPeopleInput.addEventListener('change', function() {
            const numPessoas = parseInt(this.value) || 0;
            mostrarInfoMesas(numPessoas);
        });
    }
});