$(function () {
    $('[data-toggle="tooltip"]').tooltip();
    var docHeight = $(document).height();
    var windowHeight = $(window).height();
    if (docHeight <= windowHeight) {
        $('.footer').css({'position': 'fixed', 'bottom': '0', 'left': '0','right': '0'});
    }
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