/**
 * Form submission helper to diagnose issues
 */

function logFormSubmission(formId) {
    const form = document.getElementById(formId);
    if (!form) return;
    
    form.addEventListener('submit', function(event) {
        console.log('Form submitted:', formId);
        
        // Log all form fields
        const formData = new FormData(form);
        console.log('Form data:');
        for (let [key, value] of formData.entries()) {
            console.log(`${key}: ${value}`);
        }
        
        // Don't prevent submission, this is just for logging
    });
}

// Add this script to any page where you want to debug form submissions
// Usage: logFormSubmission('pacienteForm');
