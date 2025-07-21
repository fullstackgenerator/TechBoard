// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// home page
document.addEventListener('DOMContentLoaded', function() {
    // card animation on scroll
    const observer = new IntersectionObserver((entries) => {
        entries.forEach((entry, index) => {
            if (entry.isIntersecting) {
                setTimeout(() => {
                    entry.target.style.opacity = '1';
                    entry.target.style.transform = 'translateY(0)';
                }, index * 100);
            }
        });
    }, { threshold: 0.1 });

    document.querySelectorAll('.job-card').forEach(card => {
        card.style.opacity = '0';
        card.style.transform = 'translateY(20px)';
        card.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
        observer.observe(card);
    });

    const form = document.querySelector('form');
    if (form) {
        form.addEventListener('submit', function(e) {
            const button = form.querySelector('button[type="submit"]');
            button.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Searching...';
            button.disabled = true;
        });
    }
});

// user/company registration
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

    // toggle switch changes
    registrationTypeToggle.addEventListener('change', function() {
        if (this.checked) {
            userForm.classList.add('d-none');
            companyForm.classList.remove('d-none');
        }
        else {
            companyForm.classList.add('d-none');
            userForm.classList.remove('d-none');
        }

        // clear validation errors when switching
        const allErrorSpans = document.querySelectorAll('.text-danger');
        allErrorSpans.forEach(span => span.textContent = '');

        const validationSummary = document.querySelector('[asp-validation-summary="All"]');
        if (validationSummary) {
            validationSummary.innerHTML = '';
        }
    });
});