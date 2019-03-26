// To assign an order we show a user selection modal with Assign and Unassign buttons.
// When modal is shown we inject the orderid into an attribute on each button.
// When user clicks on Assign or Unassign buttons we get the orderid from its attribute
//  and the selected userid from the dropdown box in Assign.
// Then we call the Patch method of the Orders REST API to apply the changes,
//  and we reload the order partial view so it's displayed on screen
$('#userSelectionModal').on('show.bs.modal', function (event) {
    let order = $(event.relatedTarget); // Button that triggered the modal
    let orderDesc = order.data('orderdesc');
    let orderId = order.data('orderid');
    let modal = $(this);
    modal.find('#orderUserAssigned').attr('orderid', orderId);
    modal.find('#orderUserUnassigned').attr('orderid', orderId);
    modal.find('.modal-title').text(orderDesc);
});

$('#orderUserAssigned').on('click', () => {
    let orderId = $('#orderUserAssigned').attr('orderid');
    let userId = $("#userSelection").val();

    $.ajax({
        type: 'Patch',
        url: '/api/patients/' + $('#patientIds').data('patientid') + `/orders/${orderId}?assignedUserId=${userId}`,
        success: (data) => reloadOrderAfterChanges(orderId)
    });

    $("#userSelectionModal").modal("hide");
});

$('#orderUserUnassigned').on('click', () => {
    let orderId = $('#orderUserAssigned').attr('orderid');

    $.ajax({
        type: 'Patch',
        url: '/api/patients/' + $('#patientIds').data('patientid') + `/orders/${orderId}`,
        success: (data) => reloadOrderAfterChanges(orderId)
    });

    $("#userSelectionModal").modal("hide");
});

/////////////////////////////////////////////////////////////////////
// Stopping an order or assigning it to a [new] user is a bit trickier because we may have several of them,
//  each displayed with its own "Stop"/"Assign" buttons.
// We resolve it by binding to each button an event handler on click,
//  which sends the Delete/Patch request when you click the button,
//  and on success reloads the order (we have defined a partial view for Order).

// Confirmation dialog and AJAX call to stop order (we use REST Delete action):
let stopOrderCallback = (orderDesc, orderId) =>
    confirmDialog(`Are you sure you want to stop order '${orderDesc}' ?`,
        (confirmed) => {
            if (confirmed) {
                $.ajax({
                    type: 'Delete',
                    url: '/api/patients/' + $('#patientIds').data('patientid') + `/orders/${orderId}`,
                    success: (data) => reloadOrderAfterChanges(orderId)
                });
            }
        });

// Function to bind the stop order handler to the click event of an element
// (which we assume is an element with data-orderdesc and data-orderid attributes):
let bindStopOrderHandlerToOrderElement = (element) => {
    $(element).on('click', () => {
        stopOrderCallback($(element).data('orderdesc'), $(element).data('orderid'));
    })
};

// Stop-order buttons are represented as elements of class 'nucoris-stop-order'
// This function gets all such elements and binds the stop order handler to each of them:
let addStopOrderHandlerToAllStopOrderElements = () => $('.nucoris-stop-order').each(
    (i, element) => bindStopOrderHandlerToOrderElement(element));

addStopOrderHandlerToAllStopOrderElements(); // invoke function now so it's attached to existing orders

// Function that reloads the order by invoking a partial view
let reloadOrderAfterChanges = (orderId) => {
    $('#' + orderId).load(
        "/PatientDetails/Order?patientId=" + $('#patientIds').data('patientid') + "&orderId=" + orderId,
        null,
        () => bindStopOrderHandlerToOrderElement($('#' + orderId).find('.nucoris-stop-order'))
    );
};
