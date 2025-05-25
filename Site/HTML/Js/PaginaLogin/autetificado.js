function validateLoginForm(event) {
    event.preventDefault(); // Impede o envio até a validação

    // Limpar mensagem de erro anterior
    const errorMessage = document.getElementById('error-message');
    errorMessage.textContent = '';

    // Obter valores dos campos
    const email = document.getElementById('email').value.trim();
    const senha = document.getElementById('senha').value.trim();

    // Validar E-mail
    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailPattern.test(email)) {
        errorMessage.textContent = 'Por favor, insira um e-mail válido.';
        return false;
    }

    // Validar Senha
    if (senha.length < 6) {
        errorMessage.textContent = 'A senha deve ter pelo menos 6 caracteres.';
        return false;
    }

    // Placeholder para futura integração com back-end
    alert('Login válido! (Simulação: redirecionaria para dashboard.php)');
    // Aqui você integraria o envio ao PHP (e.g., fetch ou form submission)
    return false; // Impede envio real por enquanto
}