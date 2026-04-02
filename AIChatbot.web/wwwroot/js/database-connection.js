// database-connection.js

var DatabaseConnection = (function () {

    function init() {
        bindEvents();
        toggleCredentials(); // initial state
    }

    function bindEvents() {
        $('input[name="AuthType"]').on('change', toggleCredentials);
        $('#testBtn').on('click', testConnection);
    }

    function toggleCredentials() {
        var isSql = $('#sqlAuth').is(':checked');

        if (isSql) {
            $('#sqlCredentials').slideDown(200);
        } else {
            $('#sqlCredentials').slideUp(200);
            clearCredentials();
        }
    }

    function clearCredentials() {
        $('#sqlCredentials input').val('');
    }

    function testConnection() {

        var data = {
            ServerName: $('input[name="ServerName"]').val(),
            DatabaseName: $('input[name="DatabaseName"]').val(),
            Username: $('input[name="Username"]').val(),
            Password: $('input[name="Password"]').val(),
            AuthType: $('input[name="AuthType"]:checked').val()
        };

        $.ajax({
            url: '/Database/TestConnection',
            type: 'POST',
            data: data,
            success: function () {
                alert('Connection Successful ✅');
            },
            error: function () {
                alert('Connection Failed ❌');
            }
        });
    }

    return {
        init: init
    };

})();

// INIT
$(document).ready(function () {
    DatabaseConnection.init();
});