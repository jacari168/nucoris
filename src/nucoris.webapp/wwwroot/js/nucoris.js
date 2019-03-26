// Scripts common to all views

/********************************************/
// Confirmation dialog function adapted from:
// https://medium.freecodecamp.org/custom-confirm-box-with-bootstrap-4-377aa67723c2

function confirmDialog(message, handler) {
    $(`<div class="modal fade" id="confirmationModal" role="dialog" tabindex="-1" aria-hidden="true"> 
         <div class="modal-dialog modal-dialog-centered" role="document"> 
            <div class="modal-content"> 
                <div class="modal-header border-0">
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                        <span aria-hidden="true">&times;</span>
                    </button>
                </div>
                <div class="modal-body border-0" style="padding:10px;"> 
                    <h5 class="text-center">${message}</h4> 
                </div> 
                <div class="modal-footer border-0">
                    <button type="button" class="btn btn-secondary btn-no" data-dismiss="modal">No</button>
                    <button type="button" class="btn btn-primary btn-yes">Yes</button>
                </div> 
           </div> 
        </div> 
      </div>`).appendTo('body');

    // Trigger the modal
    $("#confirmationModal").modal({
        backdrop: 'static',
        keyboard: true,
        focus: true
    });

    // When Yes button clicked, pass true to callback function
    $(".btn-yes").click(function () {
        handler(true);
        $("#confirmationModal").modal("hide");
    });

    // When No button clicked, pass false to callback function
    $(".btn-no").click(function () {
        handler(false);
        $("#confirmationModal").modal("hide");
    });

    // Remove the modal once it is closed.
    $("#confirmationModal").on('hidden.bs.modal', function () {
        $("#confirmationModal").remove();
    });
}

/********************************************/

function addTextItemDialog(message, handler) {
    $(`<div class="modal fade" id="addTextModal" role="dialog" tabindex="-1" aria-hidden="true"> 
         <div class="modal-dialog modal-dialog-centered" role="document"> 
            <div class="modal-content"> 
                <div class="modal-header border-0">
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                        <span aria-hidden="true">&times;</span>
                    </button>
                </div>
                <div class="modal-body border-0" style="padding:10px;"> 
                    <h5>${message}</h5> 
                    <input type="text" id="modalTextInput" class="form-control mt-4"/>
                </div> 
                <div class="modal-footer border-0">
                    <button type="button" class="btn btn-secondary btn-no" data-dismiss="modal">Cancel</button>
                    <button type="button" class="btn btn-primary btn-yes btn-default">Add</button>
                </div> 
           </div> 
        </div> 
      </div>`).appendTo('body');

    // Set focus on input:
    $('#addTextModal').on('shown.bs.modal', function () {
        $("#modalTextInput").focus();
    });

    // Trigger the modal
    $("#addTextModal").modal({
        backdrop: 'static',
        keyboard: true,
        focus: true
    });

    // When Yes button clicked, pass allergy to callback function
    $(".btn-yes").click(function () {
        handler($("#modalTextInput").val());
        $("#addTextModal").modal("hide");
    });

    // When No button clicked, pass null to callback function
    $(".btn-no").click(function () {
        handler(null);
        $("#addTextModal").modal("hide");
    });

    // Remove the modal once it is closed.
    $("#addTextModal").on('hidden.bs.modal', function () {
        $("#addTextModal").remove();
    });
}


/********************************************/