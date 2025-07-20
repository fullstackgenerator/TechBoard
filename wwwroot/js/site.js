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
            // Show company form if there are company validation errors
            companyForm.classList.remove('d-none');
            userForm.classList.add('d-none');
            registrationTypeToggle.checked = true;
        }
        else if (userErrors.length > 0) {
            // Show user form if there are user validation errors
            userForm.classList.remove('d-none');
            companyForm.classList.add('d-none');
            registrationTypeToggle.checked = false;
        }
        else {
            // Default to user form (this is the key fix)
            userForm.classList.remove('d-none');
            companyForm.classList.add('d-none');
            registrationTypeToggle.checked = false; // Ensure toggle is unchecked for user
        }
    }

    // Initialize form visibility on page load
    initializeFormVisibility();

    // Handle toggle switch changes
    registrationTypeToggle.addEventListener('change', function() {
        if (this.checked) {
            // Switch to company registration
            userForm.classList.add('d-none');
            companyForm.classList.remove('d-none');
        }
        else {
            // Switch to user registration
            companyForm.classList.add('d-none');
            userForm.classList.remove('d-none');
        }

        // Clear all validation errors when switching
        const allErrorSpans = document.querySelectorAll('.text-danger');
        allErrorSpans.forEach(span => span.textContent = '');

        const validationSummary = document.querySelector('[asp-validation-summary="All"]');
        if (validationSummary) {
            validationSummary.innerHTML = '';
        }
    });
});