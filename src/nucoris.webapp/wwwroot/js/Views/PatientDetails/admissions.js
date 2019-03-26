/////////////////////////////////////////////////////////////////////
// ADMISSIONS:
// Admission end: confirmation dialog and AJAX calls to web api
let endAdmissionCallback = () => confirmDialog("Are you sure you want to discharge this patient?", (confirmed) => {
    if (confirmed) {
        $.ajax({
            type: 'Delete',
            url: '/api/patients/' + $('#patientIds').data('patientid') + '/admissions/' + $('#patientIds').data('admissionid'),
            success: function () {
                location.reload();
            }
        });
    }
});
$('#endAdmission').on('click', () => endAdmissionCallback());

// Admission start (new admission): confirmation dialog and AJAX calls to web api
let newAdmissionCallback = () => confirmDialog("Are you sure you want to readmit this patient?", (confirmed) => {
    if (confirmed) {
        $.ajax({
            type: 'Post',
            url: '/api/patients/' + $('#patientIds').data('patientid') + '/admissions',
            success: function () {
                location.reload();
            }
        });
    }
});
$('#newAdmission').on('click', () => newAdmissionCallback());

