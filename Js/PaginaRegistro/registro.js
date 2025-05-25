// Formatar e limitar telefone em tempo real
const phoneInput = document.getElementById('phone');
phoneInput.addEventListener('input', function (e) {
    let value = e.target.value.replace(/\D/g, ''); // Remove tudo que não é dígito
    if (value.length > 9) {
        value = value.slice(0, 9); // Limita a 9 dígitos
    }
    const groups = value.match(/.{1,3}/g) || [];
    e.target.value = groups.join(' '); // Formata em grupos de três
    if (value.length === 9) {
        e.target.blur(); // Remove o foco após 9 dígitos
    }
});

function validateRegisterForm(event) {
    event.preventDefault(); // Impede o envio até a validação

    // Limpar mensagem de erro anterior
    const errorMessage = document.getElementById('error-message');
    errorMessage.textContent = '';

    // Obter valores dos campos
    const username = document.getElementById('username').value.trim();
    const fullName = document.getElementById('full-name').value.trim();
    const email = document.getElementById('email').value.trim();
    const phone = document.getElementById('phone').value.trim().replace(/\s/g, ''); // Remove espaços para validação
    const password = document.getElementById('password').value;
    const confirmPassword = document.getElementById('confirm-password').value;

    // Validar Nome de Usuário
    if (username.length < 3) {
        errorMessage.textContent = 'O nome de usuário deve ter pelo menos 3 caracteres.';
        return false;
    }

    // Validar Nome Completo
    if (fullName.length < 3) {
        errorMessage.textContent = 'O nome completo deve ter pelo menos 3 caracteres.';
        return false;
    }

    // Validar E-mail
    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailPattern.test(email)) {
        errorMessage.textContent = 'Por favor, insira um e-mail válido.';
        return false;
    }

    // Validar Telefone
    if (phone.length !== 9 || !/^\d+$/.test(phone)) {
        errorMessage.textContent = 'O telefone deve ter exatamente 9 dígitos (após +244).';
        return false;
    }

    // Validar Senha
    if (password.length < 6) {
        errorMessage.textContent = 'A senha deve ter pelo menos 6 caracteres.';
        return false;
    }

    // Validar Confirmação de Senha
    if (password !== confirmPassword) {
        errorMessage.textContent = 'As senhas não coincidem.';
        return false;
    }

    // Se todas as validações passarem
    errorMessage.textContent = 'Formulário válido! Pronto para registrar.';
    errorMessage.style.color = 'green';
    return true;
}