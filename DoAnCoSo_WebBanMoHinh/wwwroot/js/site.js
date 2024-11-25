// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {
    $('.zoom-container').on('mousemove', function (e) {
        var containerOffset = $(this).offset();
        var containerWidth = $(this).width();
        var containerHeight = $(this).height();

        var mouseX = e.pageX - containerOffset.left;
        var mouseY = e.pageY - containerOffset.top;

        var percentX = (mouseX / containerWidth) * 100;
        var percentY = (mouseY / containerHeight) * 100;

        $(this).find('img').css({
            'transform': 'scale(1.5)',
            'transform-origin': `${percentX}% ${percentY}%`
        });
    }).on('mouseleave', function () {
        $(this).find('img').css('transform', 'scale(1)');
    });
    $('#imageCarousel img').on('click', function () {
        var imgSrc = $(this).attr('src');
        $('#modalImage').attr('src', imgSrc);
    });
    $('#imageModal').on('show.bs.modal', function (e) {
        var imgSrc = $(e.relatedTarget).attr('src');
        $('#modalImage').attr('src', imgSrc);
    });
    $('#imageModal .modal-body').on('click', function (e) {
        if (e.target === this) {
            $('#imageModal').modal('hide');
        }
    });
});