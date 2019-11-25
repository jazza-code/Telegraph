$(function () {
    $('[data-toggle="tooltip"]').tooltip();
});
function readImage(input, viewPictureId) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();
        reader.onload = function (e) {
            document.getElementById(viewPictureId).src = e.target.result;
        }
        reader.readAsDataURL(input.files[0]);
    }
};