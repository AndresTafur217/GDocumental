// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.getElementById('searchIcon').addEventListener('click', function () {
    document.getElementById('searchIcon').classList.toggle('show');
});

document.getElementById('limpiarBtn').addEventListener('click', function () {
    const form = document.querySelector('form');
    form.reset();
});