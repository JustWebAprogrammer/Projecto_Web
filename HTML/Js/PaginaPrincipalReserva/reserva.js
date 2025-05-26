// Função para limitar input a máximo de pessoas
function limitarNumeroInput(input, maxValue = 60) {
    input.addEventListener('input', function(e) {
        let value = parseInt(e.target.value);
        
        if (value > maxValue) {
            e.target.value = maxValue;
        }
        
        if (value < 1 && e.target.value !== '') {
            e.target.value = 1;
        }
    });
    
    input.addEventListener('keydown', function(e) {
        const currentValue = e.target.value;
        const key = e.key;
        
        if (['Backspace', 'Delete', 'ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown', 'Tab'].includes(key)) {
            return;
        }
        
        if (!/\d/.test(key)) {
            e.preventDefault();
            return;
        }
        
        const newValue = currentValue + key;
        if (parseInt(newValue) > maxValue) {
            e.preventDefault();
        }
    });
}

function validateForm(event) {
    event.preventDefault();

   
    // Limpar mensagem de erro anterior
    const errorMessage = document.getElementById('error-message');
    errorMessage.textContent = '';
    errorMessage.style.color = '#ff4d4d';

    // Obter valores dos campos
    const dateInput = document.getElementById('reservation-date').value;
    const numPeopleInput = document.getElementById('num-people').value;
    const timeInput = document.getElementById('reservation-time').value;
    const isEdit = document.getElementById('reserva-form').dataset.editId;

    // Validar Data Desejada
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
    
    // Validar Número de Pessoas
    const numPeople = parseInt(numPeopleInput);
    if (isNaN(numPeople)) {
        errorMessage.textContent = 'Por favor, insira um número válido de pessoas.';
        return false;
    }
    if (numPeople <= 0) {
        errorMessage.textContent = 'O número de pessoas deve ser maior que 0.';
        return false;
    }
    if (numPeople > 60) {
        errorMessage.textContent = 'O número máximo de pessoas é 60.';
        return false;
    }

    // Validar Melhor Horário - APENAS se o campo não estiver vazio
    if (timeInput && timeInput.trim() !== '') {
        const [hours, minutes] = timeInput.split(':').map(Number);
        if (isNaN(hours) || isNaN(minutes) || hours < 9 || hours > 22) {
            errorMessage.textContent = 'O horário deve estar entre 09:00 e 22:00.';
            return false;
        }
    } else {
        errorMessage.textContent = 'Por favor, selecione um horário.';
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

    // Mostrar confirmação para grupos grandes
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

    // Mostrar loading
    const submitBtn = document.querySelector('#reserva-form button[type="submit"]');
    const originalText = submitBtn.textContent;
    submitBtn.textContent = isEdit ? 'Salvando...' : 'Processando...';
    submitBtn.disabled = true;

    // Enviar formulário
    document.getElementById('reserva-form').submit();
    
    return false;
}

function setDateLimits() {
    const dateInput = document.getElementById('reservation-date');
    const today = new Date();
    const maxDate = new Date();
    
    dateInput.min = today.toISOString().split('T')[0];
    
    maxDate.setDate(today.getDate() + 7);
    dateInput.max = maxDate.toISOString().split('T')[0];

    const timeInput = document.getElementById('reservation-time');
    timeInput.min = "09:00";
    timeInput.max = "22:00";
}

function checkEditMode() {
    const urlParams = new URLSearchParams(window.location.search);
    const isEdit = urlParams.get('edit') === 'true';
    
    if (isEdit) {
        const data = urlParams.get('data');
        const horario = urlParams.get('horario');
        const pessoas = urlParams.get('pessoas');
        const reservaId = urlParams.get('id');
        
        document.getElementById('edit-notice').style.display = 'block';
        
        // Converter data se necessário
        if (data) {
            let dataFormatted;
            if (data.includes('/')) {
                const [dia, mes, ano] = data.split('/');
                dataFormatted = `${ano}-${mes.padStart(2, '0')}-${dia.padStart(2, '0')}`;
            } else {
                dataFormatted = data;
            }
            document.getElementById('reservation-date').value = dataFormatted;
        }
        
        if (horario) {
            document.getElementById('reservation-time').value = horario;
        }
        
        if (pessoas) {
            document.getElementById('num-people').value = pessoas;
        }
        
        // Alterar textos da interface
        document.querySelector('.reserva-section h2').textContent = 'Editar Reserva';
        document.querySelector('.reserva-section p').textContent = 'Modifique os detalhes da sua reserva';
        document.querySelector('#reserva-form button[type="submit"]').textContent = 'Salvar Alterações';
        
        // Adicionar campo hidden para método PUT
        const methodField = document.createElement('input');
        methodField.type = 'hidden';
        methodField.name = '_method';
        methodField.value = 'PUT';
        document.getElementById('reserva-form').appendChild(methodField);
        
        // Adicionar campo hidden para ID da reserva
        const idField = document.createElement('input');
        idField.type = 'hidden';
        idField.name = 'reserva_id';
        idField.value = reservaId;
        document.getElementById('reserva-form').appendChild(idField);
        
        document.getElementById('reserva-form').dataset.editId = reservaId;
    }
}

function calcularMesasNecessarias(numPessoas) {
    if (numPessoas <= 4) {
        return 1;
    }
    return Math.ceil(numPessoas / 4);
}

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

function mostrarInfoMesas(numPessoas) {
    const avisoExistente = document.querySelector('.info-mesas-necessarias');
    if (avisoExistente) {
        avisoExistente.remove();
    }
    
    if (numPessoas <= 0) return;
    
    const mesasNecessarias = calcularMesasNecessarias(numPessoas);
    const distribuicao = distribuirPessoas(numPessoas, mesasNecessarias);
    
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
    
    const numPeopleGroup = document.querySelector('#num-people').closest('.form-group');
    numPeopleGroup.insertAdjacentElement('afterend', infoDiv);
}

// Verificar se há mensagens de erro ou sucesso na URL
document.addEventListener('DOMContentLoaded', function() {
    const urlParams = new URLSearchParams(window.location.search);
    const erro = urlParams.get('erro');
    const sucesso = urlParams.get('sucesso');
    
    const errorMessage = document.getElementById('error-message');
    
    if (erro) {
        errorMessage.textContent = decodeURIComponent(erro);
        errorMessage.style.color = '#ff4d4d';
    } else if (sucesso) {
        errorMessage.textContent = decodeURIComponent(sucesso);
        errorMessage.style.color = '#28a745';
    }
    
    setDateLimits();
    checkEditMode();
    
    // Configurar input de número de pessoas
    const numPeopleInput = document.getElementById('num-people');
    if (numPeopleInput) {
        limitarNumeroInput(numPeopleInput, 60);
        
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