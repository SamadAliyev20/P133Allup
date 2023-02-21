$(document).ready(function () {
    let isMain = $('#IsMain').is(':checked');

    if (isMain) {
        $('#fileInput').removeClass('d-none');
        $('#parentList').addClass('d-none');
    } else {
        $('#fileInput').addClass('d-none');
        $('#parentList').removeClass('d-none');
    }

    $('#IsMain').click(function () {
        let isMain = $(this).is(':checked');

        if (isMain) {
            $('#fileInput').removeClass('d-none');
            $('#parentList').addClass('d-none');
        } else {
            $('#fileInput').addClass('d-none');
            $('#parentList').removeClass('d-none');
        }

    })

})