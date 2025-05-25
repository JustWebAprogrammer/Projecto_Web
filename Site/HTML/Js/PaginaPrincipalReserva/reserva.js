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