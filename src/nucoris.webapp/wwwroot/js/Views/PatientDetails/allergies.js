/////////////////////////////////////////////////////////////////////
// ALLERGIES
// Adding an allergy is similar to starting a new admission, 
//  but removing allergies is a bit trickier because we may have several of them,
//  each displayed with its own "Remove" button.
// We resolve it by binding to each button an event handler on click,
//  which sends the Delete request when you click the button,
//  and on success reloads the allergies (we have defined a partial view for them)
//  and binds again event handlers to them.

// Confirmation dialog and AJAX call to remove an existing allergy:
let removeAllergyCallback = (allergy) => confirmDialog(`Are you sure you want to remove allergy '${allergy}'?`, (confirmed) => {
    if (confirmed) {
        $.ajax({
            type: 'Delete',
            url: '/api/patients/' + $('#patientIds').data('patientid') + `/allergies/${allergy}`,
            success: reloadAllergiesAfterChanges
        });
    }
});

// Function to bind the remove allergy callback to the click event of an [allergy] element:
let attachRemoveAllergyCallbackToAllergyElement = (element) => {
    $(element).on('click', () => {
        removeAllergyCallback($(element).data('allergyname'));
    })
};

// Existing allergies are represented as elements of class 'nucoris-allergy'
// This function gets all such elements and binds the remove allergy callback to each of them:
let addRemoveCallbackToAllAllergyElements = () => $('.nucoris-allergy').each((i, element) => attachRemoveAllergyCallbackToAllergyElement(element));

addRemoveCallbackToAllAllergyElements(); // invoke function now so it's attached to existing allergies when page initially loaded

// Function that reloads allergies by invoking a partial view, then binds remove callback to loaded allergies.
let reloadAllergiesAfterChanges = () => {
    $('#allergies').load("/PatientDetails/Allergies?patientId=" + $('#patientIds').data('patientid'),
        null,
        () => addRemoveCallbackToAllAllergyElements()
    );
};

// Adding a new allergy it's easier, we just need to grab the new allergy, post it, and reload the partial view:
let addAllergyCallback = () => addTextItemDialog("Please type in the new allergy:", (newAllergy) => {
    if (newAllergy) {
        $.ajax({
            type: 'Post',
            url: '/api/patients/' + $('#patientIds').data('patientid') + '/allergies/' + newAllergy,
            success: reloadAllergiesAfterChanges
        });
    }
});
$('#addAllergy').on('click', () => addAllergyCallback());
