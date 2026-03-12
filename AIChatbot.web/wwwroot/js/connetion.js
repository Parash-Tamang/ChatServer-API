document.addEventListener('DOMContentLoaded', function () {

    const form = document.getElementById('dbConnectForm');
    const connectBtn = document.getElementById('dbConnectBtn');
    const connectText = document.getElementById('dbConnectText');
    const spinner = document.getElementById('dbConnectSpinner');
    const errorMsg = document.getElementById('dbErrorMsg');
    const modal = bootstrap.Modal.getOrCreateInstance(
        document.getElementById('dbConnectModal'));

    form.addEventListener('submit', async function (e) {
        e.preventDefault(); // stops full page POST

        // --- loading state ---
        connectBtn.disabled = true;
        spinner.classList.remove('d-none');
        connectText.textContent = 'Connecting...';
        errorMsg.classList.add('d-none');

        try {
            const formData = new FormData(form);

            // Posts to your MVC controller — NOT an exposed API
            const response = await fetch(form.action, {
                method: 'POST',
                headers: {
                    // ASP.NET antiforgery token — keeps it secure
                    'RequestVerificationToken': document.querySelector(
                        'input[name="__RequestVerificationToken"]').value
                },
                body: formData
            });

            const result = await response.json();

            if (result.success) {
                // Show the connection flag
                document.getElementById('dbConnectionLabel').textContent = result.database;
                document.getElementById('dbConnectionFlag').classList.remove('d-none');

                modal.hide();
                form.reset();
            } else {
                // Show error inside modal — no page refresh
                errorMsg.textContent = result.message;
                errorMsg.classList.remove('d-none');
            }

        } catch (err) {
            errorMsg.textContent = 'Connection failed. Please try again.';
            errorMsg.classList.remove('d-none');
        } finally {
            // --- reset loading state ---
            connectBtn.disabled = false;
            spinner.classList.add('d-none');
            connectText.textContent = 'Connect';
        }
    });

    // Disconnect
    document.getElementById('disconnectDb').addEventListener('click', function () {
        document.getElementById('dbConnectionFlag').classList.add('d-none');
        // optionally call a disconnect endpoint here
    });

});