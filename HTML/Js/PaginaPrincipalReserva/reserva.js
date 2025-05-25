// Função para limitar input a máximo de pessoas
function limitarNumeroInput(input, maxValue = 60) {
    input.addEventListener('input', function(e) {
        let value = parseInt(e.target.value);
        
        // Se o valor for maior que o máximo permitido
        if (value > maxValue) {
            e.target.value = maxValue;
        }
        
        // Se o valor for menor que 1
        if (value < 1 && e.target.value !== '') {
            e.target.value = 1;
        }
    });
    
    // Impedir digitação de caracteres que resultariam em números > maxValue
    input.addEventListener('keydown', function(e) {
        const currentValue = e.target.value;
        const key = e.key;
        
        // Permitir teclas de controle (backspace, delete, arrows, etc.)
        if (['Backspace', 'Delete', 'ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown', 'Tab'].includes(key)) {
            return;
        }
        
        // Permitir apenas números
        if (!/\d/.test(key)) {
            e.preventDefault();
            return;
        }
        
        // Verificar se o novo valor seria maior que maxValue
        const newValue = currentValue + key;
        if (parseInt(newValue) > maxValue) {
            e.preventDefault();
        }
    });
}

function validateForm(event) {
    event.preventDefault(); // Impede o envio até a validação

    // Limpar mensagem de erro anterior
    const errorMessage = document.getElementById('error-message');
    errorMessage.textContent = '';
    errorMessage.style.color = '#ff4d4d'; // Reset cor

    // Obter valores dos campos
    const dateInput = document.getElementById('reservation-date').value;
    const numPeopleInput = document.getElementById('num-people').value;
    const timeInput = document.getElementById('reservation-time').value;
    const isEdit = document.getElementById('reserva-form').dataset.editId;

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

    // Enviar dados via formulário tradicional
    enviarReservaFormulario(dateInput, timeInput, numPeople, isEdit);
    
    return false; // Impedir envio tradicional do form
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

// Verificar se estamos em modo de edição
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

// NOVA FUNÇÃO - Enviar via formulário tradicional em vez de JSON
function enviarReservaFormulario(data, hora, numPessoas, isEdit = false) {
    const errorMessage = document.getElementById('error-message');
    
    // Mostrar loading
    errorMessage.textContent = 'Processando reserva...';
    errorMessage.style.color = '#007bff';
    
    // Criar formulário dinamicamente
    const form = document.createElement('form');
    form.method = 'POST';
    form.action = 'BackEnd/api/reservas.php';
    
    // Adicionar campos do formulário
    const campos = [
        { name: 'data', value: data },
        { name: 'hora', value: hora },
        { name: 'num_pessoas', value: numPessoas },
        { name: 'cliente_id', value: 1 } // Temporário - implementar sistema de login depois
    ];
    
    // Se é edição, adicionar método PUT e ID da reserva
    if (isEdit) {
        campos.push(
            { name: '_method', value: 'PUT' },
            { name: 'reserva_id', value: isEdit }
        );
    }
    
    // Criar inputs hidden
    campos.forEach(campo => {
        const input = document.createElement('input');
        input.type = 'hidden';
        input.name = campo.name;
        input.value = campo.value;
        form.appendChild(input);
    });
    
    // Adicionar formulário ao body e submeter
    document.body.appendChild(form);
    form.submit();
}

// NOVA FUNÇÃO - Alternativa usando fetch mas com FormData
function enviarReservaFetch(data, hora, numPessoas, isEdit = false) {
    const errorMessage = document.getElementById('error-message');
    
    // Mostrar loading
    errorMessage.textContent = 'Processando reserva...';
    errorMessage.style.color = '#007bff';
    
    // Criar FormData
    const formData = new FormData();
    formData.append('data', data);
    formData.append('hora', hora);
    formData.append('num_pessoas', numPessoas);
    formData.append('cliente_id', 1); // Temporário
    
    if (isEdit) {
        formData.append('_method', 'PUT');
        formData.append('reserva_id', isEdit);
    }
    
    // Enviar com fetch
    fetch('../BackEnd/api/reservas.php', {
        method: 'POST',
        body: formData
    })
    .then(response => {
        if (response.headers.get('content-type')?.includes('application/json')) {
            return response.json();
        } else {
            // Se não for JSON, provavelmente houve redirecionamento
            // Redirecionar para a página adequada
            const mensagem = isEdit ? 'Reserva editada com sucesso!' : 'Reserva criada com sucesso!';
            alert(mensagem);
            window.location.href = '../Perfil.php';
            return;
        }
    })
    .then(resultado => {
        if (resultado && resultado.sucesso) {
            const mensagem = isEdit ? 'Reserva editada com sucesso!' : 'Reserva criada com sucesso!';
            alert(mensagem);
            window.location.href = '../Perfil.php';
        } else if (resultado && resultado.erro) {
            errorMessage.textContent = resultado.erro;
            errorMessage.style.color = '#ff4d4d';
        }
    })
    .catch(error => {
        console.error('Erro:', error);
        errorMessage.textContent = 'Erro de conexão com o servidor. Tente novamente.';
        errorMessage.style.color = '#ff4d4d';
    });
}

// Event listeners principais
document.addEventListener('DOMContentLoaded', function() {
    setDateLimits();
    checkEditMode();
    
    // Aplicar limitação aos inputs de número de pessoas
    const numPeopleInput = document.getElementById('num-people');
    if (numPeopleInput) {
        limitarNumeroInput(numPeopleInput, 60);
        
        // Monitorar mudanças no número de pessoas
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