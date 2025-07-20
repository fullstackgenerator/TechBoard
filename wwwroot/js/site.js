// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener('DOMContentLoaded', function () {
    const registrationTypeToggle = document.getElementById('registrationTypeToggle');
    const userForm = document.getElementById('userRegistrationForm');
    const companyForm = document.getElementById('companyRegistrationForm');
    
    function initializeFormVisibility() {

        const companyErrors = document.querySelectorAll('#companyRegistrationForm .text-danger:not(:empty)');

        const userErrors = document.querySelectorAll('#userRegistrationForm .text-danger:not(:empty)');

        if (companyErrors.length > 0) {
            companyForm.classList.remove('d-none');
            userForm.classList.add('d-none');
            registrationTypeToggle.checked = true;
        } 
        
        else if (userErrors.length > 0) {
            userForm.classList.remove('d-none');
            companyForm.classList.add('d-none');
            registrationTypeToggle.checked = false;
        }
        
        else {
            userForm.classList.remove('d-none');
            companyForm.classList.add('d-none');
            registrationTypeToggle.checked = false;
        }
    }
    
    initializeFormVisibility();

    registrationTypeToggle.addEventListener('change', function() {
        if (this.checked) {
            userForm.classList.add('d-none');
            companyForm.classList.remove('d-none');
        } 
        
        else {
            companyForm.classList.add('d-none');
            userForm.classList.remove('d-none');
        }
        
        const allErrorSpans = document.querySelectorAll('.text-danger');
        allErrorSpans.forEach(span => span.textContent = '');
        
        const validationSummary = document.querySelector('[asp-validation-summary="All"]');
        if (validationSummary) {
            validationSummary.innerHTML = '';
        }
    });
});