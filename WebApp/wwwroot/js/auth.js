/**
 * Archivo encargado de manejar interacciones relacionadas a la Autenticación
 * Separación de responsabilidades: Lógica JS para el cliente.
 */
document.addEventListener("DOMContentLoaded", function () {
    const btnLogout = document.getElementById("btnLogout");

    if (btnLogout) {
        btnLogout.addEventListener("click", function (e) {
            e.preventDefault();


            fetch('/Auth/Logout', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            })
                .then(response => {

                    window.location.href = '/Auth/Login';
                })
                .catch(error => {
                    console.error("Ocurrió un error al intentar cerrar sesión:", error);
                });
        });
    }
});
